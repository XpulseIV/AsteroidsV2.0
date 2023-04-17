namespace Asteroids2.Source.Game.GameState.MenuStuff;

public class MainMenuState : GameState
{
    private Buttons m_selectedButton;

    private enum Buttons { Play, Shop, Exit }

    public MainMenuState(Game1 root) : base(root) { }

    public override void OnUpdate(object sender, UpdateEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    public override void Draw()
    {
        Root.PixelRenderer.ClearSimd(Game1.BackgroundColor);
    }

    public override void Enter() { }

    public override void Exit() { }
}