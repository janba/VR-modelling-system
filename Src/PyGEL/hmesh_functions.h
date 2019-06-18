//
//  hmesh_functions.hpp
//  PyGEL
//
//  Created by Jakob Andreas Bærentzen on 04/11/2017.
//  Copyright © 2017 Jakob Andreas Bærentzen. All rights reserved.
//

#ifndef hmesh_functions_hpp
#define hmesh_functions_hpp

#if defined(__APPLE__) || defined(__linux__)
#define DLLEXPORT __attribute__ ((visibility ("default")))
#else
#define DLLEXPORT __declspec(dllexport)
#endif

#include <GEL\HMesh\HMesh.h>

extern "C" {
    DLLEXPORT void stitch_mesh(HMesh::Manifold* m_ptr, double rad);

    DLLEXPORT bool valid(const HMesh::Manifold* m_ptr);
    DLLEXPORT bool closed(const HMesh::Manifold* m_ptr);

    DLLEXPORT void bbox(const HMesh::Manifold* m_ptr, CGLA::Vec3d* pmin, CGLA::Vec3d* pmax);
    DLLEXPORT void bsphere(const HMesh::Manifold* m_ptr, CGLA::Vec3d* c, double* r);

    DLLEXPORT bool obj_load(char*, HMesh::Manifold*);
    DLLEXPORT bool off_load(char*, HMesh::Manifold* m_ptr);
    DLLEXPORT bool ply_load(char*, HMesh::Manifold* m_ptr);
    DLLEXPORT bool x3d_load(char*, HMesh::Manifold* m_ptr);

    DLLEXPORT bool obj_save(char*, HMesh::Manifold* m_ptr);
    DLLEXPORT bool off_save(char*, HMesh::Manifold* m_ptr);
    DLLEXPORT bool x3d_save(char*, HMesh::Manifold* m_ptr);


    DLLEXPORT void remove_caps(HMesh::Manifold* m_ptr, float thresh);

    DLLEXPORT void remove_needles(HMesh::Manifold* m_ptr, float thresh, bool averagePositions = false);

    DLLEXPORT void close_holes(HMesh::Manifold* m_ptr);

    DLLEXPORT void flip_orientation(HMesh::Manifold* m_ptr);

    DLLEXPORT void minimize_curvature(HMesh::Manifold* m_ptr, bool anneal=false);

    DLLEXPORT void minimize_dihedral_angle(HMesh::Manifold* m_ptr, int max_iter=10000, bool anneal=false, bool alpha=false, double gamma=4.0);

    DLLEXPORT void maximize_min_angle(HMesh::Manifold* m_ptr, float thresh, bool anneal=false);

    DLLEXPORT void optimize_valency(HMesh::Manifold* m_ptr, bool anneal=false);

    DLLEXPORT void randomize_mesh(HMesh::Manifold* m_ptr, int max_iter);

    DLLEXPORT void quadric_simplify(HMesh::Manifold* m_ptr, double keep_fraction, double singular_thresh = 0.0001, bool choose_optimal_positions = true);

    DLLEXPORT float average_edge_length(const HMesh::Manifold* m_ptr);

    DLLEXPORT float median_edge_length(const HMesh::Manifold* m_ptr);

    DLLEXPORT int refine_edges(HMesh::Manifold* m_ptr, float t);

    DLLEXPORT void cc_split(HMesh::Manifold* m_ptr);

    DLLEXPORT void loop_split(HMesh::Manifold* m_ptr);

    DLLEXPORT void root3_subdivide(HMesh::Manifold* m_ptr);

    DLLEXPORT void rootCC_subdivide(HMesh::Manifold* m_ptr);

    DLLEXPORT void butterfly_subdivide(HMesh::Manifold* m_ptr);

    DLLEXPORT void cc_smooth(HMesh::Manifold* m_ptr);

    DLLEXPORT void loop_smooth(HMesh::Manifold* m_ptr);

    DLLEXPORT void shortest_edge_triangulate(HMesh::Manifold* m_ptr);

    DLLEXPORT void test_valery(HMesh::Manifold& m_ptr);

	DLLEXPORT void extrude_faces(HMesh::Manifold& m, size_t face_number, int* face_ids);

	DLLEXPORT void extrude_face_set(HMesh::Manifold& m, const HMesh::FaceSet& face_set);

	DLLEXPORT void extrude_along_edge_loop(HMesh::Manifold& m, const HMesh::HalfEdgeSet& hset);

	DLLEXPORT void triangulate(HMesh::Manifold& m);

	DLLEXPORT void to_idfs(HMesh::Manifold& m, double* points, int32_t* triangles);

	DLLEXPORT void move_vertex_along_vector(HMesh::Manifold& m, int vertex_id, double* direction);

	DLLEXPORT void move_face_along_vector(HMesh::Manifold& m, int face_id, double* direction);

	DLLEXPORT void move_faces_along_vector(HMesh::Manifold& m, size_t number_of_faces, int* face_ids, double* direction);

	DLLEXPORT void move_faces_along_normal(HMesh::Manifold& m, size_t number_of_faces, int* face_ids, double magnitude);

	DLLEXPORT void rotate_faces_around_point(HMesh::Manifold& m, size_t number_of_faces, int* face_ids, double* point, double* rotation);

	DLLEXPORT void log_manifold(HMesh::Manifold& m, char* file_name);

	DLLEXPORT void center(HMesh::Manifold& m, int face_id, double* center);

	DLLEXPORT void center_triangulated(HMesh::Manifold& m, int face_id, double* center);

	DLLEXPORT void get_first_edge_direction(HMesh::Manifold& m, int face_id, double* res);

	DLLEXPORT void get_neighbouring_faces_and_edge(HMesh::Manifold& m, int face_id, int* neighbour_faces, double* edge_centers, double* edge_direction, int32_t* edge_id);

	DLLEXPORT void test_extrude_and_move(HMesh::Manifold& m);

	DLLEXPORT void log_manifold_original(HMesh::Manifold& m, char* file_name);

	DLLEXPORT int32_t collapse_short_edges(HMesh::Manifold& m, double threshold, int32_t* movingVertices, int32_t movingVerticesCount);

	DLLEXPORT void get_hmesh_ids(HMesh::Manifold& m, int32_t* vertex_ids, int32_t* halfedge_ids, int32_t* face_ids);
}

#endif /* hmesh_functions_hpp */
