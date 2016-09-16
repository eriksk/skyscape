using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkyScape.Core.CustomVertexTypes;
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
            Vertices = new List<Vector3>(1024);
            Normals = new List<Vector3>(1024);
            Colors = new List<Color>(1024);
            Uvs = new List<Vector2>(1024);
            Indices = new List<int>(1024);
        }

        public VertexPositionColorNormal[] VertexPositionColorNormals { get; private set; }
        public VertexPositionNormalTexture[] VertexPositionNormalTextures { get; private set; }
        public int[] IndicesArray { get; private set; }

        public bool UsesColor { get; private set; }
        public bool UsesTexture { get; private set; }

        public void Optimize()
        {
            if (Uvs.Count == 0)
            {
                var vertices = new VertexPositionColorNormal[Vertices.Count];
                for (int i = 0; i < Vertices.Count; i++)
                    vertices[i] = new VertexPositionColorNormal(Vertices[i], Colors[i], Normals[i]);
                VertexPositionColorNormals = vertices;
                UsesColor = true;
            }
            else
            {
                var vertices = new VertexPositionNormalTexture[Vertices.Count];
                for (int i = 0; i < Vertices.Count; i++)
                    vertices[i] = new VertexPositionNormalTexture(Vertices[i], Normals[i], Uvs[i]);
                VertexPositionNormalTextures = vertices;
                UsesTexture = true;
            }
            IndicesArray = Indices.ToArray();
        }

        public int Triangles => Indices.Count / 3;
    }
}
