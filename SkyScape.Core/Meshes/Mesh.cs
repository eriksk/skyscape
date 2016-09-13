using Microsoft.Xna.Framework.Graphics;
using SkyScape.Core.CustomVertexTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyScape.Core.Meshes
{
    public class Mesh : IDisposable
    {
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private PrimitiveType _primitiveType;
        private int _vertexCount;
        private int _primitiveCount;

        public Mesh(GraphicsDevice graphics, MeshData data)
        {
            _primitiveType = data.PrimitiveType;
            _vertexBuffer = new VertexBuffer(graphics, VertexPositionColorNormal.VertexDeclaration, data.Vertices.Count, BufferUsage.WriteOnly);
            _indexBuffer = new IndexBuffer(graphics, typeof(int), data.Indices.Count, BufferUsage.WriteOnly);

            var vertices = new VertexPositionColorNormal[data.Vertices.Count];
            for (int i = 0; i < data.Vertices.Count; i++)
                vertices[i] = new VertexPositionColorNormal(data.Vertices[i], data.Colors[i], data.Normals[i]);
            _vertexBuffer.SetData(vertices);
            _indexBuffer.SetData(data.Indices.ToArray());

            _vertexCount = _vertexBuffer.VertexCount;
            _primitiveCount = data.Triangles;
        }

        public void Render(GraphicsDevice graphics, Effect effect)
        {
            graphics.SetVertexBuffer(_vertexBuffer);
            graphics.Indices = _indexBuffer;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawIndexedPrimitives(_primitiveType, 0, 0, _primitiveCount);
            }
        }

        public void Dispose()
        {
            if (_vertexBuffer != null)
            {
                _vertexBuffer.Dispose();
                _vertexBuffer = null;
            }
            if(_indexBuffer != null)
            {
                _indexBuffer.Dispose();
                _indexBuffer = null;
            }
        }
    }
}
