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
        /*
         * 
         *  // GREEDY ALGORITHM
         *  
function GreedyMesh(volume, dims) {
  function f(i,j,k) {
    return volume[i + dims[0] * (j + dims[1] * k)];
  }
  //Sweep over 3-axes
  var quads = [];
  for(var d=0; d<3; ++d) {
    var i, j, k, l, w, h
      , u = (d+1)%3
      , v = (d+2)%3
      , x = [0,0,0]
      , q = [0,0,0]
      , mask = new Int32Array(dims[u] * dims[v]);
    q[d] = 1;
    for(x[d]=-1; x[d]<dims[d]; ) {
      //Compute mask
      var n = 0;
      for(x[v]=0; x[v]<dims[v]; ++x[v])
      for(x[u]=0; x[u]<dims[u]; ++x[u]) {
        mask[n++] =
          (0    <= x[d]      ? f(x[0],      x[1],      x[2])      : false) !=
          (x[d] <  dims[d]-1 ? f(x[0]+q[0], x[1]+q[1], x[2]+q[2]) : false);
      }
      //Increment x[d]
      ++x[d];
      //Generate mesh for mask using lexicographic ordering
      n = 0;
      for(j=0; j<dims[v]; ++j)
      for(i=0; i<dims[u]; ) {
        if(mask[n]) {
          //Compute width
          for(w=1; mask[n+w] && i+w<dims[u]; ++w) {
          }
          //Compute height (this is slightly awkward
          var done = false;
          for(h=1; j+h<dims[v]; ++h) {
            for(k=0; k<w; ++k) {
              if(!mask[n+k+h*dims[u]]) {
                done = true;
                break;
              }
            }
            if(done) {
              break;
            }
          }
          //Add quad
          x[u] = i;  x[v] = j;
          var du = [0,0,0]; du[u] = w;
          var dv = [0,0,0]; dv[v] = h;
          quads.push([
              [x[0],             x[1],             x[2]            ]
            , [x[0]+du[0],       x[1]+du[1],       x[2]+du[2]      ]
            , [x[0]+du[0]+dv[0], x[1]+du[1]+dv[1], x[2]+du[2]+dv[2]]
            , [x[0]      +dv[0], x[1]      +dv[1], x[2]      +dv[2]]
          ]);
          //Zero-out mask
          for(l=0; l<h; ++l)
          for(k=0; k<w; ++k) {
            mask[n+k+l*dims[u]] = false;
          }
          //Increment counters and continue
          i += w; n += w;
        } else {
          ++i;    ++n;
        }
      }
    }
  }
  return quads;
}

         */

        public static MeshData GenerateMesh(World world, Chunk chunk)
        {
            var data = new MeshData()
            {
                PrimitiveType = PrimitiveType.TriangleList
            };

            var vertices = data.Vertices;
            var indices = data.Indices;
            var normals = data.Normals;
            var colors = data.Colors;

            int indexOffset = 0;

            for (int z = 0; z < World.ChunkSize; z++)
            {
                for (int y = 0; y < World.ChunkSize; y++)
                {
                    for (int x = 0; x < World.ChunkSize; x++)
                    {
                        var block = chunk.Get(x, y, z);

                        switch (block)
                        {
                            case Voxel.Empty:
                                break;
                            default:

                                VoxelMask mask = world.GetMask((chunk.X * World.ChunkSize) + x, (chunk.Y * World.ChunkSize) + y, (chunk.Z * World.ChunkSize) + z);
                                //indexOffset += AddBlock(data, indexOffset, x, y, z, Voxel.Types[block].Color, mask);
                                {
                                    var blockType = Voxel.Types[block];
                                    var color = blockType.Color;
                                    int faceCount = 0;
                                    var offset = new Vector3(x, y, z);
                                    var cols = new[]
                                    {
                                        color,color,color,color
                                    };
                                    // TODO: bitwize check flags

                                    if ((mask & VoxelMask.Left) != 0)
                                    {
                                        vertices.Add(new Vector3(-0.5f, -0.5f, -0.5f) + offset);
                                        vertices.Add(new Vector3(-0.5f, -0.5f, 0.5f) + offset);
                                        vertices.Add(new Vector3(-0.5f, 0.5f, 0.5f) + offset);
                                        vertices.Add(new Vector3(-0.5f, 0.5f, -0.5f) + offset);

                                        normals.Add(new Vector3(-1f, 0f, 0f));
                                        normals.Add(new Vector3(-1f, 0f, 0f));
                                        normals.Add(new Vector3(-1f, 0f, 0f));
                                        normals.Add(new Vector3(-1f, 0f, 0f));

                                        indices.Add(indexOffset + 3);
                                        indices.Add(indexOffset + 2);
                                        indices.Add(indexOffset + 0);
                                        indices.Add(indexOffset + 2);
                                        indices.Add(indexOffset + 1);
                                        indices.Add(indexOffset + 0);

                                        colors.AddRange(cols);

                                        data.Uvs.AddRange(blockType.GetTexCoords(VoxelMask.Left));

                                        indexOffset += 4;
                                        faceCount++;
                                    }

                                    if ((mask & VoxelMask.Right) != 0)
                                    {
                                        vertices.Add(new Vector3(0.5f, -0.5f, -0.5f) + offset);
                                        vertices.Add(new Vector3(0.5f, 0.5f, -0.5f) + offset);
                                        vertices.Add(new Vector3(0.5f, 0.5f, 0.5f) + offset);
                                        vertices.Add(new Vector3(0.5f, -0.5f, 0.5f) + offset);

                                        normals.Add(new Vector3(1f, 0f, 0f));
                                        normals.Add(new Vector3(1f, 0f, 0f));
                                        normals.Add(new Vector3(1f, 0f, 0f));
                                        normals.Add(new Vector3(1f, 0f, 0f));

                                        indices.Add(indexOffset + 3);
                                        indices.Add(indexOffset + 2);
                                        indices.Add(indexOffset + 0);
                                        indices.Add(indexOffset + 2);
                                        indices.Add(indexOffset + 1);
                                        indices.Add(indexOffset + 0);

                                        colors.AddRange(cols);

                                        data.Uvs.AddRange(blockType.GetTexCoords(VoxelMask.Right));

                                        indexOffset += 4;
                                        faceCount++;
                                    }

                                    if ((mask & VoxelMask.Up) != 0)
                                    {
                                        vertices.Add(new Vector3(-0.5f, 0.5f, -0.5f) + offset);
                                        vertices.Add(new Vector3(-0.5f, 0.5f, 0.5f) + offset);
                                        vertices.Add(new Vector3(0.5f, 0.5f, 0.5f) + offset);
                                        vertices.Add(new Vector3(0.5f, 0.5f, -0.5f) + offset);

                                        normals.Add(new Vector3(0f, 1f, 0f));
                                        normals.Add(new Vector3(0f, 1f, 0f));
                                        normals.Add(new Vector3(0f, 1f, 0f));
                                        normals.Add(new Vector3(0f, 1f, 0f));


                                        indices.Add(indexOffset + 3);
                                        indices.Add(indexOffset + 2);
                                        indices.Add(indexOffset + 0);
                                        indices.Add(indexOffset + 2);
                                        indices.Add(indexOffset + 1);
                                        indices.Add(indexOffset + 0);

                                        colors.AddRange(cols);

                                        data.Uvs.AddRange(blockType.GetTexCoords(VoxelMask.Up));

                                        indexOffset += 4;
                                        faceCount++;
                                    }

                                    if ((mask & VoxelMask.Down) != 0)
                                    {
                                        vertices.Add(new Vector3(-0.5f, -0.5f, -0.5f) + offset);
                                        vertices.Add(new Vector3(0.5f, -0.5f, -0.5f) + offset);
                                        vertices.Add(new Vector3(0.5f, -0.5f, 0.5f) + offset);
                                        vertices.Add(new Vector3(-0.5f, -0.5f, 0.5f) + offset);

                                        normals.Add(new Vector3(0f, -1f, 0f));
                                        normals.Add(new Vector3(0f, -1f, 0f));
                                        normals.Add(new Vector3(0f, -1f, 0f));
                                        normals.Add(new Vector3(0f, -1f, 0f));


                                        indices.Add(indexOffset + 3);
                                        indices.Add(indexOffset + 2);
                                        indices.Add(indexOffset + 0);
                                        indices.Add(indexOffset + 2);
                                        indices.Add(indexOffset + 1);
                                        indices.Add(indexOffset + 0);

                                        colors.AddRange(cols);

                                        data.Uvs.AddRange(blockType.GetTexCoords(VoxelMask.Down));

                                        indexOffset += 4;
                                        faceCount++;
                                    }

                                    if ((mask & VoxelMask.Forward) != 0)
                                    {
                                        vertices.Add(new Vector3(-0.5f, -0.5f, 0.5f) + offset);
                                        vertices.Add(new Vector3(0.5f, -0.5f, 0.5f) + offset);
                                        vertices.Add(new Vector3(0.5f, 0.5f, 0.5f) + offset);
                                        vertices.Add(new Vector3(-0.5f, 0.5f, 0.5f) + offset);

                                        normals.Add(new Vector3(0f, 0f, -1f));
                                        normals.Add(new Vector3(0f, 0f, -1f));
                                        normals.Add(new Vector3(0f, 0f, -1f));
                                        normals.Add(new Vector3(0f, 0f, -1f));


                                        indices.Add(indexOffset + 3);
                                        indices.Add(indexOffset + 2);
                                        indices.Add(indexOffset + 0);
                                        indices.Add(indexOffset + 2);
                                        indices.Add(indexOffset + 1);
                                        indices.Add(indexOffset + 0);

                                        colors.AddRange(cols);

                                        data.Uvs.AddRange(blockType.GetTexCoords(VoxelMask.Forward));

                                        indexOffset += 4;
                                        faceCount++;
                                    }

                                    if ((mask & VoxelMask.Back) != 0)
                                    {
                                        vertices.Add(new Vector3(-0.5f, -0.5f, -0.5f) + offset);
                                        vertices.Add(new Vector3(-0.5f, 0.5f, -0.5f) + offset);
                                        vertices.Add(new Vector3(0.5f, 0.5f, -0.5f) + offset);
                                        vertices.Add(new Vector3(0.5f, -0.5f, -0.5f) + offset);

                                        normals.Add(new Vector3(0f, 0f, 1f));
                                        normals.Add(new Vector3(0f, 0f, 1f));
                                        normals.Add(new Vector3(0f, 0f, 1f));
                                        normals.Add(new Vector3(0f, 0f, 1f));

                                        indices.Add(indexOffset + 3);
                                        indices.Add(indexOffset + 2);
                                        indices.Add(indexOffset + 0);
                                        indices.Add(indexOffset + 2);
                                        indices.Add(indexOffset + 1);
                                        indices.Add(indexOffset + 0);

                                        colors.AddRange(cols);

                                        data.Uvs.AddRange(blockType.GetTexCoords(VoxelMask.Back));

                                        indexOffset += 4;
                                        faceCount++;
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            data.Optimize();

            return data;            
        }

        //private static int AddBlock(MeshData data, int indexOffset, int x, int y, int z, Color color, VoxelMask mask)
        //{
        //    int faceCount = 0;
        //    var offset = new Vector3(x, y, z);
        //    var colors = new[]
        //    {
        //        color,color,color,color
        //    };

        //    if (mask.HasFlag(VoxelMask.Left))
        //    {
        //        ShapeFactory.CreateCubeFace(vertices, CubeFace.Left, offset);
        //        ShapeFactory.CreateCubeNormals(normals, CubeFace.Left);
        //        ShapeFactory.CreateCubeIndices(indices, indexOffset);
        //        colors.AddRange(colors);
        //        indexOffset += 4;
        //        faceCount++;
        //    }

        //    if (mask.HasFlag(VoxelMask.Right))
        //    {
        //        ShapeFactory.CreateCubeFace(vertices, CubeFace.Right, offset);
        //        ShapeFactory.CreateCubeNormals(normals, CubeFace.Right);
        //        ShapeFactory.CreateCubeIndices(indices, indexOffset);
        //        colors.AddRange(colors);
        //        indexOffset += 4;
        //        faceCount++;
        //    }

        //    if (mask.HasFlag(VoxelMask.Up))
        //    {
        //        ShapeFactory.CreateCubeFace(vertices, CubeFace.Top, offset);
        //        ShapeFactory.CreateCubeNormals(normals, CubeFace.Top);
        //        ShapeFactory.CreateCubeIndices(indices, indexOffset);
        //        colors.AddRange(colors);
        //        indexOffset += 4;
        //        faceCount++;
        //    }

        //    if (mask.HasFlag(VoxelMask.Down))
        //    {
        //        ShapeFactory.CreateCubeFace(vertices, CubeFace.Bottom, offset);
        //        ShapeFactory.CreateCubeNormals(normals, CubeFace.Bottom);
        //        ShapeFactory.CreateCubeIndices(indices, indexOffset);
        //        colors.AddRange(colors);
        //        indexOffset += 4;
        //        faceCount++;
        //    }

        //    if (mask.HasFlag(VoxelMask.Forward))
        //    {
        //        ShapeFactory.CreateCubeFace(vertices, CubeFace.Front, offset);
        //        ShapeFactory.CreateCubeNormals(normals, CubeFace.Front);
        //        ShapeFactory.CreateCubeIndices(indices, indexOffset);
        //        colors.AddRange(colors);
        //        indexOffset += 4;
        //        faceCount++;
        //    }

        //    if (mask.HasFlag(VoxelMask.Back))
        //    {
        //        ShapeFactory.CreateCubeFace(vertices, CubeFace.Back, offset);
        //        ShapeFactory.CreateCubeNormals(normals, CubeFace.Back);
        //        ShapeFactory.CreateCubeIndices(indices, indexOffset);
        //        colors.AddRange(colors);
        //        indexOffset += 4;
        //        faceCount++;
        //    }

        //    return faceCount * 4; // vertex count
        //}
    }
}
