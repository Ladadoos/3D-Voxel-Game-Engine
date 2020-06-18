using OpenTK;
using System.Collections.Generic;

namespace Minecraft
{
    class EntityMeshGenerator
    {
        protected List<float> vertexPositions = new List<float>();
        protected List<float> textureUVs = new List<float>();
        protected List<float> illumations = new List<float>();
        protected List<float> normals = new List<float>();
        protected int indicesCount;

        protected EntityModelRegistry entityModelRegistry;

        public EntityMeshGenerator(EntityModelRegistry entityModelRegistry)
        {
            this.entityModelRegistry = entityModelRegistry;
        }

        protected void ClearData()
        {
            vertexPositions.Clear();
            textureUVs.Clear();
            illumations.Clear();
            normals.Clear();
            indicesCount = 0;
        }

        public VAOModel GenerateMeshFor(EntityModel entityModel)
        {
            foreach (BlockFace face in entityModel.EntityFaces)
            {
                foreach(Vector2 uv in face.TextureCoords)
                {
                    textureUVs.Add(uv.X);
                    textureUVs.Add(uv.Y);
                }

                foreach (Vector3 modelSpacePosition in face.Positions)
                {
                    vertexPositions.Add(modelSpacePosition.X);
                    vertexPositions.Add(modelSpacePosition.Y);
                    vertexPositions.Add(modelSpacePosition.Z);
                }

                indicesCount += 4;
                for(int i = 0; i < face.Positions.Length; i++)
                {
                    illumations.Add(1.0F);
                }
                
                for (int i = 0; i < 4; i++)
                {
                    normals.Add(face.Normal.X);
                    normals.Add(face.Normal.Y);
                    normals.Add(face.Normal.Z);
                }
            }

            VAOModel chunkModel = new VAOModel(vertexPositions.ToArray(), textureUVs.ToArray(), illumations.ToArray(), normals.ToArray(), indicesCount);
            ClearData();
            return chunkModel;
        }
    }
}
