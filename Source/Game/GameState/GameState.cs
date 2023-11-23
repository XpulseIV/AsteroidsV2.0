using System.Collections.Generic;

namespace AstralAssault;

public abstract class GameState
{
    public readonly Game1 Root;

    public GameState(Game1 root)
    {
        this.Root = root;
    }

    public abstract void Enter();
    public abstract void Exit();
    public abstract void Update(float deltaTime);
    public abstract List<DrawTask> GetDrawTasks();
}