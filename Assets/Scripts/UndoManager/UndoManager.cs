using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Assets.GEL;
using Controls;
using UnityEngine;

public class UndoManager : MonoBehaviour
{
    public class UndoData
    {
        public Manifold manifold;
        public Mesh manifoldMesh;
        public Vector3 scale;
        public float initialSize;
        public float maxScaleValue;
        public Vector3 positionInitial;
        public Quaternion rotationInitial;
        public Vector3 positionFinal;
        public Quaternion rotationFinal;
        public float colliderSize;
        public Vector3 colliderOffset;
        public Mesh handleMesh;

        public UndoData(Manifold manifold, Mesh manifoldMesh, Vector3 positionInitial, Quaternion rotationInitial, Vector3 positionFinal,
            Quaternion rotationFinal, Mesh handleMesh, Vector3 scale, float colliderSize, Vector3 colliderOffset)
        {
            this.manifold = manifold;
            this.manifoldMesh = manifoldMesh;
            this.positionInitial = positionInitial;
            this.rotationInitial = rotationInitial;
            this.positionFinal = positionFinal;
            this.rotationFinal = rotationFinal;
            this.handleMesh = handleMesh;
            this.scale = scale;
            this.colliderSize = colliderSize;
            this.colliderOffset = colliderOffset;
        }
    }

    public MeshFilter actionPreviewMesh;
    public MeshRenderer actionPreview;

    public bool previewNext = false;
    public bool previewPrev = false;

    private ExtrudableMesh _extrudableMesh;
    private ControlsManager _controlsManager;

    public MeshFilter prevMesh;
    public MeshFilter nextMesh;
    public MeshRenderer prevRenderer;
    public MeshRenderer nextRenderer;
    public SphereCollider prevCollider;
    public SphereCollider nextCollider;


	public int position = 0;
	public int maxSteps = 10;
	public List<UndoData> undoActions;
	public bool hideUndo = false;
    public UndoData tempData;
	
	public bool disableVisibleUndoWidgets = false;

	public static UndoManager Instance;
	
	// Use this for initialization
	void Awake ()
	{
		Instance = this;
		_extrudableMesh = FindObjectOfType<ExtrudableMesh>();
		_controlsManager = FindObjectOfType<ControlsManager>();
		undoActions = new List<UndoData>();
		_controlsManager.undoReset += OnModelReset;
		_controlsManager.undoStartEventHandlers += OnUndoStartAction;
		_controlsManager.undoEndEventHandlers += OnUndoEndAction;
		UpdateVisuals();
	}

    void OnDestroy()
    {
        if (_controlsManager)
        {
            _controlsManager.undoStartEventHandlers -= OnUndoStartAction;
            _controlsManager.undoEndEventHandlers -= OnUndoEndAction;
            _controlsManager.undoReset -= OnModelReset;
        }
    }

    private void OnModelReset()
    {
        undoActions.Clear();
        // add initial state to undo list
        OnUndoStartAction(null, null, Vector3.zero, Quaternion.identity);
        OnUndoEndAction(null, null, Vector3.zero, Quaternion.identity);
        position = 0;
    }

	public void OnUndoStartAction(Mesh handleMesh, IInteractableObject interactableObject, Vector3 initialPosition,
		Quaternion initialRotation)
	{
        while (position < undoActions.Count - 1)
		{
			undoActions.RemoveAt(undoActions.Count - 1); // todo replace with if + RemoveRange 
		}

        if (position == maxSteps)
        {
            undoActions.RemoveAt(0); // argument out of range sometimes.
            position--; // part of quick fix 
        }
        else
        {
            //position++;
        }

        Transform t = interactableObject != null ? interactableObject.transform : transform;
        float radius = interactableObject != null ? interactableObject.GetComponent<SphereCollider>().radius : 0.0f;
        Vector3 colliderOffset = interactableObject != null ? interactableObject.GetComponent<SphereCollider>().center : Vector3.zero;
        var data = new UndoData(null, null, initialPosition, initialRotation, t.localPosition, t.localRotation,
            handleMesh, t.localScale, radius, colliderOffset);
        if (interactableObject)
        {
            var gsl = interactableObject.GetComponent<GlobalScaleListener>();
            data.initialSize = gsl.initialScale;
            data.maxScaleValue = gsl.maxSize;
        }
        else
        {
            data.initialSize = 0;
        }

        //undoActions.Add(data); // outcomment part of test fix
        tempData = data; // possible quick fix
        UpdateVisuals();
    }

    public void OnUndoEndAction(Mesh handleMesh, IInteractableObject interactableObject, Vector3 initialPosition, Quaternion initialRotation)
	{
        Transform t = interactableObject != null ? interactableObject.transform : transform;
		//int index = Mathf.Max(0, undoActions.Count - 1);
        tempData.positionFinal = t.localPosition;
        tempData.rotationFinal = t.localRotation;
        tempData.manifold = _extrudableMesh._manifold.Copy();
        tempData.manifoldMesh = _extrudableMesh.GetMeshClone();
        //undoActions[index].positionFinal = t.localPosition;
        //undoActions[index].rotationFinal = t.localRotation;
        //undoActions[index].manifold = _extrudableMesh._manifold.Copy();
        //undoActions[index].manifoldMesh = _extrudableMesh.GetMeshClone();
        undoActions.Add(tempData); // possible fix
        position++; // possible fix
        UpdateVisuals();
    }

	public bool MoveForward()
	{
		if (position +1 >= undoActions.Count) return false;
		position++;
		_extrudableMesh._manifold = undoActions[position].manifold.Copy();
		_extrudableMesh.rebuild = true;
        //_controlsManager.DestroyInvalidObjects();
        _controlsManager.Clear();
        _controlsManager.UpdateControls();
		
		UpdateVisuals();
		
		return true;
	}

	public bool MoveBackwards()
	{
        if (position - 1 < 0)
		{
			return false;
		}
		position--;

        _extrudableMesh._manifold = undoActions[position].manifold.Copy();
        _extrudableMesh.rebuild = true;
        _controlsManager.Clear();
        _controlsManager.UpdateControls();
        //_controlsManager.DestroyInvalidObjects();
        //_controlsManager.UpdateControls();
        
        UpdateVisuals();
 

        return true;
    }

	public void UpdateVisuals()
	{
		if (disableVisibleUndoWidgets) return;
		if (position > 0 && undoActions.Count>1 && !hideUndo)
		{
			prevMesh.mesh = undoActions[position].handleMesh;
			prevMesh.transform.localPosition = undoActions[position].positionInitial; 
			prevMesh.transform.localRotation = undoActions[position].rotationInitial; 
			prevMesh.transform.localScale = undoActions[position].scale;
			prevCollider.radius = undoActions[position].colliderSize;
			prevCollider.center = undoActions[position].colliderOffset;
			prevRenderer.enabled = true;
			prevCollider.enabled = true;
			var gsl = prevMesh.GetComponent<GlobalScaleListener>(); 
			gsl.initialScale = undoActions[position].initialSize;
			gsl.maxSize = undoActions[position].maxScaleValue;
		}
		else
		{
			prevRenderer.enabled = false;
			prevCollider.enabled = false;
		}

        if (position < undoActions.Count - 1 && !hideUndo)
        {
            nextMesh.mesh = undoActions[position + 1].handleMesh;
            nextMesh.transform.localPosition = undoActions[position + 1].positionFinal;
            nextMesh.transform.localRotation = undoActions[position + 1].rotationFinal;
            nextMesh.transform.localScale = undoActions[position + 1].scale;
            nextCollider.radius = undoActions[position + 1].colliderSize;
            nextCollider.center = undoActions[position + 1].colliderOffset;
            nextRenderer.enabled = true;
            nextCollider.enabled = true;
            var gsl = nextCollider.GetComponent<GlobalScaleListener>();
            gsl.initialScale = undoActions[position + 1].initialSize;
            gsl.maxSize = undoActions[position + 1].maxScaleValue;
        }
        else
        {
            nextRenderer.enabled = false;
            nextCollider.enabled = false;
        }
    }

	public void UpdatePreview() {
		if (previewNext && position < undoActions.Count - 1)
		{
			actionPreviewMesh.mesh = undoActions[position + 1].manifoldMesh;
			actionPreview.enabled = true;
		} else if (previewPrev && position > 0 && undoActions.Count>1) {
			actionPreviewMesh.mesh = undoActions[position-1].manifoldMesh;
			actionPreview.enabled = true;
		} else {
			actionPreview.enabled = false;
		}
	}
}
