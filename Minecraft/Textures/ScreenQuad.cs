using OpenTK.Graphics.OpenGL;
using System;

namespace Minecraft
{
    class ScreenQuad
    {
        //Add OnScreenSizeChanged...

        public ScreenFBO fbo { get; private set; }
        private PostRenderShader shader;

        private int vao, vbo;
        private float[] quadVertices = { 
                // positions   // texCoords
            -1.0f,  1.0f, 0.0f,  0.0f, 1.0f,
            -1.0f, -1.0f, 0.0f,  0.0f, 0.0f,
             1.0f, -1.0f, 0.0f,  1.0f, 0.0f,

            -1.0f,  1.0f, 0.0f,  0.0f, 1.0f,
             1.0f, -1.0f, 0.0f,  1.0f, 0.0f,
             1.0f,  1.0f, 0.0f,  1.0f, 1.0f
        };

        public ScreenQuad(GameWindow window)
        {
            GL.GenVertexArrays(1, out vao);
            GL.GenBuffers(1, out vbo);
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(quadVertices.Length * sizeof(float)), quadVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            shader = new PostRenderShader();
            fbo = new ScreenFBO(window.Width, window.Height);
        }

        public void RenderToScreen()
        {
            shader.Start();
            shader.LoadTexture(shader.location_colorTexture, 0, fbo.colorTexture);
            shader.LoadTexture(shader.location_normalDepthTexture, 1, fbo.normalDepthTexture);
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            shader.Stop();
        }
    }
}
