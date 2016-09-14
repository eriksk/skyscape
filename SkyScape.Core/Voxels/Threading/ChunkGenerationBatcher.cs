using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkyScape.Core.Voxels.Threading
{
    public class ChunkGenerationBatcher
    {
        public static int MaxConcurrent = 12;
        public static float ConsumeInterval = 100f; // TODO: by timespan?
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
            int countOfThreadsCanBeStarted = MaxConcurrent - working;
            if (countOfThreadsCanBeStarted <= 0) return;

            var jobsAvailable = _jobs.Where(x => !x.Started);

            foreach (var job in jobsAvailable)
            {
                job.Start();
                countOfThreadsCanBeStarted--;
                if (countOfThreadsCanBeStarted <= 0)
                    break;
            }
        }

        public void Consume(GraphicsDevice graphics)
        {
            if (_currentConsumeTime < ConsumeInterval) return;
            _currentConsumeTime = 0f;

            foreach (var job in _jobs.Where(x => x.Started && x.Done && !x.Consumed))
                job.ConsumeOnMainThread(graphics);
        }
    }

    public class ChunkGenerationWork
    {
        public Chunk Chunk { get; }
        public World World { get; }
        private Thread _thread { get; }
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

            _thread = new Thread(() => 
            {
                try
                {
                    _work(World);
                }
                catch (ThreadAbortException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                Done = true;
            });
        }

        public void Start()
        {
            if (Started) return;

            Started = true;
            _thread.Start();
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
            _thread.Abort();
        }
    }
}
