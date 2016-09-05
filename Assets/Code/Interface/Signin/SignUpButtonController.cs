using UnityEngine;

namespace Assets.Code.Interface.Signin
{
    public class SignUpButtonController : MonoBehaviour
    {
        public void OnClick() // HAS TO BE PUBLIC
        {
            Ui.LoginForm.GameObject.SetActive(false);
            Ui.RegistrationForms.GameObject.SetActive(true);
            Ui.RegistrationEmailForm.GameObject.SetActive(true);
        }
    }
}
