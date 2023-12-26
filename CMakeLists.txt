cmake_minimum_required(VERSION 3.11-3.18)

project(
    gpu_rt
    VERSION 0.1
    DESCRIPTION "GPU Raytracing")


add_subdirectory(GPURayMarching)

file(COPY ${CMAKE_SOURCE_DIR}/GPURaymarching/shaders DESTINATION ${CMAKE_BINARY_DIR}/GPURaymarching)
