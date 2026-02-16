using System;
using System.Collections.Generic;

namespace JetBrainsRecentProjectCmdPal.Helper;

public class CacheManager<T>
{
    private readonly Dictionary<string, (T Value, DateTime CacheTime)> _cache = new();
    private readonly TimeSpan _expiry;

    public CacheManager(TimeSpan expiry)
    {
        _expiry = expiry;
    }

    public bool TryGet(string key, out T? value)
    {
        value = default;

        if (!_cache.TryGetValue(key, out var cachedItem))
            return false;

        if (DateTime.Now - cachedItem.CacheTime >= _expiry)
        {
            _cache.Remove(key);
            return false;
        }

        value = cachedItem.Value;
        return true;
    }

    public void Set(string key, T value)
    {
        _cache[key] = (value, DateTime.Now);
    }

    public void Clear()
    {
        _cache.Clear();
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }
}