using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MsmhToolsWpfClass;

public class WpfTabControl : TabControl
{
    private ContentPresenter? PART_SelectedContentHost;

    private readonly int AnimSpeedMS = 200;
    private bool IsSwitching = false;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        PART_SelectedContentHost = GetTemplateChild(nameof(PART_SelectedContentHost)) as ContentPresenter;
    }

    public WpfTabControl()
    {
        DataContext = this;
        Loaded -= WpfTabControl_Loaded;
        Loaded += WpfTabControl_Loaded;
    }

    private void WpfTabControl_Loaded(object sender, RoutedEventArgs e)
    {
        // Animate TabItems Ellipses On Startup
        AnimateTabItemsEllipses();
    }

    private void AnimateTabItemsEllipses()
    {
        try
        {
            foreach (var item in Items)
            {
                if (ItemContainerGenerator.ContainerFromItem(item) is TabItem tabItem)
                {
                    foreach (Ellipse? ellipse in tabItem.GetChildrenOfType<Ellipse>())
                    {
                        if (ellipse != null)
                        {
                            if (ellipse.Name.Equals("PART_CircleEllipse"))
                            {
                                if (ellipse.RenderTransform is ScaleTransform scaleTransform)
                                {
                                    double targetScale = Equals(tabItem, SelectedItem) ? 1.5 : 1;
                                    DoubleAnimation xAnim = new(targetScale, TimeSpan.FromMilliseconds(AnimSpeedMS));
                                    DoubleAnimation yAnim = new(targetScale, TimeSpan.FromMilliseconds(AnimSpeedMS));

                                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, xAnim);
                                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, yAnim);

                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception) { }
    }

    protected override async void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        try
        {
            if (IsSwitching || PART_SelectedContentHost == null)
            {
                AnimateTabItemsEllipses(); // Animate TabItems Ellipses
                base.OnSelectionChanged(e); // Change Tab
                return;
            }

            IsSwitching = true;

            // Animate TabItems Ellipses
            AnimateTabItemsEllipses();

            // Fade Out Current Content
            DoubleAnimation fadeOut = new(1, 0, TimeSpan.FromMilliseconds(AnimSpeedMS));
            PART_SelectedContentHost.BeginAnimation(OpacityProperty, fadeOut);
            await Task.Delay(AnimSpeedMS);

            // Switch Tab After Fade Out
            base.OnSelectionChanged(e);

            // Fade In New Content
            DoubleAnimation fadeIn = new(0, 1, TimeSpan.FromMilliseconds(AnimSpeedMS));
            PART_SelectedContentHost.BeginAnimation(OpacityProperty, fadeIn);
            await Task.Delay(AnimSpeedMS);

            IsSwitching = false;
        }
        catch (Exception)
        {
            AnimateTabItemsEllipses(); // Animate TabItems Ellipses
            base.OnSelectionChanged(e); // Change Tab
        }
    }

}