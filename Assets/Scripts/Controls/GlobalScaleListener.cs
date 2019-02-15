using System.Collections;
using System.Collections.Generic;
using Controls;
using UnityEngine;

public class GlobalScaleListener : MonoBehaviour
{

	public bool inverse = true;

	public float maxSize = 999999;
	
	public float initialScale;

	private ControlsManager _controlsManager;
	
	// Use this for initialization
	void Start ()
	{
		initialScale = transform.localScale.x;
		_controlsManager = FindObjectOfType<ControlsManager>();
		_controlsManager.globalScaleChange += ControlsManagerOnGlobalScaleChange;
		ControlsManagerOnGlobalScaleChange(_controlsManager.globalScale);
	}

	private void OnDestroy()
	{
		_controlsManager.globalScaleChange -= ControlsManagerOnGlobalScaleChange;
	}

	private void ControlsManagerOnGlobalScaleChange(float val)
	{
		transform.localScale = Vector3.one * (initialScale *Mathf.Min(maxSize, inverse ? 1 / val : val));
	}
}
