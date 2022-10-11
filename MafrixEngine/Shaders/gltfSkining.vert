#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(binding = 0) uniform UniformBufferObject {
    mat4 model;
    mat4 view;
    mat4 proj;
} ubo;

layout(binding = 4) readonly buffer Ivb {
    mat4[] inverseMatrix;
};

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec3 inColor;
layout(location = 2) in vec2 inTexCoord;
layout(location = 3) in uvec4 inJoints;
layout(location = 4) in vec4 inWeights;

layout(location = 0) out vec3 fragColor;
layout(location = 1) out vec2 fragTexCoord; 
layout(location = 2) out vec4 worldPos;


void main() {
    mat4 mat =
        inWeights.x * inverseMatrix[inJoints.x] +
        inWeights.y * inverseMatrix[inJoints.y] +
        inWeights.z * inverseMatrix[inJoints.z] +
        inWeights.w * inverseMatrix[inJoints.w];
    worldPos = ubo.model * mat * vec4(inPosition, 1.0);
    gl_Position = ubo.proj * ubo.view * ubo.model * mat * vec4(inPosition, 1.0);
    fragColor = inColor;
    fragTexCoord = inTexCoord;
}