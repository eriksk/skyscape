using SkyScape.Core.Components;
using SkyScape.Core.Voxels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkyScape.Core.WorldGeneration
{
    public class WorldTraverserGenerator
    {
        private WorldGenerator _generator;
        private World _world;
        private HashSet<VoxelPosition> _surroundingChunks;
        private ChunkDataGenerationBatcher _batcher;

        public Transform Target { get; set; }

        public WorldTraverserGenerator(World world, WorldGenerator generator)
        {
            _generator = generator;
            _world = world;
            _surroundingChunks = new HashSet<VoxelPosition>();
            _batcher = new ChunkDataGenerationBatcher(generator);
        }

        public void Update(float dt)
        {
            if (Target == null) throw new Exception("No target to follow!");
            _batcher.Update(dt);
            UpdateSurroundingChunks();
        }

        private void UpdateSurroundingChunks()
        {
            var currentChunk = _world.GetChunkPosition(Target.Position);
            var nearbyChunks = GetNearbyChunks(currentChunk, 1, 2, 1);

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

                //GenerateForChunk(chunk, _generator);
                _batcher.AddJob(chunk, GenerateForChunk);              
            }
        }

        private void GenerateForChunk(Chunk chunk, WorldGenerator generator)
        {
            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int y = 0; y < World.ChunkSize; y++)
                {
                    for (int z = 0; z < World.ChunkSize; z++)
                    {
                        chunk.Set(x, y, z, generator.Get((chunk.X * World.ChunkSize) + x, (chunk.Y * World.ChunkSize) + y, (chunk.Z * World.ChunkSize) + z).Id);
                    }
                }
            }
            chunk.MarkAsReadyToGenerate();
        }

        private void RemoveChunks(List<VoxelPosition> chunks)
        {
            foreach (var chunk in chunks)
            {
                _surroundingChunks.Remove(chunk);
                _batcher.KillJobsFor(chunk);
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

    public class ChunkDataGenerationBatcher
    {
        private List<ChunkDataGenerationJob> _jobs;
        public static int MaxConcurrent = 4;

        private WorldGenerator _generator;

        public ChunkDataGenerationBatcher(WorldGenerator generator)
        {
            _generator = generator;
            _jobs = new List<ChunkDataGenerationJob>();
        }

        public void AddJob(Chunk chunk, Action<Chunk, WorldGenerator> generateAction)
        {
            if (_jobs.Any(x => x.Chunk == chunk)) return;

            _jobs.Add(new ChunkDataGenerationJob(chunk, _generator, generateAction));
        }

        public void KillJobsFor(VoxelPosition chunk)
        {
            var jobs = _jobs.Where(x => new VoxelPosition(x.Chunk.X, x.Chunk.Y, x.Chunk.Z).GetHashCode() == chunk.GetHashCode()).ToArray();
            foreach (var job in jobs)
                job.Kill();
            _jobs.RemoveAll(x => new VoxelPosition(x.Chunk.X, x.Chunk.Y, x.Chunk.Z).GetHashCode() == chunk.GetHashCode());
        }

        public void Update(float dt)
        {
            _jobs.RemoveAll(x => x.Done);
            int working = _jobs.Count(x => x.Started);
            int countOfThreadsCanBeStarted = MaxConcurrent - working;
            if (countOfThreadsCanBeStarted <= 0) return;

            var jobsAvailable = _jobs.Where(x => !x.Started).Take(countOfThreadsCanBeStarted);

            foreach (var job in jobsAvailable)
            {
                job.Start();
            }
        }
    }

    public class ChunkDataGenerationJob
    {
        //private Thread _thread;

        public bool Done { get; private set; }
        public bool Started { get; private set; }

        public Chunk Chunk { get; set; }

        private Action<Chunk, WorldGenerator> _generateAction;
        private WorldGenerator _generator;

        public ChunkDataGenerationJob(Chunk chunk, WorldGenerator generator, Action<Chunk, WorldGenerator> generateAction)
        {
            Chunk = chunk;
            _generateAction = generateAction;
            _generator = generator;

        }

        public void Start()
        {
            if (Started) return;

            Started = true;
            ThreadPool.QueueUserWorkItem((f) =>
            {
                _generateAction(Chunk, _generator);
                Done = true;
            }, null);
        }

        public void Kill()
        {
            //_thread.Abort();
        }
    }
}
