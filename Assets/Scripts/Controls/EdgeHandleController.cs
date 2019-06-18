using UnityEngine;

namespace Controls
{
    public class EdgeHandleController : IInteractableObject
    {
        public int FirstFace = -1;
        public int SecondFace = -1;
        public int AssociatedEdgeID = -1;

        public bool Locked;

        public Mesh[] Meshes;

        private MeshFilter _meshFilter;
        private HoverHighlight _hoverHighlight;
        
        void Awake()
        {
            _hoverHighlight = GetComponent<HoverHighlight>();
            _meshFilter = GetComponent<MeshFilter>();
        }

        private void Start()
        {
            _hoverHighlight.enableOverrideColor = Locked;
            _hoverHighlight.overrideColor = Color.red;
            _hoverHighlight.overrideColorHighlight = new Color(1.0f,0.4f,0.4f);
            _hoverHighlight.UpdateMaterialColor();
        }

        
        public bool IsAdjacent(int faceId1, int faceId2)
        {
            return FirstFace == faceId1 && SecondFace == faceId2 ||
                   FirstFace == faceId2 && SecondFace == faceId1;
        }

        public int GetOtherFace(int faceId)
        {
            return FirstFace == faceId ? SecondFace : FirstFace;
        }

        public override void Interact()
        {
            Locked = !Locked;
            _hoverHighlight.enableOverrideColor = Locked;
            if (_meshFilter != null)
            {
                _meshFilter.mesh = Locked ? Meshes[1] : Meshes[0];
                _hoverHighlight.UpdateMaterialColor();
            }
        }

        public override void ChangeInteraction(InteractionMode mode) {}

        public override void StopInteraction() {}


        public override void StartHighlight()
        {
            _hoverHighlight.StartHover();
        }

        public override void EndHighlight()
        {
            _hoverHighlight.EndHover();
        }

        public void Update() {
            Vector3 left_ctrl_pt = GameObject.Find("LeftHandAnchor").transform.position;
            Vector3 right_ctrl_pt = GameObject.Find("RightHandAnchor").transform.position;
            float dist_left_ctrl = (10.0f * (left_ctrl_pt - transform.position)).sqrMagnitude;
            float dist_right_ctrl = (10.0f * (right_ctrl_pt - transform.position)).sqrMagnitude;
            float sz = 0.1f + 0.9f / (Mathf.Min(dist_left_ctrl, dist_right_ctrl) + 1.0f);
            transform.localScale = new Vector3(sz, sz, sz);
        }

        public void UpdatePositionAndRotation(Vector3 edgeCenter, Vector3 edgeNormal, Vector3 upVector)
        {
            transform.localPosition = edgeCenter;
            transform.localRotation = Quaternion.LookRotation(edgeNormal, upVector);
        }
    }
}