#region
using System;
using Asteroids2.Source.Graphics;
using Asteroids2.Source.Input;
using Microsoft.Xna.Framework;
#endregion

namespace Asteroids2.Source.Game.GameState;

public class GameOverState : GameState, IKeyboardPressedEventListener
{
    public GameOverState(Game1 root) : base(root)
    {
        InputEventSource.KeyboardPressedEvent += OnKeyboardPressedEvent;
    }

    public void OnKeyboardPressedEvent(object sender, KeyboardEventArgs e)
    {
        Root.GameStateMachine.ChangeState(new GameplayState(Root));
    }

    public override void Draw()
    {
        Root.PixelRenderer.ClearSimd(Game1.BackgroundColor);

        Vector2 textPosition = new Vector2
        (
            MathF.Round(Game1.TargetWidth / 2f),
            MathF.Round(Game1.TargetHeight / 3f)
        );

        Vector2 promptPosition = new Vector2
        (
            (float)Math.Round(Game1.TargetWidth / 2f),
            (float)Math.Round(Game1.TargetHeight / 2f)
        );

        const string gameOverString = "Game Over";
        const string restartString = "Press any key to continue!";

        int gOsLenght = Root.TextRenderer.StringLen(gameOverString, 2);
        int rsLenght = Root.TextRenderer.StringLen(restartString, 2);

        Root.TextRenderer.DrawString
        (
            (int)textPosition.X - gOsLenght, (int)textPosition.Y, gameOverString,
            Palette.GetColor(Palette.Colors.Grey9), 2
        );
        Root.TextRenderer.DrawString
        (
            (int)promptPosition.X - rsLenght, (int)promptPosition.Y, restartString,
            Palette.GetColor(Palette.Colors.Grey9), 2
        );
    }

    public override void Enter() { }

    public override void Exit()
    {
        InputEventSource.KeyboardPressedEvent -= OnKeyboardPressedEvent;
    }

    public override void OnUpdate(object sender, UpdateEventArgs e) { }
}