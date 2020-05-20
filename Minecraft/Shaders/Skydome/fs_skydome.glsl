#version 400 core
layout (location = 0) out vec4 fragmentColor;

in vec3 position;

uniform sampler2D ditherTexture;
uniform vec3 sunPosition;
uniform int time;

uniform vec3 topSkyColor;
uniform vec3 bottomSkyColor;
uniform vec3 horizonColor;
uniform vec3 sunColor;
uniform vec3 sunGlowColor;
uniform vec3 moonColor;
uniform vec3 moonGlowColor;

void main()
{
	float fragmentHeight = sin(position.y);

	float posDotSunPos = dot(position, sunPosition);
	float distToSun = length(sunPosition - position);

	vec3 moonPosition = sunPosition * -1;
	float posDotMoonPos = dot(position, moonPosition);
    float distToMoon = length(moonPosition - position);

	//Calculate the horizon thickness, which is dependant on the position of the sunColor
	//For example during sunset the horizon turns orange-ish and becomes thicker on the side
	//of the sun.
	float dist = posDotSunPos + 1.0F; 
	float horizonThickness = max(0.025F * dist * dist, 0.000F);

	vec3 skyGradiant = vec3(0.0F);
	if(fragmentHeight <= horizonThickness && fragmentHeight >= -horizonThickness) //Horizon
	{
		skyGradiant = horizonColor;	
	}else if(fragmentHeight > horizonThickness) //Top sky color
	{
		skyGradiant = mix(horizonColor, topSkyColor, fragmentHeight - horizonThickness);
	}else //Bottom sky color
	{
		skyGradiant = mix(horizonColor, bottomSkyColor, abs(fragmentHeight) - horizonThickness);
	}

	//Sun glow color
	if(distToSun < 0.25F)	
	{
		float mixPerc = distToSun / 0.25F;
		skyGradiant = mix(sunGlowColor, skyGradiant, mixPerc);
	}
	//Sun color
	if(distToSun < 0.1F)
	{
		float mixPerc = distToSun / 0.1F;
		skyGradiant = mix(sunColor, skyGradiant, mixPerc * mixPerc * mixPerc);
	}	

	//Moon glow color
	if(distToMoon < 0.25F)	
	{
		float mixPerc = distToMoon / 0.25F;
		skyGradiant = mix(moonGlowColor, skyGradiant, mixPerc);
	}
	//Moon color
	if(distToMoon < 0.1F)
	{
		float mixPerc = distToMoon / 0.1F;
		skyGradiant = mix(moonColor, skyGradiant, mixPerc * mixPerc * mixPerc);
	}

	fragmentColor = vec4(skyGradiant, 1.0F);

	//Apply dithering to remove any possible color banding effects
	fragmentColor += vec4(texture2D(ditherTexture, gl_FragCoord.xy / 8.0).r / 32.0 - (1.0 / 128.0));
}