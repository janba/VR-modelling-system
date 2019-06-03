using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.GEL;
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

        private Manifold initialManifold;

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Vector3 initialFaceCenter;

        private ControlsManager controlsManager;

        private float distanceToLowestVertex;
        private bool isExtruding = false;

        private List<AdjacentLockedFaces> selectedFaceList;
        private Quaternion lastRotation;
        private Quaternion firstRotation;
        private GameObject[] measuringBands = new GameObject[4];
        bool angleSnap = true;
        bool translateSnap = true;
        bool collision = false;
        bool tickState = false;
        private Dictionary<int, Vector3> collidedFaceHandleVertexPositions;

        int collidedFaceID;

        FaceHandleController collidedFaceHandle;

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
                GrabControl gc = _controller as GrabControl;

                if (!isExtruding)
                {
                    if (move_from_initial_pos.magnitude > SNAP_DISTANCE)
                    {
                        ControlsManager.Instance.SetVertexPositionsById(initialVertexPositions);
                        Extrudable.StartExtrusion(extrudingFaces.ToArray());
                        isExtruding = true;

                        if (extrudingFaces.Count() == 1)  // multiple faces ???
                        {
                            initializeMeasuringBands();
                        }

                        if (gc)
                        {
                            StartCoroutine(Buzz(gc.Controller));
                        }
                    }
                }

                if (OVRInput.Get(OVRInput.NearTouch.PrimaryThumbButtons, gc.Controller) && extrudingFaces.Count() == 1)
                {
                    tickState = true;
                }
                else
                {
                    tickState = false;
                }

                transform.localPosition = controllerPosInLocalSpace - initialControllerOffset;
                var translate_vector = transform.localPosition;

                var norm = Extrudable.GetFaceNormal(AssociatedFaceID);

                if (translateSnap && isExtruding)
                {
                    float d = Vector3.Dot(norm, move_from_initial_pos);
                    if ((move_from_initial_pos - d * norm).magnitude > SNAP_DISTANCE)
                    {
                        translateSnap = false;
                        destroyMeasuringBands();
                       // GrabControl gc = _controller as GrabControl;
                        if (gc)
                        {
                            StartCoroutine(Buzz(gc.Controller));
                        }
                    }
                }
                    
                Extrudable.MoveTo(
                    AssociatedFaceID,
                    extrudingFaces.ToArray(),
                    translate_vector, translateSnap && isExtruding, tickState && angleSnap);

                if (isExtruding && translateSnap && angleSnap && extrudingFaces.Count() == 1)
                {
                    if (tickState)
                    {
                        float extrusionLength = (Extrudable._manifold.GetCenter(AssociatedFaceID) - initialFaceCenter).magnitude;
                        updateMeasuringBands(Mathf.Round(extrusionLength * 50) / 50);
                    }
                    else
                    {
                        updateMeasuringBands(Vector3.Dot(move_from_initial_pos, norm));
                    }
                }

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
                        destroyMeasuringBands();
                        //GrabControl gc = _controller as GrabControl;
                        if (gc)
                        {
                            StartCoroutine(Buzz(gc.Controller));
                        }
                    }
                }
                /*
                if(!angleSnap)
                {
                    var deltaRotation = controllerRotInLocalSpace * Quaternion.Inverse(lastRotation);
                    Extrudable.RotateFaceAroundPoint(extrudingFaces.ToArray(),
                        transform.localPosition, deltaRotation);
                }
                */
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
            initialFaceCenter = Extrudable._manifold.GetCenter(AssociatedFaceID);
            initialManifold = Extrudable._manifold.Copy();

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
            if (extrudingFaces.Count > 1)
            {
                ControlsManager.Instance.DeleteControlsExceptSelectedFaces(new List<int> { AssociatedFaceID });
            }
            else
            { 
            //ControlsManager.Instance.DeleteControlsExceptSelectedFaces(extrudingFaces);
            ControlsManager.Instance.DeleteControlsExceptFaces(initialManifold.GetAdjacentFaceIdsAndEdgeCenters(AssociatedFaceID).faceId, AssociatedFaceID);
            }   

            ControlsManager.FireUndoStartEvent(mesh, this, initialPosition, initialRotation);
        }

        public override void StopInteraction()
        {

            if (IsDragged)
            {
                destroyMeasuringBands();
                IsDragged = false;

                if (collision == false)
                {
                    Extrudable.MoveTo(
                        AssociatedFaceID,
                        extrudingFaces.ToArray(),
                        transform.localPosition, translateSnap, tickState);

                }
                else
                /*else if (extrudingFaces.Count == 1)*/ // yes/no maybe
                {

                    if (hasDistinctAdjacentFaces(collidedFaceHandle))
                    {
                        //collidedFaceHandleVertexPositions = controlsManager.GetVertexPositionsFromFaces(new List<int> { collidedFaceID });
                        collidedFaceHandleVertexPositions = initialManifold.GetVertexPositionsFromFace(collidedFaceID);

                        if (collidedFaceHandleVertexPositions.Count() == initialVertexPositions.Count() && extrudingFaces.Count == 1)
                        {
                            var matches = faceBridgingVertexAssignment(initialVertexPositions, collidedFaceHandleVertexPositions);

                            if (facingFaces(matches, AssociatedFaceID, initialVertexPositions, collidedFaceID, collidedFaceHandleVertexPositions))
                            {
                                Extrudable.ChangeManifold(initialManifold.Copy());

                                if (matches.Length == 6)
                                {
                                    Extrudable.bridgeFaces(AssociatedFaceID, collidedFaceID, new int[] { matches[0], matches[1], matches[2] }, new int[] { matches[3], matches[4], matches[5] }, 3);
                                }
                                else if (matches.Length == 8)
                                {
                                    Extrudable.bridgeFaces(AssociatedFaceID, collidedFaceID, new int[] { matches[0], matches[1], matches[2], matches[3] }, new int[] { matches[4], matches[5], matches[6], matches[7] }, 4);
                                }
                            }
                        }
                    }
                    collidedFaceHandleVertexPositions = null;
                }

                extrudingFaces = new List<int>(); // create new list (may be referenced by other hand)

                if (Extrudable.isValidMesh())
                {

                    int collapsed = Extrudable.CollapseShortEdges(0.01);
                    if (collapsed > 0)
                    {
                        ControlsManager.Instance.DestroyInvalidObjects();
                    }

                    ControlsManager.Instance.Extrudable.rebuild = true;
                    ControlsManager.FireUndoEndEvent(mesh, this, initialPosition, initialRotation);
                }
                else
                {
                    Extrudable.ChangeManifold(initialManifold);
                }
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

        void OnTriggerEnter(Collider collider)
        {
            
            if (IsDragged)
            {
                if (collider.GetComponent<FaceHandleController>() != null)
                {
                    collision = true;
                    collidedFaceHandle = collider.GetComponent<FaceHandleController>();
                    collidedFaceID = collidedFaceHandle.AssociatedFaceID;
                    collidedFaceHandle.StartHighlight();
                }
            }

        }

        void OnTriggerExit(Collider collider)
        {
            if (collidedFaceHandle != null)
            {
                collidedFaceHandle.EndHighlight();
                collidedFaceHandle = null;
                collision = false;
            }
        }

        public int[] faceBridgingVertexAssignment(Dictionary<int, Vector3> face1vertices, Dictionary<int, Vector3> face2vertices)
        {
            List<int> keyList1 = new List<int>(face1vertices.Keys);
            List<int> keyList2 = new List<int>(face2vertices.Keys);

            float min = 0;

            if(keyList1.Count == 4) {
                int[] res = new int[8];
                res[0] = keyList1[0]; res[1] = keyList1[1]; res[2] = keyList1[2]; res[3] = keyList1[3];

                for (int a = 0; a < 4; a++)
                {
                    for (int b = 0; b < 4; b++)
                    {
                        if (b == a) continue;

                        for (int c = 0; c < 4; c++)
                        {
                            if (c == a || c == b) continue;
                            for (int d = 0; d < 4; d++)
                            {
                                if (d == a || d == b || d == c) continue;

                                float match1 = Vector3.Distance(face1vertices[keyList1[0]], face2vertices[keyList2[a]]);
                                float match2 = Vector3.Distance(face1vertices[keyList1[1]], face2vertices[keyList2[b]]);
                                float match3 = Vector3.Distance(face1vertices[keyList1[2]], face2vertices[keyList2[c]]);
                                float match4 = Vector3.Distance(face1vertices[keyList1[3]], face2vertices[keyList2[d]]);

                                if ((match1 + match2 + match3 + match4) < min || min == 0)
                                {
                                    min = match1 + match2 + match3 + match4;
                                    res[4] = keyList2[a]; res[5] = keyList2[b]; res[6] = keyList2[c]; res[7] = keyList2[d];
                                }
                            }
                        }
                    }
                }
                return res;
            } else
            {
                int[] res = new int[6];
                res[0] = keyList1[0]; res[1] = keyList1[1]; res[2] = keyList1[2];

                for (int a = 0; a < 3; a++)
                {
                    for (int b = 0; b < 3; b++)
                    {
                        if (b == a) continue;

                        for (int c = 0; c < 3; c++)
                        {
                            if (c == a || c == b) continue;

                            float match1 = Vector3.Distance(face1vertices[keyList1[0]], face2vertices[keyList2[a]]);
                            float match2 = Vector3.Distance(face1vertices[keyList1[1]], face2vertices[keyList2[b]]);
                            float match3 = Vector3.Distance(face1vertices[keyList1[2]], face2vertices[keyList2[c]]);

                            if ((match1 + match2 + match3) < min || min == 0)
                            {
                                min = match1 + match2 + match3;
                                res[3] = keyList2[a]; res[4] = keyList2[b]; res[5] = keyList2[c];
                            }                           
                        }
                    }
                }
                return res; 
            }
        }

        public bool facingFaces(int[] matches, int face1ID, Dictionary<int, Vector3> face1vertices, int face2ID, Dictionary<int, Vector3> face2vertices)
        {
            Vector3 f1norm = Extrudable.GetFaceNormal(face1ID);
            Vector3 f2norm = Extrudable.GetFaceNormal(face2ID);
            for (int i = 0; i < (matches.Length/2); i++)
            {
                if(Vector3.Dot((face1vertices[matches[i]] - face2vertices[matches[i + (matches.Length / 2)]]), f2norm) <= 0.0f)
                {
                    return false;
                }

                if (Vector3.Dot((face2vertices[matches[i + (matches.Length / 2)]] - face1vertices[matches[i]]), f1norm) <= 0.0f)
                {
                    return false;
                }
            }
            return true;
        } 

        public bool hasDistinctAdjacentFaces(FaceHandleController faceHandle)
        {

            foreach(int face1 in initialManifold.GetAdjacentFaceIdsAndEdgeCenters(AssociatedFaceID).faceId)
            {
                foreach (int face2 in initialManifold.GetAdjacentFaceIdsAndEdgeCenters(faceHandle.AssociatedFaceID).faceId)
                {
                    if (face1 == face2 || AssociatedFaceID == face2) // if neighbours, then false
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void initializeMeasuringBands()
        {
            /*
            var texture = new Texture2D(16, 16, TextureFormat.ARGB32, false);

            Color32[] pixelColors = new Color32[16 * 16];
            for (int j = 0; j < pixelColors.Length; j = j + 2)
            {
                pixelColors[j] = Color.black;
                pixelColors[j+1] = Color.clear;
            }
            */
            int index = 0;
            var texture = new Texture2D(80, 15, TextureFormat.ARGB32, false);

            Color32[] pixelColors = new Color32[80 * 15];

            for(int a = 0; a < 30; a++)
            {
                pixelColors[index] = Color.black;
                index++;
                for(int b = 0; b < 6; b++)
                {
                    pixelColors[index] = Color.clear;
                    index++;
                }
                pixelColors[index] = Color.black;
                index++;
            }
            
            for (int a = 0; a < 2; a++)
            {
                pixelColors[index] = Color.black;
                index++;
                for (int b = 0; b < 38; b++)
                {
                    pixelColors[index] = Color.clear;
                    index++;
                }
                pixelColors[index] = Color.black;
                index++;
            }

            for (int a = 0; a < 2; a++)
            {
                pixelColors[index] = Color.black;
                index++;
                for (int b = 0; b < 78; b++)
                {
                    pixelColors[index] = Color.clear;
                    index++;
                }
                pixelColors[index] = Color.black;
                index++;
            }

            for (int a = 0; a < 3; a++)
            {
                for (int b = 0; b < 80; b++)
                {
                    pixelColors[index] = Color.clear;
                    index++;
                }
            }

            for (int a = 0; a < 2; a++)
            {
                pixelColors[index] = Color.black;
                index++;
                for (int b = 0; b < 78; b++)
                {
                    pixelColors[index] = Color.clear;
                    index++;
                }
                pixelColors[index] = Color.black;
                index++;
            }

            for (int a = 0; a < 2; a++)
            {
                pixelColors[index] = Color.black;
                index++;
                for (int b = 0; b < 38; b++)
                {
                    pixelColors[index] = Color.clear;
                    index++;
                }
                pixelColors[index] = Color.black;
                index++;
            }

            for (int a = 0; a < 30; a++)
            {
                pixelColors[index] = Color.black;
                index++;
                for (int b = 0; b < 6; b++)
                {
                    pixelColors[index] = Color.clear;
                    index++;
                }
                pixelColors[index] = Color.black;
                index++;
            }
            
            texture.SetPixels32(pixelColors);
            texture.Apply();
            texture.wrapModeU = TextureWrapMode.Repeat;
            texture.wrapModeV = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;

            int i = 0;
            foreach (int faceid in Extrudable._manifold.GetAdjacentFaceIdsAndEdgeCenters(AssociatedFaceID).faceId) // maybe copy the first gameobject instead of intializing 4 objects???
            {
                GameObject measuringBand = new GameObject("measuringBand" + i);
                measuringBand.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = measuringBand.AddComponent<MeshFilter>();
                Mesh mesh = meshFilter.mesh;

                Vector3[] vertices = new Vector3[] { new Vector3(0f, 0f, 0f),
                                                        new Vector3(0f, 0f, 0f),
                                                        new Vector3(0f, 0f, 0f),
                                                        new Vector3(0f, 0f, 0f)};
      

                var triangles = new int[] { 0, 1, 2, 0, 2, 3 };

                var uvs = new Vector2[] {   new Vector2(0f, 0f),
                                            new Vector2(0f, 1f),
                                            new Vector2(1f, 0f),
                                            new Vector2(1f, 1f)};

                mesh.Clear();
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.uv = uvs;
                //mesh.RecalculateNormals();

                measuringBand.transform.position = Extrudable.transform.position;
                measuringBand.transform.rotation = Extrudable.transform.rotation;
                measuringBand.transform.localScale = Extrudable.transform.lossyScale;

                Material mbMaterial = new Material(Shader.Find("Standard"));
                mbMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mbMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mbMaterial.SetInt("_ZWrite", 0);
                mbMaterial.DisableKeyword("_ALPHATEST_ON");
                mbMaterial.EnableKeyword("_ALPHABLEND_ON");
                mbMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mbMaterial.renderQueue = 3000;

                mbMaterial.SetTexture("_MainTex", texture);
                measuringBand.GetComponent<Renderer>().material = mbMaterial;

                measuringBands[i] = measuringBand;
                i++;
          
            }

        }

        public void updateMeasuringBands(float extrusionLength)
        {

            Vector2[] uvs = new Vector2[] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(extrusionLength * 5f, 1f), new Vector2(extrusionLength * 5f, 0f) };

            int i = 0;
            foreach (int faceid in Extrudable._manifold.GetAdjacentFaceIdsAndEdgeCenters(AssociatedFaceID).faceId)
            {
                var faceVertices = Extrudable._manifold.GetVertexPositionsFromFace(faceid).Values.ToList();
                for(int x = 0; x < faceVertices.Count(); x++)
                {
                    faceVertices[x] = faceVertices[x] + Vector3.Scale(Extrudable._manifold.GetFaceNormal(faceid), new Vector3(0.0001f, 0.0001f,  0.0001f));
                }
                measuringBands[i].GetComponent<MeshFilter>().mesh.vertices = new Vector3[] { faceVertices[0], faceVertices[1], faceVertices[2], faceVertices[3] }; // garbage collection
                measuringBands[i].GetComponent<MeshFilter>().mesh.uv = uvs;

                i++;
            }
        }

        public void destroyMeasuringBands()
        {
            foreach(GameObject measuringBand in measuringBands)
            {
                Destroy(measuringBand);
            }
        }

    }
}