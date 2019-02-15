using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.GEL;
using Controls;

// Responsible for updating the mesh based on a HMesh
[RequireComponent(typeof(MeshFilter))]
public class ExtrudableMesh : MonoBehaviour
{
    public Manifold _manifold;

    private Mesh mesh;

    public double initialSize = 0.1;

    public bool rebuild = false;



    // Use this for initialization
    void Awake()
    {
        if (mesh == null)
        {
            mesh = GetComponent<MeshFilter>().sharedMesh;
            if (mesh == null)
            {
                mesh = GetComponent<MeshFilter>().mesh;
            }

            var newMesh = new Mesh();
            newMesh.vertices = mesh.vertices;
            newMesh.normals = mesh.normals;
            newMesh.subMeshCount = 2;
            newMesh.SetIndices(mesh.triangles, MeshTopology.Triangles, 0);
            newMesh.SetIndices(new int[0], MeshTopology.Lines, 1);
            mesh = newMesh;
            mesh.UploadMeshData(false);
        }
        Reset();
    }

    public void Reset()
    {
        _manifold = BuildInitialManifold();
        TriangulateAndDrawManifold();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public Mesh GetMeshClone()
    {
        return Instantiate(mesh);
    }
    private void LateUpdate()
    {
        if (rebuild)
        {
            TriangulateAndDrawManifold();
            rebuild = false;
        }
    }

    public void StartExtrusion(int[] faceIds)
    {
        _manifold.ExtrudeFaces(faceIds);
    }

    public Vector3 GetFaceNormal(int faceId)
    {
        return _manifold.GetFaceNormal(faceId);
    }

    public void MoveTo(
        int faceId,
        int[] faceIds,
        Vector3 destinationLocalSpace, bool snap)
    {
        var translation = destinationLocalSpace - _manifold.GetCenter(faceId);
        var normal = _manifold.GetFaceNormal(faceId);
        float d = Vector3.Dot(normal, translation);
        if (snap)
            _manifold.MoveFacesAlongVector(faceIds, d * normal);
        else
            _manifold.MoveFacesAlongVector(faceIds, translation);

    }

    public void RotateFaceAroundPoint(int[] faceIds, Vector3 position, Quaternion rotation)
    {
        _manifold.RotateVerticesAroundPoint(faceIds, position, rotation);
    }

    public int CollapseShortEdges(double threshold = 0.01)
    {
        int res = _manifold.CollapseShortEdges(threshold);
        return res;
    }

    public void MoveVertexTo(int vertexId, Vector3 localSpaceDestination)
    {
        var position = _manifold.VertexPosition(vertexId);
        _manifold.MoveVertexAlongVector(vertexId, localSpaceDestination - position);
    }

    public void LoadMesh(Manifold manifold)
    {
        _manifold = manifold;
        _manifold.StitchMesh(1e-10);
        TriangulateAndDrawManifold();

        ControlsManager.Instance.Clear();
        ControlsManager.Instance.UpdateControls();
        
        UndoManager.Instance.undoActions.Clear();
        // add initial state to undo list
        UndoManager.Instance.OnUndoStartAction(null, null, Vector3.zero, Quaternion.identity);
        UndoManager.Instance.OnUndoEndAction(null, null, Vector3.zero, Quaternion.identity);
        UndoManager.Instance.position = 0;
    }

    public void UpdateMesh()
    {
        _manifold.StitchMesh(1e-10);
        TriangulateAndDrawManifold();

        mesh = GetComponent<MeshFilter>().sharedMesh;
        if (mesh == null)
        {
            mesh = GetComponent<MeshFilter>().mesh;
        }

        var newMesh = new Mesh();
        newMesh.vertices = mesh.vertices;
        newMesh.normals = mesh.normals;
        newMesh.subMeshCount = 2;
        newMesh.SetIndices(mesh.triangles, MeshTopology.Triangles, 0);
        newMesh.SetIndices(new int[0], MeshTopology.Lines, 1);
        mesh = newMesh;
        mesh.UploadMeshData(false);

        ControlsManager.Instance.Clear();
        ControlsManager.Instance.UpdateControls();
    }

    public void ChangeManifold(Manifold newManifold)
    {

        _manifold = newManifold;
        _manifold.StitchMesh(1e-10);
        TriangulateAndDrawManifold();
        //ControlsManager.Instance.Clear();
        //ControlsManager.Instance.UpdateControls();
    }

    /* UTIL FUNCTIONS */

    private Manifold BuildInitialManifold()
    {
        var manifold = new Manifold();
        double s = initialSize;
        double[] bottom =
        {
            s, -s, -s,
            -s, -s, -s,
            -s, s, -s,
            s, s, -s
        };

        double[] front =
        {
            s, -s, -s,
            s, s, -s,
            s, s, s,
            s, -s, s
        };

        double[] left =
        {
            s, -s, -s,
            s, -s, s,
            -s, -s, s,
            -s, -s, -s
        };

        double[] right =
        {
            s, s, -s,
            -s, s, -s,
            -s, s, s,
            s, s, s,
        };

        double[] top =
        {
            s, -s, s,
            s, s, s,
            -s, s, s,
            -s, -s, s
        };

        double[] back =
        {
            -s, -s, -s,
            -s, -s, s,
            -s, s, s,
            -s, s, -s,
        };

        manifold.AddFace(4, bottom);
        manifold.AddFace(4, front);
        manifold.AddFace(4, left);
        manifold.AddFace(4, right);
        manifold.AddFace(4, top);
        manifold.AddFace(4, back);

        manifold.StitchMesh(1e-10);

        return manifold;
    }

    void TriangulateAndDrawManifold()
    {

        var pointsAndQuads = _manifold.ToIdfs();

        var points = pointsAndQuads.Key;

        List<int> edges = new List<int>();
        var vertices = new Vector3[_manifold.NumberOfAllocatedVertices()];

        for (var i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3((float)points[3 * i], (float)points[3 * i + 1], (float)points[3 * i + 2]);
        }

        int[] polygons = pointsAndQuads.Value;
        List<int> polygonsFinal = new List<int>();
        List<Vector3> verticesFinal = new List<Vector3>();
        List<Vector3> normalsFinal = new List<Vector3>();

        for (var i = 0; i < polygons.Length;)
        {
            int polyCount = polygons[i];
            i++;

            // add final vertices
            int vertexBase = verticesFinal.Count;
            for (int j = 0; j < polyCount; j++)
            {
                verticesFinal.Add(vertices[polygons[i + j]]);
            }
            Vector3 normal = Vector3.zero;

            // triangulate polygon
            for (int j = 1; j + 1 < polyCount; j++)
            {
                polygonsFinal.Add(vertexBase);
                polygonsFinal.Add(vertexBase + j);
                polygonsFinal.Add(vertexBase + j + 1);

                normal += -Vector3.Cross(verticesFinal[vertexBase] - verticesFinal[vertexBase + j],
                    verticesFinal[vertexBase + j + 1] - verticesFinal[vertexBase + j]).normalized;
            }
            normal.Normalize();

            for (int j = 0; j < polyCount; j++)
            {
                normalsFinal.Add(normal);
                edges.Add(vertexBase + j);
                edges.Add(vertexBase + (j + 1) % polyCount);
            }

            i += polyCount;
        }

        mesh.name = "extruded";
        mesh.SetIndices(new int[0], MeshTopology.Triangles, 0);
        mesh.SetIndices(new int[0], MeshTopology.Lines, 1);
        mesh.vertices = verticesFinal.ToArray();
        mesh.normals = normalsFinal.ToArray();
        mesh.subMeshCount = 2;
        mesh.SetIndices(polygonsFinal.ToArray(), MeshTopology.Triangles, 0);
        mesh.SetIndices(edges.ToArray(), MeshTopology.Lines, 1);
        mesh.UploadMeshData(false);
    }
}