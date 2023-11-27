using System;
using System.Collections.Generic;
using AstralAssault.Source.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstralAssault.Source.Graphics.Controllers
{
    public class ExplosionController
    {
        private readonly Texture2D _explosionTexture;

        private readonly List<Explosion> _explosions = new();

        private const Int32 FrameDuration = 100;
        private const Int32 NumFrames = 8;
        private const Int32 ExplosionDuration = FrameDuration * NumFrames;
        private const Int32 FrameWidth = 32;
        private const Int32 FrameHeight = 32;

        public ExplosionController() {
            this._explosionTexture = AssetManager.Load<Texture2D>("Explosion");
        }

        public void SpawnExplosion(Vector2 position) {
            this._explosions.Add(new Explosion(position.ToPoint()));
        }

        public List<DrawTask> GetDrawTasks() {
            List<DrawTask> drawTasks = new();

            List<Explosion> toRemove = new();

            foreach (Explosion explosion in this._explosions) {
                if (explosion.TimeSinceSpawned >= ExplosionDuration) {
                    toRemove.Add(explosion);
                    continue;
                }

                drawTasks.Add(this.GetExplosionDrawTask(explosion));
            }

            foreach (Explosion explosion in toRemove) {
                this._explosions.Remove(explosion);
            }

            return drawTasks;
        }

        private DrawTask GetExplosionDrawTask(Explosion explosion) {
            Int32 spriteIndex = (Int32)(explosion.TimeSinceSpawned / FrameDuration) % NumFrames;
            Rectangle source = new(spriteIndex * FrameWidth, 0, FrameWidth, FrameHeight);

            return new DrawTask(this._explosionTexture,
                source,
                explosion.Position.ToVector2(),
                0,
                LayerDepth.Explosions,
                new List<IDrawTaskEffect>());
        }
    }
}