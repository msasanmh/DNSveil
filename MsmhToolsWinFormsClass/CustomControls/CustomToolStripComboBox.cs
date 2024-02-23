using System.ComponentModel;
using System.Windows.Forms.Design;
/*
 * Copyright MSasanMH, April 16, 2022.
 * Needs CustomComboBox.
 */

namespace CustomControls
{
    public delegate void DrawControlEventHandler(object sender, Graphics g, Rectangle drawRect);

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
    [ToolboxBitmap(typeof(CustomToolStripComboBox), "")]
    public class CustomToolStripComboBox : ToolStripControlHost
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
                    ComboBox.BackColor = BackColor;
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
                    ComboBox.ForeColor = ForeColor;
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
                    ComboBox.BorderColor = BorderColor;
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
                    ComboBox.SelectionColor = SelectionColor;
                    Invalidate();
                }
            }
        }

        public event EventHandler? DropDown;
        public event EventHandler? DropDownClosed;
        public event EventHandler? SelectionIndexChanged;

        public CustomToolStripComboBox() : base(new CustomComboBox())
        {
            ComboBox.DropDown += new EventHandler(ComboBox_DropDown);
            ComboBox.DropDownClosed += new EventHandler(ComboBox_DropDownClosed);
            ComboBox.SelectedIndexChanged += new EventHandler(ComboBox_SelectedIndexChanged);
            Paint += CustomToolStripComboBox_Paint;
        }

        private void CustomToolStripComboBox_Paint(object? sender, PaintEventArgs e)
        {
            ComboBox.BackColor = BackColor;
            ComboBox.ForeColor = ForeColor;
            ComboBox.BorderColor = BorderColor;
            ComboBox.SelectionColor = SelectionColor;
        }

        private void ComboBox_DropDown(object? sender, EventArgs e)
        {
            DropDown?.Invoke(sender, e);
        }

        private void ComboBox_DropDownClosed(object? sender, EventArgs e)
        {
            DropDownClosed?.Invoke(sender, e);
        }

        private void ComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            SelectionIndexChanged?.Invoke(sender, e);
        }

        public CustomComboBox ComboBox
        {
            get { return (CustomComboBox)Control; }
        }

        public ComboBox.ObjectCollection Items
        {
            get
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                return ccb.Items;
            }
        }

        public AutoCompleteSource AutoCompleteSource
        {
            get
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                return ccb.AutoCompleteSource;
            }
            set
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                ccb.AutoCompleteSource = value;
            }
        }

        public AutoCompleteMode AutoCompleteMode
        {
            get
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                return ccb.AutoCompleteMode;
            }
            set
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                ccb.AutoCompleteMode = value;
            }
        }

        public AutoCompleteStringCollection AutoCompleteCustomSource
        {
            get
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                return ccb.AutoCompleteCustomSource;
            }
            set
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                ccb.AutoCompleteCustomSource = value;
            }
        }

        public int DropDownHeight
        {
            get
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                return ccb.DropDownHeight;
            }
            set
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                ccb.DropDownHeight = value;
            }
        }

        public ComboBoxStyle DropDownStyle
        {
            get
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                return ccb.DropDownStyle;
            }
            set
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                ccb.DropDownStyle = value;
            }
        }

        public int DropDownWidth
        {
            get
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                return ccb.DropDownWidth;
            }
            set
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                ccb.DropDownWidth = value;
            }
        }

        public FlatStyle FlatStyle
        {
            get
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                return ccb.FlatStyle;
            }
            set
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                ccb.FlatStyle = value;
            }
        }

        public bool IntegralHeight
        {
            get
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                return ccb.IntegralHeight;
            }
            set
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                ccb.IntegralHeight = value;
            }
        }

        public int MaxDropDownItems
        {
            get
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                return ccb.MaxDropDownItems;
            }
            set
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                ccb.MaxDropDownItems = value;
            }
        }

        public int SelectedIndex
        {
            get
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                return ccb.SelectedIndex;
            }
            set
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                ccb.SelectedIndex = value;
            }
        }

        public object SelectedItem
        {
            get
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                return ccb.SelectedItem;
            }
            set
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                ccb.SelectedItem = value;
            }
        }

        public string SelectedText
        {
            get
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                return ccb.SelectedText;
            }
            set
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                ccb.SelectedText = value;
            }
        }

        public int SelectionLength
        {
            get
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                return ccb.SelectionLength;
            }
            set
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                ccb.SelectionLength = value;
            }
        }

        public int SelectionStart
        {
            get
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                return ccb.SelectionStart;
            }
            set
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                ccb.SelectionStart = value;
            }
        }

        public bool Sorted
        {
            get
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                return ccb.Sorted;
            }
            set
            {
                CustomComboBox ccb = (CustomComboBox)Control;
                ccb.Sorted = value;
            }
        }
    }
}
