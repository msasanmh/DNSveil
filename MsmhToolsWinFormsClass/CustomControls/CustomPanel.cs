using MsmhToolsClass;
using MsmhToolsWinFormsClass;
using System.ComponentModel;
using System.Windows.Forms.Design;
/*
* Copyright MSasanMH, May 16, 2022.
*/

namespace CustomControls
{
    public class CustomPanel : Panel
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new BorderStyle BorderStyle { get; set; }

        private BorderStyle mBorder = BorderStyle.FixedSingle;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Border Style")]
        public BorderStyle Border
        {
            get { return mBorder; }
            set
            {
                mBorder = value;
                Invalidate();
            }
        }

        private ButtonBorderStyle mButtonBorderStyle = ButtonBorderStyle.Solid;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Button Border Style")]
        public ButtonBorderStyle ButtonBorderStyle
        {
            get { return mButtonBorderStyle; }
            set
            {
                if (mButtonBorderStyle != value)
                {
                    mButtonBorderStyle = value;
                    Invalidate();
                }
            }
        }

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

        private bool ApplicationIdle = false;
        private bool once = true;

        private readonly Panel innerPanel = new();

        // Events Action
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Action"), Description("Click")]
        public new event EventHandler? Click;

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Action"), Description("Double Click")]
        public new event EventHandler? DoubleClick;

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Action"), Description("Mouse Click")]
        public new event MouseEventHandler? MouseClick;

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Action"), Description("Mouse Double Click")]
        public new event MouseEventHandler? MouseDoubleClick;

        // Events Focus
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Focus"), Description("Enter")]
        public new event EventHandler? Enter;

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Focus"), Description("Leave")]
        public new event EventHandler? Leave;

        // Events Mouse
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Mouse"), Description("Mouse Down")]
        public new event MouseEventHandler? MouseDown;

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Mouse"), Description("Mouse Enter")]
        public new event EventHandler? MouseEnter;

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Mouse"), Description("Mouse Hover")]
        public new event EventHandler? MouseHover;

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Mouse"), Description("Mouse Leave")]
        public new event EventHandler? MouseLeave;

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Mouse"), Description("Mouse Move")]
        public new event MouseEventHandler? MouseMove;

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Mouse"), Description("Mouse Up")]
        public new event MouseEventHandler? MouseUp;

        public CustomPanel() : base()
        {
            SetStyle(ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, false); // Fixes Flickers

            BorderStyle = BorderStyle.None;
            ButtonBorderStyle = ButtonBorderStyle.Solid;

            // Events Action
            innerPanel.Click += (object? sender, EventArgs e) => { Click?.Invoke(sender, e); };
            innerPanel.DoubleClick += (object? sender, EventArgs e) => { DoubleClick?.Invoke(sender, e); };
            innerPanel.MouseClick += (object? sender, MouseEventArgs e) => { MouseClick?.Invoke(sender, e); };
            innerPanel.MouseDoubleClick += (object? sender, MouseEventArgs e) => { MouseDoubleClick?.Invoke(sender, e); };

            // Events Focus
            innerPanel.Enter += (object? sender, EventArgs e) => { Enter?.Invoke(sender, e); };
            innerPanel.Leave += (object? sender, EventArgs e) => { Leave?.Invoke(sender, e); };

            // Events Mouse
            innerPanel.MouseDown += (object? sender, MouseEventArgs e) => { MouseDown?.Invoke(sender, e); };
            innerPanel.MouseEnter += (object? sender, EventArgs e) => { MouseEnter?.Invoke(sender, e); };
            innerPanel.MouseHover += (object? sender, EventArgs e) => { MouseHover?.Invoke(sender, e); };
            innerPanel.MouseLeave += (object? sender, EventArgs e) => { MouseLeave?.Invoke(sender, e); };
            innerPanel.MouseMove += (object? sender, MouseEventArgs e) => { MouseMove?.Invoke(sender, e); };
            innerPanel.MouseUp += (object? sender, MouseEventArgs e) => { MouseUp?.Invoke(sender, e); };

            Controls.Add(innerPanel);

            Application.Idle += Application_Idle;
            HandleCreated += CustomPanel_HandleCreated;
            Paint += CustomPanel_Paint;
            BackgroundImageChanged += CustomPanel_BackgroundImageChanged;
            EnabledChanged += CustomPanel_EnabledChanged;
            Invalidated += CustomPanel_Invalidated;
            ControlAdded += CustomPanel_ControlAdded;
            ControlRemoved += CustomPanel_ControlRemoved;
            Enter += CustomPanel_Enter;
            MouseDown += CustomPanel_MouseDown;
            MouseEnter += CustomPanel_MouseEnter;
            MouseLeave += CustomPanel_MouseLeave;
            MouseWheel += CustomPanel_MouseWheel;
            ParentChanged += CustomPanel_ParentChanged;
            Resize += CustomPanel_Resize;
            Scroll += CustomPanel_Scroll;
            SizeChanged += CustomPanel_SizeChanged;
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

        private void CustomPanel_HandleCreated(object? sender, EventArgs e)
        {
            foreach (Control c in Controls)
            {
                if (c is not Panel)
                    c.BringToFront();
                foreach (Control c2 in c.Controls)
                {
                    if (c2 is not Panel)
                        c2.BringToFront();
                }
            }

            // Timer is needed in some rare cases.
            var p = sender as Panel;
            int totalTime = 500;
            int elapsedTime = 0;
            var t = new System.Windows.Forms.Timer();
            t.Interval = 100;
            t.Tick += (s, e) =>
            {
                p?.Invalidate();
                elapsedTime += t.Interval;
                if (elapsedTime > totalTime) t.Stop();
            };
            t.Start();
        }

        private void CustomPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (!ApplicationIdle) return;
            if (!Visible) return;

            Rectangle rect = new(0, 0, ClientRectangle.Width, ClientRectangle.Height);

            if (sender is Panel)
            {
                if (DesignMode)
                    BorderStyle = BorderStyle.FixedSingle;
                else
                    BorderStyle = BorderStyle.None;

                Color backColor = GetBackColor();
                Color foreColor = GetForeColor();
                Color borderColor = GetBorderColor();

                ForeColor = foreColor;
                innerPanel.BackColor = backColor;
                innerPanel.ForeColor = foreColor;

                // Fill Background
                e.Graphics.Clear(backColor);

                // Draw Border
                if (Border == BorderStyle.FixedSingle)
                    ControlPaint.DrawBorder(e.Graphics, rect, borderColor, ButtonBorderStyle);
                else if (Border == BorderStyle.Fixed3D)
                {
                    Color secondBorderColor;
                    if (borderColor.DarkOrLight() == "Dark")
                        secondBorderColor = borderColor.ChangeBrightness(0.5f);
                    else
                        secondBorderColor = borderColor.ChangeBrightness(-0.5f);

                    Rectangle rect3DBorder;

                    rect3DBorder = new(rect.X, rect.Y, rect.Width, rect.Height);
                    ControlPaint.DrawBorder(e.Graphics, rect3DBorder, secondBorderColor, ButtonBorderStyle);

                    rect3DBorder = new(rect.X + 1, rect.Y + 1, rect.Width - 1, rect.Height - 1);
                    ControlPaint.DrawBorder(e.Graphics, rect3DBorder, secondBorderColor, ButtonBorderStyle);

                    rect3DBorder = new(rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                    ControlPaint.DrawBorder(e.Graphics, rect3DBorder, borderColor, ButtonBorderStyle);
                }
            }
        }

        private void CustomPanel_BackgroundImageChanged(object? sender, EventArgs e)
        {
            innerPanel.BackgroundImage = BackgroundImage;
        }

        private void CustomPanel_EnabledChanged(object? sender, EventArgs e)
        {
            if (sender is not Panel p) return;

            if (p.Enabled)
                innerPanel.Enabled = true;
            else
                innerPanel.Enabled = false;
            p.Invalidate();
        }

        private void CustomPanel_Invalidated(object? sender, InvalidateEventArgs e)
        {
            if (sender is not Panel p) return;

            if (!DesignMode && AutoScroll)
            {
                innerPanel.AutoScroll = true;
                AutoScroll = false;
            }

            innerPanel.AutoScrollMargin = p.AutoScrollMargin;
            innerPanel.AutoScrollMinSize = p.AutoScrollMinSize;
            innerPanel.AutoScrollOffset = p.AutoScrollOffset;

            if (Border == BorderStyle.FixedSingle)
            {
                innerPanel.Location = new(1, 1);
                innerPanel.Width = ClientRectangle.Width - 2;
                innerPanel.Height = ClientRectangle.Height - 2;
            }
            else if (Border == BorderStyle.Fixed3D)
            {
                innerPanel.Location = new(2, 2);
                innerPanel.Width = ClientRectangle.Width - 4;
                innerPanel.Height = ClientRectangle.Height - 4;
            }
            else
            {
                innerPanel.Location = new(0, 0);
                innerPanel.Width = ClientRectangle.Width;
                innerPanel.Height = ClientRectangle.Height;
            }

            if (BackColor.DarkOrLight() == "Dark")
                innerPanel.SetDarkControl();

            foreach (Control c in p.Controls)
                c.Invalidate();
        }

        private void CustomPanel_ControlAdded(object? sender, ControlEventArgs e)
        {
            if (sender is not Panel p) return;

            p.Invalidate();
            if (!DesignMode)
            {
                p.Controls.Remove(e.Control);
                innerPanel.Controls.Add(e.Control);
                e.Control.BringToFront(); // Makes Arrow Keys Work Correctly.
            }
        }

        private void CustomPanel_ControlRemoved(object? sender, ControlEventArgs e)
        {
            if (sender is not Panel p) return;

            p.Invalidate();
            if (!DesignMode)
                innerPanel.Controls.Remove(e.Control);
        }

        private void CustomPanel_Enter(object? sender, EventArgs e)
        {
            if (sender is not Panel p) return;
            p.Invalidate();
        }

        private void CustomPanel_MouseDown(object? sender, MouseEventArgs e)
        {
            if (sender is not Panel p) return;
            p.Invalidate();
        }

        private void CustomPanel_MouseEnter(object? sender, EventArgs e)
        {
            if (sender is not Panel p) return;
            p.Invalidate();
        }

        private void CustomPanel_MouseLeave(object? sender, EventArgs e)
        {
            if (sender is not Panel p) return;
            p.Invalidate();
        }

        private void CustomPanel_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (sender is not Panel p) return;
            p.Invalidate();
        }

        private void CustomPanel_ParentChanged(object? sender, EventArgs e)
        {
            if (sender is not Panel p) return;
            p.Invalidate();
        }

        private void CustomPanel_Resize(object? sender, EventArgs e)
        {
            if (sender is not Panel p) return;
            p.Invalidate();
        }

        private void CustomPanel_Scroll(object? sender, ScrollEventArgs e)
        {
            if (sender is not Panel p) return;
            p.Invalidate();
        }

        private void CustomPanel_SizeChanged(object? sender, EventArgs e)
        {
            if (sender is not Panel p) return;
            p.Invalidate();
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
