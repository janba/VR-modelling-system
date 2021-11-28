using System;
using System.Collections.Generic;
using System.Linq;
using Assets.GEL;
using UnityEngine;

public class MirrorMesh : MonoBehaviour {
    public Manifold Manifold;
    
    public Mesh mesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    private bool mirroringEnabled = false;

    private void Awake() {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        if (mesh != null) return;
        
        mesh = _meshFilter.sharedMesh;
        if (mesh == null)
        {
            mesh = _meshFilter.mesh;
        }

        var newMesh = new Mesh();
        newMesh.vertices = mesh.vertices;
        newMesh.normals = mesh.normals;
        newMesh.subMeshCount = 2;
        newMesh.SetIndices(mesh.triangles, MeshTopology.Triangles, 0);
        newMesh.SetIndices(new int[0], MeshTopology.Lines, 1);
        mesh = newMesh;
        mesh.UploadMeshData(false);

        mirroringEnabled = false;
        // ToggleMirroring();
    }

    public bool ToggleMirroring() {
        mirroringEnabled = !mirroringEnabled;
        _meshRenderer.enabled = mirroringEnabled;
        return mirroringEnabled;
    }

    public void UpdateMirrorMesh(Manifold originalManifold)
    {
        if (!mirroringEnabled) {
            return;
        }
        
        Manifold = UpdateManifold(originalManifold);
        TriangulateAndDrawManifold();
        _meshFilter.mesh = mesh;
    }

    private Manifold UpdateManifold(Manifold originalManifold) {
        var manifold = new Manifold();

        var pointsAndQuads = originalManifold.ToIdfs();

        var points = pointsAndQuads.Key;
        var quads = pointsAndQuads.Value;

        var vertices = new Vector3[originalManifold.NumberOfAllocatedVertices()];

        // Vertex to build from
        float minXVertex = 99999;
        
        for (var i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3((float)points[3 * i], (float)points[3 * i + 1], (float)points[3 * i + 2]);
            
            minXVertex = Mathf.Min(vertices[i].x, minXVertex);
        }

        // The origin of the mirrored figure is the distance of the closest
        // vertex times 2 (one for original object and another for the mirrored obj)
        var diff = minXVertex * 2;
        
        for (var i = 0; i < quads.Length;)
        {
            int polyCount = quads[i];
            i++;
            
            var face = new double[3 * polyCount];
            
            // for (int j = 0; j < polyCount; j++) {
            //     var vert = vertices[quads[i + j]];
            //     face[3 * j] = -vert.x + diff;
            //     face[3 * j + 1] = vert.y;
            //     face[3 * j + 2] = vert.z;
            // }
            
            int vrtxIdx = polyCount - 1; 
            for (int j = 0; j < polyCount; j++) {
                var vert = vertices[quads[i + j]];
                face[3 * vrtxIdx] = -vert.x + diff;
                face[3 * vrtxIdx + 1] = vert.y;
                face[3 * vrtxIdx + 2] = vert.z;
                vrtxIdx--;
            }

            manifold.AddFace(polyCount, face);

            i += polyCount;
        }
        
        manifold.StitchMesh(1e-10);
        return manifold;
    }
    
    private void TriangulateAndDrawManifold()
    {

        var pointsAndQuads = Manifold.ToIdfs();

        var points = pointsAndQuads.Key;

        List<int> edges = new List<int>();
        var vertices = new Vector3[Manifold.NumberOfAllocatedVertices()];

        for (var i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3((float)points[3 * i], (float)points[3 * i + 1], (float)points[3 * i + 2]);
        }

        int[] polygons = pointsAndQuads.Value;
        var polygonsFinal = new List<int>();
        var verticesFinal = new List<Vector3>();
        var normalsFinal = new List<Vector3>();

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

        
        mesh.name = "mirror";
        mesh.SetIndices(new int[0], MeshTopology.Triangles, 0);
        mesh.SetIndices(new int[0], MeshTopology.Lines, 1);
        mesh.vertices = verticesFinal.ToArray();
        mesh.normals = normalsFinal.ToArray();
        mesh.subMeshCount = 2;
        mesh.SetIndices(polygonsFinal.ToArray(), MeshTopology.Triangles, 0);
        mesh.SetIndices(edges.ToArray(), MeshTopology.Lines, 1);
        mesh.UploadMeshData(false);


        mesh.RecalculateBounds();
    }
}
