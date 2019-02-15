using System.Collections;
using System.Collections.Generic;
using Controls;
using UnityEngine;

public class ResetButton : IInteractableObject
{
	public MeshRenderer[] highlightMeshes;
	private Color[] highlightColors;

	public Color highlightColor = Color.red;

	private ControlsManager controlsManager;
	
	public void Awake()
	{
		highlightColors = new Color[highlightMeshes.Length];
		controlsManager = FindObjectOfType<ControlsManager>();
	}

	public override void Interact()
	{
		controlsManager.Reset();
	}

	public override void ChangeInteraction(InteractionMode mode)
	{
	}

	public override void StopInteraction()
	{
		
	}

	public override void StartHighlight()
	{
		for (int i = 0; i < highlightMeshes.Length; i++)
		{
			highlightColors[i] = highlightMeshes[i].material.color;
			highlightMeshes[i].material.color = highlightColor;
		}
	}

	public override void EndHighlight()
	{
		for (int i = 0; i < highlightMeshes.Length; i++)
		{
			highlightMeshes[i].material.color = highlightColors[i];
		}
	}
}
