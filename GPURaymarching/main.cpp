//
//  main.cpp
//  GPURaymarching
//
//  Created by Antoni Wójcik on 17/09/2019.
//
//  This is a simple OpenGL Ray Marching and Ray Tracing 3D graphics engine.
//  OpenGL 4.1 is used to handle the graphics. Window is created with GLFW and GLAD libraries; GLM library is used in the vector calculations as it is compatible with OpenGL. Look at the constants in "shaders/screen_raymarching.fs" to change drawing parameters and the constants in "src/camera.h" to change camera parameters. To change the displayed scene modify the code in the "scene_distance" function in "shaders/screen_raymarching.fs". The code in the other files should not be modified. If run on a computer without a retina display delete the RETINA macro below:
#define RETINA
//  To change the graphics from Ray Marching to Ray Tracing change the belowe macro from "false" to "true"
#define SWITCH_GRAPHICS false
//  In both cases, only one ray is generated per pixel to improve performance (this is why shadows in Ray Tracing case look so noisy)
//
//  TODO: add GUI to make changing the constants easier
//
//  The code was written with the help of the following tutorials and websites:
//  https://learnopengl.com
//  https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
//  The "shader.h" source code is taken directly from the https://learnopengl.com tutorial. More details in the file header.
//
//  Controls:
//  A,W,S,D       to move the camera
//  SHIFT, CTRL   to speed up/slow down camera
//  MOUSE         to rotate the camera
//  MOUSE SCROLL  to zoom the camera
//  MOUSE CLICK   to release/show and lock/hide cursor
//  ENTER         to take a screenshot - the result: "screenshot.tga" will appear in the screenshots folder
//
//  Last remark: THIS PROGRAM HAS BEEN TESTED ONLY ON MAC OS 10.14.6
//

// include OpenGL libraries
#include <glad/glad.h>
#include <GLFW/glfw3.h>
#include <glm/glm.hpp>

#include "shader.h"
#include "screen.h"
#include "camera.h"

#include <iostream>

//GLFW callbacks
void framebuffer_size_callback(GLFWwindow*, int, int);
void processInput(GLFWwindow*);
void mouse_callback(GLFWwindow*, double, double);
void mouse_button_callback(GLFWwindow*, int, int, int);
void scroll_callback(GLFWwindow*, double, double);

//function that provides a fix for Mac OS 10.14+ initial black screen
#ifdef __APPLE__
static bool macMoved = false;
void macWindowFix(GLFWwindow*);
#endif

//dimensions of the viewport (they have to be multiplied by 2 at the retina displays)
#ifndef RETINA
unsigned int SCR_WIDTH = 1280;
unsigned int SCR_HEIGHT = 720;
#else
unsigned int SCR_WIDTH = 1280*2;
unsigned int SCR_HEIGHT = 720*2;
#endif

//variables used in the main loop
float delta_time = 0.0f;
float last_frame_time = 0.0f;

//variables used in callbacks
bool first_mouse = true;
bool mouse_hidden = true;
float last_x = SCR_WIDTH / 2.0f;
float last_y = SCR_WIDTH / 2.0f;

//fps counter variables
float fps_sum = 0.0f;
int fps_steps = 10;
int fps_steps_counter = 0;

//screenshot variable
bool taking_screenshot = false;

//camera and screen
Camera camera(60.0f, glm::vec3(0, 0, -2));

int main(int argc, const char * argv[]) {
    //initialize GLFW with OpenGL 4.1
    glfwInit();
    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 4);
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 1);
    glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
    glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);
    
    //open GLFW window and make it the current context
    #ifdef RETINA
    GLFWwindow* window = glfwCreateWindow(SCR_WIDTH/2, SCR_HEIGHT/2, "OpenGL Raymarching", NULL, NULL);
    #else
    GLFWwindow* window = glfwCreateWindow(SCR_WIDTH, SCR_HEIGHT, "OpenGL Raymarching", NULL, NULL);
    #endif
    if(window == NULL) {
        std::cerr << "Failed to create GLFW window" << std::endl;
        glfwTerminate();
        return -1;
    }
    glfwMakeContextCurrent(window);
    
    //set the viewport size and apply changes every time it is changed by a user
    glfwSetFramebufferSizeCallback(window, framebuffer_size_callback);
    glfwSetCursorPosCallback(window, mouse_callback);
    glfwSetMouseButtonCallback(window, mouse_button_callback);
    glfwSetScrollCallback(window, scroll_callback);
    
    //tell GLFW to capture the mouse
    glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);
    
    //initalize GLAD
    if(!gladLoadGLLoader((GLADloadproc)glfwGetProcAddress)) {
        std::cerr << "Failed to initialize GLAD" << std::endl;
        return -1;
    }
    
    //generate drawing shader, initialize screen object and set screen ratio in the camera object
    #if SWITCH_GRAPHICS == false
    Shader shader("shaders/screen.vs", "shaders/screen_raymarching.fs");
    #else
    Shader shader("shaders/screen.vs", "shaders/screen_raytracing.fs");
    #endif

    Screen screen;
    
    float SCR_RATIO = float(SCR_WIDTH)/float(SCR_HEIGHT);
    
    camera.setSize(SCR_RATIO);
    
    //main drawing loop
    while(!glfwWindowShouldClose(window)) {
        #ifdef __APPLE__
        macWindowFix(window);
        #endif
        processInput(window);
        
        float currentFrameTime = glfwGetTime();
        delta_time = currentFrameTime - last_frame_time;
        last_frame_time = currentFrameTime;
        if(fps_steps_counter == fps_steps) {
            std::cout << glm::round(1.0f/(fps_sum/float(fps_steps))) << "\n";
            fps_steps_counter = 0;
            fps_sum = 0;
        }
        fps_sum += delta_time;
        fps_steps_counter++;
        
        camera.transferData(shader);
        
        shader.setFloat("time", currentFrameTime);
        
        screen.draw(shader);
        
        glfwSwapBuffers(window);
        glfwPollEvents();
    }
    
    glfwTerminate();
    return 0;
}

void framebuffer_size_callback(GLFWwindow* window, int width, int height) {
    glViewport(0, 0, width, height);
    SCR_WIDTH = width;
    SCR_HEIGHT = height;
    float SCR_RATIO = float(width)/float(height);
    camera.setSize(SCR_RATIO);
}

void processInput(GLFWwindow* window) {
    if(glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS) glfwSetWindowShouldClose(window, true);
    if(glfwGetKey(window, GLFW_KEY_W) == GLFW_PRESS) camera.move(FORWARD, delta_time);
    if(glfwGetKey(window, GLFW_KEY_S) == GLFW_PRESS) camera.move(BACK, delta_time);
    if(glfwGetKey(window, GLFW_KEY_A) == GLFW_PRESS) camera.move(LEFT, delta_time);
    if(glfwGetKey(window, GLFW_KEY_D) == GLFW_PRESS) camera.move(RIGHT, delta_time);
    
    if(glfwGetKey(window, GLFW_KEY_LEFT_SHIFT) == GLFW_PRESS) camera.setFasterSpeed(true);
    else if(glfwGetKey(window, GLFW_KEY_LEFT_SHIFT) == GLFW_RELEASE) camera.setFasterSpeed(false);
    if(glfwGetKey(window, GLFW_KEY_LEFT_CONTROL) == GLFW_PRESS) camera.setSlowerSpeed(true);
    else if(glfwGetKey(window, GLFW_KEY_LEFT_CONTROL) == GLFW_RELEASE && glfwGetKey(window, GLFW_KEY_LEFT_SHIFT) == GLFW_RELEASE) camera.setSlowerSpeed(false);
    
    if(glfwGetKey(window, GLFW_KEY_ENTER) == GLFW_PRESS) {
        if(!taking_screenshot) camera.takeScreenshot(SCR_WIDTH, SCR_HEIGHT);
        taking_screenshot = true;
    } else if(glfwGetKey(window, GLFW_KEY_ENTER) == GLFW_RELEASE) {
        taking_screenshot = false;
    }
}

void mouse_callback(GLFWwindow* window, double pos_x, double pos_y) {
    if(first_mouse) {
        last_x = pos_x;
        last_y = pos_y;
        first_mouse = false;
    }
    
    float offset_x = pos_x - last_x;
    float offset_y = last_y - pos_y;
    last_x = pos_x;
    last_y = pos_y;
    
    if(mouse_hidden) camera.rotate(offset_x, offset_y);
}

void mouse_button_callback(GLFWwindow* window, int button, int action, int mods) {
    if (button == GLFW_MOUSE_BUTTON_LEFT && action == GLFW_PRESS) {
        if(mouse_hidden) {
            glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_NORMAL);
        }
        else glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);
        
        mouse_hidden = !mouse_hidden;
    }
}

void scroll_callback(GLFWwindow* window, double xoffset, double yoffset) {
    camera.zoom(yoffset);
}

//for some reason on Mac OS 10.14+ OpenGL window will only display black color until it is resized for the first time. This function does that automatically
#ifdef __APPLE__
void macWindowFix(GLFWwindow* window) {
    if(!macMoved) {
        int x, y;
        glfwGetWindowPos(window, &x, &y);
        glfwSetWindowPos(window, ++x, y);
        
        glViewport(0, 0, SCR_WIDTH, SCR_HEIGHT);
        macMoved = true;
    }
}
#endif
