using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controls
{
    [RequireComponent(typeof(MeshCollider))]
    public class ExportButton : IInteractableObject
    {
        private Color initialColor;
        private MeshRenderer _meshRenderer;

        //private bool isInteracting;

        private bool fromTop;
        private bool canUse;

        public GameObject UIPanel;

        private bool isOpen;

        // Use this for initialization
        void Start()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            initialColor = _meshRenderer.material.color;
            //isInteracting = false;
            isOpen = false;
            canUse = GetComponent<MeshRenderer>().isVisible;
        }

        void Update()
        {
            canUse = GetComponent<MeshRenderer>().isVisible;

            if (isOpen && !UIPanel.GetComponent<ScreenController>().ExportUI.activeSelf)
            {
                isOpen = false;
            }
            if (!isOpen && UIPanel.GetComponent<ScreenController>().ExportUI.activeSelf)
            {
                isOpen = true;
            }
        }

        public override void ChangeInteraction(InteractionMode mode)
        {
        }

        public override void EndHighlight()
        {
        }

        public override void Interact()
        {
        }

        public override void StartHighlight()
        {
            //Since this is a button that is pushed this actually does the Interaction
            if (fromTop && canUse)
            {
                if (isOpen)
                {
                    StartCoroutine(ExportModel());
                }
                else
                {
                    StartCoroutine(OpenExportUI());
                }
                
            }
        }

        public override void StopInteraction()
        {
        }

        IEnumerator OpenExportUI()
        {
            //isInteracting = true;
            var component = GetComponent<MeshRenderer>();
            Material material = component.material;
            material.color = Color.blue;

            isOpen = true;
            ScreenController _screenController = UIPanel.GetComponent<ScreenController>();
            _screenController.OpenExportUI(isOpen);
            //_screenController.ExportMesh("TestExport");

            //_screenController.disableScreen();

            byte[] noize = { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 };
            OVRHaptics.Channels[1].Preempt(new OVRHapticsClip(noize, 10));
            yield return new WaitForSeconds(0.3f);

            material.color = initialColor;
            //isInteracting = false;

        }

        IEnumerator ExportModel()
        {
            //isInteracting = true;
            var component = GetComponent<MeshRenderer>();
            Material material = component.material;
            material.color = Color.blue;

            isOpen = false;
            ScreenController _screenController = UIPanel.GetComponent<ScreenController>();
            _screenController.ExportMesh();
            _screenController.OpenExportUI(isOpen);
            //_screenController.ExportMesh("TestExport");

            //_screenController.disableScreen();

            byte[] noize = { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 };
            OVRHaptics.Channels[1].Preempt(new OVRHapticsClip(noize, 10));
            yield return new WaitForSeconds(0.3f);

            material.color = initialColor;
            //isInteracting = false;

        }

        private void OnTriggerEnter(Collider other)
        {
            //Check if the trigger is entering the button from above
            if (other.transform.parent.name.StartsWith("hand") && !fromTop)
            {
                if (other.transform.position.y > transform.position.y)
                {
                    fromTop = true;
                }
                else
                {
                    fromTop = false;
                }
            }

        }

        private void OnTriggerExit(Collider other)
        {
            if (other.transform.parent.name.StartsWith("hand"))
            {
                fromTop = false;
            }
        }
    }
}
