using System;

namespace Asteroids2.Source.Game;

public class UpdateEventArgs : EventArgs
{
    public float DeltaTime { get; }

    public UpdateEventArgs(float deltaTime)
    {
        DeltaTime = deltaTime;
    }
}