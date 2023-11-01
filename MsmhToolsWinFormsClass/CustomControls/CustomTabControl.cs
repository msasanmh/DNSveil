using MsmhToolsClass;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms.Design;
/*
* Copyright MSasanMH, June 01, 2022.
*/

namespace CustomControls
{
    public class CustomTabControl : TabControl
    {
        private static class Methods
        {
            [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
            private extern static int SetWindowTheme(IntPtr controlHandle, string appName, string? idList);
            internal static void SetDarkControl(Control control)
            {
                _ = SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
                foreach (Control c in control.Controls)
                {
                    _ = SetWindowTheme(c.Handle, "DarkMode_Explorer", null);
                }
            }
        }

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
                Appearance = TabAppearance.Buttons;
                ItemSize = new(0, 1);
                SizeMode = TabSizeMode.Fixed;
            }
            else
            {
                Appearance = TabAppearance.Normal;
                ItemSize = Size.Empty;
                SizeMode = TabSizeMode.FillToRight;
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

            using SolidBrush sb = new(tabPageColor);
            e.Graphics.FillRectangle(sb, e.ClipRectangle);

            Control tabControl = tabPage.Parent;
            tabControl.Tag = tabPage.Tag;
            tabControl.Paint -= TabControl_Paint;
            tabControl.Paint += TabControl_Paint;
        }

        private void TabControl_Paint(object? sender, PaintEventArgs e)
        {
            // Selected Tab Can Be Paint Also Here
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
                Methods.SetDarkControl(this);
        }

        private void CustomTabControl_MouseMove(object? sender, MouseEventArgs e)
        {
            Invalidate();
        }

        private void CustomTabControl_MouseLeave(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomTabControl_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not TabControl tc) return;
            PaintTabControl(tc, e.Graphics);
        }

        private void PaintTabControl(TabControl tc, Graphics g)
        {
            Color backColor = GetBackColor();
            Color foreColor = GetForeColor();
            Color borderColor = GetBorderColor();
            int r = RoundedCorners;

            // Paint Background
            g.Clear(backColor);

            for (int n = 0; n < TabPages.Count; n++)
            {
                TabPage tabPage = TabPages[n];
                TabPage selectedTabPage = TabPages[tc.SelectedIndex];
                int index = n;
                Rectangle rectTab = GetTabRect(index);
                Rectangle rectSTab = GetTabRect(tc.SelectedIndex);
                Rectangle cr = ClientRectangle;
                Rectangle rectPage = ClientRectangle;
                using Pen pen = new(borderColor);
                using SolidBrush brush = new(backColor);

                // Paint Main Control Border
                if (Alignment == TabAlignment.Top)
                {
                    rectTab = Rectangle.FromLTRB(rectTab.Left, rectSTab.Top + 2, rectTab.Right, rectSTab.Bottom);
                    if (RightToLeft == RightToLeft.Yes && RightToLeftLayout)
                    {
                        //rectSTab = Rectangle.FromLTRB(rectSTab.Left, rectSTab.Top - 2, rectSTab.Right, rectSTab.Bottom - 3);
                        //rectTab = Rectangle.FromLTRB(rectTab.Left, rectSTab.Top + 2, rectTab.Right, rectSTab.Bottom);
                        if (!HideTabHeader)
                            rectPage = Rectangle.FromLTRB(cr.Left + 1, cr.Top + rectSTab.Height + 2, cr.Right, cr.Bottom);
                        else
                            rectPage = Rectangle.FromLTRB(cr.Left + 1, cr.Top, cr.Right, cr.Bottom);
                    }
                    else
                    {
                        //rectSTab = Rectangle.FromLTRB(rectSTab.Left, rectSTab.Top - 2, rectSTab.Right, rectSTab.Bottom - 3);
                        //rectTab = Rectangle.FromLTRB(rectTab.Left, rectSTab.Top + 2, rectTab.Right, rectSTab.Bottom);
                        if (!HideTabHeader)
                            rectPage = Rectangle.FromLTRB(cr.Left, cr.Top + rectSTab.Height + 2, cr.Right, cr.Bottom);
                        else
                            rectPage = Rectangle.FromLTRB(cr.Left, cr.Top, cr.Right, cr.Bottom);
                    }
                }
                else if (Alignment == TabAlignment.Bottom)
                {
                    rectTab = Rectangle.FromLTRB(rectTab.Left, rectSTab.Top, rectTab.Right, rectSTab.Bottom - 2);
                    if (RightToLeft == RightToLeft.Yes && RightToLeftLayout)
                    {
                        //rectSTab = Rectangle.FromLTRB(rectSTab.Left, rectSTab.Top + 1, rectSTab.Right, rectSTab.Bottom + 1);
                        //rectTab = Rectangle.FromLTRB(rectTab.Left, rectSTab.Top, rectTab.Right, rectSTab.Bottom - 2);
                        if (!HideTabHeader)
                            rectPage = Rectangle.FromLTRB(cr.Left + 1, cr.Top, cr.Right, cr.Bottom - rectSTab.Height - 1);
                        else
                            rectPage = Rectangle.FromLTRB(cr.Left + 1, cr.Top, cr.Right, cr.Bottom);
                    }
                    else
                    {
                        //rectSTab = Rectangle.FromLTRB(rectSTab.Left, rectSTab.Top + 1, rectSTab.Right, rectSTab.Bottom + 1);
                        //rectTab = Rectangle.FromLTRB(rectTab.Left, rectSTab.Top, rectTab.Right, rectSTab.Bottom - 2);
                        if (!HideTabHeader)
                            rectPage = Rectangle.FromLTRB(cr.Left, cr.Top, cr.Right, cr.Bottom - rectSTab.Height - 1);
                        else
                            rectPage = Rectangle.FromLTRB(cr.Left, cr.Top, cr.Right, cr.Bottom);
                    }
                }
                else if (Alignment == TabAlignment.Left)
                {
                    rectTab = Rectangle.FromLTRB(rectSTab.Left + 2, rectTab.Top, rectSTab.Right, rectTab.Bottom);
                    if (RightToLeft == RightToLeft.Yes && RightToLeftLayout)
                    {
                        //rectSTab = Rectangle.FromLTRB(rectSTab.Left - 1, rectSTab.Top, rectSTab.Right - 3, rectSTab.Bottom);
                        //rectTab = Rectangle.FromLTRB(rectSTab.Left + 2, rectTab.Top, rectSTab.Right, rectTab.Bottom);
                        if (!HideTabHeader)
                            rectPage = Rectangle.FromLTRB(cr.Left + 1 + rectSTab.Width + 1, cr.Top, cr.Right, cr.Bottom);
                        else
                            rectPage = Rectangle.FromLTRB(cr.Left + 1, cr.Top, cr.Right, cr.Bottom);
                    }
                    else
                    {
                        //rectSTab = Rectangle.FromLTRB(rectSTab.Left - 2, rectSTab.Top, rectSTab.Right - 2, rectSTab.Bottom);
                        //rectTab = Rectangle.FromLTRB(rectSTab.Left + 2, rectTab.Top, rectSTab.Right, rectTab.Bottom);
                        if (!HideTabHeader)
                            rectPage = Rectangle.FromLTRB(cr.Left + rectSTab.Width + 2, cr.Top, cr.Right, cr.Bottom);
                        else
                            rectPage = Rectangle.FromLTRB(cr.Left, cr.Top, cr.Right, cr.Bottom);
                    }
                }
                else if (Alignment == TabAlignment.Right)
                {
                    rectTab = Rectangle.FromLTRB(rectSTab.Left, rectTab.Top, rectSTab.Right - 2, rectTab.Bottom);
                    if (RightToLeft == RightToLeft.Yes && RightToLeftLayout)
                    {
                        //rectSTab = Rectangle.FromLTRB(rectSTab.Left + 1, rectSTab.Top, rectSTab.Right + 1, rectSTab.Bottom);
                        //rectTab = Rectangle.FromLTRB(rectSTab.Left, rectTab.Top, rectSTab.Right - 2, rectTab.Bottom);
                        if (!HideTabHeader)
                            rectPage = Rectangle.FromLTRB(cr.Left + 1, cr.Top, cr.Right - rectSTab.Width - 1, cr.Bottom);
                        else
                            rectPage = Rectangle.FromLTRB(cr.Left + 1, cr.Top, cr.Right, cr.Bottom);
                    }
                    else
                    {
                        //rectSTab = Rectangle.FromLTRB(rectSTab.Left + 1, rectSTab.Top, rectSTab.Right + 1, rectSTab.Bottom);
                        //rectTab = Rectangle.FromLTRB(rectSTab.Left, rectTab.Top, rectSTab.Right - 2, rectTab.Bottom);
                        if (!HideTabHeader)
                            rectPage = Rectangle.FromLTRB(cr.Left, cr.Top, cr.Right - rectSTab.Width - 1, cr.Bottom);
                        else
                            rectPage = Rectangle.FromLTRB(cr.Left, cr.Top, cr.Right, cr.Bottom);
                    }
                }

                //ControlPaint.DrawBorder(e.Graphics, rectPage, borderColor, ButtonBorderStyle.Solid);
                Rectangle borderRect = new(rectPage.X, rectPage.Y, rectPage.Width - 1, rectPage.Height - 1);

                if (!mHideTabHeader)
                {
                    // Paint Non-Selected Tab
                    if (tc.SelectedIndex != n)
                    {
                        //e.Graphics.FillRectangle(brush, rectTab);
                        if (Alignment == TabAlignment.Top)
                            g.FillRoundedRectangle(brush, rectTab, r, r, 0, 0);
                        else if (Alignment == TabAlignment.Bottom)
                            g.FillRoundedRectangle(brush, rectTab, 0, 0, r, r);
                        else if (Alignment == TabAlignment.Left)
                            g.FillRoundedRectangle(brush, rectTab, r, 0, 0, r);
                        else if (Alignment == TabAlignment.Right)
                            g.FillRoundedRectangle(brush, rectTab, 0, r, r, 0);

                        if (!DesignMode && Enabled && rectTab.Contains(tc.PointToClient(MousePosition)))
                        {
                            Color colorHover;
                            if (backColor.DarkOrLight() == "Dark")
                                colorHover = backColor.ChangeBrightness(0.2f);
                            else
                                colorHover = backColor.ChangeBrightness(-0.2f);
                            using SolidBrush brushHover = new(colorHover);
                            //g.FillRectangle(brushHover, rectTab);
                            if (Alignment == TabAlignment.Top)
                                g.FillRoundedRectangle(brushHover, rectTab, r, r, 0, 0);
                            else if (Alignment == TabAlignment.Bottom)
                                g.FillRoundedRectangle(brushHover, rectTab, 0, 0, r, r);
                            else if (Alignment == TabAlignment.Left)
                                g.FillRoundedRectangle(brushHover, rectTab, r, 0, 0, r);
                            else if (Alignment == TabAlignment.Right)
                                g.FillRoundedRectangle(brushHover, rectTab, 0, r, r, 0);
                        }

                        int tabImageIndex = tabPage.ImageIndex;
                        string tabImageKey = tabPage.ImageKey;

                        // Draw Text and Image inside non selected Tab headers
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

                        // Draw Border of non selected Tab headers
                        //e.Graphics.DrawRectangle(pen, rectTab);
                        using Pen borderPenRt = new(borderColor);
                        if (Alignment == TabAlignment.Top)
                            g.DrawRoundedRectangle(borderPenRt, rectTab, r, r, 0, 0);
                        else if (Alignment == TabAlignment.Bottom)
                            g.DrawRoundedRectangle(borderPenRt, rectTab, 0, 0, r, r);
                        else if (Alignment == TabAlignment.Left)
                            g.DrawRoundedRectangle(borderPenRt, rectTab, r, 0, 0, r);
                        else if (Alignment == TabAlignment.Right)
                            g.DrawRoundedRectangle(borderPenRt, rectTab, 0, r, r, 0);
                    }

                    // Paint Selected Tab
                    using SolidBrush brushST = new(backColor.ChangeBrightness(-0.3f));
                    //e.Graphics.FillRectangle(brushST, rectSTab);
                    if (Alignment == TabAlignment.Top)
                        g.FillRoundedRectangle(brushST, rectSTab, r, r, 0, 0);
                    else if (Alignment == TabAlignment.Bottom)
                        g.FillRoundedRectangle(brushST, rectSTab, 0, 0, r, r);
                    else if (Alignment == TabAlignment.Left)
                        g.FillRoundedRectangle(brushST, rectSTab, r, 0, 0, r);
                    else if (Alignment == TabAlignment.Right)
                        g.FillRoundedRectangle(brushST, rectSTab, 0, r, r, 0);

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

                    //e.Graphics.DrawRectangle(pen, rectSTab);
                    using Pen borderPenSt = new(borderColor);
                    if (Alignment == TabAlignment.Top)
                        g.DrawRoundedRectangle(borderPenSt, rectSTab, r, r, 0, 0);
                    else if (Alignment == TabAlignment.Bottom)
                        g.DrawRoundedRectangle(borderPenSt, rectSTab, 0, 0, r, r);
                    else if (Alignment == TabAlignment.Left)
                        g.DrawRoundedRectangle(borderPenSt, rectSTab, r, 0, 0, r);
                    else if (Alignment == TabAlignment.Right)
                        g.DrawRoundedRectangle(borderPenSt, rectSTab, 0, r, r, 0);

                    // Draw Border of TabPages
                    using Pen borderPen = new(borderColor);
                    if (Alignment == TabAlignment.Top)
                        g.DrawRoundedRectangle(borderPen, borderRect, 0, r, r, r);
                    else if (Alignment == TabAlignment.Bottom)
                        g.DrawRoundedRectangle(borderPen, borderRect, r, r, r, 0);
                    else if (Alignment == TabAlignment.Left)
                        g.DrawRoundedRectangle(borderPen, borderRect, 0, r, r, r);
                    else if (Alignment == TabAlignment.Right)
                        g.DrawRoundedRectangle(borderPen, borderRect, r, 0, r, r);

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
                    //ControlPaint.DrawBorder(g, ClientRectangle, borderColor, ButtonBorderStyle.Solid);
                    using Pen borderPen = new(borderColor);
                    g.DrawRoundedRectangle(borderPen, borderRect, r, r, r, r);
                }
            }
        }

        private void PaintImageText(Graphics graphics, TabControl tc, TabPage tabPage, Rectangle rectTab, Image? tabImage, Font font, Color foreColor)
        {
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
