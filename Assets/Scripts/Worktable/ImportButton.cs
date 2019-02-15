using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Controls
{
    [RequireComponent(typeof(MeshCollider))]
    public class ImportButton : IInteractableObject
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

        // Update is called once per frame
        void Update()
        {
            canUse = GetComponent<MeshRenderer>().isVisible;

            if(isOpen && !UIPanel.GetComponent<ScreenController>().ImportUI.activeSelf)
            {
                isOpen = false;
            }
            if(!isOpen && UIPanel.GetComponent<ScreenController>().ImportUI.activeSelf)
            {
                isOpen = true;
            }
        }

        public override void Interact()
        {
        }

        public override void ChangeInteraction(InteractionMode mode)
        {
        }

        public override void StopInteraction()
        {
        }

        public override void StartHighlight()
        {
            if (fromTop && canUse)
            {
                if (isOpen)
                {
                    StartCoroutine(CloseImportUI());
                }
                else
                {
                    StartCoroutine(OpenImportUI());
                }

            }
        }

        IEnumerator OpenImportUI()
        {
            //isInteracting = true;
            var component = GetComponent<MeshRenderer>();
            Material material = component.material;
            material.color = Color.blue;

            isOpen = true;
            ScreenController _screenController = UIPanel.GetComponent<ScreenController>();
            _screenController.OpenImportUI(isOpen);
            //_screenController.ExportMesh("TestExport");

            //_screenController.disableScreen();

            byte[] noize = { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 };
            OVRHaptics.Channels[1].Preempt(new OVRHapticsClip(noize, 10));
            yield return new WaitForSeconds(0.3f);

            material.color = initialColor;
            //isInteracting = false;

        }

        IEnumerator CloseImportUI()
        {
            //isInteracting = true;
            var component = GetComponent<MeshRenderer>();
            Material material = component.material;
            material.color = Color.blue;

            isOpen = false;
            ScreenController _screenController = UIPanel.GetComponent<ScreenController>();
            _screenController.OpenImportUI(isOpen);
            //_screenController.ExportMesh("TestExport");

            //_screenController.disableScreen();

            byte[] noize = { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 };
            OVRHaptics.Channels[1].Preempt(new OVRHapticsClip(noize, 10));
            yield return new WaitForSeconds(0.3f);

            material.color = initialColor;
            //isInteracting = false;

        }

        public override void EndHighlight()
        {
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
