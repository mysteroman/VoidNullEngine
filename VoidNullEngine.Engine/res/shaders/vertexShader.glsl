#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexturePosition;

out vec2 textureCoord;
out vec2 FragCoord;
out vec2 ScreenCoord;

uniform mat4 projection;
uniform mat4 model;

void main() {
    textureCoord = aTexturePosition;
    FragCoord = (model * vec4(aPosition, 0.0, 1.0)).xy;
    ScreenCoord = (projection * model * vec4(aPosition, 0.0, 1.0)).xy;
    gl_Position = projection * model * vec4(aPosition, 0.0, 1.0);
}