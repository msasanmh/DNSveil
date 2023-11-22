using CustomControls;
using MsmhToolsClass;
using MsmhToolsWinFormsClass;
using MsmhToolsWinFormsClass.Themes;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace SecureDNSClient;

public partial class FormCustomServers : Form
{
    private readonly CustomLabel LabelMoving = new();
    private static XDocument XDoc = new();
    private static readonly List<string> ListGroupNames = new();
    private static readonly string CustomServersXmlPath = SecureDNS.CustomServersXmlPath;

    // Context Menu Group
    private static readonly CustomContextMenuStrip MG = new();
    private static readonly ToolStripMenuItem MenuGroupNew = new();
    private static readonly ToolStripMenuItem MenuGroupRename = new();
    private static readonly ToolStripMenuItem MenuGroupRemove = new();
    private static readonly ToolStripMenuItem MenuGroupMoveUp = new();
    private static readonly ToolStripMenuItem MenuGroupMoveDown = new();
    private static readonly ToolStripMenuItem MenuGroupMoveToTop = new();
    private static readonly ToolStripMenuItem MenuGroupMoveToBottom = new();
    private static readonly ToolStripMenuItem MenuGroupImport = new();
    private static readonly ToolStripMenuItem MenuGroupExport = new();

    // Context Menu DNSs
    private static readonly CustomContextMenuStrip MD = new();
    private static readonly ToolStripMenuItem MenuDnsRemove = new();
    private static readonly ToolStripMenuItem MenuDnsRemoveAll = new();
    private static readonly ToolStripMenuItem MenuDnsMoveSelectedToGroup = new();
    private static readonly ToolStripMenuItem MenuDnsMoveUp = new();
    private static readonly ToolStripMenuItem MenuDnsMoveDown = new();
    private static readonly ToolStripMenuItem MenuDnsMoveToTop = new();
    private static readonly ToolStripMenuItem MenuDnsMoveToBottom = new();
    private static readonly ToolStripMenuItem MenuDnsImport = new();
    private static readonly ToolStripMenuItem MenuDnsExport = new();
    private static readonly ToolStripMenuItem MenuDnsExportAsText = new();
    private static readonly ToolStripMenuItem MenuDnsSelectAll = new();
    private static readonly ToolStripMenuItem MenuDnsInvertSelection = new();

    public FormCustomServers()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        InitializeComponent();

        // Invariant Culture
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        StartPosition = FormStartPosition.CenterScreen;

        // Load Theme
        Theme.LoadTheme(this, Theme.Themes.Dark);

        // Load XML Custom Servers
        LoadXmlCS(CustomServersXmlPath);

        // Label Moving
        Controls.Add(LabelMoving);
        LabelMoving.Text = "Now Moving...";
        LabelMoving.Size = new(300, 150);
        LabelMoving.Location = new((ClientSize.Width / 2) - (LabelMoving.Width / 2), (ClientSize.Height / 2) - (LabelMoving.Height / 2));
        LabelMoving.TextAlign = ContentAlignment.MiddleCenter;
        LabelMoving.Font = new(Font.Name, Font.Size * 2);
        Theme.SetColors(LabelMoving);
        LabelMoving.Visible = false;
        LabelMoving.SendToBack();

        Shown += FormCustomServers_Shown;
        Move += FormCustomServers_Move;
        ResizeEnd += FormCustomServers_ResizeEnd;
        Resize += FormCustomServers_Resize;
    }

    private void FormCustomServers_Shown(object? sender, EventArgs e)
    {
        LabelMoving.Visible = false;
        LabelMoving.SendToBack();
        SplitContainerMain.Visible = true;
        ReadGroups(null, false);

        // Fix Controls Location
        int spaceBottom = 6, spaceRight = 6, spaceV = 6, spaceH = 6;
        CustomDataGridViewGroups.Location = new Point(spaceRight, FormMain.LabelScreen.Height);

        CustomButtonExport.Left = CustomGroupBoxGroups.Width - CustomButtonExport.Width - spaceRight;
        CustomButtonExport.Top = CustomGroupBoxGroups.Height - CustomButtonNewGroup.Height - spaceBottom;

        CustomButtonImport.Left = CustomButtonExport.Left - CustomButtonImport.Width - spaceH;
        CustomButtonImport.Top = CustomButtonExport.Top;

        CustomButtonNewGroup.Left = CustomDataGridViewGroups.Left;
        CustomButtonNewGroup.Top = CustomButtonImport.Top;
        CustomButtonNewGroup.Width = CustomButtonImport.Left - CustomButtonNewGroup.Left - spaceH;

        CustomDataGridViewGroups.Width = CustomGroupBoxGroups.Width - (spaceH * 2);
        CustomDataGridViewGroups.Height = CustomButtonNewGroup.Top - CustomDataGridViewGroups.Top - spaceV;

        CustomDataGridViewDNSs.Location = new Point(spaceRight, FormMain.LabelScreen.Height);

        CustomButtonModifyDNS.Left = CustomGroupBoxDNSs.Width - CustomButtonModifyDNS.Width - spaceRight;
        CustomButtonModifyDNS.Top = CustomGroupBoxDNSs.Height - CustomButtonModifyDNS.Height - spaceBottom;

        CustomButtonAddServers.Left = CustomButtonModifyDNS.Left;
        CustomButtonAddServers.Top = CustomButtonModifyDNS.Top - CustomButtonAddServers.Height - spaceV;

        CustomLabelDescription.Left = CustomDataGridViewDNSs.Left;

        CustomLabelDNS.Left = CustomLabelDescription.Left;

        CustomTextBoxDescription.Left = CustomLabelDescription.Right + (spaceH * 6);
        CustomTextBoxDescription.Top = CustomButtonModifyDNS.Top;
        CustomTextBoxDescription.Width = CustomButtonModifyDNS.Left - CustomTextBoxDescription.Left - spaceH;

        CustomTextBoxDNS.Left = CustomTextBoxDescription.Left;
        CustomTextBoxDNS.Top = CustomButtonAddServers.Top + (CustomButtonAddServers.Height - CustomTextBoxDNS.Height);
        CustomTextBoxDNS.Width = CustomTextBoxDescription.Width;

        CustomLabelDescription.Top = CustomTextBoxDescription.Top + 2;

        CustomLabelDNS.Top = CustomTextBoxDNS.Top + 2;

        CustomDataGridViewDNSs.Width = CustomGroupBoxDNSs.Width - (spaceH * 2);
        CustomDataGridViewDNSs.Height = CustomButtonAddServers.Top - CustomDataGridViewDNSs.Top - spaceV;

        int btnNewGroupWidth = TextRenderer.MeasureText(CustomButtonNewGroup.Text, Font).Width;
        SplitContainerMain.SplitterDistance = btnNewGroupWidth + (CustomButtonExport.Width * 2) + (spaceRight * 2) + (spaceH * 2);
    }

    private void FormCustomServers_Move(object? sender, EventArgs e)
    {
        SplitContainerMain.Visible = false;
        LabelMoving.Location = new((ClientSize.Width / 2) - (LabelMoving.Width / 2), (ClientSize.Height / 2) - (LabelMoving.Height / 2));
        LabelMoving.Visible = true;
        LabelMoving.BringToFront();
    }

    private void FormCustomServers_ResizeEnd(object? sender, EventArgs e)
    {
        SplitContainerMain.Visible = true;
        LabelMoving.Visible = false;
        LabelMoving.SendToBack();
    }

    private void FormCustomServers_Resize(object? sender, EventArgs e)
    {
        if (WindowState != FormWindowState.Minimized)
        {
            SplitContainerMain.Visible = true;
            LabelMoving.Visible = false;
            LabelMoving.SendToBack();
        }
    }

    public void LoadXmlCS(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
                XDoc = CreateXmlCS();
                XDoc.Save(path, SaveOptions.None);
            }
            else if (string.IsNullOrWhiteSpace(File.ReadAllText(path)))
            {
                XDoc = CreateXmlCS();
                XDoc.Save(path, SaveOptions.None);
            }
            else if (!XmlTool.IsValidXML(File.ReadAllText(path)))
            {
                CustomMessageBox.Show(this, "XML file is not valid. Returned to default.", "Not Valid", MessageBoxButtons.OK, MessageBoxIcon.Error);
                XDoc = CreateXmlCS();
                XDoc.Save(path, SaveOptions.None);
            }
            XDoc = XDocument.Load(path, LoadOptions.None);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadXmlCS: {ex.Message}");
        }
    }

    private static XDocument CreateXmlCS()
    {
        XDocument doc = new();
        XElement group = new("Settings");
        group.Add(new XElement("CustomDnsList"));
        doc.Add(group);
        return doc;
    }

    private void ReadGroups(string? groupNameToSelect, bool showRow)
    {
        // Read Groups
        ListGroupNames.Clear();
        var dgvG = CustomDataGridViewGroups;
        var dgvS = CustomDataGridViewDNSs;
        int firstVisible = dgvG.FirstDisplayedScrollingRowIndex;

        if (dgvG.SelectedRows.Count > 0)
        {
            int selectedRow = dgvG.SelectedRows[0].Index;
            int displayedRowCount = dgvG.DisplayedRowCount(false);
            if (selectedRow - firstVisible == displayedRowCount)
                firstVisible++;
        }

        dgvG.Rows.Clear();

        if (XDoc.Root == null) return;
        var nodesGroups = XDoc.Root.Elements().Elements();

        if (!nodesGroups.Any())
        {
            dgvS.Rows.Clear();
            CustomGroupBoxDNSs.Text = "DNS Addresses";
            return;
        }

        dgvG.Rows.Add(nodesGroups.Count());
        for (int a = 0; a < nodesGroups.Count(); a++)
        {
            XElement node = nodesGroups.ToList()[a];
            XElement? nodeEnabled = node.Element("Enabled");
            XElement? nodeName = node.Element("Name");
            if (nodeName == null) return;
            bool cell0Value = nodeEnabled == null || Convert.ToBoolean(nodeEnabled.Value);
            dgvG.Rows[a].Cells[0].Value = cell0Value;
            dgvG.Rows[a].Cells[1].Value = nodeName.Value;
            dgvG.Rows[a].Height = TextRenderer.MeasureText(nodeName.Value.ToString(), dgvG.DefaultCellStyle.Font).Height + 5;
            dgvG.EndEdit();

            // Add Names to List
            ListGroupNames.Add(nodeName.Value);
        }

        // Select Group
        int rowIndex = 0;

        groupNameToSelect ??= dgvG.Rows[0].Cells[1].Value.ToString();
        if (string.IsNullOrEmpty(groupNameToSelect)) return;

        for (int n = 0; n < dgvG.Rows.Count; n++)
        {
            var row = dgvG.Rows[n];
            string? groupName = row.Cells[1].Value.ToString();
            if (string.IsNullOrEmpty(groupName)) continue;
            if (groupName.Equals(groupNameToSelect))
            {
                rowIndex = row.Index;
                break;
            }
        }

        if (rowIndex == 0 && showRow == false)
            ReadDNSs(groupNameToSelect); // Because SelectionChanged won't fire.

        if (showRow)
            ShowRow(dgvG, rowIndex, firstVisible);
        else
        {
            dgvG.Rows[rowIndex].Cells[0].Selected = true;
            dgvG.Rows[rowIndex].Selected = true;
        }
    }

    private void ReadDNSs(string? groupName)
    {
        Debug.WriteLine("ReadDNSs Fired: " + groupName);
        var dgvG = CustomDataGridViewGroups;
        var dgvS = CustomDataGridViewDNSs;

        if (string.IsNullOrEmpty(groupName))
        {
            dgvS.Rows.Clear();
            return;
        }

        if (dgvG.Rows.Count == 0) return;

        List<DataGridViewRow> pList = new();

        if (XDoc.Root == null) return;
        var nodes = XDoc.Root.Elements().Elements();

        for (int a = 0; a < nodes.Count(); a++)
        {
            XElement nodeG = nodes.ToList()[a];
            XElement? nodeGName = nodeG.Element("Name");
            if (nodeGName == null) return;
            if (groupName == nodeGName.Value)
            {
                int count = nodeG.Elements("DnsItem").Count();
                dgvS.Rows.Clear();

                for (int b = 0; b < count; b++)
                {
                    XElement node = nodeG.Elements("DnsItem").ToList()[b];
                    XElement? nodeEnabled = node.Element("Enabled");
                    XElement? nodeDns = node.Element("Dns");
                    XElement? nodeDescription = node.Element("Description");

                    if (nodeEnabled == null || nodeDns == null || nodeDescription == null)
                    {
                        // Clear TextBoxes
                        CustomTextBoxDNS.Text = string.Empty;
                        CustomTextBoxDescription.Texts = string.Empty;
                        return;
                    }

                    // Add by AddRange
                    DataGridViewRow row = new();
                    row.CreateCells(dgvS, "cell0", "cell1", "cell2");
                    row.Cells[0].Value = Convert.ToBoolean(nodeEnabled.Value);
                    row.Cells[1].Value = nodeDns.Value;
                    row.Cells[2].Value = nodeDescription.Value;
                    row.Height = TextRenderer.MeasureText(nodeDns.Value.ToString(), dgvS.DefaultCellStyle.Font).Height + 5;
                    pList.Add(row);
                }

                dgvS.Rows.AddRange(pList.ToArray());
            }
        }
    }

    private static void ShowRow(DataGridView dgv, int rowToSelect, int firstVisibleRow)
    {
        if (0 <= rowToSelect && rowToSelect < dgv.RowCount)
        {
            dgv.Rows[0].Cells[0].Selected = false;
            dgv.Rows[0].Selected = false;
            dgv.Rows[rowToSelect].Cells[0].Selected = true;
            dgv.Rows[rowToSelect].Selected = true;
            if (rowToSelect < firstVisibleRow || firstVisibleRow == -1)
                firstVisibleRow = rowToSelect;
            dgv.FirstDisplayedScrollingRowIndex = firstVisibleRow;
        }
    }

    private static void ShowRow(DataGridView dgv, int[] rowsToSelect, int firstVisibleRow)
    {
        bool zero = false;
        for (int n = 0; n < rowsToSelect.Length; n++)
        {
            int rowToSelect = rowsToSelect[n];
            if (rowToSelect == -1) return;
            if (0 <= rowToSelect && rowToSelect < dgv.RowCount)
            {
                if (rowToSelect != 0 && zero == false)
                {
                    dgv.Rows[0].Cells[0].Selected = false;
                    dgv.Rows[0].Selected = false;
                }
                else
                {
                    dgv.Rows[0].Cells[0].Selected = true;
                    dgv.Rows[0].Selected = true;
                    zero = true;
                }
                dgv.Rows[rowToSelect].Cells[0].Selected = true;
                dgv.Rows[rowToSelect].Selected = true;
            }
            if (rowToSelect < firstVisibleRow || firstVisibleRow == -1)
                firstVisibleRow = rowToSelect;
            dgv.FirstDisplayedScrollingRowIndex = firstVisibleRow;
        }
    }

    private static XElement GetXmlGroupByName(XDocument xDoc, string groupName)
    {
        XElement group = new("Default");

        if (xDoc.Root != null)
        {
            var nodesGroups = xDoc.Root.Elements().Elements();

            for (int a = 0; a < nodesGroups.Count(); a++)
            {
                XElement nodeG = nodesGroups.ToList()[a];
                XElement? nodeGName = nodeG.Element("Name");
                if (nodeGName != null && nodeGName.Value == groupName)
                {
                    group = nodeG;
                    break;
                }
            }
        }

        return group;
    }

    private static void RenameGroup(XDocument xDoc, string oldName, string newName)
    {
        if (xDoc.Root == null) return;
        var nodesGroups = xDoc.Root.Elements().Elements();

        for (int n = 0; n < nodesGroups.Count(); n++)
        {
            XElement nodeG = nodesGroups.ToList()[n];
            XElement? nodeGName = nodeG.Element("Name");
            if (nodeGName != null && nodeGName.Value == oldName)
            {
                nodeGName.Value = newName;
                break;
            }
        }
    }

    private void RemoveGroup(string groupName)
    {
        if (XDoc.Root != null)
        {
            var nodesGroups = XDoc.Root.Elements().Elements();

            for (int a = 0; a < nodesGroups.Count(); a++)
            {
                XElement nodeG = nodesGroups.ToList()[a];
                XElement? nodeGName = nodeG.Element("Name");
                if (nodeGName != null && nodeGName.Value == groupName)
                {
                    nodeG.Remove();
                    break;
                }
            }
        }

        // Find Previous Group Name Otherwise Next
        int index = ListGroupNames.GetIndex(groupName);
        if (index == -1 || index == 0)
        {
            // Refresh Groups
            ReadGroups(null, true);
            return;
        }
        else
        {
            string previousGroup = ListGroupNames[index - 1];
            // Refresh Groups
            ReadGroups(previousGroup, true);
        }
    }

    private void RemoveDNSs(string groupName)
    {
        var dgvS = CustomDataGridViewDNSs;

        int[] rows = new int[dgvS.SelectedRows.Count];
        for (int n = 0; n < dgvS.SelectedRows.Count; n++)
        {
            rows[n] = dgvS.SelectedRows[n].Index;
        }
        Array.Sort(rows);

        if (XDoc.Root == null) return;
        var nodesGroups = XDoc.Root.Elements().Elements();

        for (int a = 0; a < nodesGroups.Count(); a++)
        {
            XElement nodeG = nodesGroups.ToList()[a];
            XElement? nodeGName = nodeG.Element("Name");
            if (nodeGName != null && nodeGName.Value == groupName)
            {
                int less = 0;
                for (int b = 0; b < rows.Length; b++)
                {
                    int row = rows[b];
                    if (b != 0)
                    {
                        less++;
                        row = rows[b] - less;
                    }
                    var node = nodeG.Elements("DnsItem").ToList()[row];
                    node.Remove();
                    Debug.WriteLine("DNS Removed At " + row);
                }
                break;
            }
        }
    }

    private void MoveDNSsToGroup(string groupToSend, int[] dnsRows, int rowToInsert, bool addBeforeSelf)
    {
        if (XDoc.Root == null) return;
        var nodesGroups = XDoc.Root.Elements().Elements();
        List<XElement> selectedDNSs = new();

        for (int a = 0; a < nodesGroups.Count(); a++)
        {
            XElement nodeG = nodesGroups.ToList()[a];
            XElement? nodeGName = nodeG.Element("Name");
            if (nodeGName != null && nodeGName.Value == groupToSend)
            {
                // Add Selected DNSs To List
                for (int b = 0; b < dnsRows.Length; b++)
                {
                    int row = dnsRows[b];
                    XElement dns = nodeG.Elements("DnsItem").ToList()[row];
                    selectedDNSs.Add(dns);
                }

                // Remove Selected DNSs
                int less = 0;
                for (int b = 0; b < dnsRows.Length; b++)
                {
                    int row = dnsRows[b];
                    if (b != 0)
                    {
                        less++;
                        row = dnsRows[b] - less;
                    }
                    var node = nodeG.Elements("DnsItem").ToList()[row];
                    node.Remove();
                }

                // Copy Selected DNSs
                selectedDNSs.Reverse();
                for (int b = 0; b < selectedDNSs.Count; b++)
                {
                    XElement dns = selectedDNSs[b];
                    CopyDnsToGroup(groupToSend, dns, rowToInsert, addBeforeSelf);
                }

                break;
            }
        }
    }

    private void CopyDnsToGroup(string groupToSend, XElement dnsToCopy, int? rowToInsert, bool? addBeforeSelf)
    {
        var dgvG = CustomDataGridViewGroups;

        if (dgvG.RowCount == 0) return;

        if (XDoc.Root == null) return;
        var nodesGroups = XDoc.Root.Elements().Elements();

        for (int a = 0; a < nodesGroups.Count(); a++)
        {
            XElement nodeG = nodesGroups.ToList()[a];
            XElement? nodeGName = nodeG.Element("Name");
            if (nodeGName != null && nodeGName.Value == groupToSend)
            {
                if (rowToInsert == null)
                {
                    nodeG.Add(dnsToCopy);
                }
                else
                {
                    List<XElement> dnss = nodeG.Elements("DnsItem").ToList();
                    for (int b = 0; b < dnss.Count; b++)
                    {
                        XElement dns = dnss[b];
                        if (rowToInsert == b)
                        {
                            if (addBeforeSelf != null)
                            {
                                if ((bool)addBeforeSelf)
                                    dns.AddBeforeSelf(dnsToCopy);
                                else
                                    dns.AddAfterSelf(dnsToCopy);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private XElement CreateDNS()
    {
        XElement dns = new("DnsItem");

        XElement enabled = new("Enabled");
        enabled.Value = "True";
        dns.Add(enabled);

        XElement dnsAddress = new("Dns");
        dnsAddress.Value = CustomTextBoxDNS.Text.Trim();
        dns.Add(dnsAddress);

        XElement dnsDescription = new("Description");
        dnsDescription.Value = CustomTextBoxDescription.Text.Trim();
        dns.Add(dnsDescription);

        return dns;
    }

    private static XElement CreateDNS(string dnsAddress, string dnsDescription)
    {
        XElement dns = new("DnsItem");

        XElement enabled = new("Enabled");
        enabled.Value = "True";
        dns.Add(enabled);

        XElement dnsAddressElement = new("Dns");
        dnsAddressElement.Value = dnsAddress.Trim();
        dns.Add(dnsAddressElement);

        XElement dnsDescriptionElement = new("Description");
        dnsDescriptionElement.Value = dnsDescription.Trim();
        dns.Add(dnsDescriptionElement);

        return dns;
    }

    //======================================= Groups ======================================

    private void CustomDataGridViewGroups_SelectionChanged(object sender, EventArgs e)
    {
        var dgvG = CustomDataGridViewGroups;
        var dgvS = CustomDataGridViewDNSs;

        if (dgvG.RowCount == 0) return;
        if (dgvG.SelectedCells.Count <= 0) return;

        int currentRow = dgvG.SelectedCells[0].RowIndex;

        if (dgvG.Rows[currentRow].Cells[1].Value == null) return;

        string? group = dgvG.Rows[currentRow].Cells[1].Value.ToString();

        string dnsNoMsg = string.Empty;
        if (dgvS.RowCount > 0 && dgvS.SelectedCells.Count > 0)
        {
            int dnsNo = dgvS.SelectedCells[0].RowIndex + 1;
            dnsNoMsg = $" - DNS No: {dnsNo}";
        }

        string msg = $"DNSs for group \"{group}\"{dnsNoMsg}";
        CustomGroupBoxDNSs.Text = msg;

        if (!string.IsNullOrEmpty(group))
            ReadDNSs(group);
    }

    private void CustomDataGridViewGroups_KeyDown(object sender, KeyEventArgs e)
    {
        // Assign Menu Shortcuts to KeyDown (Use Shortcuts When Menu is Not Displayed)
        CreateMenuGroup();
        void checkShortcut(ToolStripMenuItem item)
        {
            if (item.ShortcutKeys == e.KeyData)
            {
                //item.PerformClick(); // Doesn't work correctly
                if (item.Text.Contains("New group..."))
                    MenuGroupNew_Click(null, null);
                else if (item.Text.Contains("Rename group..."))
                    MenuGroupRename_Click(null, null);
                else if (item.Text.Contains("Remove"))
                    MenuGroupRemove_Click(null, null);
                else if (item.Text.Contains("Move up"))
                    MenuGroupMoveUp_Click(null, null);
                else if (item.Text.Contains("Move down"))
                    MenuGroupMoveDown_Click(null, null);
                else if (item.Text.Contains("Move to top"))
                    MenuGroupMoveToTop_Click(null, null);
                else if (item.Text.Contains("Move to bottom"))
                    MenuGroupMoveToBottom_Click(null, null);
                else if (item.Text.Contains("Import"))
                    MenuGroupImport_Click(null, null);
                else if (item.Text.Contains("Export"))
                    MenuGroupExport_Click(null, null);
                else
                    item.PerformClick();
                return;
            }

            foreach (ToolStripMenuItem child in item.DropDownItems.OfType<ToolStripMenuItem>())
            {
                checkShortcut(child);
            }
        }

        foreach (ToolStripMenuItem item in MG.Items.OfType<ToolStripMenuItem>())
        {
            checkShortcut(item);
        }

        // Make ContextMenu Shortcuts Work (Move up and Move Down)
        if (e.Control && e.KeyCode == Keys.Up || e.Control && e.KeyCode == Keys.Down)
        {
            e.SuppressKeyPress = true;
        }
    }

    private void CustomDataGridViewGroups_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        var dgvG = CustomDataGridViewGroups;
        if (dgvG.Rows.Count == 0) return;
        if (dgvG.SelectedCells.Count <= 0) return;
        if (dgvG.Rows[e.RowIndex].Cells[1].Value == null) return;

        string? group = dgvG.Rows[e.RowIndex].Cells[1].Value.ToString();
        //Debug.WriteLine(group);

        // Save CheckBox State for Groups
        if (e.ColumnIndex == 0)
        {
            if (XDoc.Root == null) return;
            var nodesGroups = XDoc.Root.Elements().Elements();

            for (int a = 0; a < nodesGroups.Count(); a++)
            {
                XElement nodeG = nodesGroups.ToList()[a];
                XElement? nodeGName = nodeG.Element("Name");
                XElement? nodeGEnabled = nodeG.Element("Enabled");
                if (nodeGName != null && nodeGEnabled != null && group == nodeGName.Value)
                {
                    string? isEnabled = dgvG.Rows[a].Cells[0].Value.ToString();
                    nodeGEnabled.Value = !string.IsNullOrEmpty(isEnabled) ? isEnabled : "False";

                    // Save xDocument to File
                    try
                    {
                        XDoc.Save(CustomServersXmlPath, SaveOptions.None);
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }
                }
            }
        }
    }

    private void CustomDataGridViewGroups_MouseDown(object sender, MouseEventArgs e)
    {
        // Context Menu
        if (e.Button == MouseButtons.Right)
        {
            var dgvG = CustomDataGridViewGroups;
            dgvG.Select(); // Set Focus on Control
            CreateMenuGroup();

            int currentMouseOverRow = dgvG.HitTest(e.X, e.Y).RowIndex;
            int totalRows = dgvG.Rows.Count;

            if (currentMouseOverRow != -1)
            {
                dgvG.Rows[currentMouseOverRow].Cells[0].Selected = true;
                dgvG.Rows[currentMouseOverRow].Selected = true;
            }

            if (currentMouseOverRow == -1)
            {
                MG.Items.Remove(MenuGroupRename);
                MG.Items.Remove(MenuGroupRemove);
                MG.Items.Remove(MenuGroupMoveUp);
                MG.Items.Remove(MenuGroupMoveDown);
                MG.Items.Remove(MenuGroupMoveToTop);
                MG.Items.Remove(MenuGroupMoveToBottom);
                MG.Items.RemoveAt(2);
            }
            else if (currentMouseOverRow == 0)
            {
                MG.Items.Remove(MenuGroupMoveUp);
                MG.Items.Remove(MenuGroupMoveToTop);
            }
            else if (currentMouseOverRow == totalRows - 1)
            {
                MG.Items.Remove(MenuGroupMoveDown);
                MG.Items.Remove(MenuGroupMoveToBottom);
            }

            if (totalRows == 0)
            {
                MG.Items.Remove(MenuGroupExport);
            }
            else if (totalRows == 1)
            {
                MG.Items.Remove(MenuGroupMoveDown);
                MG.Items.Remove(MenuGroupMoveToBottom);
                if (currentMouseOverRow != -1)
                    MG.Items.RemoveAt(3);
            }

            Theme.SetColors(MG);
            MG.RoundedCorners = 5;
            MG.Show(dgvG, new Point(e.X, e.Y));
        }
    }

    private void CreateMenuGroup()
    {
        // Context Menu
        MG.Items.Clear();
        MG.Font = Font;

        MenuGroupNew.Font = Font;
        MenuGroupNew.Text = "New group...";
        MenuGroupNew.ShortcutKeys = Keys.Control | Keys.Shift | Keys.N;
        MenuGroupNew.Click -= MenuGroupNew_Click;
        MenuGroupNew.Click += MenuGroupNew_Click;
        MG.Items.Add(MenuGroupNew);

        MenuGroupRename.Font = Font;
        MenuGroupRename.Text = "Rename group...";
        MenuGroupRename.ShortcutKeys = Keys.F2;
        MenuGroupRename.Click -= MenuGroupRename_Click;
        MenuGroupRename.Click += MenuGroupRename_Click;
        MG.Items.Add(MenuGroupRename);

        MenuGroupRemove.Font = Font;
        MenuGroupRemove.Text = "Remove";
        MenuGroupRemove.ShortcutKeys = Keys.Delete;
        MenuGroupRemove.Click -= MenuGroupRemove_Click;
        MenuGroupRemove.Click += MenuGroupRemove_Click;
        MG.Items.Add(MenuGroupRemove);

        MG.Items.Add("-");

        MenuGroupMoveUp.Font = Font;
        MenuGroupMoveUp.Text = "Move up";
        MenuGroupMoveUp.ShortcutKeys = Keys.Control | Keys.Up;
        MenuGroupMoveUp.Click -= MenuGroupMoveUp_Click;
        MenuGroupMoveUp.Click += MenuGroupMoveUp_Click;
        MG.Items.Add(MenuGroupMoveUp);

        MenuGroupMoveDown.Font = Font;
        MenuGroupMoveDown.Text = "Move down";
        MenuGroupMoveDown.ShortcutKeys = Keys.Control | Keys.Down;
        MenuGroupMoveDown.Click -= MenuGroupMoveDown_Click;
        MenuGroupMoveDown.Click += MenuGroupMoveDown_Click;
        MG.Items.Add(MenuGroupMoveDown);

        MenuGroupMoveToTop.Font = Font;
        MenuGroupMoveToTop.Text = "Move to top";
        MenuGroupMoveToTop.ShortcutKeys = Keys.Control | Keys.Home;
        MenuGroupMoveToTop.Click -= MenuGroupMoveToTop_Click;
        MenuGroupMoveToTop.Click += MenuGroupMoveToTop_Click;
        MG.Items.Add(MenuGroupMoveToTop);

        MenuGroupMoveToBottom.Font = Font;
        MenuGroupMoveToBottom.Text = "Move to bottom";
        MenuGroupMoveToBottom.ShortcutKeys = Keys.Control | Keys.End;
        MenuGroupMoveToBottom.Click -= MenuGroupMoveToBottom_Click;
        MenuGroupMoveToBottom.Click += MenuGroupMoveToBottom_Click;
        MG.Items.Add(MenuGroupMoveToBottom);

        MG.Items.Add("-");

        MenuGroupImport.Font = Font;
        MenuGroupImport.Text = "Import";
        MenuGroupImport.ShortcutKeys = Keys.Control | Keys.I;
        MenuGroupImport.Click -= MenuGroupImport_Click;
        MenuGroupImport.Click += MenuGroupImport_Click;
        MG.Items.Add(MenuGroupImport);

        MenuGroupExport.Font = Font;
        MenuGroupExport.Text = "Export";
        MenuGroupExport.ShortcutKeys = Keys.Control | Keys.E;
        MenuGroupExport.Click -= MenuGroupExport_Click;
        MenuGroupExport.Click += MenuGroupExport_Click;
        MG.Items.Add(MenuGroupExport);

        Theme.SetColors(MG);
    }

    private void MenuGroupNew_Click(object? sender, EventArgs? e)
    {
        string newGroupName = string.Empty;
        switch (CustomInputBox.Show(this, ref newGroupName, "New Group Name:", false, "Create group"))
        {
            case DialogResult.OK:
                break;
            case DialogResult.Cancel:
                return;
            default:
                return;
        }

        newGroupName = newGroupName.Trim();

        if (string.IsNullOrEmpty(newGroupName) || string.IsNullOrWhiteSpace(newGroupName))
        {
            string msg = "Name cannot be empty or white space.";
            CustomMessageBox.Show(this, msg, "Message");
            return;
        }

        if (newGroupName.ToLower().Equals("msasanmh"))
        {
            string msg = $"\"{newGroupName}\" is predefined, choose another name.";
            CustomMessageBox.Show(this, msg, "Message");
            return;
        }

        if (ListGroupNames.Contains(newGroupName))
        {
            string msg = $"\"{newGroupName}\" is already exist, choose another name.";
            CustomMessageBox.Show(this, msg, "Message");
            return;
        }

        if (XDoc.Root == null) return;
        var nodes = XDoc.Root.Elements();
        for (int a = 0; a < nodes.Count(); a++)
        {
            XElement node = nodes.ToList()[a];

            XElement group = new("Group");
            group.Add(new XElement("Name", newGroupName));
            group.Add(new XElement("Enabled", "True"));
            node.Add(group);
        }

        // Refresh Groups
        ReadGroups(newGroupName, false);

        // Save xDocument to File
        try
        {
            XDoc.Save(CustomServersXmlPath, SaveOptions.None);
        }
        catch (Exception)
        {
            // do nothing
        }

        Debug.WriteLine($"New Group Created: {newGroupName}");
    }

    private void MenuGroupRename_Click(object? sender, EventArgs? e)
    {
        var dgv = CustomDataGridViewGroups;
        int currentRow = dgv.SelectedCells[0].RowIndex;
        string? groupName = dgv.Rows[currentRow].Cells[1].Value.ToString();
        if (string.IsNullOrEmpty(groupName)) return;
        string groupNameOld = groupName, groupNameNew = groupName;

        switch (CustomInputBox.Show(this, ref groupNameNew, "New Group Name:", false, "Rename group"))
        {
            case DialogResult.OK:
                break;
            case DialogResult.Cancel:
                return;
            default:
                return;
        }

        groupNameNew = groupNameNew.Trim();

        if (string.IsNullOrEmpty(groupNameNew) || string.IsNullOrWhiteSpace(groupNameNew))
        {
            string msg = "Name cannot be empty or white space.";
            CustomMessageBox.Show(this, msg, "Message");
            return;
        }

        if (groupNameNew.ToLower().Equals("msasanmh"))
        {
            string msg = $"\"{groupNameNew}\" is predefined, choose another name.";
            CustomMessageBox.Show(this, msg, "Message");
            return;
        }

        if (ListGroupNames.Contains(groupNameNew))
        {
            string msg = $"\"{groupNameNew}\" is already exist, choose another name.";
            CustomMessageBox.Show(this, msg, "Message");
            return;
        }

        RenameGroup(XDoc, groupNameOld, groupNameNew);

        // Refresh Groups
        ReadGroups(groupNameNew, false);

        // Save xDocument to File
        try
        {
            XDoc.Save(CustomServersXmlPath, SaveOptions.None);
        }
        catch (Exception)
        {
            // do nothing
        }

        Debug.WriteLine($"Group Renamed From \"{groupNameOld}\" To \"{groupNameNew}\"");
    }

    private void MenuGroupRemove_Click(object? sender, EventArgs? e)
    {
        var dgv = CustomDataGridViewGroups;

        if (dgv.SelectedCells.Count < 1) return;

        int currentRow = dgv.SelectedCells[0].RowIndex;
        string? groupName = dgv.Rows[currentRow].Cells[1].Value.ToString();
        if (string.IsNullOrEmpty(groupName)) return;

        RemoveGroup(groupName);

        // Save xDocument to File
        try
        {
            XDoc.Save(CustomServersXmlPath, SaveOptions.None);
        }
        catch (Exception)
        {
            // do nothing
        }

        Debug.WriteLine($"Group Removed: {groupName}");
    }

    private void MenuGroupMoveUp_Click(object? sender, EventArgs? e)
    {
        var dgv = CustomDataGridViewGroups;

        if (dgv.SelectedCells.Count < 1) return;

        int currentRow = dgv.SelectedCells[0].RowIndex;
        string? groupName = dgv.Rows[currentRow].Cells[1].Value.ToString();
        if (string.IsNullOrEmpty(groupName)) return;

        // Find Current Group
        if (XDoc.Root == null) return;
        var nodesGroups = XDoc.Root.Elements().Elements();
        XElement currentElement = new("msasanmh");
        for (int n = 0; n < nodesGroups.Count(); n++)
        {
            XElement nodeG = nodesGroups.ToList()[n];
            XElement? nodeGName = nodeG.Element("Name");
            if (nodeGName != null && nodeGName.Value == groupName)
            {
                currentElement = nodeG;
                break;
            }
        }

        // Find Previous Group Name
        int index = ListGroupNames.FindIndex(a => a.Equals(groupName));
        if (index == -1 || index == 0) return;
        string previousGroup = ListGroupNames[index - 1];

        // Find Previous Location
        XElement newLocation = new("msasanmh");
        for (int n = 0; n < nodesGroups.Count(); n++)
        {
            XElement nodeG = nodesGroups.ToList()[n];
            XElement? nodeGName = nodeG.Element("Name");
            if (nodeGName != null && nodeGName.Value == previousGroup)
            {
                newLocation = nodeG;
                break;
            }
        }

        if (currentElement.Name == "msasanmh" || newLocation.Name == "msasanmh") return;

        // Remove Current Group
        currentElement.Remove();

        // Copy to New Location
        newLocation.AddBeforeSelf(currentElement);

        // Refresh Groups
        ReadGroups(groupName, true);

        // Save xDocument to File
        try
        {
            XDoc.Save(CustomServersXmlPath, SaveOptions.None);
        }
        catch (Exception)
        {
            // do nothing
        }

        Debug.WriteLine($"Group Moved Up: {groupName}");
    }

    private void MenuGroupMoveDown_Click(object? sender, EventArgs? e)
    {
        var dgv = CustomDataGridViewGroups;

        if (dgv.SelectedCells.Count < 1) return;

        int currentRow = dgv.SelectedCells[0].RowIndex;
        string? groupName = dgv.Rows[currentRow].Cells[1].Value.ToString();
        if (string.IsNullOrEmpty(groupName)) return;

        // Find Current Group
        if (XDoc.Root == null) return;
        var nodesGroups = XDoc.Root.Elements().Elements();
        XElement currentElement = new("msasanmh");
        for (int n = 0; n < nodesGroups.Count(); n++)
        {
            XElement nodeG = nodesGroups.ToList()[n];
            XElement? nodeGName = nodeG.Element("Name");
            if (nodeGName != null && nodeGName.Value == groupName)
            {
                currentElement = nodeG;
                break;
            }
        }

        // Find Next Group Name
        int index = ListGroupNames.FindIndex(a => a.Equals(groupName));
        if (index == -1 || index + 1 >= ListGroupNames.Count) return;
        string nextGroup = ListGroupNames[index + 1];

        // Find Previous Location
        XElement newLocation = new("msasanmh");
        for (int n = 0; n < nodesGroups.Count(); n++)
        {
            XElement nodeG = nodesGroups.ToList()[n];
            XElement? nodeGName = nodeG.Element("Name");
            if (nodeGName != null && nodeGName.Value == nextGroup)
            {
                newLocation = nodeG;
                break;
            }
        }

        if (currentElement.Name == "msasanmh" || newLocation.Name == "msasanmh") return;

        // Remove Current Group
        currentElement.Remove();

        // Copy to New Location
        newLocation.AddAfterSelf(currentElement);

        // Refresh Groups
        ReadGroups(groupName, true);

        // Save xDocument to File
        try
        {
            XDoc.Save(CustomServersXmlPath, SaveOptions.None);
        }
        catch (Exception)
        {
            // do nothing
        }

        Debug.WriteLine($"Group Moved Down: {groupName}");
    }

    private void MenuGroupMoveToTop_Click(object? sender, EventArgs? e)
    {
        var dgv = CustomDataGridViewGroups;

        if (dgv.SelectedCells.Count < 1) return;

        int currentRow = dgv.SelectedCells[0].RowIndex;
        string? groupName = dgv.Rows[currentRow].Cells[1].Value.ToString();
        if (string.IsNullOrEmpty(groupName)) return;

        // Find Current Group
        if (XDoc.Root == null) return;
        var nodesGroups = XDoc.Root.Elements().Elements();
        XElement currentElement = new("msasanmh");
        for (int a = 0; a < nodesGroups.Count(); a++)
        {
            XElement nodeG = nodesGroups.ToList()[a];
            XElement? nodeGName = nodeG.Element("Name");
            if (nodeGName != null && nodeGName.Value == groupName)
            {
                currentElement = nodeG;
                break;
            }
        }

        // Find Top Group Name
        int index = ListGroupNames.FindIndex(a => a.Equals(groupName));
        if (index == -1 || index == 0)
        {
            return;
        }
        string topGroup = ListGroupNames[0];

        // Find Top Location
        XElement newLocation = new("msasanmh");
        for (int a = 0; a < nodesGroups.Count(); a++)
        {
            XElement nodeG = nodesGroups.ToList()[a];
            XElement? nodeGName = nodeG.Element("Name");
            if (nodeGName != null && nodeGName.Value == topGroup)
            {
                newLocation = nodeG;
                break;
            }
        }

        if (currentElement.Name == "msasanmh" || newLocation.Name == "msasanmh") return;

        // Remove Current Group
        currentElement.Remove();

        // Copy to New Location
        newLocation.AddBeforeSelf(currentElement);

        // Refresh Groups
        ReadGroups(groupName, false);

        // Save xDocument to File
        try
        {
            XDoc.Save(CustomServersXmlPath, SaveOptions.None);
        }
        catch (Exception)
        {
            // do nothing
        }

        Debug.WriteLine($"Group Moved To Top: {groupName}");
    }

    private void MenuGroupMoveToBottom_Click(object? sender, EventArgs? e)
    {
        var dgv = CustomDataGridViewGroups;

        if (dgv.SelectedCells.Count < 1) return;

        int currentRow = dgv.SelectedCells[0].RowIndex;
        string? groupName = dgv.Rows[currentRow].Cells[1].Value.ToString();
        if (string.IsNullOrEmpty(groupName)) return;

        // Find Current Group
        if (XDoc.Root == null) return;
        var nodesGroups = XDoc.Root.Elements().Elements();
        XElement currentElement = new("msasanmh");
        for (int n = 0; n < nodesGroups.Count(); n++)
        {
            XElement nodeG = nodesGroups.ToList()[n];
            XElement? nodeGName = nodeG.Element("Name");
            if (nodeGName != null && nodeGName.Value == groupName)
            {
                currentElement = nodeG;
                break;
            }
        }

        // Find Bottom Group Name
        int index = ListGroupNames.FindIndex(a => a.Equals(groupName));
        if (index == -1 || index + 1 >= ListGroupNames.Count) return;
        string bottomGroup = ListGroupNames[^1];

        // Find Bottom Location
        XElement newLocation = new("msasanmh");
        for (int n = 0; n < nodesGroups.Count(); n++)
        {
            XElement nodeG = nodesGroups.ToList()[n];
            XElement? nodeGName = nodeG.Element("Name");
            if (nodeGName != null && nodeGName.Value == bottomGroup)
            {
                newLocation = nodeG;
                break;
            }
        }

        if (currentElement.Name == "msasanmh" || newLocation.Name == "msasanmh") return;

        // Remove Current Group
        currentElement.Remove();

        // Copy to New Location
        newLocation.AddAfterSelf(currentElement);

        // Refresh Groups
        ReadGroups(groupName, false);

        // Save xDocument to File
        try
        {
            XDoc.Save(CustomServersXmlPath, SaveOptions.None);
        }
        catch (Exception)
        {
            // do nothing
        }

        Debug.WriteLine($"Group Moved To Buttom: {groupName}");
    }

    private void MenuGroupImport_Click(object? sender, EventArgs? e)
    {
        using OpenFileDialog ofd = new();
        ofd.Filter = "Custom DNS Servers (*.sdcs)|*.sdcs|Custom DNS Servers (*.xml)|*.xml";
        ofd.DefaultExt = ".sdcs";
        ofd.AddExtension = true;
        ofd.RestoreDirectory = true;

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            string filePath = ofd.FileName;

            if (!XmlTool.IsValidXML(File.ReadAllText(filePath)))
            {
                CustomMessageBox.Show(this, "XML file is not valid.", "Not Valid", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            XDocument importedReplaceList = new();
            importedReplaceList = XDocument.Load(filePath);

            if (importedReplaceList.Root == null) return;
            var nodesGroups = importedReplaceList.Root.Elements().Elements();

            if (!nodesGroups.Any())
            {
                CustomMessageBox.Show(this, "XML file has no groups.", "No Groups", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Form Import = new()
            {
                Size = new(400, Height - 100),
                Text = "Import...",
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.CenterParent,
                AutoScaleMode = AutoScaleMode.Dpi,
                AutoSizeMode = AutoSizeMode.GrowOnly
            };

            Label text = new()
            {
                Text = "Choose groups to import",
                AutoSize = true,
                Location = new(5, 5)
            };
            Import.Controls.Add(text);
            int textHeight = text.GetPreferredSize(Size.Empty).Height;

            // Form Width
            Import.Width = TextRenderer.MeasureText(text.Text, Font).Width + 150;

            CustomButton buttonCancel = new()
            {
                AutoSize = true,
                Text = "Cancel",
                RoundedCorners = 5,
                DialogResult = DialogResult.Cancel,
            };
            buttonCancel.Location = new(Import.ClientRectangle.Width - buttonCancel.Width - 5, Import.ClientRectangle.Height - buttonCancel.Height - (buttonCancel.Height / 2));
            Import.Controls.Add(buttonCancel);

            CustomButton buttonOK = new()
            {
                AutoSize = true,
                Text = "OK",
                RoundedCorners = 5,
                DialogResult = DialogResult.OK,
            };
            buttonOK.Location = new(Import.ClientRectangle.Width - buttonOK.Width - 5 - buttonCancel.Width - 5, Import.ClientRectangle.Height - buttonOK.Height - (buttonOK.Height / 2));
            Import.Controls.Add(buttonOK);

            CustomPanel panel = new()
            {
                AutoScroll = true,
                Border = BorderStyle.FixedSingle,
                ButtonBorderStyle = ButtonBorderStyle.Solid,
                Location = new(5, 10 + textHeight),
                Width = Import.ClientRectangle.Width - 10,
                Height = Import.ClientRectangle.Height - textHeight - buttonCancel.Height - 20
            };
            Import.Controls.Add(panel);

            // Modify Buttons Location
            buttonCancel.Top = panel.Bottom + 5;
            buttonOK.Top = buttonCancel.Top;

            // Measure Text Height Based On Font
            int labelScreenHeight = TextRenderer.MeasureText("MSasanMH", Font).Height;
            labelScreenHeight += labelScreenHeight / 2;

            for (int n = 0; n < nodesGroups.Count(); n++)
            {
                XElement nodeG = nodesGroups.ToList()[n];
                XElement? nodeGName = nodeG.Element("Name");
                if (nodeGName == null) continue;
                string group = nodeGName.Value;

                CustomCheckBox box = new();
                box.Checked = true;
                box.Text = group;
                box.Location = new(5, (labelScreenHeight / 2) + (n * labelScreenHeight));
                panel.Controls.Add(box);
                box.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        ImportMenu(e.X + 5, box.Location.Y + box.GetPreferredSize(Size.Empty).Height / 3);
                    }
                };
            }

            panel.SetDarkControl();

            buttonOK.Click -= ButtonOK_Click;
            buttonOK.Click += ButtonOK_Click;

            void ButtonOK_Click(object? sender, EventArgs e)
            {
                if (XDoc.Root == null) return;
                XElement? insert = XDoc.Root.Element("CustomDnsList");
                if (insert == null) return;

                string firstGroup = string.Empty;
                bool once = true;
                int count = 0;
                int countName = 1;

                for (int n = 0; n < panel.Controls.Count; n++)
                {
                    Control c = panel.Controls[n];
                    foreach (CustomCheckBox ch in c.Controls.OfType<CustomCheckBox>())
                    {
                        if (ch.Checked)
                        {
                            string? group = ch.Text;
                            if (group == null) continue;

                            // Check for Duplicate Name
                            while (ListGroupNames.GetIndex(group) != -1)
                            {
                                // Rename Variable
                                string groupNew = string.Format("{0} ({1})", ch.Text, countName++);
                                // Rename Group in Imported xDoc
                                RenameGroup(importedReplaceList, group, groupNew);
                                group = groupNew;
                            }

                            count++;
                            if (once)
                            {
                                firstGroup = group;
                                once = false;
                            }
                            insert.Add(GetXmlGroupByName(importedReplaceList, group));
                            countName = 1;
                        }
                    }
                }

                if (!XDoc.Root.Elements().Elements().Any()) return;
                if (count == 0) return;

                ReadGroups(firstGroup, false);

                // Save xDocument to File
                try
                {
                    XDoc.Save(CustomServersXmlPath, SaveOptions.None);
                }
                catch (Exception)
                {
                    // do nothing
                }
            }

            // Context Menu Import (Selection)
            panel.MouseDown -= Panel_MouseDown;
            panel.MouseDown += Panel_MouseDown;
            void Panel_MouseDown(object? sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                {
                    ImportMenu(e.X, e.Y);
                }
            }

            void ImportMenu(int eX, int eY)
            {
                CustomContextMenuStrip MGE = new();
                MGE.Font = Font;

                ToolStripMenuItem MenuSA = new();
                MenuSA.Font = Font;
                MenuSA.Text = "Select All";
                MenuSA.Click -= MenuSA_Click;
                MenuSA.Click += MenuSA_Click;
                MGE.Items.Add(MenuSA);

                ToolStripMenuItem MenuIS = new();
                MenuIS.Font = Font;
                MenuIS.Text = "Invert selection";
                MenuIS.Click -= MenuIS_Click;
                MenuIS.Click += MenuIS_Click;
                MGE.Items.Add(MenuIS);

                Theme.SetColors(MGE);
                MGE.Show(panel, new Point(eX, eY));
            }

            void MenuSA_Click(object? sender, EventArgs e)
            {
                for (int a = 0; a < panel.Controls.Count; a++)
                {
                    Control c = panel.Controls[a];
                    foreach (CustomCheckBox ch in c.Controls.OfType<CustomCheckBox>().Reverse())
                    {
                        ch.Checked = true;
                    }
                }
            }

            void MenuIS_Click(object? sender, EventArgs e)
            {
                for (int a = 0; a < panel.Controls.Count; a++)
                {
                    Control c = panel.Controls[a];
                    foreach (CustomCheckBox ch in c.Controls.OfType<CustomCheckBox>().Reverse())
                    {
                        ch.Checked = !ch.Checked;
                    }
                }
            }

            Import.AcceptButton = buttonOK;
            Import.CancelButton = buttonCancel;

            // Fix Screen DPI
            Import.Font = Font;
            ScreenDPI.FixDpiAfterInitializeComponent(Import);

            Theme.LoadTheme(Import, Theme.Themes.Dark);
            Import.ShowDialog(this);
        }
    }

    private void MenuGroupExport_Click(object? sender, EventArgs? e)
    {
        if (CustomDataGridViewGroups.Rows.Count == 0) return;

        Form Export = new()
        {
            Size = new(400, Height - 100),
            Text = "Export...",
            FormBorderStyle = FormBorderStyle.FixedToolWindow,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.CenterParent,
            AutoScaleMode = AutoScaleMode.Dpi,
            AutoSizeMode = AutoSizeMode.GrowOnly
        };

        Label text = new()
        {
            Text = "Choose groups to export",
            AutoSize = true,
            Location = new(5, 5)
        };
        Export.Controls.Add(text);
        int textHeight = text.GetPreferredSize(Size.Empty).Height;

        // Form Width
        Export.Width = TextRenderer.MeasureText(text.Text, Font).Width + 150;

        CustomButton buttonCancel = new()
        {
            AutoSize = true,
            Text = "Cancel",
            RoundedCorners = 5,
            DialogResult = DialogResult.Cancel,
        };
        buttonCancel.Location = new(Export.ClientRectangle.Width - buttonCancel.Width - 5, Export.ClientRectangle.Height - buttonCancel.Height - (buttonCancel.Height / 2));
        Export.Controls.Add(buttonCancel);

        CustomButton buttonOK = new()
        {
            AutoSize = true,
            Text = "OK",
            RoundedCorners = 5,
            DialogResult = DialogResult.OK,
        };
        buttonOK.Location = new(Export.ClientRectangle.Width - buttonOK.Width - 5 - buttonCancel.Width - 5, Export.ClientRectangle.Height - buttonOK.Height - (buttonOK.Height / 2));
        Export.Controls.Add(buttonOK);

        CustomPanel panel = new()
        {
            AutoScroll = true,
            Border = BorderStyle.FixedSingle,
            ButtonBorderStyle = ButtonBorderStyle.Solid,
            Location = new(5, 10 + textHeight),
            Width = Export.ClientRectangle.Width - 10,
            Height = Export.ClientRectangle.Height - textHeight - buttonCancel.Height - 20
        };
        Export.Controls.Add(panel);

        // Modify Buttons Location
        buttonCancel.Top = panel.Bottom + 5;
        buttonOK.Top = buttonCancel.Top;

        // Measure Text Height Based On Font
        int labelScreenHeight = TextRenderer.MeasureText("MSasanMH", Font).Height;
        labelScreenHeight += labelScreenHeight / 2;

        for (int n = 0; n < ListGroupNames.Count; n++)
        {
            string group = ListGroupNames[n];
            CustomCheckBox box = new();
            box.Checked = true;
            box.Text = group;
            box.Location = new(5, (labelScreenHeight / 2) + (n * labelScreenHeight));
            panel.Controls.Add(box);
            box.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    ExportMenu(e.X + 5, box.Location.Y + box.GetPreferredSize(Size.Empty).Height / 3);
                }
            };
        }

        panel.SetDarkControl();

        buttonOK.Click -= ButtonOK_Click;
        buttonOK.Click += ButtonOK_Click;

        void ButtonOK_Click(object? sender, EventArgs e)
        {
            List<int> ints = new();
            for (int a = 0; a < panel.Controls.Count; a++)
            {
                Control c = panel.Controls[a];
                foreach (CustomCheckBox ch in c.Controls.OfType<CustomCheckBox>().Reverse())
                {
                    if (ch.Checked)
                    {
                        string? group = ch.Text;
                        if (group == null) continue;

                        ints.Add(ListGroupNames.GetIndex(group));
                    }
                }
            }

            XDocument doc = CreateXmlCS();
            if (doc.Root == null) return;
            XElement? insert = doc.Root.Element("CustomDnsList");
            if (insert == null) return;

            ints.Sort();
            for (int b = 0; b < ints.Count; b++)
            {
                int i = ints[b];
                insert.Add(GetXmlGroupByName(XDoc, ListGroupNames[i]));
            }

            using SaveFileDialog sfd = new();
            sfd.Filter = "Custom DNS Servers (*.sdcs)|*.sdcs|Custom DNS Servers (*.xml)|*.xml";
            sfd.DefaultExt = ".sdcs";
            sfd.AddExtension = true;
            sfd.RestoreDirectory = true;
            sfd.FileName = "sdc_custom_servers";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    doc.Save(sfd.FileName, SaveOptions.None);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Export: " + ex.Message);
                    CustomMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Context Menu Export (Selection)
        panel.MouseDown -= Panel_MouseDown;
        panel.MouseDown += Panel_MouseDown;
        void Panel_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ExportMenu(e.X, e.Y);
            }
        }

        void ExportMenu(int eX, int eY)
        {
            CustomContextMenuStrip MGE = new();
            MGE.Font = Font;

            ToolStripMenuItem MenuSA = new();
            MenuSA.Font = Font;
            MenuSA.Text = "Select All";
            MenuSA.Click -= MenuSA_Click;
            MenuSA.Click += MenuSA_Click;
            MGE.Items.Add(MenuSA);

            ToolStripMenuItem MenuIS = new();
            MenuIS.Font = Font;
            MenuIS.Text = "Invert selection";
            MenuIS.Click -= MenuIS_Click;
            MenuIS.Click += MenuIS_Click;
            MGE.Items.Add(MenuIS);

            Theme.SetColors(MGE);
            MGE.Show(panel, new Point(eX, eY));
        }

        void MenuSA_Click(object? sender, EventArgs e)
        {
            for (int a = 0; a < panel.Controls.Count; a++)
            {
                Control c = panel.Controls[a];
                foreach (CustomCheckBox ch in c.Controls.OfType<CustomCheckBox>().Reverse())
                {
                    ch.Checked = true;
                }
            }
        }

        void MenuIS_Click(object? sender, EventArgs e)
        {
            for (int n = 0; n < panel.Controls.Count; n++)
            {
                Control c = panel.Controls[n];
                foreach (CustomCheckBox ch in c.Controls.OfType<CustomCheckBox>().Reverse())
                {
                    ch.Checked = !ch.Checked;
                }
            }
        }

        Export.AcceptButton = buttonOK;
        Export.CancelButton = buttonCancel;

        // Fix Screen DPI
        Export.Font = Font;
        ScreenDPI.FixDpiAfterInitializeComponent(Export);

        Theme.LoadTheme(Export, Theme.Themes.Dark);
        Export.ShowDialog(this);
    }

    //======================================= DNSs ======================================

    private void CustomDataGridViewDNSs_SelectionChanged(object sender, EventArgs e)
    {
        var dgvG = CustomDataGridViewGroups;
        var dgvS = CustomDataGridViewDNSs;

        if (dgvG.RowCount == 0) return;
        if (dgvG.SelectedCells.Count <= 0) return;
        if (dgvS.RowCount == 0) return;
        if (dgvS.SelectedCells.Count <= 0) return;

        int currentRow = dgvS.SelectedCells[0].RowIndex;
        string? dns = dgvS.Rows[currentRow].Cells[1].Value.ToString();
        string? description = dgvS.Rows[currentRow].Cells[2].Value.ToString();

        int currentGRow = dgvG.SelectedCells[0].RowIndex;
        if (dgvG.Rows[currentGRow].Cells[1].Value == null) return;
        string? group = dgvG.Rows[currentGRow].Cells[1].Value.ToString();
        if (string.IsNullOrEmpty(group)) group = "Null";
        int dnsNo = currentRow + 1;
        string dnsNoMsg = $" - DNS No: {dnsNo}";
        string msg = $"DNSs for group \"{group}\"{dnsNoMsg}";
        CustomGroupBoxDNSs.Text = msg;

        if (dgvS.SelectedRows.Count > 1)
        {
            CustomButtonAddServers.Enabled = false;
            CustomButtonModifyDNS.Enabled = false;
        }
        else
        {
            CustomButtonAddServers.Enabled = true;
            CustomButtonModifyDNS.Enabled = true;
        }

        CustomTextBoxDNS.Text = !string.IsNullOrEmpty(dns) ? dns : string.Empty;
        CustomTextBoxDescription.Text = !string.IsNullOrEmpty(description) ? description : string.Empty;
    }

    private void CustomDataGridViewDNSs_KeyDown(object sender, KeyEventArgs e)
    {
        // Assign Menu Shortcuts to KeyDown (Use Shortcuts When Menu is Not Displayed)
        CreateMenuDNS();
        void checkShortcut(ToolStripMenuItem item)
        {
            if (item.ShortcutKeys == e.KeyData)
            {
                //item.PerformClick(); // Doesn't work correctly
                if (item.Text.Contains("Remove"))
                    MenuDnsRemove_Click(null, null);
                else if (item.Text.Contains("Move up"))
                    MenuDnsMoveUp_Click(null, null);
                else if (item.Text.Contains("Move down"))
                    MenuDnsMoveDown_Click(null, null);
                else if (item.Text.Contains("Move to top"))
                    MenuDnsMoveToTop_Click(null, null);
                else if (item.Text.Contains("Move to bottom"))
                    MenuDnsMoveToBottom_Click(null, null);
                else if (item.Text.Contains("Import"))
                    MenuDnsImport_Click(null, null);
                else if (item.Text.Equals("Export"))
                    MenuDnsExport_Click(null, null);
                else if (item.Text.Equals("Export as text"))
                    MenuDnsExportAsText_Click(null, null);
                else
                    item.PerformClick();
                return;
            }

            foreach (ToolStripMenuItem child in item.DropDownItems.OfType<ToolStripMenuItem>())
            {
                checkShortcut(child);
            }
        }

        foreach (ToolStripMenuItem item in MD.Items.OfType<ToolStripMenuItem>())
        {
            checkShortcut(item);
        }

        // Make ContextMenu Shortcuts Work (Move up and Move Down)
        if (e.Control && e.KeyCode == Keys.Up || e.Control && e.KeyCode == Keys.Down)
        {
            e.SuppressKeyPress = true;
        }
    }

    private void CustomDataGridViewDNSs_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        // Save CheckBox State for DNSs
        var dgvG = CustomDataGridViewGroups;
        var dgvS = CustomDataGridViewDNSs;
        if (dgvG.Rows.Count == 0) return;
        if (dgvS.Rows.Count == 0) return;
        if (e.ColumnIndex != 0) return;

        int groupIndex = dgvG.SelectedCells[0].RowIndex;
        string? groupName = dgvG.Rows[groupIndex].Cells[1].Value.ToString();
        if (string.IsNullOrEmpty(groupName)) return;

        if (XDoc.Root == null) return;
        var nodesGroups = XDoc.Root.Elements().Elements();

        for (int a = 0; a < nodesGroups.Count(); a++)
        {
            XElement nodeG = nodesGroups.ToList()[a];
            XElement? nodeGName = nodeG.Element("Name");
            if (nodeGName != null && groupName == nodeGName.Value)
            {
                int count = nodeG.Elements("DnsItem").Count();
                for (int b = 0; b < count; b++)
                {
                    if (b == e.RowIndex)
                    {
                        XElement nodeItem = nodeG.Elements("DnsItem").ToList()[b];
                        XElement? nodeItemEnabled = nodeItem.Element("Enabled");
                        string? isItemEnabled = dgvS.Rows[b].Cells[0].Value.ToString();

                        if (nodeItemEnabled == null || string.IsNullOrEmpty(isItemEnabled)) return;
                        nodeItemEnabled.Value = isItemEnabled;

                        // Save xDocument to File
                        try
                        {
                            XDoc.Save(CustomServersXmlPath, SaveOptions.None);
                        }
                        catch (Exception)
                        {
                            // do nothing
                        }

                        break;
                    }
                }
            }
        }
    }

    private void CustomDataGridViewDNSs_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        // Scan DNS on Double Click
        var dgvG = CustomDataGridViewGroups;
        if (dgvG.RowCount == 0) return;
        if (dgvG.SelectedRows.Count == 0) return;
        var dgvS = CustomDataGridViewDNSs;
        dgvS.Select(); // Set Focus on Control

        // If it's header return
        if (e.RowIndex < 0) return;

        // If it's not DNS column return
        if (e.ColumnIndex != 1) return;

        string? dns = dgvS.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

        if (!string.IsNullOrEmpty(dns))
        {
            FormDnsScanner formDnsScanner = new(dns);
            formDnsScanner.FormClosing += (s, e) => { formDnsScanner.Dispose(); };
            formDnsScanner.ShowDialog(this);
        }
    }

    private void CustomDataGridViewDNSs_MouseDown(object sender, MouseEventArgs e)
    {
        // Context Menu
        if (e.Button == MouseButtons.Right)
        {
            var dgvG = CustomDataGridViewGroups;
            if (dgvG.RowCount == 0) return;
            if (dgvG.SelectedRows.Count == 0) return;
            var dgvS = CustomDataGridViewDNSs;
            dgvS.Select(); // Set Focus on Control

            int currentMouseOverRow = dgvS.HitTest(e.X, e.Y).RowIndex;

            // Disable MultiSelect by RightClick
            if (currentMouseOverRow != -1)
            {
                if (dgvS.Rows[currentMouseOverRow].Selected == false)
                {
                    dgvS.ClearSelection();
                    dgvS.Rows[currentMouseOverRow].Cells[0].Selected = true;
                    dgvS.Rows[currentMouseOverRow].Selected = true;
                }

                dgvS.Rows[currentMouseOverRow].Cells[0].Selected = true;
                dgvS.Rows[currentMouseOverRow].Selected = true;
            }

            CreateMenuDNS();

            int[] itemRows = new int[dgvS.SelectedRows.Count];
            for (int a = 0; a < dgvS.SelectedRows.Count; a++)
            {
                itemRows[a] = dgvS.SelectedRows[a].Index;
            }
            Array.Sort(itemRows);

            int firstSelectedCell;
            if (itemRows.Length == 0)
                firstSelectedCell = -1;
            else
                firstSelectedCell = itemRows[0];

            int totalRows = dgvS.Rows.Count;
            int totalSelectedRows = dgvS.SelectedRows.Count;

            // Remove Menus On Conditions
            showMenu1();

            void showMenu1()
            {
                if (totalRows == 0)
                {
                    MD.Items.Remove(MenuDnsRemove);
                    MD.Items.Remove(MenuDnsRemoveAll);
                    MD.Items.Remove(MenuDnsMoveSelectedToGroup);
                    MD.Items.Remove(MenuDnsMoveUp);
                    MD.Items.Remove(MenuDnsMoveDown);
                    MD.Items.Remove(MenuDnsMoveToTop);
                    MD.Items.Remove(MenuDnsMoveToBottom);
                    MD.Items.Remove(MenuDnsExport);
                    MD.Items.Remove(MenuDnsExportAsText);
                    MD.Items.Remove(MenuDnsSelectAll);
                    MD.Items.Remove(MenuDnsInvertSelection);
                    MD.Items.RemoveAt(0);
                    MD.Items.RemoveAt(0);
                    MD.Items.RemoveAt(0);
                    MD.Items.RemoveAt(1);
                }
                else if (totalRows == 1)
                {
                    if (totalSelectedRows <= 0)
                    {
                        MD.Items.Remove(MenuDnsRemove);
                        MD.Items.Remove(MenuDnsMoveSelectedToGroup);
                        MD.Items.RemoveAt(1);
                        MD.Items.Remove(MenuDnsMoveUp);
                        MD.Items.Remove(MenuDnsMoveDown);
                        MD.Items.Remove(MenuDnsMoveToTop);
                        MD.Items.Remove(MenuDnsMoveToBottom);
                        MD.Items.RemoveAt(1);
                    }
                    else //if (totalSelectedRows = 1)
                    {
                        MD.Items.Remove(MenuDnsMoveUp);
                        MD.Items.Remove(MenuDnsMoveDown);
                        MD.Items.Remove(MenuDnsMoveToTop);
                        MD.Items.Remove(MenuDnsMoveToBottom);
                        MD.Items.RemoveAt(4);
                    }
                }
                else // if (totalRows > 1)
                {
                    if (totalSelectedRows <= 0)
                    {
                        MD.Items.Remove(MenuDnsRemove);
                        MD.Items.Remove(MenuDnsMoveSelectedToGroup);
                        MD.Items.RemoveAt(1);
                        MD.Items.Remove(MenuDnsMoveUp);
                        MD.Items.Remove(MenuDnsMoveDown);
                        MD.Items.Remove(MenuDnsMoveToTop);
                        MD.Items.Remove(MenuDnsMoveToBottom);
                        MD.Items.RemoveAt(1);
                    }
                    else //if (totalSelectedRows > 1)
                    {
                        movesMenus();
                    }
                }
            }

            void movesMenus()
            {
                if (currentMouseOverRow == 0)
                {
                    MD.Items.Remove(MenuDnsMoveUp);
                    MD.Items.Remove(MenuDnsMoveToTop);
                }
                else if (currentMouseOverRow == totalRows - 1)
                {
                    MD.Items.Remove(MenuDnsMoveDown);
                    MD.Items.Remove(MenuDnsMoveToBottom);
                }

                if (totalSelectedRows == totalRows)
                {
                    MD.Items.Remove(MenuDnsMoveUp);
                    MD.Items.Remove(MenuDnsMoveToTop);
                    MD.Items.Remove(MenuDnsMoveDown);
                    MD.Items.Remove(MenuDnsMoveToBottom);
                    MD.Items.RemoveAt(4);
                }
                else if (firstSelectedCell - 1 < 0)
                {
                    MD.Items.Remove(MenuDnsMoveUp);
                    MD.Items.Remove(MenuDnsMoveToTop);
                }
                else if (firstSelectedCell > dgvS.RowCount - 1 - dgvS.SelectedRows.Count)
                {
                    MD.Items.Remove(MenuDnsMoveDown);
                    MD.Items.Remove(MenuDnsMoveToBottom);
                }

                // Remove MovesMenus If Rows are not selected in order
                if (totalRows > 2)
                {
                    for (int a = 0; a < itemRows.Length; a++)
                    {
                        if (a != 0)
                        {
                            if (itemRows[a - 1] + 1 != itemRows[a])
                            {
                                MD.Items.Remove(MenuDnsMoveUp);
                                MD.Items.Remove(MenuDnsMoveToTop);
                                MD.Items.Remove(MenuDnsMoveDown);
                                MD.Items.Remove(MenuDnsMoveToBottom);
                                MD.Items.RemoveAt(4);
                                break;
                            }
                        }
                    }
                }
            }

            Theme.SetColors(MD);
            MD.RoundedCorners = 5;
            MD.Show(dgvS, new Point(e.X, e.Y));
        }
    }

    private void CreateMenuDNS()
    {
        var dgvG = CustomDataGridViewGroups;
        if (dgvG.Rows.Count == 0) return;

        int groupIndex = dgvG.SelectedCells[0].RowIndex;
        string? groupName = dgvG.Rows[groupIndex].Cells[1].Value.ToString();

        // Context Menu
        MD.Items.Clear();
        MD.Font = Font;

        MenuDnsRemove.Font = Font;
        MenuDnsRemove.Text = "Remove";
        MenuDnsRemove.ShortcutKeys = Keys.Delete;
        MenuDnsRemove.Click -= MenuDnsRemove_Click;
        MenuDnsRemove.Click += MenuDnsRemove_Click;
        MD.Items.Add(MenuDnsRemove);

        MenuDnsRemoveAll.Font = Font;
        MenuDnsRemoveAll.Text = "Remove all";
        MenuDnsRemoveAll.Click -= MenuDnsRemoveAll_Click;
        MenuDnsRemoveAll.Click += MenuDnsRemoveAll_Click;
        MD.Items.Add(MenuDnsRemoveAll);

        MD.Items.Add("-");

        MenuDnsMoveSelectedToGroup.DropDownItems.Clear();
        MenuDnsMoveSelectedToGroup.Font = Font;
        MenuDnsMoveSelectedToGroup.Text = "Move selected DNSs to group";
        MD.Items.Add(MenuDnsMoveSelectedToGroup);

        // SubMenu for Move Selected DNSs To Group
        List<string> ListGroupsToSend = new(ListGroupNames);
        if (!string.IsNullOrEmpty(groupName))
            ListGroupsToSend.Remove(groupName); // Remove Current Group From Menu
        ToolStripItem[] subMenuItems = new ToolStripItem[ListGroupsToSend.Count];
        for (int n = 0; n < ListGroupsToSend.Count; n++)
        {
            string groupNameToSend = ListGroupsToSend[n];
            subMenuItems[n] = new ToolStripMenuItem(groupNameToSend);
            subMenuItems[n].Font = Font;
            subMenuItems[n].Click -= MenuDnsMoveSelectedToGroup_Click;
            subMenuItems[n].Click += MenuDnsMoveSelectedToGroup_Click;
        }
        MenuDnsMoveSelectedToGroup.DropDownItems.AddRange(subMenuItems);

        MD.Items.Add("-");

        MenuDnsMoveUp.Font = Font;
        MenuDnsMoveUp.Text = "Move up";
        MenuDnsMoveUp.ShortcutKeys = Keys.Control | Keys.Up;
        MenuDnsMoveUp.Click -= MenuDnsMoveUp_Click;
        MenuDnsMoveUp.Click += MenuDnsMoveUp_Click;
        MD.Items.Add(MenuDnsMoveUp);

        MenuDnsMoveDown.Font = Font;
        MenuDnsMoveDown.Text = "Move down";
        MenuDnsMoveDown.ShortcutKeys = Keys.Control | Keys.Down;
        MenuDnsMoveDown.Click -= MenuDnsMoveDown_Click;
        MenuDnsMoveDown.Click += MenuDnsMoveDown_Click;
        MD.Items.Add(MenuDnsMoveDown);

        MenuDnsMoveToTop.Font = Font;
        MenuDnsMoveToTop.Text = "Move to top";
        MenuDnsMoveToTop.ShortcutKeys = Keys.Control | Keys.Home;
        MenuDnsMoveToTop.Click -= MenuDnsMoveToTop_Click;
        MenuDnsMoveToTop.Click += MenuDnsMoveToTop_Click;
        MD.Items.Add(MenuDnsMoveToTop);

        MenuDnsMoveToBottom.Font = Font;
        MenuDnsMoveToBottom.Text = "Move to bottom";
        MenuDnsMoveToBottom.ShortcutKeys = Keys.Control | Keys.End;
        MenuDnsMoveToBottom.Click -= MenuDnsMoveToBottom_Click;
        MenuDnsMoveToBottom.Click += MenuDnsMoveToBottom_Click;
        MD.Items.Add(MenuDnsMoveToBottom);

        MD.Items.Add("-");

        MenuDnsImport.Font = Font;
        MenuDnsImport.Text = "Import";
        MenuDnsImport.ShortcutKeys = Keys.Control | Keys.I;
        MenuDnsImport.Click -= MenuDnsImport_Click;
        MenuDnsImport.Click += MenuDnsImport_Click;
        MD.Items.Add(MenuDnsImport);

        MenuDnsExport.Font = Font;
        MenuDnsExport.Text = "Export";
        MenuDnsExport.ShortcutKeys = Keys.Control | Keys.E;
        MenuDnsExport.Click -= MenuDnsExport_Click;
        MenuDnsExport.Click += MenuDnsExport_Click;
        MD.Items.Add(MenuDnsExport);

        MenuDnsExportAsText.Font = Font;
        MenuDnsExportAsText.Text = "Export as text";
        MenuDnsExportAsText.Click -= MenuDnsExportAsText_Click;
        MenuDnsExportAsText.Click += MenuDnsExportAsText_Click;
        MD.Items.Add(MenuDnsExportAsText);

        MD.Items.Add("-");

        MenuDnsSelectAll.Font = Font;
        MenuDnsSelectAll.Text = "Select all";
        MenuDnsSelectAll.Click -= MenuDnsSelectAll_Click;
        MenuDnsSelectAll.Click += MenuDnsSelectAll_Click;
        MD.Items.Add(MenuDnsSelectAll);

        MenuDnsInvertSelection.Font = Font;
        MenuDnsInvertSelection.Text = "Invert selection";
        MenuDnsInvertSelection.Click -= MenuDnsInvertSelection_Click;
        MenuDnsInvertSelection.Click += MenuDnsInvertSelection_Click;
        MD.Items.Add(MenuDnsInvertSelection);

        Theme.SetColors(MD);
    }

    private void MenuDnsRemove_Click(object? sender, EventArgs? e)
    {
        var dgvG = CustomDataGridViewGroups;
        var dgvS = CustomDataGridViewDNSs;

        if (dgvG.SelectedCells.Count < 1) return;
        if (dgvS.SelectedCells.Count < 1) return;

        int currentRowG = dgvG.SelectedRows[0].Index;
        string? groupName = dgvG.Rows[currentRowG].Cells[1].Value.ToString();
        if (string.IsNullOrEmpty(groupName)) return;

        // Find a Row to Select
        int[] itemRows = new int[dgvS.SelectedRows.Count];
        for (int a = 0; a < dgvS.SelectedRows.Count; a++)
        {
            itemRows[a] = dgvS.SelectedRows[a].Index;
        }
        int select = itemRows[0];
        if (select != 0) select--;
        int firstVisible = dgvS.FirstDisplayedScrollingRowIndex;

        // Remove Selected DNSs
        RemoveDNSs(groupName);

        // Refresh DNSs
        ReadDNSs(groupName);

        // Select and Make it Visible
        ShowRow(dgvS, select, firstVisible);

        // Clear Selection
        dgvS.ClearSelection();

        // Save xDocument to File
        try
        {
            XDoc.Save(CustomServersXmlPath, SaveOptions.None);
        }
        catch (Exception)
        {
            // do nothing
        }
    }

    private void MenuDnsRemoveAll_Click(object? sender, EventArgs e)
    {
        var dgvG = CustomDataGridViewGroups;
        var dgvS = CustomDataGridViewDNSs;

        if (dgvG.RowCount == 0) return;
        if (dgvG.SelectedCells.Count < 1) return;
        if (dgvS.RowCount == 0) return;

        int currentRowG = dgvG.SelectedRows[0].Index;
        string? groupName = dgvG.Rows[currentRowG].Cells[1].Value.ToString();
        if (string.IsNullOrEmpty(groupName)) return;

        if (XDoc.Root == null) return;
        var nodesGroups = XDoc.Root.Elements().Elements();

        for (int a = 0; a < nodesGroups.Count(); a++)
        {
            XElement nodeG = nodesGroups.ToList()[a];
            XElement? nodeGName = nodeG.Element("Name");
            if (nodeGName != null && nodeGName.Value == groupName)
            {
                var nodes = nodeG.Elements("DnsItem");
                nodes.Remove();
                break;
            }
        }

        // Refresh DNSs
        ReadDNSs(groupName);

        // Save xDocument to File
        try
        {
            XDoc.Save(CustomServersXmlPath, SaveOptions.None);
        }
        catch (Exception)
        {
            // do nothing
        }

        Debug.WriteLine($"All DNSs Removed From {groupName}");
    }

    private void MenuDnsMoveSelectedToGroup_Click(object? sender, EventArgs e)
    {
        if (sender is not ToolStripItem item) return;
        string groupToSend = item.Text;

        var dgvG = CustomDataGridViewGroups;
        var dgvS = CustomDataGridViewDNSs;

        if (dgvG.SelectedCells.Count < 1) return;
        if (dgvS.SelectedCells.Count < 1) return;

        int firstVisible = dgvS.FirstDisplayedScrollingRowIndex;

        int currentRowG = dgvG.SelectedRows[0].Index;
        string? groupName = dgvG.Rows[currentRowG].Cells[1].Value.ToString();
        if (string.IsNullOrEmpty(groupName)) return;

        int[] itemRows = new int[dgvS.SelectedRows.Count];
        for (int a = 0; a < dgvS.SelectedRows.Count; a++)
        {
            itemRows[a] = dgvS.SelectedRows[a].Index;
        }

        int select = itemRows[0];
        if (select != 0) select--;

        Array.Sort(itemRows);

        if (XDoc.Root == null) return;
        var nodesGroups = XDoc.Root.Elements().Elements();
        List<XElement> selectedDNSs = new();

        for (int a = 0; a < nodesGroups.Count(); a++)
        {
            XElement nodeG = nodesGroups.ToList()[a];
            XElement? nodeGName = nodeG.Element("Name");
            if (nodeGName != null && nodeGName.Value == groupName)
            {
                // Add Selected DNSs To List
                for (int b = 0; b < itemRows.Length; b++)
                {
                    int row = itemRows[b];
                    XElement dns = nodeG.Elements("DnsItem").ToList()[row];
                    selectedDNSs.Add(dns);
                }

                // Remove Selected DNSs
                int less = 0;
                for (int b = 0; b < itemRows.Length; b++)
                {
                    int row = itemRows[b];
                    if (b != 0)
                    {
                        less++;
                        row = itemRows[b] - less;
                    }
                    var node = nodeG.Elements("DnsItem").ToList()[row];
                    node.Remove();
                }

                // Copy Selected DNSs
                for (int b = 0; b < selectedDNSs.Count; b++)
                {
                    XElement dns = selectedDNSs[b];
                    CopyDnsToGroup(groupToSend, dns, null, null);
                }

                break;
            }
        }

        // Read DNSs
        ReadDNSs(groupName);

        // Select and Make it Visible
        ShowRow(dgvS, select, firstVisible);

        // Clear Selection
        dgvS.ClearSelection();

        // Save xDocument to File
        try
        {
            XDoc.Save(CustomServersXmlPath, SaveOptions.None);
        }
        catch (Exception)
        {
            // do nothing
        }

        Debug.WriteLine($"All Selected DNSs Moved to {groupToSend}");
    }

    private void MenuDnsMoveUp_Click(object? sender, EventArgs? e)
    {
        var dgvG = CustomDataGridViewGroups;
        var dgvS = CustomDataGridViewDNSs;

        if (dgvG.SelectedCells.Count < 1) return;
        if (dgvS.SelectedCells.Count < 1) return;

        int currentRowG = dgvG.SelectedRows[0].Index;
        string? groupName = dgvG.Rows[currentRowG].Cells[1].Value.ToString();
        if (string.IsNullOrEmpty(groupName)) return;

        // Find Prevoius DNS
        int[] itemRows = new int[dgvS.SelectedRows.Count];
        for (int a = 0; a < dgvS.SelectedRows.Count; a++)
        {
            itemRows[a] = dgvS.SelectedRows[a].Index;
        }

        Array.Sort(itemRows);

        int firstVisible = dgvS.FirstDisplayedScrollingRowIndex;
        int firstSelectedCell = itemRows[0];
        int insert = firstSelectedCell - 1;

        if (insert < 0) return; // Return When Reaches Top

        for (int a = 0; a < itemRows.Length; a++)
        {
            if (a != 0)
            {
                if (itemRows[a - 1] + 1 != itemRows[a])
                    return; // Return If Rows are not selected in order
            }
        }

        MoveDNSsToGroup(groupName, itemRows, insert, true);

        // Find New Location Of Selected DNSs To Select
        int[] selectDNSs = new int[itemRows.Length];
        for (int n = 0; n < itemRows.Length; n++)
        {
            if (n == 0)
                selectDNSs[n] = firstSelectedCell - 1;
            else
                selectDNSs[n] = selectDNSs[n - 1] + 1;
        }

        // Read DNSs
        ReadDNSs(groupName);

        // Select and Make it Visible
        ShowRow(dgvS, selectDNSs, firstVisible);

        // Save xDocument to File
        try
        {
            XDoc.Save(CustomServersXmlPath, SaveOptions.None);
        }
        catch (Exception)
        {
            // do nothing
        }

        Debug.WriteLine("Selected DNSs Moved up");
    }

    private void MenuDnsMoveDown_Click(object? sender, EventArgs? e)
    {
        var dgvG = CustomDataGridViewGroups;
        var dgvS = CustomDataGridViewDNSs;

        if (dgvG.SelectedCells.Count < 1) return;
        if (dgvS.SelectedCells.Count < 1) return;

        int currentRowG = dgvG.SelectedRows[0].Index;
        string? groupName = dgvG.Rows[currentRowG].Cells[1].Value.ToString();
        if (string.IsNullOrEmpty(groupName)) return;

        // Find Next DNS
        int[] itemRows = new int[dgvS.SelectedRows.Count];
        for (int a = 0; a < dgvS.SelectedRows.Count; a++)
        {
            itemRows[a] = dgvS.SelectedRows[a].Index;
        }

        Array.Sort(itemRows);

        int firstVisible = dgvS.FirstDisplayedScrollingRowIndex;
        int firstSelectedCell = itemRows[0];
        int insert = firstSelectedCell;

        int lastSelectedCell = itemRows[^1];
        int displayedRowCount = dgvS.DisplayedRowCount(false);
        if (lastSelectedCell - firstVisible == displayedRowCount)
            firstVisible++;

        if (insert > dgvS.RowCount - 1 - dgvS.SelectedRows.Count) return; // Return When Reaches Bottom

        for (int a = 0; a < itemRows.Length; a++)
        {
            if (a != 0)
            {
                if (itemRows[a - 1] + 1 != itemRows[a])
                    return; // Return When Rows are not selected in order
            }
        }

        MoveDNSsToGroup(groupName, itemRows, insert, false);

        // Find New Location Of Selected DNSs To Select
        int[] selectDNSs = new int[itemRows.Length];
        for (int n = 0; n < itemRows.Length; n++)
        {
            if (n == 0)
                selectDNSs[n] = firstSelectedCell + 1;
            else
                selectDNSs[n] = selectDNSs[n - 1] + 1;
        }

        // Read DNSs
        ReadDNSs(groupName);

        // Select and Make it Visible
        ShowRow(dgvS, selectDNSs, firstVisible);

        // Save xDocument to File
        try
        {
            XDoc.Save(CustomServersXmlPath, SaveOptions.None);
        }
        catch (Exception)
        {
            // do nothing
        }

        Debug.WriteLine("All Selected DNSs Moved Down");
    }

    private void MenuDnsMoveToTop_Click(object? sender, EventArgs? e)
    {
        var dgvG = CustomDataGridViewGroups;
        var dgvS = CustomDataGridViewDNSs;

        if (dgvG.SelectedCells.Count < 1) return;
        if (dgvS.SelectedCells.Count < 1) return;

        int currentRowG = dgvG.SelectedRows[0].Index;
        string? groupName = dgvG.Rows[currentRowG].Cells[1].Value.ToString();
        if (string.IsNullOrEmpty(groupName)) return;

        // Find Top DNS
        int insert = 0;

        int[] itemRows = new int[dgvS.SelectedRows.Count];
        for (int a = 0; a < dgvS.SelectedRows.Count; a++)
        {
            itemRows[a] = dgvS.SelectedRows[a].Index;
        }

        Array.Sort(itemRows);

        int firstVisible = 0;
        int firstSelectedCell = itemRows[0];

        if (firstSelectedCell - 1 < 0) return; // Return When Reaches Top

        for (int a = 0; a < itemRows.Length; a++)
        {
            if (a != 0)
            {
                if (itemRows[a - 1] + 1 != itemRows[a])
                    return; // Return If Rows are not selected in order
            }
        }

        MoveDNSsToGroup(groupName, itemRows, insert, true);

        // Find New Location Of Selected DNSs To Select
        int[] selectDNSs = new int[itemRows.Length];
        for (int n = 0; n < itemRows.Length; n++)
        {
            if (n == 0)
                selectDNSs[n] = 0;
            else
                selectDNSs[n] = selectDNSs[n - 1] + 1;
        }

        // Read DNSs
        ReadDNSs(groupName);

        // Select and Make it Visible
        ShowRow(dgvS, selectDNSs, firstVisible);

        // Save xDocument to File
        try
        {
            XDoc.Save(CustomServersXmlPath, SaveOptions.None);
        }
        catch (Exception)
        {
            // do nothing
        }

        Debug.WriteLine("All Selected DNSs Moved to Top");
    }

    private void MenuDnsMoveToBottom_Click(object? sender, EventArgs? e)
    {
        var dgvG = CustomDataGridViewGroups;
        var dgvS = CustomDataGridViewDNSs;

        if (dgvG.SelectedCells.Count < 1) return;
        if (dgvS.SelectedCells.Count < 1) return;

        int currentRowG = dgvG.SelectedRows[0].Index;
        string? groupName = dgvG.Rows[currentRowG].Cells[1].Value.ToString();
        if (string.IsNullOrEmpty(groupName)) return;

        // Find Bottom DNS
        int insert = dgvS.RowCount - 1 - dgvS.SelectedRows.Count;

        int[] itemRows = new int[dgvS.SelectedRows.Count];
        for (int a = 0; a < dgvS.SelectedRows.Count; a++)
        {
            itemRows[a] = dgvS.SelectedRows[a].Index;
        }

        Array.Sort(itemRows);

        int firstVisible = dgvS.RowCount - dgvS.DisplayedRowCount(false);
        int firstSelectedCell = itemRows[0];

        if (firstSelectedCell > insert) return; // Return When Reaches Bottom

        for (int a = 0; a < itemRows.Length; a++)
        {
            if (a != 0)
            {
                if (itemRows[a - 1] + 1 != itemRows[a])
                    return; // Return If Rows are not selected in order
            }
        }

        MoveDNSsToGroup(groupName, itemRows, insert, false);

        // Find New Location Of Selected DNSs To Select
        int[] selectDNSs = new int[itemRows.Length];
        for (int n = 0; n < itemRows.Length; n++)
        {
            if (n == 0)
                selectDNSs[n] = dgvS.RowCount - itemRows.Length;
            else
                selectDNSs[n] = selectDNSs[n - 1] + 1;
        }

        // Read DNSs
        ReadDNSs(groupName);

        // Select and Make it Visible
        ShowRow(dgvS, selectDNSs, firstVisible);

        // Save xDocument to File
        try
        {
            XDoc.Save(CustomServersXmlPath, SaveOptions.None);
        }
        catch (Exception)
        {
            // do nothing
        }

        Debug.WriteLine("All Selected DNSs Moved to Bottom");
    }

    private void MenuDnsImport_Click(object? sender, EventArgs? e)
    {
        var dgvG = CustomDataGridViewGroups;
        var dgvS = CustomDataGridViewDNSs;
        if (dgvG.SelectedRows.Count == 0) return;

        using OpenFileDialog ofd = new();
        ofd.Filter = "Custom DNS Servers (*.sdcs)|*.sdcs|Custom DNS Servers (*.xml)|*.xml";
        ofd.DefaultExt = ".sdcs";
        ofd.AddExtension = true;
        ofd.RestoreDirectory = true;
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            string? groupToSend = dgvG.SelectedRows[0].Cells[1].Value.ToString();
            if (string.IsNullOrEmpty(groupToSend)) return;

            int selectDNS;
            if (dgvS.RowCount > 0)
                selectDNS = dgvS.RowCount;
            else
                selectDNS = 0;

            string filePath = ofd.FileName;

            if (!XmlTool.IsValidXML(File.ReadAllText(filePath)))
            {
                CustomMessageBox.Show(this, "XML file is not valid.", "Not Valid", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            XDocument importedReplaceList = XDocument.Load(filePath);
            if (importedReplaceList.Root == null) return;
            var nodesGroups = importedReplaceList.Root.Elements().Elements();

            if (!nodesGroups.Any())
            {
                CustomMessageBox.Show(this, "XML file has no groups.", "No Groups", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            for (int a = 0; a < nodesGroups.Count(); a++)
            {
                XElement nodeG = nodesGroups.ToList()[a];
                var dnss = nodeG.Elements("DnsItem");

                if (!dnss.Any())
                {
                    CustomMessageBox.Show(this, "Group has no DNS.", "No DNS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                for (int b = 0; b < dnss.Count(); b++)
                {
                    XElement dns = dnss.ToList()[b];
                    CopyDnsToGroup(groupToSend, dns, null, null);
                }
            }

            // Read DNSs
            ReadDNSs(groupToSend);

            // Select and Make it Visible
            ShowRow(dgvS, selectDNS, selectDNS);

            // Save xDocument to File
            try
            {
                XDoc.Save(CustomServersXmlPath, SaveOptions.None);
            }
            catch (Exception)
            {
                // do nothing
            }
        }
    }

    private void MenuDnsExport_Click(object? sender, EventArgs? e)
    {
        var dgvG = CustomDataGridViewGroups;
        var dgvS = CustomDataGridViewDNSs;
        if (dgvG.SelectedRows.Count == 0) return;
        if (dgvS.RowCount == 0) return;

        string? groupToExport = dgvG.SelectedRows[0].Cells[1].Value.ToString();
        if (string.IsNullOrEmpty(groupToExport)) return;

        XDocument doc = CreateXmlCS();
        if (doc.Root == null) return;
        XElement? insert = doc.Root.Element("CustomDnsList");
        if (insert == null) return;

        insert.Add(GetXmlGroupByName(XDoc, groupToExport));

        using SaveFileDialog sfd = new();
        sfd.Filter = "Custom DNS Servers (*.sdcs)|*.sdcs|Custom DNS Servers (*.xml)|*.xml";
        sfd.DefaultExt = ".sdcs";
        sfd.AddExtension = true;
        sfd.RestoreDirectory = true;
        sfd.FileName = $"custom_servers_{groupToExport}";
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                doc.Save(sfd.FileName, SaveOptions.None);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Export: " + ex.Message);
                CustomMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void MenuDnsExportAsText_Click(object? sender, EventArgs? e)
    {
        var dgvG = CustomDataGridViewGroups;
        var dgvS = CustomDataGridViewDNSs;
        if (dgvG.SelectedRows.Count == 0) return;
        if (dgvS.RowCount == 0) return;

        string? groupToExport = dgvG.SelectedRows[0].Cells[1].Value.ToString();
        if (string.IsNullOrEmpty(groupToExport)) return;

        XDocument doc = CreateXmlCS();
        if (doc.Root == null) return;
        XElement? insert = doc.Root.Element("CustomDnsList");
        if (insert == null) return;

        insert.Add(GetXmlGroupByName(XDoc, groupToExport));

        string result = string.Empty;
        var nodes = doc.Root.Elements().Elements();

        for (int a = 0; a < nodes.Count(); a++)
        {
            XElement nodeG = nodes.ToList()[a];
            XElement? nodeGName = nodeG.Element("Name");
            if (nodeGName == null) return;
            if (groupToExport == nodeGName.Value)
            {
                int count = nodeG.Elements("DnsItem").Count();
                for (int b = 0; b < count; b++)
                {
                    XElement node = nodeG.Elements("DnsItem").ToList()[b];
                    XElement? nodeDns = node.Element("Dns");

                    if (nodeDns != null)
                        result += $"{nodeDns.Value}{Environment.NewLine}";
                }
                if (result.EndsWith(Environment.NewLine)) result = result.TrimEnd(Environment.NewLine);
            }
        }

        using SaveFileDialog sfd = new();
        sfd.Filter = "Custom DNS Servers (*.txt)|*.txt";
        sfd.DefaultExt = ".txt";
        sfd.AddExtension = true;
        sfd.RestoreDirectory = true;
        sfd.FileName = $"custom_servers_{groupToExport}";
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            doc.Save(sfd.FileName, SaveOptions.None);
            try
            {
                File.WriteAllText(sfd.FileName, result, new UTF8Encoding(false));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ExportAsText: " + ex.Message);
                CustomMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void MenuDnsSelectAll_Click(object? sender, EventArgs e)
    {
        var dgvG = CustomDataGridViewGroups;
        var dgvS = CustomDataGridViewDNSs;
        if (dgvG.RowCount == 0) return;
        if (dgvS.RowCount == 0) return;

        for (int n = 0; n < dgvS.RowCount; n++)
        {
            var row = dgvS.Rows[n];
            var cellCheck = row.Cells[0];

            cellCheck.Value = true;
            dgvS.EndEdit();
        }

        // Save CheckBox State for DNSs
        int groupIndex = dgvG.SelectedCells[0].RowIndex;
        string? groupName = dgvG.Rows[groupIndex].Cells[1].Value.ToString();
        if (string.IsNullOrEmpty(groupName)) return;

        if (XDoc.Root == null) return;
        var nodesGroups = XDoc.Root.Elements().Elements();

        for (int a = 0; a < nodesGroups.Count(); a++)
        {
            XElement nodeG = nodesGroups.ToList()[a];
            XElement? nodeGName = nodeG.Element("Name");
            if (nodeGName != null && groupName == nodeGName.Value)
            {
                int count = nodeG.Elements("DnsItem").Count();
                for (int b = 0; b < count; b++)
                {
                    XElement row = nodeG.Elements("DnsItem").ToList()[b];
                    XElement? rowEnabled = row.Element("Enabled");
                    string? isRowEnabled = dgvS.Rows[b].Cells[0].Value.ToString();

                    if (rowEnabled != null && !string.IsNullOrEmpty(isRowEnabled))
                        rowEnabled.Value = isRowEnabled;
                }
            }
        }
    }

    private void MenuDnsInvertSelection_Click(object? sender, EventArgs e)
    {
        var dgvG = CustomDataGridViewGroups;
        var dgvS = CustomDataGridViewDNSs;
        if (dgvG.RowCount == 0) return;
        if (dgvS.RowCount == 0) return;

        for (int a = 0; a < dgvS.RowCount; a++)
        {
            var row = dgvS.Rows[a];
            var cellCheck = row.Cells[0];
            bool cellCheckValue = Convert.ToBoolean(cellCheck.Value);

            if (cellCheckValue)
            {
                cellCheck.Value = false;
                dgvS.EndEdit();
            }
            else
            {
                cellCheck.Value = true;
                dgvS.EndEdit();
            }
        }

        // Save CheckBox State for DNSs
        int groupIndex = dgvG.SelectedCells[0].RowIndex;
        string? groupName = dgvG.Rows[groupIndex].Cells[1].Value.ToString();
        if (string.IsNullOrEmpty(groupName)) return;

        if (XDoc.Root == null) return;
        var nodesGroups = XDoc.Root.Elements().Elements();

        for (int a = 0; a < nodesGroups.Count(); a++)
        {
            XElement nodeG = nodesGroups.ToList()[a];
            XElement? nodeGName = nodeG.Element("Name");
            if (nodeGName != null && groupName == nodeGName.Value)
            {
                int count = nodeG.Elements("DnsItem").Count();
                for (int b = 0; b < count; b++)
                {
                    XElement row = nodeG.Elements("DnsItem").ToList()[b];
                    XElement? rowEnabled = row.Element("Enabled");
                    string? isRowEnabled = dgvS.Rows[b].Cells[0].Value.ToString();

                    if (rowEnabled != null && !string.IsNullOrEmpty(isRowEnabled))
                        rowEnabled.Value = isRowEnabled;
                }
            }
        }
    }

    //======================================= Add & Modify DNSs ======================================

    private void CustomButtonNewGroup_Click(object sender, EventArgs e)
    {
        MenuGroupNew_Click(sender, e);
    }

    private void CustomButtonImport_Click(object sender, EventArgs e)
    {
        MenuGroupImport_Click(sender, e);
    }

    private void CustomButtonExport_Click(object sender, EventArgs e)
    {
        MenuGroupExport_Click(sender, e);
    }

    private void CustomButtonAddServers_Click(object sender, EventArgs e)
    {
        var dgvG = CustomDataGridViewGroups;
        var dgvR = CustomDataGridViewDNSs;

        if (dgvG.RowCount == 0) return;
        if (dgvG.SelectedRows.Count == 0) return;

        // Context Menu
        CustomContextMenuStrip Add = new();
        Add.Font = Font;

        ToolStripMenuItem MenuAddBelowSelected = new();
        MenuAddBelowSelected.Font = Font;
        MenuAddBelowSelected.Text = "Add below selected";
        MenuAddBelowSelected.Click -= MenuAddBelowSelected_Click;
        MenuAddBelowSelected.Click += MenuAddBelowSelected_Click;
        Add.Items.Add(MenuAddBelowSelected);

        ToolStripMenuItem MenuAddToEnd = new();
        MenuAddToEnd.Font = Font;
        MenuAddToEnd.Text = "Add to end";
        MenuAddToEnd.Click -= MenuAddToEnd_Click;
        MenuAddToEnd.Click += MenuAddToEnd_Click;
        Add.Items.Add(MenuAddToEnd);

        ToolStripMenuItem MenuAddToEndBrowse = new();
        MenuAddToEndBrowse.Font = Font;
        MenuAddToEndBrowse.Text = "Add multiple (Browse)";
        MenuAddToEndBrowse.Click -= MenuAddToEndBrowse_Click;
        MenuAddToEndBrowse.Click += MenuAddToEndBrowse_Click;
        Add.Items.Add(MenuAddToEndBrowse);

        if (dgvR.SelectedRows.Count == 0 || dgvR.RowCount == 0)
            Add.Items.Remove(MenuAddBelowSelected);

        if (dgvR.SelectedRows.Count > 0)
            if (dgvR.SelectedRows[0].Index == dgvR.RowCount - 1)
                Add.Items.Remove(MenuAddBelowSelected);

        Theme.SetColors(Add);
        Add.RoundedCorners = 5;
        Add.Show(CustomButtonAddServers, 0, -5);

        void MenuAddBelowSelected_Click(object? sender, EventArgs e)
        {
            if (dgvR.SelectedRows.Count == 0)
            {
                CustomMessageBox.Show(this, "Select a row.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(CustomTextBoxDNS.Text.Trim()))
            {
                CustomMessageBox.Show(this, "DNS Address cannot be empty.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else if (!FormMain.IsDnsProtocolSupported(CustomTextBoxDNS.Text.Trim()))
            {
                CustomMessageBox.Show(this, "DNS protocol is not supported.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string? groupToAdd = dgvG.SelectedRows[0].Cells[1].Value.ToString();
            if (string.IsNullOrEmpty(groupToAdd)) return;

            int rowToInsert = dgvR.SelectedRows[0].Index;

            int firstVisible = dgvR.FirstDisplayedScrollingRowIndex;
            if (rowToInsert - firstVisible == dgvR.DisplayedRowCount(false) - 1)
                firstVisible++;

            XElement dns = CreateDNS();

            CopyDnsToGroup(groupToAdd, dns, rowToInsert, false);

            // Read DNSs
            ReadDNSs(groupToAdd);

            // Select and Make it Visible
            ShowRow(dgvR, rowToInsert + 1, firstVisible);

            // Save xDocument to File
            try
            {
                XDoc.Save(CustomServersXmlPath, SaveOptions.None);
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        void MenuAddToEnd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CustomTextBoxDNS.Text.Trim()))
            {
                CustomMessageBox.Show(this, "DNS Address cannot be empty.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else if (!FormMain.IsDnsProtocolSupported(CustomTextBoxDNS.Text.Trim()))
            {
                CustomMessageBox.Show(this, "DNS protocol is not supported.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string? groupToAdd = dgvG.SelectedRows[0].Cells[1].Value.ToString();
            if (string.IsNullOrEmpty(groupToAdd)) return;

            int rowToInsert;
            if (dgvR.RowCount > 0)
                rowToInsert = dgvR.RowCount;
            else
                rowToInsert = 0;

            int firstVisible = rowToInsert - dgvR.DisplayedRowCount(false) + 1;
            if (firstVisible < 0) firstVisible = 0;

            XElement dns = CreateDNS();

            CopyDnsToGroup(groupToAdd, dns, null, null);

            // Read DNSs
            ReadDNSs(groupToAdd);

            // Select and Make it Visible
            ShowRow(dgvR, rowToInsert, firstVisible);

            // Save xDocument to File
            try
            {
                XDoc.Save(CustomServersXmlPath, SaveOptions.None);
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        async void MenuAddToEndBrowse_Click(object? sender, EventArgs e)
        {
            try
            {
                string? groupToAdd = dgvG.SelectedRows[0].Cells[1].Value.ToString();
                if (string.IsNullOrEmpty(groupToAdd)) return;

                // Browse
                using OpenFileDialog ofd = new();
                ofd.Filter = "DNS Servers|*.txt";
                ofd.Multiselect = true;
                ofd.RestoreDirectory = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string allContent = string.Empty;
                    string[] files = ofd.FileNames;
                    for (int n = 0; n < files.Length; n++)
                    {
                        string file = files[n];
                        if (File.Exists(file))
                        {
                            string content = await File.ReadAllTextAsync(file, new UTF8Encoding(false));
                            if (content.Length > 0)
                            {
                                allContent += content;
                                allContent += Environment.NewLine;
                            }
                        }
                    }

                    if (allContent.Length > 0)
                    {
                        int rowToInsert = dgvR.RowCount > 0 ? dgvR.RowCount : 0;

                        int firstVisible = rowToInsert - dgvR.DisplayedRowCount(false) + 1;
                        if (firstVisible < 0) firstVisible = 0;

                        List<string> lines = allContent.SplitToLines();
                        for (int n = 0; n < lines.Count; n++)
                        {
                            string line = lines[n];

                            if (FormMain.IsDnsProtocolSupported(line))
                            {
                                XElement dns = CreateDNS(line, string.Empty);
                                CopyDnsToGroup(groupToAdd, dns, null, null);
                            }
                        }

                        // Read DNSs
                        ReadDNSs(groupToAdd);

                        // Select and Make it Visible
                        ShowRow(dgvR, rowToInsert, firstVisible);

                        // Save xDocument to File
                        try
                        {
                            XDoc.Save(CustomServersXmlPath, SaveOptions.None);
                        }
                        catch (Exception)
                        {
                            // do nothing
                        }
                    }
                }
            }
            catch (Exception)
            {
                // do nothing
            }
        }
    }

    private void CustomButtonModifyDNS_Click(object sender, EventArgs e)
    {
        var dgvG = CustomDataGridViewGroups;
        var dgvS = CustomDataGridViewDNSs;

        if (dgvG.RowCount == 0) return;
        if (dgvG.SelectedRows.Count == 0) return;
        if (dgvS.RowCount == 0) return;

        if (dgvS.SelectedRows.Count == 0)
        {
            CustomMessageBox.Show(this, "Select a DNS to modify.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Check DNS is Supported
        if (!FormMain.IsDnsProtocolSupported(CustomTextBoxDNS.Text.Trim()))
        {
            CustomMessageBox.Show(this, "DNS is not supported.", "Supported DNS");
            return;
        }

        var groupToUpdate = dgvG.SelectedRows[0].Cells[1].Value.ToString();
        int rowToSelect = dgvS.SelectedRows[0].Index;
        int firstVisible = dgvS.FirstDisplayedScrollingRowIndex;

        if (XDoc.Root == null) return;
        var groups = XDoc.Root.Elements().Elements();

        for (int a = 0; a < groups.Count(); a++)
        {
            XElement group = groups.ToList()[a];
            XElement? groupName = group.Element("Name");
            if (groupName != null && groupName.Value == groupToUpdate)
            {
                var dnss = group.Elements("DnsItem").ToList();
                for (int b = 0; b < dnss.Count; b++)
                {
                    XElement dns = dnss[b];
                    XElement? dnsAddress = dns.Element("Dns");
                    XElement? dnsDescription = dns.Element("Description");
                    if (rowToSelect == b && dnsAddress != null && dnsDescription != null)
                    {
                        dnsAddress.Value = CustomTextBoxDNS.Text.Trim();
                        dnsDescription.Value = CustomTextBoxDescription.Text.Trim();
                        break;
                    }
                }
            }
        }

        // Read DNSs
        ReadDNSs(groupToUpdate);

        // Select and Make it Visible
        ShowRow(dgvS, rowToSelect, firstVisible);

        // Save xDocument to File
        try
        {
            XDoc.Save(CustomServersXmlPath, SaveOptions.None);
        }
        catch (Exception)
        {
            // do nothing
        }
    }

    private async void FormCustomServers_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.WindowsShutDown)
        {
            // Save xDocument to File
            //XDoc.Save(CustomServersXmlPath, SaveOptions.None);
            await XDoc.SaveAsync(CustomServersXmlPath);
        }
    }

}