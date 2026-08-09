using System.Text.Json;
using System.Threading;

namespace Visual_Block_Studio.Services;

public static class Preferences<T>
{
    private static readonly string _filePath = GetFilePath();
    private static readonly Dictionary<string, JsonElement> _values = Load();
    private static readonly Lock _lock = new();

    private static string GetFilePath()
    {
        string dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ReligiousQuizCompetitionWinUI");

        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "preferences.json");
    }

    private static Dictionary<string, JsonElement> Load()
    {
        if (!File.Exists(_filePath))
            return [];

        try
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)
                   ?? [];
        }
        catch
        {
            // corrupt or unreadable file — start fresh rather than crash
            return [];
        }
    }

    private static void Save()
    {
        lock (_lock)
        {
            string json = JsonSerializer.Serialize(_values);
            File.WriteAllText(_filePath, json);
        }
    }

    public static bool ContainsKey(string key)
    {
        lock (_lock)
        {
            return _values.ContainsKey(key);
        }
    }

    public static void Clear()
    {
        lock (_lock)
        {
            _values.Clear();
            Save();
        }
    }

    public static T? Get(string key, T? defaultValue = default)
    {
        lock (_lock)
        {
            if (!_values.TryGetValue(key, out var element))
                return defaultValue;

            try
            {
                return element.Deserialize<T>();
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    public static void Remove(string key)
    {
        lock (_lock)
        {
            _values.Remove(key);
            Save();
        }
    }

    public static void Set(string key, T value)
    {
        lock (_lock)
        {
            var element = JsonSerializer.SerializeToElement(value);
            _values[key] = element;
            Save();
        }
    }
}
