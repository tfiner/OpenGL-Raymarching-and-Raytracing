add_subdirectory(glad)

add_executable(gpu-tracer main.cpp)

target_include_directories(gpu-tracer PRIVATE src glad/include /opt/homebrew/include)

target_link_options(gpu-tracer PRIVATE -L/opt/homebrew/lib)
target_link_libraries(gpu-tracer glad glfw)

target_compile_features(gpu-tracer PUBLIC cxx_std_17)
