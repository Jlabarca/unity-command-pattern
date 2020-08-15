using UnityEngine;

namespace com.jlabarca.cpattern.Singletons
{
    public abstract class BaseBehaviour : MonoBehaviour
    {
        private Transform cashedTransform;
        public Transform CachedTransform
        {
            get { return cashedTransform ?? (cashedTransform = transform); }
        }

        protected virtual void Awake() { }

        protected virtual void Start() { }

        protected virtual void OnDestroy() { }
    }
}