using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace AstralAssault.Source.Game
{
    public static class AssetManager
    {
        private static readonly Dictionary<String, Texture2D> Textures = new();
        private static readonly Dictionary<String, Effect> Effects = new();
        private static readonly Dictionary<String, SoundEffect> SoundEffects = new();
        private static Game1 _root;

        public static void Init(Game1 root) {
            _root = root;
        }

        public static T Load<T>(String path) {
            Dictionary<String, T> activeDictionary;
            String activeDirectory;

            if (typeof(T) == typeof(Texture2D)) {
                activeDictionary = Textures as Dictionary<String, T>;
                activeDirectory = "Assets";
            }
            else if (typeof(T) == typeof(SoundEffect)) {
                activeDictionary = SoundEffects as Dictionary<String, T>;
                activeDirectory = "Assets";
            }
            else if (typeof(T) == typeof(Effect)) {
                activeDictionary = Effects as Dictionary<String, T>;
                activeDirectory = "Shaders";
            }
            else {
                throw new ArgumentException("T must be Texture2D, SoundEffect or Effect");
            }

            if (activeDictionary.ContainsKey(path))
                return activeDictionary[path];

            T asset = _root.Content.Load<T>($"{activeDirectory}/{path}");
            return asset;
        }
    }
}