using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;

namespace MsmhToolsClass;

public static class Extensions_Collections
{
    public static void Add<K, V>(this IMemoryCache cache, K key, V factory) where K : notnull
    {
        try
        {
            _ = cache.GetOrCreate(key, _ => factory);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections IMemoryCache Add: " + ex.GetInnerExceptions());
        }
    }

    public static void Add<K, V>(this IMemoryCache cache, K key, V factory, TimeSpan absoluteExpirationRelativeToNow) where K : notnull
    {
        try
        {
            _ = cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
                return factory;
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections IMemoryCache Add: " + ex.GetInnerExceptions());
        }
    }

    public static bool TryAdd<K, V>(this IMemoryCache cache, K key, V factory) where K : notnull
    {
        try
        {
            V? existing = cache.GetOrCreate(key, _ => factory);
            return ReferenceEquals(existing, factory);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections IMemoryCache TryAdd: " + ex.GetInnerExceptions());
            return false;
        }
    }

    public static bool TryAdd<K, V>(this IMemoryCache cache, K key, V factory, TimeSpan absoluteExpirationRelativeToNow) where K : notnull
    {
        try
        {
            V? existing = cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
                return factory;
            });

            return ReferenceEquals(existing, factory);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections IMemoryCache TryAdd: " + ex.GetInnerExceptions());
            return false;
        }
    }

    /// <summary>
    /// It's Not Atomic
    /// </summary>
    public static void Update<K, V>(this IMemoryCache cache, K key, V factory) where K : notnull
    {
        try
        {
            bool exist = cache.TryGetValue(key, out _);
            if (exist) _ = cache.Set(key, factory); // Set Is Equal To AddOrUpdate But It's Not Atomic
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections IMemoryCache Update: " + ex.GetInnerExceptions());
        }
    }

    /// <summary>
    /// It's Not Atomic
    /// </summary>
    public static void Update<K, V>(this IMemoryCache cache, K key, V factory, TimeSpan absoluteExpirationRelativeToNow) where K : notnull
    {
        try
        {
            bool exist = cache.TryGetValue(key, out _);
            if (exist) _ = cache.Set(key, factory, absoluteExpirationRelativeToNow);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections IMemoryCache Update: " + ex.GetInnerExceptions());
        }
    }

    /// <summary>
    /// It's Not Atomic
    /// </summary>
    public static bool TryUpdate<K, V>(this IMemoryCache cache, K key, V factory) where K : notnull
    {
        try
        {
            bool exist = cache.TryGetValue(key, out _);
            if (exist) _ = cache.Set(key, factory);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections IMemoryCache TryUpdate: " + ex.GetInnerExceptions());
            return false;
        }
    }

    /// <summary>
    /// It's Not Atomic
    /// </summary>
    public static bool TryUpdate<K, V>(this IMemoryCache cache, K key, V factory, TimeSpan absoluteExpirationRelativeToNow) where K : notnull
    {
        try
        {
            bool exist = cache.TryGetValue(key, out _);
            if (exist) _ = cache.Set(key, factory, absoluteExpirationRelativeToNow);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections IMemoryCache TryUpdate: " + ex.GetInnerExceptions());
            return false;
        }
    }

    public static bool TryRemove<K>(this IMemoryCache cache, K key) where K : notnull
    {
        try
        {
            bool exist = cache.TryGetValue(key, out _);
            if (exist)
            {
                cache.Remove(key);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections IMemoryCache TryRemove: " + ex.GetInnerExceptions());
            return false;
        }
    }

    public static bool TryRemove<K, V>(this IMemoryCache cache, K key, out V? value) where K : notnull
    {
        try
        {
            bool exist = cache.TryGetValue(key, out value);
            if (exist)
            {
                cache.Remove(key);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections IMemoryCache TryRemove: " + ex.GetInnerExceptions());
            value = default;
            return false;
        }
    }

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
            Debug.WriteLine("Extensions_Collections ConcurrentDictionary TryUpdate: " + ex.Message);
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
            Debug.WriteLine("Extensions_Collections ConcurrentDictionary AddOrUpdate: " + ex.Message);
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
                Debug.WriteLine("Extensions_Collections List ToConcurrentDictionary: " + ex.Message);
            }
        }
        return keyValuePairs;
    }

    public static ConcurrentDictionary<uint, T> ToConcurrentDictionary<T>(this ObservableCollection<T> list) where T : notnull
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
                Debug.WriteLine("Extensions_Collections ObservableCollection ToConcurrentDictionary: " + ex.Message);
            }
        }
        return keyValuePairs;
    }

    public static Dictionary<uint, T> ToDictionary<T>(this List<T> list) where T : notnull
    {
        Dictionary<uint, T> keyValuePairs = new();
        for (int n = 0; n < list.Count; n++)
        {
            try
            {
                keyValuePairs.TryAdd(Convert.ToUInt32(n), list[n]);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Extensions_Collections List ToDictionary: " + ex.Message);
            }
        }
        return keyValuePairs;
    }

    public static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        try
        {
            return new SortedDictionary<TKey, TValue>(dictionary);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections Dictionary ToSortedDictionary: " + ex.Message);
            return new SortedDictionary<TKey, TValue>();
        }
    }

    public static Dictionary<TKey, TValue> SortByKey<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        try
        {
            return dictionary.OrderBy(_ => _.Key).ToDictionary(_ => _.Key, _ => _.Value);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections Dictionary SortByKey: " + ex.Message);
            return new Dictionary<TKey, TValue>();
        }
    }

    public static Dictionary<TKey, TValue> SortByValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        try
        {
            return dictionary.OrderBy(_ => _.Value).ToDictionary(_ => _.Key, _ => _.Value);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections Dictionary SortByValue: " + ex.Message);
            return new Dictionary<TKey, TValue>();
        }
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

    public static List<TOutput> ConvertAll<TInput,TOutput>(this List<TInput> list, Converter<TInput, TOutput> converter)
    {
        List<TOutput> outputList = new();

        try
        {
            for (int n = 0; n < list.Count; n++)
            {
                TInput input = list[n];
                if (input == null) continue;
                try
                {
                    TOutput output = converter(input);
                    outputList.Add(output);
                }
                catch (Exception) { }
            }
        }
        catch (Exception) { }

        return outputList;
    }

    public static List<int> ToIntList(this IEnumerable<string> list)
    {
        List<int> result = new();

        try
        {
            foreach (string item in list)
            {
                string str = item.Trim();
                if (string.IsNullOrEmpty(str)) continue;
                bool isSuccess = int.TryParse(str, out int resultVal);
                if (isSuccess) result.Add(resultVal);
            }
        }
        catch (Exception) { }

        return result;
    }

    public static List<int> ToIntList(this List<string> list)
    {
        List<int> result = new();

        try
        {
            for (int n = 0; n < list.Count; n++)
            {
                string str = list[n].Trim();
                if (string.IsNullOrEmpty(str)) continue;
                bool isSuccess = int.TryParse(str, out int resultVal);
                if (isSuccess) result.Add(resultVal);
            }
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
            Debug.WriteLine("Extensions_Collections List MoveTo<T>: " + ex.Message);
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
            Debug.WriteLine("Extensions_Collections List MoveTo<T>: " + ex.Message);
        }
    }

    public static void MoveTo<T>(this ObservableCollection<T> list, int fromIndex, int toIndex)
    {
        try
        {
            if (fromIndex < 0 || fromIndex > list.Count - 1) return;
            if (toIndex < 0 || toIndex > list.Count - 1) return;
            if (fromIndex == toIndex) return;

            list.Move(fromIndex, toIndex);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections ObservableCollection MoveTo<T>: " + ex.Message);
        }
    }

    public static void MoveTo<T>(this ObservableCollection<T> list, T item, int toIndex)
    {
        try
        {
            int fromIndex = list.IndexOf(item);
            list.MoveTo(fromIndex, toIndex);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections ObservableCollection MoveTo<T>: " + ex.Message);
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
            Debug.WriteLine("Extensions_Collections List CountDuplicates<T>: " + ex.Message);
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
            Debug.WriteLine("Extensions_Collections List CountDuplicates<T>(out _): " + ex.Message);
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
            Debug.WriteLine("Extensions_Collections List ToString<T> Char: " + ex.Message);
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
            Debug.WriteLine("Extensions_Collections List ToString<T> String: " + ex.Message);
        }

        return result;
    }

    public static bool IsContain<T>(this List<T> list, T t) where T : notnull
    {
        try
        {
            for (int n = 0; n < list.Count; n++)
                if (t.Equals(list[n])) return true;
        }
        catch (Exception) { }
        return false;
    }

    public static bool IsContainPartial(this List<string> list, string partialText, StringComparison stringComparison = StringComparison.InvariantCulture)
    {
        try
        {
            for (int n = 0; n < list.Count; n++)
            {
                string item = list[n];
                if (item.Contains(partialText, stringComparison)) return true;
            }
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
            Debug.WriteLine("Extensions_Collections List SplitToLists: " + ex.Message);
        }

        return listOut;
    }

    public static List<ObservableCollection<T>> SplitToLists<T>(this ObservableCollection<T> list, int nSize)
    {
        List<ObservableCollection<T>> listOut = new();

        try
        {
            for (int n = 0; n < list.Count; n += nSize)
            {
                listOut.Add(list.GetRange(n, Math.Min(nSize, list.Count - n)));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections ObservableCollection SplitToLists: " + ex.Message);
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
            Debug.WriteLine("Extensions_Collections List<List<T>> MergeLists: " + ex.Message);
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

    public static T? GetRandomValue<T>(this List<T> list)
    {
        try
        {
            if (list.Count > 0)
            {
                Random random = new();
                int index = random.Next(0, list.Count - 1);
                return list[index];
            }
        }
        catch (Exception) { }
        return default;
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
            Debug.WriteLine("Extensions_Collections List ChangeValue<T>: " + ex.Message);
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
            Debug.WriteLine("Extensions_Collections List RemoveValue<T>: " + ex.Message);
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
            Debug.WriteLine("Extensions_Collections List RemoveDuplicates: " + ex.Message);
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
            Debug.WriteLine("Extensions_Collections List DistinctByProperties: " + ex.Message);
            return source;
        }
    }

    /// <summary>
    /// Distinct By More Than One Property
    /// </summary>
    /// <param name="keySelector">e.g. DistinctByProperties(x => new { x.A, x.B });</param>
    public static ObservableCollection<TSource> DistinctByProperties<TSource, TKey>(this ObservableCollection<TSource> source, Func<TSource, TKey> keySelector)
    {
        try
        {
            HashSet<TKey> hashSet = new();
            return source.Where(_ => hashSet.Add(keySelector(_))).ToObservableCollection();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections ObservableCollection DistinctByProperties: " + ex.Message);
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
            Debug.WriteLine($"Extensions_Collections List SaveToFileAsync: {ex.Message}");
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
            Debug.WriteLine("Extensions_Collections List LoadFromFileAsync: " + ex.Message);
        }
    }

    public static void AddRange<T>(this ObservableCollection<T> list, IEnumerable<T> collection)
    {
        try
        {
            foreach (T item in collection)
            {
                if (item != null) list.Add(item);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections ObservableCollection AddRange<T>: " + ex.Message);
        }
    }

    public static ObservableCollection<T> GetRange<T>(this ObservableCollection<T> list, int index, int count)
    {
        try
        {
            return list.Skip(index).Take(count).ToObservableCollection();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections ObservableCollection GetRange<T>: " + ex.Message);
            return new ObservableCollection<T>();
        }
    }

    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
    {
        try
        {
            return new ObservableCollection<T>(enumerable);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_Collections IEnumerable ToObservableCollection<T>: " + ex.Message);
            return new ObservableCollection<T>();
        }
    }

}