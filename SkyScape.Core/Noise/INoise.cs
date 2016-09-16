using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyScape.Core.Noise
{
    public interface INoise
    {
        float Noise(int x, int y, int z, float scale);
        float Noise(float x, float y, float scale);
        float Noise(float x, float y, float z, float scale);
        float Height(int x, int y);
    }
}
