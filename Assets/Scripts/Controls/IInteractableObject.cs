using System;
using UnityEngine;

namespace Controls
{
    public abstract class IInteractableObject : MonoBehaviour
    {
        public enum InteractionMode
        {
            SINGLE,
            DUAL
        }

        protected SphereCollider _controllerCollider;
        protected MonoBehaviour _controller;

        public void StartInteraction(SphereCollider controllerCollider, MonoBehaviour controller)
        {
            _controllerCollider = controllerCollider;
            _controller = controller;
            Interact();
        }

        public void EndInteraction()
        {
            StopInteraction();
            _controllerCollider = null;
        }

        public abstract void Interact();

        public abstract void ChangeInteraction(InteractionMode mode);

        public abstract void StopInteraction();

        public abstract void StartHighlight();

        public abstract void EndHighlight();
    }
}