using Microsoft.Xna.Framework.Graphics;
using SkyScape.Core.Cameras;
using SkyScape.Core.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using SkyScape.Core.Voxels.Threading;

namespace SkyScape.Core.Voxels
{
    public class World
    {
        public static int ChunkSize = (int)Math.Pow(2, Chunk.LogSize);
        public static bool UseMultiThreading = true;
        public static float ViewDistance = 1000;

        private Dictionary<VoxelPosition, Chunk> _chunks;

        private ChunkGenerationBatcher _generationBatcher;

        public World()
        {
            _chunks = new Dictionary<VoxelPosition, Chunk>();
            _generationBatcher = new ChunkGenerationBatcher();
        }

        public ChunkGenerationBatcher Batcher => _generationBatcher;

        public void SetFromPosition(Vector3 worldPosition, int value)
        {
            var position = GetVoxelPosition(worldPosition);
            Set(position, value);
        }

        public VoxelPosition GetVoxelPosition(Vector3 worldPosition)
        {
            var position = new VoxelPosition((int)worldPosition.X, (int)worldPosition.Y, (int)worldPosition.Z);

            int chunkX = (position.X >> Chunk.LogSize);
            int chunkY = (position.Y >> Chunk.LogSize);
            int chunkZ = (position.Z >> Chunk.LogSize);

            int localX = position.X & Chunk.Mask;
            int localY = position.Y & Chunk.Mask;
            int localZ = position.Z & Chunk.Mask;


            return new VoxelPosition((chunkX * ChunkSize) + localX, (chunkY * ChunkSize) + localY, (chunkZ * ChunkSize) + localZ);
        }

        public VoxelPosition GetChunkPosition(Vector3 worldPosition)
        {
            var position = new VoxelPosition((int)worldPosition.X, (int)worldPosition.Y, (int)worldPosition.Z);

            int chunkX = (position.X >> Chunk.LogSize);
            int chunkY = (position.Y >> Chunk.LogSize);
            int chunkZ = (position.Z >> Chunk.LogSize);

            return new VoxelPosition(chunkX, chunkY, chunkZ);
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
            var chunkPosition = GetChunkPositionFromWorldPosition(new VoxelPosition(worldX, worldY, worldZ));
            var chunk = GetChunk(chunkPosition.X, chunkPosition.Y, chunkPosition.Z);
            if (chunk == null) return Voxel.Empty;
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
            int chunkX = (worldPosition.X >> Chunk.LogSize);
            int chunkY = (worldPosition.Y >> Chunk.LogSize);
            int chunkZ = (worldPosition.Z >> Chunk.LogSize);

            return new VoxelPosition(chunkX, chunkY, chunkZ);
        }

        public VoxelPosition WorldToLocal(VoxelPosition worldPosition, Chunk relativeToChunk)
        {
            int localX = worldPosition.X & Chunk.Mask;
            int localY = worldPosition.Y & Chunk.Mask;
            int localZ = worldPosition.Z & Chunk.Mask;

            return new VoxelPosition(localX, localY, localZ);
        }

        public Chunk GetChunk(int chunkX, int chunkY, int chunkZ)
        {
            var chunkPosition = new VoxelPosition(chunkX, chunkY, chunkZ);
            var chunk = _chunks.ContainsKey(chunkPosition) ? _chunks[chunkPosition] : null;
            return chunk;
        }
        public Chunk GetOrCreateChunk(int chunkX, int chunkY, int chunkZ)
        {
            var chunkPosition = new VoxelPosition(chunkX, chunkY, chunkZ);
            var chunk = _chunks.ContainsKey(chunkPosition) ? _chunks[chunkPosition] : null;
            if (chunk != null) return chunk;
            chunk = new Chunk(chunkX, chunkY, chunkZ);
            _chunks.Add(chunkPosition, chunk);
            return chunk;
        }

        public void Clean(GraphicsDevice graphics)
        {
            foreach (var chunk in _chunks.Values.ToArray())
            {
                if (chunk.Dirty)
                    chunk.Clean(graphics, this);
            }
        }

        public void Update(float dt)
        {
            _generationBatcher.Update(dt);
        }

        public void Render(GraphicsDevice graphics, Camera cam, StandardEffect effect)
        {
            _generationBatcher.Consume(graphics);

            foreach (var chunk in _chunks.Values)
            {
                if (Vector3.Distance(cam.Transform.Position, chunk.Center) > ViewDistance) continue;

                if (chunk.Dirty)
                    chunk.Clean(graphics, this);

                effect.ApplyForModel(chunk.View);
                chunk.Render(graphics, effect);
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
            _generationBatcher.Clear();
            foreach (var chunk in _chunks.Values)
                chunk.Clear();
            _chunks = new Dictionary<VoxelPosition, Chunk>();
        }
    }
}
