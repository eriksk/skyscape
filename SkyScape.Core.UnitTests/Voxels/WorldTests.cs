using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyScape.Core.Voxels;

namespace SkyScape.Core.UnitTests.Voxels
{
    [TestClass]
    public class WorldTests
    {
        [TestMethod]
        public void VoxelMask_IsValidFlags()
        {
            Assert.AreEqual("0", Convert.ToString((int)(VoxelMask.None), 2));
            Assert.AreEqual("1", Convert.ToString((int)(VoxelMask.Right), 2));
            Assert.AreEqual("10", Convert.ToString((int)(VoxelMask.Left), 2));
            Assert.AreEqual("100", Convert.ToString((int)(VoxelMask.Up), 2));
            Assert.AreEqual("1000", Convert.ToString((int)(VoxelMask.Down), 2));
            Assert.AreEqual("10000", Convert.ToString((int)(VoxelMask.Forward), 2));
            Assert.AreEqual("100000", Convert.ToString((int)(VoxelMask.Back), 2));
            Assert.AreEqual("111111", Convert.ToString((int)(VoxelMask.All), 2));


            Assert.AreEqual("11", Convert.ToString((int)(VoxelMask.Left | VoxelMask.Right), 2));

        }

        [TestMethod]
        public void WorldToLocal_PositiveNumber_DifferentChunks()
        {
            World.ChunkSize = 16;

            var world = new World();

            Assert.AreEqual(new VoxelPosition(1, 1, 1), world.WorldToLocal(new VoxelPosition(1, 1, 1), new Chunk(0, 0, 0)));
            Assert.AreEqual(new VoxelPosition(4, 15, 6), world.WorldToLocal(new VoxelPosition(4, 15, 6), new Chunk(0, 0, 0)));
            Assert.AreEqual(new VoxelPosition(0, 0, 0), world.WorldToLocal(new VoxelPosition(16, 0, 0), new Chunk(1, 0, 0)));
        }

        [TestMethod]
        public void __()
        {
            World.ChunkSize = 16;

            var world = new World();

            world.Set(0, 0, 0, 1);
            world.Set(16, 0, 0, 2);
            world.Set(-1, 0, 0, 3);
            world.Set(0, -1, 0, 4);
            world.Set(0, 0, -1, 5);

            Assert.AreEqual(1, world.Get(0, 0, 0));
            Assert.AreEqual(2, world.Get(16, 0, 0));

            // negative
            Assert.AreEqual(3, world.Get(-1, 0, 0));
            Assert.AreEqual(4, world.Get(0, -1, 0));
            Assert.AreEqual(5, world.Get(0, 0, -1));
        }

        [TestMethod]
        public void GetMask_VoxelSurroundedOnAllSides_MaskIsNone()
        {
            var world = new World();

            world.Set(-1, 0, 0, 1);
            world.Set(1, 0, 0, 1);
            world.Set(0, -1, 0, 1);
            world.Set(0, 1, 0, 1);
            world.Set(0, 0, -1, 1);
            world.Set(0, 0, 1, 1);

            var mask = world.GetMask(new VoxelPosition(0, 0, 0));

            Assert.AreEqual(VoxelMask.None, mask);
        }

        [TestMethod]
        public void GetMask_VoxelSurroundedOnNoSides_MaskIsAll()
        {
            var world = new World();

            var mask = world.GetMask(new VoxelPosition(0, 0, 0));

            Assert.AreEqual(VoxelMask.All, mask);
        }

        [TestMethod]
        public void GetMask_VoxelSurroundedOnLeftAndRight_MaskIsUpDownForwardBack()
        {
            var world = new World();

            world.Set(-1, 0, 0, 1);
            world.Set(1, 0, 0, 1);

            var mask = world.GetMask(new VoxelPosition(0, 0, 0));

            Assert.AreEqual(VoxelMask.Up | VoxelMask.Down | VoxelMask.Forward | VoxelMask.Back, mask);
        }

        [TestMethod]
        public void GetMask_VoxelSurroundedOnUpDownForwardAndBack_MaskIsLeftAndRight()
        {
            var position = new VoxelPosition(0, 0, 0);

            var world = SurroundTest(position, w =>
            {
                SurroundUp(position, w);
                SurroundDown(position, w);
                SurroundForward(position, w);
                SurroundBack(position, w);
            });
            
            Assert.AreEqual(VoxelMask.Left | VoxelMask.Right, world.GetMask(position));

            position = new VoxelPosition(-1, -1, -1);
            world = SurroundTest(position, w =>
            {
                SurroundUp(position, w);
                SurroundDown(position, w);
                SurroundForward(position, w);
                SurroundBack(position, w);
            });

            Assert.AreEqual(VoxelMask.Left | VoxelMask.Right, world.GetMask(position));

            position = new VoxelPosition(-134, -678, -19);
            world = SurroundTest(position, w =>
            {
                SurroundUp(position, w);
                SurroundDown(position, w);
                SurroundForward(position, w);
                SurroundBack(position, w);
            });

            Assert.AreEqual(VoxelMask.Left | VoxelMask.Right, world.GetMask(position));
        }

        private World SurroundTest(VoxelPosition position, Action<World> surroundCallback)
        {
            var world = new World();
            surroundCallback(world);
            return world;
        }

        private void SurroundLeft(VoxelPosition position, World world)
        {
            world.Set(position.X - 1, position.Y, position.Z, 1);
        }
        private void SurroundRight(VoxelPosition position, World world)
        {
            world.Set(position.X + 1, position.Y, position.Z, 1);
        }
        private void SurroundForward(VoxelPosition position, World world)
        {
            world.Set(position.X, position.Y, position.Z - 1, 1);
        }
        private void SurroundBack(VoxelPosition position, World world)
        {
            world.Set(position.X, position.Y, position.Z + 1, 1);
        }
        private void SurroundUp(VoxelPosition position, World world)
        {
            world.Set(position.X, position.Y + 1, position.Z, 1);
        }
        private void SurroundDown(VoxelPosition position, World world)
        {
            world.Set(position.X, position.Y - 1, position.Z, 1);
        }

        [TestMethod]
        public void WHAT_WHEN_THEN()
        {
            World.ChunkSize = 16;
            var world = new World();

            world.Set(-32, -32, -32, 1);
            Assert.AreEqual(1, world.Get(-32, -32, -32));
        }


        [TestMethod]
        public void WHAT_WHEN_THEN_derpderp()
        {
            World.ChunkSize = 32;
            var world = new World();

            world.Set(-32, -32, -32, 1);
            Assert.AreEqual(1, world.Get(-32, -32, -32));
        }



        [TestMethod]
        public void Goldstuff()
        {
            var world = new World();
            Box(world, new VoxelPosition(0, 32, 0), 16, Voxel.Gold);

            var mask = world.GetMask(new VoxelPosition(2, 48, -3));

            Assert.AreEqual(VoxelMask.Up, mask);
        }


        [TestMethod]
        public void WHAT_WHEN_THEN__2()
        {
            World.ChunkSize = 16;
            var world = new World();

            int size = 129;

            for (int x = -size; x < size; x++)
                for (int z = -size; z < size; z++)
                    for (int y = -size; y < size; y++)
                        TestSetVoxel(world, x, y, z, 1);

        }

        [TestMethod]
        public void ____ffff_WHEN_THEN()
        {
            var world = new World();
            World.ChunkSize = 16;

            world.Set(3, 16, -17, 1);
            var chunkPosition = world.GetChunkPositionFromWorldPosition(new VoxelPosition(3, 16, -17));

            var chunk = world.GetOrCreateChunk(chunkPosition.X, chunkPosition.Y, chunkPosition.Z);
            //Assert.AreEqual(1, chunk.Data.Count(x => x == 1));
        }

        private void TestSetVoxel(World world, int x, int y, int z, int value)
        {
            world.Set(x, y, z, value);
            Assert.AreEqual(value, world.Get(x, y, z));
        }

        private void Box(World world, VoxelPosition origin, int size, int value)
        {
            for (int x = origin.X; x < origin.X + size; x++)
                for (int y = origin.Y; y < origin.Y + size; y++)
                    for (int z = origin.Z; z < origin.Z + size; z++)
                        world.Set(x, y, z, value);
        }
    }
}
