using MsmhToolsClass;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.InteropServices;
using System.Windows.Forms.Design;
/*
* Copyright MSasanMH, May 10, 2022.
*/

namespace CustomControls
{
    [DefaultEvent("TextChanged")]
    public class CustomTextBox : UserControl
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

        // Disable
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Padding Padding { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new AutoSizeMode AutoSizeMode { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new AutoValidate AutoValidate { get; set; }

        private readonly TextBox textBox = new();
        private bool isFocused = false;
        public new bool Focused
        {
            get { return isFocused; }
            private set
            {
                if (isFocused != value)
                {
                    isFocused = value;
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

        private bool mBorder = true;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Border")]
        public bool Border
        {
            get { return mBorder; }
            set
            {
                if (mBorder != value)
                {
                    mBorder = value;
                    Invalidate();
                }
            }
        }

        private int mBorderSize = 1;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Border Size")]
        public int BorderSize
        {
            get { return mBorderSize; }
            set
            {
                if (mBorderSize != value)
                {
                    mBorderSize = value;
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

        [Category("Appearance"), Description("Font")]
        public override Font Font
        {
            get { return base.Font; }
            set
            {
                base.Font = value;
                textBox.Font = value;
                if (DesignMode)
                    UpdateControlSize();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("ScrollBar")]
        public ScrollBars ScrollBars
        {
            get { return textBox.ScrollBars; }
            set { textBox.ScrollBars = value; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get { return textBox.Text; }
            set
            {
                base.Text = value;
                textBox.Text = value;
            }
        }

        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0," +
            "Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Texts")]
        public string Texts
        {
            get { return textBox.Text; }
            set { textBox.Text = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Text Align")]
        public HorizontalAlignment TextAlign
        {
            get { return textBox.TextAlign; }
            set { textBox.TextAlign = value; }
        }

        private bool mUnderlinedStyle = false;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Border Underlined Style")]
        public bool UnderlinedStyle
        {
            get { return mUnderlinedStyle; }
            set
            {
                if (mUnderlinedStyle != value)
                {
                    mUnderlinedStyle = value;
                    Invalidate();
                }
            }
        }

        // Behavior
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Accepts Return")]
        public bool AcceptsReturn
        {
            get { return textBox.AcceptsReturn; }
            set { textBox.AcceptsReturn = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Accepts Tab")]
        public bool AcceptsTab
        {
            get { return textBox.AcceptsTab; }
            set { textBox.AcceptsTab = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Allow Drop")]
        public override bool AllowDrop
        {
            get { return textBox.AllowDrop; }
            set { textBox.AllowDrop = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Character Casing")]
        public CharacterCasing CharacterCasing
        {
            get { return textBox.CharacterCasing; }
            set { textBox.CharacterCasing = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Context Menu Strip")]
        public override ContextMenuStrip ContextMenuStrip
        {
            get { return textBox.ContextMenuStrip; }
            set { textBox.ContextMenuStrip = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Hide Selection")]
        public bool HideSelection
        {
            get { return textBox.HideSelection; }
            set { textBox.HideSelection = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Input Method Editor")]
        public new ImeMode ImeMode
        {
            get { return textBox.ImeMode; }
            set { textBox.ImeMode = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Max Length")]
        public int MaxLength
        {
            get { return textBox.MaxLength; }
            set { textBox.MaxLength = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Multiline Style")]
        public bool Multiline
        {
            get { return textBox.Multiline; }
            set
            {
                textBox.Multiline = value;
                if (DesignMode)
                    UpdateControlSize();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Read Only")]
        public bool ReadOnly
        {
            get { return textBox.ReadOnly; }
            set { textBox.ReadOnly = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Shortcuts Enabled")]
        public bool ShortcutsEnabled
        {
            get { return textBox.ShortcutsEnabled; }
            set { textBox.ShortcutsEnabled = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Tab Index")]
        public new int TabIndex
        {
            get { return textBox.TabIndex; }
            set { textBox.TabIndex = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Tab Stop")]
        public new bool TabStop
        {
            get { return textBox.TabStop; }
            set { textBox.TabStop = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Password Style")]
        public bool UsePasswordChar
        {
            get { return textBox.UseSystemPasswordChar; }
            set { textBox.UseSystemPasswordChar = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Word Wrap Style")]
        public bool WordWrap
        {
            get { return textBox.WordWrap; }
            set { textBox.WordWrap = value; }
        }

        private bool ApplicationIdle = false;
        private bool once = true;

        public CustomTextBox() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            Font = new("Segoe UI", 9);
            AutoScaleMode = AutoScaleMode.None;
            Padding = new(0);
            Size = new(100, 23);

            // Default
            BackColor = Color.DimGray;
            ForeColor = Color.White;
            AutoScroll = false;
            AutoScrollMargin = new(0, 0);
            AutoScrollMinSize = new(0, 0);
            AutoSize = false;
            
            // Disabled
            AutoSizeMode = AutoSizeMode.GrowOnly;
            AutoValidate = AutoValidate.EnablePreventFocusChange;

            Controls.Add(textBox);
            textBox.BackColor = GetBackColor();
            textBox.ForeColor = GetForeColor();
            //textBox.Dock = DockStyle.Fill;
            textBox.BorderStyle = BorderStyle.None;

            // Events
            Application.Idle += Application_Idle;
            EnabledChanged += CustomTextBox_EnabledChanged;
            BackColorChanged += CustomTextBox_BackColorChanged;
            ForeColorChanged += CustomTextBox_ForeColorChanged;
            textBox.Click += TextBox_Click;
            textBox.KeyDown += TextBox_KeyDown;
            textBox.KeyPress += TextBox_KeyPress;
            textBox.KeyUp += TextBox_KeyUp;
            textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
            textBox.MouseDown += TextBox_MouseDown;
            textBox.MouseEnter += TextBox_MouseEnter;
            textBox.MouseHover += TextBox_MouseHover;
            textBox.MouseLeave += TextBox_MouseLeave;
            textBox.MouseMove += TextBox_MouseMove;
            textBox.MouseUp += TextBox_MouseUp;
            textBox.Enter += TextBox_Enter;
            textBox.Leave += TextBox_Leave;
            textBox.Invalidated += TextBox_Invalidated;
            textBox.AcceptsTabChanged += TextBox_AcceptsTabChanged;
            textBox.HideSelectionChanged += TextBox_HideSelectionChanged;
            textBox.ModifiedChanged += TextBox_ModifiedChanged;
            textBox.MultilineChanged += TextBox_MultilineChanged;
            textBox.ReadOnlyChanged += TextBox_ReadOnlyChanged;
            textBox.TextAlignChanged += TextBox_TextAlignChanged;
            textBox.TextChanged += TextBox_TextChanged;
            textBox.LostFocus += TextBox_LostFocus;
            textBox.GotFocus += TextBox_GotFocus;
        }

        // Events
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Mark as deprecated.", true)]
        public new event EventHandler? Scroll;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Mark as deprecated.", true)]
        public new event EventHandler? Load;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Mark as deprecated.", true)]
        public new event EventHandler? PaddingChanged;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Mark as deprecated.", true)]
        public new event EventHandler? AutoSizeChanged;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Mark as deprecated.", true)]
        public new event EventHandler? AutoValidateChanged;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Mark as deprecated.", true)]
        public new event EventHandler? BackgroundImageChanged;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Mark as deprecated.", true)]
        public new event EventHandler? BackgroundImageLayoutChanged;

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

        private void CustomTextBox_EnabledChanged(object? sender, EventArgs e)
        {
            textBox.Enabled = Enabled;
            Invalidate();
            textBox.Invalidate();
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Accepts Tab Changed")]
        public event EventHandler? AcceptsTabChanged;
        private void TextBox_AcceptsTabChanged(object? sender, EventArgs e)
        {
            AcceptsTabChanged?.Invoke(sender, e);
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Hide Selection Changed")]
        public event EventHandler? HideSelectionChanged;
        private void TextBox_HideSelectionChanged(object? sender, EventArgs e)
        {
            HideSelectionChanged?.Invoke(sender, e);
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Modified Changed")]
        public event EventHandler? ModifiedChanged;
        private void TextBox_ModifiedChanged(object? sender, EventArgs e)
        {
            ModifiedChanged?.Invoke(sender, e);
        }
        
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Multiline Changed")]
        public event EventHandler? MultilineChanged;
        private bool MultilineChangedBool = false;
        private void TextBox_MultilineChanged(object? sender, EventArgs e)
        {
            if (MultilineChanged != null && ApplicationIdle == true)
            {
                MultilineChanged.Invoke(sender, e);
                MultilineChangedBool = true;
                UpdateControlSize();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Read Only Changed")]
        public event EventHandler? ReadOnlyChanged;
        private void TextBox_ReadOnlyChanged(object? sender, EventArgs e)
        {
            ReadOnlyChanged?.Invoke(sender, e);
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Text Align Changed")]
        public event EventHandler? TextAlignChanged;
        private void TextBox_TextAlignChanged(object? sender, EventArgs e)
        {
            TextAlignChanged?.Invoke(sender, e);
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Text Changed")]
        public new event EventHandler? TextChanged;
        private void TextBox_TextChanged(object? sender, EventArgs e)
        {
            //OnTextChanged(e);
            TextChanged?.Invoke(sender, e);
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Lost Focus")]
        public new event EventHandler? LostFocus;
        private void TextBox_LostFocus(object? sender, EventArgs e)
        {
            //OnLostFocus(e);
            LostFocus?.Invoke(sender, e);
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Got Focus")]
        public new event EventHandler? GotFocus;
        private void TextBox_GotFocus(object? sender, EventArgs e)
        {
            //OnGotFocus(e);
            GotFocus?.Invoke(sender, e);
        }

        private void CustomTextBox_BackColorChanged(object? sender, EventArgs e)
        {
            textBox.BackColor = GetBackColor();
            Invalidate();
        }

        private void CustomTextBox_ForeColorChanged(object? sender, EventArgs e)
        {
            textBox.ForeColor = GetForeColor();
            Invalidate();
        }

        private void TextBox_Click(object? sender, EventArgs e)
        {
            OnClick(e);
        }

        private void TextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        private void TextBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            OnKeyPress(e);
        }

        private void TextBox_KeyUp(object? sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        private void TextBox_PreviewKeyDown(object? sender, PreviewKeyDownEventArgs e)
        {
            OnPreviewKeyDown(e);
        }

        private void TextBox_MouseDown(object? sender, MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        private void TextBox_MouseEnter(object? sender, EventArgs e)
        {
            OnMouseEnter(e);
        }

        private void TextBox_MouseHover(object? sender, EventArgs e)
        {
            OnMouseHover(e);
        }

        private void TextBox_MouseLeave(object? sender, EventArgs e)
        {
            OnMouseLeave(e);
        }

        private void TextBox_MouseMove(object? sender, MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        private void TextBox_MouseUp(object? sender, MouseEventArgs e)
        {
            OnMouseUp(e);
        }

        private void TextBox_Enter(object? sender, EventArgs e)
        {
            isFocused = true;
            OnEnter(e);
            Invalidate();
        }

        private void TextBox_Leave(object? sender, EventArgs e)
        {
            isFocused = false;
            OnLeave(e);
            Invalidate();
        }

        private void TextBox_Invalidated(object? sender, InvalidateEventArgs e)
        {
            OnInvalidated(e);
            if (BackColor.DarkOrLight() == "Dark")
                Methods.SetDarkControl(textBox);
            textBox.Enabled = Enabled;
            textBox.BackColor = GetBackColor();
            textBox.ForeColor = GetForeColor();
        }

        // Overridden Methods
        protected override void OnPaint(PaintEventArgs e)
        {
            if (!Visible) return;

            base.OnPaint(e);

            Color borderColor = GetBorderColor();

            e.Graphics.Clear(GetBackColor());

            //Draw border
            using Pen penBorder = new(borderColor, mBorderSize);
            penBorder.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;

            if (Border)
            {
                if (mUnderlinedStyle) // Line Style
                    e.Graphics.DrawLine(penBorder, 0, Height - 1, Width, Height - 1);
                else //Normal Style
                {
                    int r = RoundedCorners;
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.DrawRoundedRectangle(penBorder, new Rectangle(0, 0, Width - 1, Height - 1), r, r, r, r);
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
            UpdateControlSize();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UpdateControlSize();
        }

        private void UpdateControlSize()
        {
            if (textBox.Multiline == false)
            {
                int txtHeight = TextRenderer.MeasureText("Text", Font).Height + 2;
                int padding = 6;
                if (!MultilineChangedBool)
                {
                    textBox.Multiline = true;
                    textBox.MinimumSize = new Size(0, txtHeight);
                    textBox.Multiline = false;
                }
                else
                    MultilineChangedBool = false;
                textBox.Height = txtHeight;
                textBox.Width = Width - padding;
                Height = textBox.Height + padding;
                textBox.Location = new(padding / 2, padding / 2);
            }
            else
            {
                int txtHeight = TextRenderer.MeasureText("Text", Font).Height + 2;
                int padding = 6;
                textBox.MinimumSize = new Size(0, txtHeight);
                MinimumSize = new Size(0, txtHeight + padding);
                textBox.Height = Height - padding;
                textBox.Width = Width - padding;
                textBox.Location = new(padding / 2, padding / 2);
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
            {
                if (isFocused)
                {
                    // Focused Border Color
                    if (BorderColor.DarkOrLight() == "Dark")
                        return BorderColor.ChangeBrightness(0.4f);
                    else
                        return BorderColor.ChangeBrightness(-0.4f);
                }
                else
                    return BorderColor;
            }
            else
            {
                // Disabled Border Color
                if (BorderColor.DarkOrLight() == "Dark")
                    return BorderColor.ChangeBrightness(0.3f);
                else
                    return BorderColor.ChangeBrightness(-0.3f);
            }
        }

    }
}
