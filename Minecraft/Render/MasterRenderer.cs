using OpenTK;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;

namespace Minecraft
{
    class MasterRenderer
    { 

        public float nearPlane = 0.1F;
        public float farPlane = 10000.0F;
        public float FOV = 1.5F;
        public int width;
        public int height;

        private float colorClearR = 0.57F;
        private float colorClearG = 0.73F;
        private float colorClearB = 1.0F;

        public Matrix4 projectionMatrix;

        public ShaderBasic basicShader;

        public MasterRenderer(int width, int height)
        {
            GL.Enable(EnableCap.DepthTest);
            EnableCulling();

            this.width = width;
            this.height = height;
            CreateProjectionMatrix();
            basicShader = new ShaderBasic();

            basicShader.Start();
            basicShader.LoadInt(basicShader.location_Texture1, 0);
            Matrix4 transformationMatrix = Maths.CreateTransformationMatrix(new Vector3(1, 1, 1), 0, 0, 0, 1, 1, 1);
            basicShader.LoadMatrix(basicShader.location_TransformationMatrix, transformationMatrix);
            basicShader.LoadMatrix(basicShader.location_ProjectionMatrix, projectionMatrix);
            basicShader.Stop();

            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        }

        public void Render(Camera camera,  World world)
        {
            GL.ClearColor(colorClearR, colorClearG, colorClearB, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            basicShader.Start();
            basicShader.LoadMatrix(basicShader.location_ViewMatrix, Maths.CreateViewMatrix(camera));
            foreach (KeyValuePair<Vector2, RenderChunk> renderChunk in world.renderChunks)
            {
                renderChunk.Value.HardBlocksModel.Bind();
                basicShader.LoadMatrix(basicShader.location_TransformationMatrix, renderChunk.Value.TransformationMatrix);
                GL.DrawElements(PrimitiveType.Triangles, renderChunk.Value.HardBlocksModel.indicesCount, DrawElementsType.UnsignedInt, 0);
                renderChunk.Value.HardBlocksModel.Unbind();
            }
            basicShader.Stop();
        }

        public void CleanUp()
        {
            basicShader.CleanUp();
        }

        public static void EnableCulling()
        {
           GL.Enable(EnableCap.CullFace);
           GL.CullFace(CullFaceMode.Back);
        }

        public static void DisableCulling()
        {
            GL.Disable(EnableCap.CullFace);
        }

        public void CreateProjectionMatrix()
        {
            projectionMatrix = Maths.CreateProjectionMatrix(FOV, width, height, nearPlane, farPlane);
        }
    }
}
