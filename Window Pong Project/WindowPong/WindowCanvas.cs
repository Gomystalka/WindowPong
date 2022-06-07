using System;
using System.Drawing;

using WindowPong.Native;

namespace WindowPong.Drawing {
    public class WindowCanvas {
        private readonly IntPtr deviceContext;
        private readonly IntPtr hWnd;
        private Graphics g;

        private Color fillColor = Color.Black;
        private Brush Brush { get; set; }

        private readonly StringFormat format;
        private Font font;
        public int FontSize { get; set; }
        public string FontFamily { get; set; }

        public WindowCanvas(IntPtr hWnd) {
            FontFamily = "Arial";
            FontSize = 16;
            font = new Font(FontFamily, FontSize);
            format = new StringFormat();
            Brush = new SolidBrush(fillColor);
            this.hWnd = hWnd;
            deviceContext = NativeUtils.GetDC(hWnd);
            g = Graphics.FromHdc(deviceContext);
        }

        public void Draw() {
            g = Graphics.FromHdc(deviceContext);
        }

        public void SetBrush(Brush b) {
            Brush = b;
        }

        public void SetFont(string fontFamily, int fontSize) {
            FontFamily = fontFamily;
            FontSize = fontSize;
            font = new Font(FontFamily, FontSize);
        }

        public void SetFontSize(int fontSize) {
            FontSize = fontSize;
            font = new Font(FontFamily, FontSize);
        }

        public void Fill(byte r, byte g, byte b, byte a) {
            fillColor = Color.FromArgb(a, r, g, b);
            Brush = new SolidBrush(fillColor);
        }

        public void Fill(Color color) {
            Fill(color.R, color.G, color.B, color.A);
        }

        public void Rect(int x, int y, int w, int h) {
            g.FillRectangle(Brush, new Rectangle(x, y, w, h));
        }

        public void Ellipse(int x, int y, int w, int h) {
            g.FillEllipse(Brush, new Rectangle(x, y, w, h));
        }

        public void SetTextAlignment(StringAlignment alignment) {
            format.Alignment = alignment;
            format.LineAlignment = alignment;
        }

        public void SetTextAlignment(StringAlignment alignment, StringAlignment lineAlignment) {
            format.Alignment = alignment;
            format.LineAlignment = lineAlignment;
        }

        public void Text(string str, int x, int y) {
            g.DrawString(str, font, Brush, x, y, format);
        }

        public void Dispose() {
            //g.Clear(Color.FromArgb(0, 0, 0, 0));
            g.Dispose();
        }

        public void Release() {
            NativeUtils.ReleaseDC(hWnd, deviceContext);
        }
    }
}
