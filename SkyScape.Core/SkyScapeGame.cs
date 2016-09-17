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
        private Texture2D _spriteSheet;
        Texture2D _corshair;

        private World _world;
        private Camera _cam;
        private FpsCameraController _camController;
        private WorldTraverserGenerator _traverser;
        private WorldGenerator _generator;

        private Effect _depthEffect, _bloom;
        private DepthOfField _depthOfField;
        private ScreenSpaceAmbientOcclusion _ssao;
        private RenderTarget2D _mainTarget;
        private RenderTarget2D _depthTarget;
        private RenderTarget2D _ssaoTarget;
        private RenderTarget2D _dofTarget;
        private bool _postProcessed = false;
        private RasterizerState _rasterizerState;

        private SpriteFont _debugFont;
        private FrameCounter _fpsCounter;

        private Mesh _selectBox;


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
            _game.IsFixedTimeStep = false;
            _graphicsDeviceManager.ApplyChanges();

            _rasterizerState = _graphics.RasterizerState;

            _fpsCounter = new FrameCounter();

            ThreadPool.SetMaxThreads(12, 4);
        }

        public void Load()
        {
            _debugFont = _content.Load<SpriteFont>(@"Fonts/debug");
            _depthEffect = _content.Load<Effect>(@"Shaders/Depth");
            _effect = new StandardEffect(_content.Load<Effect>(@"Shaders/Standard"));
            _spriteSheet = _content.Load<Texture2D>(@"Gfx/sheet");
            _effect.Texture = _spriteSheet;
            _depthOfField = new DepthOfField(_content.Load<Effect>(@"Shaders/DepthOfField"));
            _bloom  = _content.Load<Effect>(@"Shaders/Bloom");

            _corshair = _content.Load<Texture2D>(@"Gfx/corshair");

            _mainTarget = new RenderTarget2D(_graphics, 1280, 720, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            _ssaoTarget = new RenderTarget2D(_graphics, 1280, 720, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            _dofTarget = new RenderTarget2D(_graphics, 1280, 720, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
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

            GenerateSelectBox();
        }

        private void GenerateSelectBox()
        {
            var boxData = new MeshData();

            ShapeFactory.CreateCubeFace(boxData.Vertices, CubeFace.Left);
            ShapeFactory.CreateCubeFace(boxData.Vertices, CubeFace.Right);
            ShapeFactory.CreateCubeFace(boxData.Vertices, CubeFace.Top);
            ShapeFactory.CreateCubeFace(boxData.Vertices, CubeFace.Bottom);
            ShapeFactory.CreateCubeFace(boxData.Vertices, CubeFace.Front);
            ShapeFactory.CreateCubeFace(boxData.Vertices, CubeFace.Back);

            ShapeFactory.CreateCubeIndices(boxData.Indices, 0);
            ShapeFactory.CreateCubeIndices(boxData.Indices, 4);
            ShapeFactory.CreateCubeIndices(boxData.Indices, 8);
            ShapeFactory.CreateCubeIndices(boxData.Indices, 12 + 4);
            ShapeFactory.CreateCubeIndices(boxData.Indices, 12 + 8);
            ShapeFactory.CreateCubeIndices(boxData.Indices, 12 + 12);
            ShapeFactory.CreateCubeIndices(boxData.Indices, 12 + 20);

            ShapeFactory.CreateCubeNormals(boxData.Normals, CubeFace.Left);
            ShapeFactory.CreateCubeNormals(boxData.Normals, CubeFace.Right);
            ShapeFactory.CreateCubeNormals(boxData.Normals, CubeFace.Top);
            ShapeFactory.CreateCubeNormals(boxData.Normals, CubeFace.Bottom);
            ShapeFactory.CreateCubeNormals(boxData.Normals, CubeFace.Front);
            ShapeFactory.CreateCubeNormals(boxData.Normals, CubeFace.Back);

            boxData.Colors = new Color[] { }.AddRepeat(Color.White, 24).ToList();
            boxData.Optimize();

            _selectBox = new Mesh(_graphics, boxData);
        }

        KeyboardState _oldKeys;

        public void Update(GameTime gameTime)
        {
            var dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            var t = (float)gameTime.TotalGameTime.TotalMilliseconds;

            _fpsCounter.Update(dt * 0.001f);
            _game.Window.Title = "FPS: " + (int)(_fpsCounter.AverageFramesPerSecond + 1);


            _traverser.Update(dt, _cam);
            _world.Update(dt);
            _camController.Update(dt);

            var keyboard = Keyboard.GetState();

            if(keyboard.IsKeyDown(Keys.O) && _oldKeys.IsKeyUp(Keys.O))
            {
                _postProcessed = !_postProcessed;
            }

            #region SSAO
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

            #endregion

            var mState = Mouse.GetState();

            if (mState.LeftButton == ButtonState.Pressed || mState.RightButton == ButtonState.Pressed)
            {
                var rayHit = _world.RayCastFirst(_cam.Transform.Position, _cam.Transform.Forward, 16);
                if (rayHit.Hit)
                {
                    // This doesnt work since, world generator overwrites this
                    _world.Set(rayHit.Position, mState.LeftButton == ButtonState.Pressed ? Voxel.Rock : Voxel.Empty);
                    var chunk = _world.GetOrCreateChunk(rayHit.Position);
                    chunk?.MarkAsReadyToGenerate();
                }
            }


            _depthOfField.Update(dt);

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
            if (_postProcessed)
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

                // Ray hit block
                var rayHit = _world.RayCastFirst(_cam.Transform.Position, _cam.Transform.Forward, 16);
                if (rayHit.Hit)
                {
                    _effect.ApplyForModel(Matrix.CreateScale(1.1f, 1.1f, 1.1f) * Matrix.CreateTranslation(rayHit.WorldPosition));
                    _effect.Alpha = 0.2f;
                    _selectBox.Render(_graphics, _effect);
                    _effect.Alpha = 1f;

                }
            }


            if (_postProcessed)
            {

                // Calculate DoF distance
                var rayHit = _world.RayCastFirst(_cam.Transform.Position, _cam.Transform.Forward, 250);
                if (rayHit.Hit)
                {
                    var distance = Vector3.Distance(_cam.Transform.Position, rayHit.WorldPosition);
                    _depthOfField.Distance = distance;
                }
                else
                {
                    _depthOfField.Distance = 2f;
                }

                // render SSAO map
                _graphics.SetRenderTarget(_ssaoTarget);
                _ssao.DepthTexture = _depthTarget;
                _graphics.Clear(Color.White);
                _ssao.CurrentTechnique = _ssao.Techniques["SSAO"];
                _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, _ssao);
                _spriteBatch.Draw(_mainTarget, new Rectangle(0, 0, 1280, 720), Color.White);
                _spriteBatch.End();

                // Bake SSAO map with main target to dof target
                _graphics.SetRenderTarget(_dofTarget);
                _graphics.Clear(Color.White);
                _ssao.CurrentTechnique = _ssao.Techniques["SSAO_Apply"];
                _ssao.DepthTexture = _ssaoTarget;
                _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, _ssao);
                _spriteBatch.Draw(_mainTarget, new Rectangle(0, 0, 1280, 720), Color.White);
                _spriteBatch.End();

                // Render to BB with dof
                _graphics.SetRenderTarget(_mainTarget);
                _graphics.Clear(Color.Transparent);
                _depthOfField.DepthTexture = _depthTarget;
                _depthOfField.FarClip = _cam.FarClip;
                _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, _depthOfField);
                _spriteBatch.Draw(_dofTarget, new Rectangle(0, 0, 1280, 720), Color.White);
                _spriteBatch.End();

                _graphics.SetRenderTarget(null);
                _graphics.Clear(Color.Transparent);
                _bloom.Parameters["_Amount"].SetValue(0f);
                _bloom.Parameters["_Treshold"].SetValue(0.8f);
                _bloom.Parameters["_SampleDistance"].SetValue(0.001f);
                _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, _bloom);
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


            _spriteBatch.Begin();
            _spriteBatch.Draw(_corshair, new Vector2(1280, 720) * 0.5f, null, Color.White, 0f, new Vector2(8, 8), 1f, SpriteEffects.None, 0f);
            _spriteBatch.End();

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
