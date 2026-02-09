using MsmhToolsClass;
using MsmhToolsWinFormsClass;
using System.ComponentModel;
using System.Windows.Forms.Design;
/*
* Copyright MSasanMH, June 01, 2022.
*/

namespace CustomControls
{
    public class CustomTabControl : TabControl
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new TabAppearance Appearance { get; set; }

        private Color mBackColor = Color.DimGray;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Editor(typeof(WindowsFormsComponentEditor), typeof(Color))]
        [Category("Appearance"), Description("Back Color")]
        public new Color BackColor
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
        public new Color ForeColor
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

        private bool mHideTabHeader = false;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Hide Tab Header")]
        public bool HideTabHeader
        {
            get { return mHideTabHeader; }
            set
            {
                if (mHideTabHeader != value)
                {
                    mHideTabHeader = value;
                    HideTabHeaderChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("HideTabHeader Changed Event")]
        public event EventHandler? HideTabHeaderChanged;

        private TabAlignment LastAlignment = TabAlignment.Top;
        private TabAppearance LastAppearance = TabAppearance.Normal;
        private Size LastItemSize = Size.Empty;
        private TabSizeMode LastSizeMode = TabSizeMode.Normal;

        private int HoverTabHeaderInvalidated = -1;
        private bool ControlEnabled = true;
        private bool once = true;

        public CustomTabControl() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Opaque, true);

            ControlEnabled = Enabled;
            Appearance = TabAppearance.Normal;

            HideTabHeaderChanged += CustomTabControl_HideTabHeaderChanged;
            
            ControlAdded += CustomTabControl_ControlAdded;
            ControlRemoved += CustomTabControl_ControlRemoved;
            Application.Idle += Application_Idle;
            HandleCreated += CustomTabControl_HandleCreated;
            LocationChanged += CustomTabControl_LocationChanged;
            Move += CustomTabControl_Move;
            SizeChanged += CustomTabControl_SizeChanged;
            EnabledChanged += CustomTabControl_EnabledChanged;
            Invalidated += CustomTabControl_Invalidated;
            MouseMove += CustomTabControl_MouseMove;
            MouseLeave += CustomTabControl_MouseLeave;
            Paint += CustomTabControl_Paint;
        }

        private void CustomTabControl_HideTabHeaderChanged(object? sender, EventArgs e)
        {
            if (mHideTabHeader)
            {
                LastAlignment = Alignment;
                LastAppearance = Appearance;
                LastItemSize = ItemSize;
                LastSizeMode = SizeMode;

                Alignment = TabAlignment.Top;
                Appearance = TabAppearance.FlatButtons;
                ItemSize = new(0, 1);
                SizeMode = TabSizeMode.Fixed;
            }
            else
            {
                Alignment = LastAlignment;
                Appearance = LastAppearance;
                ItemSize = LastItemSize;
                SizeMode = LastSizeMode;
            }
        }

        private void SearchTabPages()
        {
            for (int n = 0; n < TabPages.Count; n++)
            {
                TabPage tabPage = TabPages[n];
                tabPage.Tag = n;
                tabPage.Paint -= TabPage_Paint;
                tabPage.Paint += TabPage_Paint;
            }
        }

        private void CustomTabControl_ControlAdded(object? sender, ControlEventArgs e)
        {
            if (e.Control is TabPage)
                SearchTabPages();
            Invalidate();
        }

        private void CustomTabControl_ControlRemoved(object? sender, ControlEventArgs e)
        {
            if (e.Control is TabPage)
                SearchTabPages();
            Invalidate();
        }

        private void Application_Idle(object? sender, EventArgs e)
        {
            if (Parent != null && FindForm() != null)
            {
                if (once)
                {
                    SearchTabPages();

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

        private void TabPage_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not TabPage tabPage) return;
            if (!Visible) return;

            Color tabPageColor;
            if (Enabled)
                tabPageColor = tabPage.BackColor;
            else
            {
                if (tabPage.BackColor.DarkOrLight() == "Dark")
                    tabPageColor = tabPage.BackColor.ChangeBrightness(0.3f);
                else
                    tabPageColor = tabPage.BackColor.ChangeBrightness(-0.3f);
            }

            if (tabPageColor.Equals(Color.Transparent)) tabPageColor = BackColor;

            using SolidBrush sb = new(tabPageColor);
            e.Graphics.FillRectangle(sb, e.ClipRectangle);
        }

        private void TopParent_Move(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void Parent_Move(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomTabControl_HandleCreated(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomTabControl_LocationChanged(object? sender, EventArgs e)
        {
            if (sender is TabControl tabControl)
                tabControl.Invalidate();
        }

        private void CustomTabControl_Move(object? sender, EventArgs e)
        {
            if (sender is TabControl tabControl)
                tabControl.Invalidate();
        }

        private void CustomTabControl_SizeChanged(object? sender, EventArgs e)
        {
            if (sender is TabControl tabControl)
                tabControl.Invalidate();
        }

        private void CustomTabControl_EnabledChanged(object? sender, EventArgs e)
        {
            ControlEnabled = Enabled;
        }

        private void CustomTabControl_Invalidated(object? sender, InvalidateEventArgs e)
        {
            if (BackColor.DarkOrLight() == "Dark")
                this.SetDarkControl();
        }

        private void CustomTabControl_MouseMove(object? sender, MouseEventArgs e)
        {
            InvalidateHoverTabHeader();
        }

        private void CustomTabControl_MouseLeave(object? sender, EventArgs e)
        {
            if (HoverTabHeaderInvalidated >= 0 && HoverTabHeaderInvalidated < TabPages.Count)
            {
                Invalidate(GetTabRect(HoverTabHeaderInvalidated));
                HoverTabHeaderInvalidated = -1;
            }
        }

        private void InvalidateHoverTabHeader()
        {
            for (int n = 0; n < TabPages.Count; n++)
            {
                Rectangle rectTab = GetTabRect(n);
                if (!DesignMode && Enabled && rectTab.Contains(PointToClient(MousePosition)))
                {
                    if (n != HoverTabHeaderInvalidated)
                    {
                        Invalidate(rectTab);
                        HoverTabHeaderInvalidated = n;
                    }
                }
            }
        }

        private void CustomTabControl_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not TabControl tc) return;
            PaintTabControl(tc, e.Graphics);
        }

        private void PaintTabControl(TabControl tc, Graphics g)
        {
            if (!Visible) return;

            Color backColor = GetBackColor();
            Color foreColor = GetForeColor();
            Color borderColor = GetBorderColor();
            int r = RoundedCorners;

            // Paint Background
            g.Clear(backColor);

            for (int n = 0; n < TabPages.Count; n++)
            {
                TabPage tabPage = TabPages[n];
                int selectedIndex = tc.SelectedIndex == -1 ? 0 : tc.SelectedIndex;
                TabPage selectedTabPage = TabPages[selectedIndex];
                int index = n;
                Rectangle rectTab = GetTabRect(index);
                Rectangle rectSTab = GetTabRect(selectedIndex);
                Rectangle cr = tc.ClientRectangle;
                Rectangle rectPage = cr; // tabPage.ClientRectangle

                // Paint Main Control Border
                if (Alignment == TabAlignment.Top)
                {
                    if (RightToLeft == RightToLeft.Yes && RightToLeftLayout)
                    {
                        if (!HideTabHeader)
                            rectPage = new Rectangle(rectPage.X + 1, rectSTab.Bottom, Width - 2, Height - rectSTab.Bottom);
                        else
                            rectPage = Rectangle.FromLTRB(cr.Left + 1, cr.Top, cr.Right, cr.Bottom);
                    }
                    else
                    {
                        if (!HideTabHeader)
                            rectPage = new Rectangle(rectPage.X, rectSTab.Bottom, Width, Height - rectSTab.Bottom);
                        else
                            rectPage = cr;
                    }
                }
                else if (Alignment == TabAlignment.Bottom)
                {
                    if (RightToLeft == RightToLeft.Yes && RightToLeftLayout)
                    {
                        if (!HideTabHeader)
                            rectPage = new Rectangle(rectPage.X + 1, rectPage.Y, Width - 2, Height - (Height - rectSTab.Top - 1));
                        else
                            rectPage = Rectangle.FromLTRB(cr.Left + 1, cr.Top, cr.Right, cr.Bottom);
                    }
                    else
                    {
                        if (!HideTabHeader)
                            rectPage = new Rectangle(rectPage.X, rectPage.Y, Width, Height - (Height - rectSTab.Top - 1));
                        else
                            rectPage = cr;
                    }
                }
                else if (Alignment == TabAlignment.Left)
                {
                    if (RightToLeft == RightToLeft.Yes && RightToLeftLayout)
                    {
                        if (!HideTabHeader)
                            rectPage = new Rectangle(rectSTab.Right, rectPage.Y, Width - rectSTab.Right, Height);
                        else
                            rectPage = Rectangle.FromLTRB(cr.Left + 1, cr.Top, cr.Right, cr.Bottom);
                    }
                    else
                    {
                        if (!HideTabHeader)
                            rectPage = new Rectangle(rectSTab.Right, rectPage.Y, Width - rectSTab.Right, Height);
                        else
                            rectPage = cr;
                    }
                }
                else if (Alignment == TabAlignment.Right)
                {
                    if (RightToLeft == RightToLeft.Yes && RightToLeftLayout)
                    {
                        if (!HideTabHeader)
                            rectPage = new Rectangle(rectPage.X, rectPage.Y, Width - (Width - rectSTab.Left - 1), Height);
                        else
                            rectPage = Rectangle.FromLTRB(cr.Left + 1, cr.Top, cr.Right, cr.Bottom);
                    }
                    else
                    {
                        if (!HideTabHeader)
                            rectPage = new Rectangle(rectPage.X, rectPage.Y, Width - (Width - rectSTab.Left - 1), Height);
                        else
                            rectPage = cr;
                    }
                }

                Rectangle borderRectPage = new(rectPage.X, rectPage.Y, rectPage.Width - 1, rectPage.Height - 1);

                if (!mHideTabHeader)
                {
                    // Paint Non-Selected Tab Headers
                    if (selectedIndex != n)
                    {
                        // Paint Non-Selected Tab Headers Background
                        Color tabHeaderColor = backColor;

                        if (!DesignMode && Enabled && rectTab.Contains(tc.PointToClient(MousePosition)))
                        {
                            Color colorHover;
                            if (backColor.DarkOrLight() == "Dark")
                                colorHover = backColor.ChangeBrightness(0.2f);
                            else
                                colorHover = backColor.ChangeBrightness(-0.2f);
                            tabHeaderColor = colorHover;
                        }

                        using SolidBrush brush = new(tabHeaderColor);

                        if (Alignment == TabAlignment.Top)
                            g.FillRoundedRectangle(brush, rectTab, r, r, 0, 0);
                        else if (Alignment == TabAlignment.Bottom)
                            g.FillRoundedRectangle(brush, rectTab, 0, 0, r, r);
                        else if (Alignment == TabAlignment.Left)
                            g.FillRoundedRectangle(brush, rectTab, r, 0, 0, r);
                        else if (Alignment == TabAlignment.Right)
                            g.FillRoundedRectangle(brush, rectTab, 0, r, r, 0);

                        // Paint Non-Selected Tab Headers Text & Image
                        int tabImageIndex = tabPage.ImageIndex;
                        string tabImageKey = tabPage.ImageKey;

                        if (tabImageIndex != -1 && tc.ImageList != null)
                        {
                            Image tabImage = tc.ImageList.Images[tabImageIndex];
                            PaintImageText(g, tc, tabPage, rectTab, tabImage, Font, foreColor);
                        }
                        else if (tabImageKey != null && tc.ImageList != null)
                        {
                            Image tabImage = tc.ImageList.Images[tabImageKey];
                            PaintImageText(g, tc, tabPage, rectTab, tabImage, Font, foreColor);
                        }
                        else
                        {
                            TextRenderer.DrawText(g, tabPage.Text, Font, rectTab, foreColor);
                        }

                        // Paint Non-Selected Tab Headers Border
                        using Pen borderPenRt = new(borderColor);
                        if (Alignment == TabAlignment.Top)
                        {
                            if (rectTab.Bottom + 1 < rectPage.Y)
                                g.DrawRoundedRectangle(borderPenRt, rectTab, r, r, r, r);
                            else
                                g.DrawRoundedRectangle(borderPenRt, rectTab, r, r, 0, 0);
                        }
                        else if (Alignment == TabAlignment.Bottom)
                        {
                            if (rectTab.Top + 1 > rectPage.Bottom)
                                g.DrawRoundedRectangle(borderPenRt, rectTab, r, r, r, r);
                            else
                                g.DrawRoundedRectangle(borderPenRt, rectTab, 0, 0, r, r);
                        }
                        else if (Alignment == TabAlignment.Left)
                        {
                            if (rectTab.Right + 1 < rectPage.X)
                                g.DrawRoundedRectangle(borderPenRt, rectTab, r, r, r, r);
                            else
                                g.DrawRoundedRectangle(borderPenRt, rectTab, r, 0, 0, r);
                        }
                        else if (Alignment == TabAlignment.Right)
                        {
                            if (rectTab.Left + 1 > rectPage.Right)
                                g.DrawRoundedRectangle(borderPenRt, rectTab, r, r, r, r);
                            else
                                g.DrawRoundedRectangle(borderPenRt, rectTab, 0, r, r, 0);
                        }
                    }

                    // Paint Selected Tab Header Background
                    using SolidBrush brushST = new(backColor.ChangeBrightness(-0.3f));

                    if (Alignment == TabAlignment.Top)
                        g.FillRoundedRectangle(brushST, rectSTab, r, r, 0, 0);
                    else if (Alignment == TabAlignment.Bottom)
                        g.FillRoundedRectangle(brushST, rectSTab, 0, 0, r, r);
                    else if (Alignment == TabAlignment.Left)
                        g.FillRoundedRectangle(brushST, rectSTab, r, 0, 0, r);
                    else if (Alignment == TabAlignment.Right)
                        g.FillRoundedRectangle(brushST, rectSTab, 0, r, r, 0);

                    // Paint Selected Tab Header Text & Image
                    int selectedTabImageIndex = selectedTabPage.ImageIndex;
                    string selectedTabImageKey = selectedTabPage.ImageKey;

                    if (selectedTabImageIndex != -1 && tc.ImageList != null)
                    {
                        Image tabImage = tc.ImageList.Images[selectedTabImageIndex];
                        PaintImageText(g, tc, selectedTabPage, rectSTab, tabImage, Font, foreColor);
                    }
                    else if (selectedTabImageKey != null && tc.ImageList != null)
                    {
                        Image tabImage = tc.ImageList.Images[selectedTabImageKey];
                        PaintImageText(g, tc, selectedTabPage, rectSTab, tabImage, Font, foreColor);
                    }
                    else
                    {
                        TextRenderer.DrawText(g, selectedTabPage.Text, Font, rectSTab, foreColor);
                    }

                    // Paint Selected Tab Header Border
                    using Pen borderPenSt = new(borderColor);
                    if (Alignment == TabAlignment.Top)
                        g.DrawRoundedRectangle(borderPenSt, rectSTab, r, r, 0, 0);
                    else if (Alignment == TabAlignment.Bottom)
                        g.DrawRoundedRectangle(borderPenSt, rectSTab, 0, 0, r, r);
                    else if (Alignment == TabAlignment.Left)
                        g.DrawRoundedRectangle(borderPenSt, rectSTab, r, 0, 0, r);
                    else if (Alignment == TabAlignment.Right)
                        g.DrawRoundedRectangle(borderPenSt, rectSTab, 0, r, r, 0);

                    // Paint TabPage Border
                    using Pen borderPen = new(borderColor);
                    if (Alignment == TabAlignment.Top)
                        g.DrawRoundedRectangle(borderPen, borderRectPage, 0, r, r, r);
                    else if (Alignment == TabAlignment.Bottom)
                        g.DrawRoundedRectangle(borderPen, borderRectPage, r, r, r, 0);
                    else if (Alignment == TabAlignment.Left)
                        g.DrawRoundedRectangle(borderPen, borderRectPage, 0, r, r, r);
                    else if (Alignment == TabAlignment.Right)
                        g.DrawRoundedRectangle(borderPen, borderRectPage, r, 0, r, r);

                    // Paint Overlap Between Selected Tab Header and TabPage
                    if (Alignment == TabAlignment.Top)
                    {
                        // to overlap selected tab bottom line
                        using Pen penLine = new(backColor.ChangeBrightness(-0.3f));
                        g.DrawLine(penLine, rectSTab.Left + 1, rectSTab.Bottom, rectSTab.Right - 1, rectSTab.Bottom);
                    }
                    else if (Alignment == TabAlignment.Bottom)
                    {
                        // to overlap selected tab top line
                        using Pen penLine = new(backColor.ChangeBrightness(-0.3f));
                        g.DrawLine(penLine, rectSTab.Left + 1, rectSTab.Top, rectSTab.Right - 1, rectSTab.Top);
                    }
                    else if (Alignment == TabAlignment.Left)
                    {
                        // to overlap selected tab right line
                        using Pen penLine = new(backColor.ChangeBrightness(-0.3f));
                        g.DrawLine(penLine, rectSTab.Right, rectSTab.Top + 1, rectSTab.Right, rectSTab.Bottom - 1);
                    }
                    else if (Alignment == TabAlignment.Right)
                    {
                        // to overlap selected tab left line
                        using Pen penLine = new(backColor.ChangeBrightness(-0.3f));
                        g.DrawLine(penLine, rectSTab.Left, rectSTab.Top + 1, rectSTab.Left, rectSTab.Bottom - 1);
                    }
                }
                else
                {
                    // Paint Main Control Border
                    using Pen borderPen = new(borderColor);
                    g.DrawRoundedRectangle(borderPen, borderRectPage, r, r, r, r);
                }
            }
        }

        private void PaintImageText(Graphics graphics, TabControl tc, TabPage tabPage, Rectangle rectTab, Image? tabImage, Font font, Color foreColor)
        {
            if (!Visible) return;
            if (HideTabHeader) return;
            if (tabImage != null)
            {
                Rectangle rectImage = new(rectTab.X + tc.Padding.X, rectTab.Y + tc.Padding.Y, tabImage.Width, tabImage.Height);
                rectImage.Location = new(rectImage.X, rectTab.Y + (rectTab.Height - rectImage.Height) / 2);
                graphics.DrawImage(tabImage, rectImage);
                Rectangle rectText = new(rectTab.X + rectImage.Width, rectTab.Y, rectTab.Width - rectImage.Width, rectTab.Height);
                TextRenderer.DrawText(graphics, tabPage.Text, font, rectText, foreColor);
            }
            else
                TextRenderer.DrawText(graphics, tabPage.Text, font, rectTab, foreColor);
        }

        private Color GetBackColor()
        {
            if (ControlEnabled)
                return BackColor;
            else
            {
                Color disabledBackColor;
                if (BackColor.DarkOrLight() == "Dark")
                    disabledBackColor = BackColor.ChangeBrightness(0.3f);
                else
                    disabledBackColor = BackColor.ChangeBrightness(-0.3f);
                return disabledBackColor;
            }
        }

        private Color GetForeColor()
        {
            if (ControlEnabled)
                return ForeColor;
            else
            {
                Color disabledForeColor;
                if (ForeColor.DarkOrLight() == "Dark")
                    disabledForeColor = ForeColor.ChangeBrightness(0.2f);
                else
                    disabledForeColor = ForeColor.ChangeBrightness(-0.2f);
                return disabledForeColor;
            }
        }

        private Color GetBorderColor()
        {
            if (ControlEnabled)
                return BorderColor;
            else
            {
                Color disabledBorderColor;
                if (BorderColor.DarkOrLight() == "Dark")
                    disabledBorderColor = BorderColor.ChangeBrightness(0.3f);
                else
                    disabledBorderColor = BorderColor.ChangeBrightness(-0.3f);
                return disabledBorderColor;
            }
        }
    }
}
