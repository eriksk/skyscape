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

        public static int[] CreateCubeIndices(CubeFace face)
        {
            return new int[]
            {
               3,  2,  0,      2,  1,  0
            };
        }

        public static Vector3[] CreateCubeNormals(CubeFace face)
        {
            switch (face)
            {
                case CubeFace.Left:
                    return new Vector3[] { }
                     .AddRepeat(Vector3.Left, 4)
                     .ToArray();
                case CubeFace.Right:
                    return new Vector3[] { }
                     .AddRepeat(Vector3.Right, 4)
                     .ToArray();
                case CubeFace.Top:
                    return new Vector3[] { }
                     .AddRepeat(Vector3.Up, 4)
                     .ToArray();
                case CubeFace.Bottom:
                    return new Vector3[] { }
                     .AddRepeat(Vector3.Down, 4)
                     .ToArray();
                case CubeFace.Front:
                    return new Vector3[] { }
                     .AddRepeat(Vector3.Forward, 4)
                     .ToArray();
                case CubeFace.Back:
                    return new Vector3[] { }
                     .AddRepeat(Vector3.Backward, 4)
                     .ToArray();
                default:
                    throw new Exception("Invalid face");
            }
        }

        public static Vector3[] CreateCubeFace(CubeFace face)
        {
            switch (face)
            {
                case CubeFace.Left:
                    return new[] 
                    {
                        // Left
                        new Vector3(-0.5f, -0.5f, -0.5f),
                        new Vector3(-0.5f, -0.5f,  0.5f),
                        new Vector3(-0.5f,  0.5f,  0.5f),
                        new Vector3(-0.5f,  0.5f, -0.5f)
                    };
                case CubeFace.Right:
                    return new[] 
                    { 
                        // Right
                        new Vector3(0.5f, -0.5f, -0.5f),
                        new Vector3(0.5f,  0.5f, -0.5f),
                        new Vector3(0.5f,  0.5f,  0.5f),
                        new Vector3(0.5f, -0.5f,  0.5f)
                    };
                case CubeFace.Top:
                    return new[] 
                    {
                        // Top
                        new Vector3(-0.5f,  0.5f, -0.5f),
                        new Vector3(-0.5f,  0.5f,  0.5f),
                        new Vector3(0.5f,  0.5f,  0.5f),
                        new Vector3(0.5f,  0.5f, -0.5f)
                    };
                case CubeFace.Bottom:
                    return new[] 
                    { 
                        // Bottom
                        new Vector3(-0.5f, -0.5f, -0.5f),
                        new Vector3(0.5f, -0.5f, -0.5f),
                        new Vector3(0.5f, -0.5f,  0.5f),
                        new Vector3(-0.5f, -0.5f,  0.5f)
                    };
                case CubeFace.Front:
                    return new[]
                    {
                        // Front
                        new Vector3(-0.5f, -0.5f,  0.5f),
                        new Vector3(0.5f, -0.5f,  0.5f),
                        new Vector3(0.5f,  0.5f,  0.5f),
                        new Vector3(-0.5f,  0.5f,  0.5f),
                    };
                case CubeFace.Back:
                    return new[]
                    {
                        // Back
                        new Vector3(-0.5f, -0.5f, -0.5f),
                        new Vector3(-0.5f,  0.5f, -0.5f),
                        new Vector3(0.5f,  0.5f, -0.5f),
                        new Vector3(0.5f, -0.5f, -0.5f)
                    };
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
