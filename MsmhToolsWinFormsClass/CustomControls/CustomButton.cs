using MsmhToolsClass;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Windows.Forms.Design;
/*
* Copyright MSasanMH, April 19, 2022.
*/

namespace CustomControls
{
    public class CustomButton : Button
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new FlatButtonAppearance? FlatAppearance { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new TextImageRelation? TextImageRelation { get; set; }

        private Color mBackColor = Color.DimGray;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Editor(typeof(WindowsFormsComponentEditor), typeof(Color))]
        [Category("Appearance"), Description("Back Color")]
        public override Color BackColor
        {
            get { return mBackColor; }
            set
            {
                if (mBackColor != value)
                {
                    mBackColor = value;
                    Invalidate();
                }
            }
        }

        private Color mForeColor = Color.White;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Editor(typeof(WindowsFormsComponentEditor), typeof(Color))]
        [Category("Appearance"), Description("Fore Color")]
        public override Color ForeColor
        {
            get { return mForeColor; }
            set
            {
                if (mForeColor != value)
                {
                    mForeColor = value;
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
                    Invalidate();
                }
            }
        }

        private Color mSelectionColor = Color.LightBlue;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Editor(typeof(WindowsFormsComponentEditor), typeof(Color))]
        [Category("Appearance"), Description("Selection Color")]
        public Color SelectionColor
        {
            get { return mSelectionColor; }
            set
            {
                if (mSelectionColor != value)
                {
                    mSelectionColor = value;
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

        private bool ButtonMouseHover { get; set; }
        private bool ButtonMouseDown { get; set; }
        private bool ApplicationIdle = false;
        private bool once = true;

        public CustomButton() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Opaque, true);
            
            FlatStyle = FlatStyle.Flat;
            
            Application.Idle += Application_Idle;
            HandleCreated += CustomButton_HandleCreated;
            LocationChanged += CustomButton_LocationChanged;
            Move += CustomButton_Move;
            MouseDown += CustomButton_MouseDown;
            MouseUp += CustomButton_MouseUp;
            MouseEnter += CustomButton_MouseEnter;
            MouseLeave += CustomButton_MouseLeave;
            EnabledChanged += CustomButton_EnabledChanged;
            Paint += CustomButton_Paint;
        }

        private void Application_Idle(object? sender, EventArgs e)
        {
            ApplicationIdle = true;
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

        private void CustomButton_HandleCreated(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomButton_LocationChanged(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                button.Invalidate();
            }
        }

        private void CustomButton_Move(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                button.Invalidate();
            }
        }

        private void CustomButton_MouseDown(object? sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                ButtonMouseDown = true;
                button.Invalidate();
            }
        }

        private void CustomButton_MouseUp(object? sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                ButtonMouseDown = false;
                button.Invalidate();
            }
        }

        private void CustomButton_MouseEnter(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                ButtonMouseHover = true;
                button.Invalidate();
            }
        }

        private void CustomButton_MouseLeave(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                ButtonMouseHover = false;
                button.Invalidate();
            }
        }

        private void CustomButton_EnabledChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomButton_Paint(object? sender, PaintEventArgs e)
        {
            if (ApplicationIdle == false) return;

            try
            {
                if (sender is Button button)
                {
                    Color backColor = GetBackColor(button);
                    Color foreColor = GetForeColor();
                    Color borderColor = GetBorderColor();

                    // Paint Background
                    if (Parent != null)
                    {
                        e.Graphics.Clear(Parent.BackColor);
                        if (Parent.BackColor == Color.Transparent)
                            if (Parent is TabPage tabPage)
                            {
                                if (tabPage.Parent is CustomTabControl customTabControl)
                                    e.Graphics.Clear(customTabControl.BackColor);
                                else if (tabPage.Parent is TabControl tabControl)
                                    e.Graphics.Clear(tabControl.BackColor);
                            }
                    }
                    else
                        e.Graphics.Clear(backColor);

                    Rectangle rect = new(0, 0, button.ClientSize.Width - 1, button.ClientSize.Height - 1);

                    // Paint Button Background
                    using SolidBrush sbBG = new(backColor);
                    e.Graphics.FillRoundedRectangle(sbBG, rect, RoundedCorners, RoundedCorners, RoundedCorners, RoundedCorners);

                    // Paint Hover Button Background (// MousePosition Or Cursor.Position) (Faster than rect.Contains(button.PointToClient(MousePosition)))
                    if (button.PointToScreen(Point.Empty).X <= MousePosition.X
                            && MousePosition.X <= (button.PointToScreen(Point.Empty).X + rect.Width)
                            && button.PointToScreen(Point.Empty).Y <= MousePosition.Y
                            && MousePosition.Y <= (button.PointToScreen(Point.Empty).Y + rect.Height))
                    {
                        if (ButtonMouseHover)
                        {
                            if (ButtonMouseDown)
                            {
                                Color mouseDownBackColor;
                                if (backColor.DarkOrLight() == "Dark")
                                    mouseDownBackColor = backColor.ChangeBrightness(0.2f);
                                else
                                    mouseDownBackColor = backColor.ChangeBrightness(-0.2f);

                                using SolidBrush sbDBG = new(mouseDownBackColor);
                                e.Graphics.FillRoundedRectangle(sbDBG, rect, RoundedCorners, RoundedCorners, RoundedCorners, RoundedCorners);
                                ButtonMouseDown = false; // Fix a minor bug.
                            }
                            else
                            {
                                Color mouseHoverBackColor;
                                if (backColor.DarkOrLight() == "Dark")
                                    mouseHoverBackColor = BackColor.ChangeBrightness(0.1f);
                                else
                                    mouseHoverBackColor = BackColor.ChangeBrightness(-0.1f);

                                using SolidBrush sbHBG = new(mouseHoverBackColor);
                                e.Graphics.FillRoundedRectangle(sbHBG, rect, RoundedCorners, RoundedCorners, RoundedCorners, RoundedCorners);
                            }
                        }
                    }

                    if (button.Enabled && button.Focused)
                    {
                        rect.Inflate(-2, -2);
                        using Pen pen = new(SelectionColor) { DashStyle = DashStyle.Dash };
                        //if (RoundedCorners == 0)
                        //    e.Graphics.DrawRectangle(pen, rect);
                        //else
                        e.Graphics.DrawRoundedRectangle(pen, rect, RoundedCorners, RoundedCorners, RoundedCorners, RoundedCorners);
                        rect.Inflate(+2, +2);
                    }

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

                    // Paint Button Text
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

                    TextRenderer.DrawText(e.Graphics, button.Text, button.Font, rect, foreColor, flags);

                    // Paint Button Border
                    using Pen penB = new(borderColor);
                    e.Graphics.DrawRoundedRectangle(penB, rect, RoundedCorners, RoundedCorners, RoundedCorners, RoundedCorners);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CustomButton Paint: " + ex.Message);
            }
        }

        private Color GetBackColor(Button button)
        {
            if (button.Enabled)
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
