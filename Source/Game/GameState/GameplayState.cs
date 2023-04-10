﻿#region
using System.Collections.Generic;
using Asteroids2.Source.Entity;
using Asteroids2.Source.Entity.Components;
using Asteroids2.Source.Entity.Entities;
using AstralAssault;
using Microsoft.Xna.Framework;
#endregion

namespace Asteroids2.Source.Game.GameState;

public class GameplayState : GameState
{
    public readonly CollisionSystem CollisionSystem = new CollisionSystem();

    public readonly List<Entity.Entity> Entities;
    public WaveController WaveController;

    public GameplayState(Game1 root) : base(root)
    {
        Entities = new List<Entity.Entity>();
        WaveController = new WaveController(this, Root);
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

        foreach (Collider collider in CollisionSystem.Colliders)
        {
            Root.PixelRenderer.DrawCircle
            (
                (int)collider.Position.X, (int)collider.Position.Y, collider.Radius,
                Palette.GetColor(Palette.Colors.Green8), 0xff
            );
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

        CollisionSystem.OnUpdate(e);
        WaveController.OnUpdate(e);
    }
}