#version 400 core
layout (location = 0) out vec4 fragmentColor;
layout (location = 1) out vec4 normalDepthColor;

in vec2 uv;
in float brightness;
in float sunlight;
in vec3 rgbColor;
in vec3 position;
in vec3 normal;

uniform sampler2D textureAtlas;
uniform vec3 sunColor;
uniform vec3 ambientColor;

float convertRange(float oldMin, float oldMax, float newMin, float newMax, float oldValue)
{
    float oldRange = oldMax - oldMin;
    float newRange = newMax - newMin;
    return (((oldValue - oldMin) * newRange) / oldRange) + newMin;
}

void main()
{
   vec4 albedo = texture(textureAtlas, uv);
   if(albedo.rgb == vec3(1.0F))
   {
		discard;
   }

   vec4 materialColor = albedo * vec4(rgbColor, 1.0F) + albedo * vec4(sunColor, 1.0F);
   materialColor.x = convertRange(0, 1, 0, 1 - ambientColor.x, materialColor.x);
   materialColor.y = convertRange(0, 1, 0, 1 - ambientColor.y, materialColor.y);
   materialColor.z = convertRange(0, 1, 0, 1 - ambientColor.z, materialColor.z);

   fragmentColor = (materialColor + albedo * vec4(ambientColor, 1.0F))* brightness;
   normalDepthColor = vec4(normal, 1.0F);
}