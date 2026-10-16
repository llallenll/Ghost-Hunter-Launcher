using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GhostHunterLauncher
{
    static class Theme
    {
        public static readonly Color Background = Color.FromArgb(12, 13, 18);
        public static readonly Color Panel = Color.FromArgb(22, 24, 32);
        public static readonly Color Border = Color.FromArgb(40, 44, 58);
        public static readonly Color Text = Color.FromArgb(226, 230, 238);
        public static readonly Color Muted = Color.FromArgb(132, 140, 160);
        public static readonly Color Accent = Color.FromArgb(150, 232, 200);
        public static readonly Color AccentHover = Color.FromArgb(182, 244, 220);
        public static readonly Color AccentText = Color.FromArgb(10, 22, 18);
        public static readonly Color Error = Color.FromArgb(255, 128, 128);
        public static readonly Color HeaderTop = Color.FromArgb(34, 24, 54);

        public static GraphicsPath Rounded(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            var d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    /// <summary>Big rounded call-to-action button (Install / Update / Play).</summary>
    class ActionButton : Control
    {
        bool hover;

        public ActionButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnEnabledChanged(EventArgs e) { Cursor = Enabled ? Cursors.Hand : Cursors.Default; Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnTextChanged(EventArgs e) { Invalidate(); base.OnTextChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            var fill = !Enabled ? Theme.Border : hover ? Theme.AccentHover : Theme.Accent;
            var textColor = Enabled ? Theme.AccentText : Theme.Muted;
            using (var path = Theme.Rounded(rect, 8))
            using (var brush = new SolidBrush(fill))
                g.FillPath(brush, path);
            TextRenderer.DrawText(g, Text, Font, rect, textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }
    }

    class SlimProgressBar : Control
    {
        double value;

        public SlimProgressBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        public double Value
        {
            get { return value; }
            set { this.value = Math.Max(0, Math.Min(1, value)); Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent != null ? Parent.BackColor : Theme.Panel);
            var radius = Math.Max(1, Height / 2);
            using (var track = Theme.Rounded(new Rectangle(0, 0, Width - 1, Height - 1), radius))
            using (var brush = new SolidBrush(Theme.Border))
                g.FillPath(brush, track);
            var fillWidth = (int)((Width - 1) * value);
            if (fillWidth >= Height)
            {
                using (var bar = Theme.Rounded(new Rectangle(0, 0, fillWidth, Height - 1), radius))
                using (var brush = new SolidBrush(Theme.Accent))
                    g.FillPath(brush, bar);
            }
        }
    }

    /// <summary>Painted header with the game title over a dusky gradient.</summary>
    class HeaderPanel : Panel
    {
        public HeaderPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            using (var brush = new LinearGradientBrush(ClientRectangle, Theme.HeaderTop, Theme.Background, LinearGradientMode.Vertical))
                g.FillRectangle(brush, ClientRectangle);

            var scale = DeviceDpi / 96f;
            using (var titleFont = new Font("Segoe UI Black", 30f, FontStyle.Regular))
            using (var subFont = new Font("Segoe UI Semibold", 10f, FontStyle.Regular))
            {
                var x = (int)(32 * scale);
                TextRenderer.DrawText(g, LauncherConfig.Title, titleFont, new Point(x - (int)(4 * scale), (int)(22 * scale)), Theme.Text);
                TextRenderer.DrawText(g, LauncherConfig.Subtitle, subFont, new Point(x, (int)(80 * scale)), Theme.Accent);
            }
        }
    }

    class LinkButton : Label
    {
        public LinkButton(string text)
        {
            Text = text;
            AutoSize = true;
            Cursor = Cursors.Hand;
            ForeColor = Theme.Muted;
            Font = new Font("Segoe UI", 9f);
        }

        protected override void OnMouseEnter(EventArgs e) { if (Enabled) ForeColor = Theme.Text; base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { ForeColor = Theme.Muted; base.OnMouseLeave(e); }
    }
}
