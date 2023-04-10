#region
using Microsoft.Xna.Framework;
#endregion

namespace Asteroids2.Source.Entity.Components;

public class Collider
{
    public readonly float m_mass;

    public readonly Entity Parent;
    public bool IsSolid;
    public Vector2 Position;
    public int Radius;

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