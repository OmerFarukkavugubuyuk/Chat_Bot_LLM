using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChatbotLLM
{
    public class ChatBubble : UserControl
    {
        private readonly ChatMessage _message;
        private readonly int _maxWidth;

        // Colors
        private static readonly Color UserBubbleStart = Color.FromArgb(149, 76, 233);
        private static readonly Color UserBubbleEnd   = Color.FromArgb(107, 33, 168);
        private static readonly Color AiBubbleColor   = Color.FromArgb(45, 25, 70);
        private static readonly Color UserTextColor   = Color.White;
        private static readonly Color AiTextColor     = Color.FromArgb(230, 210, 255);
        private static readonly Color TimeColor       = Color.FromArgb(160, 140, 200);

        private const int Padding   = 14;
        private const int Radius    = 18;
        private const int TailSize  = 10;

        public ChatBubble(ChatMessage message, int containerWidth)
        {
            _message   = message;
            _maxWidth  = (int)(containerWidth * 0.70);

            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.Transparent;
            Measure();
        }

        private void Measure()
        {
            using var g        = CreateGraphics();
            var msgFont        = new Font("Segoe UI", 10.5f);
            var timeFont       = new Font("Segoe UI", 7.5f);
            int innerMaxW      = _maxWidth - Padding * 2 - TailSize;

            SizeF textSize = g.MeasureString(_message.Text, msgFont,
                                             new SizeF(innerMaxW, 2000),
                                             StringFormat.GenericDefault);

            SizeF timeSize = g.MeasureString(_message.Time, timeFont);

            int bubbleW = (int)Math.Max(textSize.Width, timeSize.Width) + Padding * 2 + TailSize + 8;
            bubbleW     = Math.Min(bubbleW, _maxWidth);

            int bubbleH = (int)textSize.Height + (int)timeSize.Height + Padding * 2 + 6;

            Width  = bubbleW + 20;
            Height = bubbleH + 16;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode      = SmoothingMode.AntiAlias;
            g.TextRenderingHint  = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var msgFont  = new Font("Segoe UI", 10.5f);
            var timeFont = new Font("Segoe UI", 7.5f);

            int innerMaxW = Width - Padding * 2 - TailSize - 20;
            SizeF textSize = g.MeasureString(_message.Text, msgFont,
                                             new SizeF(innerMaxW, 2000),
                                             StringFormat.GenericDefault);
            SizeF timeSize = g.MeasureString(_message.Time, timeFont);

            int bubbleW = (int)Math.Max(textSize.Width, timeSize.Width) + Padding * 2 + 8;
            int bubbleH = (int)textSize.Height + (int)timeSize.Height + Padding * 2 + 6;

            Rectangle bubbleRect;
            PointF textOrigin;
            PointF timeOrigin;

            if (_message.IsUser)
            {
                // Right side — tail on right
                int bx = Width - bubbleW - TailSize - 4;
                bubbleRect  = new Rectangle(bx, 6, bubbleW, bubbleH);
                textOrigin  = new PointF(bx + Padding, 6 + Padding);
                timeOrigin  = new PointF(bx + bubbleW - (int)timeSize.Width - Padding + 4,
                                         6 + Padding + (int)textSize.Height + 2);

                // Gradient bubble
                using var brush = new LinearGradientBrush(bubbleRect,
                    UserBubbleStart, UserBubbleEnd, LinearGradientMode.ForwardDiagonal);
                DrawRoundedRect(g, brush, bubbleRect, Radius);

                // Right tail
                var tail = new Point[]
                {
                    new Point(bubbleRect.Right, bubbleRect.Bottom - 24),
                    new Point(bubbleRect.Right + TailSize, bubbleRect.Bottom - 14),
                    new Point(bubbleRect.Right, bubbleRect.Bottom - 10)
                };
                using var solidBrush = new SolidBrush(UserBubbleEnd);
                g.FillPolygon(solidBrush, tail);

                g.DrawString(_message.Text, msgFont, Brushes.White,
                             new RectangleF(textOrigin.X, textOrigin.Y, innerMaxW + 8, Height));
                using var timeBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
                g.DrawString(_message.Time, timeFont, timeBrush, timeOrigin);
            }
            else
            {
                // Left side — tail on left
                int bx = TailSize + 4;
                bubbleRect  = new Rectangle(bx, 6, bubbleW, bubbleH);
                textOrigin  = new PointF(bx + Padding, 6 + Padding);
                timeOrigin  = new PointF(bx + bubbleW - (int)timeSize.Width - Padding + 4,
                                         6 + Padding + (int)textSize.Height + 2);

                using var brush = new SolidBrush(AiBubbleColor);
                DrawRoundedRect(g, brush, bubbleRect, Radius);

                // Left tail
                var tail = new Point[]
                {
                    new Point(bubbleRect.Left, bubbleRect.Bottom - 24),
                    new Point(bubbleRect.Left - TailSize, bubbleRect.Bottom - 14),
                    new Point(bubbleRect.Left, bubbleRect.Bottom - 10)
                };
                using var tailBrush = new SolidBrush(AiBubbleColor);
                g.FillPolygon(tailBrush, tail);

                // Subtle border
                using var pen = new Pen(Color.FromArgb(80, 149, 76, 233), 1f);
                DrawRoundedRectBorder(g, pen, bubbleRect, Radius);

                g.DrawString(_message.Text, msgFont, new SolidBrush(AiTextColor),
                             new RectangleF(textOrigin.X, textOrigin.Y, innerMaxW + 8, Height));
                using var timeBrush = new SolidBrush(TimeColor);
                g.DrawString(_message.Time, timeFont, timeBrush, timeOrigin);
            }
        }

        private void DrawRoundedRect(Graphics g, Brush brush, Rectangle rect, int radius)
        {
            var path = GetRoundedPath(rect, radius);
            g.FillPath(brush, path);
        }

        private void DrawRoundedRectBorder(Graphics g, Pen pen, Rectangle rect, int radius)
        {
            var path = GetRoundedPath(rect, radius);
            g.DrawPath(pen, path);
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
