using MsmhToolsClass;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms.Design;
/*
* Copyright MSasanMH, April 17, 2022.
*/

namespace CustomControls
{
    public class CustomCheckBox : CheckBox
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Appearance Appearance { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new FlatStyle FlatStyle { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new FlatButtonAppearance? FlatAppearance { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ContentAlignment TextAlign { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ContentAlignment CheckAlign { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool AutoSize { get; set; }

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

        private Color mCheckColor = Color.Blue;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Editor(typeof(WindowsFormsComponentEditor), typeof(Color))]
        [Category("Appearance"), Description("Check Color")]
        public Color CheckColor
        {
            get { return mCheckColor; }
            set
            {
                if (mCheckColor != value)
                {
                    mCheckColor = value;
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

        private string? mText = string.Empty;
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0," +
            "Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Text")]
        public override string? Text
        {
            get { return mText; }
            set
            {
                if (mText != value)
                {
                    mText = value;
                    Invalidate();
                }
            }
        }

        private bool ApplicationIdle = false;
        private bool once = true;
        public CustomCheckBox() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Opaque, true);
            
            // Default
            BackColor = Color.DimGray;
            ForeColor = Color.White;

            Application.Idle += Application_Idle;
            HandleCreated += CustomCheckBox_HandleCreated;
            LocationChanged += CustomCheckBox_LocationChanged;
            Move += CustomCheckBox_Move;
            EnabledChanged += CustomCheckBox_EnabledChanged;
            BackColorChanged += CustomCheckBox_BackColorChanged;
            RightToLeftChanged += CustomCheckBox_RightToLeftChanged;
            Paint += CustomCheckBox_Paint;
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

        private void CustomCheckBox_HandleCreated(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomCheckBox_LocationChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomCheckBox_Move(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomCheckBox_EnabledChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomCheckBox_BackColorChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomCheckBox_RightToLeftChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomCheckBox_Paint(object? sender, PaintEventArgs e)
        {
            if (ApplicationIdle == false)
                return;

            if (sender is CheckBox checkBox)
            {
                Color backColor = GetBackColor(checkBox);
                Color foreColor = GetForeColor();
                Color borderColor = GetBorderColor();
                Color checkColor = GetCheckColor();

                e.Graphics.Clear(backColor);
                checkBox.Appearance = Appearance.Button;
                checkBox.FlatStyle = FlatStyle.Flat;
                
                checkBox.FlatAppearance.BorderSize = 0;
                checkBox.AutoSize = false;
                checkBox.UseVisualStyleBackColor = false;
                SizeF sizeF = checkBox.CreateGraphics().MeasureString(checkBox.Text, checkBox.Font);
                int rectSize = 12;
                checkBox.Height = (int)sizeF.Height;
                checkBox.Width = (int)(sizeF.Width + rectSize + 5);
                int x;
                float textX;

                if (checkBox.RightToLeft == RightToLeft.No)
                {
                    checkBox.TextAlign = ContentAlignment.MiddleLeft;
                    x = 1;
                    textX = (float)(rectSize * 1.3);
                }
                else
                {
                    checkBox.TextAlign = ContentAlignment.MiddleRight;
                    x = checkBox.Width - rectSize - 2;
                    textX = checkBox.Width - sizeF.Width - (float)(rectSize * 1.2);
                }

                int y = (checkBox.ClientRectangle.Y + 1 + (checkBox.ClientRectangle.Height - rectSize)) / 2;
                Point pt = new(x, y);
                Rectangle rectCheck = new(pt, new Size(rectSize, rectSize));

                // Draw Selection Border
                Rectangle cRect = new(checkBox.ClientRectangle.X, checkBox.ClientRectangle.Y, checkBox.ClientRectangle.Width - 1, checkBox.ClientRectangle.Height - 1);
                if (checkBox.Focused)
                {
                    using Pen pen = new(SelectionColor) { DashStyle = DashStyle.Dot };
                    e.Graphics.DrawRectangle(pen, cRect);
                }

                // Draw Text
                using SolidBrush brush1 = new(foreColor);
                e.Graphics.DrawString(checkBox.Text, checkBox.Font, brush1, textX, 0);

                // Fill Check Rect
                using SolidBrush brush2 = new(backColor);
                e.Graphics.FillRectangle(brush2, rectCheck);

                // Draw Check
                if (checkBox.Checked)
                {
                    if (checkBox.CheckState == CheckState.Checked)
                    {
                        // Draw Check
                        using Pen p = new(checkColor, 2);
                        rectCheck.Inflate(-2, -2);
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        e.Graphics.DrawLines(p, new Point[] { new Point(rectCheck.Left, rectCheck.Bottom - rectCheck.Height / 2), new Point(rectCheck.Left + rectCheck.Width / 3, rectCheck.Bottom), new Point(rectCheck.Right, rectCheck.Top) });
                        e.Graphics.SmoothingMode = SmoothingMode.Default;
                        rectCheck.Inflate(+2, +2);
                    }
                    else if (checkBox.CheckState == CheckState.Indeterminate)
                    {
                        // Draw Indeterminate
                        using SolidBrush sb = new(checkColor);
                        rectCheck.Inflate(-2, -2);
                        e.Graphics.FillRectangle(sb, rectCheck);
                        rectCheck.Inflate(+2, +2);
                    }
                }

                // Draw Check Rect (Check Border)
                ControlPaint.DrawBorder(e.Graphics, rectCheck, borderColor, ButtonBorderStyle.Solid);
            }
        }

        private Color GetBackColor(CheckBox checkBox)
        {
            if (checkBox.Enabled)
                return BackColor;
            else
            {
                Color backColor = BackColor;
                if (checkBox.Parent != null)
                {
                    if (checkBox.Parent.BackColor != Color.Transparent)
                    {
                        if (checkBox.Parent.Enabled)
                            backColor = checkBox.Parent.BackColor;
                        else
                            backColor = GetDisabledColor(checkBox.Parent.BackColor);
                    }
                    else
                    {
                        if (checkBox.FindForm() != null)
                        {
                            if (checkBox.Parent.Enabled)
                                backColor = checkBox.FindForm().BackColor;
                            else
                                backColor = GetDisabledColor(checkBox.FindForm().BackColor);
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

        private Color GetCheckColor()
        {
            if (Enabled)
                return CheckColor;
            else
            {
                if (CheckColor.DarkOrLight() == "Dark")
                    return CheckColor.ChangeBrightness(0.3f);
                else
                    return CheckColor.ChangeBrightness(-0.3f);
            }
        }

    }
}
