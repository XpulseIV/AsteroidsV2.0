using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using MouseButtons = AstralAssault.InputEventSource.MouseButtons;

namespace AstralAssault.Source.Entity.Entities
{
    public class Player : AstralAssault.Entity, IInputEventListener
    {
        public Single Multiplier = 1;

        private const Int32 LightAmmoCost = 1;
        private const Int32 HeavyAmmoCost = 5;

        public const Int32 MaxAmmo = 200;
        public Int32 Ammo = 50;

        private readonly Texture2D _ammoBarTexture;
        private const Int32 AmmoBarWidth = 46;
        private const Int32 AmmoBarHeight = 16;
        private const Int32 AmmoBarX = 4;
        private const Int32 AmmoBarY = Game1.TargetHeight - AmmoBarHeight - 4;

        private Tuple<Vector2, Vector2> _muzzle = new(Vector2.Zero, Vector2.Zero);
        private Boolean _lastCannon;
        private Boolean _thrusterIsOn;
        private Int64 _lastTimeFired;
        private Single _delta;
        private ParticleEmitter _particleEmitter;

        private const Single MoveSpeed = 200;
        private const Single MaxSpeed = 100;
        private const Single Friction = 30;
        private const Single Pi = 3.14F;
        private const Single BulletSpeed = 250;
        private const Int32 ShootSpeed = 200;

        public Player(GameplayState gameState, Vector2 position) :base(gameState, position)
        {
            this.Position = position;
            this.Rotation = 0;
            this.size = 1;
            
            Bounds = new Rectangle((int)position.X, (int)position.Y, 6, 6);

            this.model = new List<Vector2>
            {
                new Vector2(5.0f, 0.0f),
                new Vector2(-2.5f, -2.5f),
                new Vector2(-2.5f, 2.5f)
            };
            this.Color = Palette.GetColor(Palette.Colors.Blue8);

            this._ammoBarTexture = AssetManager.Load<Texture2D>("AmmoBar");

            this.InitParticleEmitter();

            this.StartListening();

            this.OutOfBoundsBehavior = OutOfBounds.Wrap;

            this.IsActor = true;
            this.MaxHP = 50;
            this.HP = this.MaxHP;
            this.IsFriendly = true;
            this.IsSolid = true;

            this.mass = 7;
        }

        private void InitParticleEmitter()
        {
            Texture2D particleSpriteSheet = AssetManager.Load<Texture2D>("Particle");

            Rectangle[] textureSources =
            {
                new(24, 0, 8, 8),
                new(16, 0, 8, 8),
                new(8, 0, 8, 8),
                new(0, 0, 8, 8)
            };

            IParticleProperty[] particleProperties =
            {
                new CauseOfDeathProperty(CauseOfDeathProperty.CausesOfDeath.LifeSpan, 210),
                new ColorChangeProperty(
                    new[]
                    {
                        Palette.Colors.Blue9,
                        Palette.Colors.Blue8,
                        Palette.Colors.Blue7,
                        Palette.Colors.Blue6,
                        Palette.Colors.Blue5,
                        Palette.Colors.Blue4,
                        Palette.Colors.Blue3
                    },
                    30),
                new SpriteChangeProperty(0, textureSources.Length, 40),
                new VelocityProperty(-1F, 1F, 0.04F, 0.1F)
            };

            this._particleEmitter = new ParticleEmitter(
                particleSpriteSheet,
                textureSources,
                20, this.Position, this.Rotation,
                particleProperties,
                LayerDepth.ThrusterFlame);
        }

        public override List<DrawTask> GetDrawTasks()
        {
            List<DrawTask> drawTasks = new();

            if (this._thrusterIsOn)
            {
                this._particleEmitter.StartSpawning();
            }
            else
            {
                this._particleEmitter.StopSpawning();
            }

            drawTasks.AddRange(this._particleEmitter.CreateDrawTasks());
            drawTasks.AddRange(base.GetDrawTasks());

            this._thrusterIsOn = false;

            drawTasks.AddRange(this.GetAmmoBarDrawTasks());

            return drawTasks;
        }

        private void StartListening()
        {
            InputEventSource.KeyboardEvent += this.OnKeyboardEvent;
            InputEventSource.MouseMoveEvent += this.OnMouseMoveEvent;
            InputEventSource.MouseButtonEvent += this.OnMouseButtonEvent;
        }

        private void StopListening()
        {
            InputEventSource.KeyboardEvent -= this.OnKeyboardEvent;
            InputEventSource.MouseMoveEvent -= this.OnMouseMoveEvent;
            InputEventSource.MouseButtonEvent -= this.OnMouseButtonEvent;
            this._particleEmitter.StopListening();
        }

        private void HandleMovement(Int32 yAxis)
        {
            // acceleration and deceleration
            Vector2 forward = new Vector2(
                (Single)Math.Cos(this.Rotation),
                (Single)Math.Sin(this.Rotation)
            ) * MoveSpeed * this._delta;

            this.Velocity = new Vector2(
                Math.Clamp(this.Velocity.X + forward.X * yAxis, -MaxSpeed, MaxSpeed),
                Math.Clamp(this.Velocity.Y + forward.Y * yAxis, -MaxSpeed, MaxSpeed));

            if (this.Velocity.Length() > MaxSpeed)
            {
                this.Velocity.Normalize();
                this.Velocity *= MaxSpeed;
            }
            else if (this.Velocity.Length() < -MaxSpeed)
            {
                this.Velocity.Normalize();
                this.Velocity *= -MaxSpeed;
            }
        }

        private void HandleFiring(MouseButtons mouseButton)
        {
            Int64 timeNow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            if (this._lastTimeFired + ShootSpeed > timeNow) return;

            BulletType bulletType = mouseButton switch
            {
                MouseButtons.Left => BulletType.Light,
                MouseButtons.Right => BulletType.Heavy,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (this.Ammo < bulletType switch {
                    BulletType.Light => LightAmmoCost,
                    BulletType.Heavy => HeavyAmmoCost,
                    _ => throw new ArgumentOutOfRangeException()
                }) return;


            this.Ammo -= bulletType switch
            {
                BulletType.Light => LightAmmoCost,
                BulletType.Heavy => HeavyAmmoCost,
                _ => throw new ArgumentOutOfRangeException()
            };

            Random rnd = new();
            String soundName = (bulletType == BulletType.Heavy ? "Heavy" : "") + "Shoot" + rnd.Next(1, 4);
            Jukebox.PlaySound(soundName, 0.5F);

            this._lastTimeFired = timeNow;

            this.GameState.Entities.Add(
                new Bullet(this.GameState, this._lastCannon ? this._muzzle.Item1 : this._muzzle.Item2,
                    this.Rotation,
                    BulletSpeed,
                    bulletType));

            this._lastCannon = !this._lastCannon;
        }

        private List<DrawTask> GetAmmoBarDrawTasks()
        {
            DrawTask emptyDrawTask = new(this._ammoBarTexture,
                new Rectangle(0, 0, AmmoBarWidth, AmmoBarHeight),
                new Vector2(AmmoBarX, AmmoBarY),
                0,
                LayerDepth.HUD,
                new List<IDrawTaskEffect>(),
                Color.White,
                Vector2.Zero);

            const Int32 margin = 3;
            Int32 width = (Int32)(this.Ammo / (Single)MaxAmmo * (AmmoBarWidth - margin * 2));

            DrawTask fullDrawTask = new(this._ammoBarTexture,
                new Rectangle(0, AmmoBarHeight, width + margin, AmmoBarHeight),
                new Vector2(AmmoBarX, AmmoBarY),
                0,
                LayerDepth.HUD,
                new List<IDrawTaskEffect>(),
                Color.White,
                Vector2.Zero);

            return new List<DrawTask>() { emptyDrawTask, fullDrawTask };
        }

        public override void OnCollision(AstralAssault.Entity other)
        {
            base.OnCollision(other);

            if (other is not Asteroid) return;

            if (this.Multiplier > 1)
            {
                Jukebox.PlaySound("MultiplierBroken");
            }

            this.Multiplier = 1;

            Random rnd = new();

            String soundName = rnd.Next(3) switch
            {
                0 => "Hurt1",
                1 => "Hurt2",
                2 => "Hurt3",
                _ => throw new ArgumentOutOfRangeException()
            };

            Jukebox.PlaySound(soundName, 0.5F);
        }

        public void HandleDamage(Single damage)
        {
            this.HP -= damage;
        }

        public override void Destroy()
        {
            this.StopListening();

            base.Destroy();
        }

        protected override void OnDeath()
        {
            Game1 root = this.GameState.Root;

            Jukebox.PlaySound("GameOver");

            root.GameStateMachine.ChangeState(new GameOverState(root));

            base.OnDeath();
        }

        public void OnKeyboardEvent(Object sender, KeyboardEventArgs e) {
            var yAxis = 0;

            if (e.Keys.Contains(Keys.Up)) {
                yAxis = 1;
                this._thrusterIsOn = true;
            }

            if (e.Keys.Contains(Keys.Down)) {
                yAxis = -1;
            }

            if (e.Keys.Contains(Keys.Right)) {
                this.Rotation += 5 * e.DeltaTime;
            }

            if (e.Keys.Contains(Keys.Left)) {
                this.Rotation -= 5 * e.DeltaTime;
            }

            this.HandleMovement(yAxis);
        }


        public void OnMouseMoveEvent(Object sender, MouseMoveEventArgs e) { }

        public void OnMouseButtonEvent(Object sender, MouseButtonEventArgs e) {
            if (e.Button is MouseButtons.Left or MouseButtons.Right) {
                this.HandleFiring(e.Button);
            }
        }

        public override void Update(float deltaTime) {
            base.Update(deltaTime);

            this._delta = deltaTime;

            this.ApplyFriction();

            this.UpdateMuzzlePositions();
            this.UpdateParticleEmitterPosition();
        }

        private void UpdateMuzzlePositions() {
            Vector2 muzzle1;
            Vector2 muzzle2;

            const Single x = 8;
            const Single y = 10;

            {
                Single rot = (Pi / 8) * (Single)Math.Round(this.Rotation / (Pi / 8));

                var x2 = (Single)(x * Math.Cos(rot) - y * Math.Sin(rot));
                var y2 = (Single)(y * Math.Cos(rot) + x * Math.Sin(rot));

                muzzle1 = new Vector2(this.Position.X + x2, this.Position.Y + y2);
            }

            {
                Single rot = (Pi / 8) * (Single)Math.Round(this.Rotation / (Pi / 8));

                var x2 = (Single)(x * Math.Cos(rot) + y * Math.Sin(rot));
                var y2 = (Single)(-y * Math.Cos(rot) + x * Math.Sin(rot));

                muzzle2 = new Vector2(this.Position.X + x2, this.Position.Y + y2);
            }

            this._muzzle = new Tuple<Vector2, Vector2>(muzzle1, muzzle2);
        }

        private void UpdateParticleEmitterPosition() {
            Single emitterRotation = (this.Rotation + Pi) % (2 * Pi);
            Vector2 emitterPosition = new(11, 0);

            {
                var x2 =
                    (Single)(emitterPosition.X * Math.Cos(emitterRotation) + emitterPosition.Y * Math.Sin(emitterRotation));
                var y2 =
                    (Single)(emitterPosition.Y * Math.Cos(emitterRotation) + emitterPosition.X * Math.Sin(emitterRotation));

                emitterPosition = new Vector2(this.Position.X + x2, this.Position.Y + y2);
            }

            this._particleEmitter.SetTransform(emitterPosition, emitterRotation);
        }

        private void ApplyFriction() {
            Single sign = Math.Sign(this.Velocity.Length());

            if (sign != 0) {
                var direction = (Single)Math.Atan2(this.Velocity.Y, this.Velocity.X);

                this.Velocity -=
                    new Vector2((Single)Math.Cos(direction), (Single)Math.Sin(direction)) * Friction * this._delta * sign;
            }
        }
    }
}