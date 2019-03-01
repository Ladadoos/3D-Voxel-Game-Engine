using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;

namespace Minecraft.Tools
{
    class Loader
    {
        private List<int> textures = new List<int>();

        public void CleanUp()
        {
            foreach (int texture in textures)
            {
                GL.DeleteTexture(texture);
            }
        }

        public int LoadTexture(string filePath)
        {
            int texture;
            GL.GenTextures(1, out texture);
            GL.BindTexture(TextureTarget.Texture2D, texture);

           // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, 10497); //10497 = REPEAT
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, 10497);
           // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, 9728); //9729 = LINEAR
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, 9728); //9728 = NEAREST

            Bitmap image = new Bitmap(filePath);
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            image.UnlockBits(data);
            return texture;
        }
    }
}
