using UnityEngine;

namespace Assets.Code.Interface {
    public class ButtonsContainerController : MonoBehaviour {
        public float buttonsDistance = 10f;

        public static float ButtonsDistance {
            get { return _this.buttonsDistance; }
            set { _this.buttonsDistance = value; }
        }

        private static ButtonsContainerController _this;

        private void Start()
        {
            _this = this;
        }
    }
}