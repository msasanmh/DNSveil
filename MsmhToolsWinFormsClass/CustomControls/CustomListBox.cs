using MsmhToolsClass;
using MsmhToolsWinFormsClass;
using System.ComponentModel;
using System.Windows.Forms.Design;
/*
* Copyright MSasanMH, June 01, 2022.
*/

namespace CustomControls
{
    public class CustomListBox : ListBox
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new BorderStyle BorderStyle { get; set; }

        private bool mBorder = false;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Border")]
        public bool Border
        {
            get { return mBorder; }
            set
            {
                mBorder = value;
                Invalidate();
            }
        }

        private bool mItemBorder = true;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Item Border")]
        public bool ItemBorder
        {
            get { return mItemBorder; }
            set
            {
                mItemBorder = value;
                Invalidate();
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

        private ContentAlignment mTextAlign = ContentAlignment.MiddleLeft;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Editor(typeof(WindowsFormsComponentEditor), typeof(ContentAlignment))]
        [Category("Appearance"), Description("Item Text Align")]
        public ContentAlignment TextAlign
        {
            get { return mTextAlign; }
            set
            {
                if (mTextAlign != value)
                {
                    mTextAlign = value;
                    Invalidate();
                }
            }
        }

        public event EventHandler? ItemHeightChanged;

        private int LastItemHeight = 0;
        private int HoverItemInvalidated = -1;
        private bool ApplicationIdle = false;
        private bool once = true;

        public CustomListBox() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Opaque, true);

            DrawMode = DrawMode.OwnerDrawVariable; // To Set Item Height
            base.DrawMode = DrawMode.OwnerDrawVariable;
            BorderStyle = BorderStyle.None;
            base.BorderStyle = BorderStyle.None;
            IntegralHeight = false;
            ItemHeight = TextRenderer.MeasureText("MSasanMH", Font).Height + 4;
            LastItemHeight = ItemHeight;

            MeasureItem -= CustomListBox_MeasureItem;
            MeasureItem += CustomListBox_MeasureItem;
            Application.Idle -= Application_Idle;
            Application.Idle += Application_Idle;
            HandleCreated -= CustomListBox_HandleCreated;
            HandleCreated += CustomListBox_HandleCreated;
            Invalidated -= CustomListBox_Invalidated;
            Invalidated += CustomListBox_Invalidated;
            SelectedIndexChanged -= CustomListBox_SelectedIndexChanged;
            SelectedIndexChanged += CustomListBox_SelectedIndexChanged;
            MouseMove -= CustomListBox_MouseMove;
            MouseMove += CustomListBox_MouseMove;
            MouseLeave -= CustomListBox_MouseLeave;
            MouseLeave += CustomListBox_MouseLeave;
        }

        private void CustomListBox_MeasureItem(object? sender, MeasureItemEventArgs e)
        {
            if (sender == null) return;
            e.ItemHeight = ItemHeight;
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

        private void CustomListBox_HandleCreated(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomListBox_Invalidated(object? sender, InvalidateEventArgs e)
        {
            if (BackColor.DarkOrLight() == "Dark")
                this.SetDarkControl();

            if (LastItemHeight != ItemHeight)
            {
                ItemHeightChanged?.Invoke(this, EventArgs.Empty);
                LastItemHeight = ItemHeight;
            }
        }

        private void CustomListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomListBox_MouseMove(object? sender, MouseEventArgs e)
        {
            InvalidateHoverItem();
        }

        private void CustomListBox_MouseLeave(object? sender, EventArgs e)
        {
            if (HoverItemInvalidated >= 0 && HoverItemInvalidated < Items.Count)
            {
                Invalidate(GetItemRectangle(HoverItemInvalidated));
                HoverItemInvalidated = -1;
            }
        }

        private void InvalidateHoverItem()
        {
            for (int n = 0; n < Items.Count; n++)
            {
                Rectangle itemRect = GetItemRectangle(n);
                if (!DesignMode && Enabled && itemRect.Contains(PointToClient(MousePosition)))
                {
                    if (n != HoverItemInvalidated)
                    {
                        Invalidate(itemRect);
                        HoverItemInvalidated = n;
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!ApplicationIdle) return;
            if (!Visible) return;

            Color backColor = GetBackColor();
            Color foreColor = GetForeColor();

            Rectangle rect = ClientRectangle;

            // Paint Background
            using SolidBrush bgBrush = new(backColor);
            e.Graphics.FillRectangle(bgBrush, rect);

            // Paint Items
            for (int n = 0; n < Items.Count; n++)
            {
                Rectangle itemRect = GetItemRectangle(n);
                if (e.ClipRectangle.IntersectsWith(itemRect))
                {
                    if (SelectedIndices.Contains(n))
                    {
                        DrawItemEventArgs diea = new(e.Graphics, Font, itemRect, n, DrawItemState.Selected, foreColor, backColor);
                        OnDrawItem(diea);
                    }
                    else
                    {
                        DrawItemEventArgs diea = new(e.Graphics, Font, itemRect, n, DrawItemState.None, foreColor, backColor);
                        OnDrawItem(diea);
                    }
                }
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            Color borderColor = GetBorderColor();
            Rectangle rect = e.Bounds;
            int r = RoundedCorners;
            
            if (e.Index >= 0 && e.Index < Items.Count)
            {
                //e.DrawBackground();
                string? text = Items[e.Index].ToString();

                // Correct Item Height
                if (!string.IsNullOrEmpty(text))
                {
                    int itemHeight = TextRenderer.MeasureText(text, e.Font).Height + 4;
                    if (itemHeight > ItemHeight) ItemHeight = itemHeight;
                }

                // Set Font
                Font? font = e.Font;

                if (r > 0) e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                if (e.Index == SelectedIndex)
                {
                    if (font != null) font = new(font.FontFamily, font.Size, FontStyle.Bold);

                    // Paint Selected Item Background
                    using SolidBrush sbgBrush = new(SelectionColor);
                    e.Graphics.FillRoundedRectangle(sbgBrush, rect, r, r, r, r);
                }
                else
                {
                    // Paint Non-Selected Item Background
                    Color itemBackColor = e.BackColor;

                    if (!DesignMode && Enabled && rect.Contains(PointToClient(MousePosition)))
                    {
                        Color colorHover;
                        if (e.BackColor.DarkOrLight() == "Dark")
                            colorHover = e.BackColor.ChangeBrightness(0.2f);
                        else
                            colorHover = e.BackColor.ChangeBrightness(-0.2f);
                        itemBackColor = colorHover;
                    }
                    using SolidBrush bgBrush = new(itemBackColor);
                    e.Graphics.FillRoundedRectangle(bgBrush, rect, r, r, r, r);
                }

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                
                // Paint Text
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

                TextRenderer.DrawText(e.Graphics, text, font, rect, e.ForeColor, flags);

                if (r > 0) e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Paint Item Border
                if (ItemBorder)
                {
                    using Pen itemBorderPen = new(borderColor);
                    Rectangle itemBorderRect = new(rect.X, rect.Y, rect.Width - 1, rect.Height);
                    e.Graphics.DrawRoundedRectangle(itemBorderPen, itemBorderRect, r, r, r, r);
                }
                
                //e.DrawFocusRectangle();
            }

            // Paint Border
            if (Border)
            {
                Rectangle cr = ClientRectangle;
                Rectangle borderRect = new(cr.X, cr.Y, cr.Width - 1, cr.Height - 1);
                using Pen borderPen = new(borderColor);
                e.Graphics.DrawRoundedRectangle(borderPen, borderRect, r, r, r, r);
            }

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
        }

        private void CustomListBox_DrawItem(object? sender, DrawItemEventArgs e)
        {
            Color backColor = e.BackColor.Equals(Color.Transparent) ? GetBackColor() : e.BackColor;
            Color foreColor = e.ForeColor.Equals(Color.Transparent) ? GetForeColor() : e.ForeColor;

            Rectangle rect = e.Bounds;

            string text = string.Empty;
            if (e.Index >= 0 && e.Index < Items.Count) text = Items[e.Index].ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(text)) return;

            // Paint Background
            e.Graphics.Clear(backColor);

            // Paint Selected Item Background
            using SolidBrush sbgBrush = new(SelectionColor);
            e.Graphics.FillRectangle(sbgBrush, rect);

            // Paint Text
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

            TextRenderer.DrawText(e.Graphics, text, e.Font, rect, foreColor, flags);
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
