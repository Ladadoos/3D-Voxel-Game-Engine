#version 400 core
layout (location = 0) out vec4 fragmentColor;
layout (location = 1) out vec4 normalDepthColor;

in vec2 uv;
in float brightness;
in vec3 rgbColor;
in vec3 position;
in vec3 normal;

uniform sampler2D textureAtlas;

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

   vec4 ambientLight = vec4(0.05F, 0.05F, 0.05F, 1.0F);
   vec4 materialColor = albedo * brightness * vec4(rgbColor, 1.0F);
   materialColor.x = convertRange(0, 1, 0, 1 - ambientLight.x, materialColor.x);
   materialColor.y = convertRange(0, 1, 0, 1 - ambientLight.y, materialColor.y);
   materialColor.z = convertRange(0, 1, 0, 1 - ambientLight.z, materialColor.z);

   fragmentColor = materialColor + albedo * ambientLight;
   normalDepthColor = vec4(normal, 1.0F);
}