#version 400 core
in vec3 vertexPosition;
in vec2 vertexUv;
in float vertexIllumination;

out vec2 uv;
out float illumination;
out vec3 position;

uniform mat4 transformationMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

void main()
{
    gl_Position = projectionMatrix * viewMatrix * transformationMatrix  * vec4(vertexPosition, 1.0);
	uv = vertexUv;
	illumination = vertexIllumination;
	position = vertexPosition;
}
