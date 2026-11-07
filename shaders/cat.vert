#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aTexCoord;

uniform mat4 uModel;            // rotates and moves only, so it turns the normals as well
uniform mat4 uViewProjection;

out vec3 vPosition;             // world space
out vec3 vNormal;
out vec2 vTexCoord;

void main()
{
    vec4 position = uModel * vec4(aPosition, 1.0);
    vPosition = position.xyz;
    vNormal = mat3(uModel) * aNormal;
    vTexCoord = aTexCoord;
    gl_Position = uViewProjection * position;
}
