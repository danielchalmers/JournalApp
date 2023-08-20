﻿using System.Text.Json;

namespace JournalApp;

public static class PreferenceUtil
{
    public static T GetJson<T>(this IPreferences preferences, string key)
    {
        var jsonString = preferences.Get(key, "");

        if (string.IsNullOrWhiteSpace(jsonString))
            return default;

        return JsonSerializer.Deserialize<T>(jsonString);
    }

    public static void SetJson<T>(this IPreferences preferences, string key, T obj)
    {
        preferences.Set(key, JsonSerializer.Serialize(obj));
    }
}
