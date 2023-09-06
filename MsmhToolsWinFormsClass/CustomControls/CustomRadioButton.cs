using MsmhToolsClass;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms.Design;

namespace CustomControls
{
    public class CustomRadioButton : RadioButton
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
        public CustomRadioButton() : base()
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
            HandleCreated += CustomRadioButton_HandleCreated;
            LocationChanged += CustomRadioButton_LocationChanged;
            Move += CustomRadioButton_Move;
            EnabledChanged += CustomRadioButton_EnabledChanged;
            BackColorChanged += CustomRadioButton_BackColorChanged;
            RightToLeftChanged += CustomRadioButton_RightToLeftChanged;
            Paint += CustomRadioButton_Paint;
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

        private void CustomRadioButton_HandleCreated(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomRadioButton_LocationChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomRadioButton_Move(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomRadioButton_EnabledChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomRadioButton_BackColorChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomRadioButton_RightToLeftChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }
        
        private void CustomRadioButton_Paint(object? sender, PaintEventArgs e)
        {
            if (ApplicationIdle == false)
                return;
            
            if (sender is RadioButton rb)
            {
                Color backColor = GetBackColor(rb);
                Color foreColor = GetForeColor();
                Color borderColor = GetBorderColor();
                Color checkColor = GetCheckColor();

                e.Graphics.Clear(backColor);
                rb.Appearance = Appearance.Button;
                rb.FlatStyle = FlatStyle.Flat;

                rb.FlatAppearance.BorderSize = 0;
                rb.AutoSize = false;
                rb.UseVisualStyleBackColor = false;
                SizeF sizeF = rb.CreateGraphics().MeasureString(rb.Text, rb.Font);
                int rectSize = 12;
                rb.Height = (int)sizeF.Height;
                rb.Width = (int)(sizeF.Width + rectSize + 5);
                int x;
                float textX;

                if (rb.RightToLeft == RightToLeft.No)
                {
                    rb.TextAlign = ContentAlignment.MiddleLeft;
                    x = 1;
                    textX = (float)(rectSize * 1.3);
                }
                else
                {
                    rb.TextAlign = ContentAlignment.MiddleRight;
                    x = rb.Width - rectSize - 2;
                    textX = rb.Width - sizeF.Width - (float)(rectSize * 1.2);
                }

                int y = (rb.ClientRectangle.Y + (rb.ClientRectangle.Height - rectSize)) / 2;
                Point pt = new(x, y);
                Rectangle rectCheck = new(pt, new Size(rectSize, rectSize));

                // Draw Selection Border
                Rectangle cRect = new(rb.ClientRectangle.X, rb.ClientRectangle.Y, rb.ClientRectangle.Width - 1, rb.ClientRectangle.Height - 1);
                if (rb.Focused)
                {
                    using Pen pen = new(SelectionColor) { DashStyle = DashStyle.Dot };
                    e.Graphics.DrawRectangle(pen, cRect);
                }

                // Draw Text
                using SolidBrush brush1 = new(foreColor);
                e.Graphics.DrawString(rb.Text, rb.Font, brush1, textX, 0);

                // Fill Check Rect
                using SolidBrush brush2 = new(backColor);
                e.Graphics.FillRectangle(brush2, rectCheck);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Set Points
                float centerX = rectCheck.X + (rectCheck.Width / 2);
                float centerY = rectCheck.Y + (rectCheck.Height / 2);
                float radius = rectCheck.Width / 2;

                // Draw Check
                if (rb.Checked)
                {
                    // Draw Check
                    using SolidBrush brushCheck = new(checkColor);
                    rectCheck.Inflate(-2, -2);
                    float radiusC = rectCheck.Width / 2;
                    e.Graphics.FillEllipse(brushCheck, centerX - radiusC, centerY - radiusC, radiusC + radiusC, radiusC + radiusC);
                    rectCheck.Inflate(+2, +2);
                }

                // Draw Check Rect (Check Border)
                using Pen penBorder = new(borderColor);
                e.Graphics.DrawEllipse(penBorder, centerX - radius, centerY - radius, radius + radius, radius + radius);

                e.Graphics.SmoothingMode = SmoothingMode.Default;
            }
        }

        private Color GetBackColor(RadioButton radioButton)
        {
            if (radioButton.Enabled)
                return BackColor;
            else
            {
                Color backColor = BackColor;
                if (radioButton.Parent != null)
                {
                    if (radioButton.Parent.BackColor != Color.Transparent)
                    {
                        if (radioButton.Parent.Enabled)
                            backColor = radioButton.Parent.BackColor;
                        else
                            backColor = GetDisabledColor(radioButton.Parent.BackColor);
                    }
                    else
                    {
                        if (radioButton.FindForm() != null)
                        {
                            if (radioButton.Parent.Enabled)
                                backColor = radioButton.FindForm().BackColor;
                            else
                                backColor = GetDisabledColor(radioButton.FindForm().BackColor);
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
