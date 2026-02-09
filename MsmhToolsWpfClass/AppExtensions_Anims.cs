using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows;

namespace MsmhToolsWpfClass;

public static class AppExtensions_Anims
{
    public static async Task AnimRotateAsync(this FrameworkElement element, double toDegree, int durationMS)
    {
        try
        {
            if (element == null) return;
            RotateTransform? rotateTransform;
            DoubleAnimation? animation;

            if (element.RenderTransform == null)
            {
                rotateTransform = new();
                element.RenderTransform = rotateTransform;
                element.RenderTransformOrigin = new Point(0.5, 0.5);
            }
            else
            {
                if (element.RenderTransform is RotateTransform rt)
                {
                    element.RenderTransformOrigin = new Point(0.5, 0.5);
                    rotateTransform = rt;
                }
                else
                {
                    rotateTransform = new();
                    element.RenderTransform = rotateTransform;
                    element.RenderTransformOrigin = new Point(0.5, 0.5);
                }
            }

            if (rotateTransform != null)
            {
                animation = new(toDegree, new Duration(TimeSpan.FromMilliseconds(durationMS)));
                rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animation);

                await Task.Delay(durationMS + 10);
                rotateTransform = null;
                animation = null;
                GC.Collect();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions_Anims AnimRotateAsync: " + ex.Message);
        }
    }

    public static async Task AnimSlideHAsync(this FrameworkElement element, double toX, int durationMS)
    {
        try
        {
            if (element == null) return;
            TranslateTransform? translateTransform;
            DoubleAnimation? animation;

            if (element.RenderTransform == null)
            {
                translateTransform = new();
                element.RenderTransform = translateTransform;
            }
            else
            {
                if (element.RenderTransform is TranslateTransform tt)
                {
                    translateTransform = tt;
                }
                else
                {
                    translateTransform = new();
                    element.RenderTransform = translateTransform;
                }
            }

            if (translateTransform != null)
            {
                animation = new()
                {
                    From = translateTransform.X,
                    To = toX,
                    Duration = TimeSpan.FromMilliseconds(durationMS),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };

                translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);

                await Task.Delay(durationMS + 10);
                translateTransform = null;
                animation = null;
                GC.Collect();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions_Anims AnimSlideHAsync: " + ex.Message);
        }
    }

    public static async Task AnimSlideVAsync(this FrameworkElement element, double toY, int durationMS)
    {
        try
        {
            if (element == null) return;
            TranslateTransform? translateTransform;
            DoubleAnimation? animation;

            if (element.RenderTransform == null)
            {
                translateTransform = new();
                element.RenderTransform = translateTransform;
            }
            else
            {
                if (element.RenderTransform is TranslateTransform tt)
                {
                    translateTransform = tt;
                }
                else
                {
                    translateTransform = new();
                    element.RenderTransform = translateTransform;
                }
            }

            if (translateTransform != null)
            {
                animation = new()
                {
                    From = translateTransform.Y,
                    To = toY,
                    Duration = TimeSpan.FromMilliseconds(durationMS),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };

                translateTransform.BeginAnimation(TranslateTransform.YProperty, animation);

                await Task.Delay(durationMS + 10);
                translateTransform = null;
                animation = null;
                GC.Collect();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions_Anims AnimSlideVAsync: " + ex.Message);
        }
    }

    public static async Task AnimBlurInAsync(this FrameworkElement element, double radius = 10, int durationMs = 300)
    {
        try
        {
            if (element == null) return;
            BlurEffect? blur;
            DoubleAnimation? animation;

            if (element.Effect == null)
            {
                blur = new() { Radius = 0 };
                element.Effect = blur;
            }
            else
            {
                if (element.Effect is BlurEffect be)
                {
                    be.Radius = 0;
                    blur = be;
                }
                else
                {
                    blur = new() { Radius = 0 };
                    element.Effect = blur;
                }
            }

            if (blur != null)
            {
                animation = new()
                {
                    From = blur.Radius,
                    To = radius,
                    Duration = TimeSpan.FromMilliseconds(durationMs),
                    FillBehavior = FillBehavior.HoldEnd
                };

                blur.BeginAnimation(BlurEffect.RadiusProperty, animation);

                await Task.Delay(durationMs + 10);
                blur = null;
                animation = null;
                GC.Collect();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions_Anims AnimBlurInAsync: " + ex.Message);
        }
    }

    public static async Task AnimBlurOutAsync(this FrameworkElement element, int durationMs = 300)
    {
        try
        {
            if (element == null) return;
            BlurEffect? blur;
            DoubleAnimation? animation;

            if (element.Effect is BlurEffect be)
            {
                blur = be;

                animation = new()
                {
                    From = blur.Radius,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(durationMs),
                    FillBehavior = FillBehavior.Stop
                };

                blur.BeginAnimation(BlurEffect.RadiusProperty, animation);

                await Task.Delay(durationMs + 10);
                element.Effect = null;
                blur = null;
                animation = null;
                GC.Collect();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions_Anims AnimBlurOutAsync: " + ex.Message);
        }
    }

}