using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstralAssault;

public class Bullet : Entity
{
    public Bullet(GameplayState gameState, Vector2 position, float rotation, float speed) : base(gameState, position)
    {
        Velocity = -Vector2.UnitY.RotateVector(rotation) * speed;

        Texture2D spriteSheet = new Texture2D(GameState.Root.GraphicsDevice, 2, 2);

        Color[] data = new Color[2 * 2];
        for (int i = 0; i < data.Length; ++i) data[i] = Palette.GetColor(Palette.Colors.Grey9);
        spriteSheet.SetData(data);

        Collider = new Collider
        (
            this,
            3
        );
        GameState.CollisionSystem.AddCollider(Collider);

        OutOfBoundsBehavior = OutOfBounds.Destroy;

        ContactDamage = 5;
        IsFriendly = true;
        IsBullet = true;
    }

    public override void OnCollision(Collider other)
    {
        if (IsFriendly == other.Parent.IsFriendly) return;

        Destroy();
    }

    public override void OnUpdate(UpdateEventArgs e)
    {
        base.OnUpdate(e);

        if (Position.X is > Game1.TargetWidth or < 0 ||
            Position.Y is > Game1.TargetHeight or < 0) Destroy();
    }
}