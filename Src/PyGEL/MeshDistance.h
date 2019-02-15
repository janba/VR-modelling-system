//
//  MeshDistance.hpp
//  PyGEL
//
//  Created by Jakob Andreas Bærentzen on 13/12/2017.
//  Copyright © 2017 Jakob Andreas Bærentzen. All rights reserved.
//

#ifndef MeshDistance_hpp
#define MeshDistance_hpp

#if defined(__APPLE__) || defined(__linux__)
#define DLLEXPORT __attribute__ ((visibility ("default")))
#else
#define DLLEXPORT __declspec(dllexport)
#endif

#include <GEL/HMesh/Manifold.h>
#include <GEL/Geometry/build_bbtree.h>

class MeshDistance {
    Geometry::AABBTree aabb_tree;
public:
    MeshDistance(HMesh::Manifold* m);
    
    float signed_distance(const CGLA::Vec3f& p, float upper);
    bool ray_inside_test(const CGLA::Vec3f& p, int no_rays);
};

extern "C" {
    DLLEXPORT MeshDistance* MeshDistance_new(HMesh::Manifold* m);
    DLLEXPORT void MeshDistance_delete(MeshDistance*);
    
    DLLEXPORT float MeshDistance_signed_distance(MeshDistance* self,
                                                 const CGLA::Vec3f* p,
                                                 float upper);

    DLLEXPORT bool MeshDistance_ray_inside_test(MeshDistance* self,
                                                 const CGLA::Vec3f* p,
                                                 int no_rays);

    
}

#endif /* MeshDistance_hpp */
