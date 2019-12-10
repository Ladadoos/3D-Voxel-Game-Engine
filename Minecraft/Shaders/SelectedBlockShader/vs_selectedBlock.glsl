#version 400 core
layout (location = 0) in vec3 aPos;

uniform mat4 transformationMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

void main()
{
    gl_Position = projectionMatrix * viewMatrix * transformationMatrix  * vec4(aPos, 1.0);
}
