using System;
using System.Xml;
using System.Data;
using System.Xml.Linq;
using System.Diagnostics;

namespace MsmhToolsClass;

public static class XmlTool
{
    //-----------------------------------------------------------------------------------
    public static XDocument RemoveEmptyElements(XDocument xDoc)
    {
        xDoc.Descendants().Where(a => a.IsEmpty && !a.HasAttributes && !a.HasElements && string.IsNullOrWhiteSpace(a.Value)).Remove();
        return xDoc;
    }
    //-----------------------------------------------------------------------------------
    public static void RemoveNodesWithoutChild(string xmlFile)
    {
        if (File.Exists(xmlFile))
        {
            bool isXmlValid = IsValidXML(File.ReadAllText(xmlFile));
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
        else
            Console.WriteLine("XML File Not Exist.");
    }
    //-----------------------------------------------------------------------------------
    public static bool IsValidXML(string content)
    {
        try
        {
            if (!string.IsNullOrEmpty(content))
            {
                XmlDocument xmlDoc = new();
                xmlDoc.LoadXml(content);
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (XmlException ex)
        {
            Debug.WriteLine("XML Error: " + ex.Message);
            return false;
        }
    }
    //-----------------------------------------------------------------------------------
    public static bool IsValidXMLFile(string xmlFilePath)
    {
        try
        {
            if (!string.IsNullOrEmpty(xmlFilePath))
            {
                string content = File.ReadAllText(xmlFilePath);
                XmlDocument xmlDoc = new();
                xmlDoc.LoadXml(content);
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (XmlException ex)
        {
            Debug.WriteLine("XML Error: " + ex.Message);
            return false;
        }
    }
    //-----------------------------------------------------------------------------------
}