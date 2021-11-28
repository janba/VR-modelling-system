using UnityEngine;
public class MirroringButton : MonoBehaviour {
    
    private Material _material;
    private Color _initialColor;
    
    private WorktableController _worktableController;

    private bool _mirroringEnabled = false;

    private void Start() {
        _material = GetComponent<MeshRenderer>().material;
        _initialColor = _material.color;
        
        _worktableController = transform.root.gameObject.GetComponent<WorktableController>();
        
        // _mirroringEnabled = _worktableController.ToggleMirrorMesh();
    }

    private void OnTriggerEnter(Collider other) {
        // Debug.Log("Trigger mirroring entered");
        _material.color = Color.yellow;
    }

    private void OnTriggerExit(Collider other) {
        // Debug.Log("Trigger mirroring exited");
        _mirroringEnabled = _worktableController.ToggleMirrorMesh();
        
        _material.color = _mirroringEnabled ? Color.green : _initialColor;
        
    }
}
