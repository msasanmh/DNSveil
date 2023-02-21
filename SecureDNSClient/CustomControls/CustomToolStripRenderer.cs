using MsmhTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
/*
* Copyright MSasanMH, June 20, 2022.
*/

namespace CustomControls
{
    public class CustomToolStripRenderer : ToolStripProfessionalRenderer
    {
        private Color mBackColor = Color.DimGray;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Editor(typeof(WindowsFormsComponentEditor), typeof(Color))]
        [Category("Appearance"), Description("Back Color")]
        public Color BackColor
        {
            get { return mBackColor; }
            set
            {
                if (mBackColor != value)
                {
                    mBackColor = value;
                }
            }
        }

        private Color mForeColor = Color.White;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Editor(typeof(WindowsFormsComponentEditor), typeof(Color))]
        [Category("Appearance"), Description("Fore Color")]
        public Color ForeColor
        {
            get { return mForeColor; }
            set
            {
                if (mForeColor != value)
                {
                    mForeColor = value;
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
                }
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            if (e.Item as ToolStripSeparator == null)
                base.OnRenderSeparator(e);
            else
            {
                var toolStripSeparator = e.Item as ToolStripSeparator;
                Rectangle rect = e.Item.Bounds;

                // Paint Background
                using SolidBrush bgBrush = new(toolStripSeparator.BackColor);
                e.Graphics.FillRectangle(bgBrush, rect);

                Color line2;
                if (toolStripSeparator.ForeColor.DarkOrLight() == "Dark")
                    line2 = ControlPaint.Light(toolStripSeparator.ForeColor);
                else
                    line2 = ControlPaint.Dark(toolStripSeparator.ForeColor);
                if (e.Vertical)
                {
                    // Paint Vertical Separator
                    Rectangle bounds = rect;
                    bounds.Y += 3;
                    bounds.Height = Math.Max(0, bounds.Height - 6);
                    if (bounds.Height >= 4) bounds.Inflate(0, -2);
                    int x = bounds.Width / 2;
                    using Pen pen1 = new(toolStripSeparator.ForeColor);
                    e.Graphics.DrawLine(pen1, x, bounds.Top, x, bounds.Bottom - 1);
                    using Pen pen2 = new(line2);
                    e.Graphics.DrawLine(pen2, x + 1, bounds.Top + 1, x + 1, bounds.Bottom);
                }
                else
                {
                    // Paint Horizontal Separator
                    Rectangle bounds = rect;
                    int x = 25;
                    int y = bounds.Height / 2;
                    using Pen pen1 = new(toolStripSeparator.ForeColor);
                    e.Graphics.DrawLine(pen1, x, y, bounds.Right - 2, y);
                    using Pen pen2 = new(line2);
                    e.Graphics.DrawLine(pen2, x + 1, y + 1, bounds.Right - 1, y + 1);
                }
            }
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using SolidBrush brush = new(BackColor);
            e.Graphics.FillRectangle(brush, e.ConnectedArea);
            e.Graphics.FillRectangle(brush, e.AffectedBounds); // Contains Border Color of DropDownMenuItem's Container
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (e.ToolStrip is null)
            {
                base.OnRenderToolStripBorder(e);
            }
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            Rectangle rect = new(0, 0, e.ToolStrip.Width, e.ToolStrip.Height);
            ControlPaint.DrawBorder(e.Graphics, rect, BorderColor, ButtonBorderStyle.Solid);
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            Rectangle rect = new(e.ImageRectangle.Left - 2, e.ImageRectangle.Top - 2,
                                 e.ImageRectangle.Width + 4, e.ImageRectangle.Height + 4);

            if (e.Item.ImageIndex == -1 && string.IsNullOrEmpty(e.Item.ImageKey) && e.Item.Image == null)
            {
                // Draw Check
                using Pen pen = new(ForeColor, 2);
                rect.Inflate(-6, -6);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Point[] points = new Point[]
                {
                    new Point(rect.Left, rect.Bottom - rect.Height / 2),
                    new Point(rect.Left + rect.Width / 3, rect.Bottom),
                    new Point(rect.Right, rect.Top)
                };
                e.Graphics.DrawLines(pen, points);
                e.Graphics.SmoothingMode = SmoothingMode.Default;
                rect.Inflate(+6, +6);
            }
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            Color bgColor = e.Item.Selected ? SelectionColor : e.Item.BackColor;

            if (!e.Item.Enabled)
                bgColor = e.Item.BackColor;

            // Normal Item
            Rectangle rect = new(2, 0, e.Item.Width - 3, e.Item.Height);

            using SolidBrush sb1 = new(bgColor);
            e.Graphics.FillRectangle(sb1, rect);

            // Header Item On Open Menu
            if (e.Item.GetType() == typeof(ToolStripMenuItem))
            {
                if (((ToolStripMenuItem)e.Item).DropDown.Visible && e.Item.IsOnDropDown == false)
                {
                    using SolidBrush sb2 = new(BackColor);
                    e.Graphics.FillRectangle(sb2, rect);
                }
            }
        }

        
    }
}
