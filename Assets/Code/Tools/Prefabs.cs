using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Code.Tools
{
    public static class Prefabs
    {
        private static readonly Dictionary<string, GameObject> _prefabs;

        private static readonly string[] _names =
        {
            "Holder",
            "Plain",
            "Rock",
            "Water",
            "Forest",
            "House - wood, 1",
            "House - wood, 2",
        };

        static Prefabs()
        {
            _prefabs = new Dictionary<string, GameObject>(_names.Length);
            foreach (var name in _names)
            {
                _prefabs[name] = (GameObject) Resources.Load(name);
            }
        }

        public static GameObject Get(string name)
        {
            return _prefabs[name];
        }
    }
}