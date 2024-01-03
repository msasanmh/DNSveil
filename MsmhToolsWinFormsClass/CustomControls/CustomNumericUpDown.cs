using MsmhToolsClass;
using System.ComponentModel;
using System.Windows.Forms.Design;
/*
* Copyright MSasanMH, June 02, 2022.
*/

namespace CustomControls
{
    public class CustomNumericUpDown : NumericUpDown
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

        private Color? enabledBackColor;
        private bool enabledBackColorBool = true;
        private bool once = true;

        public CustomNumericUpDown() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Opaque, true);

            // Default Back Color
            BackColor = Color.DimGray;
            if (Enabled)
                enabledBackColor = BackColor;

            // Default BorderStyle
            BorderStyle = BorderStyle.FixedSingle;

            Application.Idle += Application_Idle;
            HandleCreated += CustomNumericUpDown_HandleCreated;
            LocationChanged += CustomNumericUpDown_LocationChanged;
            Move += CustomNumericUpDown_Move;
            EnabledChanged += CustomNumericUpDown_EnabledChanged;
            ForeColorChanged += CustomNumericUpDown_ForeColorChanged;
            Invalidated += CustomNumericUpDown_Invalidated;

            Control UpDown = Controls[0];
            UpDown.Paint += UpDown_Paint;
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

        private void CustomNumericUpDown_HandleCreated(object? sender, EventArgs e)
        {
            if (Enabled)
                enabledBackColor = BackColor;
            Invalidate();
        }

        private void CustomNumericUpDown_LocationChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomNumericUpDown_Move(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomNumericUpDown_EnabledChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomNumericUpDown_ForeColorChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomNumericUpDown_Invalidated(object? sender, InvalidateEventArgs e)
        {
            InvalidateSubControls();
        }

        private void InvalidateSubControls()
        {
            for (int n = 0; n < Controls.Count; n++)
            {
                Control c = Controls[n];
                c.ForeColor = ForeColor;
                if (n == 1)
                    c.BackColor = GetBackColor();
                c.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!Visible) return;

            base.OnPaint(e);

            Rectangle rect = ClientRectangle;
            Color backColor = GetBackColor();
            Color borderColor = GetBorderColor();

            e.Graphics.Clear(backColor);

            int r = RoundedCorners;
            using SolidBrush brush = new(backColor);
            using Pen penBorder = new(borderColor);
            Rectangle rectBorder = new(rect.X + 0, rect.Y + 0, rect.Width - 1, rect.Height - 1);
            
            if (BorderStyle == BorderStyle.FixedSingle)
            {
                //ControlPaint.DrawBorder(e.Graphics, rect, borderColor, ButtonBorderStyle.Solid);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.DrawRoundedRectangle(penBorder, rectBorder, r, 0, 0, r);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            }
            else if (BorderStyle == BorderStyle.Fixed3D)
            {
                Color secondBorderColor;
                if (borderColor.DarkOrLight() == "Dark")
                    secondBorderColor = borderColor.ChangeBrightness(0.5f);
                else
                    secondBorderColor = borderColor.ChangeBrightness(-0.5f);

                Rectangle rect3DBorder;

                rect3DBorder = new(rect.X, rect.Y, rect.Width, rect.Height);
                ControlPaint.DrawBorder(e.Graphics, rect3DBorder, secondBorderColor, ButtonBorderStyle.Solid);

                rect3DBorder = new(rect.X + 1, rect.Y + 1, rect.Width - 1, rect.Height - 1);
                ControlPaint.DrawBorder(e.Graphics, rect3DBorder, secondBorderColor, ButtonBorderStyle.Solid);

                rect3DBorder = new(rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                ControlPaint.DrawBorder(e.Graphics, rect3DBorder, borderColor, ButtonBorderStyle.Solid);
            }
        }

        private void UpDown_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Control UpDown) return;
            if (!Visible) return;

            Color backColor = GetBackColor();
            Color foreColor = GetForeColor();
            Color borderColor = GetBorderColor();

            // UpDown Rectangle
            Rectangle rectUpDown = UpDown.ClientRectangle;

            // Up Rectangle
            Rectangle rectUp = new(0, 0, rectUpDown.Width, rectUpDown.Height / 2);

            // Down Rectangle
            Rectangle rectDown = new(0, rectUpDown.Height / 2, rectUpDown.Width, rectUpDown.Height / 2 + 1);

            // Mouse Position
            Point mP = UpDown.PointToClient(MousePosition);

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            // Paint UpDown Background
            e.Graphics.Clear(backColor.ChangeBrightness(-0.3f));
            
            // Paint UpDown Border
            using Pen arrowButtonsBorderPen = new(borderColor);
            Point[] arrowButtonsBorderPointsRight = new[]
            {
                new Point(0, 0),
                new Point(0, rectUpDown.Height),
                new Point(0, rectUpDown.Height / 2),
                new Point(rectUpDown.Width, rectUpDown.Height / 2)
            };
            Point[] arrowButtonsBorderPointsLeft = new[]
            {
                new Point(rectUpDown.Width - 1, 0),
                new Point(rectUpDown.Width - 1, rectUpDown.Height),
                new Point(rectUpDown.Width - 1, rectUpDown.Height / 2),
                new Point(0, rectUpDown.Height / 2)
            };
            if (BorderStyle != BorderStyle.None)
            {
                if (RightToLeft == RightToLeft.No && UpDownAlign == LeftRightAlignment.Right ||
                    RightToLeft == RightToLeft.Yes && UpDownAlign == LeftRightAlignment.Left)
                {
                    e.Graphics.DrawLines(arrowButtonsBorderPen, arrowButtonsBorderPointsRight);
                }
                else
                    e.Graphics.DrawLines(arrowButtonsBorderPen, arrowButtonsBorderPointsLeft);
            }

            // MouseOver Background and Border
            if (Enabled && rectUpDown.Contains(mP))
            {
                // Paint UpDown Background Hover
                Color hoverColor = backColor.DarkOrLight() == "Dark" ? backColor.ChangeBrightness(0.3f) : backColor.ChangeBrightness(-0.3f);
                using SolidBrush sb = new(hoverColor);
                if (rectUp.Contains(mP))
                    e.Graphics.FillRectangle(sb, rectUp);
                else
                    e.Graphics.FillRectangle(sb, rectDown);

                // Paint UpDown Border
                if (BorderStyle != BorderStyle.None)
                {
                    if (RightToLeft == RightToLeft.No && UpDownAlign == LeftRightAlignment.Right ||
                        RightToLeft == RightToLeft.Yes && UpDownAlign == LeftRightAlignment.Left)
                    {
                        e.Graphics.DrawLines(arrowButtonsBorderPen, arrowButtonsBorderPointsRight);
                    }
                    else
                        e.Graphics.DrawLines(arrowButtonsBorderPen, arrowButtonsBorderPointsLeft);
                }
            }

            // Paint Arrows
            using SolidBrush arrowBrush = new(foreColor);

            // UpArrow Points
            Point upArrowCenter;
            if (RightToLeft == RightToLeft.No && UpDownAlign == LeftRightAlignment.Right ||
                RightToLeft == RightToLeft.Yes && UpDownAlign == LeftRightAlignment.Left)
            {
                upArrowCenter = new(rectUp.Left + rectUp.Width / 2, rectUp.Top + rectUp.Height / 2);
            }
            else
                upArrowCenter = new((rectUp.Left + rectUp.Width / 2) - 1, rectUp.Top + rectUp.Height / 2);
            Point[] upArrowPoints = new Point[]
            {
                new Point(upArrowCenter.X - 4, upArrowCenter.Y + 2), // Bottom Left
                new Point(upArrowCenter.X + 4, upArrowCenter.Y + 2), // Bottom Right
                new Point(upArrowCenter.X, upArrowCenter.Y - 3) // Top
            };

            // DownArrow Points
            Point downArrowCenter;
            if (RightToLeft == RightToLeft.No && UpDownAlign == LeftRightAlignment.Right ||
                RightToLeft == RightToLeft.Yes && UpDownAlign == LeftRightAlignment.Left)
            {
                downArrowCenter = new Point(rectDown.Left + rectDown.Width / 2, rectDown.Top + rectDown.Height / 2);
            }
            else
                downArrowCenter = new Point((rectDown.Left + rectDown.Width / 2) - 1, rectDown.Top + rectDown.Height / 2);
            Point[] downArrowPoints = new Point[]
            {
                new Point(downArrowCenter.X - 3, downArrowCenter.Y - 1), // Top Left
                new Point(downArrowCenter.X + 4, downArrowCenter.Y - 1), // Top Right
                new Point(downArrowCenter.X, downArrowCenter.Y + 3) // Bottom
            };

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

            // Paint UpArrow
            e.Graphics.FillPolygon(arrowBrush, upArrowPoints);

            // Paint DownArrow
            e.Graphics.FillPolygon(arrowBrush, downArrowPoints);
        }

        private Color GetBackColor()
        {
            if (Enabled)
            {
                if (enabledBackColor != null)
                    return (Color)(!enabledBackColorBool ? enabledBackColor : BackColor);
                else
                    return BackColor;
            }
            else
            {
                if (enabledBackColorBool)
                {
                    enabledBackColor = BackColor;
                    enabledBackColorBool = false;
                }
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
