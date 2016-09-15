using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SkyScape.Core.Cameras;
using SkyScape.Core.Components;
using SkyScape.Core.Effects;
using SkyScape.Core.Meshes;
using SkyScape.Core.Metrics;
using SkyScape.Core.Noise;
using SkyScape.Core.Shapes;
using SkyScape.Core.Voxels;
using SkyScape.Core.Voxels.Meshes;
using SkyScape.Core.Voxels.Threading;
using SkyScape.Core.WorldGeneration;
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
        private Game _game;

        StandardEffect _effect;

        private World _world;
        private Camera _cam;
        private FpsCameraController _camController;
        private WorldTraverserGenerator _traverser;
        private WorldGenerator _generator;

        private Effect _depthEffect;
        private ScreenSpaceAmbientOcclusion _ssao;
        private RenderTarget2D _mainTarget;
        private RenderTarget2D _depthTarget;
        private RenderTarget2D _ssaoTarget;
        private bool _ambientOcclusion = false;
        private RasterizerState _rasterizerState;

        private SpriteFont _debugFont;
        private FrameCounter _fpsCounter;


        public SkyScapeGame(Game game, GraphicsDevice graphics, ContentManager content, GraphicsDeviceManager graphicsDeviceManager)
        {
            _graphics = graphics;
            _content = content;
            _game = game;
            _graphicsDeviceManager = graphicsDeviceManager;
            _spriteBatch = new SpriteBatch(_graphics);
        }

        public void Initialize()
        {
            _graphicsDeviceManager.PreferredBackBufferWidth = 1280;
            _graphicsDeviceManager.PreferredBackBufferHeight = 720;
            _graphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
            _game.IsFixedTimeStep = true;
            _graphicsDeviceManager.ApplyChanges();

            _rasterizerState = _graphics.RasterizerState;

            _fpsCounter = new FrameCounter();

            ThreadPool.SetMaxThreads(4, 4);
        }

        public void Load()
        {
            _debugFont = _content.Load<SpriteFont>(@"Fonts/debug");
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
            
            _cam = new Camera(_graphicsDeviceManager.PreferredBackBufferWidth, _graphicsDeviceManager.PreferredBackBufferHeight);
            _cam.Transform.Position = new Vector3(32, 32, 32);
            _cam.FOV = 0.60f;

            _world = new World();

            _camController = new FpsCameraController(_cam);

            _generator = new WorldGenerator(0);
            _traverser = new WorldTraverserGenerator(_world, _generator);
            _traverser.Target = _cam.Transform;
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
            _effect.Alpha = 1f;
            var rand = new Random();

            var sw = new Stopwatch();
            sw.Start();

            int seed = 0;//rand.Next(0, 1000000);
            int worldSize = 512;

            var perlinTex = _content.Load<Texture2D>(@"Gfx/perlin_1");
            var pixels = TextureTo2DArray(perlinTex);
            int offset = 0;

            _world.Clear();

            //for (int x = 0; x < pixels.GetLength(0) / 2; x++)
            //{
            //    for (int z = 0; z < pixels.GetLength(1) / 2; z++)
            //    {
            //        var mag = Math.Min(pixels[z, x].R / 2, 64);

            //        for (int y = 0; y < mag; y++)
            //        {
            //            var voxel = (int)MathHelper.Lerp(0, Voxel.VoxelTypes.Length, y / (float)128) + 1;
            //            _world.Set(x + offset, y + offset, z + offset, voxel);
            //        }

            //    }
            //}

            var voxels = new[]
            {
                Voxel.Water,
                Voxel.Grass,
                Voxel.Stone,
                Voxel.Stone,
                Voxel.Snow
            };
            var noise = new NoiseGenerator(new Random(seed));
            noise.Octaves = 2;
            noise.Frequency = 0.1f;

            for (int x = 0; x < worldSize; x++)
            {
                for (int z = 0; z < worldSize; z++)
                {
                    var perlin = 0.5f + noise.Noise(x + seed, z + seed);

                    int height = (int)(perlin * worldSize * 0.2f);
                    for (int y = 0; y < height; y++)
                    {
                        var voxel = voxels[(int)MathHelper.Lerp(0, voxels.Length, y / (float)worldSize) + 1];
                        _world.Set(x + offset, y + offset, z + offset, voxel);
                    }
                }
            }

            //for (int i = 0; i < 32; i++)
            //{
            //    _world.Set(0, 0, i, Voxel.Grass);
            //    _world.Set(0, i, 0, Voxel.Rock);
            //}

            //_world.Set(0, 0, 0, Voxel.Rock);

            //Box(new VoxelPosition(0, 0, 0), 16, Voxel.Rock);
            //Box(new VoxelPosition(32, 0, 0), 16, Voxel.Grass);
            //Box(new VoxelPosition(0, 32, 0), 16, Voxel.Gold);
            //Box(new VoxelPosition(-32, -32, -32), 16, Voxel.Dirt);
            //Box(new VoxelPosition(-16, -16, -16), 16, Voxel.Water);

            sw.Stop();

            Console.WriteLine($"Set world: {sw.ElapsedMilliseconds} ms");
        }

        private void Box(VoxelPosition origin, int size, int value)
        {
            for (int x = origin.X; x < origin.X + size; x++)
                for (int y = origin.Y; y < origin.Y + size; y++)
                    for (int z = origin.Z; z < origin.Z + size; z++)
                        _world.Set(x, y, z, value);
        }

        KeyboardState _oldKeys;

        public void Update(GameTime gameTime)
        {
            var dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            var t = (float)gameTime.TotalGameTime.TotalMilliseconds;

            _fpsCounter.Update(dt * 0.001f);
            _game.Window.Title = "FPS: " + (int)(_fpsCounter.AverageFramesPerSecond + 1);


            _traverser.Update(dt);
            _world.Update(dt);
            _camController.Update(dt);

            var keyboard = Keyboard.GetState();

            if(keyboard.IsKeyDown(Keys.O) && _oldKeys.IsKeyUp(Keys.O))
            {
                _ambientOcclusion = !_ambientOcclusion;
            }

            float radius = 0.0001f;
            float area = 0.0001f;
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
                //Generate();
                Console.WriteLine($@"
                    Base: {_ssao.Base},
                    Falloff: {_ssao.Falloff},
                    Area: {_ssao.Area},
                    Radius: {_ssao.Radius}
                    ");
            }

            _oldKeys = keyboard;

        }

        private void DrawLine(Vector3 from, Vector3 to, Color color)
        {
            _graphics.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, new VertexPositionColor[]
            {
                new VertexPositionColor(from, color),
                new VertexPositionColor(to, color),
            }, 0, 1);

        }

        public void Draw(GameTime gameTime)
        {
            if (_ambientOcclusion)
            {
                _graphics.SetRenderTargets(_mainTarget, _depthTarget);
                _graphics.RasterizerState = _rasterizerState;
            }
            else
            {
                _graphics.SetRenderTargets(_mainTarget);
                _graphics.RasterizerState = new RasterizerState()
                {
                    FillMode = FillMode.WireFrame,
                    CullMode = CullMode.None
                };
            }

            _graphics.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
            _graphics.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Transparent, 1000f, 0);

            // Mesh renderer stuff
            _effect.View = _cam.View;
            _effect.Projection = _cam.Projection;
            _effect.FarClip = _cam.FarClip; // * 0.1f;

            _world.Render(_graphics, _cam, _effect);

            // debug lines
            {
                _effect.ApplyForModel(Matrix.CreateTranslation(0, 0, 0));
                _effect.CurrentTechnique.Passes[0].Apply();

                DrawLine(Vector3.Zero, Vector3.Forward * 500, Color.Blue);
                DrawLine(Vector3.Zero, Vector3.Right * 500, Color.Green);
                DrawLine(Vector3.Zero, Vector3.Up * 500, Color.Red);
            }


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
                _graphics.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1000f, 0);
                _spriteBatch.Begin();
                _spriteBatch.Draw(_mainTarget, new Rectangle(0, 0, 1280, 720), Color.White);
                _spriteBatch.End();
            }

            DrawUi();

        }

        private void DrawUi()
        {
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_debugFont, "P: " + _cam.Transform.Position.ToString(), new Vector2(16, 16), Color.LightGreen);
            _spriteBatch.DrawString(_debugFont, "Chunk P: " + _world.GetChunkPosition(_cam.Transform.Position).ToString(), new Vector2(16, 32), Color.LightGreen);
            _spriteBatch.DrawString(_debugFont, "Voxel P: " + _world.GetVoxelPosition(_cam.Transform.Position).ToString(), new Vector2(16, 32 + 16), Color.LightGreen);
            var camVoxelPosition = _world.GetVoxelPosition(_cam.Transform.Position);
            _spriteBatch.DrawString(_debugFont, "Mask: " + _world.GetMask(camVoxelPosition.X, camVoxelPosition.Y, camVoxelPosition.Z).ToString(), new Vector2(16, 32+ 32), Color.LightGreen);

            _spriteBatch.DrawString(_debugFont, "Chunk Q: " + _world.Batcher.JobsInQueue, new Vector2(16, 32 + 32 + 32), Color.LightGreen);
            _spriteBatch.DrawString(_debugFont, "Chunk gen: " + _world.Batcher.ActiveJobs, new Vector2(16, 32 + 32 + 32 + 16), Color.LightGreen);
            _spriteBatch.DrawString(_debugFont, "Chunk Q-dt: " + _world.Batcher.JobsAboutToBeConsumed, new Vector2(16, 32 + 32 + 32 + 32), Color.LightGreen);


            _spriteBatch.DrawString(_debugFont, "C Mean: " + ChunkGeneratorPerformanceTracker.Mean, new Vector2(16, 256), Color.Red);
            _spriteBatch.DrawString(_debugFont, "C Min: " + ChunkGeneratorPerformanceTracker.Min, new Vector2(16, 256 + 16), Color.Red);
            _spriteBatch.DrawString(_debugFont, "C Max: " + ChunkGeneratorPerformanceTracker.Max, new Vector2(16, 256 + 32), Color.Red);
            _spriteBatch.DrawString(_debugFont, "C Median: " + ChunkGeneratorPerformanceTracker.Median, new Vector2(16, 256 + 32 + 16), Color.Red);

            _spriteBatch.End();
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
