#version 400 core
layout (location = 0) in vec3 vertexPosition;
layout (location = 1) in vec3 vertexNormal;
layout (location = 2) in vec2 vertexUv;
layout (location = 3) in uint vertexIllumination;

out vec2 uv;
out float brightness;
out float sunlight;
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

	brightness  = ((vertexIllumination >> 24) & 0x3F) / 64.0F;
	sunlight    = ((vertexIllumination >> 18) & 0x3F) / 64.0F;
	float red   = (vertexIllumination         & 0x3F) / 64.0F;
	float green = ((vertexIllumination >> 6)  & 0x3F) / 64.0F;
	float blue  = ((vertexIllumination >> 12) & 0x3F) / 64.0F;
	rgbColor = vec3(red, green, blue);
}
