#version 330 core

#define MAX_LIGHTS 8

in vec3 vNormal;
in vec2 vUV;          // <- from vertex shader

out vec4 FragColor;

// ---------- material ----------
uniform sampler2D uBaseMap;   // albedo / color texture
uniform vec3 uTintColor;      // optional multiplier (can be vec3(1))
uniform float uAmbient;

// ---------- lighting ----------
uniform int   uLightCount;
uniform vec3  uLightDirs[MAX_LIGHTS];
uniform vec3  uLightColors[MAX_LIGHTS];
uniform float uLightIntensity[MAX_LIGHTS];

// ---------- future normal mapping (NOT USED YET) ----------
uniform sampler2D uNormalMap;
uniform bool uUseNormalMap;

void main()
{
    // sample base texture
    vec3 baseColor = uTintColor;

    vec3 N = normalize(vNormal);

    vec3 lightAccum = vec3(0.0);

    for (int i = 0; i < uLightCount; i++)
    {
        vec3 L = normalize(-uLightDirs[i]);

        // Half-Lambert diffuse
        float diff = dot(N, L) * 0.5 + 0.5;
        diff = diff * diff;

        lightAccum += uLightColors[i] * diff * uLightIntensity[i];
    }

    vec3 finalColor = baseColor * (lightAccum + uAmbient);

    FragColor = vec4(finalColor, 1.0);
}