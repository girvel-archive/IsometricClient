using System;
using System.Collections.Generic;
using Assets.Code.Game;
using UnityEngine;

namespace Assets.Code.Tools.Prefabs
{
    public class PrefabsContainer
    {
        #region Singleton

        [Obsolete("using backing field")]
        private static PrefabsContainer _instance;

        #pragma warning disable 618

        public static PrefabsContainer Instance
        {
            get { return _instance ?? (_instance = new PrefabsContainer()); }
        }

        #pragma warning restore 618

        #endregion



        private readonly Dictionary<string, GameObject> _prefabs;

        private readonly string[] _names =
        {
            "Holder",
            "Plain",
            "Rock",
            "Water",
            "Forest",
            "House - wood, 1",
            "House - wood, 2",
        };



        private PrefabsContainer()
        {
            _prefabs = new Dictionary<string, GameObject>(_names.Length);
            foreach (var name in _names)
            {
                _prefabs[name] = Resources.Load<GameObject>(name);
            }
        }

        public GameObject Get(string name)
        {
            return _prefabs[name];
        }
    }
}