﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace AstralAssault;

public class Game1 : Game
{
    private enum Height { Full = 1080, Half = 540, Quarter = 270 }

    private enum Width { Full = 1920, Half = 960, Quarter = 480 }

    public GameStateMachine GameStateMachine;

    // render
    public SpriteBatch SpriteBatch;
    private RenderTarget2D m_renderTarget;
    private static readonly Effect HighlightEffect = AssetManager.Load<Effect>("Highlight");
    private static readonly Effect ColorEffect = AssetManager.Load<Effect>("Color");

    // display
    private static readonly Color BackgroundColor = new Color(28, 23, 41);
    public const int TargetWidth = (int)Width.Quarter;
    public const int TargetHeight = (int)Height.Quarter;
    private readonly Matrix m_scale;
    public readonly float ScaleX;
    public readonly float ScaleY;

    // debug tools
    public bool ShowDebug;
    private float m_frameRate;
    private float m_renderTime;
    private long m_lastStatUpdate;
    private const int StatUpdateInterval = 300;
    private KeyboardState m_prevKeyState = Keyboard.GetState();

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
        TextRenderer.Init();
        InputEventSource.Init();
        Palette.Init();

        GameStateMachine = new GameStateMachine(new GameplayState(this));

        base.Initialize();
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

        List<DrawTask> drawTasks = GameStateMachine.GetDrawTasks().OrderBy(static dt => (int)dt.LayerDepth).ToList();

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

            List<DrawTask> frameRateTask =
                frameRate.CreateDrawTasks(Vector2.Zero, Palette.GetColor(Palette.Colors.Yellow9), LayerDepth.Debug);
            List<DrawTask> renderTimeTask =
                renderTime.CreateDrawTasks
                    (new Vector2(0, 9), Palette.GetColor(Palette.Colors.Yellow9), LayerDepth.Debug);

            drawTasks.AddRange(frameRateTask);
            drawTasks.AddRange(renderTimeTask);
        }

        SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointWrap);

        foreach (DrawTask drawTask in drawTasks)
        {
            foreach (IDrawTaskEffect effect in drawTask.EffectContainer.Effects)
            {
                switch (effect)
                {
                case ColorEffect colorEffect:
                    ColorEffect.CurrentTechnique.Passes[1].Apply();
                    ColorEffect.Parameters["newColor"].SetValue(colorEffect.Color);
                    ColorEffect.CurrentTechnique.Passes[0].Apply();

                    break;
                }
            }

            SpriteBatch.Draw
            (
                drawTask.Texture,
                drawTask.Destination,
                drawTask.Source,
                Color.White,
                drawTask.Rotation,
                drawTask.Origin,
                SpriteEffects.None,
                0
            );

            HighlightEffect.CurrentTechnique.Passes[1].Apply();
            ColorEffect.CurrentTechnique.Passes[1].Apply();
        }

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
}