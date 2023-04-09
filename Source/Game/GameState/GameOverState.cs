using System;
using System.Collections.Generic;
using Asteroids2.Source.Graphics;
using Asteroids2.Source.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Asteroids2.Source.Game.GameState;

public class GameOverState : GameState, IKeyboardPressedEventListener
{
    private static Color BackgroundColor = new Color(28, 23, 41);

    private Texture2D m_gameOverText;
    private Texture2D m_restartPrompt;

    public GameOverState(Game1 root) : base(root)
    {
        InputEventSource.KeyboardPressedEvent += OnKeyboardPressedEvent;
    }

    public override void Draw()
    {
        Root.PixelRenderer.ClearSimd(BackgroundColor);

        Vector2 textPosition = new Vector2
        (
            (float)Math.Round(Game1.TargetWidth / 2D),
            (float)Math.Round(Game1.TargetHeight / 3D)
        );

        Vector2 promptPosition = new Vector2
        (
            (float)Math.Round(Game1.TargetWidth / 2D),
            (float)Math.Round(Game1.TargetHeight / 2D)
        );

        DrawTask gameOverText = new DrawTask
            (m_gameOverText, textPosition, 0, LayerDepth.HUD, new List<IDrawTaskEffect>());

        DrawTask restartPrompt = new DrawTask
            (m_restartPrompt, promptPosition, 0, LayerDepth.HUD, new List<IDrawTaskEffect>());

        Texture2D texture = Root.PixelRenderer.GetPixelScreen();
        DrawTask pixelScreen = new DrawTask
        (
            texture, new Vector2(0, 0), 0, LayerDepth.Background, new List<IDrawTaskEffect>(),
            Color.Gray, new Vector2(0, 0)
        );

        //return new List<DrawTask> { pixelScreen, gameOverText, restartPrompt };
    }

    public void OnKeyboardPressedEvent(object sender, KeyboardEventArgs e)
    {
        Root.GameStateMachine.ChangeState(new GameplayState(Root));
    }

    public override void Enter()
    {
        m_gameOverText = AssetManager.Load<Texture2D>("GameOver");
        m_restartPrompt = AssetManager.Load<Texture2D>("Restart");
    }

    public override void Exit()
    {
        InputEventSource.KeyboardPressedEvent -= OnKeyboardPressedEvent;
    }

    public override void OnUpdate(object sender, UpdateEventArgs e) { }
}