using Microsoft.Xna.Framework.Graphics;
using SkyScape.Core.Cameras;
using SkyScape.Core.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;

namespace SkyScape.Core.Voxels
{
    public class World
    {
        public const int ChunkSize = 64;

        private Dictionary<VoxelPosition, Chunk> _chunks;
        private object _chunksLock = new object();

        public World()
        {
            _chunks = new Dictionary<VoxelPosition, Chunk>();
        }

        public void SetFromPosition(Vector3 worldPosition, int rock)
        {
            Set(GetVoxelPosition(worldPosition), rock);
        }

        public VoxelPosition GetVoxelPosition(Vector3 worldPosition)
        {
            return new VoxelPosition((int)worldPosition.X, (int)worldPosition.Y, (int)worldPosition.Z);
        }

        public int Get(VoxelPosition worldPosition)
        {
            return Get(worldPosition.X, worldPosition.Y, worldPosition.Z);
        }
        public void Set(VoxelPosition worldPosition, int value)
        {
            Set(worldPosition.X, worldPosition.Y, worldPosition.Z, value);
        }

        public int Get(int worldX, int worldY, int worldZ)
        {
            // http://gamedev.stackexchange.com/questions/65800/when-storing-voxels-in-chunks-how-do-i-access-them-at-the-world-level

            //if (worldX < 0) return Voxel.Empty; // TODO: handle negative cells
            //if (worldY < 0) return Voxel.Empty; // TODO: handle negative cells
            //if (worldZ < 0) return Voxel.Empty; // TODO: handle negative cells


            int chunkX = worldX / ChunkSize;
            int chunkY = worldY / ChunkSize;
            int chunkZ = worldZ / ChunkSize;

            var chunk = GetOrCreateChunk(chunkX, chunkY, chunkZ);

            var localX = worldX % ChunkSize; //worldX - (chunkX * ChunkSize);
            var localY = worldY % ChunkSize; //worldY - (chunkY * ChunkSize);
            var localZ = worldZ % ChunkSize; //worldZ - (chunkZ * ChunkSize);

            return chunk.Get(localX, localY, localZ);
        }

        public void Set(int worldX, int worldY, int worldZ, int value)
        {
            //if (worldX < 0) throw new NotImplementedException("Negative cells");
            //if (worldY < 0) throw new NotImplementedException("Negative cells");
            //if (worldZ < 0) throw new NotImplementedException("Negative cells");

            int chunkX = worldX / ChunkSize;
            int chunkY = worldY / ChunkSize;
            int chunkZ = worldZ / ChunkSize;

            //int chunkX = (int)Math.Floor(worldX / (float)ChunkSize);
            //int chunkY = (int)Math.Floor(worldY / (float)ChunkSize);
            //int chunkZ = (int)Math.Floor(worldZ / (float)ChunkSize);

            var chunk = GetOrCreateChunk(chunkX, chunkY, chunkZ);

            var localX = worldX % ChunkSize; //worldX - (chunkX * ChunkSize);
            var localY = worldY % ChunkSize; //worldY - (chunkY * ChunkSize);
            var localZ = worldZ % ChunkSize; //worldZ - (chunkZ * ChunkSize);

            chunk.Set(localX, localY, localZ, value);
        }

        private Chunk GetOrCreateChunk(int chunkX, int chunkY, int chunkZ)
        {
            lock (_chunksLock)
            {
                var chunkPosition = new VoxelPosition(chunkX, chunkY, chunkZ);

                var chunk = _chunks.ContainsKey(chunkPosition) ? _chunks[chunkPosition] : null;

                if (chunk != null) return chunk;

                chunk = new Chunk(chunkX, chunkY, chunkZ);
                _chunks.Add(chunkPosition, chunk);
                return chunk;
            }
        }

        public void Clean(GraphicsDevice graphics)
        {
            lock (_chunksLock)
            {
                foreach (var chunk in _chunks.Values.ToArray())
                {
                    if (chunk.Dirty)
                        chunk.Clean(graphics, this);
                }
                foreach (var chunk in _chunks.Values)
                {
                    if (chunk.Dirty)
                        chunk.Clean(graphics, this);
                }
            }
        }

        public void Render(GraphicsDevice graphics, Camera cam, StandardEffect effect)
        {
            lock (_chunksLock)
            {
                foreach (var chunk in _chunks.Values.ToArray())
                {
                    if (chunk.Dirty)
                        chunk.Clean(graphics, this);
                }
                foreach (var chunk in _chunks.Values)
                {
                    effect.World = chunk.View;
                    chunk.Render(graphics, effect);
                }
            }
        }
    }
}
