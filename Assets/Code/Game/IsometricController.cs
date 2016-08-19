using UnityEngine;
using Assets.Code.Tools;

namespace Assets.Code.Game
{
    [ExecuteInEditMode]
    public class IsometricController : BehaviourPattern {
        public Vector2 IsometricPosition = Vector2.zero;
        public Vector2 PlatformSize = Vector2.one;

        protected Vector2 LastIsometricPosition = Vector2.zero;
        protected Vector2 LastPosition;
        
        public static readonly Vector2 DefaultPlatformSize = new Vector2(0.56f, 0.26f);

        protected override void Start() {
            base.Start();
            LastPosition = ThisTransform.position;
        }

        protected virtual void Update() {
            IsometricPositionRefresh();
        }

        private void IsometricPositionRefresh() {
            if (IsometricPosition != LastIsometricPosition) {
                ThisTransform.position = new Vector2(
                    0.5f * DefaultPlatformSize.x * ThisTransform.localScale.x *
                        (IsometricPosition.y - IsometricPosition.x),
                    0.5f * DefaultPlatformSize.y * ThisTransform.localScale.y *
                        (IsometricPosition.y + IsometricPosition.x));

                ThisSpriteRenderer.sortingOrder = -(int) (IsometricPosition.x + IsometricPosition.y);

                LastIsometricPosition = IsometricPosition;
            }
            else if ((Vector2)ThisTransform.position != LastPosition) {
                var roundx = Mathf.RoundToInt(ThisTransform.position.x /
                                     (0.5f * DefaultPlatformSize.x * ThisTransform.localScale.x));

                var roundy = Mathf.RoundToInt(ThisTransform.position.y /
                                     (0.5f * DefaultPlatformSize.y * ThisTransform.localScale.y));

                IsometricPosition = new Vector2(
                    (roundy - roundx) / 2,
                    (roundx + roundy) / 2);

                ThisTransform.position = new Vector2(
                    0.5f * DefaultPlatformSize.x * ThisTransform.localScale.x *
                        (IsometricPosition.y - IsometricPosition.x),
                    0.5f * DefaultPlatformSize.y * ThisTransform.localScale.y *
                        (IsometricPosition.y + IsometricPosition.x));
            }
            LastPosition = ThisTransform.position;
        }
    }
}