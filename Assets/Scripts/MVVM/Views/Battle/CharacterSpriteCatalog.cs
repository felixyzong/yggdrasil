using System;
using System.Collections.Generic;
using UnityEngine;

namespace MVVM.Views.Battle
{
    [CreateAssetMenu(menuName = "Yggdrasil/Battle/Character Sprite Catalog")]
    public class CharacterSpriteCatalog : ScriptableObject
    {
        [SerializeField] private List<Entry> entries = new List<Entry>();
        [SerializeField] private Sprite fallbackSprite;

        public Sprite GetSprite(string spriteKey)
        {
            if (string.IsNullOrWhiteSpace(spriteKey)) return fallbackSprite;

            foreach (var entry in entries)
            {
                if (entry != null && entry.Key == spriteKey)
                {
                    return entry.Sprite != null ? entry.Sprite : fallbackSprite;
                }
            }

            return fallbackSprite;
        }

        [Serializable]
        private class Entry
        {
            [SerializeField] private string key;
            [SerializeField] private Sprite sprite;

            public string Key => key;
            public Sprite Sprite => sprite;
        }
    }
}
