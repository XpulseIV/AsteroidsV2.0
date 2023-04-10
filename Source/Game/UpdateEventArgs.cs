#region
using System;
#endregion

namespace Asteroids2.Source.Game;

public class UpdateEventArgs : EventArgs
{
    public UpdateEventArgs(float deltaTime)
    {
        DeltaTime = deltaTime;
    }

    public float DeltaTime { get; }
}