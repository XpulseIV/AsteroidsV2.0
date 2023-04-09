using Microsoft.Xna.Framework;

namespace Asteroids2.Source.Entity.Components;

public class Collider
{
    public Vector2 Position;

    public readonly Entity Parent;
    public int Radius;
    public bool IsSolid;
    public readonly float m_mass;

    public Collider(Entity parent, int radius, bool isSolid, float mass)
    {
        Parent = parent;
        Radius = radius;
        IsSolid = isSolid;
        m_mass = mass;
    }

    public Collider(Entity parent, int radius)
    {
        Parent = parent;
        Radius = radius;
        IsSolid = false;
        m_mass = 0;
    }

    public void SetPosition(Vector2 nPos)
    {
        Position = nPos;
    }
}