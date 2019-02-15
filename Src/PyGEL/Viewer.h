//
//  Viewer.hpp
//  PyGEL
//
//  Created by Jakob Andreas Bærentzen on 06/10/2017.
//  Copyright © 2017 Jakob Andreas Bærentzen. All rights reserved.
//

#ifndef Viewer_hpp
#define Viewer_hpp

#if defined(__APPLE__) || defined(__linux__)
#define DLLEXPORT __attribute__ ((visibility ("default")))
#else
#define DLLEXPORT __declspec(dllexport)
#endif

#include <GEL/HMesh/Manifold.h>

class GLManifoldViewer {
    GLFWwindow* window = 0;
    std::vector<CGLA::Vec3d> annotation_points;
    bool active_annotation = false;
    bool do_pick = false;
    bool mouse_down = false;
    GLGraphics::GLViewController* glv = 0;
    GLGraphics::ManifoldRenderer* renderer = 0;
    bool escaping = false;

public:
    GLManifoldViewer();
    ~GLManifoldViewer();
    
    bool was_initialized() const {return glv != 0;}
    
    void display_init(HMesh::Manifold& m,
                 char mode,
                 bool smooth_shading,
                 CGLA::Vec3f* bg_color,                 
                 double* attrib_vec,
                 bool reset_view);
    
    void display();
    
    CGLA::Vec2i mouse_pos;

    void roll_ball() {
        if(mouse_down)
            glv->roll_ball(mouse_pos);
    }
    void grab_ball(GLGraphics::TrackBallAction tba) {
        glv->grab_ball(tba, mouse_pos);
        mouse_down = true;
    }
    void release_ball() {
        glv->release_ball();
        mouse_down = false;
    }
    
    void set_picking_true() {
        do_pick = true;
    }
    
    void set_escaping_true() {
        escaping = true;
        
    }
    
    bool get_escaping() {
        if(escaping) {
            escaping = false;
            return true;
        }
        return false;
    }
    
    void clear_annotation() {
        annotation_points.clear();
        active_annotation = false;
    }
    
    std::vector<CGLA::Vec3d>& get_annotation_points() {
        return annotation_points;
    }
    
    void set_annotation_points(const std::vector<CGLA::Vec3d>& pts) {
        active_annotation = pts.size()>0 ? true : false;
        annotation_points = pts;
    }

};

extern "C" {
    DLLEXPORT GLManifoldViewer* GLManifoldViewer_new();
    
    DLLEXPORT void GLManifoldViewer_event_loop(bool once);
    
    DLLEXPORT void GLManifoldViewer_display(GLManifoldViewer* self,
                                  HMesh::Manifold* m,
                                  char mode,
                                  bool smooth_shading,
                                  CGLA::Vec3f* bg_color,
                                  double* attrib_vec,
                                  bool reset_view,
                                  bool once);
    
    DLLEXPORT void GLManifoldViewer_delete(GLManifoldViewer*);
    
    DLLEXPORT size_t GLManifoldViewer_get_annotation_points(GLManifoldViewer* self, double** data);

    DLLEXPORT void GLManifoldViewer_set_annotation_points(GLManifoldViewer* self, int n, double* data);

}

#endif /* Viewer_hpp */
