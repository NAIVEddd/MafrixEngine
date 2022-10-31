#version 450

layout (location = 0) in vec3 inPos;
layout (location = 1) in vec3 inColor;
layout (location = 2) in vec2 inTexCoord;

layout (binding = 0) uniform UBO 
{
	mat4 depthMVP;
} ubo;

out gl_PerVertex 
{
    vec4 gl_Position;   
};

 
void main()
{
	gl_Position =  ubo.depthMVP * vec4(inPos, 1.0);
}