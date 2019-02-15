using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// autohide controls based on touch
// does not work well
public class AutoHideControls : MonoBehaviour {
	public Camera[] cameras;

	public bool wasTouching = false;

	public LayerMask _layerMask;

	public string value;
	
	// Use this for initialization
	void Start ()
	{
		cameras = FindObjectsOfType<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
		bool isTouchingHandles = OVRInput.Get(OVRInput.Touch.Any) || 
		                         OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > 0 || 
		                         OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0 ;
		if (wasTouching != isTouchingHandles)
		{
			wasTouching = isTouchingHandles;
			_layerMask.value = (~0 - (1 << 8)) | (isTouchingHandles ? (1<<8) : 0);
			value = _layerMask.value.ToString("X"); 
			foreach (var cam in cameras)
			{
				cam.cullingMask = _layerMask.value;
			}
		}
		
	}
}
