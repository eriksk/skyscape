﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkyScape.Core.Meshes;
using SkyScape.Core.Voxels.Meshes;
using System;
using System.Threading;

namespace SkyScape.Core.Voxels
{
    public class Chunk
    {
        public readonly int[,,] _data;
        private bool _dirty;
        private bool _canGenerate;
        private readonly int _x;
        private readonly int _y;
        private readonly int _z;

        private Mesh _mesh;

        public static readonly int LogSize = (int)Math.Log(World.ChunkSize, 2);
        public static readonly int Size = 1 << LogSize;
        public static readonly int Mask = Size - 1;

        public Chunk(int x, int y, int z)
        {
            _x = x;
            _y = y;
            _z = z;
            _data = new int[World.ChunkSize, World.ChunkSize, World.ChunkSize];
            _dirty = true;
        }

        public bool Dirty => _dirty && _canGenerate;

        public int X => _x;
        public int Y => _y;
        public int Z => _z;

        public Matrix View => Matrix.CreateTranslation(_x * World.ChunkSize + 0.5f, _y * World.ChunkSize + 0.5f, _z * World.ChunkSize + 0.5f);
        public Vector3 Center => new Vector3(_x * World.ChunkSize, _y * World.ChunkSize, _z * World.ChunkSize) + new Vector3(World.ChunkSize) * 0.5f;

        public void Set(int localX, int localY, int localZ, int value)
        {
            _data[localX, localY, localZ] = value;
            _dirty = true;
        }

        public int Get(int localX, int localY, int localZ)
        {
            return _data[localX, localY, localZ];
        }

        private bool _isCleaning = false;
        public void Clean(GraphicsDevice graphics, World world)
        {
            if (_isCleaning) return;

            _isCleaning = true;

            if (World.UseMultiThreading)
            {
                if (world.Batcher.HasWork(this)) return; // wait until previous is done
                // flag as cleaned so we don't try to clean again, unless something is changed, but then we wait until it is done with the previous gen
                _dirty = false;
                world.Batcher.QueueWork(world, this, GenerateChunkMesh, FinalizeMeshOnMainThread);
            }
            else
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

            }

        }

        public void MarkAsReadyToGenerate()
        {
            _canGenerate = true;
        }

        private MeshData _tempMeshData = null;

        private void FinalizeMeshOnMainThread(GraphicsDevice graphics)
        {
            if (_tempMeshData.Vertices.Count > 0)
            {
                var newMesh = new Mesh(graphics, _tempMeshData);

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
            _tempMeshData = null; // clear mesh data ref
            // reset states
            _isCleaning = false;
            _canGenerate = false;
        }

        private void GenerateChunkMesh(World world)
        {
            _tempMeshData = ChunkMeshGenerator.GenerateMesh(world, this);
        }

        public void Render(GraphicsDevice graphics, Effect effect)
        {
            if (_mesh == null) return;
            _mesh.Render(graphics, effect);
        }

        public void Clear()
        {
            if (_mesh != null)
            {
                _mesh.Dispose();
                _mesh = null;
            }
        }
    }
}