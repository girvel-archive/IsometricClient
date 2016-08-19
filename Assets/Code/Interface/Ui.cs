using Assets.Code.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code.Interface
{
    public static class Ui
    {
        #region UI classes

        public class InputField
        {
            public class MainObject
            {
                public GameObject GameObject;
                public Image Image;
                public UnityEngine.UI.InputField Field;

                public MainObject(GameObject gameObject)
                {
                    GameObject = gameObject;
                    Image = gameObject.GetComponent<Image>();
                    Field = gameObject.GetComponent<UnityEngine.UI.InputField>();
                }
            }

            public class FieldChild
            {
                public GameObject GameObject;
                public Text Text;

                public FieldChild(GameObject parent, string name)
                {
                    GameObject = parent.transform.FindChild(name).gameObject;
                    Text = GameObject.GetComponent<Text>();
                }
            }



            public MainObject Main;
            public FieldChild Placeholder, Text;

            public string Content
            {
                get { return Main.Field.text; }
                set { Main.Field.text = value; }
            }

            public InputField(string name)
            {
                Main = new MainObject(GameObject.Find(name));
                Placeholder = new FieldChild(Main.GameObject, "Placeholder");
                Text = new FieldChild(Main.GameObject, "Text");
            }
        }



        public class Status
        {
            public GameObject GameObject;
            public Text Text;

            public string Content {
                get { return Text.text; }
                set { Text.text = value; }
            }

            public Status(string name)
            {
                GameObject = GameObject.Find(name);
                Text = GameObject.GetComponent<Text>();
            }
        }



        public class Resource
        {
            public GameObject Container { get; private set; }
            public Text Text { get; private set; }

            public int Content
            {
                get { return int.Parse(Text.text); }
                set { Text.text = value.ToString(); }
            }

            public Resource(string name)
            {
                Container = GameObject.Find(name);
                Text = Container.GetComponentInChildren<Text>();
            }
        }



        public class Container
        {
            public GameObject GameObject;
            public Transform Transform;

            public Container(string name)
            {
                GameObject = GameObject.Find(name);
                Transform = GameObject.transform;
            }
        }



        public class UiImage
        {
            public GameObject GameObject;
            public Image Image;

            public UiImage(string name)
            {
                GameObject = GameObject.Find(name);
                Image = GameObject.GetComponent<Image>();
            }
        }

        #endregion



        #region UI fields

        public static Container 
            LoginForm,
            RegistrationForms,
            RegistrationEmailForm,
            RegistrationNumbersForm,
            RegistrationUserDataForm,
            ResourcesLineForm,
            GameBuildingsContainer,
            ContextForm,
            ContextButtonsContainer,
            ContextOpaqueElementsContainer;

        public static InputField
            LoginEmailField,
            LoginPasswordField,
            RegistrationEmailField,
            RegistrationNumbersField,
            RegistrationLoginField,
            RegistrationPasswordField;

        public static Status
            RegistrationEmailStatus,
            RegistrationNumbersStatus,
            RegistrationUserDataStatus,
            LoginStatus,
            ResourcesLineConnectionStatus;

        public static Resource
            ResourceWood,
            ResourceMeat,
            ResourceCorn,
            ResourceStone,
            ResourcePeople,
            ResourceGold,
            ResourceProgress;

        public static UiImage
            ContextBuildingImage,
            ContextHolderImage,
            ContextBackgroundImage;

        #endregion



        static Ui()
        {
            #region definitions

            LoginForm = new Container(Names.LoginForm);
            RegistrationForms = new Container(Names.RegistrationForms);
            RegistrationEmailForm = new Container(Names.RegistrationEmailForm);
            RegistrationNumbersForm = new Container(Names.RegistrationNumbersForm);
            RegistrationUserDataForm = new Container(Names.RegistrationUserDataForm);
            ResourcesLineForm = new Container(Names.ResourcesLineForm);
            GameBuildingsContainer = new Container(Names.GameBuildingsContainer);
            ContextForm = new Container(Names.ContextForm);
            ContextButtonsContainer = new Container(Names.ContextButtons);
            ContextOpaqueElementsContainer = new Container(Names.ContextOpaqueElements);

            RegistrationEmailField = new InputField(Names.RegistrationEmailField);
            RegistrationNumbersField = new InputField(Names.RegistrationNumbersField);
            RegistrationLoginField = new InputField(Names.RegistrationLoginField);
            RegistrationPasswordField = new InputField(Names.RegistrationPasswordField);
            LoginEmailField = new InputField(Names.LoginEmailField);
            LoginPasswordField = new InputField(Names.LoginPasswordField);

            RegistrationEmailStatus = new Status(Names.RegistrationEmailStatus);
            RegistrationNumbersStatus = new Status(Names.RegistrationNumbersStatus);
            RegistrationUserDataStatus = new Status(Names.RegistrationUserDataStatus);
            LoginStatus = new Status(Names.LoginStatus);
            ResourcesLineConnectionStatus = new Status(Names.ResourcesLineConnectionStatus);

            ResourceWood = new Resource(Names.ResourceWood);
            ResourceMeat = new Resource(Names.ResourceMeat);
            ResourceCorn = new Resource(Names.ResourceCorn);
            ResourceStone = new Resource(Names.ResourceStone);
            ResourcePeople = new Resource(Names.ResourcePeople);
            ResourceGold = new Resource(Names.ResourceGold);
            ResourceProgress = new Resource(Names.ResourceProgress);

            ContextBuildingImage = new UiImage(Names.ContextBuilding);
            ContextHolderImage = new UiImage(Names.ContextHolder);
            ContextBackgroundImage = new UiImage(Names.ContextBackground);

            #endregion
        }
    }
}
