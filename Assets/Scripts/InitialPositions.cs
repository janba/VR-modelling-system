using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialPositions : MonoBehaviour {

    public Transform VRtransform;
    public Vector3 offset = new Vector3(-0.3324f, 0.21365f, -0.2710f);

    // Use this for initialization
    void Start () {
        transform.position = VRtransform.position - offset;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
