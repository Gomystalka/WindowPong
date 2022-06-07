using System;

/*
 * Written by Tomasz Galka 2019 (Tommy.galk@gmail.com) 
 * Class used to handle key events received from the KeyboardHook class.
 * Implements IKeyEventListener interface.
 */

namespace WindowPong.Input
{
    public class InputHandler : IKeyEventListener
    {
        public bool[] flags = new bool[255];
        public const ushort K_UP = 38, K_DOWN = 40, K_W = 87, K_S = 83, K_R = 82, K_SPACE = 32, K_CTRL = 162, K_D = 68;
        private readonly WindowPong gameInstance;

        public InputHandler(WindowPong gameInstance)
        {
            this.gameInstance = gameInstance;
            
        }

        public void OnKeyPressed(ushort keyCode)
        {
            Console.WriteLine($"KEY: {keyCode}");
            if (gameInstance == null) return; //Do not continue event if the game instance has not been set.
            flags[keyCode] = true;
            switch (keyCode)
            {
                case K_UP:
                    gameInstance.player1.vY = -gameInstance.player1.speed;
                    break;
                case K_DOWN:
                    gameInstance.player1.vY = gameInstance.player1.speed;
                    break;
                case K_W:
                    gameInstance.player2.vY = -gameInstance.player2.speed;
                    break;
                case K_S:
                    gameInstance.player2.vY = gameInstance.player2.speed;
                    break;
                case K_R:
                    break;
                case K_SPACE:
                    if (gameInstance.ball.vX == 0 && gameInstance.ball.vY == 0)
                    {
                        Random rng = new Random();
                        int randX = rng.Next(2, WindowPong.INITIAL_BALL_SPEED);
                        int randY = rng.Next(1, 4);
                        gameInstance.ball.vX = rng.Next(0, 2) == 0 ? randX : -randX;
                        gameInstance.ball.vY = rng.Next(0, 2) == 0 ? randY : -randY;
                    }
                    break;
                default:
                    break;
            }
        }

        public void OnKeyReleased(ushort keyCode)
        {
            if (gameInstance == null) return; //Do not continue event if the game instance has not been set.
            flags[keyCode] = false;
            switch (keyCode)
            {
                case K_UP:
                    gameInstance.player1.vY = 0;
                    break;
                case K_DOWN:
                    gameInstance.player1.vY = 0;
                    break;
                case K_W:
                    gameInstance.player2.vY = 0;
                    break;
                case K_S:
                    gameInstance.player2.vY = 0;
                    break;
                case K_R:
                    //if(gameInstance.OffScreen())
                        gameInstance.ResetBall();
                    break;
                case K_SPACE:
                    break;
                default:
                    break;
            }
        }

        public void OnSingleKeyPressed(ushort keyCode)
        {
        }
    }
}
