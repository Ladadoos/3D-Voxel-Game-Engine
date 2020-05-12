#version 400 core
layout (location = 0) out vec4 fragmentColor;

uniform vec3 color;

void main()
{
   fragmentColor = vec4(color, 1);
}