//
//  hmesh_functions.cpp
//  PyGEL
//
//  Created by Jakob Andreas Bærentzen on 04/11/2017.
//  Copyright © 2017 Jakob Andreas Bærentzen. All rights reserved.
//
#include "hmesh_functions.h"
#include <string>
#include <fstream>
#include <iostream>

#include <GEL/HMesh/triangulate.h>
#include <GEL/CGLA/Mat4x4d.h>
#include <GEL/CGLA/Quatd.h>

using namespace std;
using namespace HMesh;


bool valid(const Manifold* m_ptr) {
    return valid(*m_ptr);
}
bool closed(const Manifold* m_ptr) {
    return closed(*m_ptr);
}

void bbox(const Manifold* m_ptr, CGLA::Vec3d* pmin, CGLA::Vec3d* pmax) {
    bbox(*m_ptr, *pmin, *pmax);
}
void bsphere(const Manifold* m_ptr, CGLA::Vec3d* c, double* _r) {
    float r;
    bsphere(*m_ptr, *c, r);
    *_r = r;
}

void stitch_mesh(HMesh::Manifold* m_ptr, double rad) {
    HMesh::stitch_mesh(*m_ptr, rad);
}

bool obj_load(char* fn, HMesh::Manifold* m_ptr) {
    return obj_load(string(fn), *m_ptr);
}

bool off_load(char* fn, HMesh::Manifold* m_ptr) {
    return off_load(string(fn), *m_ptr);
}

bool ply_load(char* fn, HMesh::Manifold* m_ptr) {
    return ply_load(string(fn), *m_ptr);
}

bool x3d_load(char* fn, HMesh::Manifold* m_ptr) {
    return x3d_load(string(fn), *m_ptr);
}


bool obj_save(char* fn, HMesh::Manifold* m_ptr) {
    return obj_save(string(fn), *m_ptr);
}

bool off_save(char* fn, HMesh::Manifold* m_ptr) {
    return off_save(string(fn), *m_ptr);

}
bool x3d_save(char* fn, HMesh::Manifold* m_ptr) {
    return x3d_save(string(fn), *m_ptr);
}


void remove_caps(HMesh::Manifold* m_ptr, float thresh) {
    remove_caps(*m_ptr, thresh);
}

void remove_needles(HMesh::Manifold* m_ptr, float thresh, bool averagePositions) {
    remove_needles(*m_ptr, thresh, averagePositions);
}

void close_holes(HMesh::Manifold* m_ptr) {
    close_holes(*m_ptr);
}

void flip_orientation(HMesh::Manifold* m_ptr) {
    flip_orientation(*m_ptr);
}

void minimize_curvature(HMesh::Manifold* m_ptr, bool anneal) {
    minimize_curvature(*m_ptr, anneal);
}

void minimize_dihedral_angle(HMesh::Manifold* m_ptr, int max_iter, bool anneal, bool alpha, double gamma) {
    minimize_dihedral_angle(*m_ptr, max_iter, anneal, alpha, gamma);
}


void maximize_min_angle(HMesh::Manifold* m_ptr, float thresh, bool anneal) {
    maximize_min_angle(*m_ptr, thresh, anneal);
}

void optimize_valency(HMesh::Manifold* m_ptr, bool anneal) {
    optimize_valency(*m_ptr, anneal);
}

void randomize_mesh(HMesh::Manifold* m_ptr, int max_iter) {
    randomize_mesh(*m_ptr, max_iter);
}

void quadric_simplify(HMesh::Manifold* m_ptr, double keep_fraction,
                      double singular_thresh,
                      bool choose_optimal_positions) {
    quadric_simplify(*m_ptr, keep_fraction, singular_thresh, choose_optimal_positions);
}

float average_edge_length(const HMesh::Manifold* m_ptr) {
    return average_edge_length(*m_ptr);
}

float median_edge_length(const HMesh::Manifold* m_ptr) {
    return median_edge_length(*m_ptr);
}

int refine_edges(HMesh::Manifold* m_ptr, float t) {
    return refine_edges(*m_ptr, t);
}

void cc_split(HMesh::Manifold* m_ptr) {
    cc_split(*m_ptr, *m_ptr);
}

void loop_split(HMesh::Manifold* m_ptr) {
    loop_split(*m_ptr, *m_ptr);
}

void root3_subdivide(HMesh::Manifold* m_ptr) {
    root3_subdivide(*m_ptr, *m_ptr);
}

void rootCC_subdivide(HMesh::Manifold* m_ptr) {
    rootCC_subdivide(*m_ptr, *m_ptr);
}

void butterfly_subdivide(HMesh::Manifold* m_ptr) {
    butterfly_subdivide(*m_ptr, *m_ptr);
}

void cc_smooth(HMesh::Manifold* m_ptr) {
    cc_smooth(*m_ptr);
}

void loop_smooth(HMesh::Manifold* m_ptr) {
    loop_smooth(*m_ptr);
}

void shortest_edge_triangulate(HMesh::Manifold* m_ptr) {
    shortest_edge_triangulate(m_ptr);
}

void extrude_faces(Manifold & m, size_t face_number, int* face_ids) {
	FaceSet faceSet;
	for (int i = 0; i < face_number; i++) {
		FaceID f = FaceID(face_ids[i]);
		faceSet.insert(f);
	}
	extrude_face_set(m, faceSet);
}

void extrude_face_set(Manifold& m, const FaceSet& face_set)
{
	HalfEdgeSet hset;

	for (auto f : face_set)
		circulate_face_ccw(m, f, [&](Walker& w) {
			if (face_set.find(w.opp().face()) == face_set.end()) {
				hset.insert(w.halfedge());
			}
		});

	extrude_along_edge_loop(m, hset);
}

void extrude_along_edge_loop(Manifold& m, const HalfEdgeSet& hset)
{

	vector<pair<HalfEdgeID, HalfEdgeID>> h_pairs;

	for (auto h : hset) {
		auto w = m.walker(h);
		h_pairs.push_back(make_pair(w.halfedge(), w.opp().halfedge()));
	}

	// Slit edges
	for (auto h_in : hset) {
		Walker w_in = m.walker(h_in);
		VertexID v = w_in.vertex();
		for (auto h_out : hset) {
			Walker w_out = m.walker(h_out);
			if (w_out.opp().vertex() == v) {
				VertexID new_v = m.slit_vertex(v, h_in, h_out);
				m.pos(new_v) = m.pos(v);
				break;
			}
		}
	}

	VertexAttributeVector<int> cluster_id(m.allocated_vertices(), -1);
	int CLUSTER_CNT = 0;
	auto assign_cluster_id = [&](VertexID v) {
		if (cluster_id[v] == -1) {
			CLUSTER_CNT++;
			cluster_id[v] = CLUSTER_CNT;
		}
	};


	FaceSet new_faces;
	// Make new faces
	for (auto h_pair : h_pairs)
	{
		Walker w1 = m.walker(h_pair.first);
		Walker w2 = m.walker(h_pair.second);
		assign_cluster_id(w1.vertex());
		assign_cluster_id(w1.opp().vertex());
		assign_cluster_id(w2.vertex());
		assign_cluster_id(w2.opp().vertex());

		vector<CGLA::Vec3d> pts(4);
		pts[0] = m.pos(w1.vertex());
		pts[1] = m.pos(w1.opp().vertex());
		pts[2] = m.pos(w2.vertex());
		pts[3] = m.pos(w2.opp().vertex());



		FaceID f = m.add_face(pts);
		new_faces.insert(f);
		Walker w = m.walker(f);

		//file << "Vertex " << w.vertex().get_index() << " CLUSTER BEFORE ASSIGN: " << cluster_id[w.vertex()] << " REFERENCE before first assign: " << &cluster_id[w.vertex()] << " POSITION " << m.pos(w.vertex()) << endl;
		cluster_id[w.vertex()] = cluster_id[w1.vertex()];
		cluster_id[w.vertex()] = cluster_id[w1.vertex()];
		w = w.next();
		//file << "Vertex " << w.vertex().get_index() << " CLUSTER BEFORE ASSIGN: " << cluster_id[w.vertex()] << " REFERENCE before first assign: " << &cluster_id[w.vertex()] << " POSITION " << m.pos(w.vertex()) << endl;
		cluster_id[w.vertex()] = cluster_id[w1.opp().vertex()];
		cluster_id[w.vertex()] = cluster_id[w1.opp().vertex()];
		w = w.next();
		//file << "Vertex " << w.vertex().get_index() << " CLUSTER BEFORE ASSIGN: " << cluster_id[w.vertex()] << " REFERENCE before first assign: " << &cluster_id[w.vertex()] << " POSITION " << m.pos(w.vertex()) << endl;
		cluster_id[w.vertex()] = cluster_id[w2.vertex()];
		cluster_id[w.vertex()] = cluster_id[w2.vertex()];
		w = w.next();
		//file << "Vertex " << w.vertex().get_index() << " CLUSTER BEFORE ASSIGN: " << cluster_id[w.vertex()] << " REFERENCE before first assign: " << &cluster_id[w.vertex()] << " POSITION " << m.pos(w.vertex()) << endl;
		cluster_id[w.vertex()] = cluster_id[w2.opp().vertex()];
		cluster_id[w.vertex()] = cluster_id[w2.opp().vertex()];

	}

	// Stitch
	stitch_mesh(m, cluster_id);
	
	//return new_faces;
}


void triangulate(HMesh::Manifold* manifold) {
	shortest_edge_triangulate(manifold);
	//triangulate_by_vertex_face_split(manifold);
}

void to_idfs(HMesh::Manifold& m, double* points, int32_t* polygons) {
	vector<CGLA::Vec3d> pts;
	VertexAttributeVector<int32_t> vertex_indices;
	int i = 0;
	for (auto v : m.vertices()) {
		pts.push_back(m.pos(v));
		vertex_indices[v] = i;
		++i;
	}

	i = 0; 
	for (auto& p : pts) { // insert x,y,z into points
		points[3*i] = p[0];
		points[3*i + 1] = p[1];
		points[3*i + 2] = p[2];
		i++;
	}

	vector<CGLA::Vec3i> faces;
	for (auto f : m.faces()) {
		int32_t* polyCount = polygons;
		polygons++;
		int j = 0;
		circulate_face_ccw(m, f, [&](VertexID v) {
			*polygons = vertex_indices[v];
			polygons++;
			j++;
		});
		*polyCount = j;
	}
}

void move_face_along_vector(HMesh::Manifold& m, int face_id, double* direction) {
	auto f = FaceID(face_id);
	circulate_face_ccw(m, f, [&](VertexID v) {
		auto old_pos = m.pos(v);
		auto new_pos = CGLA::Vec3d();
		new_pos[0] = old_pos[0] + direction[0];
		new_pos[1] = old_pos[1] + direction[1];
		new_pos[2] = old_pos[2] + direction[2];
		m.pos(v) = new_pos;
	});
}

void rotate_faces_around_point(HMesh::Manifold& m, size_t number_of_faces, int* face_ids, double* point, double* rotation) {
	using namespace CGLA;
	Mat4x4d transform = translation_Mat4x4d({ point[0], point[1], point[2] })
		* Quatd(rotation[0], rotation[1], rotation[2], rotation[3]).get_Mat4x4d()
		* translation_Mat4x4d({ -point[0], -point[1], -point[2] });
	vector<VertexID> passed;
	for (int i = 0; i < number_of_faces; i++) {
		auto f = FaceID(face_ids[i]);
		circulate_face_ccw(m, f, [&](VertexID v) {
			bool is_passed = false;
			for (auto p : passed) {
				if (is_passed = p == v) break;
			}
			if (!is_passed) {
				passed.push_back(v);
				auto old_pos = CGLA::Vec4d(m.pos(v), 1.0);
				auto new_pos = transform * old_pos;
				m.pos(v) = CGLA::Vec3d(new_pos[0], new_pos[1], new_pos[2]);
			}
		});
	}
}

void move_faces_along_vector(HMesh::Manifold& m, size_t number_of_faces, int* face_ids, double* direction) {
	vector<VertexID> passed;
	for (int i = 0; i < number_of_faces; i++) {
		auto f = FaceID(face_ids[i]);
		circulate_face_ccw(m, f, [&](VertexID v) {
			bool is_passed = false;
			for (auto p : passed) {
				if (is_passed = p == v) break;
			}
			if (!is_passed) {
				passed.push_back(v);
				auto old_pos = m.pos(v);
				auto new_pos = CGLA::Vec3d();
				new_pos[0] = old_pos[0] + direction[0];
				new_pos[1] = old_pos[1] + direction[1];
				new_pos[2] = old_pos[2] + direction[2];
				m.pos(v) = new_pos;
			}
		});
	}
}


void move_vertex_along_vector(HMesh::Manifold& m, int vertex_id, double* direction) {
	auto v = VertexID(vertex_id);
	auto old_pos = m.pos(v);
	auto new_pos = CGLA::Vec3d();
	new_pos[0] = old_pos[0] + direction[0];
	new_pos[1] = old_pos[1] + direction[1];
	new_pos[2] = old_pos[2] + direction[2];
	m.pos(v) = new_pos;
}

void move_faces_along_normal(HMesh::Manifold& m, size_t number_of_faces, int* face_ids, double magnitude) {
	for (int i = 0; i < number_of_faces; i++) {
		auto f = FaceID(face_ids[i]);
		auto n = normal(m, f);
		n[0] *= magnitude;
		n[1] *= magnitude;
		n[2] *= magnitude;

		circulate_face_ccw(m, f, [&](VertexID v) {
			auto old_pos = m.pos(v);
			auto new_pos = CGLA::Vec3d();
			new_pos[0] = old_pos[0] + n[0];
			new_pos[1] = old_pos[1] + n[1];
			new_pos[2] = old_pos[2] + n[2];
			m.pos(v) = new_pos;
		});
	}
}

void center(Manifold& m_ptr, int _f, double* c) {
	auto face = FaceID(_f);
	int i = 0;
	int n = circulate_face_ccw(m_ptr, face, ([&](VertexID v) {
		auto v_pos = m_ptr.pos(v);
		c[0] += v_pos[0];
		c[1] += v_pos[1];
		c[2] += v_pos[2];
	}));

	c[0] /= n;
	c[1] /= n;
	c[2] /= n;
}

// If quad find the center on the triangulated diagonal edge (between vertex 0 and 2).
// This assumes that the triangualtion is performed naively [0,1,2][0,2,3] etc)
void center_triangulated(HMesh::Manifold& m, int _f, double* c) {
	auto face = FaceID(_f);
	size_t edges = no_edges(m, face);
	int i = 0;
	int n = circulate_face_ccw(m, face, ([&](VertexID v) {
		if (!(edges >= 4 && (i == 1 || i == 3))){
			auto v_pos = m.pos(v);
			c[0] += v_pos[0];
			c[1] += v_pos[1];
			c[2] += v_pos[2];
		}
		i++;
	}));
	if (edges >= 4) {
		n = 2;
	}
	if (edges )
	c[0] /= n;
	c[1] /= n;
	c[2] /= n;
}

void get_first_edge_direction(HMesh::Manifold& m, int _f, double* res) {
	auto face = FaceID(_f);
	int i = 0;
	auto faceWalker = m.walker(face);
	auto v0 = m.pos(faceWalker.vertex());
	faceWalker = faceWalker.circulate_face_ccw();
	auto v1 = m.pos(faceWalker.vertex());
	
	for (int i=0;i<3;i++){
		res[i] = v1[i] - v0[i];
	}
}

void get_neighbouring_faces_and_edge(HMesh::Manifold& m, int face_id, int* neighbour_faces, double* edge_centers, double* edge_direction, int32_t* edge_id) {
	int i = 0;
	circulate_face_ccw(m, FaceID(face_id), [&](HalfEdgeID h) {
		auto w = m.walker(h);
		auto v1 = m.pos(w.vertex());
		auto v2 = m.pos(w.opp().vertex());
		auto opp_f = w.opp().face();
		edge_id[i] = w.halfedge().get_index();
		neighbour_faces[i]	= opp_f.get_index();
		edge_centers[3*i]	= (v1[0] + v2[0]) / 2;
		edge_centers[3*i+1] = (v1[1] + v2[1]) / 2;
		edge_centers[3*i+2] = (v1[2] + v2[2]) / 2;
		edge_direction[3 * i]     = (v1[0] - v2[0]);
		edge_direction[3 * i + 1] = (v1[1] - v2[1]);
		edge_direction[3 * i + 2] = (v1[2] - v2[2]);
		i++;
	});
	
	
}

void log_manifold(HMesh::Manifold& m, char* file_name) {
	vector<CGLA::Vec3d> pts;
	VertexAttributeVector<int> vertex_indices;
	vector<bool> is_used;
	int i = 0;
	for (auto v : m.vertices()) {
		pts.push_back(m.pos(v));
		vertex_indices[v] = i;
		is_used.push_back(m.in_use(v));
		++i;
	}

	vector<CGLA::Vec3i> triangles;
	for (auto f : m.faces()) {
		f = FaceID(f.get_index());
		CGLA::Vec3i face;
		int j = 0;
		circulate_face_ccw(m, f, [&](VertexID v) {
			face[j++] = vertex_indices[v];
		});
		triangles.push_back(face);
	}

}

void log_manifold_original(HMesh::Manifold& m, char* file_name) {
	vector<CGLA::Vec3d> pts;
	VertexAttributeVector<int> vertex_indices;
	vector<bool> is_used;
	int i = 0;
	for (auto v : m.vertices()) {
		pts.push_back(m.pos(v));
		vertex_indices[v] = i;
		is_used.push_back(m.in_use(v));
		++i;
	}

	vector<vector<int>> triangles;
	for (auto f : m.faces()) {
		f = FaceID(f.get_index());
		vector<int> face;
		int j = 0;
		circulate_face_ccw(m, f, [&](VertexID v) {
			face.push_back(vertex_indices[v]);
			//face[j++] = vertex_indices[v];
		});
		triangles.push_back(face);
	}

}

void test_valery(HMesh::Manifold& m) {
	vector<CGLA::Vec3d> pts;
	VertexAttributeVector<int> vertex_indices;
	int i = 0;
	for (auto v : m.vertices()) {
		pts.push_back(m.pos(v));
		vertex_indices[v] = i;
		++i;
	}

	vector<CGLA::Vec3i> triangles;
	for (auto f : m.faces()) {
		f = FaceID(f.get_index());
		CGLA::Vec3i face;
		int j = 0;
		circulate_face_ccw(m, f, [&](VertexID v) {
			face[j++] = vertex_indices[v];
		});
		triangles.push_back(face);
	}

}

void test_extrude_and_move(HMesh::Manifold& m) {
	int face_id = 0;
	int* faces = new int[1];
	faces[0] = face_id;
	FaceSet faceSet;
	for (auto f : m.faces()) {
		faceSet.insert(f);
		break;
	}
	//faceSet.insert(m.faces()[0])
	//extrude_face_set(m, faceSet);
	extrude_faces(m, 1, faces);
	double* direction = new double[3];
	direction[0] = -0.5;
	direction[1] = 0;
	direction[2] = -0.5;
	move_face_along_vector(m, face_id, direction);
}

int32_t collapse_short_edges(HMesh::Manifold& m, double threshold, int32_t* movingVertices, int32_t movingVerticesCount) {
	std::vector<HalfEdgeID> toCollapse;
	for (HalfEdgeID h : m.halfedges()) {
		if (length(m, h) < threshold) {
			toCollapse.push_back(h);
		}
	}
	int32_t count = 0;
	for (HalfEdgeID id : toCollapse) {
		if (m.in_use(id)) {
			if (precond_collapse_edge(m, id)){
				m.collapse_edge(id, true);
				count++;
			}
		}
	}
	return count;
}

void get_hmesh_ids(HMesh::Manifold& m, int32_t* vertex_ids, int32_t* halfedge_ids, int32_t* face_ids) {
	for (VertexID id : m.vertices()) {
		*vertex_ids = id.get_index();
		vertex_ids++;
	}

	for (HalfEdgeID id : m.halfedges()) {
		*halfedge_ids = id.get_index();
		halfedge_ids++;
	}

	for (FaceID id : m.faces()) {
		*face_ids = id.get_index();
		face_ids++;
	}
}


