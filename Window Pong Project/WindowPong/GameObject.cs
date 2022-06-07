using System;
using System.Diagnostics;

using WindowPong.Native;
using WindowPong.Utility;

/*
 * Written by Tomasz Galka 2019 (Tommy.galk@gmail.com) 
 * Class used to hold game object data for paddles and the ball.
 * Uses native calls to SetWindowTextA, SetWindowPos and GetWindowRect.
 */

namespace WindowPong
{
    public class GameObject {
        //Constants
        public const int DEFAULT_SPEED = 3;
        public const int WINNING_CONDITION = 10;

        //Properties
        public string name;
        public string processName;
        public Process process;
        public IntPtr Handle { get; set; } //Memory pointer to the process window
        public bool created = false;
        public bool isPaddle = true; //Is paddle by default.
        public byte score; //Doesn't need to be larger than 128

        //Position and Velocity
        public float x, y, vX, vY, centerX, centerY;
        public int w, h;
        public int speed = DEFAULT_SPEED;

        //Options
        public bool useUserDefinedWindowSize = true;

        //Misc
        public Vector screenSize;

        public GameObject(string name, string processName) {
            this.name = name;
            this.processName = processName;
        }

        public void Update() {
            if (!created) return;
            x += vX * (float)WindowPong.deltaTime; //Delta doesn't help because the Native method SetWindowPos only accepts integers for x and y
            y += vY * (float)WindowPong.deltaTime;
            centerX = x + (w / 2);
            centerY = y + (h / 2);
            if(isPaddle)
                y = Utils.Constrain(y, 0, screenSize.y - (w * 2));

            NativeUtils.SetWindowPos(Handle, 0, (int)x, (int)y, w, h, 0);
        }

        public GameObject CheckForCollision(GameObject[] gameObjects) {
            foreach (GameObject go in gameObjects) {
                if (CollidesWith(go) && go != this) {
                    return go;
                }
            }
            return null;
        }

        public bool CollidesWith(GameObject go) {
            return (go.x < x + w) && (x < (go.x + go.w)) && (go.y < y + h) && (y < go.y + go.h);
        }

        public bool CollidesWith(int x1, int y1, int w1, int h1) {
            return (x1 < x + w) && (x < (x1 + w1)) && (y1 < y + h) && (y < y1 + h1);
        }

        public void CreateObject(Process process) {
            this.process = process;
            NativeUtils.SetWindowTextA(Handle, name);
            NativeUtils.Rect r = new NativeUtils.Rect();
            NativeUtils.GetWindowRect(Handle, ref r);
            if (isPaddle)
            {
                w = useUserDefinedWindowSize ? r.GetWidth() : WindowPong.PADDLE_WIDTH;
                h = useUserDefinedWindowSize ? r.GetHeight() : WindowPong.PADDLE_HEIGHT;
            }
            else {
                w = useUserDefinedWindowSize ? r.GetWidth() : WindowPong.BALL_WIDTH;
                h = useUserDefinedWindowSize ? r.GetHeight() : WindowPong.BALL_HEIGHT;
                x = (screenSize.x / 2) - w / 2;
                y = (screenSize.y / 2) - h / 2;
            }
            created = true;
        }
    }
}
