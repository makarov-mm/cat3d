#version 330 core

in vec3 vPosition;
in vec3 vNormal;
in vec2 vTexCoord;

out vec4 fragColor;

uniform sampler2D uDiffuseMap;
uniform sampler2D uBumpMap;     // a height map in the red channel
uniform float uBumpDepth;       // world units from black to white in the height map, 0 for none
uniform vec3 uLightDirection;   // the way the light travels
uniform vec3 uCameraPosition;

const float Ambient = 0.18;
const float SpecularPower = 24.0;
const float SpecularStrength = 0.15;

// Tilts the normal by the slope of the height map, without tangents: the slope comes from
// how the height and the position change from pixel to pixel (Mikkelsen, "Bump Mapping
// Unparametrized Surfaces on the GPU", 2010).
vec3 bumpedNormal(vec3 normal)
{
    vec2 texCoordDx = dFdx(vTexCoord);
    vec2 texCoordDy = dFdy(vTexCoord);
    float height = texture(uBumpMap, vTexCoord).r;
    float heightDx = (texture(uBumpMap, vTexCoord + texCoordDx).r - height) * uBumpDepth;
    float heightDy = (texture(uBumpMap, vTexCoord + texCoordDy).r - height) * uBumpDepth;

    vec3 positionDx = dFdx(vPosition);
    vec3 positionDy = dFdy(vPosition);
    vec3 r1 = cross(positionDy, normal);
    vec3 r2 = cross(normal, positionDx);
    float determinant = dot(positionDx, r1);
    vec3 gradient = sign(determinant) * (heightDx * r1 + heightDy * r2);
    return normalize(abs(determinant) * normal - gradient);
}

void main()
{
    vec3 albedo = texture(uDiffuseMap, vTexCoord).rgb;
    vec3 normal = normalize(vNormal);
    if (uBumpDepth > 0.0)
        normal = bumpedNormal(normal);

    vec3 toLight = normalize(-uLightDirection);
    vec3 toCamera = normalize(uCameraPosition - vPosition);
    vec3 halfway = normalize(toLight + toCamera);

    float diffuse = max(dot(normal, toLight), 0.0);
    float specular = SpecularStrength * pow(max(dot(normal, halfway), 0.0), SpecularPower);
    fragColor = vec4(albedo * (Ambient + diffuse) + specular, 1.0);
}
