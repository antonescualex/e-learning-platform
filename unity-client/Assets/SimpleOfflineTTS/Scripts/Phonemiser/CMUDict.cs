using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SimpleOfflineTTS
{
    /// <summary>
    /// Holds the CMU Pronouncing Dictionary
    /// </summary>
    public class CMUDict
    {
        private Dictionary<string, string[]> _dict;

        public CMUDict(TextAsset cmuDictAsset)
        {
            LoadFromAsset(cmuDictAsset);
        }

        /// <summary>
        /// Load CMUDict from file. Only happens once.
        /// </summary>
        void LoadFromAsset(TextAsset cmuDictAsset)
        {
            _dict = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

            if (cmuDictAsset == null)
            {
                Debug.LogError("No CMUDict asset was set.");
                return;
            }

            // Split the TextAsset text into lines
            var lines = cmuDictAsset.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";;;"))
                {
                    continue;
                }

                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    continue;
                }

                var word = parts[0];
                var phonemes = parts.Skip(1).ToArray();

                if (!_dict.ContainsKey(word))
                {
                    _dict[word] = phonemes;
                }
            }

            Debug.Log($"Loaded {_dict.Count} entries from CMUDict.");
        }

        public bool TryGetPhonemes(string word, out string[] phonemes)
        {
            return _dict.TryGetValue(word, out phonemes);
        }
    }
}
