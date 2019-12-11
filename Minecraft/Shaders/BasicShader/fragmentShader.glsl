#version 400 core
out vec4 FragColor;

in vec2 TexCoord;
in float Light;

uniform sampler2D textureAtlas;

void main()
{
   FragColor = texture(textureAtlas, TexCoord) / Light;
}