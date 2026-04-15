#version 460 core

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec2 aUV;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in vec3 aTangent;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProj;

out vec2 vUV;
out vec3 vWorldPos;
out mat3 vTBN;

void main()
{
    vec3 worldPos = vec3(uModel * vec4(aPos, 1.0));

    mat3 normalMatrix = mat3(transpose(inverse(uModel)));

    vec3 N = normalize(normalMatrix * aNormal);
    vec3 T = normalize(normalMatrix * aTangent);
    T = normalize(T - dot(T, N) * N);
    vec3 B = cross(N, T);

    vUV = aUV;
    vWorldPos = worldPos;
    vTBN = mat3(T, B, N);

    gl_Position = uProj * uView * vec4(worldPos, 1.0);
}