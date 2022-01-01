using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace FlavBattle.Resources
{
    using CategoryMap = Dictionary<string, string>;

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
        private Dictionary<StringResourceCategory, CategoryMap> _values { get; } = new Dictionary<StringResourceCategory, CategoryMap>();

        private Dictionary<StringResourceCategory, SortedSet<string>> _sortedKeys { get; } = new Dictionary<StringResourceCategory, SortedSet<string>>();

        public void Set(StringResourceCategory category, string key, string value)
        {
            if (!_values.ContainsKey(category)) 
            {
                _values[category] = new CategoryMap();    
            }

            if (!_sortedKeys.ContainsKey(category))
            {
                _sortedKeys[category] = new SortedSet<string>();
            }

            _sortedKeys[category].Add(key);
            _values[category][key] = value;
        }

        private CategoryMap GetCategoryMap(StringResourceCategory category)
        {
            var map = _values.GetValueOrDefault(category);
            if (map == null)
            {
                Debug.LogWarning($"Warning: category not found: {category}");
                return null;
            }

            return map;
        }

        public bool KeyExists(StringResourceCategory category, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            var map = GetCategoryMap(category);
            if (map == null)
            {
                return false;
            }

            return map.ContainsKey(key);
        }

        public string Get(StringResourceCategory category, string key)
        {
            var map = GetCategoryMap(category);
            if (map == null)
            {
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

            var set = _sortedKeys.GetValueOrDefault(category);
            if (set == null)
            {
                return null;
            }

            return set.ToList();
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(_values, Formatting.Indented);
        }
    }

    public static class StringResources
    {
        private static StringResourceMap _cache = null;
        private static TextAsset _asset = null;

        /// <summary>
        /// Updates the resource caches. This should only be used by the
        /// editor.
        /// </summary>
        public static void UpdateStrings(StringResourceMap map)
        {
            _cache = map;
        }

        /// <summary>
        /// Loads the string assets, or cached values thereof.
        /// </summary>
        public static TextAsset GetTextAsset()
        {
            if (_asset == null)
            {
                _asset = UnityEngine.Resources.Load<TextAsset>("Strings/strings");
            }

            return _asset;
        }

        /// <summary>
        /// Gets and parses the list of strings from Resources/Strings/strings.json.
        /// </summary>
        /// <param name="refresh">If false, will get cached value of strings if available. If true,
        /// will acquire and parse it (such as if it's expected to be updated).</param>
        /// <returns></returns>
        public static StringResourceMap GetStrings()
        {
            if (_cache != null)
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

            _cache = resources;
            return resources;
        }
    }
}