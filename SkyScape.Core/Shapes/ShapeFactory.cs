using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyScape.Core.Shapes
{
    public class ShapeFactory
    {
        #region Crap
        //public static Vector3[] CreateCubeVertices()
        //{
        //    throw new NotImplementedException();
        //}

        //public static int[] CreateCubeIndices(int offset = 0)
        //{
        //    throw new NotImplementedException();
        //}

        //public static Vector3[] CreateCubeFrontVertices(float scale = 1f)
        //{
        //    return new Vector3[]
        //    {
        //        ((Vector3.Left + Vector3.Up) * 0.5f + Vector3.Forward * 0.5f) * scale,
        //        ((Vector3.Left + Vector3.Down) * 0.5f + Vector3.Forward * 0.5f) * scale,
        //        ((Vector3.Right + Vector3.Up) * 0.5f + Vector3.Forward * 0.5f) * scale,
        //        ((Vector3.Right + Vector3.Down) * 0.5f + Vector3.Forward * 0.5f) * scale,

        //        //((Vector3.Left + Vector3.Down) * 0.5f + Vector3.Forward * 0.5f) * scale,
        //        //((Vector3.Left + Vector3.Up) * 0.5f + Vector3.Forward * 0.5f) * scale,
        //        //((Vector3.Right + Vector3.Down) * 0.5f + Vector3.Forward * 0.5f) * scale,
        //        //((Vector3.Right + Vector3.Up) * 0.5f + Vector3.Forward * 0.5f) * scale,
        //    };
        //}


        //public static Vector3[] CreateCubeBackVertices(float scale = 1f)
        //{
        //    return new Vector3[]
        //    {
        //        ((Vector3.Left + Vector3.Down) * 0.5f + Vector3.Backward * 0.5f) * scale,
        //        ((Vector3.Left + Vector3.Up) * 0.5f + Vector3.Backward * 0.5f) * scale,
        //        ((Vector3.Right + Vector3.Down) * 0.5f + Vector3.Backward * 0.5f) * scale,
        //        ((Vector3.Right + Vector3.Up) * 0.5f + Vector3.Backward * 0.5f) * scale,
        //    };
        //}

        //public static Vector3[] CreateCubeLeftVertices(float scale = 1f)
        //{
        //    return new Vector3[]
        //    {
        //        ((Vector3.Left + Vector3.Down) * 0.5f + Vector3.Left * 0.5f) * scale,
        //        ((Vector3.Left + Vector3.Up) * 0.5f + Vector3.Left* 0.5f) * scale,
        //        ((Vector3.Right + Vector3.Down) * 0.5f + Vector3.Left* 0.5f) * scale,
        //        ((Vector3.Right + Vector3.Up) * 0.5f + Vector3.Left* 0.5f) * scale,
        //    };
        //}

        //public static int[] CreateCubeFaceIndices(int offset = 0)
        //{
        //    return new int[]
        //    {
        //        offset + 0,
        //        offset + 1,
        //        offset + 2,
        //        offset + 2,
        //        offset + 1,
        //        offset + 3
        //    };
        //}

        #endregion


        public static Vector3[] CreateCubeFaceLeft()
        {
            return new[] 
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f,  0.5f),
                new Vector3(-0.5f,  0.5f,  0.5f),
                new Vector3(-0.5f,  0.5f, -0.5f)
            };
        }

        public static Vector3[] CreateCubeNormalsLeft()
        {
            return new Vector3[] { }.AddRepeat(Vector3.Left, 4).ToArray();
        }

        public static int[] CreateCubeIndicesLeft()
        {
            return new[] 
            {
                20, 21, 22,
                20, 22, 23    // left
            }.Reverse().ToArray();
        }

        public static void CreateCubeIndices(List<int> indices, int offset)
        {
            indices.AddRange(new int[]
            {
                offset + 3,
                offset + 2,
                offset + 0,
                offset + 2,
                offset + 1,
                offset + 0
            });
        }

        public static readonly Vector3[] NormalsLeft = new Vector3[] { }.AddRepeat(Vector3.Left, 4).ToArray();
        public static readonly Vector3[] NormalsRight = new Vector3[] { }.AddRepeat(Vector3.Right, 4).ToArray();
        public static readonly Vector3[] NormalsUp = new Vector3[] { }.AddRepeat(Vector3.Up, 4).ToArray();
        public static readonly Vector3[] NormalsDown = new Vector3[] { }.AddRepeat(Vector3.Down, 4).ToArray();
        public static readonly Vector3[] NormalsForward = new Vector3[] { }.AddRepeat(Vector3.Forward, 4).ToArray();
        public static readonly Vector3[] NormalsBack = new Vector3[] { }.AddRepeat(Vector3.Backward, 4).ToArray();

        public static void CreateCubeNormals(List<Vector3> normals, CubeFace face)
        {
            switch (face)
            {
                case CubeFace.Left:
                    normals.AddRange(NormalsLeft);
                    break;
                case CubeFace.Right:
                    normals.AddRange(NormalsRight);
                    break;
                case CubeFace.Top:
                    normals.AddRange(NormalsUp);
                    break;
                case CubeFace.Bottom:
                    normals.AddRange(NormalsDown);
                    break;
                case CubeFace.Front:
                    normals.AddRange(NormalsForward);
                    break;
                case CubeFace.Back:
                    normals.AddRange(NormalsBack);
                    break;

                default:
                    throw new Exception("Invalid face");
            }
        }

        public static void CreateCubeFace(List<Vector3> vertices, CubeFace face, Vector3 offset = new Vector3())
        {
            switch (face)
            {
                case CubeFace.Left:
                    vertices.AddRange(new[] 
                    {
                        // Left
                        new Vector3(-0.5f, -0.5f, -0.5f) + offset,
                        new Vector3(-0.5f, -0.5f,  0.5f) + offset,
                        new Vector3(-0.5f,  0.5f,  0.5f) + offset,
                        new Vector3(-0.5f,  0.5f, -0.5f) + offset
                    });
                    break;
                case CubeFace.Right:
                    vertices.AddRange(new[]
                    { 
                        // Right
                        new Vector3(0.5f, -0.5f, -0.5f) + offset,
                        new Vector3(0.5f,  0.5f, -0.5f) + offset,
                        new Vector3(0.5f,  0.5f,  0.5f) + offset,
                        new Vector3(0.5f, -0.5f,  0.5f) + offset
                    });
                    break;
                case CubeFace.Top:
                    vertices.AddRange(new[]
                    {
                        // Top
                        new Vector3(-0.5f,  0.5f, -0.5f)+offset,
                        new Vector3(-0.5f,  0.5f,  0.5f)+offset,
                        new Vector3(0.5f,  0.5f,  0.5f)+offset,
                        new Vector3(0.5f,  0.5f, -0.5f) +offset
                    });
                    break;
                case CubeFace.Bottom:
                    vertices.AddRange(new[]
                    { 
                        // Bottom
                        new Vector3(-0.5f, -0.5f, -0.5f)+offset,
                        new Vector3(0.5f, -0.5f, -0.5f)+offset,
                        new Vector3(0.5f, -0.5f,  0.5f)+offset,
                        new Vector3(-0.5f, -0.5f,  0.5f)+offset
                    });
                    break;
                case CubeFace.Front:
                    vertices.AddRange(new[]
                    {
                        // Front
                        new Vector3(-0.5f, -0.5f,  0.5f)+offset,
                        new Vector3(0.5f, -0.5f,  0.5f)+offset,
                        new Vector3(0.5f,  0.5f,  0.5f)+offset,
                        new Vector3(-0.5f,  0.5f,  0.5f)+offset,
                    });
                    break;
                case CubeFace.Back:
                    vertices.AddRange(new[]
                    {
                        // Back
                        new Vector3(-0.5f, -0.5f, -0.5f)+offset,
                        new Vector3(-0.5f,  0.5f, -0.5f)+offset,
                        new Vector3(0.5f,  0.5f, -0.5f)+offset,
                        new Vector3(0.5f, -0.5f, -0.5f)+offset
                    });
                    break;
                default:
                    throw new Exception("Invalid face");
            }
        }

        public static Vector3[] CreateCubeFaces()
        {
            return new[]
             {
                // Front
                new Vector3(-0.5f, -0.5f,  0.5f),
                new Vector3(0.5f, -0.5f,  0.5f),
                new Vector3(0.5f,  0.5f,  0.5f),
                new Vector3(-0.5f,  0.5f,  0.5f),
                // Back
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f,  0.5f, -0.5f),
                new Vector3(0.5f,  0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                // Top
                new Vector3(-0.5f,  0.5f, -0.5f),
                new Vector3(-0.5f,  0.5f,  0.5f),
                new Vector3(0.5f,  0.5f,  0.5f),
                new Vector3(0.5f,  0.5f, -0.5f),
                // Bottom
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f,  0.5f),
                new Vector3(-0.5f, -0.5f,  0.5f),
                // Right
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f,  0.5f, -0.5f),
                new Vector3(0.5f,  0.5f,  0.5f),
                new Vector3(0.5f, -0.5f,  0.5f),
                // Left
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f,  0.5f),
                new Vector3(-0.5f,  0.5f,  0.5f),
                new Vector3(-0.5f,  0.5f, -0.5f)
            };
        }

        public static Vector3[] CreateCubeNormals()
        {
            return new Vector3[] { }
                .AddRepeat(Vector3.Backward, 4)
                .AddRepeat(Vector3.Forward, 4)
                .AddRepeat(Vector3.Up, 4)
                .AddRepeat(Vector3.Down, 4)
                .AddRepeat(Vector3.Right, 4)
                .AddRepeat(Vector3.Left, 4)
                .ToArray();
        }

        public static int[] CreateCubeIndices()
        {
            return new int[]
            {
                  0,  1,  2,      0,  2,  3,    // front
                  4,  5,  6,      4,  6,  7,    // back
                  8,  9,  10,     8,  10, 11,   // top
                  12, 13, 14,     12, 14, 15,   // bottom
                  16, 17, 18,     16, 18, 19,   // right
                  20, 21, 22,     20, 22, 23    // left
            }.Reverse().ToArray();
        }

    }

    public enum CubeFace
    {
        Left,
        Right,
        Top,
        Bottom,
        Front,
        Back
    }
}
