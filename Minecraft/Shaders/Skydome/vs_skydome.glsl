#version 400 core
layout (location = 0) in vec3 vertexPosition;

out vec3 position;

uniform mat4 viewProjectionMatrix;

void main()
{
    gl_Position = viewProjectionMatrix * vec4(vertexPosition, 1.0F);
	position = vertexPosition;
}
