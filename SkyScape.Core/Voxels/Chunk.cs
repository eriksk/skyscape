using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkyScape.Core.Meshes;
using SkyScape.Core.Voxels.Meshes;
using System;
using System.Threading;

namespace SkyScape.Core.Voxels
{
    public class Chunk
    {
        private readonly int[] _data;
        private bool _dirty;
        private readonly int _x;
        private readonly int _y;
        private readonly int _z;

        private Mesh _mesh;

        public Chunk(int x, int y, int z)
        {
            _x = x;
            _y = y;
            _z = z;
            _data = new int[World.ChunkSize * World.ChunkSize * World.ChunkSize];
            _dirty = true;
        }

        public bool Dirty => _dirty;

        public int X => _x;
        public int Y => _y;
        public int Z => _z;

        public Matrix View => 
            Matrix.CreateTranslation(_x * World.ChunkSize + 0.5f, _y * World.ChunkSize + 0.5f, _z * World.ChunkSize + 0.5f);

        public void Set(int localX, int localY, int localZ, int value)
        {
            _data[localX * World.ChunkSize * World.ChunkSize + localY * World.ChunkSize + localZ] = value;
            _dirty = true;
        }

        public int Get(int localX, int localY, int localZ)
        {
            if (localX < 0) throw new ArgumentException(nameof(localX));
            if (localX > World.ChunkSize - 1) throw new ArgumentException(nameof(localX));
            if (localY < 0) throw new ArgumentException(nameof(localY));
            if (localY > World.ChunkSize - 1) throw new ArgumentException(nameof(localY));
            if (localZ < 0) throw new ArgumentException(nameof(localZ));
            if (localZ > World.ChunkSize - 1) throw new ArgumentException(nameof(localZ));

            return _data[localX * World.ChunkSize * World.ChunkSize + localY * World.ChunkSize + localZ];
        }

        private bool _isCleaning = false;
        public void Clean(GraphicsDevice graphics, World world)
        {
            if (_isCleaning) return;

            _isCleaning = true;

            ThreadPool.QueueUserWorkItem((f) =>
            {
                var meshData = ChunkMeshGenerator.GenerateMesh(world, this);
                if (meshData.Vertices.Count > 0)
                {
                    var newMesh = new Mesh(graphics, meshData);

                    // swap references
                    var oldMesh = _mesh;
                    _mesh = newMesh;

                    if (oldMesh != null)
                        oldMesh.Dispose();
                }
                else
                {
                    // No verts, so just destroy it
                    if (_mesh != null)
                        _mesh.Dispose();
                    _mesh = null;
                }
                _isCleaning = false;
                _dirty = false;
            }, 0);
            //new Thread(() =>
            //{
            //}).Start();


        }

        public void Render(GraphicsDevice graphics, Effect effect)
        {
            if (_mesh == null) return;
            _mesh.Render(graphics, effect);
        }
    }
}