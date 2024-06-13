using System;
using System.Threading;

class PlatformerGame {
    static GameMap? map;
    static Player? player;
    static Random random = new Random();
    static bool gameRunning = true;
    static int score = 0;

    // Переменные для текста на старте игры
    static string startText = "Press 'space' to jump.";
    static int startTextX;

    static readonly string _title = "JustMove ({0})";
    static void Main(string[] args) {
        Console.Title = "JustMove";
        Console.CursorVisible = false;

        SetWindowSize();

        while (true) {
            StartGame();

            while (gameRunning) {
                map.DrawMap(player.X, player.Y, startText, startTextX);
                HandleInput();
                UpdateGame();
                Thread.Sleep(2); // Задержка для регулировки скорости игры
            }

            ShowGameOverScreen();
        }
    }

    static void StartGame() {
        map = new GameMap();
        player = new Player(10, 14);

        startTextX = GameMap.MapWidth / 2 - startText.Length / 2 + 4;
        map.DrawGroundLine();
        gameRunning = true;
        score = 0;
    }

    static void SetWindowSize() {
        Console.SetWindowSize(GameMap.MapWidth + 2, GameMap.MapHeight + 2);
        Console.SetBufferSize(GameMap.MapWidth + 2, GameMap.MapHeight + 2);
    }

    static void HandleInput() {
        // Проверка нажатия клавиши
        if (Console.KeyAvailable) {
            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.Spacebar) {
                player.Jump();
            }
        }
    }

    static void UpdateGame() {
        map.MoveObstacles();
        map.GenerateObstacles();
        player.ApplyGravity();
        CheckCollision();

        score++;
        Console.Title = string.Format(_title, score);

        startTextX--;
    }

    static void CheckCollision() {
        if (!map.CheckCollision(player.X, player.Y)) {
            return;
        }
        gameRunning = false;
    }

    static void ShowGameOverScreen() {
        Console.Clear();
        SetWindowSize();
        Console.ResetColor();

        Console.BackgroundColor = ConsoleColor.DarkRed;
        Console.ForegroundColor = ConsoleColor.White;

        for (int i = 0; i < 22; i++) {
            if (i == 11) i = 12;

            if (i == 10) {
                Console.Write("GAME OVER!");
            } else if (i % 2 == 0) {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.DarkRed;
            } else {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.White;
            }

            Console.Write("  ");
        }


        Console.ResetColor();
        Console.Write("\n\n");
        Console.Write(" Your score: ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(score);
        Console.ResetColor();

        Console.Write("\n ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.Red;
        Console.Write("[1]");
        Console.ResetColor();
        Console.Write(" Exit");

        Console.Write("\n\n ");
        Console.ForegroundColor = ConsoleColor.Black;
        Console.BackgroundColor = ConsoleColor.White;
        Console.Write("[2]");
        Console.ResetColor();
        Console.Write(" Retry");



        Console.ResetColor();

        while (true) {
            ConsoleKey userAnswer = Console.ReadKey(true).Key;

            switch (userAnswer) {
                case ConsoleKey.D2:
                    return;
                case ConsoleKey.D1:
                    Environment.Exit(0);
                    break;
            }
        }
    }
}

class GameMap {
    public const int MapWidth = 50;
    public const int MapHeight = 20;
    public const int GroundLevel = 19;
    char[,] map;
    static Random random = new Random();

    public GameMap() {
        map = new char[MapWidth, MapHeight];
        GenerateMap();
    }

    void GenerateMap() {
        for (int x = 0; x < MapWidth; x++) {
            for (int y = 0; y < MapHeight; y++) {
                map[x, y] = ' ';
            }
        }
    }

    public void DrawGroundLine() {
        Console.BackgroundColor = ConsoleColor.DarkGreen;
        Console.SetCursorPosition(0, GroundLevel);
        for (int x = 0; x < 4; x++) {
            Console.Write(new string(' ', MapWidth) + (x != 2 ? '\n' : null));
        }
        Console.ResetColor();
    }

    public void DrawMap(int playerX, int playerY, string startText, int startTextX) {
        Console.SetCursorPosition(0, 0);
        for (int y = 0; y < GroundLevel; y++) {
            for (int x = 0; x < MapWidth; x++) {
                if (x == playerX && y == playerY) {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.Write(" ");
                    Console.ResetColor();
                } else {
                    Console.ForegroundColor = map[x, y] == 'X' ? ConsoleColor.DarkGreen : ConsoleColor.Gray;
                    Console.Write(map[x, y]);
                }
            }
            Console.WriteLine();
        }

        if (startTextX + startText.Length > 0) {
            Console.SetCursorPosition(Math.Max(startTextX, 0), GroundLevel - 8);
            Console.ResetColor();
            Console.Write(startText.Substring(Math.Max(-startTextX, 0)));
        }
    }

    public void MoveObstacles() {
        for (int x = 0; x < MapWidth; x++) {
            for (int y = 0; y < MapHeight; y++) {
                if (map[x, y] != ' ') {
                    char current = map[x, y];
                    map[x, y] = ' ';
                    if (x != 0) {
                        map[x - 1, y] = current;
                    }
                }
            }
        }
    }

    // эта хуйта вроде терь работает как надо
    public void GenerateObstacles() {

        int groundY = GroundLevel - 1;
        bool canPlaceObstacleX = map[MapWidth - 2, groundY] != 'X' && map[MapWidth - 3, groundY] != 'X';
        bool canPlaceObstacleV = true;

        for (int i = 0; i < MapHeight; i++) {
            if (map[MapWidth - 2, i] == 'V' || map[MapWidth - 3, i] == 'V') {
                canPlaceObstacleV = false;
                break;
            }
        }

        if (random.Next(0, 100) < 3 && canPlaceObstacleX) {
            map[MapWidth - 1, groundY] = 'X';
        }

        if (random.Next(0, 100) < 2 && canPlaceObstacleV) {
            int obstacleHeight = random.Next(2, 4);
            int obstaclePosition = groundY - obstacleHeight - 1;

            bool canPlaceTallObstacle = true;
            for (int i = 0; i < obstacleHeight; i++) {
                if (map[MapWidth - 2, obstaclePosition + i] == 'V' || map[MapWidth - 3, obstaclePosition + i] == 'V') {
                    canPlaceTallObstacle = false;
                    break;
                }
            }

            if (canPlaceTallObstacle && (map[MapWidth - 1, obstaclePosition - 1] != 'V' || map[MapWidth - 1, obstaclePosition - 1] != 'X')) {
                map[MapWidth - 1, obstaclePosition] = 'V';
            }
        }
    }



    public bool CheckCollision(int playerX, int playerY) {
        return map[playerX, playerY] == 'X' || map[playerX, playerY] == 'V';
    }
}

class Player {
    public int X { get; private set; }
    public int Y { get; private set; }
    const int JumpHeight = 3;
    int jumpProgress;
    bool isJumping;

    public Player(int startX, int startY) {
        X = startX;
        Y = startY;
        isJumping = false;
        jumpProgress = 0;
    }

    public void Jump() {
        if (Y == GameMap.GroundLevel - 1) {
            isJumping = true;
            jumpProgress = 0;
        }
    }

    public void ApplyGravity() {
        if (isJumping) {
            if (jumpProgress < JumpHeight) {
                Y--;
                jumpProgress++;
            } else {
                isJumping = false;
            }
        } else if (Y < GameMap.GroundLevel - 1) {
            Y++;
        }
    }
}
