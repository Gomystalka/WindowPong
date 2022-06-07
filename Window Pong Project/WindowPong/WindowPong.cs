using System;
using System.Threading;
using System.Diagnostics;
using System.Drawing;

using WindowPong.Input;
using WindowPong.Native;
using WindowPong.Hooks;
using WindowPong.Drawing;
using WindowPong.Utility;

/*  Written by Tomasz Galka 2019 (Tommy.galk@gmail.com)
 *  Window Pong - Turn any 3 windows into a pong game using the power of the Windows API!
 *  Uses Native Function Calls from Kernel32.dll and User32.dll.
 *  Only tested on Windows 10 using .Net Framework Version 4.6.1.
 *  This application uses the same game loop as my Java 2D Game Engine (which ported poorly).
*/
namespace WindowPong {
    public class WindowPong {
        //Game Objects
        public const string PLAYER1_PROC = "notepad";
        public const string PLAYER2_PROC = "notepad";
        public static string ballProcessName = "";
        public const int PADDLE_WIDTH = 135, PADDLE_HEIGHT = 300, BALL_WIDTH = 200, BALL_HEIGHT = 200;
        public const byte X_OFFSET = 6;
        public const int INITIAL_BALL_SPEED = 6;

        public GameObject player1;
        public GameObject player2;
        public GameObject ball;
        private readonly GameObject[] gameObjects;

        //Logic
        public const int TIME_SCALE = 1;
        public const double TARGET_FPS = 120 * TIME_SCALE;
        public bool running = false; //Thread state flag
        public bool gameStarted = false; //Game state flag
        public static double deltaTime; 
        public int fps; //Current frames per second

        //Input
        public KeyboardHook keyboardHook;
        public InputHandler inputHandler;

        //Screen
        public Vector screenSize;
        public static Canvas canvas;
        public IntPtr consoleWindowHandle;

        //Misc
        private readonly string scoreSpacing;
        public bool EnableRainbowScore { get; set; }
        private bool canToggleRainbowScore = true;
        private int hue;

        public WindowPong() {
            scoreSpacing = Utils.RepeatCharacter(' ', 1) + ":" + Utils.RepeatCharacter(' ', 1); //This is a meme
            consoleWindowHandle = NativeUtils.GetConsoleWindow();
            //canvas = new Drawing.Canvas(NativeUtils.GetDesktopWindow());
            screenSize = RetrieveScreenInfo();
            keyboardHook = new KeyboardHook(KeyboardHook.WH_KEYBOARD_LL);
            inputHandler = new InputHandler(this);

            gameObjects = SetupGameObjects();

            //Keys that will only be registered by this application and nothing else.
            int[] interceptedKeys = { InputHandler.K_UP, InputHandler.K_DOWN, InputHandler.K_W, InputHandler.K_S, InputHandler.K_R, InputHandler.K_CTRL, InputHandler.K_D};
            foreach (ushort k in interceptedKeys)
                keyboardHook.AddInterceptedKey(k);
        }

        private GameObject[] SetupGameObjects() { //Create and setup gameobjects
            //Setup ball
            ball = new GameObject("Ball", "");
            ball.isPaddle = false;
            ball.speed = INITIAL_BALL_SPEED;
            ball.useUserDefinedWindowSize = false;

            //Setup player
            player1 = new GameObject("Player 1", PLAYER1_PROC);
            player1.useUserDefinedWindowSize = false;

            player2 = new GameObject("Player 2", PLAYER2_PROC);
            player2.useUserDefinedWindowSize = false;

            return new GameObject[] { player1, player2, ball };
        }

        public void Run() { //Lol. I should separate this into its own string table.
            running = true;

            Console.Title = "Window Pong by Tomasz Galka";
            Console.WriteLine("Welcome to Window Pong!\nThis application lets you play pong against another player using open windows as the paddles and ball!\nPress any key to continue...");
            Console.ReadKey(true);
            Console.Clear();
            Console.WriteLine("The controls are as follows: ");
            Console.WriteLine("Player 1: W = Up | S = Down");
            Console.WriteLine("Player 2: Up Arrow = Up | Down Arrow = Down");
            Console.WriteLine("Space = Start");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            Console.Clear();
            Console.WriteLine("Currently, Player 1 process is set to [" + PLAYER1_PROC + "]");
            Console.WriteLine("Currently, Player 2 process is set to [" + PLAYER2_PROC + "]");

            string ballName;
            //Dangerous to make this always true but this relies on break to trigger.
            while (true) //Make sure a real process is found for the ball.
            {
                Console.WriteLine("Please type in the EXACT name of the process you wish to use as the ball.");
                ballName = Console.ReadLine();
                if (CheckIfProcessExists(ballName))
                    break;
                else
                {
                    Console.Clear();
                    Console.WriteLine($"The specified process [{ballName}] doesn't exist! Please specify an open process.");
                }
            }
            ballProcessName = ballName;
            ball.processName = ballName;
            Console.Clear();

            Console.WriteLine("Currently, Ball process is set to [" + ballProcessName + "]");
            Console.WriteLine("If you are fine with these settings, press any key to continue, otherwise close the application and change the settings.");
            Console.ReadKey(true);
            Console.Clear();

            bool result = WaitForProcesses(); //Block current thread until all processes are found and assigned.

            if (result)
                StartGameLoop();
            else
                Console.WriteLine("An error has occurred while fetching processes. The application will now exit.");
            Console.WriteLine("Bye Bye.");

            //canvas.Release(); //Release the desktop from the graphics instance.
            keyboardHook.Unhook(); //Unhook the keyboard hook.
            Console.ReadKey();

        }

        //Game loop
        //public void StartGameLoop() {
        //    long prev = Millis();
        //    int framesPerSecond = 0;
        //    long lastFrameTime = 0;
        //    double opt = 1000 / TARGET_FPS;
        //    while (running) {
        //        long currentMs = Millis();
        //        long lastUpdate = currentMs - prev;
        //        prev = currentMs;
        //        deltaTime = lastUpdate / (double)opt;
        //        lastFrameTime += lastUpdate;
        //        framesPerSecond++;
        //        if (lastFrameTime >= 1000) {
        //            UpdateEverySecond();
        //            fps = framesPerSecond;
        //            lastFrameTime = 0;
        //            framesPerSecond = 0;
        //        }
        //        Update();
        //        //Render();

        //        double sleepTime = (prev - Millis() + opt) / 1d;

        //        if (sleepTime > 0)
        //            Thread.Sleep((int)sleepTime);
        //    }
        //}

        public void StartGameLoop() {
            long prevTime = NanoTime();
            long prevMillis = Millis();
            double tickCount = TARGET_FPS;
            double opt = 1000000000 / tickCount;
            double accumulator = 0;
            long secondTimer = Millis();

            int framesPerSecond = 0;
            while (running) {
                long currentMillis = Millis();
                long time = NanoTime();
                accumulator += (time - prevTime) / opt;
                deltaTime = accumulator;
                prevTime = time;
                prevMillis = Millis();

                while (accumulator >= 1d)
                {
                    Update();
                    accumulator--;
                }

                framesPerSecond++;

                if(currentMillis - secondTimer > 1000)
                {
                    secondTimer += 1000;
                    fps = framesPerSecond;
                    framesPerSecond = 0;
                    UpdateEverySecond();
                }
                double sleepTime = (prevMillis - currentMillis) / 1d;
                if (sleepTime > 0) //Ganbare CPU-kun!
                    Thread.Sleep((int)sleepTime);
            }
            //Thread.Sleep(1);
        }

        private bool WaitForProcesses() { //Block current thread until all processes have been launched by the user.
            bool error = false;
            Console.WriteLine("Waiting for processes...");
            while (!ProcessesReady() && !error) {
                for (int i = 0; i < gameObjects.Length; i++) {
                    GameObject go = gameObjects[i];
                    if (go.process != null) continue;
                    Process[] procs = Process.GetProcessesByName(go.processName);
                    foreach (Process p in procs) {
                        IntPtr hWnd = p.MainWindowHandle;
                        if (!IsHandleOccupied(hWnd)) { //Check if the process handle is already tied to a gameobject
                            try
                            {
                                p.EnableRaisingEvents = true;
                                p.Exited += (sender, args) => ProcessExit(sender, args, go); //Assign exit handlers to all gameobject processes.
                                go.Handle = hWnd;
                                go.screenSize = screenSize;
                                go.CreateObject(p);
                                gameObjects[i] = go;
                                Console.WriteLine("Assigned Process [" + p.ProcessName + "] to gameobject [" + go.name + "]");
                                break;
                            }
                            catch (System.ComponentModel.Win32Exception e) { //Handle permission exception
                                if (e.Message != null && e.Message.ToLower().Contains("access"))
                                {
                                    Console.Clear();
                                    Console.WriteLine("Failed to fetch process [" + p.ProcessName + "] Access is denied. Try running this application as administrator or change the process.\nThis application will now exit.");
                                    Console.ReadKey();
                                    error = true;
                                    running = false;
                                    return false;
                                }
                            }
                        }
                    }
                }
                Thread.Sleep(10); //So the CPU doesn't die.
            }

            Console.WriteLine("All processes ready. Closing any process required by this application will terminate the entire application.");
            Console.WriteLine("Press any key to start the game...");
            gameStarted = true;
            keyboardHook.Active = gameStarted;
            //keyboardHook.SinglePressEnabled = true;
            keyboardHook.RegisterKeyboardHook(inputHandler);
            Console.ReadKey();
            foreach (GameObject go in gameObjects) {
                //NativeUtils.SetForegroundWindow(go.handle);
                NativeUtils.ShowWindow(go.Handle, NativeUtils.SW_RESTORE);
            }
            return true;
        }

        public void Update() { //Main update loop
            if (gameStarted)
            {
                NativeUtils.PeekMessage(out NativeUtils.TagMSG msg, consoleWindowHandle, 0, 0, 0);
                UpdateGameObjects(); //Update all game objects

                if (EnableRainbowScore)
                {
                    PerformUIUpdate();
                    hue = hue < 360 ? ++hue : 0;
                }

                if (inputHandler.flags[InputHandler.K_CTRL] && inputHandler.flags[InputHandler.K_D]) { //Ctrl+D = Rainbow Mode lol
                    if (canToggleRainbowScore)
                    {
                        canToggleRainbowScore = false;
                        EnableRainbowScore = !EnableRainbowScore;
                    }
                } else
                {
                    canToggleRainbowScore = true;
                }
            }
        }

        //private bool CheckForWinner() {
        //    return player1.score >= GameObject.WINNING_CONDITION || player2.score >= GameObject.WINNING_CONDITION;
        //}

        public void UpdateEverySecond() {
            if(!EnableRainbowScore)
                PerformUIUpdate();
        }

        public void PerformUIUpdate() {
            canvas.BeginInvoke(new Canvas.UIUpdate(UpdateUI));
        }

        public void UpdateUI() { //Update the UI elements on the desktop canvas.
            Text ct = canvas.CenterText;
            if (EnableRainbowScore)
                ct.Brush = new SolidBrush(Utils.HSVtoRGB(hue, 100, 100));
            else
                ct.Brush = new SolidBrush(Color.Red);

            ct.String = player1.score + scoreSpacing + player2.score;
            canvas.CenterText = ct;

            Text tl = canvas.TopLeftText;
            tl.String = "FPS: " + fps;
            canvas.TopLeftText = tl;

            canvas.textPanel.Invalidate();
            //canvas.Invalidate();
        }

        private bool IsHandleOccupied(IntPtr handle) { //Check if the specified handle is occupied by any gameobject
            foreach (GameObject go in gameObjects) {
                if (go.Handle == handle)
                    return true;
            }
            return false;
        }

        public void ProcessExit(object sender, EventArgs args, GameObject owner) { //Detect process termination
            Process p = owner.process;
            owner.process = null;
            owner.Handle = IntPtr.Zero;
            owner.created = false;
            Console.Clear();
            Console.WriteLine("Process: " + p.ProcessName + " assigned to " + owner.name + " has been terminated.");
            Console.WriteLine("The application will now be terminated!");
            Console.ReadKey();
            running = false;
        }

        public bool CheckIfProcessExists(string processName) {
            Process[] processes = Process.GetProcessesByName(processName); //Good luck GC-chan!
            return processes.Length > 0;
        }

        private void UpdateGameObjects() { //Update game objects
            player1.x = -X_OFFSET - 1;
            player2.x = screenSize.x - player2.w + X_OFFSET;
            foreach (GameObject go in gameObjects) {
                go.Update();
                GameObject col = go.CheckForCollision(gameObjects);
                if (col != null) {
                    if (!col.isPaddle && go.isPaddle) //Check for Player on Ball collision
                    { //Collisions should probably have their own class defined.
                        //This was causing the collision bug.
                        //if (col.x < go.w - 25) continue; //Don't allow Player1 to hit the ball once it passes the paddle.
                        //if (col.x > screenSize.x - (go.w + 16)) continue; //Don't allow Player2 to hit the ball once it passes the paddle.

                        Console.WriteLine(col.name + " collided with: " + go.name);
                        //Random rng = new Random();
                        col.speed = -col.speed;
                        bool direction = col.vX < 0;
                        col.x = direction ? go.x + go.w + 5 : go.x - col.w - 5;
                        float centerY = go.y + (go.h / 2f);
                        float impactY = col.y + (col.h / 2f);
                        float t = (direction ? 45 : 135) * (float)Math.PI / 180;
                        float a = Utils.Map((impactY - centerY), 0, go.h, t, -t);
                        col.vX = col.speed * (float)Math.Cos(a);
                        col.vY = -col.speed * (float)Math.Sin(a);
                    }
                }

                //Check for terrain collision
                if (!go.isPaddle) {
                    //Top
                    if (go.CollidesWith(0, -5, screenSize.x, 5)) {
                        go.y += 5;
                        go.vY = -go.vY;
                    }

                    //Bottom
                    if (go.CollidesWith(0, screenSize.y, screenSize.x, 50)) {
                        go.y -= 5;
                        go.vY = -go.vY;
                    }

                    if (go.CollidesWith(screenSize.x, 0, X_OFFSET, screenSize.y)) { //Right | Player 1 scores
                        ResetBall();
                        player1.score++;
                        PerformUIUpdate();
                    }

                    if (go.CollidesWith(-X_OFFSET, 0, X_OFFSET, screenSize.y)) { //Left | Player 2 scores
                        ResetBall();
                        player2.score++;
                        PerformUIUpdate();
                    }
                }
            }
        }

        public void ResetBall() {
            ball.vX = 0;
            ball.vY = 0;
            ball.x = (screenSize.x / 2) - ball.w / 2;
            ball.y = (screenSize.y / 2) - ball.h / 2;
        }

        public bool ProcessesReady() {
            int nullCount = 0;
            foreach (GameObject p in gameObjects) {
                if (p == null || p.process == null) nullCount++;
            }
            return nullCount == 0;
        }


        public static Vector RetrieveScreenInfo() { //Find screen size. Doesn't support multiple displays so far.
            Rectangle screenRect = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            return new Vector(screenRect.Width, screenRect.Height);
        }

        public long NanoTime()
        { //This is a terrible idea.
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000000; //Get the current Epoch time stamp in nano seconds
        }

        public long Millis()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); //Get the current Epoch time stamp in milliseconds
        }

        public static void Main(string[] args) {
            WindowPong wp = new WindowPong();
            Thread t = new Thread(new ThreadStart(wp.Run));
            t.Start();

            canvas = new Canvas();
            System.Windows.Forms.Application.Run(canvas);
        }
    }

    public struct Vector
    {
        public int x;
        public int y;

        public Vector(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool CompareTo(Vector v) {
            return x == v.x && y == v.y;
        }
    }
}
