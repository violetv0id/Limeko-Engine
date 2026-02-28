#version 330 core

in vec3 vNormal;
out vec4 FragColor;

uniform vec3 uLightDir;
uniform vec3 uRandomColor;
uniform float uAmbient = 0.0; // 0 = no ambient, 1 = fully lit

void main()
{
    float diff = max(dot(normalize(vNormal), normalize(-uLightDir)), 0.0);

    // combine diffuse with ambient
    vec3 color = uRandomColor * (diff + uAmbient);

    FragColor = vec4(color, 1.0);
}