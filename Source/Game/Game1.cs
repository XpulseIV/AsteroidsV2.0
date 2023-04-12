#region
using System;
using Asteroids2.Source.Game.GameState;
using Asteroids2.Source.Game.GameState.MenuStuff;
using Asteroids2.Source.Graphics;
using Asteroids2.Source.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Asteroids2.Source.Game;

public class Game1 : Microsoft.Xna.Framework.Game
{
    internal static readonly Color BackgroundColor = Palette.GetColor(Palette.Colors.Black);

    // display
    public const int TargetWidth = (int)Width.Quarter;
    public const int TargetHeight = (int)Height.Quarter;
    private const int StatUpdateInterval = 300;
    private readonly Matrix m_scale;
    public readonly float ScaleX;
    public readonly float ScaleY;

    public GameStateMachine GameStateMachine;
    private float m_frameRate;
    private long m_lastStatUpdate;
    private KeyboardState m_prevKeyState = Keyboard.GetState();
    private RenderTarget2D m_renderTarget;
    private float m_renderTime;
    public PixelRenderer PixelRenderer;

    // debug tools
    public bool ShowDebug;

    // render
    public SpriteBatch SpriteBatch;
    public TextRenderer TextRenderer;

    public Game1()
    {
        // set up game class
        GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";

        // set up rendering
        graphics.PreferredBackBufferWidth = (int)Width.Half;
        graphics.PreferredBackBufferHeight = (int)Height.Half;

        ScaleX = graphics.PreferredBackBufferWidth / (float)TargetWidth;
        ScaleY = graphics.PreferredBackBufferHeight / (float)TargetHeight;
        m_scale = Matrix.CreateScale(new Vector3(ScaleX, ScaleY, 1));

        graphics.SynchronizeWithVerticalRetrace = false;
        IsFixedTimeStep = false;

        ShowDebug = false;
    }

    protected override void Initialize()
    {
        base.Initialize();

        PixelRenderer = new PixelRenderer(this, TargetWidth, TargetHeight);

        m_renderTarget = new RenderTarget2D
        (
            GraphicsDevice,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight,
            false,
            GraphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24
        );

        AssetManager.Init(this);
        TextRenderer = new TextRenderer();
        TextRenderer.Init(this);
        InputEventSource.Init();
        Palette.Init();

        GameStateMachine = new GameStateMachine(new GameplayState(this));
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (Keyboard.GetState().IsKeyDown(Keys.F3) && !m_prevKeyState.IsKeyDown(Keys.F3))
            ShowDebug = !ShowDebug;

        m_prevKeyState = Keyboard.GetState();

        UpdateEventSource.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // draw sprites to render target
        GraphicsDevice.SetRenderTarget(m_renderTarget);

        GameStateMachine.Draw();

        if (ShowDebug)
        {
            long timeNow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            if ((m_lastStatUpdate + StatUpdateInterval) < timeNow)
            {
                m_frameRate = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;
                m_renderTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                m_lastStatUpdate = timeNow;
            }

            string frameRate = Math.Round(m_frameRate).ToString();
            string renderTime = m_renderTime.ToString();

            TextRenderer.DrawString(0, 0, frameRate, Palette.GetColor(Palette.Colors.Yellow9), 1);
            TextRenderer.DrawString(0, 7, renderTime, Palette.GetColor(Palette.Colors.Yellow9), 1);
        }

        Texture2D finalScreen = PixelRenderer.GetPixelScreen();

        SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointWrap);

        SpriteBatch.Draw(finalScreen, new Rectangle(0, 0, TargetWidth, TargetHeight), Color.White);

        SpriteBatch.End();

        // draw render target to screen
        GraphicsDevice.SetRenderTarget(null);

        SpriteBatch.Begin
        (
            SpriteSortMode.Immediate, null, SamplerState.PointWrap,
            null, null, null, m_scale
        );
        SpriteBatch.Draw
        (
            m_renderTarget,
            new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
            Color.White
        );
        SpriteBatch.End();

        base.Draw(gameTime);
    }

    private enum Height { Full = 1080, Half = 540, Quarter = 270 }

    private enum Width { Full = 1920, Half = 960, Quarter = 480 }
}