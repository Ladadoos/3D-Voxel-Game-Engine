#version 400 core
layout (location = 0) in vec3 vertexPosition;
layout (location = 1) in vec3 vertexNormal;
layout (location = 2) in vec2 vertexUv;
layout (location = 3) in float vertexIllumination;

out vec2 uv;
out float illumination;
out vec3 position;
out vec3 normal;

uniform mat4 transformationMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

void main()
{
    gl_Position = projectionMatrix * viewMatrix * transformationMatrix  * vec4(vertexPosition, 1.0);
	uv = vertexUv;
	illumination = vertexIllumination;
	position = vertexPosition;
	normal = vertexNormal;
}
