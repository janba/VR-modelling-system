using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controls
{
    [RequireComponent(typeof(MeshCollider))]
    public class RotationController : IInteractableObject
    {
        private HoverHighlight _hoverHighlight;

        //private Mesh mesh;

        private bool isHeld = false;
        private Quaternion initialDialRotation;
        private Quaternion initialControllerRotation;

        void Awake()
        {
            _hoverHighlight = GetComponent<HoverHighlight>();
            //var meshFilter = GetComponent<MeshFilter>();
            //mesh = meshFilter.sharedMesh;
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            //Debug.Log(mesh.name);
            if (isHeld)
            {
                Quaternion currentControllerRotation = _controllerCollider.transform.rotation;

                float angle = currentControllerRotation.eulerAngles.z - initialControllerRotation.eulerAngles.z;
                //initialHandRotation = currentControllerRotation;
                this.transform.rotation = Quaternion.Euler(initialDialRotation.eulerAngles + new Vector3(0f, -angle, 0f));
            }
            

        }

        public override void Interact()
        {
            Debug.Log("Interacting");
            isHeld = true;

            initialDialRotation = this.transform.rotation;
            initialControllerRotation = _controllerCollider.transform.rotation;
        }

        public override void StartHighlight()
        {
            _hoverHighlight.StartHover();
        }

        public override void EndHighlight()
        {
            _hoverHighlight.EndHover();
        }

        public override void ChangeInteraction(InteractionMode mode)
        {
        }

        public override void StopInteraction()
        {
            Debug.Log("No longer Interacting");
            isHeld = false;
        }
    }
}
