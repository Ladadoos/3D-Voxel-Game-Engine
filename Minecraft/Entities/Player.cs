using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;

using Minecraft.World;
using Minecraft.Physics;
using Minecraft.World.Sections;
using Minecraft.Tools;
using Minecraft.World.Blocks;

namespace Minecraft.Entities
{
    class Player
    {
        public Camera camera;
        public Vector3 position;
        public Vector3 velocity;
        public float speedMultiplier = Constants.PLAYER_BASE_MOVE_SPEED * 35;

        public AABB hitbox;

        private float verticalSpeed = 0;
        private bool isInAir = true;

        private MouseRay mouseRay;

        public Player(Matrix4 proj)
        {
            camera = new Camera();
            position = new Vector3(Constants.CHUNK_SIZE / 2, 148, Constants.CHUNK_SIZE / 2);
            mouseRay = new MouseRay(camera, proj);

            float x1 = position.X + Constants.PLAYER_WIDTH;
            float y1 = position.Y + Constants.PLAYER_HEIGHT;
            float z1 = position.Z + Constants.PLAYER_LENGTH;
            hitbox = new AABB(position, new Vector3(x1, y1, z1));
        }

        public void Update(GameWindow window, WorldMap map, float time, Input input)
        {
            if (window.Focused) {
           
                if (Keyboard.GetState().IsKeyDown(Key.W)) {
                    AddForce(0.0F, 0.0F, 1.0F * speedMultiplier);
                }
                if (Keyboard.GetState().IsKeyDown(Key.S)) {
                    AddForce(0.0F, 0.0F, -1.0F * speedMultiplier);
                }
                if (Keyboard.GetState().IsKeyDown(Key.D)) {
                    AddForce(1.0F * speedMultiplier, 0.0F, 0.0F);
                }
                if (Keyboard.GetState().IsKeyDown(Key.A)) {
                    AddForce(-1.0F * speedMultiplier, 0.0F, 0.0F);
                }
                if (Keyboard.GetState().IsKeyDown(Key.Space)) {
                    AddForce(0.0F, 1.0F * speedMultiplier, 0.0F);
                    //Jump();
                }
                if (Keyboard.GetState().IsKeyDown(Key.ShiftLeft)) {
                    AddForce(0.0F, -1.0F * speedMultiplier, 0.0F);
                }

                Vector2 chunkPos = map.GetChunkPosition(position.X, position.Z);
                if (!map.chunks.ContainsKey(chunkPos) && Keyboard.GetState().IsKeyDown(Key.Z)) {
                    map.GenerateBlocksForChunk((int)chunkPos.X, (int)chunkPos.Y);
                }

                /*Vector2 chunkPos = Vector2.Zero;
                for (int i = -1; i < 3; i++)
                {
                  for(int j = -1; j < 3; j++)
                    {
                        float x = position.X + i * Constants.CHUNK_SIZE;
                        float z = position.Z + j * Constants.CHUNK_SIZE;
                        chunkPos = map.GetChunkPosition(x, z);
                        if (!map.chunks.ContainsKey(chunkPos))
                        {
                            map.GenerateBlocksForChunk((int)chunkPos.X, (int)chunkPos.Y);
                        }
                    }
                }*/

                sbyte h = (sbyte)(position.Y / Constants.CHUNK_SIZE);

                position.X += velocity.X * time;
                CalculateHitbox();

                Chunk chunk = null;
                Section section = null;

                map.chunks.TryGetValue(chunkPos, out chunk);
               /* if (chunk != null)
                {
                    if(h <= 15 || h >= 0)
                    {
                        section = chunk.sections[h];
                        if (section != null)
                        {
                            //foreach (KeyValuePair<Vector3, sbyte> block in section.blocks.ToArray())
                            //{
                            for (int x = 0; x < 16; x++)
                            {
                                for (int z = 0; z < 16; z++)
                                {
                                    for (int y = 0; y < 16; y++)
                                    {
                                        Vector3 pos = new Vector3(chunkPos.X * 16 + x, h - section.height)
                                        AABB blockAABB = Cube.GetAABB(pos.X + chunkPos.X * 16, pos.Y, pos.Z + chunkPos.Y * 16);
                                        if (hitbox.intersects(blockAABB) && block.Value != 0)
                                        {
                                            if (velocity.X > 0.0F)
                                            {
                                                position.X = blockAABB.min.X - Constants.PLAYER_WIDTH;
                                                velocity.X = 0.0F;
                                            }
                                            if (velocity.X < 0.0F)
                                            {
                                                position.X = blockAABB.max.X;
                                                velocity.X = 0.0F;
                                            }
                                        }
                                    }
                                }
                            }

                            //}
                        }
                    }
                }*/

                chunkPos = map.GetChunkPosition(position.X, position.Z);

                /*if (verticalSpeed > Constants.GRAVITY_THRESHOLD) {
                    verticalSpeed += Constants.GRAVITY * (float)time;
                } else {
                    verticalSpeed = Constants.GRAVITY_THRESHOLD;
                }*/
                AddForce(0.0F, verticalSpeed, 0.0F);

                bool collidedY = false;
                position.Y += velocity.Y * time;
                CalculateHitbox();
                chunk = null;
                h = (sbyte)(position.Y / Constants.CHUNK_SIZE);
                // blockAABB.min = Vector3.Zero; blockAABB.max = Vector3.Zero;
                map.chunks.TryGetValue(chunkPos, out chunk);
                // if (chunk != null) {
                //    section = null;
                //    chunk.sections.TryGetValue(h, out section);
                //    if (section != null) {
                //foreach (KeyValuePair<Vector3, sbyte> block in section.blocks.ToArray()) {
                //   Vector3 pos = block.Key;
                //  blockAABB.min.X = pos.X + chunkPos.X * 16;
                // blockAABB.min.Y = pos.Y;
                // blockAABB.min.Z = pos.Z + chunkPos.Y * 16;
                // blockAABB.max.X += blockAABB.min.X + Constants.CUBE_SIZE;
                // blockAABB.max.Y += blockAABB.min.Y + Constants.CUBE_SIZE;
                // blockAABB.max.Z += blockAABB.min.Z + Constants.CUBE_SIZE;
                /* AABB blockAABB = Cube.getAABB(pos.X + chunkPos.X * 16, pos.Y, pos.Z + chunkPos.Y * 16);
                 if (hitbox.intersects(blockAABB) && block.Value != 0) {
                     if (velocity.Y > 0.0F) {
                         position.Y = blockAABB.min.Y - Constants.PLAYER_HEIGHT;
                         velocity.Y = 0.0F;
                         verticalSpeed = 0.0F;
                     }
                     if (velocity.Y < 0.0F) {
                         position.Y = blockAABB.max.Y;
                         velocity.Y = 0.0F;
                         verticalSpeed = 0.0F;
                         collidedY = true;
                     }
                 }*/
                // }
                //  }
                // }
                isInAir = !collidedY;

                chunkPos = map.GetChunkPosition(position.X, position.Z);

                position.Z += velocity.Z * time;
                CalculateHitbox();
                chunk = null;
                h = (sbyte)(position.Y / Constants.CHUNK_SIZE);
                // blockAABB.min = Vector3.Zero; blockAABB.max = Vector3.Zero;
                map.chunks.TryGetValue(chunkPos, out chunk);
                // if (chunk != null) {
                //     section = null;
                //     chunk.sections.TryGetValue(h, out section);
                //     if(section != null) {
                //foreach (KeyValuePair<Vector3, sbyte> block in section.blocks.ToArray()) {
                //    Vector3 pos = block.Key;
                // blockAABB.min.X = pos.X + chunkPos.X * 16;
                // blockAABB.min.Y = pos.Y;
                /// blockAABB.min.Z = pos.Z + chunkPos.Y * 16;
                // blockAABB.max.X += blockAABB.min.X + Constants.CUBE_SIZE;
                // blockAABB.max.Y += blockAABB.min.Y + Constants.CUBE_SIZE;
                // blockAABB.max.Z += blockAABB.min.Z + Constants.CUBE_SIZE;
                /*AABB blockAABB = Cube.getAABB(pos.X + chunkPos.X * 16, pos.Y, pos.Z + chunkPos.Y * 16);
                if (hitbox.intersects(blockAABB) && block.Value != 0) {
                    if (velocity.Z > 0) {
                        position.Z = blockAABB.min.Z - Constants.PLAYER_LENGTH;
                        velocity.Z = 0;
                    }
                    if (velocity.Z < 0) {
                        position.Z = blockAABB.max.Z;
                        velocity.Z = 0;
                    }
                }*/
                // }
                //    }
                //  }


                velocity *= Constants.PLAYER_STOP_FORCE_MULTIPLIER;

                mouseRay.Update();
                if (input.OnMousePress(MouseButton.Right))
                {

                    //float x = Maths.round((camera.position.X + itMouse.ray.currentRay.X * 20) - Constants.HALF_CUBE_SIZE, 8);
                    // float y = Maths.round((camera.position.Y + itMouse.ray.currentRay.Y * 20) - Constants.HALF_CUBE_SIZE, 8);
                    //float z = Maths.round((camera.position.Z + itMouse.ray.currentRay.Z * 20) - Constants.HALF_CUBE_SIZE, 8);
                    int offset = 2;
                    int x = (int)(camera.position.X + mouseRay.ray.currentRay.X * offset);
                    int y = (int)(camera.position.Y + mouseRay.ray.currentRay.Y * offset);
                    int z = (int)(camera.position.Z + mouseRay.ray.currentRay.Z * offset);
                    
                    map.AddBlockToWorld(x, y, z, BlockType.Cobblestone);
                }
                if (input.OnMousePress(MouseButton.Left))
                {

                    //float x = Maths.round((camera.position.X + itMouse.ray.currentRay.X * 20) - Constants.HALF_CUBE_SIZE, 8);
                    // float y = Maths.round((camera.position.Y + itMouse.ray.currentRay.Y * 20) - Constants.HALF_CUBE_SIZE, 8);
                    //float z = Maths.round((camera.position.Z + itMouse.ray.currentRay.Z * 20) - Constants.HALF_CUBE_SIZE, 8);
                    int offset = 2;
                    int x = (int)(camera.position.X + mouseRay.ray.currentRay.X * offset);
                    int y = (int)(camera.position.Y + mouseRay.ray.currentRay.Y * offset);
                    int z = (int)(camera.position.Z + mouseRay.ray.currentRay.Z * offset);
                    
                    map.AddBlockToWorld(x, y, z, BlockType.Air);
                }

                camera.SetPosition(position);
                camera.Rotate();
                camera.ResetCursor(window.Bounds);
            }
        }

        private void AddForce(float x, float y, float z)
        {
            Vector3 offset = new Vector3();
            Vector3 forward = new Vector3((float)Math.Sin(camera.orientation.X), 0, (float)Math.Cos(camera.orientation.X));
            Vector3 right = new Vector3(-forward.Z, 0, forward.X);

            offset += x * right;
            offset += z * forward;
            offset.Y += y;

           // offset.X *= Constants.PLAYER_BASE_MOVE_SPEED;
           // offset.Y *= Constants.PLAYER_BASE_MOVE_SPEED;
            //offset.Z *= Constants.PLAYER_BASE_MOVE_SPEED;

            offset.X *= 1.0F;
            offset.Y *= 1.0F;
            offset.Z *= 1.0F;

            velocity += offset;
        }

        private void Jump()
        {
            if (!isInAir) {
                verticalSpeed = Constants.PLAYER_JUMP_FORCE;
                isInAir = true;
            }
        }

        private void CalculateHitbox()
        {
            float x1 = position.X + Constants.PLAYER_WIDTH / 2.0F;
            float y1 = position.Y + Constants.PLAYER_HEIGHT;
            float z1 = position.Z + Constants.PLAYER_LENGTH / 2.0F;
            hitbox.setHitbox(position, new Vector3(x1, y1, z1));
        }

        /*private List<Vector3> GetCollisionDetectionBlockPositions(WorldMap map)
        {
            List<Vector3> blockPositions = new List<Vector3>();

            Vector2 playerPositionInChunk = map.GetChunkPosition(position.X, position.Z);

            List<Chunk> chunks = new List<Chunk>();
            Chunk chunk = null;

            Vector2 surrounding = playerPositionInChunk;
            map.chunks.TryGetValue(surrounding, out chunk);
            if(chunk != null)
            {
                chunks.Add(chunk);
                chunk = null;
            }

            surrounding.X += 1;
            map.chunks.TryGetValue(surrounding, out chunk);
            if (chunk != null)
            {
                chunks.Add(chunk);
                chunk = null;
            }
            surrounding = playerPositionInChunk;

            surrounding.X -= 1;
            map.chunks.TryGetValue(surrounding, out chunk);
            if (chunk != null)
            {
                chunks.Add(chunk);
                chunk = null;
            }
            surrounding = playerPositionInChunk;

            surrounding.Y += 1;
            map.chunks.TryGetValue(surrounding, out chunk);
            if (chunk != null)
            {
                chunks.Add(chunk);
                chunk = null;
            }
            surrounding = playerPositionInChunk;

            surrounding.Y -= 1;
            map.chunks.TryGetValue(surrounding, out chunk);
            if (chunk != null)
            {
                chunks.Add(chunk);
                chunk = null;
            }
            surrounding = playerPositionInChunk;

            surrounding.Y -= 1;
            surrounding.X -= 1;
            map.chunks.TryGetValue(surrounding, out chunk);
            if (chunk != null)
            {
                chunks.Add(chunk);
                chunk = null;
            }
            surrounding = playerPositionInChunk;

            surrounding.Y -= 1;
            surrounding.X += 1;
            map.chunks.TryGetValue(surrounding, out chunk);
            if (chunk != null)
            {
                chunks.Add(chunk);
                chunk = null;
            }
            surrounding = playerPositionInChunk;

            surrounding.Y += 1;
            surrounding.X -= 1;
            map.chunks.TryGetValue(surrounding, out chunk);
            if (chunk != null)
            {
                chunks.Add(chunk);
                chunk = null;
            }
            surrounding = playerPositionInChunk;

            surrounding.Y += 1;
            surrounding.X += 1;
            map.chunks.TryGetValue(surrounding, out chunk);
            if (chunk != null)
            {
                chunks.Add(chunk);
                chunk = null;
            }
            surrounding = playerPositionInChunk;

            return blockPositions;
        }*/

        private double GetLength(int x, int y)
        {
            return Math.Sqrt(x * x + y * y);
        }

    }
}
