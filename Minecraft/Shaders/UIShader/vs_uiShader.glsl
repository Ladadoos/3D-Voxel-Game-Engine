#version 400 core
layout (location = 0) in vec3 vertexPosition;
layout (location = 1) in vec2 vertexUv;

out vec2 uv;
out vec3 position;

uniform mat4 transformationMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

void main()
{
    gl_Position = projectionMatrix * viewMatrix * transformationMatrix * vec4(vertexPosition, 1.0);
	uv = vertexUv;
	position = vertexPosition;
}
