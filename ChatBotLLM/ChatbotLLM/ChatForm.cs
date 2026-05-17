using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatbotLLM
{
    public class ChatForm : Form
    {
        // ─── Colors ───────────────────────────────────────────────────────────
        private static readonly Color BgDark       = Color.FromArgb(12, 5, 28);
        private static readonly Color BgMid        = Color.FromArgb(22, 10, 48);
        private static readonly Color HeaderBg     = Color.FromArgb(30, 10, 65);
        private static readonly Color InputBg      = Color.FromArgb(28, 12, 58);
        private static readonly Color InputBorder  = Color.FromArgb(120, 76, 200);
        private static readonly Color AccentPurple = Color.FromArgb(149, 76, 233);
        private static readonly Color LightLila    = Color.FromArgb(200, 170, 255);
        private static readonly Color SendBtnStart = Color.FromArgb(140, 60, 220);
        private static readonly Color SendBtnEnd   = Color.FromArgb(80, 20, 160);

        // ─── Controls ─────────────────────────────────────────────────────────
        private Panel    _chatPanel;
        private Panel    _inputPanel;
        private Panel    _headerPanel;
        private FlowLayoutPanel _messageFlow;
        private RichTextBox _inputBox;
        private Button   _sendBtn;
        private Button   _clearBtn;
        private Label    _statusLabel;
        private Panel    _typingPanel;
        private Label    _typingLabel;

        private GeminiService _gemini;

        private bool   _isWaiting = false;

        public ChatForm()
        {
            InitializeComponents();
            string json = File.ReadAllText("appsettings.json");
            string apiKey = JsonDocument.Parse(json).RootElement.GetProperty("GeminiApiKey").GetString()!;
            _gemini = new GeminiService(apiKey);

            AddWelcomeMessage();
        }

        private void InitializeComponents()
        {
            // ── Form ──────────────────────────────────────────────────────────
            Text            = "Efe ile Sohbet";
            Size            = new Size(480, 780);
            MinimumSize     = new Size(400, 600);
            BackColor       = BgDark;
            StartPosition   = FormStartPosition.CenterScreen;
            Font            = new Font("Segoe UI", 10f);
            DoubleBuffered  = true;

            // ── Header ────────────────────────────────────────────────────────
            _headerPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 68,
                BackColor = Color.Transparent
            };
            _headerPanel.Paint += HeaderPanel_Paint;

            var avatarLabel = new Label
            {
                Text      = "M",
                Font      = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = AccentPurple,
                Size      = new Size(42, 42),
                Location  = new Point(14, 13),
                TextAlign = ContentAlignment.MiddleCenter
            };
            MakeCircle(avatarLabel);

            var nameLabel = new Label
            {
                Text      = "Efe",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = LightLila,
                BackColor = Color.Transparent,
                Location  = new Point(64, 12),
                AutoSize  = true
            };

            var onlineLabel = new Label
            {
                Text      = "● çevrimiçi",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(120, 220, 130),
                BackColor = Color.Transparent,
                Location  = new Point(65, 34),
                AutoSize  = true
            };

            _clearBtn = new Button
            {
                Text      = "🗑 Temizle",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(180, 140, 255),
                BackColor = Color.FromArgb(50, 149, 76, 233),
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(82, 28),
                Anchor    = AnchorStyles.Right | AnchorStyles.Top,
                Cursor    = Cursors.Hand
            };
            _clearBtn.FlatAppearance.BorderColor = Color.FromArgb(80, 149, 76, 233);
            _clearBtn.FlatAppearance.BorderSize  = 1;
            _clearBtn.Click += ClearBtn_Click;
            _clearBtn.Location = new Point(_headerPanel.Width - 96, 20);
            _clearBtn.Anchor   = AnchorStyles.Top | AnchorStyles.Right;

            _headerPanel.Controls.AddRange(new Control[] { avatarLabel, nameLabel, onlineLabel, _clearBtn });

            // ── Message area ──────────────────────────────────────────────────
            _chatPanel = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding   = new Padding(0)
            };

            _messageFlow = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                AutoScroll    = true,
                BackColor     = Color.Transparent,
                Padding       = new Padding(10, 10, 10, 10)
            };
            _messageFlow.Paint += MessageFlow_Paint;

            // Typing indicator
            _typingPanel = new Panel
            {
                Visible   = false,
                Height    = 36,
                Dock      = DockStyle.Bottom,
                BackColor = Color.Transparent
            };
            _typingLabel = new Label
            {
                Text      = "Efe yazıyor...",
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(150, 120, 200),
                BackColor = Color.Transparent,
                Location  = new Point(16, 10),
                AutoSize  = true
            };
            _typingPanel.Controls.Add(_typingLabel);

            _chatPanel.Controls.Add(_messageFlow);
            _chatPanel.Controls.Add(_typingPanel);

            // ── Input area ────────────────────────────────────────────────────
            _inputPanel = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 76,
                BackColor = InputBg,
                Padding   = new Padding(10, 10, 10, 10)
            };
            _inputPanel.Paint += InputPanel_Paint;

            _inputBox = new RichTextBox
            {
                BackColor    = Color.FromArgb(38, 18, 72),
                ForeColor    = Color.FromArgb(230, 210, 255),
                BorderStyle  = BorderStyle.None,
                Font         = new Font("Segoe UI", 10.5f),
                ScrollBars   = RichTextBoxScrollBars.None,
                Multiline    = true,
                WordWrap     = true,
                Height       = 38,
                Width        = 300,
                Location     = new Point(12, 18),
                Anchor       = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            _inputBox.KeyDown += InputBox_KeyDown;
            _inputBox.TextChanged += InputBox_TextChanged;

            // Placeholder
            SetPlaceholder();

            _sendBtn = new Button
            {
                Size      = new Size(52, 44),
                Location  = new Point(_inputPanel.Width - 66, 16),
                Anchor    = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Cursor    = Cursors.Hand,
                Text      = "➤",
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold)
            };
            _sendBtn.FlatAppearance.BorderSize  = 0;
            _sendBtn.FlatAppearance.BorderColor = Color.FromArgb(1, 0, 0, 0);
            _sendBtn.Paint  += SendBtn_Paint;
            _sendBtn.Click  += SendBtn_Click;

            _statusLabel = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(150, 120, 200),
                BackColor = Color.Transparent,
                Dock      = DockStyle.Bottom,
                Height    = 16,
                TextAlign = ContentAlignment.MiddleCenter
            };

            _inputPanel.Controls.AddRange(new Control[] { _inputBox, _sendBtn });

            // ── Assemble ──────────────────────────────────────────────────────
            Controls.Add(_chatPanel);
            Controls.Add(_inputPanel);
            Controls.Add(_statusLabel);
            Controls.Add(_headerPanel);

            Resize += (s, e) =>
            {
                _inputBox.Width = _inputPanel.Width - 80;
            };
        }

        // ── Painting ──────────────────────────────────────────────────────────

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var brush = new LinearGradientBrush(ClientRectangle,
                BgDark, BgMid, LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(brush, ClientRectangle);
        }

        private void HeaderPanel_Paint(object sender, PaintEventArgs e)
        {
            var g    = e.Graphics;
            var rect = (sender as Panel).ClientRectangle;
            using var brush = new LinearGradientBrush(rect,
                HeaderBg, Color.FromArgb(18, 6, 40), LinearGradientMode.Vertical);
            g.FillRectangle(brush, rect);

            // Bottom border line
            using var pen = new Pen(Color.FromArgb(80, 149, 76, 233), 1f);
            g.DrawLine(pen, 0, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
        }

        private void MessageFlow_Paint(object sender, PaintEventArgs e)
        {
            var g    = e.Graphics;
            var rect = (sender as FlowLayoutPanel).ClientRectangle;
            using var brush = new LinearGradientBrush(rect,
                BgDark, BgMid, LinearGradientMode.Vertical);
            g.FillRectangle(brush, rect);
        }

        private void InputPanel_Paint(object sender, PaintEventArgs e)
        {
            var g    = e.Graphics;
            var rect = (sender as Panel).ClientRectangle;
            g.FillRectangle(new SolidBrush(InputBg), rect);
            using var pen = new Pen(Color.FromArgb(60, 149, 76, 233), 1f);
            g.DrawLine(pen, 0, 0, rect.Right, 0);

            // Input box rounded border
            int bx = _inputBox.Left - 6;
            int by = _inputBox.Top - 6;
            int bw = _inputBox.Width + 12;
            int bh = _inputBox.Height + 12;
            using var borderPen = new Pen(InputBorder, 1.5f);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            DrawRoundedRectBorder(g, borderPen, new Rectangle(bx, by, bw, bh), 14);
        }

        private void SendBtn_Paint(object sender, PaintEventArgs e)
        {
            var g    = e.Graphics;
            var btn  = sender as Button;
            var rect = new Rectangle(0, 0, btn.Width, btn.Height);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new LinearGradientBrush(rect, SendBtnStart, SendBtnEnd,
                LinearGradientMode.ForwardDiagonal);
            var path = GetRoundedPath(rect, 14);
            g.FillPath(brush, path);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("➤", btn.Font, Brushes.White, rect, sf);
        }

        // ── Message handling ──────────────────────────────────────────────────

        private void AddWelcomeMessage()
        {
            AddMessage(new ChatMessage("yo ne haber, buradayım 🤙", false));
        }

        private void AddMessage(ChatMessage msg)
        {
            var bubble = new ChatBubble(msg, _messageFlow.Width - 20);
            bubble.Margin = new Padding(0, 4, 0, 4);

            if (msg.IsUser)
            {
                bubble.Anchor = AnchorStyles.Right;
                // Align right
                int pad = _messageFlow.Width - bubble.Width - 24;
                bubble.Margin = new Padding(Math.Max(0, pad), 4, 8, 4);
            }
            else
            {
                bubble.Margin = new Padding(8, 4, 0, 4);
            }

            _messageFlow.Controls.Add(bubble);
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            _messageFlow.ScrollControlIntoView(
                _messageFlow.Controls.Count > 0
                    ? _messageFlow.Controls[_messageFlow.Controls.Count - 1]
                    : null);
        }

        private async void SendMessage()
        {
            if (_isWaiting) return;
            string text = _inputBox.Text.Trim();
            if (string.IsNullOrEmpty(text) || text == "Bir şeyler yaz...") return;

            // Clear input
            _inputBox.Clear();
            SetPlaceholder();

            // Show user bubble
            var userMsg = new ChatMessage(text, true);
            AddMessage(userMsg);

            // Show typing indicator
            _isWaiting = true;
            _typingPanel.Visible  = true;
            _statusLabel.Text     = "Efe yazıyor...";
            _sendBtn.Enabled      = false;

            try
            {
                string reply = await _gemini.SendMessageAsync(text);
                var aiMsg = new ChatMessage(reply, false);
                AddMessage(aiMsg);
            }
            catch (Exception ex)
            {
                var errMsg = new ChatMessage($"bağlantı sorunum var sanki 😕 ({ex.Message})", false);
                AddMessage(errMsg);
            }
            finally
            {
                _isWaiting            = false;
                _typingPanel.Visible  = false;
                _statusLabel.Text     = "";
                _sendBtn.Enabled      = true;
                _inputBox.Focus();
            }
        }

        // ── Events ────────────────────────────────────────────────────────────

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                SendMessage();
            }
        }

        private void InputBox_TextChanged(object sender, EventArgs e)
        {
            if (_inputBox.ForeColor == Color.FromArgb(110, 90, 150)) return; // placeholder
        }

        private void SendBtn_Click(object sender, EventArgs e) => SendMessage();

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Sohbeti sıfırlayalım mı?",
                "Temizle",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _messageFlow.Controls.Clear();
                _gemini.ClearHistory();
                AddWelcomeMessage();
            }
        }

        private void SetPlaceholder()
        {
            _inputBox.Text      = "Bir şeyler yaz...";
            _inputBox.ForeColor = Color.FromArgb(110, 90, 150);

            _inputBox.GotFocus += (s, e) =>
            {
                if (_inputBox.Text == "Bir şeyler yaz...")
                {
                    _inputBox.Text      = "";
                    _inputBox.ForeColor = Color.FromArgb(230, 210, 255);
                }
            };
            _inputBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_inputBox.Text))
                    SetPlaceholder();
            };
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void MakeCircle(Control ctrl)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddEllipse(0, 0, ctrl.Width, ctrl.Height);
            ctrl.Region = new Region(path);
        }

        private void DrawRoundedRectBorder(Graphics g, Pen pen, Rectangle rect, int radius)
        {
            g.DrawPath(pen, GetRoundedPath(rect, radius));
        }

        private System.Drawing.Drawing2D.GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            int d    = radius * 2;
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
