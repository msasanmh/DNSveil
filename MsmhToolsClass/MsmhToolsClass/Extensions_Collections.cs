using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Diagnostics;

namespace MsmhToolsClass;

public static class Extensions_Collections
{
    public static bool TryUpdate<K, V>(this ConcurrentDictionary<K, V> ccDic, K key, V newValue) where K : notnull
    {
        try
        {
            if (key == null) return false;
            bool isKeyExist = ccDic.TryGetValue(key, out V? oldValue);
            if (isKeyExist && oldValue != null)
                return ccDic.TryUpdate(key, newValue, oldValue);
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections TryUpdate: " + ex.Message);
            return false;
        }
    }
    
    public static V? AddOrUpdate<K, V>(this ConcurrentDictionary<K, V> ccDic, K key, V newValue) where K : notnull
    {
        try
        {
            if (key == null) return default;
            return ccDic.AddOrUpdate(key, newValue, (oldkey, oldvalue) => newValue);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections AddOrUpdate: " + ex.Message);
            return default;
        }
    }

    public static ConcurrentDictionary<uint, T> ToConcurrentDictionary<T>(this List<T> list) where T : notnull
    {
        ConcurrentDictionary<uint, T> keyValuePairs = new();
        for (int n = 0; n < list.Count; n++)
        {
            try
            {
                keyValuePairs.TryAdd(Convert.ToUInt32(n), list[n]);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Extensions_Collections ToConcurrentDictionary: " + ex.Message);
            }
        }
        return keyValuePairs;
    }

    /// <summary>
    /// To List
    /// </summary>
    public static List<Tuple<string, string>> ToList(this NameValueCollection nvc)
    {
        List<Tuple<string, string>> result = new();

        try
        {
            for (int n = 0; n < nvc.Count; n++)
            {
                string? key = nvc.GetKey(n);
                string? val = nvc.Get(n);
                if (string.IsNullOrEmpty(key)) continue;
                if (string.IsNullOrEmpty(val)) continue;
                result.Add(new Tuple<string, string>(key, val));
            }
        }
        catch (Exception) { }

        return result;
    }

    /// <summary>
    /// To Dictionary
    /// </summary>
    public static Dictionary<string, string> ToDictionary(this NameValueCollection nvc)
    {
        Dictionary<string, string> result = new();

        try
        {
            for (int n = 0; n < nvc.Count; n++)
            {
                string? key = nvc.GetKey(n);
                string? val = nvc.Get(n);
                if (string.IsNullOrEmpty(key)) continue;
                if (string.IsNullOrEmpty(val)) continue;
                result.TryAdd(key, val);
            }
        }
        catch (Exception) { }

        return result;
    }

    /// <summary>
    /// If Key Exist Adds The Value (Comma-Separated)
    /// </summary>
    public static void AddAndUpdate(this NameValueCollection nvc, string? key, string? value)
    {
        try
        {
            if (string.IsNullOrEmpty(key)) return;
            if (string.IsNullOrEmpty(value)) return;

            string? theKey = nvc[key];
            if (!string.IsNullOrEmpty(theKey)) // Key Exist
            {
                string tempVal = theKey;
                tempVal += "," + value;
                nvc.Remove(key);
                nvc.Add(key, tempVal);
            }
            else
            {
                nvc.Add(key, value);
            }
        }
        catch (Exception) { }
    }

    /// <summary>
    /// Get Value By Key
    /// </summary>
    /// <returns>Returns string.Empty If Key Not Exist Or Value Is Empty.</returns>
    public static string GetValueByKey(this NameValueCollection nvc, string? key)
    {
        string result = string.Empty;
        if (string.IsNullOrWhiteSpace(key)) return result;

        try
        {
            string? value = nvc[key];
            result = value ?? string.Empty;
        }
        catch (Exception) { }

        return result;
    }

    public static void MoveTo<T>(this List<T> list, int fromIndex, int toIndex)
    {
        try
        {
            if (fromIndex < 0 || fromIndex > list.Count - 1) return;
            if (toIndex < 0 || toIndex > list.Count - 1) return;
            if (fromIndex == toIndex) return;

            T t = list[fromIndex];

            list.RemoveAt(fromIndex);
            list.Insert(toIndex, t);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections MoveTo<T>: " + ex.Message);
        }
    }

    public static void MoveTo<T>(this List<T> list, T item, int toIndex)
    {
        try
        {
            int fromIndex = list.IndexOf(item);
            list.MoveTo(fromIndex, toIndex);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections MoveTo<T>: " + ex.Message);
        }
    }

    public static int CountDuplicates<T>(this List<T> list)
    {
        try
        {
            HashSet<T> hashset = new();
            int count = 0;
            for (int n = 0; n < list.Count; n++)
            {
                T item = list[n];
                if (!hashset.Add(item)) count++;
            }
            return count;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections CountDuplicates<T>: " + ex.Message);
            return 0;
        }
    }

    public static int CountDuplicates<T>(this List<T> list, out Dictionary<T, int> report) where T : notnull
    {
        report = new();

        try
        {
            int totalCount = 0;
            Dictionary<T, int> duplicates = new();
            for (int n = 0; n < list.Count; n++)
            {
                T item = list[n];
                if (duplicates.ContainsKey(item))
                {
                    duplicates[item]++;
                    totalCount++;
                }
                else
                {
                    duplicates.TryAdd(item, 1);
                }
            }

            // Remove Items Where Count Is 1
            foreach (KeyValuePair<T, int> kvp in duplicates)
            {
                if (kvp.Value > 1) report.TryAdd(kvp.Key, kvp.Value);
            }
            
            return totalCount;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections CountDuplicates<T>(out _): " + ex.Message);
            return 0;
        }
    }

    public static string ToString<T>(this List<T> list, char separator)
    {
        string result = string.Empty;

        try
        {
            if (list.Count > 0) result = string.Join(separator, list);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections ToString<T> Char: " + ex.Message);
        }

        return result;
    }

    public static string ToString<T>(this List<T> list, string separator)
    {
        string result = string.Empty;

        try
        {
            //for (int n = 0; n < list.Count; n++)
            //{
            //    T t = list[n];
            //    result += $"{t}{separator}";
            //}
            //if (result.EndsWith(separator)) result = result.TrimEnd(separator);
            if (list.Count > 0) result = string.Join(separator, list);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections ToString<T> String: " + ex.Message);
        }

        return result;
    }

    public static bool IsContain<T>(this List<T> list, T t)
    {
        try
        {
            for (int n = 0; n < list.Count; n++)
                if (t != null && t.Equals(list[n])) return true;
        }
        catch (Exception) { }
        return false;
    }

    public static List<List<T>> SplitToLists<T>(this List<T> list, int nSize)
    {
        List<List<T>> listOut = new();

        try
        {
            for (int n = 0; n < list.Count; n += nSize)
            {
                listOut.Add(list.GetRange(n, Math.Min(nSize, list.Count - n)));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections SplitToLists: " + ex.Message);
        }

        return listOut;
    }

    public static List<T> MergeLists<T>(this List<List<T>> lists)
    {
        List<T> listOut = new();

        try
        {
            for (int n = 0; n < lists.Count; n++)
            {
                listOut.AddRange(lists[n]);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections MergeLists: " + ex.Message);
        }

        return listOut;
    }

    public static List<string> SplitToLines(this string s, StringSplitOptions stringSplitOptions = StringSplitOptions.None)
    {
        try
        {
            return s.ReplaceLineEndings().Split(Environment.NewLine, stringSplitOptions).ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections SplitToLines: " + ex.Message);
        }

        return new List<string>();
    }

    public static int GetIndex<T>(this List<T> list, T value)
    {
        try
        {
            return list.FindIndex(_ => _ != null && _.Equals(value));
            // If the item is not found, it will return -1
        }
        catch (Exception)
        {
            return -1;
        }
    }

    public static void ChangeValue<T>(this List<T> list, T oldValue, T newValue)
    {
        try
        {
            list[list.IndexOf(oldValue)] = newValue;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections ChangeValue<T>: " + ex.Message);
        }
    }

    public static void RemoveValue<T>(this List<T> list, T value)
    {
        try
        {
            list.RemoveAt(list.IndexOf(value));
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections RemoveValue<T>: " + ex.Message);
        }
    }

    public static List<T> RemoveDuplicates<T>(this List<T> list)
    {
        try
        {
            List<T> NoDuplicates = list.Distinct().ToList();
            return NoDuplicates;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections RemoveDuplicates: " + ex.Message);
            return list;
        }
    }

    /// <summary>
    /// Distinct By More Than One Property
    /// </summary>
    /// <param name="keySelector">e.g. DistinctByProperties(x => new { x.A, x.B });</param>
    public static List<TSource> DistinctByProperties<TSource, TKey>(this List<TSource> source, Func<TSource, TKey> keySelector)
    {
        try
        {
            HashSet<TKey> hashSet = new();
            return source.Where(_ => hashSet.Add(keySelector(_))).ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections DistinctByProperties: " + ex.Message);
            return source;
        }
    }

    public static bool Compare(this List<string> list1, List<string> list2)
    {
        return Enumerable.SequenceEqual(list1, list2);
    }

    public static async Task SaveToFileAsync(this List<string> list, string filePath)
    {
        try
        {
            FileStreamOptions streamOptions = new()
            {
                Access = FileAccess.ReadWrite,
                Share = FileShare.ReadWrite,
                Mode = FileMode.Create,
                Options = FileOptions.RandomAccess
            };
            using StreamWriter file = new(filePath, streamOptions);
            for (int n = 0; n < list.Count; n++)
                if (list[n] != null)
                {
                    await file.WriteLineAsync(list[n]);
                }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Extensions_Collections SaveToFileAsync: {ex.Message}");
        }
    }

    public static async Task LoadFromFileAsync(this List<string> list, string filePath, bool ignoreEmptyLines, bool trimLines)
    {
        try
        {
            if (!File.Exists(filePath)) return;
            string content = await File.ReadAllTextAsync(filePath);
            List<string> lines = content.SplitToLines();
            for (int n = 0; n < lines.Count; n++)
            {
                string line = lines[n];
                if (ignoreEmptyLines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        if (trimLines) list.Add(line.Trim());
                        else list.Add(line);
                    }
                }
                else
                {
                    if (trimLines) list.Add(line.Trim());
                    else list.Add(line);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections LoadFromFileAsync: " + ex.Message);
        }
    }

}