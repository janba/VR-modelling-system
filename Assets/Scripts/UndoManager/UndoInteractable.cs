using System.Collections;
using System.Collections.Generic;
using Controls;
using UnityEngine;

public class UndoInteractable : IInteractableObject
{

    public bool undo = true;

    public bool show = true;

    private HoverHighlight hover;
    private UndoManager undoManager;

    void Awake()
    {
        if (!show)
        {
            gameObject.SetActive(false);
        }
        hover = GetComponent<HoverHighlight>();
        undoManager = GetComponentInParent<UndoManager>();
    }

    public override void Interact()
    {
        var undoManager = FindObjectOfType<UndoManager>();
        if (undoManager)
        {
            if (undo)
                undoManager.MoveBackwards();
            else
                undoManager.MoveForward();
        }
        else
        {
            Debug.Log("Cannot find manager");
        }
        undoManager.previewPrev = false;
        undoManager.previewNext = false;
        undoManager.UpdatePreview();
    }

    public override void ChangeInteraction(InteractionMode mode)
    {

    }

    public override void StopInteraction()
    {

    }

    public override void StartHighlight()
    {
        hover.StartHover();
        if (undo)
        {
            undoManager.previewPrev = true;
        }
        else
        {
            undoManager.previewNext = true;
        }
        undoManager.UpdatePreview();
    }

    public override void EndHighlight()
    {
        hover.EndHover();
        if (undo)
        {
            undoManager.previewPrev = false;
        }
        else
        {
            undoManager.previewNext = false;
        }

        undoManager.UpdatePreview();
    }
}
