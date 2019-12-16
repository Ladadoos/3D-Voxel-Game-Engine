#version 400 core
in vec3 vertexPosition;			
in vec2 vertexUv;				

out vec2 uv;				
out vec2 fragmentPosition;				

void main()
{
	uv = vertexUv;
	fragmentPosition = vec2(vertexPosition) * 0.5 + vec2(0.5, 0.5);
	gl_Position = vec4(vertexPosition, 1);
}