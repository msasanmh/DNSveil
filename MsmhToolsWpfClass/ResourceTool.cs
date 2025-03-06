using System.Diagnostics;
using System.Windows;

namespace MsmhToolsWpfClass;

public class ResourceTool
{
    public static string GetValueByKeyID(ResourceDictionary resourceDictionary, string keyID)
    {
        string result = string.Empty;

        try
        {
            foreach (ComponentResourceKey key in resourceDictionary.Keys)
            {
                if (key.ResourceId.Equals(keyID))
                {
                    var value = resourceDictionary[key]; // Application.Current.TryFindResource(key);
                    result = Convert.ToString(value) ?? string.Empty;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ResourceTool GetValueByKeyID: " + ex.Message);
        }

        return result;
    }

    public static string GetValueByKey(ResourceDictionary resourceDictionary, ComponentResourceKey key)
    {
        string result = string.Empty;

        try
        {
            // Must Be var
            foreach (var currentKey in resourceDictionary.Keys)
            {
                if (currentKey.Equals(key))
                {
                    var value = resourceDictionary[key]; // Application.Current.TryFindResource(key);
                    result = Convert.ToString(value) ?? string.Empty;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ResourceTool GetValueByKey: " + ex.Message);
        }

        return result;
    }
}