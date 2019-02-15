using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using Assets.GEL;
using System.IO;

public class ScreenController : MonoBehaviour {

    [SerializeField]
    private ExtrudableMesh _extrudableMesh;
    //[SerializeField]
    public GameObject ExportUI;
    //[SerializeField]
    public GameObject ImportUI;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
        if (Input.GetKeyDown("s"))
        {
            RefinementTest();
        }

        else if (Input.GetKeyDown("l"))
        {
            LoadTest();
        }
    }

    public void OpenImportUI(bool active) {
        ExportUI.SetActive(false);
        ImportUI.SetActive(active);
        if (active)
        {
            PopulateImportPanel();
        }
    }

    public void OpenExportUI(bool active)
    {
        ImportUI.SetActive(false);
        ExportUI.SetActive(active);
    }

    public bool ExportMesh()
    {
        Text[] UITexts = ExportUI.GetComponentsInChildren<Text>();
        foreach(Text t in UITexts)
        {
            if (t.gameObject.tag == "InputField")
            {
                string file_name = t.text;
                if (!String.IsNullOrEmpty(file_name))
                {
                    return ExportMesh(file_name);
                }
            }
        }
        return false;
    }
    

    public bool ExportMesh(string fname)
    {
        string fileWithPath = String.Format("Assets/Models/Saved/{0}.obj", fname);
        
        bool saved = _extrudableMesh._manifold.SaveToOBJ(fileWithPath);
        Text TextUI = GetComponentInChildren<Text>();
        if (saved)
        {
            string otherText = TextUI.text;
            TextUI.text = String.Format("{0}\n>> 3D Model saved to\n>>  \t{1}", otherText, fileWithPath);
        }
        else
        {
            string otherText = TextUI.text;
            TextUI.text = String.Format("{0}\n>> 3D Model FAILED to save", otherText);
        }

        TextUI.fontSize = 75;
        return saved;

    }

    public bool LoadMesh(string filename)
    {
        filename = String.Format("Assets/Models/Saved/{0}", filename);
        _extrudableMesh._manifold = new Manifold();

        var manifold = new Manifold();

        bool loaded = manifold.LoadFromOBJ(filename);
        Text TextUI = GetComponentInChildren<Text>();
        if (loaded)
        {
            string otherText = TextUI.text;
            TextUI.text = String.Format("{0}\n>> 3D Model loaded", otherText);
            manifold.StitchMesh(1e-10);
            _extrudableMesh.LoadMesh(manifold);
        }
        else
        {
            string otherText = TextUI.text;
            TextUI.text = String.Format("{0}\n>> 3D Model FAILED to load", otherText);
        }

        TextUI.fontSize = 75;
        return loaded;
    }

    void PopulateImportPanel()
    {
        ImportUI.GetComponentInChildren<LoadSelectionScreen>().PopulateScreen();
    }


    public int RefinementTest()
    {
        //int[] faces = new int[_extrudableMesh._manifold.NumberOfFaces()];
        //int[] edges = new int[_extrudableMesh._manifold.NumberOfHalfEdges()];
        //int[] vertices = new int[_extrudableMesh._manifold.NumberOfVertices()];
        //_extrudableMesh._manifold.GetHMeshIds(faces, edges, vertices);
        //foreach(int i in edges)
        //{
        //    Debug.Log(i);
        //}
        //IntVector hedges = _extrudableMesh._manifold.GetHalfEdges();
        int output = _extrudableMesh._manifold.SplitEdge(0);
        int output2 = _extrudableMesh._manifold.SplitEdge(4);
        int output3 = _extrudableMesh._manifold.SplitFaceByEdges(0, output, output2);
        _extrudableMesh.UpdateMesh();
        Debug.Log(output);
        Debug.Log(output2);
        Debug.Log(output3);
        return 1;
    }

    public bool LoadTest()
    {
        string filename = String.Format("Assets/Models/Saved/{0}", "testing.obj");
        _extrudableMesh._manifold = new Manifold();

        var manifold = new Manifold();

        bool loaded = manifold.LoadFromOBJ(filename);
        if (loaded)
        {
            manifold.StitchMesh(1e-10);
            _extrudableMesh.LoadMesh(manifold);
        }
        
        return loaded;
    }
}
