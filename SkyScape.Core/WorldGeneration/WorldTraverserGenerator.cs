using SkyScape.Core.Components;
using SkyScape.Core.Voxels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyScape.Core.WorldGeneration
{
    public class WorldTraverserGenerator
    {
        private WorldGenerator _generator;
        private World _world;
        private HashSet<VoxelPosition> _surroundingChunks;

        public Transform Target { get; set; }

        public WorldTraverserGenerator(World world, WorldGenerator generator)
        {
            _generator = generator;
            _world = world;
            _surroundingChunks = new HashSet<VoxelPosition>();
        }

        public void Update(float dt)
        {
            if (Target == null) throw new Exception("No target to follow!");
            UpdateSurroundingChunks();
        }

        private void UpdateSurroundingChunks()
        {
            var currentChunk = _world.GetChunkPosition(Target.Position);
            var nearbyChunks = GetNearbyChunks(currentChunk, 2, 4, 2);

            var addedChunks = new List<VoxelPosition>();
            foreach (var chunk in nearbyChunks)
            {
                if (_surroundingChunks.Contains(chunk))
                {
                    // do nothing, already exist
                }
                else
                {
                    addedChunks.Add(chunk);
                }
            }

            var removedChunks = new List<VoxelPosition>();
            foreach (var chunk in _surroundingChunks)
            {
                if (!nearbyChunks.Contains(chunk))
                    removedChunks.Add(chunk);
            }

            RemoveChunks(removedChunks);
            AddChunks(addedChunks);
        }

        private void AddChunks(List<VoxelPosition> chunks)
        {
            foreach (var chunkPosition in chunks)
            {
                _surroundingChunks.Add(chunkPosition);
                if (_world.ChunkExist(chunkPosition)) continue;
                var chunk = _world.GetOrCreateChunk(chunkPosition);

                for (int x = 0; x < World.ChunkSize; x++)
                {
                    for (int y = 0; y < World.ChunkSize; y++)
                    {
                        for (int z = 0; z < World.ChunkSize; z++)
                        {
                            chunk.Set(x, y, z, _generator.Get((chunk.X * World.ChunkSize) + x, (chunk.Y * World.ChunkSize) + y, (chunk.Y * World.ChunkSize) + y).Id);
                        }
                    }
                }                
            }
        }

        private void RemoveChunks(List<VoxelPosition> chunks)
        {
            foreach (var chunk in chunks)
            {
                _surroundingChunks.Remove(chunk);
                //_world.RemoveChunk(chunk);
            }
        }

        private VoxelPosition[] GetNearbyChunks(VoxelPosition currentChunk, int rangeX, int rangeY, int rangeZ)
        {
            var chunks = new List<VoxelPosition>();

            for (int x = -rangeX; x <= rangeX; x++)
            {
                for (int y = -rangeY; y <= rangeY; y++)
                {
                    for (int z = -rangeZ; z <= rangeZ; z++)
                    {
                        chunks.Add(new VoxelPosition(currentChunk.X + x, currentChunk.Y + y, currentChunk.Z + z));
                    }
                }
            }

            return chunks.ToArray();
        }
    }
}
