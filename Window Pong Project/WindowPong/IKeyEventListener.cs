/*
 * Written by Tomasz Galka 2019 (Tommy.galk@gmail.com) 
 * Interface used to handle Key Events received from the KeyboardHook class.
 */

namespace WindowPong.Input
{
    public interface IKeyEventListener {
        void OnKeyPressed(ushort keyCode);
        void OnKeyReleased(ushort keyCode);
        void OnSingleKeyPressed(ushort keyCode);
    }
}
