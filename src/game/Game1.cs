using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AsteroidsV2._0
{
    internal sealed class Game1 : Game
    {
        internal readonly GraphicsDeviceManager Graphics;
        public Renderer Renderer;

        private readonly List<SpaceObject> _asteroids = new();
        private readonly List<SpaceObject> _bullets = new();
        private SpaceObject _player;
        private bool _dead;
        private int _score = 0;

        private List<Vector2> _shipModel;
        private readonly List<Vector2> _asteroidModel = new();

        public Game1()
        {
            Graphics = new(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Do render stuff
            Renderer = new(GraphicsDevice, this);

            // Actual game logic
            _shipModel = new()
            {
                new(0.0f, -5.0f),
                new(-2.5f, 2.5f),
                new(2.5f, 2.5f)
            };

            const int verts = 20;
            for (int i = 0; i < verts; i++)
            {
                float noise = (float)(new Random().NextDouble()) * 0.4f + 0.8f;
                _asteroidModel.Add(new(noise * MathF.Sin((i / (float)verts) * 6.28318f),
                    noise * MathF.Cos((i / (float)verts) * 6.28318f)));
            }

            ResetGame();
        }

        private void ResetGame()
        {
            _player = new(this, _shipModel, new(Renderer.RenderWidth / 2f, Renderer.RenderHeight / 2f), new(0, 0), 0, 1);

            _bullets.Clear();
            _asteroids.Clear();

            _asteroids.Add(new(this, _asteroidModel, new(20.0f, 20.0f), new(8.0f, -6.0f), 0.0f, 8));
            _asteroids.Add(new(this, _asteroidModel, new(100.0f, 20.0f), new(-5.0f, 3.0f), 0.0f, 8));

            _dead = false;
            _score = 0;
        }

        private static bool IsPointInsideCircle(float cx, float cy, float radius, float x, float y) => MathF.Sqrt((x -
            cx) * (x - cx) + (y - cy) * (y - cy)) < radius;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            KeyboardState keyboard = Keyboard.GetState();

            float elapsedTime = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

            if (_dead) ResetGame();

            // Steer Ship
            if (keyboard.IsKeyDown(Keys.Left)) _player.Angle -= 5.0f * elapsedTime;
            if (keyboard.IsKeyDown(Keys.Right)) _player.Angle += 5.0f * elapsedTime;

            // Thrust Apply accel
            if (keyboard.IsKeyDown(Keys.Up))
            {
                // ACCELERATION changes VELOCITY (with respect to time)
                _player.DPos += _player.MakeHeadingVector(20.0f * elapsedTime);
            }

            _player.Update(elapsedTime);

            // Check ship collision with asteroids
            for (int i = 0; i < _asteroids.Count; i++)
            {
                if (Game1.IsPointInsideCircle(_asteroids[i].Pos.X, _asteroids[i].Pos.Y, _asteroids[i].Size * 2 /* Radius be damned */, _player.Pos.X, _player.Pos.Y))
                    _dead = true; // Uh oh...
            }

            if (keyboard.IsKeyDown(Keys.Space))
            {
                var bulletHeading = _player.MakeHeadingVector(200.0f);
                _bullets.Add(new(this, null, _player.Pos, bulletHeading, 0, 0));
            }

            // Update asteroids
            for (int i = 0; i < _asteroids.Count; i++)
            {
                _asteroids[i].Angle += 0.5f * elapsedTime; // Add swanky rotation :)
                _asteroids[i].Update(elapsedTime);
            }

            List<SpaceObject> newAsteroids = new();

            // Update Bullets
            for (int i = 0; i < _bullets.Count; i++)
            {
                _bullets[i].Angle -= 1.0f * elapsedTime;
                _bullets[i].Update(elapsedTime);

                // Check collision with asteroids
                for (int j = 0; j < _asteroids.Count; j++)
                {
                    //if (IsPointInsideRectangle(a.x, a.y, a.x + a.nSize, a.y + a.nSize, b.x, b.y))
                    if (Game1.IsPointInsideCircle(_asteroids[j].Pos.X, _asteroids[j].Pos.Y, _asteroids[j].Size * 2, _bullets[i].Pos.X, _bullets[i].Pos.Y))
                    {
                        // Asteroid Hit - Remove bullet
                        // We've already updated the bullets, so force bullet to be offscreen
                        // so it is cleaned up by the removal algorithm.
                        _bullets[i].Pos.X = -100;

                        // Create child asteroids
                        if (_asteroids[j].Size > 1)
                        {
                            Random random = new();
                            float angle1 = (float)random.NextDouble() * 6.283185f;
                            float angle2 = (float)random.NextDouble() * 6.283185f;

                            newAsteroids.Add(new(this, _asteroidModel, _asteroids[j].Pos, new(10.0f * MathF.Sin(angle1), 10.0f * MathF.Cos(angle1)), 0.0f, _asteroids[j].Size >> 1));
                            newAsteroids.Add(new(this, _asteroidModel, _asteroids[j].Pos, new(10.0f * MathF.Sin(angle2), 10.0f * MathF.Cos(angle2)), 0.0f, _asteroids[j].Size >> 1));
                        }

                        // Remove asteroid - Same approach as bullets
                        _asteroids[j].Pos.X = -100;
                        _score += 100; // Small score increase for hitting asteroid
                    }
                }
            }

            // Append new asteroids to existing vector
            for (int i = 0; i < newAsteroids.Count; i++)
                _asteroids.Add(newAsteroids[i]);

            // Remove asteroids that have been blown up
            if (_asteroids.Count > 0) _asteroids.RemoveAll(static o => o.Pos.X < 0);

            // Remove bullets that have gone off-screen
            if (_bullets.Count > 0) _bullets.RemoveAll(static o => o.Pos.X < 1 || o.Pos.Y < 1 || o.Pos.X >= Renderer.RenderWidth - 1 || o.Pos.Y >= Renderer.RenderHeight - 1);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Renderer.ClearSIMD(new(28, 23, 41));
            
            _player.Draw(Color.White, false);
            
            for (int i = 0; i < _asteroids.Count; i++)
                _asteroids[i].Draw(Color.Yellow, false);
            
            for (int i = 0; i < _bullets.Count; i++)
                _bullets[i].Draw(Color.CornflowerBlue, true);

            Renderer.PixelPass();

            base.Draw(gameTime);
        }
    }
}