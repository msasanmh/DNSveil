using MsmhTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
/*
* Copyright MSasanMH, June 20, 2022.
*/

namespace CustomControls
{
    public class CustomStatusStrip : StatusStrip
    {
        private readonly CustomToolStripRenderer MyRenderer = new();

        private bool mBorder = false;
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
                    BorderColorChanged?.Invoke(this, EventArgs.Empty);
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
                    SelectionColorChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Border Color Changed Event")]
        public event EventHandler? BorderColorChanged;

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Selection Color Changed Event")]
        public event EventHandler? SelectionColorChanged;

        public CustomStatusStrip() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            // Default
            BackColor = Color.DimGray;
            ForeColor = Color.White;

            MyRenderer.BackColor = GetBackColor();
            MyRenderer.ForeColor = GetForeColor();
            MyRenderer.BorderColor = GetBorderColor();
            MyRenderer.SelectionColor = SelectionColor;
            Renderer = MyRenderer;

            BackColorChanged += CustomStatusStrip_BackColorChanged;
            ForeColorChanged += CustomStatusStrip_ForeColorChanged;
            BorderColorChanged += CustomStatusStrip_BorderColorChanged;
            SelectionColorChanged += CustomStatusStrip_SelectionColorChanged;
            Paint += CustomStatusStrip_Paint;
        }

        private void CustomStatusStrip_BackColorChanged(object? sender, EventArgs e)
        {
            MyRenderer.BackColor = GetBackColor();
            Invalidate();
        }

        private void CustomStatusStrip_ForeColorChanged(object? sender, EventArgs e)
        {
            MyRenderer.ForeColor = GetForeColor();
            Invalidate();
        }

        private void CustomStatusStrip_BorderColorChanged(object? sender, EventArgs e)
        {
            MyRenderer.BorderColor = GetBorderColor();
            Invalidate();
        }

        private void CustomStatusStrip_SelectionColorChanged(object? sender, EventArgs e)
        {
            MyRenderer.SelectionColor = SelectionColor;
            Invalidate();
        }

        private void CustomStatusStrip_Paint(object? sender, PaintEventArgs e)
        {
            if (Border)
            {
                Color borderColor = GetBorderColor();
                ControlPaint.DrawBorder(e.Graphics, ClientRectangle, borderColor, ButtonBorderStyle.Solid);
            }
        }

        private Color GetBackColor()
        {
            if (Enabled)
                return BackColor;
            else
            {
                if (BackColor.DarkOrLight() == "Dark")
                    return BackColor.ChangeBrightness(0.2f);
                else
                    return BackColor.ChangeBrightness(-0.2f);
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
