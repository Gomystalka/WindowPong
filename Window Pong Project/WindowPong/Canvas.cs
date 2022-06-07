using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Text;

using WindowPong.Native;

/*
 * Written by Tomasz Galka 2019 (Tommy.galk@gmail.com) 
 * Simple transparent Windows Form used to display score and FPS.
 * Font used: Kongtext (https://www.dafont.com/kongtext.font) Completely free to use.
 */

namespace WindowPong.Drawing {
    public class Canvas : Form {
        public WindowPong WindowPong { get; set; }
        public Vector screenSize;
        public Text CenterText { get; set; }
        public Text TopLeftText { get; set; }
        public TextPanel textPanel;

        private Size MainSize { get; set; }
        private PrivateFontCollection pfc = new PrivateFontCollection();

        public Canvas() {
            LoadFont();
            CenterText = new Text("", new Font(pfc.Families[0], 128), new SolidBrush(Color.Red));
            TopLeftText = new Text("", new Font("Arial", 32), new SolidBrush(Color.Yellow));
            screenSize = WindowPong.RetrieveScreenInfo();
            MainSize = new Size(screenSize.x, screenSize.y / 5);
            Run();
        }

        public delegate void UIUpdate();

        private CreateParams GetClickThroughParams() {
           CreateParams cParams = base.CreateParams;
           cParams.ExStyle |= NativeUtils.WS_EX_LAYERED | NativeUtils.WS_EX_TRANSPARENT;
           return cParams;
        }

        protected override CreateParams CreateParams {
            get {
                return GetClickThroughParams();
            }
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;
            Location = new Point(0, 0);
            Size = MainSize;
            TransparencyKey = Color.Wheat;
            BackColor = Color.Wheat;
            TopMost = true;
        }

        private void InitializeComponent() {
            textPanel = new TextPanel(this)
            {
                Size = MainSize
            };
            Controls.Add(textPanel);
            //Double Buffer the text panel to stop flickering on update.
            typeof(TextPanel).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, textPanel, new object[] { true });
        }

        private void LoadFont() {
            pfc = new PrivateFontCollection();
            byte[] fontBuffer = Properties.Resources.kongtext;
            int len = Properties.Resources.kongtext.Length;
            IntPtr fontPointer = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(len);
            System.Runtime.InteropServices.Marshal.Copy(fontBuffer, 0, fontPointer, len);
            pfc.AddMemoryFont(fontPointer, len);
        }

        [STAThread]
        public void Run()
        {
            Application.EnableVisualStyles();
            InitializeComponent();
        }
    }

    public class TextPanel : Panel {
        private readonly StringFormat format;
        private readonly Canvas canvas;
        private readonly Vector screenSize;

        public TextPanel(Canvas canvas) {
            this.canvas = canvas;
            screenSize = canvas.screenSize;
            format = new StringFormat();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            format.Alignment = StringAlignment.Near;
            format.LineAlignment = StringAlignment.Near;
            DrawString(g, canvas.TopLeftText, 0, 0, format);

            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            //DrawOutlinedString(g, canvas.CenterText, screenSize.x / 2, canvas.CenterText.Font.Height / 2, format, Pens.Black, Brushes.Red);
            DrawString(g, canvas.CenterText, screenSize.x / 2, (canvas.CenterText.Font.Height / 2) + 16, format);
           
            //g.FillRectangle(Brushes.Aqua, new Rectangle(0, 0, Size.Width, Size.Height));
        }

        private void DrawString(Graphics g, Text text, int x, int y, StringFormat format) {
            DrawString(g, text, x, y, format, 0, null);
        }
         
        private void DrawString(Graphics g, Text text, int x, int y, StringFormat format, int fontSizeOffset, Brush brush) {
            if (text.String == null || text.Font == null || text.Brush == null) return;
            g.DrawString(text.ToString(), new Font(text.Font.FontFamily, text.Font.SizeInPoints + fontSizeOffset), brush == null ? text.Brush : brush, x, y, format);
        }

        private void DrawOutlinedString(Graphics g, Text text, int x, int y, StringFormat format, Pen pen, Brush brush) { //Bad quality
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddString(text.String, text.Font.FontFamily, (int) FontStyle.Regular, text.Font.Size, new Point(x, y), format);
            
            g.DrawPath(pen, path);
            g.FillPath(brush, path);
        }
    }

    public struct Text {
        public string String { get; set; }
        public Font Font { get; set; }
        public SolidBrush Brush {get; set;}

        public Text(string s, Font f, SolidBrush b) {
            String = s;
            Font = f;
            Brush = b;
        }

        public override string ToString() => String;
    }
}
