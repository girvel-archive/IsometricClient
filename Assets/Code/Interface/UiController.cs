using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Assets.Code.Interface;
using UnityEngine;
using UnityEngine.UI;
using Assets.Code.Tools;
using Assets.Code.Game;

namespace Assets.Code.Interface
{
    public class UiController : MonoBehaviour
    {
        public class BuildingChoosedArgs : EventArgs
        {
            public string Name { get; private set; }
            public Vector2 IsometricPosition { get; private set; }

            public BuildingChoosedArgs(string name, Vector2 isometricPosition)
            {
                Name = name;
                IsometricPosition = isometricPosition;
            }
        }
        


        public class BuildingAction : NetController.BuildingAction
        {
            public static readonly Sprite[] DefaultKeysSprites = {
                Resources.Load<Sprite>("Q"),
                Resources.Load<Sprite>("W"),
                Resources.Load<Sprite>("E"),
                Resources.Load<Sprite>("R"),
                Resources.Load<Sprite>("T"),
            };

            public static readonly string[] DefaultKeyNames = {
                "Q", "W", "E", "R", "T"
            };

            public Sprite KeySprite { get; set; }
            public string KeyName { get; set; }

            public BuildingAction(string name, bool possible, Sprite sprite, string keyName)
                : base(name, possible)
            {

                KeySprite = sprite;
                KeyName = keyName;
            }

            public BuildingAction(NetController.BuildingAction buildingAction, int n)
                : base(buildingAction.Name, buildingAction.Possible)
            {

                KeySprite = DefaultKeysSprites[n];
                KeyName = DefaultKeyNames[n];
            }
        }
        


        public class ActionChoosedArgs : EventArgs
        {
            public BuildingAction Action { get; private set; }

            public ActionChoosedArgs(BuildingAction action)
            {
                Action = action;
            }
        }



        public enum ResourceType
        {
            Wood,
            Meat,
            Corn,
            Stone,
            People,
            Gold,
            Progress
        }



        public static bool ContextMenuActive { get; private set; }

        public const string ConnectionStatusConnection = "connection";
        public const string ConnectionStatusConnected = "connected";

        public static GameObject ContextMenuBuilding
        {
            get { return _contextMenuBuilding; }
            set
            {
                if (value == null)
                    ContextMenuActive = false;
                else
                    ViewBuilding(value);
                _contextMenuBuilding = value;
            }
        }

        private static GameObject _contextMenuBuilding;

        public static event EventHandler<BuildingChoosedArgs> BuildingChoosed;
        public static event EventHandler<ActionChoosedArgs> ActionChoosed;

        public static ReadOnlyCollection<NetController.BuildingAction> BuildingActions
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
                CurrentBuildingActions.Add(new BuildingAction("Exit", true, Resources.Load<Sprite>("Space"), "Space"));

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

                    currentText.text = currentBuildingAction.Name;
                    currentImage.sprite = currentBuildingAction.KeySprite; 

                    if (!currentBuildingAction.Possible)
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
        private static ReadOnlyCollection<NetController.BuildingAction> _buildingActions;

        protected static List<BuildingAction> CurrentBuildingActions;

        protected const float ContextMenuSubjectSizeFactor = 4;

        protected static float ContextMenuDelaySecs = 0.8f;
        protected static float CurrentContextMenuDelaySecs;
        protected static float ContextMenuBackgroundMaxAlpha = 0.8f;



        protected virtual void Start()
        {
            Ui.RegistrationEmailForm.GameObject.SetActive(false);
            Ui.RegistrationNumbersForm.GameObject.SetActive(false);
            Ui.RegistrationUserDataForm.GameObject.SetActive(false);
            Ui.RegistrationForms.GameObject.SetActive(false);
        }

        protected virtual void Update()
        {
            KeysUpdate();
            ContextMenuUpdate();
        }



        protected static void ViewBuilding(GameObject building)
        {
            BuildingActions = null;

            if (BuildingChoosed != null)
            {
                try
                { 
                    BuildingChoosed(null, new BuildingChoosedArgs(
                        building.name,
                        building.GetComponent<IsometricController>().IsometricPosition));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            
            Ui.ContextBuildingImage.Image.sprite = building.GetComponent<SpriteRenderer>().sprite;
            Ui.ContextHolderImage.Image.sprite
                = building.GetComponent<BuildingController>().Holder.GetComponent<HolderController>().UnactiveSprite;

            CameraController.TargetPosition
                = building.transform.position + new Vector3(0, 0, -1);

            ContextMenuActive = true;
        }

        private static void KeysUpdate()
        {
            if (CurrentBuildingActions == null || CurrentBuildingActions.Count < 1)
                return;

            CurrentBuildingActions.ForEach(action => {
                if (!Input.GetButtonDown(action.KeyName) || !action.Possible)
                    return;

                if (action.KeyName == "Space")
                {
                    ContextMenuBuilding = null;
                }
                else if (ActionChoosed != null)
                {
                    try
                    { 
                        ActionChoosed(null, new ActionChoosedArgs(action));
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