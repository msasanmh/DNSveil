using System;
using System.Reflection;

namespace MsmhToolsWinFormsClass
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
                FieldInfo field = fields[n];
                object? value = field.GetValue(control);

                if (value != null &&
                    (value.GetType().IsSubclassOf(typeof(T)) || value.GetType() == typeof(T)))
                {
                    var t = (T)value;
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
            if (item is ToolStripMenuItem toolStripMenuItem1)
            {
                foreach (ToolStripItem tsi in toolStripMenuItem1.DropDownItems)
                {
                    if (tsi is ToolStripMenuItem toolStripMenuItem2)
                    {
                        if (toolStripMenuItem2.HasDropDownItems)
                        {
                            foreach (ToolStripItem? subItem in GetAllToolStripItems(toolStripMenuItem2))
                                yield return subItem;
                        }
                        yield return toolStripMenuItem2;
                    }
                    else if (tsi is ToolStripSeparator toolStripSeparator1)
                    {
                        yield return toolStripSeparator1;
                    }
                }
            }
            else if (item is ToolStripSeparator toolStripSeparator2)
            {
                yield return toolStripSeparator2;
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
