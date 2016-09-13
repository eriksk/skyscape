using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyScape.Core
{
    public static class VectorExtensions
    {
        public static Vector3 Normalized(this Vector3 vector)
        {
            vector.Normalize();
            return vector;
        }
    }
}
