using UnityEngine;

namespace Assets.Code.Interface.Signup
{
    public class ExitButtonController : MonoBehaviour
    {
        public void OnClick() // HAS TO BE PUBLIC
        {
            Ui.RegistrationEmailField.Content = "";
            Ui.RegistrationNumbersField.Content = "";
            Ui.RegistrationLoginField.Content = "";
            Ui.RegistrationPasswordField.Content = "";

            Ui.RegistrationEmailForm.GameObject.SetActive(false);
            Ui.RegistrationNumbersForm.GameObject.SetActive(false);
            Ui.RegistrationUserDataForm.GameObject.SetActive(false);
            Ui.RegistrationForms.GameObject.SetActive(false);

            Ui.LoginForm.GameObject.SetActive(true);
        }
    }
}