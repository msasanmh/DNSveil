using System;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace MsmhTools
{
    public static class Drawing
    {
        //-----------------------------------------------------------------------------------
        public static GraphicsPath RoundedRectangle(Rectangle bounds, int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft)
        {
            int diameterTopLeft = radiusTopLeft * 2;
            int diameterTopRight = radiusTopRight * 2;
            int diameterBottomRight = radiusBottomRight * 2;
            int diameterBottomLeft = radiusBottomLeft * 2;

            Rectangle arc1 = new(bounds.Location, new Size(diameterTopLeft, diameterTopLeft));
            Rectangle arc2 = new(bounds.Location, new Size(diameterTopRight, diameterTopRight));
            Rectangle arc3 = new(bounds.Location, new Size(diameterBottomRight, diameterBottomRight));
            Rectangle arc4 = new(bounds.Location, new Size(diameterBottomLeft, diameterBottomLeft));
            GraphicsPath path = new();

            // Top Left Arc  
            if (radiusTopLeft == 0)
            {
                path.AddLine(arc1.Location, arc1.Location);
            }
            else
            {
                path.AddArc(arc1, 180, 90);
            }
            // Top Right Arc  
            arc2.X = bounds.Right - diameterTopRight;
            if (radiusTopRight == 0)
            {
                path.AddLine(arc2.Location, arc2.Location);
            }
            else
            {
                path.AddArc(arc2, 270, 90);
            }
            // Bottom Right Arc
            arc3.X = bounds.Right - diameterBottomRight;
            arc3.Y = bounds.Bottom - diameterBottomRight;
            if (radiusBottomRight == 0)
            {
                path.AddLine(arc3.Location, arc3.Location);
            }
            else
            {
                path.AddArc(arc3, 0, 90);
            }
            // Bottom Left Arc 
            arc4.X = bounds.Right - diameterBottomLeft;
            arc4.Y = bounds.Bottom - diameterBottomLeft;
            arc4.X = bounds.Left;
            if (radiusBottomLeft == 0)
            {
                path.AddLine(arc4.Location, arc4.Location);
            }
            else
            {
                path.AddArc(arc4, 90, 90);
            }
            path.CloseFigure();
            return path;
        }
        //-----------------------------------------------------------------------------------
        public static Bitmap Invert(Bitmap source)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new(source.Width, source.Height);
            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);
            // create the negative color matrix
            ColorMatrix colorMatrix = new(new float[][]
            {
                    new float[] {-1, 0, 0, 0, 0},
                    new float[] {0, -1, 0, 0, 0},
                    new float[] {0, 0, -1, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {1, 1, 1, 0, 1}
            });
            // create some image attributes
            ImageAttributes attributes = new();
            attributes.SetColorMatrix(colorMatrix);
            g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height),
                        0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }
        //-----------------------------------------------------------------------------------
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
        private const int WM_SETREDRAW = 11;
        public static void SuspendDrawing(Control parent)
        {
            _ = SendMessage(parent.Handle, WM_SETREDRAW, false, 0);
        }
        public static void ResumeDrawing(Control parent)
        {
            _ = SendMessage(parent.Handle, WM_SETREDRAW, true, 0);
            parent.Refresh();
        }
        //-----------------------------------------------------------------------------------
    }
}
