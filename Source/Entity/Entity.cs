using System;
using System.Collections.Generic;
using System.Diagnostics;
using AstralAssault.Source.Entity.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace AstralAssault;

public class Entity
{
    public Vector2 Position;
    public Vector2 Velocity;
    protected Single ContactDamage;
    protected Single Rotation;
    public List<Vector2> model;
    protected Color Color;
    protected Int32 size;
    protected readonly GameplayState GameState;
    protected OutOfBounds OutOfBoundsBehavior = OutOfBounds.Wrap;
    protected Boolean IsActor = false;
    protected Single MaxHP;
    protected Single HP;
    protected Int32 HealthBarYOffset = 20;

    public bool IsSolid;
    public int mass;

    public Rectangle Bounds;

    public Boolean IsFriendly;

    private readonly Int64 _timeSpawned;
    public Int64 TimeSinceSpawned => DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - this._timeSpawned;

    private Texture2D _healthBarTexture;

    protected enum OutOfBounds
    {
        DoNothing,
        Wrap,
        Destroy
    }

    protected Entity(GameplayState gameState, Vector2 position) {
        this.GameState = gameState;
        this.Position = position;
        this._timeSpawned = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        this.CreateHealthBarTexture();
    }

    public virtual void Update(float deltaTime)
    {
        if (this.IsActor && this.HP <= 0)
        {
            this.OnDeath();
            return;
        }

        this.Position += this.Velocity * deltaTime;

        switch (this.OutOfBoundsBehavior)
        {
            case OutOfBounds.DoNothing:
            {
                break;
            }

            case OutOfBounds.Destroy:
            {
                if (this.Position.X is < 0 or > Game1.TargetWidth || this.Position.Y is < 0 or > Game1.TargetHeight)
                {
                    this.Destroy();
                }

                break;
            }

            case OutOfBounds.Wrap:
            {
                this.Position.X = this.Position.X switch
                {
                    < 0 => Game1.TargetWidth,
                    > Game1.TargetWidth => 0,
                    _ => this.Position.X
                };

                this.Position.Y = this.Position.Y switch
                {
                    < 0 => Game1.TargetHeight,
                    > Game1.TargetHeight => 0,
                    _ => this.Position.Y
                };

                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }

    public virtual void OnCollision(Entity other)
    {
        if (!this.IsActor || other.IsFriendly == this.IsFriendly) return;

        if (this is Player)
        {
            Player player = (Player)this;
            player.HandleDamage(other.ContactDamage);
        }
        else
        {
            this.HP = Math.Max(0, this.HP - other.ContactDamage);
        }
    }

    public virtual List<DrawTask> GetDrawTasks()
    {
        List<DrawTask> drawTasks = new();

        Vector4 fullColor = Palette.GetColorVector(Palette.Colors.Green7);
        if (this.IsActor) drawTasks.AddRange(this.CreateBarDrawTasks(this.HP, this.MaxHP, fullColor, this.HealthBarYOffset));

        this.GameState.Root.Renderer.DrawWireFrameModel(this.model, this.Position.X, this.Position.Y, this.Rotation, this.size, this.Color);

        return drawTasks;
    }

    public virtual void Destroy()
    {
        this.GameState.Entities.Remove(this);
    }

    protected virtual void OnDeath()
    {
        this.Destroy();
    }

    private void CreateHealthBarTexture()
    {
        this._healthBarTexture = new Texture2D(this.GameState.Root.GraphicsDevice, 1, 1);
        Color[] data = { Color.White };
        this._healthBarTexture.SetData(data);
    }

    protected List<DrawTask> CreateBarDrawTasks(Single value, Single maxValue, Vector4 fillColor, Int32 yOffset)
    {
        const Int32 width = 20;
        const Int32 height = 3;

        Int32 filled = (Int32)Math.Ceiling(value / maxValue * width);

        Int32 x = (Int32)this.Position.X - width / 2;
        Int32 y = (Int32)this.Position.Y - yOffset;

        Rectangle outline = new(x - 1, y - 1, width + 2, height + 2);
        Rectangle emptyHealthBar = new(x, y, width, height);
        Rectangle fullHealthBar = new(x, y, filled, height);

        Vector4 outlineColor = Palette.GetColorVector(Palette.Colors.Black);
        Vector4 emptyColor = Palette.GetColorVector(Palette.Colors.Red6);

        Rectangle source = new(0, 0, 1, 1);

        DrawTask background = new(this._healthBarTexture,
            source,
            outline,
            0,
            LayerDepth.HealthBar,
            new List<IDrawTaskEffect> { new ColorEffect(outlineColor) },
            Color.Black);

        DrawTask empty = new(this._healthBarTexture,
            source,
            emptyHealthBar,
            0,
            LayerDepth.HealthBar,
            new List<IDrawTaskEffect> { new ColorEffect(emptyColor) },
            Color.Red);

        DrawTask full = new(this._healthBarTexture,
            source,
            fullHealthBar,
            0,
            LayerDepth.HealthBar,
            new List<IDrawTaskEffect> { new ColorEffect(fillColor) },
            Color.LimeGreen);

        return new List<DrawTask> { background, empty, full };
    }
}