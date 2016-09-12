using Assets.Code.Tools;
using UnityEngine;

namespace Assets.Code.Interface.Game
{
    public class CameraController : BehaviourPattern {
        public float ScrollingFactor = 0.005f;
        public float ScalingFactor = 1.2f;

        public float ScrollingSpeedMax = 3f;

        public float ScaleMax = 12;
        public float ScaleMin = 2;

        public float CameraMovingSpeed = 3f;

        public static Vector3 TargetPosition { get; set; }

        protected Vector3 PreviousMousePosition;
        protected Camera ThisCamera;

        protected override void Start() {
            base.Start();
            TargetPosition = ThisTransform.position;

            ThisCamera = GetComponent<Camera>();

       }

        protected virtual void Update() {
            CameraControlUpdate();
            CameraMoving();
        }

        private void CameraMoving() {
            if (TargetPosition == ThisTransform.position)
                return;

            var path = TargetPosition - ThisTransform.position;
            if (path.magnitude <= CameraMovingSpeed * Time.deltaTime * ThisCamera.orthographicSize) {
                ThisTransform.position = TargetPosition;
            }
            else {
                ThisTransform.position += path.normalized * CameraMovingSpeed * Time.deltaTime * ThisCamera.orthographicSize;
            }
        }

        private void CameraControlUpdate() {
            if (UiController.ContextMenuActive)
                return;

            if (Input.GetMouseButton(2)) {
                var path = PreviousMousePosition - Input.mousePosition;

                ThisTransform.position +=
                    path.magnitude * ScrollingFactor * ThisCamera.orthographicSize > ScrollingSpeedMax
                        ? ScrollingSpeedMax * path.normalized
                        : path * ScrollingFactor * ThisCamera.orthographicSize;
                TargetPosition = ThisTransform.position;
            }

            if (Input.mouseScrollDelta.y != 0)
                ThisCamera.orthographicSize = Mathf.Clamp(
                    ThisCamera.orthographicSize * ( Input.mouseScrollDelta.y < 0 ? ScalingFactor : 1 / ScalingFactor ),
                    ScaleMin, ScaleMax);

            PreviousMousePosition = Input.mousePosition;
        }
    }
}