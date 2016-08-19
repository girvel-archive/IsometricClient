using Assets.Code.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code.Interface.Authentication
{
    public class LoginButtonController : MonoBehaviour
    {
        public float FormAlphaChangingSpeed;


        
        public static bool AuthenticationFormActive = true;

        public const string
            LoginStatusSucces = "Success!",
            LoginStatusConnectionFail = "Connection error",
            LoginStatusLoginFail = "Login failed";

        

        private float _formAlpha;



        public void OnClick() // HAS TO BE PUBLIC
        {
            NetController.Email = Ui.LoginEmailField.Content;
            NetController.Password = Ui.LoginPasswordField.Content;
            NetController.LoginConnect();
        }




        private void Start()
        {
            _formAlpha = AuthenticationFormActive ? 1 : 0;

            AuthenticationFormActive = true;
            
            NetController.OnConnectionFail += (sender, args) => {
                Ui.LoginStatus.Content = LoginStatusConnectionFail;
            };

            NetController.OnConnectionSuccess += (sender, args) => {
                Ui.LoginStatus.Content = LoginStatusSucces;
            };
        }

        private void Update()
        {
            sbyte direction = 0;

            if (AuthenticationFormActive && _formAlpha < 1)
            {
                direction = 1;
            }
            else if (!AuthenticationFormActive && _formAlpha > 0)
            {
                direction = -1;
            }

            if (direction != 0)
            {
                _formAlpha += direction * Time.deltaTime * FormAlphaChangingSpeed;

                foreach (var element 
                    in Ui.LoginForm.GameObject.GetComponentsInChildren<MaskableGraphic>())
                {
                    element.color = new Color(
                        element.color.r,
                        element.color.g,
                        element.color.b,
                        _formAlpha);
                }

                if (_formAlpha <= 0)
                {
                    Ui.LoginForm.GameObject.SetActive(false);
                }
            }
        }
    }
}