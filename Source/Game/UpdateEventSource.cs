using System;
using Microsoft.Xna.Framework;

namespace AstralAssault.Source.Game
{
    public static class UpdateEventSource
    {
        public static event EventHandler<UpdateEventArgs> UpdateEvent;

        public static void Update(GameTime gameTime) {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateEvent?.Invoke(null, new UpdateEventArgs(deltaTime));
        }
    }
}