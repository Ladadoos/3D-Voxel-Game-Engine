#version 400 core
layout (location = 0) out vec4 fragmentColor;
layout (location = 1) out vec4 normalDepthColor;

in vec2 uv;
in float illumination;
in vec3 position;
in vec3 normal;

uniform sampler2D textureAtlas;

void main()
{
   vec4 albedo = texture(textureAtlas, uv);
   if(albedo.rgb == vec3(1, 1, 1))
   {
		discard;
   }
   fragmentColor = albedo * illumination;  
   normalDepthColor = vec4(1, 0, 0, 1);
}