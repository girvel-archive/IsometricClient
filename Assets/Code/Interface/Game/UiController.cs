using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Assets.Code.Game;
using Assets.Code.Tools.Prefabs;
using CommonStructures;
using UnityEngine;
using UnityEngine.UI;
using Resources = UnityEngine.Resources;

namespace Assets.Code.Interface.Game
{
    public class UiController : MonoBehaviour
    {
        public class BuildingAction
        {
            public static readonly Sprite[] DefaultKeysSprites;

            public static readonly string[] DefaultKeyNames =
            {
                "Q", "W", "E", "R", "T",
            };



            static BuildingAction()
            {
                DefaultKeysSprites
                    = DefaultKeyNames.Select(
                        name => SpritesContainer.Instance.Get(name))
                        .ToArray();
            }



            public CommonBuildingAction Common { get; set; }

            public Sprite KeySprite { get; set; }

            public string KeyName { get; set; }



            public BuildingAction(Sprite sprite, string keyName, CommonBuildingAction common)
            {
                Common = common;
                KeySprite = sprite;
                KeyName = keyName;
            }

            public BuildingAction(CommonBuildingAction common, int n)
            {
                Common = common;
                KeySprite = DefaultKeysSprites[n];
                KeyName = DefaultKeyNames[n];
            }
        }



        public static bool ContextMenuActive { get; private set; }

        public const string ConnectionStatusConnection = "connection";
        public const string ConnectionStatusConnected = "connected";

        public static Building ContextMenuBuilding
        {
            get { return _contextMenuBuilding; }
            set
            {
                if (value == null)
                {
                    ContextMenuActive = false;
                }
                else
                {
                    ViewBuilding(value);
                }
                    
                _contextMenuBuilding = value;
            }
        }

        private static Building _contextMenuBuilding;



        public delegate void BuildingChoosedEvent(string name, Vector2 isometricPosition);

        public delegate void ActionChoosedEvent(BuildingAction action);

        public static event BuildingChoosedEvent BuildingChoosed;
        public static event ActionChoosedEvent ActionChoosed;



        public static ReadOnlyCollection<CommonBuildingAction> BuildingActions
        {
            get { return _buildingActions; }
            set
            {
                // TODO присваивается 2 раза вместо одного. Раскомментируй строку ниже
                // Debug.Log("building actions setting");

                _buildingActions = value;

                CurrentBuildingActions = new List<BuildingAction>();
                if (BuildingActions != null)
                {
                    var i = 0;
                    foreach (var buildingAction in BuildingActions)
                    {
                        CurrentBuildingActions.Add(new BuildingAction(buildingAction, i++));
                    }
                }

                CurrentBuildingActions.Add(
                    new BuildingAction(
                        Resources.Load<Sprite>("Space"), 
                        "Space", 
                        new CommonBuildingAction(true, "Exit", new CommonBuilding(), -1)));

                for (var i = 0; i < Ui.ContextButtonsContainer.Transform.childCount; i++)
                {
                    Destroy(Ui.ContextButtonsContainer.Transform.GetChild(i).gameObject);
                }

                var i2 = 0;
                foreach (var currentBuildingAction in CurrentBuildingActions)
                {
                    var currentButton = Instantiate(Resources.Load<GameObject>("Context menu button info"));

                    var currentText = currentButton.GetComponentInChildren<Text>();
                    var currentImage = currentButton.GetComponentInChildren<Image>();

                    currentText.text = currentBuildingAction.Common.Name;
                    currentImage.sprite = currentBuildingAction.KeySprite; 

                    if (!currentBuildingAction.Common.Active)
                    {
                        currentText.color = new Color(0.5f, 0.5f, 0.5f);
                        currentImage.color = new Color(0.5f, 0.5f, 0.5f);
                    }

                    currentButton.transform.SetParent(Ui.ContextButtonsContainer.Transform, true);
                    currentButton.transform.localPosition = new Vector3(0, i2 * ButtonsContainerController.ButtonsDistance);
                    
                    i2++;
                }
            }
        }
        private static ReadOnlyCollection<CommonBuildingAction> _buildingActions;

        protected static List<BuildingAction> CurrentBuildingActions;

        protected const float ContextMenuSubjectSizeFactor = 4;

        protected static float ContextMenuDelaySecs = 0.2f;
        protected static float CurrentContextMenuDelaySecs;
        protected static float ContextMenuBackgroundMaxAlpha = 0.8f;



        protected virtual void Start()
        {
            Ui.RegistrationEmailForm.GameObject.SetActive(false);
            Ui.RegistrationNumbersForm.GameObject.SetActive(false);
            Ui.RegistrationUserDataForm.GameObject.SetActive(false);
            Ui.RegistrationForms.GameObject.SetActive(false);

            Building.OnBuildingSpriteChanged += building =>
            {
                if (ContextMenuBuilding == building)
                {
                    Ui.ContextBuildingImage.Image.sprite = building.BuildingSpriteRenderer.sprite;
                }
            };
        }

        protected virtual void Update()
        {
            KeysUpdate();
            ContextMenuUpdate();
        }



        protected static void ViewBuilding(Building building)
        {
            BuildingActions = null;

            if (BuildingChoosed != null)
            {
                try
                {
                    BuildingChoosed(
                        building.Instance.name,
                        building.BuildingIsometricController.IsometricPosition);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            
            Ui.ContextBuildingImage.Image.sprite = building.BuildingSpriteRenderer.sprite;
            Ui.ContextHolderImage.Image.sprite = building.HolderController.UnactiveSprite;

            CameraController.TargetPosition
                = building.Instance.transform.position + new Vector3(0, 0, -1);

            ContextMenuActive = true;
        }

        private static void KeysUpdate()
        {
            if (CurrentBuildingActions == null || CurrentBuildingActions.Count < 1)
                return;

            CurrentBuildingActions.ForEach(action => {
                if (!Input.GetButtonDown(action.KeyName) || !action.Common.Active)
                    return;

                if (action.KeyName == "Space")
                {
                    ContextMenuBuilding = null;
                }
                else if (ActionChoosed != null)
                {
                    try
                    { 
                        ActionChoosed(action);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            });
        }

        private static void ContextMenuUpdate()
        {
            if (ContextMenuActive)
            {
                if (CurrentContextMenuDelaySecs < ContextMenuDelaySecs)
                {
                    CurrentContextMenuDelaySecs += Time.deltaTime;
                }
                else
                {
                    return;
                }
            }
            else if (CurrentContextMenuDelaySecs > 0)
            {
                CurrentContextMenuDelaySecs -= Time.deltaTime;
            }
            else
            {
                return;
            }

            foreach (var image 
                in Ui.ContextOpaqueElementsContainer.Transform.GetComponentsInChildren<MaskableGraphic>())
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b,
                        CurrentContextMenuDelaySecs / ContextMenuDelaySecs);
            }

            Ui.ContextBackgroundImage.Image.color = new Color(1, 1, 1,
                ContextMenuBackgroundMaxAlpha * CurrentContextMenuDelaySecs / ContextMenuDelaySecs);
        }
    }
}