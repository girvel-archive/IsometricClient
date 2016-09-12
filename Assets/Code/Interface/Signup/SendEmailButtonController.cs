using System.Net.Sockets;
using Assets.Code.Interface.Game;
using UnityEngine;

namespace Assets.Code.Interface.Signup
{
    public class SendEmailButtonController : MonoBehaviour
    {
        public const string ConnectionStatusError = "connection error";

        public void OnClick() // HAS TO BE PUBLIC
        {
            var email = Ui.RegistrationEmailField.Text.Text.text;
            if (email == string.Empty)
            {
                return;
            }

            try
            {
                NetController.Instance.SignUpConnect();
                NetController.Instance.SendEmail(email);

                Ui.RegistrationEmailForm.GameObject.SetActive(false);
                Ui.RegistrationNumbersForm.GameObject.SetActive(true);
            }
            catch (SocketException)
            {
                Ui.RegistrationEmailStatus.Content = ConnectionStatusError;
            }
        }
    }
}
