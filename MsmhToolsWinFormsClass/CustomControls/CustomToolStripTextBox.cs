using System;
using System.ComponentModel;
using System.Windows.Forms.Design;
/*
* Copyright MSasanMH, June 20, 2022.
* Needs CustomTextBox.
*/

namespace CustomControls
{
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
    [ToolboxBitmap(typeof(CustomToolStripComboBox), "")]
    public class CustomToolStripTextBox : ToolStripControlHost
    {
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
                    TextBox.BackColor = BackColor;
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
                    TextBox.ForeColor = ForeColor;
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
                    TextBox.BorderColor = BorderColor;
                    Invalidate();
                }
            }
        }

        public new event EventHandler? EnabledChanged;
        public event EventHandler? MultilineChanged;
        public new event EventHandler? RightToLeftChanged;
        public event EventHandler? TextAlignChanged;
        public new event EventHandler? TextChanged;

        public CustomToolStripTextBox() : base(new CustomTextBox())
        {
            TextBox.TextChanged += TextBox_TextChanged;
            TextBox.EnabledChanged += TextBox_EnabledChanged;
            TextBox.MultilineChanged += TextBox_MultilineChanged;
            TextBox.RightToLeftChanged += TextBox_RightToLeftChanged;
            TextBox.TextAlignChanged += TextBox_TextAlignChanged;
        }

        private void TextBox_TextChanged(object? sender, EventArgs e)
        {
            TextChanged?.Invoke(sender, e);
        }

        private void TextBox_EnabledChanged(object? sender, EventArgs e)
        {
            EnabledChanged?.Invoke(sender, e);
        }

        private void TextBox_MultilineChanged(object? sender, EventArgs e)
        {
            MultilineChanged?.Invoke(sender, e);
        }

        private void TextBox_RightToLeftChanged(object? sender, EventArgs e)
        {
            RightToLeftChanged?.Invoke(sender, e);
        }

        private void TextBox_TextAlignChanged(object? sender, EventArgs e)
        {
            TextAlignChanged?.Invoke(sender, e);
        }

        public CustomTextBox TextBox
        {
            get { return (CustomTextBox)Control; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Right To Left Mode")]
        public override RightToLeft RightToLeft
        {
            get
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                return ctb.RightToLeft;
            }
            set
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                ctb.RightToLeft = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("ScrollBar")]
        public ScrollBars ScrollBars
        {
            get
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                return ctb.ScrollBars;
            }
            set
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                ctb.ScrollBars = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Text")]
        public override string Text
        {
            get
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                return ctb.Text;
            }
            set
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                ctb.Text = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Text Align")]
        public new HorizontalAlignment TextAlign
        {
            get
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                return ctb.TextAlign;
            }
            set
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                ctb.TextAlign = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Underlined Style Border")]
        public bool UnderlinedStyle
        {
            get
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                return ctb.UnderlinedStyle;
            }
            set
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                ctb.UnderlinedStyle = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Use Wait Cursor")]
        public bool UseWaitCursor
        {
            get
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                return ctb.UseWaitCursor;
            }
            set
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                ctb.UseWaitCursor = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Accepts Return")]
        public bool AcceptsReturn
        {
            get
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                return ctb.AcceptsReturn;
            }
            set
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                ctb.AcceptsReturn = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Accepts Tab")]
        public bool AcceptsTab
        {
            get
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                return ctb.AcceptsTab;
            }
            set
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                ctb.AcceptsTab = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Allow Drop")]
        public override bool AllowDrop
        {
            get
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                return ctb.AllowDrop;
            }
            set
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                ctb.AllowDrop = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Character Casing")]
        public CharacterCasing CharacterCasing
        {
            get
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                return ctb.CharacterCasing;
            }
            set
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                ctb.CharacterCasing = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Enable Or Disable Control")]
        public override bool Enabled
        {
            get
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                return ctb.Enabled;
            }
            set
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                ctb.Enabled = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Hide Selection")]
        public bool HideSelection
        {
            get
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                return ctb.HideSelection;
            }
            set
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                ctb.HideSelection = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Max Length")]
        public int MaxLength
        {
            get
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                return ctb.MaxLength;
            }
            set
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                ctb.MaxLength = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Multiline Style")]
        public bool Multiline
        {
            get
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                return ctb.Multiline;
            }
            set
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                ctb.Multiline = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Read Only")]
        public bool ReadOnly
        {
            get
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                return ctb.ReadOnly;
            }
            set
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                ctb.ReadOnly = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Password Style")]
        public bool UsePasswordChar
        {
            get
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                return ctb.UsePasswordChar;
            }
            set
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                ctb.UsePasswordChar = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Visible")]
        public new bool Visible
        {
            get
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                return ctb.Visible;
            }
            set
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                ctb.Visible = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Word Wrap Mode")]
        public bool WordWrap
        {
            get
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                return ctb.WordWrap;
            }
            set
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                ctb.WordWrap = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Layout"), Description("Auto Size")]
        public new bool AutoSize
        {
            get
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                return ctb.AutoSize;
            }
            set
            {
                CustomTextBox ctb = (CustomTextBox)Control;
                ctb.AutoSize = value;
            }
        }

    }
}
