using MsmhToolsClass;
using System;
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

        private void CustomTabControl_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is TabControl tc)
            {
                Color backColor = GetBackColor();
                Color foreColor = GetForeColor();
                Color borderColor = GetBorderColor();
                
                // Paint Background
                e.Graphics.Clear(backColor);

                for (int n = 0; n < TabPages.Count; n++)
                {
                    TabPage tabPage = TabPages[n];
                    TabPage selectedTabPage = TabPages[tc.SelectedIndex];
                    int index = n;
                    Rectangle rectTab = GetTabRect(index);
                    using Pen pen = new(borderColor);
                    using SolidBrush brush = new(backColor);
                    
                    // Mouse Position
                    Point mP = tc.PointToClient(MousePosition);

                    // Selected tab Rectangle
                    Rectangle rectSelectedTab = GetTabRect(tc.SelectedIndex);
                    if (Alignment == TabAlignment.Top)
                    {
                        if (RightToLeft == RightToLeft.No || RightToLeft == RightToLeft.Yes && !RightToLeftLayout)
                            rectSelectedTab = Rectangle.FromLTRB(rectSelectedTab.Left - 2, rectSelectedTab.Top - 2, rectSelectedTab.Right + 1, rectSelectedTab.Bottom);
                        else if (RightToLeft == RightToLeft.Yes && RightToLeftLayout)
                            rectSelectedTab = Rectangle.FromLTRB(rectSelectedTab.Left - 1, rectSelectedTab.Top - 2, rectSelectedTab.Right, rectSelectedTab.Bottom);
                    }
                    
                    if (!mHideTabHeader)
                    {
                        // Paint Non-Selected Tab
                        if (tc.SelectedIndex != n)
                        {
                            e.Graphics.FillRectangle(brush, rectTab);

                            if (!DesignMode && Enabled && rectTab.Contains(mP))
                            {
                                Color colorHover;
                                if (backColor.DarkOrLight() == "Dark")
                                    colorHover = backColor.ChangeBrightness(0.2f);
                                else
                                    colorHover = backColor.ChangeBrightness(-0.2f);
                                using SolidBrush brushHover = new(colorHover);
                                e.Graphics.FillRectangle(brushHover, rectTab);
                            }

                            int tabImageIndex = tabPage.ImageIndex;
                            string tabImageKey = tabPage.ImageKey;
                            
                            if (tabImageIndex != -1 && tc.ImageList != null)
                            {
                                Image tabImage = tc.ImageList.Images[tabImageIndex];
                                PaintImageText(e.Graphics, tc, tabPage, rectTab, tabImage, Font, foreColor);
                            }
                            else if (tabImageKey != null && tc.ImageList != null)
                            {
                                Image tabImage = tc.ImageList.Images[tabImageKey];
                                PaintImageText(e.Graphics, tc, tabPage, rectTab, tabImage, Font, foreColor);
                            }
                            else
                            {
                                TextRenderer.DrawText(e.Graphics, tabPage.Text, Font, rectTab, foreColor);
                            }

                            e.Graphics.DrawRectangle(pen, rectTab);
                        }

                        // Paint Selected Tab
                        using SolidBrush brushST = new(backColor.ChangeBrightness(-0.3f));
                        e.Graphics.FillRectangle(brushST, rectSelectedTab);

                        int selectedTabImageIndex = selectedTabPage.ImageIndex;
                        string selectedTabImageKey = selectedTabPage.ImageKey;

                        if (selectedTabImageIndex != -1 && tc.ImageList != null)
                        {
                            Image tabImage = tc.ImageList.Images[selectedTabImageIndex];
                            PaintImageText(e.Graphics, tc, selectedTabPage, rectSelectedTab, tabImage, Font, foreColor);
                        }
                        else if (selectedTabImageKey != null && tc.ImageList != null)
                        {
                            Image tabImage = tc.ImageList.Images[selectedTabImageKey];
                            PaintImageText(e.Graphics, tc, selectedTabPage, rectSelectedTab, tabImage, Font, foreColor);
                        }
                        else
                        {
                            TextRenderer.DrawText(e.Graphics, selectedTabPage.Text, Font, rectSelectedTab, foreColor);
                        }

                        e.Graphics.DrawRectangle(pen, rectSelectedTab);

                        // Paint Main Control Border
                        Rectangle rectPage = ClientRectangle;
                        if (Alignment == TabAlignment.Top)
                        {
                            rectPage = Rectangle.FromLTRB(ClientRectangle.Left, rectSelectedTab.Bottom, ClientRectangle.Right, ClientRectangle.Bottom);
                            if (RightToLeft == RightToLeft.Yes && RightToLeftLayout)
                                rectPage = Rectangle.FromLTRB(ClientRectangle.Left, rectSelectedTab.Bottom, ClientRectangle.Right - 1, ClientRectangle.Bottom);
                        }
                        else if (Alignment == TabAlignment.Bottom)
                        {
                            rectPage = Rectangle.FromLTRB(ClientRectangle.Left, ClientRectangle.Top, ClientRectangle.Right, rectSelectedTab.Top + 1);
                        }
                        else if (Alignment == TabAlignment.Left)
                        {
                            rectPage = Rectangle.FromLTRB(rectSelectedTab.Right, ClientRectangle.Top, ClientRectangle.Right, ClientRectangle.Bottom);
                        }
                        else if (Alignment == TabAlignment.Right)
                        {
                            rectPage = Rectangle.FromLTRB(ClientRectangle.Left, ClientRectangle.Top, rectSelectedTab.Left + 1, ClientRectangle.Bottom);
                        }

                        ControlPaint.DrawBorder(e.Graphics, rectPage, borderColor, ButtonBorderStyle.Solid);

                        if (Alignment == TabAlignment.Top)
                        {
                            // to overlap selected tab bottom line
                            using Pen penLine = new(backColor.ChangeBrightness(-0.3f));
                            e.Graphics.DrawLine(penLine, rectSelectedTab.Left + 1, rectSelectedTab.Bottom, rectSelectedTab.Right - 1, rectSelectedTab.Bottom);
                        }
                        else if (Alignment == TabAlignment.Bottom)
                        {
                            // to overlap selected tab top line
                            using Pen penLine = new(backColor.ChangeBrightness(-0.3f));
                            e.Graphics.DrawLine(penLine, rectSelectedTab.Left + 1, rectSelectedTab.Top, rectSelectedTab.Right - 1, rectSelectedTab.Top);
                        }
                        else if (Alignment == TabAlignment.Left)
                        {
                            // to overlap selected tab right line
                            using Pen penLine = new(backColor.ChangeBrightness(-0.3f));
                            e.Graphics.DrawLine(penLine, rectSelectedTab.Right, rectSelectedTab.Top + 1, rectSelectedTab.Right, rectSelectedTab.Bottom - 1);
                        }
                        else if (Alignment == TabAlignment.Right)
                        {
                            // to overlap selected tab left line
                            using Pen penLine = new(backColor.ChangeBrightness(-0.3f));
                            e.Graphics.DrawLine(penLine, rectSelectedTab.Left, rectSelectedTab.Top + 1, rectSelectedTab.Left, rectSelectedTab.Bottom - 1);
                        }
                    }
                    else
                    {
                        // Paint Main Control Border
                        ControlPaint.DrawBorder(e.Graphics, ClientRectangle, borderColor, ButtonBorderStyle.Solid);
                    }
                }
            }
        }

        private void PaintImageText(Graphics graphics, TabControl tc, TabPage tabPage, Rectangle rectTab, Image? tabImage, Font font, Color foreColor)
        {
            if (HideTabHeader)
                return;
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
