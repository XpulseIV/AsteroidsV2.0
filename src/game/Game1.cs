using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AsteroidsV2._0
{
    public class Game1 : Game
    {
        internal GraphicsDeviceManager _graphics;
        private PixelRenderer _pixelRenderer;

        private List<SpaceObject> _asteroids = new();
        private List<SpaceObject> _bullets = new();
        public SpaceObject _player;
        private bool _dead;
        private int _score = 0;

        private List<Vector2> _shipModel;
        private List<Vector2> _asteroidModel = new();

        public Game1()
        {
            this._graphics = new(this);
            this.Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Fix render stuff
            this._pixelRenderer = new(GraphicsDevice,this);
            this._pixelRenderer.SetupWindow();

            // Actual game logic
            this._shipModel = new()
            {
                new(0.0f, -5.0f),
                new(-2.5f, 2.5f),
                new(2.5f, 2.5f),
            };

            const int verts = 20;
            for (int i = 0; i < verts; i++)
            {
                float noise = (float)(new Random().NextDouble()) * 0.4f + 0.8f;
                this._asteroidModel.Add(new(noise * MathF.Sin((i / (float)verts) * 6.28318f),
                    noise * MathF.Cos((i / (float)verts) * 6.28318f)));
            }

            ResetGame();
        }

        void ResetGame()
        {
            _player = new(this._shipModel, new(PixelRenderer.RenderWidth / 2f, PixelRenderer.RenderHeight / 2f), new(0, 0), 0, 1);

            _bullets.Clear();
            _asteroids.Clear();

            _asteroids.Add(new(this._asteroidModel, new(20.0f, 20.0f), new(8.0f, -6.0f), 0.0f, 16));
            _asteroids.Add(new(this._asteroidModel, new(100.0f, 20.0f), new(-5.0f, 3.0f), 0.0f, 16));

            this._dead = false;
            _score = 0;
        }

        private bool IsPointInsideCircle(float cx, float cy, float radius, float x, float y)
        {
            return MathF.Sqrt((x-cx)*(x-cx) + (y-cy)*(y-cy)) < radius;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();
            KeyboardState keyboard = Keyboard.GetState();

            float elapsedTime = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

            if (this._dead) this.ResetGame();

            // Steer Ship
            if (keyboard.IsKeyDown(Keys.Left))
                _player._angle -= 5.0f * elapsedTime;
            if (keyboard.IsKeyDown(Keys.Right))
                _player._angle += 5.0f * elapsedTime;

            // Thrust Apply accel
            if (keyboard.IsKeyDown(Keys.Up))
            {
                // ACCELERATION changes VELOCITY (with respect to time)
                _player._dPos.X += MathF.Sin(_player._angle) * 20.0f * elapsedTime;
                _player._dPos.Y += -MathF.Cos(_player._angle) * 20.0f * elapsedTime;
            }

            // VELOCITY changes POSITION (with respect to time)
            this._player._pos += this._player._dPos * elapsedTime;

            // Keep ship in gamespace
            this._player._pos = PixelRenderer.Wrap(this._player._pos);

            // Check ship collision with asteroids
            for (int i = 0; i < this._asteroids.Count; i++)
            {
                if (this.IsPointInsideCircle(this._asteroids[i]._pos.X, this._asteroids[i]._pos.Y, this._asteroids[i]._size * 2 /* Radius be damned */, this._player._pos.X, this._player._pos.Y))
                    this._dead = true; // Uh oh...
            }

            if (keyboard.IsKeyDown(Keys.Space))
                this._bullets.Add(new(null, this._player._pos,
                    new(50.0f * MathF.Sin(_player._angle), -50.0f * MathF.Cos(_player._angle)), 100, 0));

            // Update asteroids
            for (int i = 0; i < this._asteroids.Count; i++)
            {
                // VELOCITY changes POSITION (with respect to time)
                this._asteroids[i]._pos += this._asteroids[i]._dPos * elapsedTime;
                this._asteroids[i]._angle += 0.5f * elapsedTime; // Add swanky rotation :)

                // Asteroid coordinates are kept in game space (toroidal mapping)
                PixelRenderer.Wrap(this._asteroids[i]._pos);
            }

            List<SpaceObject> newAsteroids = new();

            // Update Bullets
            for (int i = 0; i < this._bullets.Count; i++)
            {
                this._bullets[i]._pos += this._bullets[i]._dPos * elapsedTime;
                this._bullets[i]._pos = PixelRenderer.Wrap(this._bullets[i]._pos);
                this._bullets[i]._angle -= 1.0f * elapsedTime;

                // Check collision with asteroids
                for (int j = 0; j < this._asteroids.Count; j++)
                {
                    //if (IsPointInsideRectangle(a.x, a.y, a.x + a.nSize, a.y + a.nSize, b.x, b.y))
                    if(IsPointInsideCircle(this._asteroids[j]._pos.X, this._asteroids[j]._pos.Y, this._asteroids[j]._size * 2, this._bullets[i]._pos.X, this._bullets[i]._pos.Y))
                    {
                        // Asteroid Hit - Remove bullet
                        // We've already updated the bullets, so force bullet to be offscreen
                        // so it is cleaned up by the removal algorithm.
                        this._bullets[i]._pos.X = -100;

                        // Create child asteroids
                        if (this._asteroids[j]._size > 4)
                        {
                            Random random = new();
                            float angle1 = (float)random.NextDouble() * 6.283185f;
                            float angle2 = (float)random.NextDouble() * 6.283185f;

                            newAsteroids.Add(new(this._asteroidModel, _asteroids[j]._pos, new(10.0f * MathF.Sin(angle1), 10.0f * MathF.Cos(angle1)), 0.0f, this._asteroids[j]._size >> 1));
                            newAsteroids.Add(new(this._asteroidModel, _asteroids[j]._pos, new(10.0f * MathF.Sin(angle2), 10.0f * MathF.Cos(angle2)), 0.0f, this._asteroids[j]._size >> 1));
                        }

                        // Remove asteroid - Same approach as bullets
                        this._asteroids[j]._pos.X = -100;
                        _score += 100; // Small score increase for hitting asteroid
                    }
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            this._pixelRenderer.Clear(Color.Black);

            // Draw player
            this._pixelRenderer.DrawWireFrameModel(this._player._objModel, this._player._pos.X, this._player._pos.Y, this._player._angle, this._player._size, Color.White);

            for (int i = 0; i < this._asteroids.Count; i++)
            {
                // Draw Asteroids
                this._pixelRenderer.DrawWireFrameModel(this._asteroidModel, _asteroids[i]._pos.X, _asteroids[i]._pos.Y, _asteroids[i]._angle, _asteroids[i]._size, Color.Yellow);
            }

            for (int i = 0; i < this._bullets.Count; i++)
            {
                // Draw bullets
                this._pixelRenderer.DrawPixel(this._bullets[i]._pos, Color.CornflowerBlue);
            }

            this._pixelRenderer.DrawPixels();

            base.Draw(gameTime);
        }
    }
}