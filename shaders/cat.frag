#version 460 core

in vec2 vUV;
in vec3 vWorldPos;
in mat3 vTBN;

out vec4 FragColor;

uniform sampler2D uDiffuse;
uniform sampler2D uNormalMap;
uniform vec3 uLightDir;
uniform vec3 uViewPos;
uniform int uInvertNormalY;

void main()
{
    vec3 albedo = texture(uDiffuse, vUV).rgb;

    vec3 normalTS = texture(uNormalMap, vUV).rgb * 2.0 - 1.0;
    if (uInvertNormalY != 0)
        normalTS.y = -normalTS.y;

    normalTS = normalize(normalTS);

    vec3 N = normalize(vTBN * vec3(0,0,1));
    vec3 L = normalize(-uLightDir);
    vec3 V = normalize(uViewPos - vWorldPos);
    vec3 H = normalize(L + V);

    float ambientStrength = 0.18;
    vec3 ambient = albedo * ambientStrength;

    float diff = max(dot(N, L), 0.0);
    vec3 diffuse = albedo * diff;

    float specPower = 24.0;
    float specStrength = 0.15;
    
    float spec = pow(max(dot(N, H), 0.0), specPower);
    vec3 specular = vec3(spec * specStrength);

    vec3 color = ambient + diffuse + specular;
    FragColor = vec4(color, 1.0);
}