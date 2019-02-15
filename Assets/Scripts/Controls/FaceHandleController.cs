using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Controls
{
    [RequireComponent(typeof(MeshCollider))]
    public class FaceHandleController : IInteractableObject
    {
        struct AdjacentLockedFaces
        {
            public int faceid;
            public bool dragged;

            public AdjacentLockedFaces(int faceid, bool dragged)
            {
                this.faceid = faceid;
                this.dragged = dragged;
            }

            public override string ToString()
            {
                return "{faceid:" + faceid + ",dragged:" + dragged + "}";
            }
        }

        public ExtrudableMesh Extrudable;
        public int AssociatedFaceID;

        private Vector3 screenPoint;
        private Vector3 offset;

        public bool IsDragged = false;
        private Vector3 initialControllerOffset;
        private Dictionary<int, Vector3> initialVertexPositions;

        public List<EdgeHandleController> ConnectedLatches = new List<EdgeHandleController>();

        private List<int> extrudingFaces = new List<int>();

        private HoverHighlight _hoverHighlight;

        private Mesh mesh;
        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private ControlsManager controlsManager;

        private float distanceToLowestVertex;
        private bool isExtruding = false;

        private List<AdjacentLockedFaces> selectedFaceList;
        private Quaternion lastRotation;
        private Quaternion firstRotation;
        bool angleSnap = true;
        bool translateSnap = true;

        private Vector3 initialControllerPosInLocalSpace;

        void Awake()
        {
            _hoverHighlight = GetComponent<HoverHighlight>();
            var meshFilter = GetComponent<MeshFilter>();
            mesh = meshFilter.sharedMesh;
            controlsManager = FindObjectOfType<ControlsManager>();
        }

        public IEnumerator Buzz(OVRInput.Controller gcController)
        {
            OVRInput.SetControllerVibration(0.1f, 1, gcController);
            yield return new WaitForSeconds(0.1f);
            OVRInput.SetControllerVibration(0, 0, gcController);
        }

        private void Update()
        {
            const float SNAP_DISTANCE = 0.04f;
            const float SNAP_ANGLE = 20.0f;

            Vector3 left_ctrl_pt = GameObject.Find("LeftHandAnchor").transform.position;
            Vector3 right_ctrl_pt = GameObject.Find("RightHandAnchor").transform.position;
            float dist_left_ctrl = (10.0f * (left_ctrl_pt - transform.position)).sqrMagnitude;
            float dist_right_ctrl = (10.0f * (right_ctrl_pt - transform.position)).sqrMagnitude;
            float sz = 0.01f + 0.09f / (Mathf.Min(dist_left_ctrl,dist_right_ctrl) + 1.0f);
            transform.localScale = new Vector3(sz, sz, sz);

            if (IsDragged)
            {
                Vector3 controllerPosInLocalSpace = transform.parent.worldToLocalMatrix.MultiplyPoint(_controllerCollider.transform.position);
                var move_from_initial_pos = controllerPosInLocalSpace - initialControllerPosInLocalSpace;
                if (!isExtruding)
                {
                    if (move_from_initial_pos.magnitude > SNAP_DISTANCE)
                    {
                        ControlsManager.Instance.SetVertexPositionsById(initialVertexPositions);
                        Extrudable.StartExtrusion(extrudingFaces.ToArray());
                        isExtruding = true;
                        GrabControl gc = _controller as GrabControl;
                        if (gc)
                        {
                            StartCoroutine(Buzz(gc.Controller));
                        }
                    }
                }

                transform.localPosition = controllerPosInLocalSpace - initialControllerOffset;
                var translate_vector = transform.localPosition;

                if (translateSnap && isExtruding)
                {
                    var norm = Extrudable.GetFaceNormal(AssociatedFaceID);
                    float d = Vector3.Dot(norm, move_from_initial_pos);
                    if ((move_from_initial_pos - d * norm).magnitude > SNAP_DISTANCE)
                    {
                        translateSnap = false;
                        GrabControl gc = _controller as GrabControl;
                        if (gc)
                        {
                            StartCoroutine(Buzz(gc.Controller));
                        }
                    }
                }
                    
                Extrudable.MoveTo(
                    AssociatedFaceID,
                    extrudingFaces.ToArray(),
                    translate_vector, translateSnap && isExtruding);

                // change between previous rotation and current rotation
                var controllerRotInLocalSpace = GetControllerRotationInLocalSpace();
                if (angleSnap)
                {
                    // Compute the rotation from the original controller orientation to the present.
                    // If that rotation is greater than 10 degrees, snapping is turned off, and the face orientation
                    // tracks the hand´s orientation.
                    var fullRotation = controllerRotInLocalSpace * Quaternion.Inverse(firstRotation);
                    float angle = 0.0f;
                    Vector3 axis = Vector3.zero;
                    fullRotation.ToAngleAxis(out angle, out axis);
                    if (angle > SNAP_ANGLE)
                    {
                        angleSnap = false;
                        GrabControl gc = _controller as GrabControl;
                        if (gc)
                        {
                            StartCoroutine(Buzz(gc.Controller));
                        }
                    }
                }
                if(!angleSnap)
                {
                    var deltaRotation = controllerRotInLocalSpace * Quaternion.Inverse(lastRotation);
                    Extrudable.RotateFaceAroundPoint(extrudingFaces.ToArray(),
                        transform.localPosition, deltaRotation);
                }
                lastRotation = GetControllerRotationInLocalSpace();
                ControlsManager.Instance.Extrudable.rebuild = true;
                if (extrudingFaces.Count > 1)
                {
                    controlsManager.UpdateFacesAndSelectedEdges(extrudingFaces);
                }
            }
        }

        public Quaternion GetControllerRotationInLocalSpace()
        {
            return Quaternion.Inverse(transform.parent.rotation) * _controller.transform.parent.localRotation;
        }

        public void AttachLatch(EdgeHandleController edgeHandle)
        {
            if (!ConnectedLatches.Contains(edgeHandle))
                ConnectedLatches.Add(edgeHandle);
        }

        public override void StartHighlight()
        {
            _hoverHighlight.StartHover();
        }

        public override void EndHighlight()
        {
            _hoverHighlight.EndHover();
        }

        private List<AdjacentLockedFaces> GetAdjacentLockedFaces(int faceId, List<AdjacentLockedFaces> currentFaceSet)
        {
            var handle = ControlsManager.Instance.GetHandle(faceId);
            if (handle == null)
                return currentFaceSet.Distinct().ToList();
            currentFaceSet.Add(new AdjacentLockedFaces(faceId, handle.IsDragged));
            foreach (var latch in handle.ConnectedLatches)
            {
                if (latch != null)
                {
                    var latchController = latch.GetComponent<EdgeHandleController>();
                    if (latchController.Locked && !currentFaceSet.ConvertAll(pair => pair.faceid).Contains(latchController.GetOtherFace(faceId)))
                    {
                        GetAdjacentLockedFaces(latchController.GetOtherFace(faceId), currentFaceSet);
                    }
                }
            }
            return currentFaceSet.Distinct().ToList();
        }

        //        private bool 

        public override void ChangeInteraction(InteractionMode mode)
        {

        }

        public override void Interact()
        {

            initialPosition = transform.localPosition;
            initialRotation = transform.localRotation;
            selectedFaceList = GetAdjacentLockedFaces(AssociatedFaceID, new List<AdjacentLockedFaces>());


            extrudingFaces = selectedFaceList.ConvertAll(pair => pair.faceid);
            ControlsManager.Instance.RemoveLatches(extrudingFaces);
            initialVertexPositions = ControlsManager.Instance.GetVertexPositionsFromFaces(extrudingFaces);
            isExtruding = false;

            var worldToLocal = transform.parent.worldToLocalMatrix;
            initialControllerPosInLocalSpace = worldToLocal.MultiplyPoint(_controllerCollider.transform.position);
            initialControllerOffset = initialControllerPosInLocalSpace - transform.localPosition;
            IsDragged = true;

            lastRotation = GetControllerRotationInLocalSpace();
            firstRotation = lastRotation;
            angleSnap = true;
            translateSnap = true;

            ControlsManager.Instance.Extrudable.rebuild = true;

            ControlsManager.Instance.DeleteControlsExceptSelectedFaces(extrudingFaces);


            ControlsManager.FireUndoStartEvent(mesh, this, initialPosition, initialRotation);
        }

        public override void StopInteraction()
        {
            if (IsDragged)
            {
                IsDragged = false;
                Extrudable.MoveTo(
                    AssociatedFaceID,
                    extrudingFaces.ToArray(),
                    transform.localPosition, translateSnap);
                extrudingFaces = new List<int>(); // create new list (may be referenced by other hand)
                int collapsed = Extrudable.CollapseShortEdges(0.01);
                if (collapsed > 0)
                {
                    ControlsManager.Instance.DestroyInvalidObjects();
                }
                ControlsManager.Instance.Extrudable.rebuild = true;
                ControlsManager.FireUndoEndEvent(mesh, this, initialPosition, initialRotation);
            }
        }

        public void UpdatePositionAndRotation(Vector3 center, Vector3 normal, Vector3 edge)
        {
            transform.localPosition = center;
            Vector3 direction = Vector3.Cross(normal, edge);
            Quaternion rotation = Quaternion.LookRotation(direction, normal);
            transform.localRotation = rotation;
            // Adjust rotation
            transform.Rotate(90, 0, 0);
        }
    }
}