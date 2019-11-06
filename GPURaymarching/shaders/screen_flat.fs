#version 410 core
in vec2 fragPos;
out vec4 FragColor;

uniform float time;

const float EPSILON = 0.0001;


float circle_distance(in vec2 p, in vec2 center, float radius) {
    return distance(p, center) - radius;
}

float point_distance(in vec2 p, in vec2 pos) {
    return distance(p, pos);
}

float combine(float dist1, float dist2) {
    return min(dist1, dist2);
}

float flat_distance(in vec2 p) {
    float anim_time = time*0.05;
    float d1 = point_distance(p, vec2(0.2-cos(1*anim_time)/2, 0.8+sin(anim_time*2.3)/2));
    float d2 = point_distance(p, vec2(0.7-cos(2*anim_time)/2, 0.4+sin(anim_time*1.3)/2));
    float d3 = point_distance(p, vec2(0.4-cos(3*anim_time)/2, 0.6+sin(anim_time*1.8)/2));
    float d4 = point_distance(p, vec2(0.5-cos(1.4*anim_time)/2, 0.6+sin(anim_time*0.9)/2));
    float d5 = point_distance(p, vec2(0.3-cos(3.14*anim_time)/2, 0.9+sin(anim_time*0.8)/2));
    float d6 = point_distance(p, vec2(0.5-cos(2.56*anim_time)/2, 0.1+sin(anim_time*1.3)/2));
    float d7 = point_distance(p, vec2(0.4-cos(2.1*anim_time)/2, 0.3+sin(anim_time*1.7)/2));
    float d8 = point_distance(p, vec2(0.8-cos(1.8*anim_time)/2, 0.5+sin(anim_time)/2));
    return combine(combine(combine(d1, d2), combine(d3, d4)), combine(combine(d5, d6), combine(d7, d8)));
}

vec3 gradient(in vec2 p) {
    float x = (flat_distance(vec2(p.x+EPSILON, p.y))-flat_distance(vec2(p.x-EPSILON, p.y)))/(2.0*EPSILON);
    float y = (flat_distance(vec2(p.x, p.y+EPSILON))-flat_distance(vec2(p.x, p.y-EPSILON)))/(2.0*EPSILON);
    return vec3(x, y, 0);
}

void main() {
    vec3 col = (vec3(1)+normalize(gradient(vec2(fragPos.x, fragPos.y))))/2;
    
    FragColor = vec4(col, 1.0);
}
