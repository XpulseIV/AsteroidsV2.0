using System;

namespace AstralAssault.Source.Game
{
    public class UpdateEventArgs : EventArgs
    {
        public float DeltaTime { get; }

        public UpdateEventArgs(float deltaTime) {
            this.DeltaTime = deltaTime;
        }
    }
}