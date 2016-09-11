using System;
using System.Collections.Generic;
using Assets.Code.Game;

namespace Assets.Code.Tools.Prefabs
{
    public class BuildingPrefabsContainer
    {
        #region Singleton

        [Obsolete("using backing field")]
        private static BuildingPrefabsContainer _instance;

        #pragma warning disable 618

        public static BuildingPrefabsContainer Instance
        {
            get { return _instance ?? (_instance = new BuildingPrefabsContainer()); }
        }

        #pragma warning restore 618

        #endregion



        private readonly Dictionary<string, Building> Prefabs;

        private readonly string[] Names =
        {
            "Holder",
            "Plain",
            "Rock",
            "Water",
            "Forest",
            "House - wood, 1",
            "House - wood, 2",
        };

        private BuildingPrefabsContainer()
        {
            Prefabs = new Dictionary<string, Building>(Names.Length);
            foreach (var name in Names)
            {
                Prefabs[name] = new Building(
                    PrefabsContainer.Instance.Get(name), 
                    PrefabsContainer.Instance.Get("Holder"));
            }
        }

        public Building Get(string name)
        {
            return Prefabs[name];
        }
    }
}