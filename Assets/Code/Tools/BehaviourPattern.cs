using UnityEngine;

namespace Assets.Code.Tools
{
    public abstract class BehaviourPattern : MonoBehaviour {
        protected Transform ThisTransform;
        protected SpriteRenderer ThisSpriteRenderer;

        // Use this for initialization
        protected virtual void Start() {
            ThisTransform = GetComponent<Transform>();
            ThisSpriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
}
