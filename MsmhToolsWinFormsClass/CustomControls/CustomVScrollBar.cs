using MsmhToolsClass;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Diagnostics;
/*
* Copyright MSasanMH, May 10, 2022.
*/

namespace CustomControls
{
    [Designer(typeof(ScrollBarControlDesigner))]
    public class CustomVScrollBar : UserControl
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Mark as deprecated.", true)]
        public new BorderStyle BorderStyle { get; set; }

        protected int mNewValue = 0;
        private int nClickPoint;

        protected int mThumbTop = 0;

        protected bool mAutoSize = false;

        private bool mThumbDown = false;
        private bool mThumbDragging = false;
        private bool mDown = false;

        public new event EventHandler<EventArgs>? Scroll;
        public event EventHandler? ValueChanged = null;

        private static readonly int ScrollBarWidth = 16;
        private static readonly int ScrollBarWidthMin = 14;
        private static readonly int ScrollBarWidthMax = 30;
        private static readonly int ThumbMinHeight = 20;
        private static Size UpArrow = new(ScrollBarWidth, ScrollBarWidth);
        private static Size Thumb = new(ScrollBarWidth, ThumbMinHeight);
        private static Size DownArrow = new(ScrollBarWidth, ScrollBarWidth);

        private static Rectangle rect;
        private static Rectangle rectUpArrow;
        private static Rectangle rectThumb;
        private static Rectangle rectDownArrow;

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

        protected int mLargeChange = 10;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Large Change")]
        public int LargeChange
        {
            get { return mLargeChange; }
            set
            {
                mLargeChange = value;
                Invalidate();
            }
        }

        protected int mSmallChange = 1;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Small Change")]
        public int SmallChange
        {
            get { return mSmallChange; }
            set
            {
                mSmallChange = value;
                Invalidate();
            }
        }

        protected int mMinimum = 0;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Minimum")]
        public int Minimum
        {
            get { return mMinimum; }
            set
            {
                mMinimum = value;
                Invalidate();
            }
        }

        protected int mMaximum = 100;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Maximum")]
        public int Maximum
        {
            get { return mMaximum; }
            set
            {
                mMaximum = value;
                Invalidate();
            }
        }

        protected int mValue = 0;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Value")]
        public int Value
        {
            get
            {
                if (mValue < Minimum)
                    mValue = Minimum;
                else if (mValue > Maximum)
                    mValue = Maximum;
                return mValue;
            }
            set
            {
                mValue = value;

                int nTrackHeight = Height - (UpArrow.Height + DownArrow.Height);
                int nThumbHeight = (int)GetThumbHeight();

                // Compute Value
                int nPixelRange = nTrackHeight - nThumbHeight;
                int nRealRange = Maximum - Minimum - LargeChange;
                float fPerc = 0.0f;
                if (nRealRange != 0)
                {
                    fPerc = (float)mValue / nRealRange;
                }

                float fTop = fPerc * nPixelRange;
                mThumbTop = (int)fTop;
                Invalidate();
            }
        }

        public override bool AutoSize
        {
            get { return base.AutoSize; }
            set
            {
                base.AutoSize = value;
                if (base.AutoSize)
                {
                    Width = UpArrow.Width;
                }
            }
        }

        private static Color[]? OriginalColors;
        private bool once = true;

        public CustomVScrollBar() : base()
        {
            SuspendLayout();
            Name = "CustomVScrollBar";
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            Width = ScrollBarWidth;
            base.MinimumSize = new Size(ScrollBarWidthMin, UpArrow.Height + Thumb.Height + DownArrow.Height);

            // Events
            MouseWheel += CustomVScrollBar_MouseWheel;
            MouseDown += CustomVScrollBar_MouseDown;
            MouseMove += CustomVScrollBar_MouseMove;
            MouseUp += CustomVScrollBar_MouseUp;
            LocationChanged += CustomVScrollBar_LocationChanged;
            Move += CustomVScrollBar_Move;
            EnabledChanged += CustomVScrollBar_EnabledChanged;
            Application.Idle += Application_Idle;
            ResumeLayout(false);
        }

        private void Application_Idle(object? sender, EventArgs e)
        {
            if (Parent != null && FindForm() != null)
            {
                if (once)
                {
                    Parent.MouseWheel -= Parent_MouseWheel;
                    Parent.MouseWheel += Parent_MouseWheel;
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

        private void Parent_MouseWheel(object? sender, MouseEventArgs e)
        {
            CustomVScrollBar_MouseWheel(sender, e);
        }

        private void CustomVScrollBar_LocationChanged(object? sender, EventArgs e)
        {
            if (sender is CustomVScrollBar csb)
                csb.Invalidate();
        }

        private void CustomVScrollBar_Move(object? sender, EventArgs e)
        {
            if (sender is CustomVScrollBar csb)
                csb.Invalidate();
        }

        private void CustomVScrollBar_EnabledChanged(object? sender, EventArgs e)
        {
            if (sender is CustomVScrollBar csb)
                csb.Invalidate();
        }

        private void CustomVScrollBar_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (!Enabled) return;

            int nTrackHeight = Height - (UpArrow.Height + DownArrow.Height);
            int nThumbHeight = (int)GetThumbHeight();

            int nRealRange = Maximum - Minimum - LargeChange;
            int nPixelRange = nTrackHeight - nThumbHeight;
            if (e.Delta > 0)
            {
                Value -= SmallChange;
                Value = Math.Max(Value, 0);
                MoveThumb(e.Y);
                Debug.WriteLine(Value);
            }
            else
            {
                if (nRealRange > 0)
                {
                    if (nPixelRange > 0)
                    {
                        Value += SmallChange;
                        if ((mThumbTop + SmallChange) > nPixelRange)
                        {
                            mThumbTop = nPixelRange;
                            // Compute Value
                            float fPerc = mThumbTop / nPixelRange;
                            float fValue = fPerc * (Maximum - LargeChange);
                            Value = (int)fValue;
                        }
                        MoveThumb(e.Y);
                        Debug.WriteLine(Value);
                    }
                }
            }

            ValueChanged?.Invoke(this, new EventArgs());
            Scroll?.Invoke(this, new EventArgs());
        }

        private void CustomVScrollBar_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
                return;
            Debug.WriteLine("Mouse Down");
            mDown = true;
            Point ptPoint = PointToClient(Cursor.Position);
            
            int nTrackHeight = Height - (UpArrow.Height + DownArrow.Height);
            int nThumbHeight = (int)GetThumbHeight();

            int nTop = mThumbTop;
            nTop += UpArrow.Height;

            int nRealRange = Maximum - Minimum - LargeChange;
            int nPixelRange = nTrackHeight - nThumbHeight;

            // rectUpArrow Action
            if (rectUpArrow.Contains(ptPoint))
            {
                if (nRealRange > 0)
                {
                    if (nPixelRange > 0)
                    {
                        while (mDown == true)
                        {
                            Value -= SmallChange;
                            Value = Math.Max(Value, 0);
                            MoveThumb(e.Y);
                            Debug.WriteLine(Value);

                            ValueChanged?.Invoke(this, new EventArgs());
                            Scroll?.Invoke(this, new EventArgs());

                            Invalidate();
                            Task.Delay(10).Wait();
                            Application.DoEvents();
                            if (mDown == false)
                                break;
                        }
                    }
                }
            }

            // rectThumbBefore Action
            Rectangle rectThumbBefore = new(new Point(1, UpArrow.Height), new Size(Thumb.Width, nTop - (UpArrow.Height)));
            if (rectThumbBefore.Contains(ptPoint))
            {
                if (nRealRange > 0)
                {
                    if (nPixelRange > 0)
                    {
                        while (mDown == true)
                        {
                            Value -= LargeChange;
                            if (mThumbTop + UpArrow.Height < e.Y)
                            {
                                if (mThumbTop <= 0)
                                {
                                    mThumbTop = 0;
                                }
                                return;
                            }
                            Value = Math.Max(Value, 0);
                            MoveThumb(e.Y);
                            Debug.WriteLine(Value);
                            Task.Delay(10).Wait();
                            Application.DoEvents();
                            if (mDown == false)
                                break;
                        }
                    }
                }
            }

            // rectThumb Action
            if (rectThumb.Contains(ptPoint))
            {
                // Hit the thumb
                nClickPoint = ptPoint.Y - nTop;
                mThumbDown = true;
            }

            // rectThumbAfter Action
            Rectangle rectThumbAfter = new(1, nTop + Thumb.Height, Thumb.Width, Height - (nTop + Thumb.Height) - (DownArrow.Height));
            if (rectThumbAfter.Contains(ptPoint))
            {
                if (nRealRange > 0)
                {
                    if (nPixelRange > 0)
                    {
                        while (mDown == true)
                        {
                            Value += LargeChange;
                            if (mThumbTop + Thumb.Height > e.Y)
                            {
                                if (mThumbTop >= nPixelRange)
                                {
                                    mThumbTop = nPixelRange;
                                }
                                return;
                            }
                            if ((mThumbTop + SmallChange) > nPixelRange)
                            {
                                mThumbTop = nPixelRange;
                                // Compute Value
                                float fPerc = mThumbTop / nPixelRange;
                                float fValue = fPerc * (Maximum - LargeChange);
                                Value = (int)fValue;
                            }
                            MoveThumb(e.Y);
                            Debug.WriteLine(Value);
                            Task.Delay(10).Wait();
                            Application.DoEvents();
                            if (mDown == false)
                                break;
                        }
                    }
                }
            }

            // rectDownArrow Action
            if (rectDownArrow.Contains(ptPoint))
            {
                if (nRealRange > 0)
                {
                    if (nPixelRange > 0)
                    {
                        while (mDown == true)
                        {
                            Value += SmallChange;
                            if ((mThumbTop + SmallChange) > nPixelRange)
                            {
                                mThumbTop = nPixelRange;
                                // Compute Value
                                float fPerc = mThumbTop / nPixelRange;
                                float fValue = fPerc * (Maximum - LargeChange);
                                Value = (int)fValue;
                            }
                            MoveThumb(e.Y);
                            Debug.WriteLine(Value);

                            ValueChanged?.Invoke(this, new EventArgs());
                            Scroll?.Invoke(this, new EventArgs());

                            Invalidate();
                            Task.Delay(10).Wait();
                            Application.DoEvents();
                            if (mDown == false)
                                break;
                        }
                    }
                }
            }
        }

        private void CustomVScrollBar_MouseUp(object? sender, MouseEventArgs e)
        {
            Debug.WriteLine("Mouse Up");
            mThumbDown = false;
            mThumbDragging = false;
            mDown = false;
        }

        private void CustomVScrollBar_MouseMove(object? sender, MouseEventArgs e)
        {
            if (mThumbDown == true)
            {
                mThumbDragging = true;
            }

            if (mThumbDragging)
            {
                MoveThumb(e.Y);
            }

            ValueChanged?.Invoke(this, new EventArgs());
            Scroll?.Invoke(this, new EventArgs());
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!Visible) return;

            // Update Colors
            OriginalColors = new Color[] { BackColor, ForeColor, BorderColor };

            // Set Maximum Size
            Width = Math.Min(Width, ScrollBarWidthMax);

            // Update Size
            UpArrow = new(Width, Width);
            Thumb = new(Width, (int)GetThumbHeight());
            DownArrow = new(Width, Width);
            base.MinimumSize = new Size(ScrollBarWidthMin, UpArrow.Height + ThumbMinHeight + DownArrow.Height);

            //Thumb.Height = (int)GetThumbHeight();

            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            if (OriginalColors == null)
                return;

            Color backColor;
            Color foreColor;
            Color borderColor;

            if (Enabled)
            {
                backColor = OriginalColors[0];
                foreColor = OriginalColors[1];
                borderColor = OriginalColors[2];
            }
            else
            {
                // Disabled Colors
                if (OriginalColors[0].DarkOrLight() == "Dark")
                    backColor = OriginalColors[0].ChangeBrightness(0.3f);
                else
                    backColor = OriginalColors[0].ChangeBrightness(-0.3f);

                if (OriginalColors[1].DarkOrLight() == "Dark")
                    foreColor = OriginalColors[1].ChangeBrightness(0.2f);
                else
                    foreColor = OriginalColors[1].ChangeBrightness(-0.2f);

                if (OriginalColors[2].DarkOrLight() == "Dark")
                    borderColor = OriginalColors[2].ChangeBrightness(0.3f);
                else
                    borderColor = OriginalColors[2].ChangeBrightness(-0.3f);
            }

            // Fill Background
            using Brush sb0 = new SolidBrush(backColor);
            rect = new(0, 0, Width, Height);
            e.Graphics.FillRectangle(sb0, rect);

            // Draw Up Arrow BG
            using SolidBrush sbUpArrow = new(foreColor);
            rectUpArrow = new(rect.X + 2, rect.Y, rect.Width - 4, UpArrow.Height);
            e.Graphics.FillRectangle(sbUpArrow, rectUpArrow);

            // Draw Up Spaces
            using Pen penUpArrow = new(backColor);
            e.Graphics.DrawLine(penUpArrow, rectUpArrow.X, rectUpArrow.Y, rectUpArrow.X + rectUpArrow.Width - 1, rectUpArrow.Y);
            e.Graphics.DrawLine(penUpArrow, rectUpArrow.X, rectUpArrow.Y + 1, rectUpArrow.X + rectUpArrow.Width - 1, rectUpArrow.Y + 1);
            e.Graphics.DrawLine(penUpArrow, rectUpArrow.X, rectUpArrow.Y + rectUpArrow.Height - 1, rectUpArrow.X + rectUpArrow.Width - 1, rectUpArrow.Y + rectUpArrow.Height - 1);

            // Draw Up Arrow Button Icon
            var pthUpArrow = new System.Drawing.Drawing2D.GraphicsPath();
            var TopUpArrow = new PointF(rectUpArrow.X + rectUpArrow.Width / 2, rectUpArrow.Y + rectUpArrow.Height * 2 / 5);
            var ButtomLeftUpArrow = new PointF(rectUpArrow.X + rectUpArrow.Width * 1 / 5, rectUpArrow.Y + rectUpArrow.Height * 3 / 5);
            var BottomRightUpArrow = new PointF(rectUpArrow.X + rectUpArrow.Width * 4 / 5, rectUpArrow.Y + rectUpArrow.Height * 3 / 5);
            pthUpArrow.AddLine(TopUpArrow, ButtomLeftUpArrow);
            pthUpArrow.AddLine(ButtomLeftUpArrow, BottomRightUpArrow);

            // Draw Up Arrow
            using SolidBrush arrowBrushUpArrow = new(backColor);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.FillPath(arrowBrushUpArrow, pthUpArrow);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

            //Thumb Location
            int nTop = mThumbTop;
            nTop += UpArrow.Height;

            //Draw Thumb
            using SolidBrush sb3 = new(foreColor);
            rectThumb = new(rectUpArrow.X, nTop, rectUpArrow.Width, Thumb.Height);
            e.Graphics.FillRectangle(sb3, rectThumb);

            // Draw Down Arrow BG
            using SolidBrush sbDownArrow = new(foreColor);
            rectDownArrow = new(rectUpArrow.X, Height - DownArrow.Height, rectUpArrow.Width, DownArrow.Height);
            e.Graphics.FillRectangle(sbDownArrow, rectDownArrow);

            // Draw Down Spaces
            using Pen penDownArrow = new(backColor);
            e.Graphics.DrawLine(penDownArrow, rectDownArrow.X, rectDownArrow.Y, rectDownArrow.X + rectDownArrow.Width - 1, rectDownArrow.Y);
            e.Graphics.DrawLine(penDownArrow, rectDownArrow.X, rectDownArrow.Y + rectDownArrow.Height - 1, rectDownArrow.X + rectDownArrow.Width - 1, rectDownArrow.Y + rectDownArrow.Height - 1);
            e.Graphics.DrawLine(penDownArrow, rectDownArrow.X, rectDownArrow.Y + rectDownArrow.Height - 2, rectDownArrow.X + rectDownArrow.Width - 1, rectDownArrow.Y + rectDownArrow.Height - 2);

            // Draw Down Arrow Button Icon
            var pthDownArrow = new System.Drawing.Drawing2D.GraphicsPath();
            var TopLeftDownArrow = new PointF(rectDownArrow.X + rectDownArrow.Width * 1 / 5, rectDownArrow.Y + rectDownArrow.Height * 2 / 5);
            var TopRightDownArrow = new PointF(rectDownArrow.X + rectDownArrow.Width * 4 / 5, rectDownArrow.Y + rectDownArrow.Height * 2 / 5);
            var BottomDownArrow = new PointF(rectDownArrow.X + rectDownArrow.Width / 2, rectDownArrow.Y + rectDownArrow.Height * 3 / 5);
            pthDownArrow.AddLine(TopLeftDownArrow, TopRightDownArrow);
            pthDownArrow.AddLine(TopRightDownArrow, BottomDownArrow);

            // Draw Down Arrow
            using SolidBrush arrowBrushDownArrow = new(backColor);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.FillPath(arrowBrushDownArrow, pthDownArrow);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

            // Draw Border
            using Pen penBorder = new(borderColor);
            Rectangle rectBorder = new(rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
            e.Graphics.DrawRectangle(penBorder, rectBorder);
        }

        private float GetThumbHeight()
        {
            int nTrackHeight = Height - (UpArrow.Height + DownArrow.Height);
            //float eachRowHeight = nTrackHeight / (float)LargeChange;
            float fThumbHeight = (float)LargeChange / Maximum * nTrackHeight;
            int nThumbHeight = (int)fThumbHeight;
            if (nThumbHeight < ThumbMinHeight)
            {
                nThumbHeight = ThumbMinHeight;
                fThumbHeight = (float)ThumbMinHeight;
            }
            else if (nThumbHeight > nTrackHeight)
            {
                nThumbHeight = nTrackHeight;
                fThumbHeight = (float)nTrackHeight;
            }
            Thumb.Height = nThumbHeight;
            return fThumbHeight;
        }

        private void MoveThumb(int y)
        {
            int nRealRange = Maximum - Minimum;
            int nTrackHeight = Height - (UpArrow.Height + DownArrow.Height);
            int nThumbHeight = (int)GetThumbHeight();

            int nPixelRange = nTrackHeight - nThumbHeight;
            if (mThumbDown && nRealRange > 0)
            {
                //Debug.WriteLine(nPixelRange);
                if (nPixelRange > 0)
                {
                    int nNewThumbTop = y - (UpArrow.Height + nClickPoint);

                    if (nNewThumbTop < 0)
                    {
                        mThumbTop = 0;
                    }
                    else if (nNewThumbTop > nPixelRange)
                    {
                        mThumbTop = nPixelRange;
                    }
                    else
                    {
                        mThumbTop = y - (UpArrow.Height + nClickPoint);
                    }

                    //Compute Value
                    float fPerc = (float)mThumbTop / nPixelRange;
                    float fValue = fPerc * (Maximum - LargeChange);
                    mValue = (int)fValue;
                    Debug.WriteLine(mValue.ToString());

                    Application.DoEvents();

                    Invalidate();
                }
            }
        }

    }

    internal class ScrollBarControlDesigner : ControlDesigner
    {
        public override SelectionRules SelectionRules
        {
            get
            {
                SelectionRules selectionRules = base.SelectionRules;
                PropertyDescriptor? propDescriptor = TypeDescriptor.GetProperties(Component)["AutoSize"];
                if (propDescriptor != null)
                {
                    object? value = propDescriptor.GetValue(Component);
                    if (value != null)
                    {
                        bool autoSize = Convert.ToBoolean(value);
                        if (autoSize)
                        {
                            selectionRules = SelectionRules.Visible | SelectionRules.Moveable | SelectionRules.BottomSizeable | SelectionRules.TopSizeable;
                        }
                        else
                        {
                            selectionRules = SelectionRules.Visible | SelectionRules.AllSizeable | SelectionRules.Moveable;
                        }
                    }
                }
                return selectionRules;
            }
        }
    }
}