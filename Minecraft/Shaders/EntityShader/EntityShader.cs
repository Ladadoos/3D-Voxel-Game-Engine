﻿namespace Minecraft
{
    class EntityShader : Shader
    {
        private static readonly string vertexFile = "../../Shaders/EntityShader/vs_entityShader.glsl";
        private static readonly string fragmentFile = "../../Shaders/EntityShader/fs_entityShader.glsl";

        public int location_TextureAtlas;
        public int location_TransformationMatrix;
        public int location_ViewMatrix;
        public int location_ProjectionMatrix;

        public EntityShader() : base(vertexFile, fragmentFile)
        {

        }

        protected override void GetAllUniformLocations()
        {
            location_TextureAtlas = GetUniformLocation("textureAtlas");
            location_TransformationMatrix = GetUniformLocation("transformationMatrix");
            location_ViewMatrix = GetUniformLocation("viewMatrix");
            location_ProjectionMatrix = GetUniformLocation("projectionMatrix");
        }

        protected override void BindAttributes()
        {
            BindAttribute(0, "vertexPosition");
            BindAttribute(1, "vertexNormal");
            BindAttribute(2, "vertexUv");
            BindAttribute(3, "vertexIllumination");
        }
    }
}