#region
using System;
using System.Linq;
using Asteroids2.Source.Entity.Entities;
using Asteroids2.Source.Game.GameState;
using Asteroids2.Source.Graphics;
using Microsoft.Xna.Framework;
#endregion

namespace Asteroids2.Source.Game;

public class WaveController
{
    private const long WaveTextDuration = 2000;
    private const long WaveDelay = 5000;
    public readonly GameplayState GameState;
    private readonly Game1 m_root;
    private int m_currentWave;

    private bool m_drawWaveText;
    private long m_waveTextTimer;

    private long m_waveTimer;

    public WaveController(GameplayState gameState, Game1 root)
    {
        GameState = gameState;
        m_root = root;

        StartNextWave();
    }

    private void StartNextWave()
    {
        m_currentWave++;

        int enemiesToSpawn = (int)(m_currentWave * 1.5F);

        Random rnd = new Random();

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            int side = rnd.Next(0, 4);

            int x = side switch
            {
                0 => 0,
                1 => Game1.TargetWidth,
                2 => rnd.Next(0, Game1.TargetWidth),
                3 => rnd.Next(0, Game1.TargetWidth),
                var _ => throw new ArgumentOutOfRangeException()
            };

            int y = side switch
            {
                0 => rnd.Next(0, Game1.TargetHeight),
                1 => rnd.Next(0, Game1.TargetHeight),
                2 => 0,
                3 => Game1.TargetHeight,
                var _ => throw new ArgumentOutOfRangeException()
            };

            Vector2 position = new Vector2(x, y);
            Asteroid.Sizes size = (Asteroid.Sizes)rnd.Next(0, 3);

            Vector2 gameCenter = new Vector2(Game1.TargetWidth / 2F, Game1.TargetHeight / 2F);
            float angleToCenter = MathF.Atan2(gameCenter.Y - position.Y, gameCenter.X - position.X);
            angleToCenter += MathHelper.ToRadians(rnd.Next(-45, 45));

            GameState.Entities.Add(new Asteroid(GameState, position, angleToCenter, size));
        }

        m_drawWaveText = true;
        m_waveTextTimer = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }

    public void Draw()
    {
        if (!m_drawWaveText) return;

        string text = $"Wave: {m_currentWave}";
        GameState.Root.TextRenderer.DrawString(new Vector2(10, 10), text, Palette.GetColor(Palette.Colors.Blue6), 1);
    }

    public void OnUpdate(UpdateEventArgs e)
    {
        long timeNow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        if (m_drawWaveText && ((timeNow - m_waveTextTimer) > WaveTextDuration)) m_drawWaveText = false;

        int enemiesAlive = GameState.Entities.Count(static x => x is Asteroid);

        if (enemiesAlive == 0)
        {
            if ((timeNow - m_waveTimer) < WaveDelay) return;

            StartNextWave();
            m_waveTimer = timeNow;
        }
    }
}