using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Code.Game
{
    public class Building
    {
        public GameObject Instance { get; set; }

        public BuildingController BuildingController { get; set; }

        public IsometricController BuildingIsometricController { get; set; }

        public SpriteRenderer BuildingSpriteRenderer { get; set; }



        public GameObject Holder { get; set; }

        public HolderController HolderController { get; set; }

        public IsometricController HolderIsometricController { get; set; }

        public SpriteRenderer HolderSpriteRenderer { get; set; }



        public static event Action<Building> OnBuildingSpriteChanged;



        public Building(GameObject instance, GameObject holder)
        {
            Instance = instance;
            Holder = holder;

            BuildingController = Instance.GetComponent<BuildingController>();
            BuildingIsometricController = Instance.GetComponent<IsometricController>();
            BuildingSpriteRenderer = Instance.GetComponent<SpriteRenderer>();

            HolderController = Holder.GetComponent<HolderController>();
            HolderIsometricController = Holder.GetComponent<IsometricController>();
            HolderSpriteRenderer = Holder.GetComponent<SpriteRenderer>();
        }



        public Building Instantiate()
        {
            return new Building(
                Object.Instantiate(Instance),
                Object.Instantiate(Holder));
        }

        public void ChangeBuildingSprite(Sprite sprite)
        {
            BuildingSpriteRenderer.sprite = sprite;

            if (OnBuildingSpriteChanged != null)
            {
                OnBuildingSpriteChanged(this);
            }
        }
    }
}