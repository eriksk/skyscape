﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkyScape.Core.Voxels.Threading
{
    public class ChunkGenerationBatcher
    {
        public static float ConsumeInterval = 19f; // TODO: by timespan?
        public static int MaxToConsumeEachTime = 4;

        public static bool PerformaceTracking = true;

        private float _currentConsumeTime = ConsumeInterval;

        private List<ChunkGenerationWork> _jobs;

        public ChunkGenerationBatcher()
        {
            _jobs = new List<ChunkGenerationWork>();
        }

        public int ActiveJobs => _jobs.Count(x => x.Started && !x.Done);
        public int JobsAboutToBeConsumed => _jobs.Count(x => x.Started && x.Done && !x.Consumed);
        public int JobsInQueue => _jobs.Count(x => !x.Started);

        public void Clear()
        {
            foreach (var job in _jobs)
            {
                job.Kill();
            }
            _jobs.Clear();
        }

        public bool HasWork(Chunk chunk)
        {
            return _jobs.Any(x => x.Chunk == chunk);
        }

        public void CancelWork(Chunk chunk)
        {
            foreach (var job in _jobs.Where(x => x.Chunk == chunk).ToArray())
                job.Kill();
            _jobs.RemoveAll(x => x.Chunk == chunk);
        }

        public bool QueueWork(World world, Chunk chunk, Action<World> work, Action<GraphicsDevice> mainThreadWork)
        {
            if (HasWork(chunk)) return false;

            _jobs.Add(new ChunkGenerationWork(world, chunk, work, mainThreadWork));
            return true;
        }

        public void Update(float dt)
        {
            _currentConsumeTime += dt;
            _jobs.RemoveAll(x => x.Done && x.Consumed);
            int working = _jobs.Count(x => x.Started);

            var jobsAvailable = _jobs.Where(x => !x.Started);

            foreach (var job in jobsAvailable)
            {
                job.Start();
            }
        }

        public void Consume(GraphicsDevice graphics, Vector3 position)
        {
            if (_currentConsumeTime < ConsumeInterval) return;
            _currentConsumeTime = 0f;

            foreach (var job in _jobs
                .Where(x => x.Started && x.Done && !x.Consumed)
                .OrderByDescending(x => Vector3.Distance(x.Chunk.Center, position))
                .Take(MaxToConsumeEachTime).ToArray())
                job.ConsumeOnMainThread(graphics);
        }
    }

    public class ChunkGeneratorPerformanceTracker
    {
        private static List<float> _generations = new List<float>();
        private static object _lock = new object();

        public static void Log(float duration)
        {
            lock (_lock)
            {
                _generations.Add(duration);
            }
        }

        public static float Mean
        {
            get
            {
                lock (_lock)
                {
                    return _generations.Count > 0 ? _generations.Sum() / (float)_generations.Count : 0;
                }
            }
        }
        public static float Min
        {
            get
            {
                lock (_lock)
                {
                    return _generations.Count > 0 ? _generations.Min() : 0;
                }
            }
        }
        public static float Max
        {
            get
            {
                lock (_lock)
                {
                    return _generations.Count > 0 ? _generations.Max() : 0;
                }
            }
        }
        public static float Median
        {
            get
            {
                lock (_lock)
                {
                    return _generations.Count > 2 ? _generations.OrderBy(x => x).ToArray()[_generations.Count / 2] : 0f;
                }
            }
        }
    }

    public class ChunkGenerationWork
    {
        public Chunk Chunk { get; }
        public World World { get; }
        //private Thread _thread { get; }
        private Action<World> _work { get; }
        private Action<GraphicsDevice> _mainThreadWork { get; }

        public bool Done { get; private set; }
        public bool Started { get; private set; }
        public bool Consumed { get; private set; }

        public ChunkGenerationWork(World world, Chunk chunk, Action<World> work, Action<GraphicsDevice> mainThreadWork)
        {
            Chunk = chunk;
            World = world;
            _work = work;
            _mainThreadWork = mainThreadWork;

        }

        public void Start()
        {
            if (Started) return;

            Started = true;

            ThreadPool.QueueUserWorkItem((f) =>
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                _work(World);
                stopwatch.Stop();
                if (ChunkGenerationBatcher.PerformaceTracking)
                {
                    ChunkGeneratorPerformanceTracker.Log(stopwatch.ElapsedMilliseconds);
                }
                Done = true;
            }, null);
        }

        public void ConsumeOnMainThread(GraphicsDevice graphics)
        {
            if (!Started) return;
            if (!Done) return;
            _mainThreadWork(graphics);
            Consumed = true;
        }

        public void Kill()
        {
            //_thread.Abort();
        }
    }
}
