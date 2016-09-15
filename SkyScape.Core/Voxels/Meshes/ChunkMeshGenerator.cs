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

                                VoxelMask mask = world.GetMask((chunk.X * World.ChunkSize) + x, (chunk.Y * World.ChunkSize) + y, (chunk.Z * World.ChunkSize) + z);
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
            var offset = new Vector3(x, y, z);
            var colors = new[]
            {
                color,color,color,color
            };

            if (mask.HasFlag(VoxelMask.Left))
            {
                ShapeFactory.CreateCubeFace(data.Vertices, CubeFace.Left, offset);
                ShapeFactory.CreateCubeNormals(data.Normals, CubeFace.Left);
                ShapeFactory.CreateCubeIndices(data.Indices, indexOffset);
                data.Colors.AddRange(colors);
                indexOffset += 4;
                faceCount++;
            }

            if (mask.HasFlag(VoxelMask.Right))
            {
                ShapeFactory.CreateCubeFace(data.Vertices, CubeFace.Right, offset);
                ShapeFactory.CreateCubeNormals(data.Normals, CubeFace.Right);
                ShapeFactory.CreateCubeIndices(data.Indices, indexOffset);
                data.Colors.AddRange(colors);
                indexOffset += 4;
                faceCount++;
            }

            if (mask.HasFlag(VoxelMask.Up))
            {
                ShapeFactory.CreateCubeFace(data.Vertices, CubeFace.Top, offset);
                ShapeFactory.CreateCubeNormals(data.Normals, CubeFace.Top);
                ShapeFactory.CreateCubeIndices(data.Indices, indexOffset);
                data.Colors.AddRange(colors);
                indexOffset += 4;
                faceCount++;
            }

            if (mask.HasFlag(VoxelMask.Down))
            {
                ShapeFactory.CreateCubeFace(data.Vertices, CubeFace.Bottom, offset);
                ShapeFactory.CreateCubeNormals(data.Normals, CubeFace.Bottom);
                ShapeFactory.CreateCubeIndices(data.Indices, indexOffset);
                data.Colors.AddRange(colors);
                indexOffset += 4;
                faceCount++;
            }

            if (mask.HasFlag(VoxelMask.Forward))
            {
                ShapeFactory.CreateCubeFace(data.Vertices, CubeFace.Front, offset);
                ShapeFactory.CreateCubeNormals(data.Normals, CubeFace.Front);
                ShapeFactory.CreateCubeIndices(data.Indices, indexOffset);
                data.Colors.AddRange(colors);
                indexOffset += 4;
                faceCount++;
            }

            if (mask.HasFlag(VoxelMask.Back))
            {
                ShapeFactory.CreateCubeFace(data.Vertices, CubeFace.Back, offset);
                ShapeFactory.CreateCubeNormals(data.Normals, CubeFace.Back);
                ShapeFactory.CreateCubeIndices(data.Indices, indexOffset);
                data.Colors.AddRange(colors);
                indexOffset += 4;
                faceCount++;
            }

            return faceCount * 4; // vertex count
        }
    }
}
