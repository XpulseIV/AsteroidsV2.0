using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstralAssault;

public class Asteroid : Entity
{
    private readonly Single _rotSpeed;
    private readonly Sizes _size;
    private Boolean _hasExploded;

    public enum Sizes
    {
        Smallest,
        Small,
        Medium
    }

    public Asteroid(
        GameplayState gameState,
        Vector2 position,
        Single direction,
        Sizes size)
        :base(gameState, position)
    {
        this._size = size;
        this.size = ((Int32)this._size + 1) * 4;

        this.Color = Palette.GetColor(Palette.Colors.Red6);
        this.model = new List<Vector2>();
        
        Bounds = new Rectangle((int)position.X, (int)position.Y, ((int)_size + 1) * 4 + 2, ((int)_size + 1) * 4 + 2);

        const Int32 verts = 20;
        for (Int32 i = 0; i < verts; i++)
        {
            Single noise = (Single)new Random().NextDouble() * 0.4f + 0.8f;
            this.model.Add
            (
                new Vector2
                (
                    noise * MathF.Sin(i / (Single)verts * 6.28318f),
                    noise * MathF.Cos(i / (Single)verts * 6.28318f)
                )
            );
        }

        Random rnd = new();
        this._rotSpeed = rnd.Next(5, 20) / 10F;
        Int32 speed = rnd.Next(30, 100);

        this.Velocity = new Vector2((Single)Math.Cos(direction), (Single)Math.Sin(direction)) * speed;

        switch (size)
        {
            case Sizes.Smallest:
                this.MaxHP = 12;
                this.HP = this.MaxHP;
                this.ContactDamage = 5;
                mass = 6;
                break;
            case Sizes.Small:
                this.MaxHP = 24;
                this.HP = this.MaxHP;
                this.ContactDamage = 7;
                mass = 12;
                break;
            case Sizes.Medium:
                this.MaxHP = 36;
                this.HP = this.MaxHP;
                this.ContactDamage = 12;
                mass = 18;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        this.OutOfBoundsBehavior = OutOfBounds.Bounce;

        this.IsActor = true;
        this.IsSolid = true;
    }

    protected override void OnDeath()
    {
        if (!this._hasExploded)
        {
            Random rnd = new();

            String soundToPlay = rnd.Next(3) switch
            {
                0 => "Explosion1",
                1 => "Explosion2",
                2 => "Explosion3",
                _ => throw new ArgumentOutOfRangeException()
            };

            Jukebox.PlaySound(soundToPlay);

            if (this._size - 1 < 0)
            {
                this._hasExploded = true;
                return;
            }

            Int32 amount = rnd.Next(1, 4);

            Vector2 playerPosition = this.GameState.Player.Position;
            Single angleToPlayer = MathF.Atan2(this.Position.Y - playerPosition.Y, this.Position.X - playerPosition.X);

            for (Int32 i = 0; i < amount; i++)
            {
                angleToPlayer += (Single)rnd.NextDouble() * MathF.PI / 1 - MathF.PI / 2;

                this.GameState.Entities.Add(
                    new Asteroid(this.GameState, this.Position, angleToPlayer, this._size - 1));
            }
        }

        this._hasExploded = true;

        this.GameState.Player.Multiplier += 0.1F;

        Int32 score = this._size switch
        {
            Sizes.Smallest => 100,
            Sizes.Small => 300,
            Sizes.Medium => 700,
            _ => 0
        };

        this.GameState.DebrisController.SpawnDebris(this.Position, (Int32)this._size);
        this.GameState.DebrisController.SpawnScoreText(this.Position, score);

        this.GameState.Root.Score += (Int32)(score * this.GameState.Player.Multiplier);

        this.GameState.EnemySpawner.EnemiesKilled++;

        base.OnDeath();
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        this.Rotation += this._rotSpeed * deltaTime;
        if (this.Rotation > Math.PI) {
            this.Rotation = (Single)(-Math.PI);
        }

        if (this.Velocity.Length() > 200)
        {
            this.Velocity = Vector2.Normalize(this.Velocity) * 200;
        }
    }

    public override void OnCollision(Entity other)
    {
        base.OnCollision(other);

        if (other is not Bullet) return;

        Random rnd = new();

        String soundName = "Hurt" + rnd.Next(1, 4);

        Jukebox.PlaySound(soundName, 0.5F);
    }
}