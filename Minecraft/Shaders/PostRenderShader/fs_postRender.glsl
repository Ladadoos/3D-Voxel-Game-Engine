#version 400 core
out vec4 outputColor;

in vec2 fragmentPosition;  //Fragment position in screen space
in vec2 uv;                //Interpolated texture coordinates

uniform sampler2D colorTexture;
uniform sampler2D depthNormalTexture;

vec4 vignette(vec4 color){
	vec2 texSize = textureSize(colorTexture, 0);
	vec2 relativeToCenter = gl_FragCoord.xy / texSize - 0.5F;
	float distToCenter = length(relativeToCenter);

	const float innerDistance = 0.4;
	const float outerDistance = 0.8;
	const float blendFactor = 0.325;
	float vignette = smoothstep(outerDistance, innerDistance, distToCenter);
	return vec4(mix(color.rgb, color.rgb * vignette, blendFactor), 1);
}   

void main()
{
	vec4 rawColor = texture(colorTexture, uv);
	outputColor = vignette(rawColor);
}