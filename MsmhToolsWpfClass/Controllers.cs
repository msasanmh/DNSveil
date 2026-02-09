using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MsmhToolsWpfClass;

public class Controllers
{
    public static List<Control> GetAllControls(Visual parent)
    {
        List<Control> listC = new();
        EnumVisual(parent);

        void EnumVisual(Visual visual)
        {
            try
            {
                if (visual is Control c1) listC.Add(c1);

                if (visual is TabControl tabControl)
                {
                    for (int n = 0; n < tabControl.Items.Count; n++)
                    {
                        object tabItemObj = tabControl.Items[n];
                        if (tabItemObj is TabItem tabItem)
                        {
                            EnumVisual(tabItem);
                        }
                    }
                }

                for (int n = 0; n < VisualTreeHelper.GetChildrenCount(visual); n++)
                {
                    if (VisualTreeHelper.GetChild(visual, n) is Visual childVisual)
                    {
                        EnumVisual(childVisual);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Controllers GetAllControls: " + ex.Message);
            }
        }

        return listC;
    }

    public static List<Visual> GetAllVisuals(Visual parent)
    {
        List<Visual> listC = new();
        EnumVisual(parent);

        void EnumVisual(Visual visual)
        {
            try
            {
                listC.Add(visual);

                if (visual is TabControl tabControl)
                {
                    for (int n = 0; n < tabControl.Items.Count; n++)
                    {
                        object tabItemObj = tabControl.Items[n];
                        if (tabItemObj is Visual tabItem)
                        {
                            EnumVisual(tabItem);
                        }
                    }
                }

                for (int n = 0; n < VisualTreeHelper.GetChildrenCount(visual); n++)
                {
                    if (VisualTreeHelper.GetChild(visual, n) is Visual childVisual)
                    {
                        EnumVisual(childVisual);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Controllers GetAllVisuals: " + ex.Message);
            }
        }

        return listC;
    }

    public static List<T> GetAllElementsByType<T>(Visual parent)
    {
        List<T> listT = new();

        try
        {
            List<Visual> visuals = GetAllVisuals(parent);
            for (int n = 0; n < visuals.Count; n++)
            {
                Visual visual = visuals[n];
                if (visual is T t) listT.Add(t);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Controllers GetAllElementsByType: " + ex.Message);
        }

        return listT;
    } // Usage: var toolStripButtons = GetAllElementsByType<ToolStripDropButton>(window);

    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject? depObj) where T : DependencyObject
    {
        if (depObj != null)
        {
            for (int n = 0; n < VisualTreeHelper.GetChildrenCount(depObj); n++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, n);
                if (child is not null and T tChild) yield return tChild;
                foreach (T childOfChild in FindVisualChildren<T>(child)) yield return childOfChild;
            }
        }
    } // Usage: foreach (var rectangle in FindVisualChildren<Rectangle>(this))

    public static bool IsElementChildOfParent(DependencyObject child, DependencyObject parent)
    {
        try
        {
            if (child.GetHashCode() == parent.GetHashCode()) return true;
            IEnumerable<DependencyObject> elemList = FindVisualChildren<DependencyObject>(parent);
            foreach (DependencyObject obj in elemList)
            {
                if (obj.GetHashCode() == child.GetHashCode()) return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Controllers IsElementChildOfParent: " + ex.Message);
        }

        return false;
    }

}