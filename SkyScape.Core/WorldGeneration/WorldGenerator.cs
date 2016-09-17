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
        private INoise _heightNoise;
        private INoise _biomeNoise;

        public WorldGenerator(int seed)
        {
            _seed = seed;
            _random = new Random(seed);
            //_baseNoise = new NoiseGenerator(_random);
            _baseNoise = new SimplexNoiseGenerator(_random.Next());

            _heightNoise = new SimplexNoiseGenerator(_random.Next());

            _biomeNoise = new SimplexNoiseGenerator(_random.Next());
            
            //_baseNoise.Octaves = 8;
            //_baseNoise.Frequency = 0.015f;
        }

        public VoxelType Get(int x, int y, int z)
        {

            var maxHeight = (1.0 + _heightNoise.Noise(Math.Abs(x), Math.Abs(z), 0.0278f)) * 14 + 7;
            var floor = 0;

            if (y < floor)
            {
                if (y == floor - 1)
                    return Voxel.Types[Voxel.Rock];

                return Voxel.Types[Voxel.Empty];
            }

            if (y > maxHeight)
                return Voxel.Types[Voxel.Empty];

            var noise = (_baseNoise.Noise((float)x, (float)y, (float)z, 0.0467f) +
                _baseNoise.Noise((float)x, (float)y, (float)z, 0.1467f)) / 2f;

            if(noise < 0f)
                return Voxel.Types[Voxel.Empty];

            var type = _biomeNoise.Noise((float)x, (float)y, (float)z, 0.0032f);

            return type < 0f ? Voxel.Types[Voxel.Grass] : Voxel.Types[Voxel.Snow];
        }
    }
}
