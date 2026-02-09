using MsmhToolsClass;
using MsmhToolsWinFormsClass;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Windows.Forms.Design;

namespace CustomControls
{
    public class CustomDataGridView : DataGridView
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Color BackgroundColor { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool EnableHeadersVisualStyles { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new DataGridViewHeaderBorderStyle ColumnHeadersBorderStyle { get; set; }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Column Headers Border")]
        public bool ColumnHeadersBorder { get; set; }

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

        private Color mGridColor = Color.LightBlue;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Editor(typeof(WindowsFormsComponentEditor), typeof(Color))]
        [Category("Appearance"), Description("Grid Lines Color")]
        public new Color GridColor
        {
            get { return mGridColor; }
            set
            {
                if (mGridColor != value)
                {
                    mGridColor = value;
                    Invalidate();
                }
            }
        }

        private Color mCheckColor = Color.Blue;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Editor(typeof(WindowsFormsComponentEditor), typeof(Color))]
        [Category("Appearance"), Description("Check Color for CheckBox Cell Type")]
        public Color CheckColor
        {
            get { return mCheckColor; }
            set
            {
                if (mCheckColor != value)
                {
                    mCheckColor = value;
                    Invalidate();
                }
            }
        }

        private bool mSelectionModeFocus = false;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Set Focus On Full Row\nWorks on SelectionMode = FullRowSelect")]
        public bool SelectionModeFocus
        {
            get { return mSelectionModeFocus; }
            set
            {
                if (mSelectionModeFocus != value)
                {
                    mSelectionModeFocus = value;
                    Invalidate();
                }
            }
        }

        private readonly string LabelScreen = "MSasanMH";
        private Color BackColorDarker { get; set; }
        public Color SelectionUnfocused { get; private set; }

        private bool ApplicationIdle = false;
        private bool once = true;

        public CustomDataGridView() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Opaque, true);

            BorderStyle = BorderStyle.FixedSingle;
            EnableHeadersVisualStyles = false;
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            
            ColumnHeadersBorder = true;
            Application.Idle += Application_Idle;
            RowsAdded += CustomDataGridView_RowsAdded;
            GotFocus += CustomDataGridView_GotFocus;
            LostFocus += CustomDataGridView_LostFocus;
            HandleCreated += DataGridView_HandleCreated;
            Invalidated += CustomDataGridView_Invalidated;
            MouseEnter += CustomDataGridView_MouseEnter;
            MouseUp += CustomDataGridView_MouseUp;
            MouseDown += CustomDataGridView_MouseDown;
            MouseMove += CustomDataGridView_MouseMove;
            LocationChanged += CustomDataGridView_LocationChanged;
            Move += CustomDataGridView_Move;
            Scroll += DataGridView_Scroll;
            GotFocus += DataGridView_GotFocus;
            LostFocus += DataGridView_LostFocus;
            EnabledChanged += DataGridView_EnabledChanged;
            CellClick -= DataGridView_CellClick;
            CellClick += DataGridView_CellClick;
            CellPainting += DataGridView_CellPainting;
            Paint += DataGridView_Paint;
            EditingControlShowing += CustomDataGridView_EditingControlShowing;
            
            KeyDown += CustomDataGridView_KeyDown;
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

        private void CustomDataGridView_RowsAdded(object? sender, DataGridViewRowsAddedEventArgs e)
        {
            if (sender is CustomDataGridView gv)
                DataGridViewColor(gv);
        }

        private void CustomDataGridView_GotFocus(object? sender, EventArgs e)
        {
            if (sender is CustomDataGridView gv)
                DataGridViewColor(gv);
        }

        private void CustomDataGridView_LostFocus(object? sender, EventArgs e)
        {
            if (sender is CustomDataGridView gv)
                DataGridViewColor(gv);
        }

        private void DataGridView_HandleCreated(object? sender, EventArgs e)
        {
            if (sender is null || e is null) return;
            BackColorDarker = mBackColor.ChangeBrightness(-0.3f);
            SelectionUnfocused = mSelectionColor.ChangeBrightness(0.3f);
            if (sender is DataGridView gv)
            {
                DataGridViewColor(gv);
                gv.Invalidate();
            }
        }

        private void CustomDataGridView_Invalidated(object? sender, InvalidateEventArgs e)
        {
            if (BackColor.DarkOrLight() == "Dark")
                this.SetDarkControl();
        }

        private void CustomDataGridView_MouseEnter(object? sender, EventArgs e)
        {
            if (sender is DataGridView gv) gv.Invalidate();
        }

        private void CustomDataGridView_MouseUp(object? sender, MouseEventArgs e)
        {
            if (sender is DataGridView gv) gv.Invalidate();
        }

        private void CustomDataGridView_MouseDown(object? sender, MouseEventArgs e)
        {
            if (sender is DataGridView gv) gv.Invalidate();
        }

        private void CustomDataGridView_MouseMove(object? sender, MouseEventArgs e)
        {
            if (sender is DataGridView gv) gv.Invalidate();
        }

        private void CustomDataGridView_LocationChanged(object? sender, EventArgs e)
        {
            if (sender is DataGridView gv) gv.Invalidate();
        }

        private void CustomDataGridView_Move(object? sender, EventArgs e)
        {
            if (sender is DataGridView gv) gv.Invalidate();
        }

        private void DataGridView_Scroll(object? sender, ScrollEventArgs e)
        {
            if (sender is DataGridView gv) gv.Invalidate();
        }

        private void DataGridView_GotFocus(object? sender, EventArgs e)
        {
            if (sender is DataGridView gv) gv.Invalidate();
        }

        private void DataGridView_LostFocus(object? sender, EventArgs e)
        {
            if (sender is DataGridView gv) gv.Invalidate();
        }

        private void DataGridView_EnabledChanged(object? sender, EventArgs e)
        {
            if (sender is DataGridView gv) gv.Invalidate();
        }

        private void DataGridViewColor(DataGridView? gv)
        {
            if (gv == null) return;

            // Update Colors
            BackColorDarker = GetBackColor().ChangeBrightness(-0.3f);
            SelectionUnfocused = GetSelectionColor().ChangeBrightness(0.3f);
            //OriginalColors = new Color[] { BackColor, ForeColor, BorderColor, SelectionColor, GridColor, CheckColor, BackColorDarker, SelectionUnfocused };
            
            Color backColor = GetBackColor();
            Color foreColor = GetForeColor();
            Color gridColor = GetGridColor();
            Color backColorDarker = BackColorDarker;

            gv.BackgroundColor = backColor;
            gv.GridColor = gridColor;
            gv.ColumnHeadersDefaultCellStyle.BackColor = backColorDarker;
            gv.ColumnHeadersDefaultCellStyle.SelectionBackColor = backColorDarker;
            gv.ColumnHeadersDefaultCellStyle.ForeColor = foreColor;
            gv.ColumnHeadersDefaultCellStyle.SelectionForeColor = foreColor;
            gv.DefaultCellStyle.BackColor = backColor;
            gv.DefaultCellStyle.ForeColor = foreColor;
            if (gv.Focused)
                gv.DefaultCellStyle.SelectionBackColor = SelectionColor;
            else
                gv.DefaultCellStyle.SelectionBackColor = SelectionUnfocused;
            gv.DefaultCellStyle.SelectionForeColor = foreColor;
            gv.EnableHeadersVisualStyles = false;
            if (ColumnHeadersBorder)
                gv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            else
                gv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        }
        private static void DataGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (sender is not DataGridView gv) return;
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var cell = gv[e.ColumnIndex, e.RowIndex];
                if (cell.Value == DBNull.Value) return;

                // DataGridViewCheckBoxCell
                if (cell.GetType().ToString().Contains("CheckBox", StringComparison.OrdinalIgnoreCase))
                {
                    if (cell is DataGridViewCheckBoxCell checkBox)
                    {
                        if (cell.GetType().ToString().Contains("CheckBox", StringComparison.OrdinalIgnoreCase))
                        {
                            if (checkBox.ThreeState)
                            {
                                if (cell.Value is null or "null")
                                {
                                    cell.Value = false;
                                    gv.UpdateCellValue(e.ColumnIndex, e.RowIndex);
                                    gv.EndEdit();
                                    gv.InvalidateCell(cell);
                                }
                                else
                                {
                                    bool cellValue = Convert.ToBoolean(cell.Value);
                                    if (cellValue == true)
                                    {
                                        cell.Value = "null";
                                        gv.UpdateCellValue(e.ColumnIndex, e.RowIndex);
                                        gv.EndEdit();
                                        gv.InvalidateCell(cell);
                                    }
                                    else
                                    {
                                        cell.Value = true;
                                        gv.UpdateCellValue(e.ColumnIndex, e.RowIndex);
                                        gv.EndEdit();
                                        gv.InvalidateCell(cell);
                                    }
                                }
                            }
                            else
                            {
                                cell.Value ??= false;

                                if (cell.Value is not null)
                                {
                                    bool cellValue = Convert.ToBoolean(cell.Value);
                                    if (cellValue == true)
                                    {
                                        cell.Value = false;
                                        gv.UpdateCellValue(e.ColumnIndex, e.RowIndex);
                                        gv.EndEdit();
                                        gv.InvalidateCell(cell);
                                    }
                                    else
                                    {
                                        cell.Value = true;
                                        gv.UpdateCellValue(e.ColumnIndex, e.RowIndex);
                                        gv.EndEdit();
                                        gv.InvalidateCell(cell);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (cell.Value is null or DBNull)
                        cell.Value = string.Empty;
                }

                //// DataGridViewComboBoxCell
                //if (cell.GetType().ToString().Contains("ComboBox", StringComparison.OrdinalIgnoreCase))
                //{

                //}

            }
        }
        
        private void DataGridView_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (sender is not DataGridView gv) return;
            if (!Visible) return;

            // Update Colors
            DataGridViewColor(gv);

            Color backColor = GetBackColor();
            Color foreColor = GetForeColor();
            Color borderColor = GetBorderColor();
            Color gridColor = GetGridColor();
            Color checkColor = GetCheckColor();
            Color backColorDarker = BackColorDarker;

            if (ApplicationIdle == false) return;

            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridViewCell cell = gv[e.ColumnIndex, e.RowIndex];

                // DataGridViewCheckBoxCell
                if (cell.GetType().ToString().Contains("CheckBox", StringComparison.OrdinalIgnoreCase))
                {
                    if (cell.Value is DBNull) cell.Value = false;

                    Rectangle rectCell = gv.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                    if (cell is not DataGridViewCheckBoxCell checkBox) return;
                    
                    e.Handled = true;
                    e.PaintBackground(rectCell, true);

                    rectCell = new(rectCell.X, rectCell.Y, rectCell.Width - 2, rectCell.Height - 2);
                    
                    // Measure Text Height Based On Font
                    SizeF labelScreenSizeF = TextRenderer.MeasureText(LabelScreen, Font);
                    int checkSize = Convert.ToInt32(labelScreenSizeF.Height - 2);
                    checkSize = Math.Min(checkSize, rectCell.Width);
                    checkSize = Math.Min(checkSize, rectCell.Height);
                    int centerX = rectCell.X + rectCell.Width / 2 - checkSize / 2;
                    int centerY = rectCell.Y + rectCell.Height / 2 - checkSize / 2;

                    rectCell = new(centerX, centerY, checkSize, checkSize);

                    // Fill Check Rect
                    using SolidBrush brush1 = new(backColor);
                    e.Graphics.FillRectangle(brush1, rectCell);

                    // Draw Check
                    if (cell.Value != DBNull.Value)
                    {
                        int rectCheck = rectCell.Height - 1;
                        if (rectCheck <= 0) rectCheck = 1;

                        if (rectCheck > 1)
                        {
                            if (cell.Value is null or "null")
                            {
                                if (checkBox.ThreeState)
                                {
                                    // Draw Indeterminate
                                    using SolidBrush sb = new(checkColor);
                                    rectCell.Inflate(-2, -2);
                                    e.Graphics.FillRectangle(sb, rectCell);
                                    rectCell.Inflate(+2, +2);
                                }
                            }
                            else
                            {
                                cell.Value ??= false;

                                if (cell.Value is not null)
                                {
                                    if (Convert.ToBoolean(cell.Value) == true)
                                    {
                                        // Draw Check
                                        using var p = new Pen(checkColor, 2);
                                        rectCell.Inflate(-2, -2);
                                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                                        e.Graphics.DrawLines(p, new Point[] { new Point(rectCell.Left, rectCell.Bottom - rectCell.Height / 2), new Point(rectCell.Left + rectCell.Width / 3, rectCell.Bottom), new Point(rectCell.Right, rectCell.Top) });
                                        e.Graphics.SmoothingMode = SmoothingMode.Default;
                                        rectCell.Inflate(+2, +2);
                                    }
                                }
                            }
                        }
                    }

                    // Draw Check Rect (Check Border)
                    ControlPaint.DrawBorder(e.Graphics, rectCell, borderColor, ButtonBorderStyle.Solid);
                }

                // DataGridViewComboBoxCell
                if (cell.GetType().ToString().Contains("ComboBox", StringComparison.OrdinalIgnoreCase))
                {
                    Rectangle rectCell = gv.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                    if (cell is not DataGridViewComboBoxCell comboBox) return;

                    int myPadding = 10;
                    e.Handled = true;
                    e.PaintBackground(rectCell, true);
                    rectCell = new(rectCell.X, rectCell.Y, rectCell.Width - 1, rectCell.Height - 1);

                    // Fill Background
                    using SolidBrush sb = new(backColor);
                    e.Graphics.FillRectangle(sb, rectCell);
                    
                    // Draw Border
                    using Pen p = new(gridColor, 1);
                    Rectangle modRect1 = new(rectCell.Left, rectCell.Top, rectCell.Width - 1, rectCell.Height - 1);
                    e.Graphics.DrawRectangle(p, modRect1);

                    // Fill Arrow Button Back Color
                    using SolidBrush sb2 = new(backColorDarker);
                    rectCell = new(rectCell.X + 1, rectCell.Y, rectCell.Width - 1, rectCell.Height);
                    int x = (rectCell.X + rectCell.Width) - 15;
                    int y = rectCell.Y + 1;
                    int buttonWidth = (rectCell.X + rectCell.Width) - x - 1;
                    int buttonHeight = rectCell.Height - 2;
                    Rectangle modRect2 = new(x, y, buttonWidth, buttonHeight);
                    e.Graphics.FillRectangle(sb2, modRect2);

                    // Draw Arrow Button Icon
                    var pth = new GraphicsPath();
                    var TopLeft = new PointF(x + buttonWidth * 1 / 5, y + buttonHeight * 2 / 5);
                    var TopRight = new PointF(x + buttonWidth * 4 / 5, y + buttonHeight * 2 / 5);
                    var Bottom = new PointF(x + buttonWidth / 2, y + buttonHeight * 3 / 5);
                    pth.AddLine(TopLeft, TopRight);
                    pth.AddLine(TopRight, Bottom);
                    // Determine the Arrow's Color.
                    using SolidBrush arrowBrush = new(foreColor);
                    // Draw the Arrow
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(arrowBrush, pth);
                    e.Graphics.SmoothingMode = SmoothingMode.Default;

                    var text = comboBox.Value != null ? comboBox.Value.ToString() : Text;

                    using SolidBrush b = new(foreColor);
                    int padding = 2;
                    int arrowWidth = (int)(TopRight.X - TopLeft.X);
                    Rectangle modRect3 = new(rectCell.Left + padding,
                                                rectCell.Top + padding,
                                                rectCell.Width - arrowWidth - (myPadding / 2) - (padding * 2),
                                                rectCell.Height - (padding * 2));

                    var stringFormat = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = StringAlignment.Near,
                        FormatFlags = StringFormatFlags.NoWrap,
                        Trimming = StringTrimming.EllipsisCharacter
                    };
                    e.Graphics.DrawString(text, Font, b, modRect3, stringFormat);
                }

            }
        }

        private void DataGridView_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not DataGridView gv) return;
            if (!Visible) return;

            // Update Colors
            DataGridViewColor(gv);

            if (ApplicationIdle == false) return;

            Rectangle rectGv = new(0, 0, ClientSize.Width, ClientSize.Height);

            Color borderColor = GetBorderColor();
            using Pen pen = new(borderColor);

            if (BorderStyle == BorderStyle.FixedSingle)
            {
                ControlPaint.DrawBorder(e.Graphics, rectGv, borderColor, ButtonBorderStyle.Solid);
            }
            else if (BorderStyle == BorderStyle.Fixed3D)
            {
                Color secondBorderColor;
                if (borderColor.DarkOrLight() == "Dark")
                    secondBorderColor = borderColor.ChangeBrightness(0.6f);
                else
                    secondBorderColor = borderColor.ChangeBrightness(-0.6f);

                Rectangle rect3D = rectGv;
                ControlPaint.DrawBorder(e.Graphics, rect3D, secondBorderColor, ButtonBorderStyle.Solid);

                rect3D = new(rectGv.X + 1, rectGv.Y + 1, rectGv.Width - 1, rectGv.Height - 1);
                ControlPaint.DrawBorder(e.Graphics, rect3D, secondBorderColor, ButtonBorderStyle.Solid);

                rect3D = new(rectGv.X, rectGv.Y, rectGv.Width - 1, rectGv.Height - 1);
                ControlPaint.DrawBorder(e.Graphics, rect3D, borderColor, ButtonBorderStyle.Solid);
            }
        }

        private void CustomDataGridView_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
        {
            //if (sender is CheckBox checkBox)
            //{
            //    if (checkBox.CheckState == CheckState.Indeterminate)

            //}
        }

        private void CustomDataGridView_KeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is not DataGridView dgv) return;
            if (SelectionModeFocus && SelectionMode == DataGridViewSelectionMode.FullRowSelect)
            {
                if (dgv.Rows.Count == 0) return;
                int currentRow = dgv.CurrentCell.RowIndex;

                if (e.KeyCode == Keys.Tab)
                {
                    if (currentRow + 1 < dgv.Rows.Count && dgv.Rows[currentRow].Cells.Count > 0)
                    {
                        e.SuppressKeyPress = true;

                        currentRow++;
                        dgv.Rows[currentRow].Cells[0].Selected = true;
                        dgv.Rows[currentRow].Selected = true;
                    }
                    else
                    {
                        Control ctl;
                        ctl = (Control)sender;
                        ctl.SelectNextControl(GetNextControl(ctl, true), true, false, true, true);
                    }
                }

                if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Right || e.KeyCode == Keys.Left)
                {
                    if (dgv.Rows[currentRow].Cells.Count > 0)
                    {
                        e.SuppressKeyPress = true;
                        int cellN = 0;
                        for (int n = 0; n < dgv.Rows[currentRow].Cells.Count; n++)
                        {
                            var cell = dgv.Rows[currentRow].Cells[n];
                            if (cell.GetType().ToString().Contains("CheckBox", StringComparison.OrdinalIgnoreCase))
                            {
                                cellN = n;
                                cell.Selected = true;
                                dgv.Rows[currentRow].Selected = true;
                            }
                        }
                        dgv.Rows[currentRow].Cells[cellN].Selected = true;
                        dgv.Rows[currentRow].Selected = true;
                    }
                }

                
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

        private Color GetSelectionColor()
        {
            if (Enabled)
                return SelectionColor;
            else
            {
                if (SelectionColor.DarkOrLight() == "Dark")
                    return SelectionColor.ChangeBrightness(0.3f);
                else
                    return SelectionColor.ChangeBrightness(-0.3f);
            }
        }

        private Color GetGridColor()
        {
            if (Enabled)
                return GridColor;
            else
            {
                if (GridColor.DarkOrLight() == "Dark")
                    return GridColor.ChangeBrightness(0.3f);
                else
                    return GridColor.ChangeBrightness(-0.3f);
            }
        }

        private Color GetCheckColor()
        {
            if (Enabled)
                return CheckColor;
            else
            {
                if (CheckColor.DarkOrLight() == "Dark")
                    return CheckColor.ChangeBrightness(0.3f);
                else
                    return CheckColor.ChangeBrightness(-0.3f);
            }
        }

    }
}
