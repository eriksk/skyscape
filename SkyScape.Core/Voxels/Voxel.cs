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
            new VoxelType(Empty, Color.Black),
            new VoxelType(Rock, Color.DarkGray),
            new VoxelType(Grass, Color.Green),
            new VoxelType(Dirt, Color.LightGoldenrodYellow),
            new VoxelType(Stone, Color.Gray),
            new VoxelType(Sand, Color.LightYellow),
            new VoxelType(Snow, Color.White),
            new VoxelType(Gold, Color.Orange),
            new VoxelType(Water, Color.LightBlue)
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
            int hash = 13;
            hash = (hash * 7) + X;
            hash = (hash * 7) + Y;
            hash = (hash * 7) + Z;
            return hash;
        }
    }

    public struct VoxelType
    {
        public int Id;
        public Color Color;

        public VoxelType(int id, Color color)
        {
            Id = id;
            Color = color;
        }
    }
}
