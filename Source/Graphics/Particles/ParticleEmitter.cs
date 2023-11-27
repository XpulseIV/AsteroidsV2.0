using System;
using System.Collections.Generic;
using System.Linq;
using AstralAssault.Source.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstralAssault.Source.Graphics.Particles
{
    public class ParticleEmitter
    {
        private readonly Texture2D _spriteSheet;
        private readonly Rectangle[] _textureSources;
        private readonly Int32 _particlesPerSecond;
        private readonly List<Particle> _particles = new();
        private readonly IParticleProperty[] _particleProperties;
        private readonly LayerDepth _layerDepth;
        private Vector2 _position;
        private Single _rotation;
        private Single TimeBetweenParticles => 1000F / this._particlesPerSecond;
        private Int32 _particlesSpawned;
        private Int32 _particlesToSpawn;
        private Int64 _lastTimeSpawned;
        private Boolean _isSpawning;

        public ParticleEmitter(
            Texture2D spriteSheet,
            Rectangle[] textureSources,
            Int32 particlesPerSecond,
            Vector2 position,
            Single rotation,
            IParticleProperty[] particleProperties,
            LayerDepth layerDepth) {
            this._spriteSheet = spriteSheet;
            this._textureSources = textureSources;
            this._particlesPerSecond = particlesPerSecond;
            this._position = position;
            this._rotation = rotation;
            this._particleProperties = particleProperties;
            this._layerDepth = layerDepth;

            List<Type> particlePropertyTypes = new();
            foreach (IParticleProperty particleProperty in particleProperties) {
                Type particlePropertyType = particleProperty.GetType();
                if (particlePropertyTypes.Contains(particlePropertyType)) {
                    throw new ArgumentException();
                }
            }

            Type causeOfDeathPropertyType = typeof(CauseOfDeathProperty);
            if (!particleProperties.Any(p => causeOfDeathPropertyType.IsInstanceOfType(p))) {
                throw new ArgumentException();
            }
        }

        public void Update(float deltaTime) {
            Int64 timeNow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            if ((timeNow - this._lastTimeSpawned > this.TimeBetweenParticles || this._particlesPerSecond == 0) &&
                this._isSpawning &&
                (this._particlesSpawned < this._particlesToSpawn || this._particlesToSpawn == 0)) {
                Vector2 velocity = Vector2.Zero;

                if (this._particleProperties.OfType<VelocityProperty>().Any()) {
                    velocity = this._particleProperties.OfType<VelocityProperty>().First().GetVelocity();
                    velocity = Vector2.Transform(velocity, Matrix.CreateRotationZ(this._rotation));
                }

                Int32 textureIndex = this._textureSources.Length - 1;

                if (this._particleProperties.OfType<RandomSpriteProperty>().Any()) {
                    textureIndex = this._particleProperties.OfType<RandomSpriteProperty>().First().SpriteIndex;
                }

                this.ActivateParticle(
                    textureIndex, this._position,
                    velocity);

                this._lastTimeSpawned = timeNow;
                this._particlesSpawned++;
            }

            foreach (Particle particle in this._particles.Where(particle => particle.IsActive)) {
                this.HandleParticleProperties(particle);
            }
        }

        public void StartSpawning(Int32 particlesToSpawn = 0) {
            this._particlesToSpawn = particlesToSpawn;
            this._particlesSpawned = 0;
            this._isSpawning = true;
        }

        public void StopListening() { }

        public void StopSpawning() {
            this._isSpawning = false;
        }

        public void SetTransform(Vector2 position, Single rotation) {
            this._position = position;
            this._rotation = rotation;
        }

        public void SetPosition(Vector2 position) {
            this._position = position;
        }

        public List<DrawTask> CreateDrawTasks() {
            List<DrawTask> drawTasks = new();

            foreach (Particle particle in this._particles.Where(p => p.IsActive)) {
                drawTasks.Add(new DrawTask(this._spriteSheet, this._textureSources[particle.TextureIndex],
                    particle.Position,
                    0, this._layerDepth,
                    particle.EffectContainer.Effects));
            }

            return drawTasks;
        }

        private void ActivateParticle(Int32 textureIndex, Vector2 startingPosition, Vector2 velocity) {
            Int64 timeNow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            if (this._particles.All(p => p.IsActive)) {
                this._particles.Add(new Particle(textureIndex, startingPosition, velocity, timeNow));
                return;
            }

            Int32 inactiveParticleIndex = this._particles.FindIndex(p => !p.IsActive);
            this._particles[inactiveParticleIndex].Set(textureIndex, startingPosition, velocity, timeNow);
        }

        private void HandleParticleProperties(Particle particle) {
            foreach (IParticleProperty particleProperty in this._particleProperties) {
                switch (particleProperty) {
                    case CauseOfDeathProperty causeOfDeathProperty:
                        HandleCauseOfDeathProperty(particle, causeOfDeathProperty);
                        break;
                    case ColorChangeProperty colorChangeProperty:
                        HandleColorChangeProperty(particle, colorChangeProperty);
                        break;
                    case SpriteChangeProperty spriteChangeProperty:
                        HandleSpriteChangeProperty(particle, spriteChangeProperty);
                        break;
                }
            }
        }

        private static void HandleCauseOfDeathProperty(Particle particle, CauseOfDeathProperty property) {
            switch (property.CauseOfDeath) {
                case CauseOfDeathProperty.CausesOfDeath.LifeSpan: {
                    Int64 timeNow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    if (timeNow - particle.TimeSpawned > property.LifeSpan) {
                        particle.Deactivate();
                    }

                    break;
                }

                case CauseOfDeathProperty.CausesOfDeath.OutOfBounds: {
                    if (particle.Position.X is < 0 or > Game1.GameWidth ||
                        particle.Position.Y is < 0 or > Game1.GameHeight) {
                        particle.Deactivate();
                    }

                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void HandleColorChangeProperty(Particle particle, ColorChangeProperty property) {
            Int64 timeNow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Single timeSinceSpawned = timeNow - particle.TimeSpawned;

            Int32 colorIndex = (Int32)(timeSinceSpawned / property.TimeBetweenColorsMS);

            if (colorIndex >= property.Colors.Length) return;

            particle.EffectContainer.SetEffect<ColorEffect, Vector4>(Palette.GetColorVector(property.Colors[colorIndex]));
        }

        private static void HandleSpriteChangeProperty(Particle particle, SpriteChangeProperty property) {
            Int64 timeNow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Single timeSinceSpawned = timeNow - particle.TimeSpawned;

            Int32 spriteIndex = (Int32)(timeSinceSpawned / property.TimeBetweenChangesMS);

            if (spriteIndex >= property.EndIndex) return;

            particle.TextureIndex = spriteIndex;
        }
    }
}