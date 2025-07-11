using System.Xml;
using System.Data;
using System.Xml.Linq;
using System.Diagnostics;

namespace MsmhToolsClass;

public static class XmlTool
{
    public static bool IsValid(string content)
    {
        bool result = false;

        try
        {
            if (!string.IsNullOrEmpty(content))
            {
                XmlDocument? xmlDoc = new();
                xmlDoc.LoadXml(content);
                result = true;
                xmlDoc = null;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("XmlTools IsValid: " + ex.Message);
        }

        return result;
    }

    public static bool IsValidFile(string xmlFilePath)
    {
        bool result = false;

        try
        {
            if (!string.IsNullOrEmpty(xmlFilePath) && File.Exists(xmlFilePath))
            {
                string content = File.ReadAllText(xmlFilePath);
                XmlDocument? xmlDoc = new();
                xmlDoc.LoadXml(content);
                result = true;
                xmlDoc = null;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("XmlTools IsValidFile: " + ex.Message);
        }

        return result;
    }

    public static async Task<bool> IsValidFileAsync(string xmlFilePath)
    {
        bool result = false;

        try
        {
            if (!string.IsNullOrEmpty(xmlFilePath) && File.Exists(xmlFilePath))
            {
                string content = await File.ReadAllTextAsync(xmlFilePath);
                XmlDocument? xmlDoc = new();
                xmlDoc.LoadXml(content);
                result = true;
                xmlDoc = null;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("XmlTools IsValidFileAsync: " + ex.Message);
        }

        return result;
    }

    public struct XmlAttributeCondition
    {
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }

        public XmlAttributeCondition(string attributeName, string attributeValue)
        {
            AttributeName = attributeName;
            AttributeValue = attributeValue;
        }
    }

    public struct XmlChildCondition
    {
        public string ElementName { get; set; }
        public List<XmlAttributeCondition> AttributeConditions { get; set; } = new();
        public readonly bool HasValueCondition => !string.IsNullOrWhiteSpace(NodeValue);
        public XmlNodeType NodeType { get; set; } = XmlNodeType.Text;
        public string NodeValue { get; set; } = string.Empty;

        public XmlChildCondition(string elementName)
        {
            ElementName = elementName;
        }

        public XmlChildCondition(string elementName, List<XmlAttributeCondition> attributeConditions)
        {
            ElementName = elementName;
            AttributeConditions = attributeConditions;
        }

        public XmlChildCondition(string elementName, XmlNodeType nodeType, string nodeValue)
        {
            ElementName = elementName;
            NodeType = nodeType;
            NodeValue = nodeValue;
        }

        public XmlChildCondition(string elementName, List<XmlAttributeCondition> attributeConditions, XmlNodeType nodeType, string nodeValue)
        {
            ElementName = elementName;
            AttributeConditions = attributeConditions;
            NodeType = nodeType;
            NodeValue = nodeValue;
        }
    }

    public struct XmlPath
    {
        /// <summary>
        /// Key (Name) To Find
        /// </summary>
        public string ElementName { get; set; }

        /// <summary>
        /// Break After N Elements
        /// </summary>
        public int Count { get; set; } = 0;

        /// <summary>
        /// Attribute Conditions To Match
        /// </summary>
        public List<XmlAttributeCondition> AttributeConditions { get; set; } = new();

        /// <summary>
        /// If True: Check Every ChildCondition In Its Matched Descendant (Similar To Check In Series)
        /// </summary>
        public bool TreatChildConditionsAsPath { get; set; } = false;

        /// <summary>
        /// Conditions To Match
        /// </summary>
        public List<XmlChildCondition> ChildConditions { get; set; } = new();

        public XmlPath(string elementName)
        {
            ElementName = elementName;
        }

        public XmlPath(string elementName, int count)
        {
            ElementName = elementName;
            Count = count;
        }

        public XmlPath(string elementName, int count, List<XmlAttributeCondition> attributeConditions)
        {
            ElementName = elementName;
            Count = count;
            AttributeConditions = attributeConditions;
        }

        public XmlPath(string elementName, int count, List<XmlChildCondition> childConditions)
        {
            ElementName = elementName;
            Count = count;
            ChildConditions = childConditions;
        }

        public XmlPath(string elementName, int count, bool treatChildConditionsAsPath, List<XmlChildCondition> childConditions)
        {
            ElementName = elementName;
            Count = count;
            TreatChildConditionsAsPath = treatChildConditionsAsPath;
            ChildConditions = childConditions;
        }

        public XmlPath(string elementName, int count, List<XmlAttributeCondition> attributeConditions, List<XmlChildCondition> childConditions)
        {
            ElementName = elementName;
            Count = count;
            AttributeConditions = attributeConditions;
            ChildConditions = childConditions;
        }

        public XmlPath(string elementName, int count, bool treatChildConditionsAsPath, List<XmlAttributeCondition> attributeConditions, List<XmlChildCondition> childConditions)
        {
            ElementName = elementName;
            Count = count;
            TreatChildConditionsAsPath = treatChildConditionsAsPath;
            AttributeConditions = attributeConditions;
            ChildConditions = childConditions;
        }
    }

    // e.g.
    // List<XmlTool.XmlPath> paths = new()
    // {
    //      new XmlTool.XmlPath("Group", 1, new () { new("Name", XmlNodeType.Text, "Test2") }),
    //      new XmlTool.XmlPath("DnsItem", 0, new () { new("Enabled", XmlNodeType.Text, "False") }),
    // };
    public static List<XElement> GetElements(XDocument xDoc, List<XmlPath> paths)
    {
        List<XElement> result_XElements = new();
        if (xDoc.Root == null) Debug.WriteLine("XmlTool GetElements: XDocument.Root Is NULL.");
        if (xDoc.Root == null || paths.Count == 0) return result_XElements;
        
        try
        {
            static bool checkAttributeCondition(List<XmlAttributeCondition> attributeConditions, XElement child)
            {
                bool go = false;
                try
                {
                    int counted = 0;
                    foreach (XmlAttributeCondition condition in attributeConditions)
                    {
                        foreach (XAttribute attribute in child.Attributes())
                        {
                            if (condition.AttributeName.Equals(attribute.Name.LocalName))
                            {
                                // attribute.NodeType Is Always Attribute
                                if (attribute.Value != null && condition.AttributeValue.Equals(attribute.Value))
                                {
                                    counted++;
                                }
                            }
                        }
                        if (counted == attributeConditions.Count) break;
                    }
                    if (counted == attributeConditions.Count) go = true;
                    if (attributeConditions.Count == 0) go = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("XmlTool GetElements checkAttributeCondition: " + ex.Message);
                }
                return go;
            }

            static XElement? checkChildConditionInSeriesOne(XmlChildCondition childCondition, XElement child)
            {
                XElement? result = null;
                try
                {
                    foreach (XElement descendant in child.Elements())
                    {
                        // XML tags are case sensitive. All XML elements must be properly nested. All XML documents must have a root element. Attribute values must always be quoted.
                        if (childCondition.ElementName.Equals(descendant.Name.LocalName))
                        {
                            bool goAttrib = checkAttributeCondition(childCondition.AttributeConditions, descendant);
                            if (goAttrib)
                            {
                                if (childCondition.HasValueCondition)
                                {
                                    foreach (XNode xNode in descendant.Nodes())
                                    {
                                        if (childCondition.NodeType == xNode.NodeType)
                                        {
                                            if (childCondition.NodeValue.Equals(xNode.ToString()))
                                            {
                                                result = descendant;
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    result = descendant;
                                    break;
                                }
                            }
                        }
                        if (result != null) break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("XmlTool GetElements checkChildConditionOne: " + ex.Message);
                }
                return result;
            }

            static bool checkChildConditionInSeries(List<XmlChildCondition> childConditions, XElement child)
            {
                bool go = false;
                try
                {
                    int counted = 0;
                    XElement? xElement = null;
                    for (int n = 0; n < childConditions.Count; n++)
                    {
                        XmlChildCondition childCondition = childConditions[n];
                        if (n == 0)
                        {
                            xElement = checkChildConditionInSeriesOne(childCondition, child);
                        }
                        else
                        {
                            if (xElement != null)
                            {
                                xElement = checkChildConditionInSeriesOne(childCondition, xElement);
                            }
                        }
                        if (xElement != null) counted++;
                        if (counted == childConditions.Count) break;
                    }
                    if (counted == childConditions.Count) go = true;
                    if (childConditions.Count == 0) go = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("XmlTool GetElements checkChildCondition: " + ex.Message);
                }
                return go;
            }

            static bool checkChildCondition(List<XmlChildCondition> childConditions, XElement child)
            {
                bool go = false;
                try
                {
                    int counted = 0;
                    foreach (XmlChildCondition childCondition in childConditions)
                    {
                        foreach (XElement descendant in child.Elements())
                        {
                            // XML tags are case sensitive. All XML elements must be properly nested. All XML documents must have a root element. Attribute values must always be quoted.
                            if (childCondition.ElementName.Equals(descendant.Name.LocalName))
                            {
                                bool goAttrib = checkAttributeCondition(childCondition.AttributeConditions, descendant);
                                if (goAttrib)
                                {
                                    if (childCondition.HasValueCondition)
                                    {
                                        foreach (XNode xNode in descendant.Nodes())
                                        {
                                            if (childCondition.NodeType == xNode.NodeType)
                                            {
                                                if (childCondition.NodeValue.Equals(xNode.ToString()))
                                                {
                                                    counted++;
                                                }
                                            }
                                            if (counted == childConditions.Count) break;
                                        }
                                    }
                                    else
                                    {
                                        counted++;
                                    }
                                }
                            }
                            if (counted == childConditions.Count) break;
                        }
                        if (counted == childConditions.Count) break;
                    }
                    if (counted == childConditions.Count) go = true;
                    if (childConditions.Count == 0) go = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("XmlTool GetElements checkChildCondition: " + ex.Message);
                }
                return go;
            }

            static List<XElement> loop(XmlPath path, List<XElement> xElements)
            {
                List<XElement> children = new();
                try
                {
                    for (int n = 0; n < xElements.Count; n++)
                    {
                        XElement xElement = xElements[n];
                        foreach (XElement child in xElement.Elements())
                        {
                            if (path.ElementName.Equals(child.Name.LocalName))
                            {
                                bool goAttrib = checkAttributeCondition(path.AttributeConditions, child);
                                if (goAttrib)
                                {
                                    bool go = false;
                                    if (path.TreatChildConditionsAsPath)
                                    {
                                        go = checkChildConditionInSeries(path.ChildConditions, child);
                                    }
                                    else
                                    {
                                        go = checkChildCondition(path.ChildConditions, child);
                                    }
                                    if (go) children.Add(child);
                                }
                            }
                            if (path.Count > 0 && n + 1 >= path.Count && children.Count > 0) break;
                        }
                        if (path.Count > 0 && n + 1 >= path.Count && children.Count > 0) break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("XmlTool GetElements loop: " + ex.Message);
                }
                return children;
            }

            if (paths.Count > 0)
            {
                for (int n = 0; n < paths.Count; n++)
                {
                    XmlPath path = paths[n];
                    if (n == 0)
                    {
                        result_XElements = loop(path, xDoc.Elements().ToList());
                    }
                    else
                    {
                        result_XElements = loop(path, result_XElements);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("XmlTool GetElements: " + ex.Message);
        }

        return result_XElements;
    }

    public static XElement UpdateElementValue(XElement xElement, string newValue)
    {
        try
        {
            List<XNode> xNodes = xElement.Nodes().ToList();
            if (xNodes.Count > 0)
            {
                for (int n = 0; n < xNodes.Count; n++)
                {
                    XNode xNode = xNodes[n];
                    if (xNode.NodeType == XmlNodeType.Text)
                    {
                        //Debug.WriteLine(xElement.Value + " ---> " + newValue);
                        xElement.Value = newValue;
                        //Debug.WriteLine(xElement.Value + " ---> " + newValue);
                    }
                }
            }
            else
            {
                // Element Has No Value
                xElement.Value = newValue;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("XmlTool UpdateElementValue: " + ex.Message);
        }
        return xElement;
    }

    /// <summary>
    /// Update Elements Value
    /// </summary>
    /// <param name="xDoc">XDocument</param>
    /// <param name="paths">Paths</param>
    /// <param name="index">Use -1 To Update All Same Elements</param>
    /// <param name="newValue">New Value</param>
    /// <returns></returns>
    public static XDocument UpdateElementsValue(XDocument xDoc, List<XmlPath> paths, int index, string newValue)
    {
        if (xDoc.Root == null) Debug.WriteLine("XmlTool UpdateElementsValue: XDocument.Root Is NULL.");
        if (xDoc.Root == null || paths.Count == 0) return xDoc;

        try
        {
            List<XElement> elements = GetElements(xDoc, paths);
            for (int n1 = 0; n1 < elements.Count; n1++)
            {
                XElement xElement = elements[n1];
                int currentElementIndex = xElement.ElementsBeforeSelf().Count();
                if (currentElementIndex == index || index == -1)
                {
                    xElement.ReplaceWith(UpdateElementValue(xElement, newValue));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("XmlTool UpdateElementsValue: " + ex.Message);
        }

        return xDoc;
    }

    public static XDocument UpdateElementsPosition(XDocument xDoc, List<XmlPath> paths, List<int> fromIndexes, int toIndex)
    {
        if (xDoc.Root == null) Debug.WriteLine("XmlTool UpdateElementsPosition: XDocument.Root Is NULL.");
        if (xDoc.Root == null || paths.Count == 0) return xDoc;
        if (fromIndexes.Count == 0) return xDoc;
        
        try
        {
            fromIndexes = fromIndexes.Distinct().ToList();
            fromIndexes.Sort();
            int firstFromIndex = fromIndexes[0];
            int lastFromIndex = fromIndexes[^1];
            if (firstFromIndex <= toIndex && toIndex <= lastFromIndex) return xDoc;

            int firstDiff = 0;
            if (firstFromIndex > toIndex) firstDiff = firstFromIndex - toIndex;
            else firstDiff = toIndex - firstFromIndex;

            int lastDiff = 0;
            if (lastFromIndex > toIndex) lastDiff = lastFromIndex - toIndex;
            else lastDiff = toIndex - lastFromIndex;

            Debug.WriteLine($"F {firstDiff}, L {lastDiff}, T {toIndex}");
            bool moveUp = firstDiff < lastDiff || (firstDiff == lastDiff && firstFromIndex > toIndex);
            
            List<XElement> fromElements = GetElements(xDoc, paths);
            if (!moveUp) fromElements.Reverse();

            for (int n1 = 0; n1 < fromElements.Count; n1++)
            {
                XElement fromElement = fromElements[n1];
                int fromElementIndex = fromElement.ElementsBeforeSelf().Count();
                for (int n2 = 0; n2 < fromIndexes.Count; n2++)
                {
                    int fromIndex = fromIndexes[n2];
                    if (fromElementIndex == fromIndex)
                    {
                        //Debug.WriteLine("FromElement: " + fromElement.ToString());
                        if (moveUp) // Move Up
                        {
                            toIndex = fromIndex - firstDiff;
                            Debug.WriteLine($"MoveUp FromIndex: {fromIndex}, ToIndex: {toIndex}, FirstDiff: {firstDiff}");
                            List<XElement> toElements = fromElement.ElementsBeforeSelf().ToList();
                            for (int n3 = 0; n3 < toElements.Count; n3++)
                            {
                                XElement toElement = toElements[n3];
                                int toElementIndex = toElement.ElementsBeforeSelf().Count();
                                if (toElementIndex == toIndex)
                                {
                                    //Debug.WriteLine("ToElement: " + toElement.ToString());
                                    toElement.AddBeforeSelf(fromElement);
                                    fromElement.Remove();
                                    break;
                                }
                            }
                        }
                        else // Move Down
                        {
                            toIndex = fromIndex + lastDiff;
                            Debug.WriteLine($"MoveDown FromIndex: {fromIndex}, ToIndex: {toIndex}, LastDiff: {lastDiff}");
                            List<XElement> toElements = fromElement.ElementsAfterSelf().ToList();
                            for (int n3 = 0; n3 < toElements.Count; n3++)
                            {
                                XElement toElement = toElements[n3];
                                int toElementIndex = toElement.ElementsBeforeSelf().Count();
                                if (toElementIndex == toIndex)
                                {
                                    //Debug.WriteLine("ToElement: " + toElement.ToString());
                                    toElement.AddAfterSelf(fromElement);
                                    fromElement.Remove();
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("XmlTool UpdateElementsPosition: " + ex.Message);
        }

        return xDoc;
    }

    public static XDocument UpdateElementsPosition(XDocument xDoc, List<XmlPath> paths, int fromIndex, int toIndex)
    {
        return UpdateElementsPosition(xDoc, paths, new List<int> { fromIndex }, toIndex);
    }

    public static XDocument RemoveElements(XDocument xDoc, List<XmlPath> paths)
    {
        if (xDoc.Root == null) Debug.WriteLine("XmlTool RemoveElements: XDocument.Root Is NULL.");
        if (xDoc.Root == null || paths.Count == 0) return xDoc;

        try
        {
            List<XElement> elements = GetElements(xDoc, paths);
            for (int n1 = 0; n1 < elements.Count; n1++)
            {
                XElement element = elements[n1];
                element.Remove();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("XmlTool RemoveElements: " + ex.Message);
        }

        return xDoc;
    }

    public static XDocument RemoveChildElements(XDocument xDoc, List<XmlPath> paths)
    {
        if (xDoc.Root == null) Debug.WriteLine("XmlTool RemoveElements: XDocument.Root Is NULL.");
        if (xDoc.Root == null || paths.Count == 0) return xDoc;

        try
        {
            List<XElement> elements = GetElements(xDoc, paths);
            for (int n1 = 0; n1 < elements.Count; n1++)
            {
                XElement element = elements[n1];
                element.RemoveNodes();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("XmlTool RemoveChildElements: " + ex.Message);
        }

        return xDoc;
    }

    public static XDocument RemoveEmptyElements(XDocument xDoc)
    {
        xDoc.Descendants().Where(a => a.IsEmpty && !a.HasAttributes && !a.HasElements && string.IsNullOrWhiteSpace(a.Value)).Remove();
        return xDoc;
    }

    public static void RemoveNodesWithoutChild(string xmlFile)
    {
        if (File.Exists(xmlFile))
        {
            bool isXmlValid = IsValid(File.ReadAllText(xmlFile));
            if (isXmlValid == true)
            {
                XmlDocument doc = new();
                doc.Load(xmlFile);
                var nodes = doc.DocumentElement;
                if (nodes != null)
                {
                    foreach (XmlNode node in nodes)
                        if (node.HasChildNodes == false)
                            nodes.RemoveChild(node);
                    doc.Save(xmlFile);
                }
            }
        }
        else Console.WriteLine("XML File Not Exist.");
    }

}