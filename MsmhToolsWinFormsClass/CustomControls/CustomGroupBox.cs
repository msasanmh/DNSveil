using MsmhToolsClass;
using MsmhToolsWinFormsClass;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Windows.Forms.Design;
/*
* Copyright MSasanMH, May 01, 2022.
*/

namespace CustomControls
{
    public class CustomGroupBox : GroupBox
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new FlatStyle FlatStyle { get; set; }

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

        private bool ApplicationIdle = false;
        private bool once = true;

        private Point GetPoint = new(0, 0);
        private string GetName = string.Empty;

        public CustomGroupBox() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Opaque, true);

            FlatStyle = FlatStyle.Flat;

            Application.Idle += Application_Idle;
            HandleCreated += CustomGroupBox_HandleCreated;
            Invalidated += CustomGroupBox_Invalidated;
            LocationChanged += CustomGroupBox_LocationChanged;
            Move += CustomGroupBox_Move;
            ControlAdded += CustomGroupBox_ControlAdded;
            ControlRemoved += CustomGroupBox_ControlRemoved;
            Enter += CustomGroupBox_Enter;
            MouseEnter += CustomGroupBox_MouseEnter;
            MouseLeave += CustomGroupBox_MouseLeave;
            ParentChanged += CustomGroupBox_ParentChanged;
            Resize += CustomGroupBox_Resize;
            SizeChanged += CustomGroupBox_SizeChanged;
            EnabledChanged += CustomGroupBox_EnabledChanged;
            Paint += CustomGroupBox_Paint;
        }

        private void Application_Idle(object? sender, EventArgs e)
        {
            ApplicationIdle = true;
            if (Parent != null && FindForm() != null && !DesignMode)
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
            if (GetPoint != PointToScreen(Location) && GetName == Name)
            {
                if (Parent != null)
                {
                    Control topParent = FindForm();
                    if (topParent.Visible && Visible)
                    {
                        Debug.WriteLine("Top Parent of " + Name + " is " + topParent.Name);
                        topParent.Refresh(); // Needed when there are many GroupBoxes.
                    }
                }
            }
            Invalidate();
        }

        private void Parent_Move(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomGroupBox_HandleCreated(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomGroupBox_Invalidated(object? sender, InvalidateEventArgs e)
        {
            if (BackColor.DarkOrLight() == "Dark")
                this.SetDarkControl();
        }

        private void CustomGroupBox_LocationChanged(object? sender, EventArgs e)
        {
            if (sender is GroupBox groupBox)
                groupBox.Invalidate();
        }

        private void CustomGroupBox_Move(object? sender, EventArgs e)
        {
            if (sender is GroupBox groupBox)
                groupBox.Invalidate();
        }

        private void CustomGroupBox_ControlAdded(object? sender, ControlEventArgs e)
        {
            if (sender is GroupBox groupBox)
                groupBox.Invalidate();
        }

        private void CustomGroupBox_ControlRemoved(object? sender, ControlEventArgs e)
        {
            if (sender is GroupBox groupBox)
                groupBox.Invalidate();
        }

        private void CustomGroupBox_Enter(object? sender, EventArgs e)
        {
            if (sender is GroupBox groupBox)
                groupBox.Invalidate();
        }

        private void CustomGroupBox_MouseEnter(object? sender, EventArgs e)
        {
            if (sender is GroupBox groupBox)
                groupBox.Invalidate();
        }

        private void CustomGroupBox_MouseLeave(object? sender, EventArgs e)
        {
            if (sender is GroupBox groupBox)
                groupBox.Invalidate();
        }

        private void CustomGroupBox_ParentChanged(object? sender, EventArgs e)
        {
            if (sender is GroupBox groupBox)
                groupBox.Invalidate();
        }

        private void CustomGroupBox_Resize(object? sender, EventArgs e)
        {
            if (sender is GroupBox groupBox)
                groupBox.Invalidate();
        }

        private void CustomGroupBox_SizeChanged(object? sender, EventArgs e)
        {
            if (sender is GroupBox groupBox)
                groupBox.Invalidate();
        }

        private void CustomGroupBox_EnabledChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomGroupBox_Paint(object? sender, PaintEventArgs e)
        {
            if (!ApplicationIdle) return;
            if (!Visible) return;

            GetPoint = PointToScreen(Location);
            GetName = Name;

            if (sender is GroupBox box)
            {
                Color backColor = GetBackColor(box);
                Color foreColor = GetForeColor();
                Color borderColor = GetBorderColor();

                SizeF strSize = e.Graphics.MeasureString(box.Text, box.Font);
                Rectangle rect = new(box.ClientRectangle.X,
                                               box.ClientRectangle.Y + (int)(strSize.Height / 2),
                                               box.ClientRectangle.Width - 1,
                                               box.ClientRectangle.Height - (int)(strSize.Height / 2) - 1);

                e.Graphics.Clear(backColor);

                // Draw Text
                using SolidBrush sbForeColor = new(foreColor);
                // Draw Border
                using Pen penBorder = new(borderColor);

                int radius = RoundedCorners;
                int diameter = radius * 2;
                int spacer = diameter + box.Padding.Left + 1;

                Point p1 = new (0, 0);
                Point p2 = new(0, 0);
                Point p3 = new(0, 0);
                Point p4 = new(0, 0);
                Point p5 = new(0, 0);
                Point p6 = new(0, 0);
                Point p7 = new(0, 0);
                Point p8 = new(0, 0);
                Point p9 = new(0, 0);
                Point p10 = new(0, 0);

                if (box.RightToLeft == RightToLeft.Yes)
                {
                    if (string.IsNullOrEmpty(box.Text))
                        p1 = new(rect.X + (rect.Width / 2), rect.Y);
                    else
                        p1 = new(rect.X + rect.Width - box.Padding.Left - 1 - diameter, rect.Y);
                    p2 = new(rect.X + rect.Width, rect.Y);
                    p3 = new(rect.X + rect.Width, rect.Y + rect.Height);
                    p4 = new(rect.X + rect.Width, rect.Y);
                    p5 = new(rect.X + rect.Width, rect.Y + rect.Height);
                    p6 = new(rect.X, rect.Y + rect.Height);
                    p7 = new(rect.X, rect.Y + rect.Height);
                    p8 = rect.Location;
                    p9 = new(rect.X, rect.Y);
                    p10 = new(rect.X + rect.Width - box.Padding.Left - 1 - Convert.ToInt32(strSize.Width), rect.Y);
                }
                else
                {
                    p1 = new(rect.X + box.Padding.Left - 1 + Convert.ToInt32(strSize.Width), rect.Y);
                    p2 = new(rect.X + rect.Width, rect.Y);
                    p3 = new(rect.X + rect.Width, rect.Y + rect.Height);
                    p4 = new(rect.X + rect.Width, rect.Y);
                    p5 = new(rect.X + rect.Width, rect.Y + rect.Height);
                    p6 = new(rect.X, rect.Y + rect.Height);
                    p7 = new(rect.X, rect.Y + rect.Height);
                    p8 = rect.Location;
                    p9 = new(rect.X, rect.Y);
                    if (string.IsNullOrEmpty(box.Text))
                        p10 = new(rect.X + (rect.Width / 2), rect.Y);
                    else
                        p10 = new(rect.X + box.Padding.Left + radius, rect.Y);
                }

                Point[] points = new Point[] { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10 };
                
                // Draw Text
                if (box.RightToLeft == RightToLeft.Yes)
                    e.Graphics.DrawString(box.Text, box.Font, sbForeColor, box.Width - spacer - strSize.Width, 0);
                else
                    e.Graphics.DrawString(box.Text, box.Font, sbForeColor, spacer, 0);

                GraphicsPath path = new();
                if (box.RightToLeft != RightToLeft.Yes)
                    p1.X += spacer;
                p2.X -= diameter;
                path.AddLine(p1, p2);
                Rectangle r = new(p2, new Size(diameter, diameter));
                if (radius == 0)
                    path.AddLine(r.Location, r.Location);
                else
                    path.AddArc(r, 270, 90);
                p3.Y -= diameter;
                p4.Y += diameter;
                path.AddLine(p3, p4);
                p5.X -= diameter;
                p5.Y -= diameter;
                r = new(p5, new Size(diameter, diameter));
                if (radius == 0)
                    path.AddLine(r.Location, r.Location);
                else
                    path.AddArc(r, 0, 90);
                p5.Y += diameter;
                p6.X += diameter;
                path.AddLine(p5, p6);
                p6.X -= diameter;
                p6.Y -= diameter;
                r = new(p6, new Size(diameter, diameter));
                if (radius == 0)
                    path.AddLine(r.Location, r.Location);
                else
                    path.AddArc(r, 90, 90);
                p7.Y -= diameter;
                p8.Y += diameter;
                path.AddLine(p7, p8);
                p8.Y -= diameter;
                r = new(p8, new Size(diameter, diameter));
                if (radius == 0)
                    path.AddLine(r.Location, r.Location);
                else
                    path.AddArc(r, 180, 90);
                p9.X += diameter;
                if (box.RightToLeft == RightToLeft.Yes)
                    p10.X -= spacer;
                path.AddLine(p9, p10);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(penBorder, path);
                e.Graphics.SmoothingMode = SmoothingMode.Default;
            }
        }

        private Color GetBackColor(GroupBox groupBox)
        {
            if (groupBox.Enabled)
                return BackColor;
            else
            {
                if (groupBox.Parent != null)
                {
                    if (groupBox.Parent.Enabled == false)
                        return GetDisabledColor();
                    else
                        return GetDisabledColor();
                }
                else
                {
                    return GetDisabledColor();
                }

                Color GetDisabledColor()
                {
                    if (BackColor.DarkOrLight() == "Dark")
                        return BackColor.ChangeBrightness(0.3f);
                    else
                        return BackColor.ChangeBrightness(-0.3f);
                }
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
