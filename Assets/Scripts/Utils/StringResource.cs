using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace FlavBattle.Resources
{
    public enum StringResourceCategory
    {
        General = 0,
        Dialog = 1,
        UI = 2,
    }

    [Serializable]
    public class StringResource
    {
        public StringResourceCategory Category;

        public string Key;

        public string Text
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Key))
                {
                    return string.Empty;
                }

                var map = StringResources.GetStrings();
                return map.Get(Category, Key) ?? string.Empty;
            }
        }
    }

    public class StringResourceMap
    {
        private Dictionary<StringResourceCategory, Dictionary<string, string>> _values { get; } = new Dictionary<StringResourceCategory, Dictionary<string, string>>();

        public void Set(StringResourceCategory category, string key, string value)
        {
            if (!_values.ContainsKey(category)) 
            {
                _values[category] = new Dictionary<string, string>();
            }

            _values[category][key] = value;
        }

        public string Get(StringResourceCategory category, string key)
        {
            var map = _values.GetValueOrDefault(category);
            if (map == null)
            {
                Debug.LogWarning($"Warning: category not found: {category}");
                return null;
            }

            var value = map.GetValueOrDefault(key);
            if (value == null)
            {
                Debug.LogWarning($"Warning: key not found: category [{category}] key [{key}]");
                return null;
            }

            return value;
        }

        public IEnumerable<string> GetKeys(StringResourceCategory category)
        {
            var map = _values.GetValueOrDefault(category);
            if (map == null)
            {
                Debug.LogWarning($"Warning: category not found: {category}");
                return new List<string>();
            }

            return map.Keys.ToList();
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(_values, Formatting.Indented);
        }
    }

    public static class StringResources
    {
        public static StringResourceMap _cache = null;

        public static void UpdateStrings(StringResourceMap map)
        {
            _cache = map;
        }

        public static TextAsset GetTextAsset()
        {
            return UnityEngine.Resources.Load<TextAsset>("Strings/strings");
        }

        /// <summary>
        /// Gets and parses the list of strings from Resources/Strings/strings.json.
        /// </summary>
        /// <param name="refresh">If false, will get cached value of strings if available. If true,
        /// will acquire and parse it (such as if it's expected to be updated).</param>
        /// <returns></returns>
        public static StringResourceMap GetStrings(bool refresh = false)
        {
            if (_cache != null && !refresh)
            {
                return _cache;
            }

            var textAsset = GetTextAsset();

            var resources = new StringResourceMap();

            var parsed = JObject.Parse(textAsset.text);
            
            foreach (StringResourceCategory categoryType in Enum.GetValues(typeof(StringResourceCategory)))
            {
                var category = parsed[categoryType.ToString()];
                if (category == null) {
                    Debug.LogWarning($"Warning: No category found in strings for {categoryType}");
                    continue;
                }

                var jobject = category.ToObject<JObject>();
                foreach (var pair in jobject)
                {
                    var val = pair.Value.ToString() ?? "unknown";
                    resources.Set(categoryType, pair.Key, val);
                }
            }

            return resources;
        }
    }
}