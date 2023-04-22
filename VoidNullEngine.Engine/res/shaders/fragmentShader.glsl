#version 330 core

struct PointLight {
    vec2 position;  // 8:0

    vec3 ambient;   // 16:8
    vec4 light;     // 16:32

    //              size: 48
};

struct DirLight {
    vec2 position;  // 8:0
    vec2 direction; // 8:8
    float focus;    // 4:16

    vec3 ambient;   // 16:32
    vec4 light;     // 16:48

    //              size: 64
};

#define MAX_NB_LIGHT_SOURCES 64

out vec4 FragColor;

in vec2 textureCoord;
in vec2 FragCoord;
in vec2 ScreenCoord;

uniform vec4 clearColor;
uniform vec4 viewport;
uniform struct {
    sampler2D sample;
    mat3 transform;
} texture;
uniform int textured;

layout (std140) uniform Light
{
    vec3 ambient;                                   // 16:0
    
    PointLight pointLights[MAX_NB_LIGHT_SOURCES];   // 48 * 64 = 3072:16
    int nbPointLights;                              // 4:3088

    DirLight dirLights[MAX_NB_LIGHT_SOURCES];       // 64 * 64 = 4096:3104
    int nbDirLights;                                // 4:7200
} lights;

float fade(float x) {
    if (x >= 1) return 1;
    if (x <= 0) return 0;
    x = x * x * x * (x * (x * 6 - 15) + 10);
    return x;
}

vec3 calculatePointLight(PointLight light) {
    float distance = length(light.position - FragCoord);
    float energy = fade(pow(light.light.w / distance, 2));
    if (energy < 0.01) return vec3(0, 0, 0);
    return light.light.xyz * energy;
}

vec3 calculateDirLight(DirLight light) {
    float focus;
    if (light.focus > 1) return vec3(0, 0, 0);
    if (light.focus > -1) {
        vec2 direction = normalize(light.direction);
        focus = dot(direction, normalize(FragCoord - light.position));
        if (focus <= light.focus) return vec3(0, 0, 0);
        focus = fade((focus - light.focus) / (1 - light.focus));
    }
    else focus = 1;
    float distance = length(light.position - FragCoord);
    float energy = fade(pow(light.light.w / distance, 2));
    if (energy < 0.01) return vec3(0, 0, 0);
    return light.light.xyz * energy * focus;
}

void main() {
    if (ScreenCoord.x < viewport.x || ScreenCoord.x > viewport.z || ScreenCoord.y < viewport.y || ScreenCoord.y > viewport.w) {
        FragColor = clearColor;
        return;
    }

    vec3 ambient = lights.ambient;
    vec3 light = vec3(0, 0, 0);

    for (int i = 0; i < lights.nbPointLights; ++i) {
        ambient = max(ambient, lights.pointLights[i].ambient);
        light += calculatePointLight(lights.pointLights[i]);
    }

    for (int i = 0; i < lights.nbDirLights; ++i) {
        ambient = max(ambient, lights.dirLights[i].ambient);
        light += calculateDirLight(lights.dirLights[i]);
    }

    vec4 color = vec4(0.0, 0.0, 0.0, 0.0);
    if (textured != 0) color = mix(color, texture(texture.sample, (texture.transform * vec3(textureCoord, 1.0f)).xy), 1.0);

    FragColor = color * vec4(ambient + light, 1.0);
}