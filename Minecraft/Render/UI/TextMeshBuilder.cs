using OpenTK;
using System.Linq;

namespace Minecraft
{
    class TextMeshBuilder
    {
        public float[] GetVerticesForText(UIText textComponent)
        {
            int charCount = textComponent.text.Count(c => c != '\n');
            float[] allVertices = new float[charCount * 6 * 3];

            float xNdc = (textComponent.pixelPositionInCanvas.X / textComponent.parentCanvas.pixelWidth) * 2 - 1;
            float yNdc = 1 - (textComponent.pixelPositionInCanvas.Y / textComponent.parentCanvas.pixelHeight) * 2;

            int xPointer = 0;
            int yPointer = 0;
            int charPointer = 0;
            int charCounter = 0;
            for (int j = 0; j < textComponent.text.Length; j++)
            {
                char c = textComponent.text[charPointer++];
                if (c == '\n')
                {
                    yPointer -= (int)(textComponent.font.desiredPixelLineHeight * textComponent.scale.Y);
                    xPointer = 0;
                    continue;
                }

                textComponent.font.fontChars.TryGetValue(c, out Character charc);

                float cxPointer = xNdc + (float)xPointer / textComponent.parentCanvas.pixelWidth;
                float cyPointer = yNdc + (float)yPointer / textComponent.parentCanvas.pixelHeight;

                float cwidth = (float)charc.width / textComponent.parentCanvas.pixelWidth * textComponent.scale.X;
                float cHeight = -(float)charc.height / textComponent.parentCanvas.pixelHeight * textComponent.scale.Y;
                float cxOffset = (float)charc.xOffset / textComponent.parentCanvas.pixelWidth * textComponent.scale.X;
                float cyOffset = -(float)charc.yOffset / textComponent.parentCanvas.pixelHeight * textComponent.scale.Y;
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
                xPointer += (int)(charc.xAdvance * textComponent.scale.X);
            }

            return allVertices;
        }

        public float[] GetTexturesForText(UIText textComponent)
        {
            int charCount = textComponent.text.Count(c => c != '\n');
            float[] allTextures = new float[charCount * 6 * 2];
            int charPointer = 0;
            int charCounter = 0;
            for (int j = 0; j < textComponent.text.Length; j++)
            {
                char c = textComponent.text[charPointer++];
                textComponent.font.fontChars.TryGetValue(c, out Character charc);
                if (c == '\n')
                {
                    continue;
                }

                int i = charCounter * 12;
                charCounter++;
                allTextures[i + 0] = charc.xTextureMin; allTextures[i + 1] = charc.yTextureMin + charc.yTextureOffset; //bottom-left
                allTextures[i + 2] = charc.xTextureMin + charc.xTextureOffset; allTextures[i + 3] = charc.yTextureMin + charc.yTextureOffset; //bottom-right
                allTextures[i + 4] = charc.xTextureMin + charc.xTextureOffset; allTextures[i + 5] = charc.yTextureMin;                        //top-right
                allTextures[i + 6] = charc.xTextureMin; allTextures[i + 7] = charc.yTextureMin + charc.yTextureOffset; //bottom-left
                allTextures[i + 8] = charc.xTextureMin + charc.xTextureOffset; allTextures[i + 9] = charc.yTextureMin;                        //top-right
                allTextures[i + 10] = charc.xTextureMin; allTextures[i + 11] = charc.yTextureMin;                        //top-left
            }
            return allTextures;
        }
    }
}
