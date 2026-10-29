
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace OfficeFileGuard.App
{
    public class RoundedPanel : Panel
    {
        private int borderRadius = 20;
        private int borderSize = 0;
        private Color borderColor = Color.Transparent;

        [Category("Aspetto")]
        [Description("Raggio degli angoli arrotondati.")]
        public int BorderRadius
        {
            get => borderRadius;
            set
            {
                borderRadius = Math.Max(0, value);
                AggiornaForma();
                Invalidate();
            }
        }

        [Category("Aspetto")]
        [Description("Spessore del bordo.")]
        public int BorderSize
        {
            get => borderSize;
            set
            {
                borderSize = Math.Max(0, value);
                Invalidate();
            }
        }

        [Category("Aspetto")]
        [Description("Colore del bordo.")]
        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                Invalidate();
            }
        }

        public RoundedPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BorderStyle = BorderStyle.None;

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            AggiornaForma();
        }

        private GraphicsPath CreaPercorso(
            Rectangle rettangolo, int raggio)
        {
            GraphicsPath path = new GraphicsPath();

            int d = Math.Min(
                raggio * 2,
                Math.Min(rettangolo.Width, rettangolo.Height));

            if (d <= 0)
            {
                path.AddRectangle(rettangolo);
                return path;
            }

            path.AddArc(rettangolo.X, rettangolo.Y, d, d, 180, 90);
            path.AddArc(rettangolo.Right - d, rettangolo.Y, d, d, 270, 90);
            path.AddArc(rettangolo.Right - d, rettangolo.Bottom - d, d, d, 0, 90);
            path.AddArc(rettangolo.X, rettangolo.Bottom - d, d, d, 90, 90);

            path.CloseFigure();
            return path;
        }

        private void AggiornaForma()
        {
            if (Width <= 0 || Height <= 0)
                return;

            using (GraphicsPath path = CreaPercorso(new Rectangle(0, 0, Width, Height), borderRadius))
            {
                Region nuovaRegione = new Region(path);
                Region vecchiaRegione = Region;
                Region = nuovaRegione;
                vecchiaRegione?.Dispose();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AggiornaForma();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        { 
            // per evitare il disegno dello sfondo rettangolare predefinito, lo sfondo viene dipinto in OnPaint.
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle rettangolo = ClientRectangle;

            if (rettangolo.Width <= 0 || rettangolo.Height <= 0)
                return;

            // Disegna lo sfondo con angoli arrotondati.
            using (GraphicsPath pathSfondo =
                CreaPercorso(rettangolo, borderRadius))
            using (SolidBrush pennello = new SolidBrush(BackColor))
            {
                g.FillPath(pennello, pathSfondo);
            }

            // Disegna il bordo solo se richiesto.
            if (borderSize > 0 && borderColor != Color.Transparent)
            {
                float inset = borderSize / 2f;

                RectangleF rettangoloBordo = new RectangleF(
                    inset,
                    inset,
                    Width - borderSize - 1f,
                    Height - borderSize - 1f);

                if (rettangoloBordo.Width > 0 &&
                    rettangoloBordo.Height > 0)
                {
                    int raggioBordo = Math.Max(
                        0, borderRadius - (int)Math.Ceiling(inset));

                    using (GraphicsPath pathBordo =
                        CreaPercorso(
                            Rectangle.Round(rettangoloBordo),
                            raggioBordo))
                    using (Pen pen = new Pen(borderColor, borderSize))
                    {
                        pen.Alignment = PenAlignment.Center;
                        g.DrawPath(pen, pathBordo);
                    }
                }
            }
        }
    }
}
