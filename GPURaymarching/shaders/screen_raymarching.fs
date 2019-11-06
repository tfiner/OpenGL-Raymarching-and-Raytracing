#version 410 core
in vec2 fragPos;
out vec4 FragColor;

uniform vec3 origin;
uniform vec3 camera_llc; // camera's lower left corner position
uniform vec3 horizontal;
uniform vec3 vertical;

uniform float time;


//###### COMPUTATION CONSTANTS #######

const float HIT_DISTANCE = 0.001;
const float MAX_DISTANCE = 300.0;
const int MAX_ITERATIONS = 1000;
const float EPSILON = 0.0001;

const float AMBIENT_INTENSITY = 0.1;
const int SPECULAR_INTENSITY = 32;
const int REFLECTIONS_DEPTH = 20; // 0 = reflections off
const float REFLECTION_INTENSITY = 0.5; //0 - min (reflections have no effect); 1 - max
const int HARD_SHADOW_SHARPNESS = 10; // the higher the less rips there will be on the shadows
const float FOG_STRENGTH = 0.02;

//####### FUNCTION DECLARATIONS #######

vec3 gradient(in vec3);
float sphere_distance(in vec3, in vec3, float);
float cuboid_distance(in vec3, in vec3, float);
float simple_repeat_distance(in vec3, in vec3, float, float);
float sierpinski_distance(in vec3, int);
float mandelbulb(in vec3);

//!!!!!!! THE ONLY FUNCTION THAT SHOULD BE CHANGED !!!!!!!
float scene_distance(in vec3 p) {
    return min(sphere_distance(p, vec3(0, 2 + sin(time*1.32), 1), 0.5), min(sphere_distance(p, vec3(0, 0, 2 + sin(time*1.32)), 0.5), sphere_distance(p, vec3(0, 0, 0), 0.5)));
    //return simple_repeat_distance(p, vec3(0), 0.5, 5);
    //p.xy = mod((p.xy),1.0)-vec2(0.5); // instance on xy-plane
    //return length(p)-0.3;
    //return mandelbulb(p);
    //return sierpinski_distance(p, 8);
}

//####### RAY OPERATIONS #######

//using Ray structs will allow for shadows and reflections
struct Ray {
    vec3 start; //starting location
    vec3 dir; //has to be normalized
    float parameter; //has to be set to 0 when initalized
    float previous_distance; //the distance the origin of the ray has travelled from the camera (includes reflections), used in the fog calculations
};

vec3 current_ray_point(in Ray r) {
    return r.start + r.dir*r.parameter;
}

Ray gen_initial_ray(in vec3 start, float s, float t) {
    Ray r;
    r.start = start;
    r.dir = normalize(camera_llc + s*horizontal + t*vertical);
    r.parameter = 0;
    r.previous_distance = 0;
    return r;
}

Ray gen_reflected_ray(in Ray r) { //the ray is assumed to be at a point of contact with the mirror surface
    Ray ray;
    ray.start = current_ray_point(r);
    ray.dir = reflect(r.dir, normalize(gradient(ray.start)));
    ray.parameter = HIT_DISTANCE * 2;
    ray.previous_distance = r.previous_distance+r.parameter;
    return ray;
}

bool check_collision(inout Ray r) {
    if(r.dir == vec3(0, 0, 0)) return false;
    float dist = MAX_DISTANCE;
    int i = 0;
    do {
        dist = scene_distance(current_ray_point(r));
        r.parameter += dist;
        i++;
    } while(i < MAX_ITERATIONS && dist > HIT_DISTANCE && r.parameter < MAX_DISTANCE);
    if(dist <= HIT_DISTANCE) return true;
    else return false;
}

//####### VECTOR OPERATIONS #######

//used to get a normal to the objects surface
vec3 gradient(in vec3 p) {
    float x = (scene_distance(vec3(p.x+EPSILON, p.y, p.z))-scene_distance(vec3(p.x-EPSILON, p.y, p.z)))/(2.0*EPSILON);
    float y = (scene_distance(vec3(p.x, p.y+EPSILON, p.z))-scene_distance(vec3(p.x, p.y-EPSILON, p.z)))/(2.0*EPSILON);
    float z = (scene_distance(vec3(p.x, p.y, p.z+EPSILON))-scene_distance(vec3(p.x, p.y, p.z-EPSILON)))/(2.0*EPSILON);
    return vec3(x, y, z);
}

//####### SIGNED DISTANCE FUNCTIONS #######

float floor_distance(in vec3 p, float height) {
    return p.y + height;
}

float sphere_distance(in vec3 p, in vec3 center, float radius) {
    return distance(p, center) - radius;
}

float cuboid_distance( vec3 p, vec3 dim) {
  vec3 shifted_p = abs(p) - dim;
  return length(max(shifted_p, 0.0)) + min(max(shifted_p.x,max(shifted_p.y, shifted_p.z)), 0.0);
}

//draws sphere of a given radius in a given separation at a position pos
float simple_repeat_distance(in vec3 p, in vec3 pos, float radius, float separation) {
    vec3 shifted_p = abs(p - pos);
    p = mod(p, separation) - 0.5*separation;
    
    return length(p) - radius;
}

float mandelbulb(in vec3 pos) {

    int Iterations = 20;
    float Bailout = 100;
    int Power = 3;
    vec3 z = pos;
    float dr = 1.0;
    float r = 0.0;
    for (int i = 0; i < Iterations ; i++) {
        r = length(z);
        if (r>Bailout) break;
        
        // convert to polar coordinates
        float theta = acos(z.z/r);
        float phi = atan(z.y,z.x);
        dr =  pow( r, Power-1.0)*Power*dr + 1.0;
        
        // scale and rotate the point
        float zr = pow( r,Power);
        theta = theta*Power;
        phi = phi*Power;
        
        // convert back to cartesian coordinates
        z = zr*vec3(sin(theta)*cos(phi), sin(phi)*sin(theta), cos(theta));
        z+=pos;
    }
    return 0.5*log(r)*r/dr;
}

// Code for this function taken from https://www.iquilezles.org/www/articles/menger/menger.htm
float sierpinski_distance(in vec3 p, int iterations) {
    float scale = 1;
    float result = cuboid_distance(p, vec3(scale));
    for(int i = 0; i < iterations; i++) {
       vec3 r = mod(p*scale, 2.0)-1.0;
       scale *= 3.0;
       r = abs(1.0 - 3.0*abs(r));

       float da = max(r.x, r.y);
       float db = max(r.y, r.z);
       float dc = max(r.z, r.x);

       result = max(result, (min(da, min(db,dc))-1.0)/scale);
    }
    return result;
}

//####### COLOR AND LIGHT FUNCTIONS #######

float fog(in Ray r) {
    if(FOG_STRENGTH == 0) return 1;
    else return exp(-(r.previous_distance+r.parameter)*FOG_STRENGTH);
}

float simple_light(in Ray r) {
    float shade = -dot(r.dir, normalize(gradient(current_ray_point(r))));
    shade = (shade < 0) ? 0.0 : shade;
    shade *= fog(r);
    return shade;
}

float light_soft(in Ray r, vec3 light_pos) {
    float ambient_strength = AMBIENT_INTENSITY;
    vec3 hit_point = current_ray_point(r);
    float diff_strength = max(dot(normalize(light_pos-hit_point), normalize(gradient(hit_point))), 0.0);
    return min(diff_strength + ambient_strength, 1.0);
}

float light_soft_specular(in Ray r, vec3 light_pos) {
    vec3 hit_point = current_ray_point(r);
    vec3 light_dir = normalize(light_pos-hit_point);
    vec3 normal = normalize(gradient(hit_point));
    
    float ambient_strength = AMBIENT_INTENSITY;
    float diff_strength = max(dot(light_dir, normal), 0.0);
    float specular_strength = pow(max(dot(light_dir, reflect(r.dir, normal)), 0.0), SPECULAR_INTENSITY);
    return min(diff_strength + ambient_strength + specular_strength, 1.0);
}

//this includes ambient light, diffuse light and specular light as well as hard shadows
float light_hard(in Ray r, vec3 light_pos) { //TODO: a better way to elliminate sharp edges
    vec3 hit_point = current_ray_point(r);
    vec3 light_dir = normalize(light_pos-hit_point);
    vec3 normal = normalize(gradient(hit_point));
    
    Ray shadow_ray;
    shadow_ray.start = light_pos;
    shadow_ray.dir = -light_dir;
    shadow_ray.parameter = 0;
    float point_to_light_dist = distance(hit_point, light_pos);
    bool in_light = check_collision(shadow_ray);
    bool reached_by_light = in_light && (shadow_ray.parameter < point_to_light_dist+HIT_DISTANCE*HARD_SHADOW_SHARPNESS && shadow_ray.parameter > point_to_light_dist-HIT_DISTANCE*HARD_SHADOW_SHARPNESS);
    
    float ambient_strength = AMBIENT_INTENSITY;
    float diff_strength = max(dot(light_dir, normal), 0.0);
    float specular_strength = pow(max(dot(light_dir, reflect(r.dir, normal)), 0.0), SPECULAR_INTENSITY);
    float result = min(ambient_strength + (reached_by_light ? (diff_strength + specular_strength) : 0), 1.0);
    
    return result;
}

float light_reflected(in Ray r, in vec3 light_pos) {
    float reflected_strength = fog(r)*light_hard(r, light_pos);
    Ray reflected = r;
    for(int i = 0; i < REFLECTIONS_DEPTH; i++) {
        reflected = gen_reflected_ray(reflected);
        if(check_collision(reflected)) reflected_strength = mix(reflected_strength, fog(reflected)*light_hard(reflected, light_pos), 0.5);
        else break;
    }
    return reflected_strength;
}

vec3 get_color(in Ray r) {
    vec3 col = vec3(1.0, 1.0, 1.0);
    bool hit = check_collision(r);
    if(hit) {
        vec3 light_pos = vec3(4, 0*sin(time)*10, 4);
        col *= light_reflected(r, light_pos);
        //col *= light_soft(r, light_pos);
        //col *= light_hard(r, light_pos);
        //col *= simple_light(r);
    } else {
        col *= 0;
    }
    return col;
}

//####### MAIN PROGRAM FUNCTION #######

void main() {
    vec3 col = get_color(gen_initial_ray(origin, fragPos.x, fragPos.y));
    
    FragColor = vec4(col, 1.0);
}
