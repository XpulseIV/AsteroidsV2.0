using System;
using System.Collections.Generic;
using System.Linq;
using Asteroids2.Source.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace AstralAssault;

public class Game1 : Game
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
    private static readonly Color BackgroundColor = new(28, 23, 41);
    public const Int32 TargetWidth = (Int32)Width.Quarter;
    public const Int32 TargetHeight = (Int32)Height.Quarter;
    private Matrix _scale;
    public Single ScaleX;
    public Single ScaleY;
    private readonly GraphicsDeviceManager _graphics;

    // debug tools
    public Boolean ShowDebug;
    private Single _frameRate;
    private Single _renderTime;
    private Int64 _lastStatUpdate;
    private const Int32 StatUpdateInterval = 300;
    private KeyboardState _prevKeyState = Keyboard.GetState();

    public Game1()
    {
        // set up game class
        this._graphics = new GraphicsDeviceManager(this);
        this.Content.RootDirectory = "Content";

        // set up rendering
        this._graphics.PreferredBackBufferWidth = (Int32)Width.Half;
        this._graphics.PreferredBackBufferHeight = (Int32)Height.Half;

        this.ScaleX = this._graphics.PreferredBackBufferWidth / (Single)TargetWidth;
        this.ScaleY = this._graphics.PreferredBackBufferHeight / (Single)TargetHeight;
        this._scale = Matrix.CreateScale(new Vector3(this.ScaleX, this.ScaleY, 1));

        this._graphics.SynchronizeWithVerticalRetrace = false;
        this.IsFixedTimeStep = false;

        this.ShowDebug = false;
    }

    protected override void Initialize()
    {
        this.Renderer = new PixelRenderer(this, this._graphics.PreferredBackBufferWidth, this._graphics.PreferredBackBufferHeight);

        this._renderTarget = new RenderTarget2D(this.GraphicsDevice, this.GraphicsDevice.PresentationParameters.BackBufferWidth, this.GraphicsDevice.PresentationParameters.BackBufferHeight,
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

    protected override void LoadContent()
    {
        this._spriteBatch = new SpriteBatch(this.GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            this.Exit();

        if (Keyboard.GetState().IsKeyDown(Keys.F3) && !this._prevKeyState.IsKeyDown(Keys.F3)) this.ShowDebug = !this.ShowDebug;

        if (Keyboard.GetState().IsKeyDown(Keys.F) && !this._prevKeyState.IsKeyDown(Keys.F)) {
            if (this._graphics.IsFullScreen) {
                this._graphics.PreferredBackBufferWidth = (Int32)Width.Half;
                this._graphics.PreferredBackBufferHeight = (Int32)Height.Half;

                this._graphics.IsFullScreen = false;
                this._graphics.ApplyChanges();
            }
            else {
                this._graphics.PreferredBackBufferWidth = (Int32)Width.Full;
                this._graphics.PreferredBackBufferHeight = (Int32)Height.Full;

                this._graphics.IsFullScreen = true;
                this._graphics.ApplyChanges();
            }
        }

        this._prevKeyState = Keyboard.GetState();

        UpdateEventSource.Update(gameTime);

        this.GameStateMachine.Update((Single)gameTime.ElapsedGameTime.TotalSeconds);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // draw sprites to render target
        this.GraphicsDevice.SetRenderTarget(this._renderTarget);
        this.GraphicsDevice.Clear(BackgroundColor);

        List<DrawTask> drawTasks = new();

        drawTasks.AddRange(this.GameStateMachine.GetDrawTasks());

        drawTasks = drawTasks.OrderBy(dt => (Int32)dt.LayerDepth).ToList();

        this._spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointWrap);

        this._spriteBatch.Draw(
            this.Renderer.GetPixelScreen(),
            new Rectangle(0, 0, this.GraphicsDevice.Viewport.Width, this.GraphicsDevice.Viewport.Height),
            Color.White);

        this._spriteBatch.End();

        this.Renderer.ClearSimd(Color.Transparent);

        this._spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointWrap);

        foreach (DrawTask drawTask in drawTasks)
        {
            foreach (IDrawTaskEffect effect in drawTask.EffectContainer.Effects)
            {
                switch (effect)
                {
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

        this._spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointWrap,
            null, null, null, this._scale);
        this._spriteBatch.Draw(
            this._renderTarget,
            new Rectangle(0, 0, this.GraphicsDevice.Viewport.Width, this.GraphicsDevice.Viewport.Height),
            Color.White);
        this._spriteBatch.End();

        base.Draw(gameTime);
    }
}
