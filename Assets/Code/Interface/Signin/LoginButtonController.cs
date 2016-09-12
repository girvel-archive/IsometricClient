using Assets.Code.Interface.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code.Interface.Signin
{
    public class LoginButtonController : MonoBehaviour
    {
        public float FormAlphaChangingSpeed;


        
        public static bool AuthenticationFormActive = true;
        
        

        private float _formAlpha;



        public void OnClick() // HAS TO BE PUBLIC
        {
            NetController.Instance.Email = Ui.LoginEmailField.Content;
            NetController.Instance.Password = Ui.LoginPasswordField.Content;
            NetController.Instance.LoginConnect();
        }




        private void Start()
        {
            _formAlpha = AuthenticationFormActive ? 1 : 0;

            AuthenticationFormActive = true;
            
            NetController.Instance.OnConnectionFail += () => {
                Ui.LoginStatus.Content = LoginStatus.ConnectionFail;
            };

            NetController.Instance.OnConnectionSuccess += () => {
                Ui.LoginStatus.Content = LoginStatus.Succes;
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