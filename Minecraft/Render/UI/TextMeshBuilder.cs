using OpenTK;
using System.Linq;

namespace Minecraft
{
    class TextMeshBuilder
    {
        public float[] GetVerticesForText(UIText textComponent)
        {
            int charCount = textComponent.Text.Count(c => c != '\n');
            float[] allVertices = new float[charCount * 6 * 3];

            float xNdc = (textComponent.PixelPositionInCanvas.X / textComponent.ParentCanvas.PixelWidth) * 2 - 1;
            float yNdc = 1 - (textComponent.PixelPositionInCanvas.Y / textComponent.ParentCanvas.PixelHeight) * 2;

            int xPointer = 0;
            int yPointer = 0;
            int charPointer = 0;
            int charCounter = 0;
            for (int j = 0; j < textComponent.Text.Length; j++)
            {
                char c = textComponent.Text[charPointer++];
                if (c == '\n')
                {
                    yPointer -= (int)(textComponent.Font.DesiredPixelLineHeight * textComponent.Scale.Y);
                    xPointer = 0;
                    continue;
                }

                textComponent.Font.FontChars.TryGetValue(c, out Character charc);

                float cxPointer = xNdc + (float)xPointer / textComponent.ParentCanvas.PixelWidth;
                float cyPointer = yNdc + (float)yPointer / textComponent.ParentCanvas.PixelHeight;

                float cwidth = (float)charc.Width / textComponent.ParentCanvas.PixelWidth * textComponent.Scale.X;
                float cHeight = -(float)charc.Height / textComponent.ParentCanvas.PixelHeight * textComponent.Scale.Y;
                float cxOffset = (float)charc.XOffset / textComponent.ParentCanvas.PixelWidth * textComponent.Scale.X;
                float cyOffset = -(float)charc.YOffset / textComponent.ParentCanvas.PixelHeight * textComponent.Scale.Y;
                Vector3 topLeft = new Vector3(cxPointer + cxOffset, cyPointer + cyOffset, 0);
                Vector3 bottomLeft = new Vector3(cxPointer + cxOffset, cyPointer + cyOffset + cHeight, 0);
                Vector3 bottomRight = new Vector3(cxPointer + cxOffset + cwidth, cyPointer + cyOffset + cHeight, 0);
                Vector3 topRight = new Vector3(cxPointer + cxOffset + cwidth, cyPointer + cyOffset, 0);

                int i = charCounter * 18;
                charCounter++;
                allVertices[i + 0] = bottomLeft.X; allVertices[i + 1] = bottomLeft.Y; allVertices[i + 2] = bottomLeft.Z;  //bottom-left
                allVertices[i + 3] = bottomRight.X; allVertices[i + 4] = bottomRight.Y; allVertices[i + 5] = bottomRight.Z; //bottom-right
                allVertices[i + 6] = topRight.X; allVertices[i + 7] = topRight.Y; allVertices[i + 8] = topRight.Z;    //top-right
                allVertices[i + 9] = bottomLeft.X; allVertices[i + 10] = bottomLeft.Y; allVertices[i + 11] = bottomLeft.Z;  //bottom-left
                allVertices[i + 12] = topRight.X; allVertices[i + 13] = topRight.Y; allVertices[i + 14] = topRight.Z;    //top-right
                allVertices[i + 15] = topLeft.X; allVertices[i + 16] = topLeft.Y; allVertices[i + 17] = topLeft.Z;     //top-left
                xPointer += (int)(charc.XAdvance * textComponent.Scale.X);
            }

            return allVertices;
        }

        public float[] GetTexturesForText(UIText textComponent)
        {
            int charCount = textComponent.Text.Count(c => c != '\n');
            float[] allTextures = new float[charCount * 6 * 2];
            int charPointer = 0;
            int charCounter = 0;
            for (int j = 0; j < textComponent.Text.Length; j++)
            {
                char c = textComponent.Text[charPointer++];
                textComponent.Font.FontChars.TryGetValue(c, out Character charc);
                if (c == '\n')
                {
                    continue;
                }

                int i = charCounter * 12;
                charCounter++;
                allTextures[i + 0] = charc.XTextureMin; allTextures[i + 1] = charc.YTextureMin + charc.YTextureOffset; //bottom-left
                allTextures[i + 2] = charc.XTextureMin + charc.XTextureOffset; allTextures[i + 3] = charc.YTextureMin + charc.YTextureOffset; //bottom-right
                allTextures[i + 4] = charc.XTextureMin + charc.XTextureOffset; allTextures[i + 5] = charc.YTextureMin;                        //top-right
                allTextures[i + 6] = charc.XTextureMin; allTextures[i + 7] = charc.YTextureMin + charc.YTextureOffset; //bottom-left
                allTextures[i + 8] = charc.XTextureMin + charc.XTextureOffset; allTextures[i + 9] = charc.YTextureMin;                        //top-right
                allTextures[i + 10] = charc.XTextureMin; allTextures[i + 11] = charc.YTextureMin;                        //top-left
            }
            return allTextures;
        }
    }
}
