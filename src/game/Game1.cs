using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            this.Graphics = new(this);
            this.Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Do render stuff
            this.Renderer = new(this.GraphicsDevice, this);

            // Actual game logic
            this._shipModel = new()
            {
                new(0.0f, -5.0f),
                new(-2.5f, 2.5f),
                new(2.5f, 2.5f)
            };

            const int verts = 20;
            for (int i = 0; i < verts; i++)
            {
                float noise = (float)(new Random().NextDouble()) * 0.4f + 0.8f;
                this._asteroidModel.Add(new(noise * MathF.Sin((i / (float)verts) * 6.28318f),
                    noise * MathF.Cos((i / (float)verts) * 6.28318f)));
            }

            this.ResetGame();
        }

        private void ResetGame()
        {
            this._player = new(this, this._shipModel, new(Renderer.RenderWidth / 2f, Renderer.RenderHeight / 2f), new(0, 0), 0, 1);

            this._bullets.Clear();
            this._asteroids.Clear();

            this._asteroids.Add(new(this, this._asteroidModel, new(20.0f, 20.0f), new(8.0f, -6.0f), 0.0f, 8));
            this._asteroids.Add(new(this, this._asteroidModel, new(100.0f, 20.0f), new(-5.0f, 3.0f), 0.0f, 8));

            this._dead = false;
            this._score = 0;
        }

        private static bool IsPointInsideCircle(float cx, float cy, float radius, float x, float y) => MathF.Sqrt((x -
            cx) * (x - cx) + (y - cy) * (y - cy)) < radius;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            KeyboardState keyboard = Keyboard.GetState();

            float elapsedTime = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

            if (this._dead) this.ResetGame();

            // Steer Ship
            if (keyboard.IsKeyDown(Keys.Left)) this._player.Angle -= 5.0f * elapsedTime;
            if (keyboard.IsKeyDown(Keys.Right)) this._player.Angle += 5.0f * elapsedTime;

            // Thrust Apply accel
            if (keyboard.IsKeyDown(Keys.Up))
            {
                // ACCELERATION changes VELOCITY (with respect to time)
                this._player.DPos += _player.MakeHeadingVector(20.0f * elapsedTime);
            }

            this._player.Update(elapsedTime);

            // Check ship collision with asteroids
            for (int i = 0; i < this._asteroids.Count; i++)
            {
                if (Game1.IsPointInsideCircle(this._asteroids[i].Pos.X, this._asteroids[i].Pos.Y, this._asteroids[i].Size * 2 /* Radius be damned */, this._player.Pos.X, this._player.Pos.Y))
                    this._dead = true; // Uh oh...
            }

            if (keyboard.IsKeyDown(Keys.Space))
            {
                var bulletHeading = _player.MakeHeadingVector(50.0f);
                this._bullets.Add(new(this, null, this._player.Pos, bulletHeading, 0, 0));
            }

            // Update asteroids
            for (int i = 0; i < this._asteroids.Count; i++)
            {
                this._asteroids[i].Angle += 0.5f * elapsedTime; // Add swanky rotation :)
                this._asteroids[i].Update(elapsedTime);
            }

            List<SpaceObject> newAsteroids = new();

            // Update Bullets
            for (int i = 0; i < this._bullets.Count; i++)
            {
                this._bullets[i].Angle -= 1.0f * elapsedTime;
                this._bullets[i].Update(elapsedTime);

                // Check collision with asteroids
                for (int j = 0; j < this._asteroids.Count; j++)
                {
                    //if (IsPointInsideRectangle(a.x, a.y, a.x + a.nSize, a.y + a.nSize, b.x, b.y))
                    if (Game1.IsPointInsideCircle(this._asteroids[j].Pos.X, this._asteroids[j].Pos.Y, this._asteroids[j].Size * 2, this._bullets[i].Pos.X, this._bullets[i].Pos.Y))
                    {
                        // Asteroid Hit - Remove bullet
                        // We've already updated the bullets, so force bullet to be offscreen
                        // so it is cleaned up by the removal algorithm.
                        this._bullets[i].Pos.X = -100;

                        // Create child asteroids
                        if (this._asteroids[j].Size > 1)
                        {
                            Random random = new();
                            float angle1 = (float)random.NextDouble() * 6.283185f;
                            float angle2 = (float)random.NextDouble() * 6.283185f;

                            newAsteroids.Add(new(this, this._asteroidModel, this._asteroids[j].Pos, new(10.0f * MathF.Sin(angle1), 10.0f * MathF.Cos(angle1)), 0.0f, this._asteroids[j].Size >> 1));
                            newAsteroids.Add(new(this, this._asteroidModel, this._asteroids[j].Pos, new(10.0f * MathF.Sin(angle2), 10.0f * MathF.Cos(angle2)), 0.0f, this._asteroids[j].Size >> 1));
                        }

                        // Remove asteroid - Same approach as bullets
                        this._asteroids[j].Pos.X = -100;
                        this._score += 100; // Small score increase for hitting asteroid
                    }
                }
            }

            // Append new asteroids to existing vector
            for (int i = 0; i < newAsteroids.Count; i++)
                this._asteroids.Add(newAsteroids[i]);

            // Remove asteroids that have been blown up
            if (this._asteroids.Count > 0) this._asteroids.RemoveAll(static o => o.Pos.X < 0);

            // Remove bullets that have gone off-screen
            if (this._bullets.Count > 0) this._bullets.RemoveAll(static o => o.Pos.X < 1 || o.Pos.Y < 1 || o.Pos.X >= Renderer.RenderWidth - 1 || o.Pos.Y >= Renderer.RenderHeight - 1);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            this.Renderer.Clear(Color.Black);

            this._player.Draw(Color.White, false);

            for (int i = 0; i < this._asteroids.Count; i++)
                this._asteroids[i].Draw(Color.Yellow, false);

            for (int i = 0; i < this._bullets.Count; i++)
                this._bullets[i].Draw(Color.CornflowerBlue, true);

            this.Renderer.PixelPass();

            base.Draw(gameTime);
        }
    }
}