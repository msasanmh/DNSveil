using System;
using System.Reflection;

namespace MsmhTools
{
    public static class Controllers
    {
        //-----------------------------------------------------------------------------------
        public static List<Control> GetAllControls(Control control)
        {
            List<Control> listC = new();
            GetAllSubControlsByType(control);

            void GetAllSubControlsByType(Control control)
            {
                listC.Add(control);

                if (control.HasChildren)
                {
                    for (int n = 0; n < control.Controls.Count; n++)
                    {
                        Control c = control.Controls[n];
                        GetAllSubControlsByType(c);
                    }
                }
            }
            return listC;
        }
        //-----------------------------------------------------------------------------------
        public static List<Control> GetAllChildControls(Control control)
        {
            List<Control> listC = new();
            GetAllSubControlsByType(control);

            void GetAllSubControlsByType(Control control)
            {
                if (control.HasChildren)
                {
                    for (int n = 0; n < control.Controls.Count; n++)
                    {
                        Control c = control.Controls[n];
                        listC.Add(c);
                        GetAllSubControlsByType(c);
                    }
                }
            }
            return listC;
        }
        //-----------------------------------------------------------------------------------
        public static List<T> GetAllControlsByType<T>(Control control)
        {
            List<T> listT = new();
            var type = control.GetType();
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            for (int n = 0; n < fields.Length; n++)
            {
                var field = fields[n];
                if (field.GetValue(control) != null &&
                    (field.GetValue(control).GetType().IsSubclassOf(typeof(T)) || field.GetValue(control).GetType() == typeof(T)))
                {
                    var t = (T)field.GetValue(control);
                    if (t != null)
                        listT.Add(t);
                }
            }
            return listT;
        } // Usage: var toolStripButtons = GetSubControls<ToolStripDropButton>(form);
          //-----------------------------------------------------------------------------------
        /// <summary>
        /// Recursively get SubMenu Items. Includes Separators.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IEnumerable<ToolStripItem?> GetAllToolStripItems(ToolStripItem? item)
        {
            if (item is ToolStripMenuItem)
            {
                foreach (ToolStripItem tsi in (item as ToolStripMenuItem).DropDownItems)
                {
                    if (tsi is ToolStripMenuItem)
                    {
                        if ((tsi as ToolStripMenuItem).HasDropDownItems)
                        {
                            foreach (ToolStripItem subItem in GetAllToolStripItems(tsi as ToolStripMenuItem))
                                yield return subItem;
                        }
                        yield return tsi as ToolStripMenuItem;
                    }
                    else if (tsi is ToolStripSeparator)
                    {
                        yield return tsi as ToolStripSeparator;
                    }
                }
            }
            else if (item is ToolStripSeparator)
            {
                yield return item as ToolStripSeparator;
            }
        }
        // Usage:
        // if(toolItem is ToolStripMenuItem)
        // { 
        //      ToolStripMenuItem tsmi = (toolItem as ToolStripMenuItem);
        //      //Do something with it
        // }
        // else if(toolItem is ToolStripSeparator)
        // {
        //      ToolStripSeparator tss = (toolItem as ToolStripSeparator);
        //      //Do something with it
        // }
        //-----------------------------------------------------------------------------------
        public static Control GetTopParent(Control control)
        {
            Control parent = control;
            if (control.Parent != null)
            {
                parent = control.Parent;
                if (parent.Parent != null)
                    while (parent.Parent != null)
                        parent = parent.Parent;
            }
            return parent;
        }
        //-----------------------------------------------------------------------------------
        public static void SetDarkControl(Control control)
        {
            _ = Methods.SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
            foreach (Control c in GetAllControls(control))
            {
                _ = Methods.SetWindowTheme(c.Handle, "DarkMode_Explorer", null);
            }
        }
        //-----------------------------------------------------------------------------------
    }
}
