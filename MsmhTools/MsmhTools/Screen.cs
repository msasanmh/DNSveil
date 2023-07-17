using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsmhTools
{
    public class ScreenDPI
    {
        public static void ScaleForm(Form form, bool scaleX, bool scaleY)
        {
            using Graphics g = form.CreateGraphics();
            float scaleFactorX = 1;
            float scaleFactorY = 1;

            // 96 = 100%
            // 120 = 125%
            // 144 = 150%

            if (g.DpiX > 96)
                scaleFactorX = g.DpiX / 96;

            if (g.DpiY > 96)
                scaleFactorY = g.DpiY / 96;

            if (form.AutoScaleDimensions == form.CurrentAutoScaleDimensions)
            {
                if (!scaleX && !scaleY)
                    form.Scale(new SizeF(1, 1));
                else if (scaleX && !scaleY)
                    form.Scale(new SizeF(scaleFactorX, 1));
                else if (!scaleX && scaleY)
                    form.Scale(new SizeF(1, scaleFactorY));
                else if (scaleX && scaleY)
                    form.Scale(new SizeF(scaleFactorX, scaleFactorY));
            }
            // Doesn't work!
            //AutoScaleMode = AutoScaleMode.Font;
            //AutoScaleDimensions = new SizeF(6F, 13F);
        }
    }
}
