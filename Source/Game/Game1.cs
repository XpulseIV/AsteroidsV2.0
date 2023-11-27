using System;
using System.Collections.Generic;
using System.Linq;
using AstralAssault.Source.Game.GameState;
using AstralAssault.Source.Graphics;
using AstralAssault.Source.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AstralAssault.Source.Game
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public enum Width
        {
            Full = 1920,
            Half = 960,
            Quarter = 480
        }

        public enum Height
        {
            Full = 1080,
            Half = 540,
            Quarter = 270
        }

        public GameStateMachine GameStateMachine;
        public Int32 Score;
        public Int32 HighScore;

        // render
        public PixelRenderer Renderer;

        //public TextRenderer TextRenderer;
        private SpriteBatch _spriteBatch;
        public RenderTarget2D _renderTarget;
        private static readonly Effect ColorEffect = AssetManager.Load<Effect>("Color");
        private static readonly Effect HighlightEffect = AssetManager.Load<Effect>("Highlight");

        // display
        public const Int32 GameWidth = (Int32)Width.Quarter;
        public const Int32 GameHeight = (Int32)Height.Quarter;

        public Int32 RenderWidth = (Int32)Width.Half;
        public Int32 RenderHeight = (Int32)Height.Half;

        private static readonly Color BackgroundColor = new(28, 23, 41);
        private readonly GraphicsDeviceManager _graphics;

        // debug tools
        public Boolean ShowDebug;
        private Single _frameRate;
        private Single _renderTime;
        private Int64 _lastStatUpdate;
        private const Int32 StatUpdateInterval = 300;
        private KeyboardState _prevKeyState = Keyboard.GetState();

        public Game1() {
            // set up game class
            this._graphics = new GraphicsDeviceManager(this);
            this.Content.RootDirectory = "Content";

            // set up rendering
            this._graphics.PreferredBackBufferWidth = this.RenderWidth;
            this._graphics.PreferredBackBufferHeight = this.RenderHeight;

            this._graphics.SynchronizeWithVerticalRetrace = false;
            this.IsFixedTimeStep = false;

            this.ShowDebug = false;
        }

        protected override void Initialize() {
            this.Renderer = new PixelRenderer(this, GameWidth,
                GameHeight);

            this._renderTarget = new RenderTarget2D(this.GraphicsDevice,
                GameWidth,
                GameHeight,
                false, this.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

            AssetManager.Init(this);
            TextRenderer.Init();
            InputEventSource.Init();
            Palette.Init();
            Jukebox.Init();

            this.GameStateMachine = new GameStateMachine(new GameplayState(this));

            base.Initialize();
        }

        protected override void LoadContent() {
            this._spriteBatch = new SpriteBatch(this.GraphicsDevice);
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.F3) && !this._prevKeyState.IsKeyDown(Keys.F3))
                this.ShowDebug = !this.ShowDebug;

            if (Keyboard.GetState().IsKeyDown(Keys.F) && !this._prevKeyState.IsKeyDown(Keys.F)) {
                if (this._graphics.IsFullScreen) {
                    this.RenderWidth = (Int32)Width.Half;
                    this.RenderHeight = (Int32)Height.Half;

                    this._graphics.PreferredBackBufferWidth = this.RenderWidth;
                    this._graphics.PreferredBackBufferHeight = this.RenderHeight;

                    this._graphics.IsFullScreen = false;
                    this._graphics.ApplyChanges();
                }
                else {
                    this.RenderWidth = (Int32)Width.Full;
                    this.RenderHeight = (Int32)Height.Full;

                    this._graphics.PreferredBackBufferWidth = this.RenderWidth;
                    this._graphics.PreferredBackBufferHeight = this.RenderHeight;

                    this._graphics.IsFullScreen = true;
                    this._graphics.ApplyChanges();
                }
            }

            this._prevKeyState = Keyboard.GetState();

            UpdateEventSource.Update(gameTime);

            this.GameStateMachine.Update((Single)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            // Draw Screen to render target
            this.GraphicsDevice.SetRenderTarget(this._renderTarget);
            this.GraphicsDevice.Clear(BackgroundColor);

            List<DrawTask> drawTasks = new();

            drawTasks.AddRange(this.GameStateMachine.GetDrawTasks());

            drawTasks = drawTasks.OrderBy(dt => (Int32)dt.LayerDepth).ToList();

            this._spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointWrap);

            this._spriteBatch.Draw(
                this.Renderer.GetPixelScreen(),
                new Rectangle(0, 0, GameWidth, GameHeight),
                Color.White);

            this._spriteBatch.End();

            this.Renderer.ClearSimd(Color.Transparent);

            this._spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointWrap);

            foreach (DrawTask drawTask in drawTasks) {
                foreach (IDrawTaskEffect effect in drawTask.EffectContainer.Effects) {
                    switch (effect) {
                        case HighlightEffect highlightEffect:
                            HighlightEffect.CurrentTechnique.Passes[1].Apply();
                            HighlightEffect.Parameters["blendAlpha"].SetValue(highlightEffect.Alpha);
                            HighlightEffect.CurrentTechnique.Passes[0].Apply();
                            break;

                        case ColorEffect colorEffect:
                            ColorEffect.CurrentTechnique.Passes[1].Apply();
                            ColorEffect.Parameters["newColor"].SetValue(colorEffect.Color);
                            ColorEffect.CurrentTechnique.Passes[0].Apply();
                            break;
                    }
                }

                this._spriteBatch.Draw(
                    drawTask.Texture,
                    drawTask.Destination,
                    drawTask.Source,
                    Color.White,
                    drawTask.Rotation,
                    drawTask.Origin,
                    SpriteEffects.None,
                    0);

                HighlightEffect.CurrentTechnique.Passes[1].Apply();
                ColorEffect.CurrentTechnique.Passes[1].Apply();
            }

            this._spriteBatch.End();

            // draw render target to screen
            this.GraphicsDevice.SetRenderTarget(null);

            this._spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointWrap);
            this._spriteBatch.Draw(
                this._renderTarget,
                new Rectangle(0, 0, this.GraphicsDevice.Viewport.Width, this.GraphicsDevice.Viewport.Height),
                Color.White);
            this._spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
