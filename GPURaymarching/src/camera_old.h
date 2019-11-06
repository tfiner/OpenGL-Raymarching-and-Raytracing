//
//  camera.h
//  GPURaymarching
//
//  Created by Antoni Wójcik on 17/09/2019.
//

#ifndef camera_h
#define camera_h

#include "shader.h"

const float FOV = 90.0f;

enum CameraMovement {
    FORWARD,
    BACK,
    LEFT,
    RIGHT
};

class Camera {
private:
    float half_height;
    float half_width;
    
    glm::vec3 origin, horizontal, vertical, lower_left_corner;
    glm::vec3 u, v, w;
public:
    Camera() {}
    Camera(const glm::vec3& look_from, const glm::vec3& look_at, const glm::vec3& up, float aspect) {
        float angle = FOV*M_PI/180.0f;
        half_height = tan(angle*0.5f);
        half_width = aspect * half_height;
        origin = look_from;
        w = normalize(look_at - look_from);
        u = normalize(-cross(w, up));
        v = cross(u, w);
        lower_left_corner = w-(half_width*u + half_height*v);
        horizontal = 2.0f*half_width*u;
        vertical = 2.0f*half_height*v;
    }
    void update(const glm::vec3& look_from, const glm::vec3& look_at, const glm::vec3& up) {
        origin = look_from;
        w = normalize(look_at - look_from);
        u = normalize(-cross(w, up)); 
        v = cross(u, w);
        lower_left_corner = w-(half_width*u + half_height*v);
        horizontal = 2.0f*half_width*u;
        vertical = 2.0f*half_height*v;
    }
    void set_size(float aspect) {
        float angle = FOV*M_PI/180.0f;
        half_height = tan(angle*0.5f);
        half_width = aspect * half_height;
    }
    void transfer_data(Shader& shader) {
        shader.setVec3("origin", origin);
        shader.setVec3("camera_llc", lower_left_corner);
        shader.setVec3("horizontal", horizontal);
        shader.setVec3("vertical", vertical);
    }
    void process_mouse_input(float x, float y) {
        
    }
};

#endif /* camera_h */
