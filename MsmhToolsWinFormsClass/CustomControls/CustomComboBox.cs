using MsmhToolsClass;
using MsmhToolsWinFormsClass;
using System.ComponentModel;
using System.Windows.Forms.Design;
/*
 * Copyright MSasanMH, April 16, 2022.
 */

namespace CustomControls
{
    public class CustomComboBox : ComboBox
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new FlatStyle FlatStyle { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ComboBoxStyle DropDownStyle { get; set; }

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

        private Color mSelectionColor = Color.DodgerBlue;
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

        private string mText = string.Empty;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Text")]
        public override string Text
        {
            get { return mText; }
            set
            {
                if (value == null) return;
                if (mText != value)
                {
                    // "Text" will set to "SelectedItem"'s text by default and I don't want that.
                    if (SelectedItem == null || (SelectedItem != null && !value.Equals(GetItemText(SelectedItem))))
                    {
                        mText = value;
                        Invalidate();
                    }
                }
            }
        }

        private bool ApplicationIdle = false;
        private bool once = true;

        public CustomComboBox() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Opaque, true);

            DrawMode = DrawMode.OwnerDrawVariable;

            // Default
            BackColor = Color.DimGray;
            ForeColor = Color.White;

            base.FlatStyle = FlatStyle.Flat;
            base.DropDownStyle = ComboBoxStyle.DropDownList;

            Application.Idle += Application_Idle;
            HandleCreated += CustomComboBox_HandleCreated;
            MeasureItem += CustomComboBox_MeasureItem;
            BackColorChanged += CustomComboBox_BackColorChanged;
            ForeColorChanged += CustomComboBox_ForeColorChanged;
            MouseMove += CustomComboBox_MouseMove;
            LocationChanged += CustomComboBox_LocationChanged;
            Move += CustomComboBox_Move;
            EnabledChanged += CustomComboBox_EnabledChanged;
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

        private void CustomComboBox_HandleCreated(object? sender, EventArgs e)
        {
            if (BackColor.DarkOrLight() == "Dark")
                this.SetDarkControl();
            Invalidate();
        }

        private void CustomComboBox_MeasureItem(object? sender, MeasureItemEventArgs e)
        {
            // Fix ItemHeight On High DPI Screen Scale
            string? text = SelectedItem != null ? SelectedItem.ToString() : Text;
            text ??= "MSasanMH";
            Size textSize = TextRenderer.MeasureText(text, Font);
            e.ItemHeight = textSize.Height != 0 ? textSize.Height + 2 : 17;
        }

        private void CustomComboBox_BackColorChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomComboBox_ForeColorChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomComboBox_MouseMove(object? sender, MouseEventArgs e)
        {
            Invalidate();
        }

        private void CustomComboBox_LocationChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomComboBox_Move(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomComboBox_EnabledChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override void OnTabStopChanged(EventArgs e)
        {
            base.OnTabStopChanged(e); Invalidate();
        }

        protected override void OnTabIndexChanged(EventArgs e)
        {
            base.OnTabIndexChanged(e); Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e); Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e); Invalidate();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e); Invalidate();
        }

        protected override void OnTextUpdate(EventArgs e)
        {
            base.OnTextUpdate(e); Invalidate();
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e); Invalidate();
        }

        protected override void OnSelectedValueChanged(EventArgs e)
        {
            base.OnSelectedValueChanged(e); Invalidate();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e); Invalidate();
        }

        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            base.OnInvalidated(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e); Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!ApplicationIdle) return;
            if (!Visible) return;

            Color backColor = GetBackColor();
            Color foreColor = GetForeColor();
            Color borderColor = GetBorderColor();
            Color backColorDarker = backColor.ChangeBrightness(-0.3f);

            int r = RoundedCorners;
            
            // Selected Border Color
            if (Focused && TabStop)
                borderColor = GetBorderColor();

            // Rectangle
            Rectangle rect = ClientRectangle;

            // Mouse Position
            Point mP = PointToClient(MousePosition);

            // Paint Background
            using SolidBrush sb = new(backColor);
            e.Graphics.FillRectangle(sb, rect);

            // Button X, Y, Width, Height
            int buttonX;
            if (RightToLeft == RightToLeft.No)
                buttonX = rect.Right - 15;
            else
                buttonX = rect.Left + 1;
            int buttonY = rect.Top + 1;
            int buttonWidth = 14;
            int buttonHeight = rect.Height - 2;

            // Button Rectangle
            Rectangle rectButton = new(buttonX, buttonY, buttonWidth, buttonHeight);

            // Paint Button
            using SolidBrush buttonBrush = new(backColorDarker);
            e.Graphics.FillRectangle(buttonBrush, rectButton);

            // Paint MouseOver Button
            if (Enabled && rectButton.Contains(mP))
            {
                Color hoverColor = backColor.DarkOrLight() == "Dark" ? backColor.ChangeBrightness(0.3f) : backColor.ChangeBrightness(-0.3f);
                using SolidBrush sbHover = new(hoverColor);
                e.Graphics.FillRectangle(sbHover, rectButton);
            }

            // Arrow Points
            Point center = new(rectButton.X + rectButton.Width / 2, rectButton.Y + rectButton.Height / 2);
            Point TopLeft = new(center.X - 3, center.Y - 1);
            Point TopRight = new(center.X + 4, center.Y - 1);
            Point Bottom = new(center.X, center.Y + 3);
            Point[] arrowPoints = new Point[]
            {
                TopLeft, // Top Left
                TopRight, // Top Right
                Bottom // Bottom
            };

            // Paint Arrow
            using SolidBrush arrowBrush = new(foreColor);
            e.Graphics.FillPolygon(arrowBrush, arrowPoints);

            // Text X, Y, Width, Height
            int textX;
            if (RightToLeft == RightToLeft.No)
                textX = rect.Left + 1;
            else
                textX = rect.Left + buttonWidth + 1;
            int textY = rect.Top;
            int textWidth = rect.Width - buttonWidth - 2;
            int textHeight = rect.Height;

            // Text Rectangle
            Rectangle rectText = new(textX, textY, textWidth, textHeight);

            StringFormat stringFormat = new()
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Near,
                FormatFlags = StringFormatFlags.NoWrap,
                Trimming = StringTrimming.EllipsisCharacter
            };

            // Paint Text
            string? text = Text;
            try { text = SelectedItem != null ? SelectedItem.ToString() : Text; } catch (Exception) { }
            using SolidBrush textBrush = new(foreColor);
            e.Graphics.DrawString(text, Font, textBrush, rectText, stringFormat);

            // Paint Border
            using Pen pen = new(borderColor);
            rect = new Rectangle(rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawRoundedRectangle(pen, rect, r, 0, 0, r);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

            // ComboBox Height
            text ??= "MSasanMH";
            Size textSize = TextRenderer.MeasureText(text, Font);
            ItemHeight = textSize.Height != 0 ? textSize.Height + 2 : 17;

            base.OnPaint(e);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (!Visible) return;

            Graphics g = e.Graphics;
            Rectangle rect = e.Bounds;

            Color textColor = ForeColor;
            Color fillColor = BackColor;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected ||
                (e.State & DrawItemState.Focus) == DrawItemState.Focus)
                fillColor = SelectionColor;

            using SolidBrush sb = new(fillColor);
            g.FillRectangle(sb, rect);

            if (e.Index >= 0 && e.Index < Items.Count)
            {
                var text = Items[e.Index].ToString();

                using SolidBrush b = new(textColor);
                var padding = 2;

                Rectangle modRect = new(rect.Left + padding,
                    rect.Top + padding,
                    rect.Width - (padding * 2),
                    rect.Height - (padding * 2));

                var stringFormat = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                g.DrawString(text, Font, b, modRect, stringFormat);
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
