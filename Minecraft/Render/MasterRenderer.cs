using OpenTK;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;

namespace Minecraft
{
    class MasterRenderer
    {
        // The projection matrix used when the player isn't doing or affected by anything special like running, shifting etc...
        private ProjectionMatrixInfo standardProjectionMatrixInfo;
        public ProjectionMatrixInfo currentProjectionMatrixInfo;

        private float colorClearR = 0.57F;
        private float colorClearG = 0.73F;
        private float colorClearB = 1.0F;

        public Matrix4 currentProjectionMatrix;
        public ShaderBasic basicShader;

        public MasterRenderer(int width, int height)
        {
            EnableDepthTest();
            EnableCulling();

            basicShader = new ShaderBasic();
            basicShader.Start();
            basicShader.LoadInt(basicShader.location_Texture1, 0);
            Matrix4 transformationMatrix = Maths.CreateTransformationMatrix(new Vector3(1, 1, 1), 0, 0, 0, 1, 1, 1);
            basicShader.LoadMatrix(basicShader.location_TransformationMatrix, transformationMatrix);
            basicShader.LoadMatrix(basicShader.location_ProjectionMatrix, currentProjectionMatrix);
            basicShader.Stop();

            standardProjectionMatrixInfo = new ProjectionMatrixInfo(0.1F, 10000.0F, 1.5F, width, height);
            currentProjectionMatrixInfo = standardProjectionMatrixInfo;
            UpdateCurrentProjectionMatrix();
        }

        public void Render(Camera camera,  World world)
        {
            GL.ClearColor(colorClearR, colorClearG, colorClearB, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            basicShader.Start();
            basicShader.LoadMatrix(basicShader.location_ViewMatrix, Maths.CreateViewMatrix(camera));
            foreach (KeyValuePair<Vector2, RenderChunk> renderChunk in world.renderChunks)
            {
                Vector3 min = new Vector3(renderChunk.Key.X * 16, 0, renderChunk.Key.Y * 16);
                Vector3 max = min + new Vector3(16, 256, 16);
                if (!camera.viewFrustum.IsAABBInFrustum(new AABB(min, max)))
                {
                    continue;
                }
                renderChunk.Value.HardBlocksModel.Bind();
                basicShader.LoadMatrix(basicShader.location_TransformationMatrix, renderChunk.Value.TransformationMatrix);
                GL.DrawElements(PrimitiveType.Triangles, renderChunk.Value.HardBlocksModel.indicesCount, DrawElementsType.UnsignedInt, 0);
                renderChunk.Value.HardBlocksModel.Unbind();
            }
            basicShader.Stop();
        }

        public void OnWindowResize(int newWidth, int newHeight)
        {
            standardProjectionMatrixInfo.windowWidth = newWidth;
            standardProjectionMatrixInfo.windowHeight = newHeight;

            currentProjectionMatrixInfo.windowWidth = newWidth;
            currentProjectionMatrixInfo.windowHeight = newHeight;

            UpdateCurrentProjectionMatrix();
        }

        public void SetFieldOfView(float fieldOfView)
        {
            if (currentProjectionMatrixInfo.fieldOfView != fieldOfView)
            {
                currentProjectionMatrixInfo.fieldOfView = fieldOfView;
                UpdateCurrentProjectionMatrix();
            }
        }

        public void ResetToDefaultFieldOfView()
        {
            SetFieldOfView(standardProjectionMatrixInfo.fieldOfView);
        }

        public void CleanUp()
        {
            basicShader.CleanUp();
        }

        private void EnableCulling()
        {
           GL.Enable(EnableCap.CullFace);
           GL.CullFace(CullFaceMode.Back);
        }

        private void DisableCulling()
        {
            GL.Disable(EnableCap.CullFace);
        }

        /// <summary> Renders only the lines formed by connecting the vertices together.
        private void EnableLineModeRendering()
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        }

        /// <summary> Enabling depth test insures that object A behind object B isn't rendered over object B </summary>
        private void EnableDepthTest()
        {
            GL.Enable(EnableCap.DepthTest);
        }

        private void UpdateCurrentProjectionMatrix()
        {
            currentProjectionMatrix = Maths.CreateProjectionMatrix(
                currentProjectionMatrixInfo.fieldOfView,
                currentProjectionMatrixInfo.windowWidth,
                currentProjectionMatrixInfo.windowHeight,
                currentProjectionMatrixInfo.distanceNearPlane,
                currentProjectionMatrixInfo.distanceFarPlane
            );

            basicShader.Start();
            basicShader.LoadMatrix(basicShader.location_ProjectionMatrix, currentProjectionMatrix);
            basicShader.Stop();
        }
    }
}
