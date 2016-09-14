using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkyScape.Core.Meshes;
using SkyScape.Core.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyScape.Core.Voxels.Meshes
{
    public class ChunkMeshGenerator
    {
        public static int SkippedBlocks = 0;

        public static MeshData GenerateMesh(World world, Chunk chunk)
        {
            var data = new MeshData()
            {
                PrimitiveType = PrimitiveType.TriangleList
            };

            int indexOffset = 0;

            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    for (int y = 0; y < World.ChunkSize; y++)
                    {
                        var block = chunk.Get(x, y, z);

                        switch (block)
                        {
                            case Voxel.Empty:
                                break;
                            default:

                                VoxelMask mask = world.GetMask(new VoxelPosition((chunk.X * World.ChunkSize) + x, (chunk.Y * World.ChunkSize) + y, (chunk.Z * World.ChunkSize) + z));
                                indexOffset += AddBlock(data, indexOffset, x, y, z, Voxel.Types[block].Color, mask);
                                break;
                        }
                    }
                }
            }

            return data;            
        }

        private static int AddBlock(MeshData data, int indexOffset, int x, int y, int z, Color color, VoxelMask mask)
        {
            int faceCount = 0;

            if (mask.HasFlag(VoxelMask.Left))
            {
                data.Vertices.AddRange(ShapeFactory.CreateCubeFace(CubeFace.Left).Select(f => f + new Vector3(x, y, z)));
                data.Normals.AddRange(ShapeFactory.CreateCubeNormals(CubeFace.Left));
                data.Indices.AddRange(ShapeFactory.CreateCubeIndices(CubeFace.Left).Select(f => indexOffset + f));
                data.Colors.AddRange(new Color[] { }.AddRepeat(color, 4));
                indexOffset += 4;
                faceCount++;
            }

            if (mask.HasFlag(VoxelMask.Right))
            {
                data.Vertices.AddRange(ShapeFactory.CreateCubeFace(CubeFace.Right).Select(f => f + new Vector3(x, y, z)));
                data.Normals.AddRange(ShapeFactory.CreateCubeNormals(CubeFace.Right));
                data.Indices.AddRange(ShapeFactory.CreateCubeIndices(CubeFace.Right).Select(f => indexOffset + f));
                data.Colors.AddRange(new Color[] { }.AddRepeat(color, 4));
                indexOffset += 4;
                faceCount++;
            }

            if (mask.HasFlag(VoxelMask.Up))
            {
                data.Vertices.AddRange(ShapeFactory.CreateCubeFace(CubeFace.Top).Select(f => f + new Vector3(x, y, z)));
                data.Normals.AddRange(ShapeFactory.CreateCubeNormals(CubeFace.Top));
                data.Indices.AddRange(ShapeFactory.CreateCubeIndices(CubeFace.Top).Select(f => indexOffset + f));
                data.Colors.AddRange(new Color[] { }.AddRepeat(color, 4));
                indexOffset += 4;
                faceCount++;
            }

            if (mask.HasFlag(VoxelMask.Down))
            {
                data.Vertices.AddRange(ShapeFactory.CreateCubeFace(CubeFace.Bottom).Select(f => f + new Vector3(x, y, z)));
                data.Normals.AddRange(ShapeFactory.CreateCubeNormals(CubeFace.Bottom));
                data.Indices.AddRange(ShapeFactory.CreateCubeIndices(CubeFace.Bottom).Select(f => indexOffset + f));
                data.Colors.AddRange(new Color[] { }.AddRepeat(color, 4));
                indexOffset += 4;
                faceCount++;
            }

            if (mask.HasFlag(VoxelMask.Forward))
            {
                data.Vertices.AddRange(ShapeFactory.CreateCubeFace(CubeFace.Front).Select(f => f + new Vector3(x, y, z)));
                data.Normals.AddRange(ShapeFactory.CreateCubeNormals(CubeFace.Front));
                data.Indices.AddRange(ShapeFactory.CreateCubeIndices(CubeFace.Front).Select(f => indexOffset + f));
                data.Colors.AddRange(new Color[] { }.AddRepeat(color, 4));
                indexOffset += 4;
                faceCount++;
            }

            if (mask.HasFlag(VoxelMask.Back))
            {
                data.Vertices.AddRange(ShapeFactory.CreateCubeFace(CubeFace.Back).Select(f => f + new Vector3(x, y, z)));
                data.Normals.AddRange(ShapeFactory.CreateCubeNormals(CubeFace.Back));
                data.Indices.AddRange(ShapeFactory.CreateCubeIndices(CubeFace.Back).Select(f => indexOffset + f));
                data.Colors.AddRange(new Color[] { }.AddRepeat(color, 4));
                indexOffset += 4;
                faceCount++;
            }

            return faceCount * 4; // vertex count
        }
    }
}
