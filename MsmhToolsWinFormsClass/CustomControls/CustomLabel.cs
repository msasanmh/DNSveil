using MsmhToolsClass;
using System.ComponentModel;
using System.Windows.Forms.Design;

namespace CustomControls
{
    public class CustomLabel : Label
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override BorderStyle BorderStyle { get; set; }

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
                    Invalidate();
                }
            }
        }

        private bool mBorder = false;
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

        private int mRoundedCorners = 0;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Rounded Corners")]
        public int RoundedCorners
        {
            get { return mRoundedCorners; }
            set
            {
                if (mRoundedCorners != value)
                {
                    mRoundedCorners = value;
                    Invalidate();
                }
            }
        }

        private bool once = true;

        public CustomLabel() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Opaque, true);

            // Default
            FlatStyle = FlatStyle.Flat;
            BackColor = Color.DimGray;
            ForeColor = Color.White;

            Application.Idle += Application_Idle;
            HandleCreated += CustomLabel_HandleCreated;
            RightToLeftChanged += CustomLabel_RightToLeftChanged;
            LocationChanged += CustomLabel_LocationChanged;
            Move += CustomLabel_Move;
            EnabledChanged += CustomLabel_EnabledChanged;
            Paint += CustomLabel_Paint;
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
                    Parent.BackColorChanged += Parent_BackColorChanged;
                    Invalidate();
                    once = false;
                }
            }
        }

        private void Parent_BackColorChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void TopParent_Move(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void Parent_Move(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomLabel_HandleCreated(object? sender, EventArgs e)
        {
            if (sender is Label label)
                label.Invalidate();
        }

        private void CustomLabel_RightToLeftChanged(object? sender, EventArgs e)
        {
            if (sender is Label label)
                label.Refresh();
        }

        private void CustomLabel_LocationChanged(object? sender, EventArgs e)
        {
            if (sender is Label label)
                label.Invalidate();
        }

        private void CustomLabel_Move(object? sender, EventArgs e)
        {
            if (sender is Label label)
                label.Invalidate();
        }

        private void CustomLabel_EnabledChanged(object? sender, EventArgs e)
        {
            if (sender is Label label)
                label.Invalidate();
        }

        private void CustomLabel_Paint(object? sender, PaintEventArgs e)
        {
            if (!Visible) return;

            Color backColor = GetBackColor();
            Color foreColor = GetForeColor();
            Color borderColor = GetBorderColor();
            BorderStyle = BorderStyle.FixedSingle;

            // Paint Background
            if (Parent != null)
            {
                e.Graphics.Clear(Parent.BackColor);
                if (Parent.BackColor == Color.Transparent)
                    if (Parent is TabPage tabPage)
                    {
                        if (tabPage.Parent is CustomTabControl customTabControl)
                        {
                            e.Graphics.Clear(customTabControl.BackColor);
                        }
                        else if (tabPage.Parent is TabControl tabControl)
                        {
                            e.Graphics.Clear(tabControl.BackColor);
                        }
                    }
            }
            else
                e.Graphics.Clear(backColor);

            if (AutoSize)
            {
                try
                {
                    SizeF sizeF = e.Graphics.MeasureString(Text, Font);
                    Width = Convert.ToInt32(Math.Round(sizeF.Width));
                    Height = Convert.ToInt32(Math.Round(sizeF.Height));
                }
                catch (Exception)
                {
                    // do nothing
                }
            }

            Rectangle rect = new(0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1);

            // Paint Label Background
            using SolidBrush sbBG = new(backColor);
            e.Graphics.FillRoundedRectangle(sbBG, rect, RoundedCorners, RoundedCorners, RoundedCorners, RoundedCorners);

            // Paint Image
            if (Image != null)
            {
                Rectangle rectImage = new(0, 0, Image.Width, Image.Height);
                int pad = 2;
                int top = rect.Y + pad;
                int bottom = rect.Y + (rect.Height - rectImage.Height) - pad;
                int left = rect.X + pad;
                int right = rect.X + (rect.Width - rectImage.Width) - pad;
                int centerX = rect.X + ((rect.Width - rectImage.Width) / 2);
                int centerY = rect.Y + ((rect.Height - rectImage.Height) / 2);
                if (RightToLeft == RightToLeft.No)
                {
                    if (ImageAlign == ContentAlignment.BottomCenter)
                        rectImage.Location = new(centerX, bottom);
                    else if (ImageAlign == ContentAlignment.BottomLeft)
                        rectImage.Location = new(left, bottom);
                    else if (ImageAlign == ContentAlignment.BottomRight)
                        rectImage.Location = new(right, bottom);
                    else if (ImageAlign == ContentAlignment.MiddleCenter)
                        rectImage.Location = new(centerX, centerY);
                    else if (ImageAlign == ContentAlignment.MiddleLeft)
                        rectImage.Location = new(left, centerY);
                    else if (ImageAlign == ContentAlignment.MiddleRight)
                        rectImage.Location = new(right, centerY);
                    else if (ImageAlign == ContentAlignment.TopCenter)
                        rectImage.Location = new(centerX, top);
                    else if (ImageAlign == ContentAlignment.TopLeft)
                        rectImage.Location = new(left, top);
                    else if (ImageAlign == ContentAlignment.TopRight)
                        rectImage.Location = new(right, top);
                    else
                        rectImage.Location = new(centerX, centerY);
                }
                else
                {
                    if (ImageAlign == ContentAlignment.BottomCenter)
                        rectImage.Location = new(centerX, bottom);
                    else if (ImageAlign == ContentAlignment.BottomLeft)
                        rectImage.Location = new(right, bottom);
                    else if (ImageAlign == ContentAlignment.BottomRight)
                        rectImage.Location = new(left, bottom);
                    else if (ImageAlign == ContentAlignment.MiddleCenter)
                        rectImage.Location = new(centerX, centerY);
                    else if (ImageAlign == ContentAlignment.MiddleLeft)
                        rectImage.Location = new(right, centerY);
                    else if (ImageAlign == ContentAlignment.MiddleRight)
                        rectImage.Location = new(left, centerY);
                    else if (ImageAlign == ContentAlignment.TopCenter)
                        rectImage.Location = new(centerX, top);
                    else if (ImageAlign == ContentAlignment.TopLeft)
                        rectImage.Location = new(right, top);
                    else if (ImageAlign == ContentAlignment.TopRight)
                        rectImage.Location = new(left, top);
                    else
                        rectImage.Location = new(centerX, centerY);
                }

                e.Graphics.DrawImage(Image, rectImage);
            }

            // Paint Label Text
            TextFormatFlags flags;

            if (RightToLeft == RightToLeft.No)
            {
                if (TextAlign == ContentAlignment.BottomCenter)
                    flags = TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter;
                else if (TextAlign == ContentAlignment.BottomLeft)
                    flags = TextFormatFlags.Bottom | TextFormatFlags.Left;
                else if (TextAlign == ContentAlignment.BottomRight)
                    flags = TextFormatFlags.Bottom | TextFormatFlags.Right;
                else if (TextAlign == ContentAlignment.MiddleCenter)
                    flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
                else if (TextAlign == ContentAlignment.MiddleLeft)
                    flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
                else if (TextAlign == ContentAlignment.MiddleRight)
                    flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
                else if (TextAlign == ContentAlignment.TopCenter)
                    flags = TextFormatFlags.Top | TextFormatFlags.HorizontalCenter;
                else if (TextAlign == ContentAlignment.TopLeft)
                    flags = TextFormatFlags.Top | TextFormatFlags.Left;
                else if (TextAlign == ContentAlignment.TopRight)
                    flags = TextFormatFlags.Top | TextFormatFlags.Right;
                else
                    flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
            }
            else
            {
                if (TextAlign == ContentAlignment.BottomCenter)
                    flags = TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter;
                else if (TextAlign == ContentAlignment.BottomLeft)
                    flags = TextFormatFlags.Bottom | TextFormatFlags.Right;
                else if (TextAlign == ContentAlignment.BottomRight)
                    flags = TextFormatFlags.Bottom | TextFormatFlags.Left;
                else if (TextAlign == ContentAlignment.MiddleCenter)
                    flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
                else if (TextAlign == ContentAlignment.MiddleLeft)
                    flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
                else if (TextAlign == ContentAlignment.MiddleRight)
                    flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
                else if (TextAlign == ContentAlignment.TopCenter)
                    flags = TextFormatFlags.Top | TextFormatFlags.HorizontalCenter;
                else if (TextAlign == ContentAlignment.TopLeft)
                    flags = TextFormatFlags.Top | TextFormatFlags.Right;
                else if (TextAlign == ContentAlignment.TopRight)
                    flags = TextFormatFlags.Top | TextFormatFlags.Left;
                else
                    flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;

                flags |= TextFormatFlags.RightToLeft;
            }

            TextRenderer.DrawText(e.Graphics, Text, Font, rect, foreColor, flags);

            // Paint Label Border
            using Pen penB = new(borderColor);
            using Pen penNB = new(backColor);
            if (Border)
                e.Graphics.DrawRoundedRectangle(penB, rect, RoundedCorners, RoundedCorners, RoundedCorners, RoundedCorners);
            else if (!Border)
                e.Graphics.DrawRoundedRectangle(penNB, rect, RoundedCorners, RoundedCorners, RoundedCorners, RoundedCorners);
        }

        private Color GetBackColor()
        {
            if (Enabled)
                return BackColor;
            else
            {
                Color backColor = BackColor;
                if (Parent != null)
                {
                    if (Parent.BackColor != Color.Transparent)
                    {
                        if (Parent.Enabled)
                            backColor = Parent.BackColor;
                        else
                            backColor = GetDisabledColor(Parent.BackColor);
                    }
                    else
                    {
                        if (FindForm() != null)
                        {
                            if (Parent.Enabled)
                                backColor = FindForm().BackColor;
                            else
                                backColor = GetDisabledColor(FindForm().BackColor);
                        }
                    }
                }
                return backColor;
            }

            static Color GetDisabledColor(Color color)
            {
                if (color.DarkOrLight() == "Dark")
                    return color.ChangeBrightness(0.3f);
                else
                    return color.ChangeBrightness(-0.3f);
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
