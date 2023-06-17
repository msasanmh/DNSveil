using MsmhTools;
using System;
using System.ComponentModel;
using System.Diagnostics;
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
            if (ApplicationIdle == false)
                return;

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

                if (box.RightToLeft == RightToLeft.Yes)
                {
                    // Draw Text
                    e.Graphics.DrawString(box.Text, box.Font, sbForeColor, box.Width - (box.Padding.Left + 1) - strSize.Width, 0);
                    // Draw Border TopLeft
                    e.Graphics.DrawLine(penBorder, new Point(rect.X, rect.Y), new Point((rect.X + rect.Width) - (box.Padding.Left + 1) - (int)strSize.Width, rect.Y));
                    // Draw Border TopRight
                    e.Graphics.DrawLine(penBorder, new Point((rect.X + rect.Width) - (box.Padding.Left + 1), rect.Y), new Point(rect.X + rect.Width, rect.Y));
                }
                else
                {
                    // Draw Text
                    e.Graphics.DrawString(box.Text, box.Font, sbForeColor, (box.Padding.Left + 1), 0);
                    // Draw Border TopLeft
                    e.Graphics.DrawLine(penBorder, new Point(rect.X, rect.Y), new Point(rect.X + (box.Padding.Left + 1), rect.Y));
                    // Draw Border TopRight
                    e.Graphics.DrawLine(penBorder, new Point(rect.X + (box.Padding.Left + 1) + (int)strSize.Width, rect.Y), new Point(rect.X + rect.Width, rect.Y));
                }

                // Draw Border Left
                e.Graphics.DrawLine(penBorder, rect.Location, new Point(rect.X, rect.Y + rect.Height));
                // Draw Border Bottom
                e.Graphics.DrawLine(penBorder, new Point(rect.X, rect.Y + rect.Height), new Point(rect.X + rect.Width, rect.Y + rect.Height));
                // Draw Border Right
                e.Graphics.DrawLine(penBorder, new Point(rect.X + rect.Width, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height));

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
