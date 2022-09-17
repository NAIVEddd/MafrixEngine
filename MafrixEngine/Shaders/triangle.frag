#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(binding = 1) uniform sampler2D texSampler;
layout(binding = 2) uniform sampler2D metallicRoughSampler;
layout(binding = 3) uniform sampler2D normalSampler;

layout(location = 0) in vec3 fragColor;
layout(location = 1) in vec2 fragTexCoord;
layout(location = 2) in vec4 worldPos;

layout(location = 0) out vec4 outColor;

void main() {
    vec2 mr = texture(metallicRoughSampler, fragTexCoord).rg;
    vec4 metallic = vec4(mr.r, mr.r, mr.r, 1.0);
    vec4 roughness = vec4(mr.g, mr.g, mr.g, 1.0);
    vec4 normal = texture(normalSampler, fragTexCoord);
    outColor = texture(texSampler, fragTexCoord);
    //outColor = metallic;
}