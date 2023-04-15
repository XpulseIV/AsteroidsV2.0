#region
using System.Collections.Generic;
using Asteroids2.Source.Entity.Entities;
using Asteroids2.Source.Graphics;
using Microsoft.Xna.Framework;
using Quadtree = Asteroids2.Source.Entity.ColStuff.QuadTree;
#endregion

namespace Asteroids2.Source.Game.GameState;

public class GameplayState : GameState
{
    public readonly List<Entity.Entity> Entities;
    public WaveController WaveController;
    public readonly Quadtree QuadTree;

    public GameplayState(Game1 root) : base(root)
    {
        Entities = new List<Entity.Entity>();
        WaveController = new WaveController(this, Root);

        QuadTree = new Quadtree(0, new Rectangle(0, 0, Game1.TargetWidth, Game1.TargetWidth));
    }

    public Player Player
    {
        get => (Player)Entities.Find(static entity => entity is Player);
    }

    public override void Draw()
    {
        Root.PixelRenderer.ClearSimd(Game1.BackgroundColor);

        foreach (Entity.Entity entity in Entities) entity.Draw();
        WaveController.Draw();

        if (!Root.ShowDebug) return;

        Root.TextRenderer.DrawString(0, 21, Player.m_money.ToString(), Palette.GetColor(Palette.Colors.Blue7), 1);

        foreach (Entity.Entity entity in Entities)
        {
            Root.PixelRenderer.DrawCircle((int)entity.Position.X, (int)entity.Position.Y, entity.Bounds.Width, new Color(Palette.GetColor(Palette.Colors.Green8), 0.8f), 255);
        }
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

        QuadTree.Update(e.Gt, Entities);
        WaveController.OnUpdate(e);
    }
}