﻿using System.Collections.Generic;
using Asteroids2.Source.Graphics;

namespace Asteroids2.Source.Game.GameState;

public abstract class GameState : IUpdateEventListener
{
    public readonly Game1 Root;

    public GameState(Game1 root)
    {
        Root = root;
    }

    public abstract List<DrawTask> GetDrawTasks();
    public abstract void Enter();
    public abstract void Exit();
    public abstract void OnUpdate(object sender, UpdateEventArgs e);
}