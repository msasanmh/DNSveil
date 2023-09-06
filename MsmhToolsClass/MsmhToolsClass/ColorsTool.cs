using System;
using System.Drawing;

#nullable enable
namespace MsmhToolsClass
{
    public static class ColorsTool
    {
        //-----------------------------------------------------------------------------------
        /// <summary>
        /// Converts the HSL values to a Color.
        /// </summary>
        /// <param name="alpha">The alpha. (0 - 255)</param>
        /// <param name="hue">The hue. (0f - 360f)</param>
        /// <param name="saturation">The saturation. (0f - 1f)</param>
        /// <param name="lighting">The lighting. (0f - 1f)</param>
        /// <returns></returns>
        public static Color FromHsl(int alpha, float hue, float saturation, float lighting)
        {
            if (0 > alpha || 255 < alpha)
            {
                throw new ArgumentOutOfRangeException(nameof(alpha));
            }
            if (0f > hue || 360f < hue)
            {
                throw new ArgumentOutOfRangeException(nameof(hue));
            }
            if (0f > saturation || 1f < saturation)
            {
                throw new ArgumentOutOfRangeException(nameof(saturation));
            }
            if (0f > lighting || 1f < lighting)
            {
                throw new ArgumentOutOfRangeException(nameof(lighting));
            }

            if (0 == saturation)
            {
                return Color.FromArgb(alpha, Convert.ToInt32(lighting * 255), Convert.ToInt32(lighting * 255), Convert.ToInt32(lighting * 255));
            }

            float fMax, fMid, fMin;
            int iSextant, iMax, iMid, iMin;

            if (0.5 < lighting)
            {
                fMax = lighting - (lighting * saturation) + saturation;
                fMin = lighting + (lighting * saturation) - saturation;
            }
            else
            {
                fMax = lighting + (lighting * saturation);
                fMin = lighting - (lighting * saturation);
            }

            iSextant = (int)Math.Floor(hue / 60f);
            if (300f <= hue)
            {
                hue -= 360f;
            }
            hue /= 60f;
            hue -= 2f * (float)Math.Floor(((iSextant + 1f) % 6f) / 2f);
            if (0 == iSextant % 2)
            {
                fMid = hue * (fMax - fMin) + fMin;
            }
            else
            {
                fMid = fMin - hue * (fMax - fMin);
            }

            iMax = Convert.ToInt32(fMax * 255);
            iMid = Convert.ToInt32(fMid * 255);
            iMin = Convert.ToInt32(fMin * 255);

            switch (iSextant)
            {
                case 1:
                    return Color.FromArgb(alpha, iMid, iMax, iMin);
                case 2:
                    return Color.FromArgb(alpha, iMin, iMax, iMid);
                case 3:
                    return Color.FromArgb(alpha, iMin, iMid, iMax);
                case 4:
                    return Color.FromArgb(alpha, iMid, iMin, iMax);
                case 5:
                    return Color.FromArgb(alpha, iMax, iMin, iMid);
                default:
                    return Color.FromArgb(alpha, iMax, iMid, iMin);
            }
        }
        //-----------------------------------------------------------------------------------
    }
}
