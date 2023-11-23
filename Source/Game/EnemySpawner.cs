using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstralAssault;

public class EnemySpawner
{
    public Int32 EnemiesKilled { get; set; }

    private readonly GameplayState _gameState;

    private const Single BaseAsteroidSpawnInterval = 24000;
    private Single _asteroidSpawnInterval = BaseAsteroidSpawnInterval;
    private Int64 _lastAsteroidSpawnTime;

    private readonly Int64 _timeStarted = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    public EnemySpawner(GameplayState gameState)
    {
        this._gameState = gameState;
    }

    private void SpawnAsteroid()
    {
        Random rnd = new();

        Vector2 position = GenerateEnemyPosition();
        Asteroid.Sizes size = (Asteroid.Sizes)rnd.Next(0, 3);

        Vector2 gameCenter = new(Game1.TargetWidth / 2F, Game1.TargetHeight / 2F);
        Single angleToCenter = MathF.Atan2(gameCenter.Y - position.Y, gameCenter.X - position.X);
        angleToCenter += MathHelper.ToRadians(rnd.Next(-45, 45));

        this._gameState.Entities.Add(new Asteroid(this._gameState, position, angleToCenter, size));
    }

    public void Update(float deltaTime)
    {
        Int64 timeNow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        if (timeNow - this._lastAsteroidSpawnTime > this._asteroidSpawnInterval)
        {
            this._lastAsteroidSpawnTime = timeNow;
            this.SpawnAsteroid();
        }

        this._asteroidSpawnInterval = BaseAsteroidSpawnInterval * MathF.Pow(0.96F, this.EnemiesKilled);

        if (this._gameState.EnemiesAlive == 0)
        {
            this._asteroidSpawnInterval = 0;
        }
    }

    public void StopListening()
    {
    }

    private static Vector2 GenerateEnemyPosition()
    {
        Random rnd = new();
        Int32 side = rnd.Next(0, 4);

        Int32 x = side switch
        {
            0 => 0,
            1 => Game1.TargetWidth,
            2 => rnd.Next(0, Game1.TargetWidth),
            3 => rnd.Next(0, Game1.TargetWidth),
            _ => throw new ArgumentOutOfRangeException()
        };

        Int32 y = side switch
        {
            0 => rnd.Next(0, Game1.TargetHeight),
            1 => rnd.Next(0, Game1.TargetHeight),
            2 => 0,
            3 => Game1.TargetHeight,
            _ => throw new ArgumentOutOfRangeException()
        };

        return new Vector2(x, y);
    }
}