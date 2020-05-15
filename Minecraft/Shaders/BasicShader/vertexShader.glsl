#version 400 core
layout (location = 0) in vec3 vertexPosition;
layout (location = 1) in vec3 vertexNormal;
layout (location = 2) in vec2 vertexUv;
layout (location = 3) in uint vertexIllumination;

out vec2 uv;
out float brightness;
out vec3 rgbColor;
out vec3 position;
out vec3 normal;

uniform mat4 transformationMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

void main()
{
    gl_Position = projectionMatrix * viewMatrix * transformationMatrix  * vec4(vertexPosition, 1.0F);
	uv = vertexUv;
	position = vertexPosition;
	normal = vertexNormal;

	brightness  = (((vertexIllumination >> 12) & 0xFF) / 15.0F);
	float red   = ((vertexIllumination        & 0xF)  / 15.0F) + 0.05f;
	float green = (((vertexIllumination >> 4)  & 0xF)  / 15.0F) + 0.05f;
	float blue  = (((vertexIllumination >> 8)  & 0xF)  / 15.0F) + 0.05f;
	rgbColor = vec3(red, green, blue);
}
