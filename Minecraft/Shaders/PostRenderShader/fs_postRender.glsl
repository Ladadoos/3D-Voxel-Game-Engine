#version 400 core
out vec4 outputColor;

in vec2 fragmentPosition;  //Fragment position in screen space
in vec2 uv;                //Interpolated texture coordinates

uniform sampler2D colorTexture;
uniform sampler2D depthNormalTexture;

void main()
{
   outputColor = texture(colorTexture, uv);
}