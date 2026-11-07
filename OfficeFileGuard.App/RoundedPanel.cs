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
            set { borderRadius = Math.Max(0, value); Invalidate(); }
        }

        [Category("Aspetto")]
        [Description("Spessore del bordo.")]
        public int BorderSize
        {
            get => borderSize;
            set { borderSize = Math.Max(0, value); Invalidate(); }
        }

        [Category("Aspetto")]
        [Description("Colore del bordo.")]
        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }

        public RoundedPanel()
        {
            BorderStyle = BorderStyle.None;

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
        }

        private static GraphicsPath CreaPercorso(RectangleF r, float raggio)
        {
            GraphicsPath path = new GraphicsPath();

            float d = Math.Min(raggio * 2f, Math.Min(r.Width, r.Height));

            if (d <= 0f)
            {
                path.AddRectangle(r);
                return path;
            }

            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // Se cambia il colore del contenitore, bisogna ridisegnare.
        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Volutamente vuoto: lo sfondo viene dipinto interamente in OnPaint.
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (Width <= 0 || Height <= 0)
                return;

            // 1) Pulisco TUTTO con il colore del contenitore (niente nero sotto l'antialiasing).
            Color coloreEsterno = Parent?.BackColor ?? SystemColors.Control;
            g.Clear(coloreEsterno);

            g.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF esterno = new RectangleF(0, 0, Width, Height);
            bool haBordo = borderSize > 0 && borderColor != Color.Transparent;

            // 2) Se c'è il bordo, riempio la forma esterna con il colore del bordo.
            if (haBordo)
            {
                using (GraphicsPath pathEsterno = CreaPercorso(esterno, borderRadius))
                using (SolidBrush brushBordo = new SolidBrush(borderColor))
                {
                    g.FillPath(brushBordo, pathEsterno);
                }
            }

            // 3) Forma interna (ridotta dello spessore del bordo) con il colore di sfondo.
            float s = haBordo ? borderSize : 0f;
            RectangleF interno = new RectangleF(s, s, Width - 2f * s, Height - 2f * s);

            if (interno.Width > 0 && interno.Height > 0)
            {
                using (GraphicsPath pathInterno = CreaPercorso(interno, Math.Max(0f, borderRadius - s)))
                using (SolidBrush brushSfondo = new SolidBrush(BackColor))
                {
                    g.FillPath(brushSfondo, pathInterno);
                }
            }
        }
    }
}