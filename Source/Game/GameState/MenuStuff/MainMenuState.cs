using Asteroids2.Source.Graphics;

namespace Asteroids2.Source.Game.GameState.MenuStuff;

public class MainMenuState : GameState
{
    private string title;
    private string title2;

    public MainMenuState(Game1 root) : base(root)
    {
        title = "How it work V3";
        title2 = "The game guide";
    }

    public override void OnUpdate(object sender, UpdateEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    public override void Draw()
    {
        Root.PixelRenderer.ClearSimd(Game1.BackgroundColor);

        Root.TextRenderer.DrawString(Game1.TargetWidth / 2 - Root.TextRenderer.StringLen(title, 4) / 2, 7, title, Palette.GetColor(Palette.Colors.Grey9), 4);
        Root.TextRenderer.DrawString(Game1.TargetWidth / 2 - Root.TextRenderer.StringLen(title2, 2) / 2, 42, title2, Palette.GetColor(Palette.Colors.Grey9), 2);
    }

    public override void Enter() { }

    public override void Exit() { }
}