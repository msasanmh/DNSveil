using MsmhTools;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Windows.Forms.Design;
/*
* Copyright MSasanMH, May 10, 2022.
*/

namespace CustomControls
{
    public class CustomProgressBar : ProgressBar
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool? RightToLeftLayout { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler? RightToLeftLayoutChanged;

        private Color mBorderColor = Color.Red;
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
                    Invalidate();
                }
            }
        }

        private Color mChunksColor = Color.LightBlue;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Editor(typeof(WindowsFormsComponentEditor), typeof(Color))]
        [Category("Appearance"), Description("Chunks Color")]
        public Color ChunksColor
        {
            get { return mChunksColor; }
            set
            {
                if (mChunksColor != value)
                {
                    mChunksColor = value;
                    Invalidate();
                }
            }
        }

        private string mCustomText = string.Empty;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Custom Text")]
        public string CustomText
        {
            get { return mCustomText; }
            set
            {
                if (mCustomText != value)
                {
                    mCustomText = value;
                    Invalidate();
                }
            }
        }

        private Font mFont = DefaultFont;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Text Font")]
        public override Font Font
        {
            get { return mFont; }
            set
            {
                if (mFont != value)
                {
                    mFont = value;
                    Invalidate();
                }
            }
        }

        private DateTime? mStartTime = null;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Set Start Time Programmatically (Optional)")]
        public DateTime? StartTime
        {
            get { return mStartTime; }
            set
            {
                if (mStartTime != value)
                {
                    mStartTime = value;
                    Invalidate();
                }
            }
        }

        private static bool ApplicationIdle = false;
        private TimeSpan elapsedTime;
        private string? elapsedTimeString;
        private int once = 0;
        private bool onceIV = true;

        public CustomProgressBar() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            // Default
            BackColor = Color.DimGray;
            ForeColor = Color.White;

            Application.Idle += Application_Idle;
            HandleCreated += CustomProgressBar_HandleCreated;
            EnabledChanged += CustomProgressBar_EnabledChanged;
            RightToLeftChanged += CustomProgressBar_RightToLeftChanged;
        }

        private void Application_Idle(object? sender, EventArgs e)
        {
            ApplicationIdle = true;
            if (Parent != null && FindForm() != null)
            {
                if (onceIV)
                {
                    Control topParent = FindForm();
                    topParent.Move -= TopParent_Move;
                    topParent.Move += TopParent_Move;
                    Parent.Move -= Parent_Move;
                    Parent.Move += Parent_Move;
                    Invalidate();
                    onceIV = false;
                }
            }
            RightToLeftLayout = false;
        }

        private void TopParent_Move(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void Parent_Move(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomProgressBar_HandleCreated(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomProgressBar_EnabledChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomProgressBar_RightToLeftChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!ApplicationIdle)
                return;
            
            Color backColor = GetBackColor();
            Color foreColor = GetForeColor();
            Color borderColor = GetBorderColor();
            Color chunksColor = GetChunksColor();
            Color chunksColorGradient;

            if (chunksColor.DarkOrLight() == "Dark")
                chunksColorGradient = chunksColor.ChangeBrightness(0.5f);
            else
                chunksColorGradient = chunksColor.ChangeBrightness(-0.5f);

            Rectangle rect = ClientRectangle;
            Graphics g = e.Graphics;
            // Draw horizontal bar (Background and Border) With Default System Color:
            //ProgressBarRenderer.DrawHorizontalBar(g, rect);

            // Draw horizontal bar (Background and Border) With Custom Color:
            // Fill Background
            using SolidBrush bgBrush = new(backColor);
            g.FillRectangle(bgBrush, rect);

            // Draw Border
            ControlPaint.DrawBorder(g, rect, borderColor, ButtonBorderStyle.Solid);

            // Padding
            if (Value > 0)
            {
                // Draw Chunks By Default Color (Green):
                //Rectangle clip = new(rect.X, rect.Y, (int)Math.Round((float)Value / Maximum * rect.Width), rect.Height);
                //ProgressBarRenderer.DrawHorizontalChunks(g, clip);

                // Draw Chunks By Custom Color:
                // The Following Is The Width Of The Bar. This Will Vary With Each Value.
                int fillWidth = Width * Value / (Maximum - Minimum);

                // GDI+ Doesn't Like Rectangles 0px Wide or Height
                if (fillWidth == 0)
                {
                    // Draw Only Border And Exit
                    ControlPaint.DrawBorder(g, rect, borderColor, ButtonBorderStyle.Solid);
                    return;
                }
                // Rectangles For Upper And Lower Half Of Bar
                Rectangle topRect = new(0, 0, fillWidth, Height / 2);
                Rectangle buttomRect = new(0, Height / 2, fillWidth, Height / 2);

                // Paint Upper Half
                using LinearGradientBrush gbUH = new(new Point(0, 0), new Point(0, Height / 2), chunksColorGradient, chunksColor);
                e.Graphics.FillRectangle(gbUH, topRect);

                // Paint Lower Half
                // (this.Height/2 - 1 Because There Would Be A Dark Line In The Middle Of The Bar)
                using LinearGradientBrush gbLH = new(new Point(0, Height / 2 - 1), new Point(0, Height), chunksColor, chunksColorGradient);
                e.Graphics.FillRectangle(gbLH, buttomRect);

                // Paint Border
                ControlPaint.DrawBorder(g, rect, borderColor, ButtonBorderStyle.Solid);
            }

            // Compute Percent
            int percent = (int)(Value / (double)Maximum * 100);
            string textPercent;
            if (Value > 0)
                textPercent = percent.ToString() + '%';
            else
            {
                // If Value Is Zero Don't Write Anything
                textPercent = string.Empty;
                if (!DesignMode)
                    CustomText = string.Empty;
            }

            // Brush For Writing CustomText And Persentage On Progressbar
            using SolidBrush brush = new(foreColor);

            // Percent
            SizeF lenPercent = g.MeasureString(textPercent, Font);
            Point locationPercentCenter = new(Convert.ToInt32((Width / 2) - lenPercent.Width / 2), Convert.ToInt32((Height / 2) - lenPercent.Height / 2));
            g.DrawString(textPercent, Font, brush, locationPercentCenter);

            // Custom Text
            if (!string.IsNullOrEmpty(CustomText))
            {
                SizeF lenCustomText = g.MeasureString(CustomText, Font);
                if (RightToLeft == RightToLeft.No)
                {
                    Point locationCustomTextLeft = new(5, Convert.ToInt32((Height / 2) - lenCustomText.Height / 2));
                    g.DrawString(CustomText, Font, brush, locationCustomTextLeft);
                }
                else
                {
                    Point locationCustomTextRight = new(Convert.ToInt32(Width - lenCustomText.Width - 5), Convert.ToInt32((Height / 2) - lenCustomText.Height / 2));
                    g.DrawString(CustomText, Font, brush, locationCustomTextRight);
                }
            }
            
            // Compute Elapsed Time
            if (StartTime.HasValue == true)
            {
                if (percent == 0)
                {
                    elapsedTime = new(0, 0, 0, 0, 0);
                    elapsedTimeString = string.Empty;
                }
                else if (1 < percent && percent < 100)
                {
                    elapsedTime = (TimeSpan)(DateTime.Now - StartTime);
                    elapsedTimeString = string.Format(@"Time: {0:00}:{1:00}:{2:000}", elapsedTime.Minutes, elapsedTime.Seconds, Math.Round((double)elapsedTime.Milliseconds / 10));
                    once = 0;
                }
                else if (percent == 100)
                {
                    if (once == 0)
                    {
                        elapsedTime = (TimeSpan)(DateTime.Now - StartTime);
                        elapsedTimeString = string.Format(@"Time: {0:00}:{1:00}:{2:000}", elapsedTime.Minutes, elapsedTime.Seconds, Math.Round((double)elapsedTime.Milliseconds / 10));
                        once++;
                    }
                }

                SizeF lenElapsedTime = g.MeasureString(elapsedTimeString, Font);
                if (RightToLeft == RightToLeft.No)
                {
                    Point locationElapsedTimeRight = new(Convert.ToInt32(Width - lenElapsedTime.Width - 5), Convert.ToInt32((Height / 2) - lenElapsedTime.Height / 2));
                    g.DrawString(elapsedTimeString, Font, brush, locationElapsedTimeRight);
                }
                else
                {
                    Point locationElapsedTimeLeft = new(5, Convert.ToInt32((Height / 2) - lenElapsedTime.Height / 2));
                    g.DrawString(elapsedTimeString, Font, brush, locationElapsedTimeLeft);
                }
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

        private Color GetChunksColor()
        {
            if (Enabled)
                return ChunksColor;
            else
            {
                if (ChunksColor.DarkOrLight() == "Dark")
                    return ChunksColor.ChangeBrightness(0.3f);
                else
                    return ChunksColor.ChangeBrightness(-0.3f);
            }
        }

    }
}
