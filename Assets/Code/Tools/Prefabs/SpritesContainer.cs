using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Code.Tools.Prefabs
{
    public class SpritesContainer
    {
        #region Singleton

        [Obsolete("using backing field")]
        private static SpritesContainer _instance;

        #pragma warning disable 618

        public static SpritesContainer Instance
        {
            get { return _instance ?? (_instance = new SpritesContainer()); }
        }

        #pragma warning restore 618

        #endregion



        private readonly Dictionary<string, Sprite> _prefabs;

        private readonly string[] _names =
        {
            "Q", "W", "E", "R", "T",
        };



        private SpritesContainer()
        {
            _prefabs = new Dictionary<string, Sprite>(_names.Length);
            foreach (var name in _names)
            {
                _prefabs[name] = Resources.Load<Sprite>(name);
            }
        }

        public Sprite Get(string name)
        {
            return _prefabs[name];
        }
    }
}