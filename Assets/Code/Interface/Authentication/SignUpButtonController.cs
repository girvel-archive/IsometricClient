using UnityEngine;

namespace Assets.Code.Interface.Authentication
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
