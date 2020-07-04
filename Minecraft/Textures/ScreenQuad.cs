using OpenTK.Graphics.OpenGL;
using System;

namespace Minecraft
{
    /// <summary>
    /// The screen filling used to render a texture with post-processing effects.
    /// </summary>
    class ScreenQuad
    {
        private readonly ScreenFBO fbo;
        private readonly PostRenderShader shader;

        private readonly int vao, vbo;
        private readonly float[] quadVertices = { 
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
            shader.LoadTexture(shader.Location_ColorTexture, 0, fbo.ColorTextureID);
            shader.LoadTexture(shader.Location_NormalDepthTexture, 1, fbo.NormalDepthTextureID);
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            shader.Stop();
        }

        public void AdjustToWindowSize(int screenWidth, int screenHeight)
        {
            fbo.AdjustToWindowSize(screenWidth, screenHeight);
        }

        public void Bind()
        {
            fbo.BindFBO();
        }

        public void Unbind()
        {
            fbo.UnbindFBO();
        }
    }
}
