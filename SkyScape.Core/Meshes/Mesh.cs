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
            if (data.UsesColor)
            {
                _vertexBuffer = new VertexBuffer(graphics, VertexPositionColorNormal.VertexDeclaration, data.Vertices.Count, BufferUsage.WriteOnly);
                _vertexBuffer.SetData(data.VertexPositionColorNormals);
            }
            else if (data.UsesTexture)
            {
                _vertexBuffer = new VertexBuffer(graphics, VertexPositionNormalTexture.VertexDeclaration, data.Vertices.Count, BufferUsage.WriteOnly);
                _vertexBuffer.SetData(data.VertexPositionNormalTextures);
            }

            _indexBuffer = new IndexBuffer(graphics, typeof(int), data.IndicesArray.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(data.IndicesArray);

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
                graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _primitiveCount);
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
