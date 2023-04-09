using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstralAssault;

public class GameplayState : GameState
{
    public readonly PixelRenderer PixelRenderer;

    private static Color BackgroundColor = new Color(28, 23, 41);

    public readonly List<Entity> Entities;
    public readonly CollisionSystem CollisionSystem = new CollisionSystem();
    public WaveController WaveController;

    public Player Player
    {
        get => (Player)Entities.Find(static entity => entity is Player);
    }

    public GameplayState(Game1 root) : base(root)
    {
        PixelRenderer = new PixelRenderer(root, Game1.TargetWidth, Game1.TargetHeight);

        Entities = new List<Entity>();
        WaveController = new WaveController(this, Root);
    }

    public override List<DrawTask> GetDrawTasks()
    {
        List<DrawTask> drawTasks = new List<DrawTask>();

        PixelRenderer.ClearSimd(BackgroundColor);

        foreach (Entity entity in Entities) drawTasks.AddRange(entity.GetDrawTasks());

        drawTasks.AddRange(WaveController.GetDrawTasks());


        Texture2D texture = PixelRenderer.GetPixelScreen();
        DrawTask pixelScreen = new DrawTask
        (
            texture, new Vector2(0, 0), 0, LayerDepth.Background, new List<IDrawTaskEffect>(),
            Palette.GetColor(Palette.Colors.Grey9), new Vector2(0, 0)
        );

        drawTasks.Add(pixelScreen);

        if (!Root.ShowDebug) return drawTasks;

        foreach (Collider collider in CollisionSystem.Colliders)
        {
            Texture2D circle = PixelRenderer.CreateCircle
                (collider.Radius, new Color(Palette.GetColor(Palette.Colors.Grey9), 0.15F));

            drawTasks.Add
            (
                new DrawTask
                (
                    circle,
                    collider.Position,
                    0,
                    LayerDepth.Debug,
                    new List<IDrawTaskEffect>(),
                    Palette.GetColor(Palette.Colors.Grey9),
                    Vector2.Zero
                )
            );
        }

        return drawTasks;
    }

    public override void Enter()
    {
        Entities.Add(new Player(this, new Vector2(Game1.TargetWidth / 2F, Game1.TargetHeight / 2F)));
        UpdateEventSource.UpdateEvent += OnUpdate;
    }

    public override void Exit()
    {
        while (Entities.Count > 0) Entities[0].Destroy();
        UpdateEventSource.UpdateEvent -= OnUpdate;
    }

    public override void OnUpdate(object sender, UpdateEventArgs e)
    {
        for (int i = 0; i < Entities.Count; i++) Entities[i].OnUpdate(e);

        CollisionSystem.OnUpdate(e);
        WaveController.OnUpdate(e);
    }
}