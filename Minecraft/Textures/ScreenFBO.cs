using OpenTK.Graphics.OpenGL;
using System;

namespace Minecraft
{
    class ScreenFBO
    {
        private int fbo;
        public int colorTexture { get; private set; }
        public int normalDepthTexture { get; private set; }
        private int renderBuffer;

        public ScreenFBO(int screenWidth, int screenHeight)
        {
            CreateFBO(screenWidth, screenHeight);
        }

        private void CreateFBO(int screenWidth, int screenHeight)
        {
            // Create and bind FBO
            GL.Ext.GenFramebuffers(1, out fbo);
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, fbo);

            // create the color buffers
            DrawBuffersEnum[] buffers = new DrawBuffersEnum[2];
            for (int i = 0; i < 2; i++)
            {
                buffers[i] = DrawBuffersEnum.ColorAttachment0 + i;
            }
            GL.DrawBuffers(2, buffers);

            // Create and bind color texture
            colorTexture = GL.Ext.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, colorTexture);
            // Set some color texture Settings
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, screenWidth, screenHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            // Attach color texture to FBO
            GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTexture, 0);

            // Create and bind color texture
            normalDepthTexture = GL.Ext.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, normalDepthTexture);
            // Set some color texture Settings
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, screenWidth, screenHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            // Attach color texture to FBO
            GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, normalDepthTexture, 0);

            // Create render buffer object and bind it
            GL.Ext.GenRenderbuffers(1, out renderBuffer);
            GL.Ext.BindRenderbuffer(RenderbufferTarget.RenderbufferExt, renderBuffer);

            // Set internal format to depth component
            GL.Ext.RenderbufferStorage(RenderbufferTarget.RenderbufferExt, RenderbufferStorage.DepthComponent24, screenWidth, screenHeight);
            GL.Ext.FramebufferRenderbuffer(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachmentExt, RenderbufferTarget.RenderbufferExt, renderBuffer);

            ValidateFBO();
            UnbindFBO();
        }

        public void AdjustToWindowSize(int screenWidth, int screenHeight)
        {
            OnCloseGame();
            CreateFBO(screenWidth, screenHeight);
        }

        public void BindFBO()
        {
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, fbo);
        }

        public void UnbindFBO()
        {
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
        }

        public void OnCloseGame()
        {
            GL.DeleteTexture(colorTexture);
            GL.DeleteTexture(normalDepthTexture);
            GL.DeleteRenderbuffer(renderBuffer);
            GL.DeleteFramebuffer(fbo);
        }

        private bool ValidateFBO()
        {
            FramebufferErrorCode code = GL.Ext.CheckFramebufferStatus(FramebufferTarget.FramebufferExt);
            switch (code)
            {
                case FramebufferErrorCode.FramebufferCompleteExt:
                    Logger.Info("FBO: The framebuffer is complete and valid for rendering.");
                    return true;
                case FramebufferErrorCode.FramebufferIncompleteAttachmentExt:
                    Logger.Error("FBO: One or more attachment points are not framebuffer attachment complete. This could mean there’s no texture attached or the format isn’t renderable. For color textures this means the base format must be RGB or RGBA and for depth textures it must be a DEPTH_COMPONENT format. Other causes of this error are that the width or height is zero or the z-offset is out of range in case of render to volume.");
                    break;
                case FramebufferErrorCode.FramebufferIncompleteMissingAttachmentExt:
                    Logger.Error("FBO: There are no attachments.");
                    break;
                case FramebufferErrorCode.FramebufferIncompleteDimensionsExt:
                    Logger.Error("FBO: Attachments are of different size. All attachments must have the same width and height.");
                    break;
                case FramebufferErrorCode.FramebufferIncompleteFormatsExt:
                    Logger.Error("FBO: The color attachments have different format. All color attachments must have the same format.");
                    break;
                case FramebufferErrorCode.FramebufferIncompleteDrawBufferExt:
                    Logger.Error("FBO: An attachment point referenced by GL.DrawBuffers() doesn’t have an attachment.");
                    break;
                case FramebufferErrorCode.FramebufferIncompleteReadBufferExt:
                    Logger.Error("FBO: The attachment point referenced by GL.ReadBuffers() doesn’t have an attachment.");
                    break;
                case FramebufferErrorCode.FramebufferUnsupportedExt:
                    Logger.Error("FBO: This particular FBO configuration is not supported by the implementation.");
                    break;
                default:
                    Logger.Error("FBO: Status unknown: " + code);
                    break;
            }
            return false;
        }
    }
}
