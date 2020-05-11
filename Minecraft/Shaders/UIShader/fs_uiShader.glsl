#version 400 core
layout (location = 0) out vec4 fragmentColor;

in vec2 uv;
in vec3 position;

uniform sampler2D uiTexture;
uniform float transparency;
uniform vec3 color;

void main()
{
	vec4 albedo = texture(uiTexture, uv);
	if(albedo.a == 0)
    {
        discard;
    }
	fragmentColor = vec4(albedo.rgb * color, transparency);
}