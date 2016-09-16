using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyScape.Core.Voxels
{
    public class Voxel
    {
        public const int Empty = 0;
        public const int Rock = 1;
        public const int Grass = 2;
        public const int Dirt = 3;
        public const int Stone = 4;
        public const int Sand = 5;
        public const int Snow = 6;
        public const int Gold = 7;
        public const int Water = 8;

        public static readonly int[] VoxelTypes = new int[]
        {
            Empty,
            Rock ,
            Grass,
            Dirt ,
            Stone,
            Sand ,
            Snow ,
            Gold ,
            Water
        };

        public static readonly VoxelType[] Types = new[]
        {
            new VoxelType(Empty, Color.Black, -1, -1),
            new VoxelType(Rock, Color.White, 8, 8),
            new VoxelType(Grass, Color.Green, 0, 1),
            new VoxelType(Dirt, Color.LightGoldenrodYellow, 2, 3),
            new VoxelType(Stone, Color.Gray, 4, 4),
            new VoxelType(Sand, Color.LightYellow, 6, 6),
            new VoxelType(Snow, Color.White, 6, 7),
            new VoxelType(Gold, Color.Orange, 9, 9),
            new VoxelType(Water, Color.Blue, 9, 9)
        };

    }

    [Flags]
    public enum VoxelMask
    {
        None = 0,
        Right = 1,
        Left = 2,
        Up = 4,
        Down = 8,
        Forward = 16,
        Back = 32,
        All = 63
    }

    public struct VoxelPosition
    {
        public int X, Y, Z;

        public VoxelPosition(int x = 0, int y = 0, int z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override bool Equals(object obj)
        {
            var o = (VoxelPosition)obj;

            return o.X == X && o.Y == Y && o.Z == Z;
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 23 + X.GetHashCode();
            hash = hash * 23 + Y.GetHashCode();
            hash = hash * 23 + Z.GetHashCode();

            return hash;
        }

        public override string ToString()
        {
            return $"(X:{X}, Y:{Y}, Z:{Z})";
        }
    }

    public struct VoxelType
    {
        public int Id;
        public Color Color;
        public int SideTextureIndex, TopTextureIndex;

        public VoxelType(int id, Color color, int sideTextureIndex, int topTextureIndex)
        {
            Id = id;
            Color = color;
            SideTextureIndex = sideTextureIndex;
            TopTextureIndex = topTextureIndex;
        }

        public Vector2[] GetTexCoords(VoxelMask side)
        {
            const int width = 256;
            const int height = 256;
            const int cellSize = width / 8;
            const float coordSize = cellSize / (float)width;

            int index = 0;
            if(side.HasFlag(VoxelMask.Left) ||
                side.HasFlag(VoxelMask.Right) ||
                side.HasFlag(VoxelMask.Forward) ||
                side.HasFlag(VoxelMask.Back) ||
                side.HasFlag(VoxelMask.Down))
            {
                index = SideTextureIndex;
            }
            if (side.HasFlag(VoxelMask.Up))
            {
                index = TopTextureIndex;
            }

            int col = index % 8;
            int row = index / 8;


            if (side.HasFlag(VoxelMask.Left))
            {
                return new[]
                {
                    new Vector2((col + 1) * coordSize, (row + 1) * coordSize),
                    new Vector2(col * coordSize, (row + 1) * coordSize),
                    new Vector2(col * coordSize, row * coordSize),
                    new Vector2((col + 1) * coordSize, row * coordSize),
                };

            }

            if (side.HasFlag(VoxelMask.Right))
            {
                return new[]
                {
                    new Vector2(col * coordSize, (row + 1) * coordSize),
                    new Vector2(col * coordSize, row * coordSize),
                    new Vector2((col + 1) * coordSize, row * coordSize),
                    new Vector2((col + 1) * coordSize, (row + 1) * coordSize),
                };
            }

            if (side.HasFlag(VoxelMask.Back))
            {
                return new[]
                {
                    new Vector2(col * coordSize, (row + 1) * coordSize),
                    new Vector2(col * coordSize, row * coordSize),
                    new Vector2((col + 1) * coordSize, row * coordSize),
                    new Vector2((col + 1) * coordSize, (row + 1) * coordSize)
                };
            }

            if (side.HasFlag(VoxelMask.Forward))
            {
                return new[]
                {
                    new Vector2((col + 1) * coordSize, (row + 1) * coordSize),
                    new Vector2(col * coordSize, (row + 1) * coordSize),
                    new Vector2(col * coordSize, row * coordSize),
                    new Vector2((col + 1) * coordSize, row * coordSize),
                };
            }

            return new[]
            {
                new Vector2(col * coordSize, row * coordSize),
                new Vector2((col + 1) * coordSize, row * coordSize),
                new Vector2((col + 1) * coordSize, (row + 1) * coordSize),
                new Vector2(col * coordSize, (row + 1) * coordSize)
            };
        }
    }
}
