using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeMirrorButton : MonoBehaviour {
    private Material _material;
    private Color _initialColor;
    
    private WorktableController _worktableController;

    private bool _canMerge = true;
    
    private void Start() {
        _material = GetComponent<MeshRenderer>().material;
        _initialColor = _material.color;
        
        _worktableController = transform.root.gameObject.GetComponent<WorktableController>();
    }

    private void OnTriggerEnter(Collider other) {
        // Debug.Log("Trigger mirroring entered");
        _material.color = Color.yellow;
    }

    private void OnTriggerExit(Collider other) {
        // Debug.Log("Trigger mirroring exited");
        _material.color = Color.red;

        if (!_canMerge) {
            return;
        }
        _canMerge = false;
        
        _worktableController.MergeMirror();

        Invoke ("EnableMerge", 2);
    }
    
    public void EnableMerge()
    {
        _canMerge = true;
        _material.color = _initialColor;
    }
}