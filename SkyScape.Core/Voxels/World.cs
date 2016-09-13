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
        public static int ChunkSize = 16;
        public static bool UseMultiThreading = false;

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

            var chunkPosition = GetChunkPositionFromWorldPosition(new VoxelPosition(worldX, worldY, worldZ));

            var chunk = GetOrCreateChunk(chunkPosition.X, chunkPosition.Y, chunkPosition.Z);

            var local = WorldToLocal(new VoxelPosition(worldX, worldY, worldZ), chunk);

            return chunk.Get(local.X, local.Y, local.Z);
        }

        public void Set(int worldX, int worldY, int worldZ, int value)
        {
            var chunkPosition = GetChunkPositionFromWorldPosition(new VoxelPosition(worldX, worldY, worldZ));

            var chunk = GetOrCreateChunk(chunkPosition.X, chunkPosition.Y, chunkPosition.Z);

            var local = WorldToLocal(new VoxelPosition(worldX, worldY, worldZ), chunk);

            chunk.Set(local.X, local.Y, local.Z, value);
        }

        public VoxelPosition GetChunkPositionFromWorldPosition(VoxelPosition worldPosition)
        {
            int chunkX = worldPosition.X / ChunkSize;
            int chunkY = worldPosition.Y / ChunkSize;
            int chunkZ = worldPosition.Z / ChunkSize;

            if (worldPosition.X < 0)
                chunkX -= 1;
            if (worldPosition.Y < 0)
                chunkY -= 1;
            if (worldPosition.Z < 0)
                chunkZ -= 1;

            return new VoxelPosition(chunkX, chunkY, chunkZ);
        }

        public VoxelPosition WorldToLocal(VoxelPosition worldPosition, Chunk relativeToChunk)
        {
            var localX = worldPosition.X - (relativeToChunk.X * ChunkSize);
            var localY = worldPosition.Y - (relativeToChunk.Y * ChunkSize);
            var localZ = worldPosition.Z - (relativeToChunk.Z * ChunkSize);

            if (relativeToChunk.X < 0)
                localX--;
            if (relativeToChunk.Y < 0)
                localY--;
            if (relativeToChunk.Z < 0)
                localZ--;

            return new VoxelPosition(localX, localY, localZ);
        }

        public Chunk GetOrCreateChunk(int chunkX, int chunkY, int chunkZ)
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

        /// <summary>
        /// Gets a mask with info about surrounding voxels
        /// VoxelMask describes the sides that DO NOT have anything blocking them
        /// i.e the faces that are visible should be rendered
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public VoxelMask GetMask(VoxelPosition worldPosition)
        {
            var mask = VoxelMask.None;

            if(Get(worldPosition.X + 1, worldPosition.Y, worldPosition.Z) == Voxel.Empty)
                mask |= VoxelMask.Right;
            if (Get(worldPosition.X - 1, worldPosition.Y, worldPosition.Z) == Voxel.Empty)
                mask |= VoxelMask.Left;
            if (Get(worldPosition.X, worldPosition.Y + 1, worldPosition.Z) == Voxel.Empty)
                mask |= VoxelMask.Up;
            if (Get(worldPosition.X, worldPosition.Y - 1, worldPosition.Z) == Voxel.Empty)
                mask |= VoxelMask.Down;
            if (Get(worldPosition.X, worldPosition.Y, worldPosition.Z + 1) == Voxel.Empty)
                mask |= VoxelMask.Forward;
            if (Get(worldPosition.X, worldPosition.Y, worldPosition.Z - 1) == Voxel.Empty)
                mask |= VoxelMask.Back;

            return mask;
        }

        public void Clear()
        {
            lock (_chunksLock)
            {
                foreach (var chunk in _chunks.Values)
                    chunk.Clear();
                _chunks.Clear();
            }
        }
    }
}
