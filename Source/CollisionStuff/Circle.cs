using Microsoft.Xna.Framework;

namespace Asteroids2.Source.CollisionStuff;

/// <summary>
/// This is a simple circle class... it has a center and a radius, that's about it
/// </summary>
internal sealed class Circle : ICircle
{
#region Properties
    private Vector2 m_position = Vector2.Zero;

    /// <summary>
    /// Update or get the center position of this circle
    /// </summary>
    public Vector2 Pos
    {
        get => m_position;
        set
        {
            OldPos = m_position;
            m_position = value;
        }
    }

    /// <summary>
    /// The previous position of this circle
    /// </summary>
    internal Vector2 OldPos { get; private set; }

    private float m_radius;

    /// <summary>
    /// update or get the radius of this circle
    /// </summary>
    public float Radius
    {
        get => m_radius;
        set
        {
            m_radius = value;
            RadiusSquared = value * value;
        }
    }

    /// <summary>
    /// Get the radius squared of this circle
    /// </summary>
    public float RadiusSquared { get; private set; }
#endregion //Properties

#region Methods
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="position"></param>
    /// <param name="radius"></param>
    internal Circle(Vector2 position, float radius)
    {
        Initialize(position, radius);
    }

    /// <summary>
    /// Set teh initial values of this circle
    /// It's SUPER important that this method is used first, because it sets up both the old and current position
    /// </summary>
    /// <param name="position">the initial position</param>
    /// <param name="radius">the initial radius</param>
    private void Initialize(Vector2 position, float radius)
    {
        m_position = position;
        OldPos = position;
        Radius = radius;
    }
#endregion //Methods
}