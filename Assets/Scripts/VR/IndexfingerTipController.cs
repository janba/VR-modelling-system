using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndexfingerTipController : MonoBehaviour {

	public OVRInput.Controller Controller = OVRInput.Controller.RTouch;
	
	// Update is called once per frame
	void Update ()
	{
		var skinnedMeshRenderer = transform.parent.GetComponentInChildren<SkinnedMeshRenderer>();
		if (skinnedMeshRenderer)
		{
			foreach (Transform handElem in skinnedMeshRenderer.transform.GetChild(0).GetChild(0))
			{
				if (handElem.name.EndsWith("index1"))
				{
					var tip = handElem.GetChild(0).GetChild(0).GetChild(0);
					transform.parent = tip;
					transform.localPosition = Vector3.zero;
					enabled = false;
				}
			}
		}
	}
}
