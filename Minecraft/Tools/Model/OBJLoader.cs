using System;
using System.Collections.Generic;
using System.IO;

namespace Minecraft
{
    static class OBJLoader
    {
        public static ModelData Load(string fileName)
        {
            ModelData m = new ModelData();
            try
            {
                using(StreamReader streamReader = new StreamReader(fileName))
                {
                    m = Load(streamReader);
                    streamReader.Close();
                    return m;
                }
            } catch(Exception e) { Console.WriteLine(e); }
            return m;
        }

        private static char[] splitCharacters = new char[] { ' ' };
        private static char[] faceParamaterSplitter = new char[] { '/' };

        private static List<float> vertices = new List<float>();
        private static List<float> normals = new List<float>();
        private static List<float> texCoords = new List<float>();
        private static List<int> indices = new List<int>();

        private static ModelData Load(TextReader textReader)
        {
            string line;
            while((line = textReader.ReadLine()) != null)
            {
                line = line.Trim(splitCharacters);
                line = line.Replace("  ", " ");
                string[] parameters = line.Split(splitCharacters);
                switch(parameters[0])
                {
                    case "p": // point
                        throw new NotImplementedException();
                    case "v": // vertex
                        float x = float.Parse(parameters[1]);
                        float y = float.Parse(parameters[2]);
                        float z = float.Parse(parameters[3]);
                        vertices.Add(x);
                        vertices.Add(y);
                        vertices.Add(z);
                        break;
                    case "vt": // texCoord
                        throw new NotImplementedException();
                    case "vn": // normal
                        throw new NotImplementedException();
                    case "f":
                        switch(parameters.Length)
                        {
                            case 4:
                                ParseFace(parameters[1]);
                                ParseFace(parameters[2]);
                                ParseFace(parameters[3]);
                                break;
                            case 5:
                                throw new NotImplementedException();
                        }
                        break;
                }
            }

            ModelData model = new ModelData()
            {
                positions = vertices.ToArray(),
                normals = normals.ToArray(),
                textureCoordinates = texCoords.ToArray(),
                indices = indices.ToArray()
            };

            vertices.Clear();
            normals.Clear();
            texCoords.Clear();
            indices.Clear();

            return model;
        }

        private static void ParseFace(string faceParameter)
        {
            string[] parameters = faceParameter.Split(faceParamaterSplitter);
            int vertexIndex = int.Parse(parameters[0]);

            if(vertexIndex < 0)
            {
                vertexIndex = vertices.Count + vertexIndex;
            } else
            {
                vertexIndex = vertexIndex - 1;
            }
            indices.Add(vertexIndex);
        }
    }
}
