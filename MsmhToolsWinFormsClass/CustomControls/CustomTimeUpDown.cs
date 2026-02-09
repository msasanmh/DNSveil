using MsmhToolsClass;
using System.ComponentModel;
using System.Windows.Forms.Design;

namespace CustomControls
{
    public class CustomTimeUpDown : UserControl
    {
        protected override CreateParams CreateParams // Fixes Flickers
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;   // WS_EX_COMPOSITED
                return cp;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new BorderStyle BorderStyle { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool AutoScroll { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Size AutoScrollMargin { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Size AutoScrollMinSize { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool AutoSize { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new AutoSizeMode AutoSizeMode { get; set; }

        private readonly MaskedTextBox MaskedTextBox1 = new();
        private readonly CustomNumericUpDown NumericUpDown1 = new();

        protected bool mBorder = true;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Border")]
        public bool Border
        {
            get { return mBorder; }
            set
            {
                if (mBorder != value)
                {
                    mBorder = value;
                    Invalidate();
                }
            }
        }

        private Color mBorderColor = Color.Blue;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Editor(typeof(WindowsFormsComponentEditor), typeof(Color))]
        [Category("Appearance"), Description("Border Color")]
        public Color BorderColor
        {
            get { return mBorderColor; }
            set
            {
                if (mBorderColor != value)
                {
                    mBorderColor = value;
                    NumericUpDown1.BorderColor = value;
                    Invalidate();
                }
            }
        }

        private bool once = true;
        private const decimal MaxTimeTotalMilliseconds = 359999999; // new TimeConvert(99, 59, 59, 999).TotalMilliseconds

        private decimal mValue = 0;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Value (Milliseconds)")]
        public decimal Value
        {
            get { return mValue; }
            set
            {
                if (mValue != value)
                {
                    if (value > MaxTimeTotalMilliseconds)
                        value = MaxTimeTotalMilliseconds;
                    else if (value < 0)
                        value = 0;

                    mValue = value;
                    NumericUpDown1.Value = value;
                    TimeConvert timeCode = new((double)value);
                    MaskedTextBox1.Text = timeCode.ToString();
                    Invalidate();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Action"), Description("Value Changed Event")]
        public event EventHandler? ValueChanged;

        public CustomTimeUpDown() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            UpdateSizeAndColor();
            
            // Default
            BackColor = Color.DimGray;
            ForeColor = Color.White;
            NumericUpDown1.Value = 0;
            NumericUpDown1.Increment = 100;
            NumericUpDown1.Minimum = 0;
            NumericUpDown1.Maximum = 359999999; // ~100 Hours
            MaskedTextBox1.Text = "00:00:00.000";

            Controls.Add(NumericUpDown1.Controls[0]);
            Controls.Add(MaskedTextBox1);
            MaskedTextBox1.BringToFront();
            
            NumericUpDown1.ValueChanged += NumericUpDown1_ValueChanged;
            MaskedTextBox1.TextChanged += MaskedTextBox1_TextChanged;
            MaskedTextBox1.KeyDown += MaskedTextBox1_KeyDown;

            Application.Idle += Application_Idle;
            HandleCreated += CustomTimeUpDown_HandleCreated;
            FontChanged += CustomTimeUpDown_FontChanged;
            LocationChanged += CustomTimeUpDown_LocationChanged;
            Move += CustomTimeUpDown_Move;
            EnabledChanged += CustomTimeUpDown_EnabledChanged;
            Paint += CustomTimeUpDown_Paint;
            
        }

        private void Application_Idle(object? sender, EventArgs e)
        {
            if (Parent != null && FindForm() != null)
            {
                if (once)
                {
                    Control topParent = FindForm();
                    topParent.Move -= TopParent_Move;
                    topParent.Move += TopParent_Move;
                    Parent.Move -= Parent_Move;
                    Parent.Move += Parent_Move;
                    Invalidate();
                    once = false;
                }
            }
        }

        private void TopParent_Move(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void Parent_Move(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomTimeUpDown_HandleCreated(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomTimeUpDown_FontChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomTimeUpDown_LocationChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomTimeUpDown_Move(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomTimeUpDown_EnabledChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomTimeUpDown_Paint(object? sender, PaintEventArgs e)
        {
            if (!Visible) return;

            UpdateSizeAndColor();
            e.Graphics.Clear(GetBackColor());
            if (Border)
            {
                ControlPaint.DrawBorder(e.Graphics, ClientRectangle, GetBorderColor(), ButtonBorderStyle.Solid);
            }
        }

        private void UpdateSizeAndColor()
        {
            Color backColor = GetBackColor();
            Color foreColor = GetForeColor();

            int UpDownWidth = 18;
            BorderStyle = BorderStyle.None;
            AutoSize = false;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            MinimumSize = new(0, 0);
            MaximumSize = new(0, 0);
            int width = (int)Math.Round(Graphics.FromImage(new Bitmap(1, 1)).MeasureString("00:00:00.000", Font).Width); // More accurate than MeasureText()
            NumericUpDown1.Font = Font;
            NumericUpDown1.AutoSize = false;
            NumericUpDown1.Size = new(width + UpDownWidth, 0);
            Size = NumericUpDown1.Size;
            ClientSize = Size;
            NumericUpDown1.BorderStyle = BorderStyle.FixedSingle;
            if (Border)
                NumericUpDown1.BorderColor = BorderColor;
            else
                NumericUpDown1.BorderColor = BackColor;
            NumericUpDown1.Margin = new(0);
            NumericUpDown1.TabStop = false;
            NumericUpDown1.BackColor = backColor;
            NumericUpDown1.ForeColor = foreColor;
            MaskedTextBox1.InsertKeyMode = InsertKeyMode.Overwrite;
            MaskedTextBox1.Mask = "00:00:00.000";
            MaskedTextBox1.PromptChar = '_';
            MaskedTextBox1.AutoSize = false;
            MaskedTextBox1.Size = new(ClientRectangle.Width - (UpDownWidth + 2), ClientRectangle.Height - 4);
            MaskedTextBox1.Margin = new(0);
            MaskedTextBox1.InsertKeyMode = InsertKeyMode.Overwrite;
            MaskedTextBox1.BorderStyle = BorderStyle.None;
            MaskedTextBox1.BackColor = backColor;
            MaskedTextBox1.ForeColor = foreColor;
            if (RightToLeft == RightToLeft.No)
            {
                NumericUpDown1.RightToLeft = RightToLeft.No;
                MaskedTextBox1.RightToLeft = RightToLeft.No;
                NumericUpDown1.Location = new(0, 0);
                MaskedTextBox1.Location = new(2, 2);
            }
            else
            {
                NumericUpDown1.RightToLeft = RightToLeft.Yes;
                MaskedTextBox1.RightToLeft = RightToLeft.Yes;
                NumericUpDown1.Location = new(0, 0);
                MaskedTextBox1.Location = new(18, 2);
            }
            NumericUpDown1.Enabled = Enabled;
            MaskedTextBox1.Enabled = Enabled;
        }

        private void NumericUpDown1_ValueChanged(object? sender, EventArgs e)
        {
            Value = NumericUpDown1.Value;
            TimeConvert timeCode = new((double)NumericUpDown1.Value);
            MaskedTextBox1.Text = timeCode.ToString();
            ValueChanged?.Invoke(this, e);
        }

        private void MaskedTextBox1_TextChanged(object? sender, EventArgs e)
        {
            decimal value = (decimal)TimeConvert.ParseToMilliseconds(MaskedTextBox1.Text);
            if (value != Value)
            {
                if (NumericUpDown1.Minimum <= value && value <= NumericUpDown1.Maximum)
                {
                    if (NumericUpDown1.Value != value)
                        NumericUpDown1.Value = value;
                }
                else
                {
                    if (NumericUpDown1.Minimum > value)
                    {
                        MaskedTextBox1.Text = "00:00:00.000";
                        NumericUpDown1.Value = NumericUpDown1.Minimum;
                    }
                    else if (value > NumericUpDown1.Maximum)
                    {
                        MaskedTextBox1.Text = "99:59:59.999";
                        NumericUpDown1.Value = NumericUpDown1.Maximum;
                    }
                }
            }
        }

        private void MaskedTextBox1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Up)
            {
                NumericUpDown1.UpButton();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == Keys.Down)
            {
                NumericUpDown1.DownButton();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == Keys.Enter)
            {
                ValueChanged?.Invoke(this, e);
                e.SuppressKeyPress = true;
            }
        }

        private Color GetBackColor()
        {
            if (Enabled)
                return BackColor;
            else
            {
                if (BackColor.DarkOrLight() == "Dark")
                    return BackColor.ChangeBrightness(0.3f);
                else
                    return BackColor.ChangeBrightness(-0.3f);
            }
        }

        private Color GetForeColor()
        {
            if (Enabled)
                return ForeColor;
            else
            {
                if (ForeColor.DarkOrLight() == "Dark")
                    return ForeColor.ChangeBrightness(0.2f);
                else
                    return ForeColor.ChangeBrightness(-0.2f);
            }
        }

        private Color GetBorderColor()
        {
            if (Enabled)
                return BorderColor;
            else
            {
                if (BorderColor.DarkOrLight() == "Dark")
                    return BorderColor.ChangeBrightness(0.3f);
                else
                    return BorderColor.ChangeBrightness(-0.3f);
            }
        }

    }
}
