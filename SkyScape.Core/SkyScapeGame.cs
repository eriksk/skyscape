using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SkyScape.Core.Cameras;
using SkyScape.Core.Components;
using SkyScape.Core.Effects;
using SkyScape.Core.Meshes;
using SkyScape.Core.Noise;
using SkyScape.Core.Shapes;
using SkyScape.Core.Voxels;
using SkyScape.Core.Voxels.Meshes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkyScape.Core
{
    public class SkyScapeGame : ISkyScapeGame
    {
        private GraphicsDeviceManager _graphicsDeviceManager;
        private GraphicsDevice _graphics;
        private ContentManager _content;
        private SpriteBatch _spriteBatch;

        StandardEffect _effect;

        private World _world;
        private Camera _cam;
        private FpsCameraController _camController;

        private Effect _depthEffect;
        private ScreenSpaceAmbientOcclusion _ssao;
        private RenderTarget2D _mainTarget;
        private RenderTarget2D _depthTarget;
        private RenderTarget2D _ssaoTarget;
        private bool _ambientOcclusion = false;


        public SkyScapeGame(GraphicsDevice graphics, ContentManager content, GraphicsDeviceManager graphicsDeviceManager)
        {
            _graphics = graphics;
            _content = content;
            _graphicsDeviceManager = graphicsDeviceManager;
            _spriteBatch = new SpriteBatch(_graphics);
        }

        public void Initialize()
        {
            _graphicsDeviceManager.PreferredBackBufferWidth = 1280;
            _graphicsDeviceManager.PreferredBackBufferHeight = 720;
            _graphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
            _graphicsDeviceManager.ApplyChanges();

            ThreadPool.SetMaxThreads(4, 4);
        }

        public void Load()
        {

            _depthEffect = _content.Load<Effect>(@"Shaders/Depth");
            _effect = new StandardEffect(_content.Load<Effect>(@"Shaders/Standard"));

            _mainTarget = new RenderTarget2D(_graphics, 1280, 720, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            _ssaoTarget = new RenderTarget2D(_graphics, 1280, 720, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            _depthTarget = new RenderTarget2D(_graphics,
                                                1280,
                                                720,
                                                false,
                                                SurfaceFormat.Single,
                                                DepthFormat.Depth24);


            _ssao = new ScreenSpaceAmbientOcclusion(_content.Load<Effect>(@"Shaders/SSAO"));
            _ssao.RandomTextureSampler = _content.Load<Texture2D>(@"Shaders/ssao_random_vectors");
            _ssao.DepthTexture = _depthTarget;

            _world = new World();

            _cam = new Camera(_graphicsDeviceManager.PreferredBackBufferWidth, _graphicsDeviceManager.PreferredBackBufferHeight);
            _cam.Transform.Position = new Vector3(32, 32, 32);
            _cam.FOV = 0.60f;

            _camController = new FpsCameraController(_cam);

            Generate();

            var sw = new Stopwatch();
            sw.Start();

            _world.Clean(_graphics);

            sw.Stop();
            Console.WriteLine($"Generate meshes: {sw.ElapsedMilliseconds} ms, skipped {ChunkMeshGenerator.SkippedBlocks} blocks");
        }

        private Color[,] TextureTo2DArray(Texture2D texture)
        {
            Color[] colorsOne = new Color[texture.Width * texture.Height]; //The hard to read,1D array
            texture.GetData(colorsOne); //Get the colors and add them to the array

            Color[,] colorsTwo = new Color[texture.Width, texture.Height]; //The new, easy to read 2D array
            for (int x = 0; x < texture.Width; x++) //Convert!
                for (int y = 0; y < texture.Height; y++)
                    colorsTwo[x, y] = colorsOne[x + y * texture.Width];

            return colorsTwo; //Done!
        }

        private void Generate()
        {

            var rand = new Random();

            var sw = new Stopwatch();
            sw.Start();

            int seed = rand.Next(0, 1000000);
            int worldSize = 64;

            var perlinTex = _content.Load<Texture2D>(@"Gfx/perlin_1");
            var pixels = TextureTo2DArray(perlinTex);
            int offset = 32;

            for (int x = 0; x < pixels.GetLength(0)/ 4; x++)
            {
                for (int z = 0; z < pixels.GetLength(1) / 4; z++)
                {
                    var mag = Math.Min(pixels[z, x].R / 2, 64);

                    //_world.Set(x + 32, mag + 32, z + 32, Voxel.Grass);

                    for (int y = 0; y < mag; y++)
                    {
                        var voxel = (int)MathHelper.Lerp(0, Voxel.VoxelTypes.Length, y / (float)128) + 1;
                        _world.Set(x + offset, y + offset, z + offset, voxel);
                    }

                }
            }

            //for (int x = 0; x < worldSize; x++)
            //{
            //    for (int z = 0; z < worldSize; z++)
            //    {
            //        var perlin = 1f + NoiseGenerator.Noise(x + seed, z + seed);

            //        int height = (int)(perlin * worldSize * 0.3f);
            //        for (int y = 0; y < height; y++)
            //        {
            //            _world.Set(x, y, z, Voxel.Grass);
            //        }
            //        // stair
            //        //_world.Set(x, worldSize - z, z, Voxel.Grass);

            //    }
            //}

            //for (int i = 0; i < 32; i++)
            //{
            //    _world.Set(0, 0, i, Voxel.Grass);
            //    _world.Set(0, i, 0, Voxel.Rock);
            //}

            //_world.Set(0, 0, 0, Voxel.Rock);
            //for (int x = 0; x < worldSize; x++)
            //{
            //    for (int z = 0; z < worldSize; z++)
            //    {
            //        for (int y = 0; y < worldSize; y++)
            //        {
            //            _world.Set(x, y, z, Voxel.Rock);
            //        }
            //    }
            //}
            sw.Stop();

            Console.WriteLine($"Set world: {sw.ElapsedMilliseconds} ms");
        }

        KeyboardState _oldKeys;

        public void Update(GameTime gameTime)
        {
            var dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            var t = (float)gameTime.TotalGameTime.TotalMilliseconds;

            _camController.Update(dt);

            var keyboard = Keyboard.GetState();

            if(keyboard.IsKeyDown(Keys.O) && _oldKeys.IsKeyUp(Keys.O))
            {
                _ambientOcclusion = !_ambientOcclusion;
            }

            float radius = 0.0001f;
            float area = 0.001f;
            float fallOff = 0.000001f;
            float _base = 0.0001f;
            float blur = 0.01f;

            if (keyboard.IsKeyDown(Keys.NumPad1))
            {
                _ssao.Radius += radius * dt;
            }
            else if (keyboard.IsKeyDown(Keys.NumPad2))
            {
                _ssao.Radius -= radius * dt;
            }
            if (keyboard.IsKeyDown(Keys.NumPad4))
            {
                _ssao.Area += area * dt;
            }
            else if (keyboard.IsKeyDown(Keys.NumPad5))
            {
                _ssao.Area -= area * dt;
            }
            if (keyboard.IsKeyDown(Keys.NumPad7))
            {
                _ssao.Falloff += fallOff * dt;
            }
            else if (keyboard.IsKeyDown(Keys.NumPad8))
            {
                _ssao.Falloff -= fallOff * dt;
            }
            if (keyboard.IsKeyDown(Keys.NumPad9))
            {
                _ssao.Base += _base * dt;
            }
            else if (keyboard.IsKeyDown(Keys.NumPad6))
            {
                _ssao.Base -= _base * dt;
            }
            if (keyboard.IsKeyDown(Keys.B))
            {
                _ssao.Blur += blur * dt;
            }
            else if (keyboard.IsKeyDown(Keys.N))
            {
                _ssao.Blur -= blur * dt;
            }

            if(Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                _world.SetFromPosition(_cam.Transform.Position, Voxel.Rock);
            }

            if (keyboard.IsKeyDown(Keys.Enter))
            {
                Generate();
                Console.WriteLine($@"
                    Base: {_ssao.Base},
                    Falloff: {_ssao.Falloff},
                    Area: {_ssao.Area},
                    Radius: {_ssao.Radius}
                    ");
            }

            _oldKeys = keyboard;

        }

        public void Draw(GameTime gameTime)
        {
            if (_ambientOcclusion)
            {
                _graphics.SetRenderTargets(_mainTarget, _depthTarget);
            }
            else
            {
                _graphics.SetRenderTargets(_mainTarget);
            }

            _graphics.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
            _graphics.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Transparent, 1000f, 0);

            // Mesh renderer stuff
            _effect.View = _cam.View;
            _effect.Projection = _cam.Projection;
            _effect.FarClip = _cam.FarClip; // * 0.1f;

            _world.Render(_graphics, _cam, _effect);

            //_effect.World = Matrix.CreateTranslation(0, 0, 0);
            //_effect.CurrentTechnique.Passes[0].Apply();
            //_graphics.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, new VertexPositionColor[]
            //{
            //    new VertexPositionColor(Vector3.Zero, Color.Blue),
            //    new VertexPositionColor(Vector3.Forward * 500, Color.Blue),
            //    new VertexPositionColor(Vector3.Zero, Color.Green),
            //    new VertexPositionColor(Vector3.Right * 500, Color.Green),
            //    new VertexPositionColor(Vector3.Zero, Color.Red),
            //    new VertexPositionColor(Vector3.Up * 500, Color.Red),
            //}, 0, 3);


            if (_ambientOcclusion)
            {
                // render SSAO map
                _graphics.SetRenderTarget(_ssaoTarget);
                _ssao.DepthTexture = _depthTarget;
                _graphics.Clear(Color.White);
                _ssao.CurrentTechnique = _ssao.Techniques["SSAO"];
                _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, _ssao);
                _spriteBatch.Draw(_mainTarget, new Rectangle(0, 0, 1280, 720), Color.White);
                _spriteBatch.End();

                // Bake SSAO map with main target
                _graphics.SetRenderTarget(null);
                _graphics.Clear(Color.CornflowerBlue);
                _ssao.CurrentTechnique = _ssao.Techniques["SSAO_Apply"];
                _ssao.DepthTexture = _ssaoTarget;
                _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, _ssao);
                _spriteBatch.Draw(_mainTarget, new Rectangle(0, 0, 1280, 720), Color.White);
                _spriteBatch.End();
            }
            else
            {
                _graphics.SetRenderTarget(null);
                _graphics.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.CornflowerBlue, 1000f, 0);
                _spriteBatch.Begin();
                _spriteBatch.Draw(_mainTarget, new Rectangle(0, 0, 1280, 720), Color.White);
                _spriteBatch.End();
            }



        }
    }

    public interface ISkyScapeGame
    {
        void Initialize();
        void Load();
        void Update(GameTime gameTime);
        void Draw(GameTime gameTime);
    }
}
