using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverHighlight : MonoBehaviour
{
	private Color initialColor;
	public Color overrideColor;
	public Color overrideColorHighlight;
	public bool enableOverrideColor;
	private Color highlightColor = Color.yellow;
	private Material material;
	private bool isHovering = false;
	void Awake()
	{
		var component = GetComponent<MeshRenderer>();
		material = component.material; // create material instance
		initialColor = material.color;
	}

	public void StartHover()
	{
		isHovering = true;
		UpdateMaterialColor();
	}
	
	public void EndHover()
	{
		isHovering = false;
		UpdateMaterialColor();	
	}

	public void UpdateMaterialColor()
	{
		if (isHovering)
			material.color = enableOverrideColor ? overrideColorHighlight : highlightColor;
		else 
			material.color = enableOverrideColor ? overrideColor : initialColor;
	}
}
