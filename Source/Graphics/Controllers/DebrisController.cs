using System;
using System.Collections.Generic;
using AstralAssault.Source.Game;
using AstralAssault.Source.Game.GameState;
using AstralAssault.Source.Graphics.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstralAssault.Source.Graphics.Controllers
{
    public class DebrisController
    {
        private readonly ParticleEmitter _particleEmitter;
        private readonly Random _rnd = new();
        private readonly GameplayState _gameplayState;
        private readonly List<Tuple<Int64, Tuple<Vector2, Int32>>> _scores = new(); // (timeSpawned, (coords, score))

        public DebrisController(GameplayState gameplayState) {
            this._gameplayState = gameplayState;

            Texture2D particleSpriteSheet = AssetManager.Load<Texture2D>("AsteroidDebris");

            Rectangle[] textureSources = {
                new(0, 0, 8, 8),
                new(8, 0, 8, 8),
                new(16, 0, 8, 8),
                new(32, 0, 8, 8)
            };

            IParticleProperty[] particleProperties = {
                new VelocityProperty(-1, 1, 0.05F, 0.15F),
                new RandomSpriteProperty(0, textureSources.Length - 1),
                new CauseOfDeathProperty(CauseOfDeathProperty.CausesOfDeath.OutOfBounds)
            };

            this._particleEmitter = new ParticleEmitter(
                particleSpriteSheet,
                textureSources,
                0,
                Vector2.Zero,
                0,
                particleProperties,
                LayerDepth.Debris);
        }

        public void SpawnDebris(Vector2 position, Int32 asteroidSize) {
            Int32 amount = this._rnd.Next(4, (1 + asteroidSize) * 4);

            Single angleFromPlayer = MathF.Atan2(
                position.Y - this._gameplayState.Player.Position.Y,
                position.X - this._gameplayState.Player.Position.X);

            this._particleEmitter.SetTransform(position, angleFromPlayer);
            this._particleEmitter.StartSpawning(amount);
        }

        public void SpawnScoreText(Vector2 position, Int32 score) {
            this._scores.Add(new Tuple<Int64, Tuple<Vector2, Int32>>(
                DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond,
                new Tuple<Vector2, Int32>(position, score)));
        }

        public List<DrawTask> GetDrawTasks() {
            List<DrawTask> drawTasks = this._particleEmitter.CreateDrawTasks();

            if (this._scores.Count == 0) return drawTasks;

            Int64 timeNow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            for (Int32 i = 0; i < this._scores.Count; i++) {
                if (timeNow - this._scores[i].Item1 > 1000) {
                    this._scores.RemoveAt(i);
                    i--;
                    continue;
                }

                String scoreText = ((Int32)(this._scores[i].Item2.Item2 * (this._gameplayState.Player.Multiplier - 0.1F)))
                    .ToString();
                Int32 scoreX = (Int32)this._scores[i].Item2.Item1.X - scoreText.Length * 4;
                Vector2 scorePosition = new(scoreX, this._scores[i].Item2.Item1.Y - 5);

                drawTasks.AddRange(scoreText.CreateDrawTasks(
                    scorePosition,
                    Palette.GetColor(Palette.Colors.Grey9),
                    LayerDepth.Background));
            }

            return drawTasks;
        }
    }
}