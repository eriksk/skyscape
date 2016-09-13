using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyScape.Core.Meshes
{
    public class MeshData
    {
        public List<Vector3> Vertices;
        public List<Vector3> Normals;
        public List<Color> Colors;
        public List<int> Indices;
        public List<Vector2> Uvs;
        public VertexDeclaration VertexDeclaration;
        public PrimitiveType PrimitiveType = PrimitiveType.TriangleList;

        public MeshData()
        {
            Vertices = new List<Vector3>();
            Normals = new List<Vector3>();
            Colors = new List<Color>();
            Uvs = new List<Vector2>();
            Indices = new List<int>();
        }

        public int Triangles => Indices.Count / 3;
    }
}
