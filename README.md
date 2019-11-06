# OpenGL-Raymarching-and-Raytracing
C++ OpenGL implementation of a simple Ray Marching and Ray Tracing engine

------ LICENSE ------

MIT License

Copyright (c) 2019 Antoni Wojcik

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.


---- EXPLANATION ----

Remark: THIS PROGRAM HAS BEEN TESTED ONLY ON MAC OS 10.14.6

Description:
This is a simple OpenGL Ray Marching and Ray Tracing 3D graphics engine.
OpenGL 4.1 is used to handle the graphics. 
Window is created with GLFW and GLAD libraries; GLM library is used in the vector calculations as it is compatible with OpenGL. 
To improve performance, only one ray is generated in both Ray Marching and Ray Tracing scripts.
The Ray Marching script supports only black and white colors and Ray Tracing script supports full range of RGB colors.
Ray Marching supports reflections, lighting - soft and hard shadows and shadow casting.
Ray Tracing supports reflective and refractive materials, and uses the camera as a light source. There is also a support of diffuse materials although the result looks very noisy due to using only one ray per pixel.

How to modify the scene:
Look at the constants in "shaders/screen_raymarching.fs" to change drawing parameters and the constants in "src/camera.h" to change camera parameters. 
To change the displayed scene modify the code in the "scene_distance" function in "shaders/screen_raymarching.fs".
The code in the other files should not be modified.

Controls:
A,W,S,D       to move the camera
SHIFT, CTRL   to speed up/slow down camera
MOUSE         to rotate the camera
MOUSE SCROLL  to zoom the camera
MOUSE CLICK   to release/show and lock/hide cursor
ENTER         to take a screenshot - the result: "screenshot.tga" will appear in the screenshots folder
