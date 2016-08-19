using Assets.Code.Interface;
using UnityEngine;
using Assets.Code.Tools;

namespace Assets.Code.Game
{
    public class HolderController : BehaviourPattern {
        public GameObject Building;

        public Sprite ActiveSprite {
            get {
                return Resources.Load<Sprite>("Pattern - 1 x 1, active");
            }
        }

        public Sprite UnactiveSprite {
            get {
                return Resources.Load<Sprite>("Pattern - 1 x 1");
            }
        }

        protected void OnMouseEnter() {
            if (!UiController.ContextMenuActive)
                ThisSpriteRenderer.sprite = ActiveSprite;
        }

        protected void OnMouseExit() {
            ThisSpriteRenderer.sprite = UnactiveSprite;
        }

        protected void OnMouseDown() {
            if (!UiController.ContextMenuActive)
            {
                UiController.ContextMenuBuilding = Building;
            }
        }
    }
}