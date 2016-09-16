using SkyScape.Core.Noise;
using SkyScape.Core.Voxels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyScape.Core.WorldGeneration
{
    public class WorldGenerator
    {
        private int _seed;
        private Random _random;

        private INoise _baseNoise;
        private int _baseNoiseSeed;

        public WorldGenerator(int seed)
        {
            _seed = seed;
            _random = new Random(seed);
            //_baseNoise = new NoiseGenerator(_random);
            _baseNoise = new SimplexNoiseGenerator();
            SimplexNoiseGenerator.Seed = _random.Next();
            //_baseNoise.Octaves = 8;
            //_baseNoise.Frequency = 0.015f;
        }

        public int Seed => _seed;

        public VoxelType Get(int x, int y, int z)
        {

            var maxHeight = 64;
            if (y > maxHeight)
                return Voxel.Types[Voxel.Empty];

            var noise = _baseNoise.Noise((float)x, (float)y, (float)z, 0.05f);
            

            if(noise < 0f)
                return Voxel.Types[Voxel.Empty];

            return Voxel.Types[Voxel.Grass]; //Voxel.Types[_random.Next(1, Voxel.Types.Length)];
        }
    }
}
