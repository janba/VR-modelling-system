using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ExtrudableMesh))]
public class ExtrudeMeshEditor : Editor {
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ExtrudableMesh ex = target as ExtrudableMesh;
        if (GUILayout.Button("Print debug"))
        {
            var manifold = ex._manifold; 
            var faceIds = new int[manifold.NumberOfFaces()];
            var vertexIds = new int[manifold.NumberOfVertices()];
            var halfedgeIds = new int[manifold.NumberOfHalfEdges()];
            manifold.GetHMeshIds(vertexIds, halfedgeIds, faceIds);
            string res = "";
            res += "\nVertexIds: ";
            foreach (var vertexId in vertexIds)
            {
                res += vertexId + ", ";
            }
            res +=  "\nFaceids: ";
            foreach (var faceId in faceIds)
            {
                res += faceId + ", ";
            }
           
            res += "\nHalfedgeIds: ";
            foreach (var halfedgeId in halfedgeIds)
            {
                res += halfedgeId + ", ";
            }

            Debug.Log(res);
        }

    }
}
