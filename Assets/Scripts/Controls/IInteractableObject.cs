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
            Debug.Log("Start interacting...");
            _controllerCollider = controllerCollider;
            _controller = controller;
            Interact();
            Debug.Log("Start interacting end");
        }

        public void EndInteraction()
        {
            Debug.Log("End interacting...");
            StopInteraction();
            _controllerCollider = null;
            Debug.Log("Ended interaction");

        }

        public abstract void Interact();

        public abstract void ChangeInteraction(InteractionMode mode);

        public abstract void StopInteraction();

        public abstract void StartHighlight();

        public abstract void EndHighlight();
    }
}