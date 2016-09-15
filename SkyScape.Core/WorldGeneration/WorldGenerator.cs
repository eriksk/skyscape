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

        private NoiseGenerator _baseNoise;

        public WorldGenerator(int seed)
        {
            _seed = seed;
            _random = new Random(seed);
            _baseNoise = new NoiseGenerator(_random);
            //_baseNoise.Octaves = 8;
            //_baseNoise.Frequency = 0.015f;
        }

        public int Seed => _seed;

        public VoxelType Get(int x, int y, int z)
        {
            // the perlin is really slow...
            var perlin = 0.5f + _baseNoise.Noise(x, z);
            int height = (int)(perlin * 32 * 1f);

            if (y > height)
                return Voxel.Types[Voxel.Empty];
            if (y < 0)
                return Voxel.Types[Voxel.Empty];

            return Voxel.Types[Voxel.Grass];
        }
    }
}
