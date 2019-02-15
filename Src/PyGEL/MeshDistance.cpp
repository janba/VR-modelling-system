//
//  MeshDistance.cpp
//  PyGEL
//
//  Created by Jakob Andreas Bærentzen on 13/12/2017.
//  Copyright © 2017 Jakob Andreas Bærentzen. All rights reserved.
//

#include <GEL/CGLA/CGLA.h>
#include "MeshDistance.h"
using namespace HMesh;
using namespace CGLA;
using namespace Geometry;

MeshDistance::MeshDistance(Manifold* m) {
    build_AABBTree(*m, aabb_tree);
}

float MeshDistance::signed_distance(const CGLA::Vec3f& p, float upper){
    
    return aabb_tree.compute_signed_distance(p);
    
}
bool MeshDistance::ray_inside_test(const CGLA::Vec3f& p, int no_rays) {
    auto rand_vec = []() {return Vec3f(gel_rand()/double(GEL_RAND_MAX),
                                       gel_rand()/double(GEL_RAND_MAX),
                                       gel_rand()/double(GEL_RAND_MAX));
    };
    
    int even=0;
    int odd=0;
    for (int i=0;i<no_rays;++i) {
        int cnt = aabb_tree.intersect_cnt(p, rand_vec());
        if(cnt % 2 == 0)
            ++even;
        else
            ++odd;
    }
    return odd > even;
}

MeshDistance* MeshDistance_new(HMesh::Manifold* m) {
    return new MeshDistance(m);
}

void MeshDistance_delete(MeshDistance* self) {
    delete self;
}

float MeshDistance_signed_distance(MeshDistance* self,
                      const CGLA::Vec3f* p,
                      float upper) {
    return self->signed_distance(*p, upper);
}

 bool MeshDistance_ray_inside_test(MeshDistance* self,
                                            const CGLA::Vec3f* p,
                                   int no_rays) {
     return self->ray_inside_test(*p, no_rays);
 }

