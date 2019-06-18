using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Assets.GEL
{
    public class FaceIdAndEdgeCenter
    {
        public int[] edgeId;
        public int[] faceId;
        public Vector3[] edgeCenter;
        public Vector3[] edgeNormals;
    }

    public class Manifold : IManifold
    {
        private IntPtr _manifold;

        public Manifold()
        {
            _manifold = Manifold_new();
        }

        public Manifold(IntPtr manifold)
        {
            _manifold = manifold;
        }

        ~Manifold()
        {
            Manifold_delete(_manifold);
        }

        public Manifold Copy()
        {
            return new Manifold(Manifold_copy(_manifold));
        }

        public int NumberOfAllocatedVertices()
        {
            return Manifold_no_allocated_vertices(_manifold);
        }

        public int NumberOfAllocatedFaces()
        {
            return Manifold_no_allocated_faces(_manifold);
        }

        public int NumberOfAllocatedHalfEdges()
        {
            return Manifold_no_allocated_halfedges(_manifold);
        }

        public int GetOppHalfEdge(int halfedgeId)
        {
            return Walker_opposite_halfedge(_manifold, halfedgeId);
        }

        public int NumberOfVertices()
        {
            return Manifold_no_vertices(_manifold);
        }

        public int NumberOfFaces()
        {
            return Manifold_no_faces(_manifold);
        }

        public int NumberOfHalfEdges()
        {
            return Manifold_no_halfedges(_manifold);
        }

        public int NumberOfPolygons()
        {
            return Manifold_no_polygon_indices(_manifold);
        }

        public int GetVertices(IntPtr[] vert)
        {
            return Manifold_vertices(_manifold, vert);
        }

        public IntVector GetVertices()
        {
            //            var n = NumberOfAllocatedVertices();
            var intVector = new IntVector(0);
            Manifold_vertices(_manifold, intVector.GetVector());
            return intVector;
        }
        
        public void BridgeFaces(int fid1, int fid2, int[] f1vids, int[] f2vids, int noVertPairs)
        {
            Manifold_bridge_faces(_manifold, fid1, fid2, f1vids, f2vids, noVertPairs);
        }
        
        [Obsolete("GetFaces is deprecated.")]
        public int GetFaces(IntPtr[] faces)
        {
            return Manifold_faces(_manifold, faces);
        }

        public IntVector GetFaces()
        {
            var n = NumberOfAllocatedFaces();
            var intVector = new IntPtr[n];
            var i = Manifold_faces(_manifold, intVector);
            return new IntVector(intVector);
        }

        public IntVector GetHalfEdges()
        {
            var n = NumberOfAllocatedHalfEdges();
            var intVector = new IntVector(n);
            Manifold_halfedges(_manifold, intVector.GetVector());
            return intVector;
        }

        public void AddFace(int noVertices, double[] pos)
        {
            Manifold_add_face(_manifold, noVertices, pos);
        }

        public void RemoveFace(int fid)
        {
            Manifold_remove_face(_manifold, fid);
        }

        public static Manifold RandomMesh(int noIter)
        {
            var random = new Manifold();
            randomize_mesh(random._manifold, noIter);
            return random;
        }

        public void TestValery()
        {
            test_valery(_manifold);
        }

        public void ExtrudeFaces(int[] faces)
        {
            extrude_faces(_manifold, faces.Length, faces);
        }

        public double[,] Positions()
        {
            double[,] positions = new double[NumberOfAllocatedVertices(), 3];
            Manifold_positions(_manifold, positions);
            return positions;
        }

        public Vector3 VertexPosition(int vertexId)
        {
            double[] positions = new double[3];
            Manifold_pos(_manifold, vertexId, positions);
            return new Vector3((float)positions[0], (float)positions[1], (float)positions[2]);
        }

        public bool IsVertexInUse(int vertexId)
        {
            return Manifold_vertex_in_use(_manifold, vertexId);
        }

        public bool IsFaceInUse(int faceId)
        {
            return Manifold_face_in_use(_manifold, faceId);
        }

        public bool IsHalfedgeInUse(int edgeId)
        {
            return Manifold_halfedge_in_use(_manifold, edgeId);
        }

        public void Triangulate()
        {
            triangulate(_manifold);
        }

        public KeyValuePair<double[], int[]> ToIdfs()
        {
            double[] points = new double[NumberOfAllocatedVertices() * 3];
            int[] polygons = new int[NumberOfPolygons()];
            to_idfs(_manifold, points, polygons);
            return new KeyValuePair<double[], int[]>(points, polygons);
        }

        public void MoveVertexAlongVector(int vertexId, Vector3 direction)
        {
            move_vertex_along_vector(_manifold, vertexId, new double[] { direction.x, direction.y, direction.z });
        }

        public void MoveFaceAlongVector(int faceId, Vector3 direction)
        {
            MoveFaceAlongVector(faceId, new double[] { direction.x, direction.y, direction.z });
        }

        public void MoveFaceAlongVector(int faceId, double[] direction)
        {
            move_face_along_vector(_manifold, faceId, direction);
        }

        public void MoveFacesAlongVector(int[] faceIds, Vector3 direction)
        {
            MoveFacesAlongVector(faceIds, new double[] { direction.x, direction.y, direction.z });
        }
        
        public void RotateVerticesAroundPoint(int[] faceIds, Vector3 position, Quaternion rotation)
        {
            var pos = new double[] { position.x, position.y, position.z };
            var quat = new double[] { rotation.x, rotation.y, rotation.z, rotation.w };
            rotate_faces_around_point(_manifold, faceIds.Length, faceIds, pos, quat);
        }
        
        public void MoveFacesAlongVector(int[] faceIds, double[] direction)
        {
            move_faces_along_vector(_manifold, faceIds.Length, faceIds, direction);
        }

        public void MoveFacesAlongNormal(int[] faceIds, double magnitude)
        {
            move_faces_along_normal(_manifold, faceIds.Length, faceIds, magnitude);
        }

        public void StitchMesh(double rad)
        {
            stitch_mesh(_manifold, rad);
        }

        public void CleanUp()
        {
            Manifold_cleanup(_manifold);
        }

        public Vector3 GetCenter(int faceId)
        {
            double[] c = new double[3];
            center(_manifold, faceId, c);
            return new Vector3((float)c[0], (float)c[1], (float)c[2]);
        }

        public Vector3 GetCenterTriangulated(int faceId)
        {
            double[] c = new double[3];
            center_triangulated(_manifold, faceId, c);
            return new Vector3((float)c[0], (float)c[1], (float)c[2]);
        }


        public Vector3 GetFaceNormal(int faceId)
        {
            double[] n = new double[3];
            face_normal(_manifold, faceId, n);
            return new Vector3((float)n[0], (float)n[1], (float)n[2]);
        }

        public Vector3 GetVertexNormal(int vertexId)
        {
            double[] n = new double[3];
            vertex_normal(_manifold, vertexId, n);
            return new Vector3((float)n[0], (float)n[1], (float)n[2]);
        }



        public FaceIdAndEdgeCenter GetAdjacentFaceIdsAndEdgeCenters(int faceId)
        {
            FaceIdAndEdgeCenter res = new FaceIdAndEdgeCenter();
            var edgeCount = no_edges(_manifold, faceId);
            res.faceId = new int[edgeCount];
            res.edgeCenter = new Vector3[edgeCount];
            res.edgeNormals = new Vector3[edgeCount];
            res.edgeId = new int[edgeCount];
            double[] edge_centers = new double[edgeCount * 3];
            double[] edge_normals = new double[edgeCount * 3];
            get_neighbouring_faces_and_edge(_manifold, faceId, res.faceId, edge_centers, edge_normals, res.edgeId);
            for (int i = 0; i < edgeCount; i++)
            {
                res.edgeCenter[i] = new Vector3((float)edge_centers[3 * i], (float)edge_centers[3 * i + 1], (float)edge_centers[3 * i + 2]);
                res.edgeNormals[i] = new Vector3((float)edge_normals[3 * i], (float)edge_normals[3 * i + 1], (float)edge_normals[3 * i + 2]);
            }

            return res;
        }

        public void TestExtrudeAndMove()
        {
            test_extrude_and_move(_manifold);
        }

        public Vector3 GetFirstEdgeDirection(int faceId)
        {
            double[] n = new double[3];
            get_first_edge_direction(_manifold, faceId, n);
            return new Vector3((float)n[0], (float)n[1], (float)n[2]);
        }

        public int CollapseShortEdges(double threshold = 0.01)
        {
            int[] i = { };
            return collapse_short_edges(_manifold, threshold, i, 0);
        }

        public void GetHMeshIds(int[] vertex_ids, int[] halfedge_ids, int[] face_ids)
        {
            get_hmesh_ids(_manifold, vertex_ids, halfedge_ids, face_ids);
        }

        public int GetVertexId(int halfedgeid)
        {
            return Walker_incident_vertex(_manifold, halfedgeid);
        }

        public HashSet<int> GetVertexIds(int faceId)
        {
            var adjacentFaceIds = GetAdjacentFaceIdsAndEdgeCenters(faceId);
            HashSet<int> res = new HashSet<int>();
            foreach (var edgeId in adjacentFaceIds.edgeId)
            {
                res.Add(Walker_incident_vertex(_manifold, edgeId));
            }

            return res;
        }

        public Dictionary<int, Vector3> GetVertexPositionsFromFace(int faceId)
        {
            HashSet<int> vertices = GetVertexIds(faceId);
            Dictionary<int, Vector3> res = new Dictionary<int, Vector3>();

            foreach (int vertexId in vertices)
            {
                res.Add(vertexId, VertexPosition(vertexId));
            }
            return res;
        }

        public Dictionary<int, Vector3> GetVertexPositionsFromFaces(List<int> faceIds)
        {
            Dictionary<int, Vector3> res = new Dictionary<int, Vector3>();

            foreach (int faceId in faceIds)
            {
                HashSet<int> vertices = GetVertexIds(faceId);

                foreach (int vertexId in vertices) if (!res.ContainsKey(vertexId))
                {
                    res.Add(vertexId, VertexPosition(vertexId));
                }
            }
            return res;
        }

        public bool SaveToOBJ(string file_name)
        {
            return obj_save(file_name, _manifold);
        }

        public bool LoadFromOBJ(string file_name)
        {
            return obj_load(file_name, _manifold);
        }

        public bool SaveToX3d(string file_name)
        {
            return x3d_save(file_name, _manifold);
        }

        public bool LoadFromX3d(string file_name)
        {
            return x3d_load(file_name, _manifold);
        }

        public int SplitFaceByEdges(int face, int edge1, int edge2)
        {
            return Manifold_split_face_by_edge(_manifold, face, edge1, edge2);
        }

        public int SplitEdge(int edge)
        {
            return Manifold_split_edge(_manifold, edge);
        }

        public int GetNextHalfEdge(int halfedgeId)
        {
            return Walker_next_halfedge(_manifold, halfedgeId);
        }

        public int GetIncidentFace(int halfedgeId)
        {
            return Walker_incident_face(_manifold, halfedgeId);
        }

        public bool RefineFaceloop(int halfedge1, int halfedge2)
        {
            if (!this.ValidateFaceLoop(halfedge1, halfedge2))
            {
                return false;
            }

            int h = halfedge1;
            int initital_f = this.GetIncidentFace(halfedge1);
            int f = initital_f;

            List<int> faceloop = new List<int>();
            List<int> edgeloop = new List<int>();

            while (true)
            {
                if (this.IsFaceInUse(f))
                {
                    if (this.IsHalfedgeInUse(h))
                    {
                        int h2 = this.GetNextHalfEdge(h);
                        h2 = this.GetNextHalfEdge(h2);
                        if (this.IsHalfedgeInUse(h2))
                        {
                            int w = this.SplitEdge(h);
                            edgeloop.Add(w);
                            faceloop.Add(f);

                            h = this.GetOppHalfEdge(h2);
                            f = this.GetIncidentFace(h);
                            if (f == initital_f)
                            {
                                int[] farray = faceloop.ToArray();
                                int[] earray = edgeloop.ToArray();
                                for (int i = 0; i < farray.Length; i++)
                                {
                                    if (i == farray.Length - 1)
                                    {
                                        this.SplitFaceByEdges(farray[i], earray[i], earray[0]); // earray[1]);
                                    }
                                    else
                                    {
                                        this.SplitFaceByEdges(farray[i], earray[i], earray[i + 1]);
                                    }

                                }
                                //UpdateMesh();
                                this.StitchMesh(1e-10);
                                //TriangulateAndDrawManifold();
                                return true;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public bool ValidateFaceLoop(int halfedgeId1, int halfedgeId2)
        {
            //Manifold manifold = _manifold;

            if (!this.IsHalfedgeInUse(halfedgeId1) || !this.IsHalfedgeInUse(halfedgeId2))
            {
                return false;
            }

            int f = this.GetIncidentFace(halfedgeId1);
            int h = halfedgeId1;
            while (true)
            {
                int h2 = this.GetNextHalfEdge(h);
                h2 = this.GetNextHalfEdge(h2);
                int h3 = this.GetOppHalfEdge(h2);


                if (h2 == halfedgeId2 || h3 == halfedgeId2)
                {
                    return true;
                }

                else if (this.GetIncidentFace(h3) == f)
                {
                    return false;
                }
                h = h3;
            }
        }
    }
}
