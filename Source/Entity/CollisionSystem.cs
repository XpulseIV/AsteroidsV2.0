#region
using System;
using System.Collections.Generic;
using System.Numerics;
using Asteroids2.Source.Entity.Components;
using Asteroids2.Source.Game;
using Vector2 = Microsoft.Xna.Framework.Vector2;
#endregion

namespace Asteroids2.Source.Entity;

class tupleComaparer : IComparer<Tuple<Collider, int, Collider, int>>
{
    public int Compare(Tuple<Collider, int, Collider, int> x, Tuple<Collider, int, Collider, int> y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(null, y)) return 1;
        if (ReferenceEquals(null, x)) return -1;
        int item2Comparison = Comparer<int>.Default.Compare(x.Item2, y.Item2);

        if (item2Comparison != 0) return item2Comparison;

        return Comparer<int>.Default.Compare(x.Item4, y.Item4);
    }
}

public class CollisionSystem
{
    private readonly Func<float, float, float, float, float, float, bool> m_doCirclesOverlap
        = static (x1, y1, r1, x2, y2, r2) => MathF.Abs
            ((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2)) <= ((r1 + r2) * (r1 + r2));

    private List<Tuple<Collider, int, Collider, int>> m_lastCollisions = new List<Tuple<Collider, int, Collider, int>>();
    public List<Collider> Colliders { get; } = new List<Collider>();
    
    private IComparer<Tuple<Collider, int, Collider, int>> m_compyes = new tupleComaparer();

    public void OnUpdate(UpdateEventArgs e)
    {
        var currentCollisions = new List<Tuple<Collider, int, Collider, int>>();

        int i = 0;

        foreach (Collider collider in Colliders)
        {
            int j = 0;

            foreach (Collider other in Colliders)
            {
                if (collider == other) continue;

                if (m_doCirclesOverlap
                    (
                        collider.Position.X, collider.Position.Y, collider.Radius, other.Position.X,
                        other.Position.Y,
                        other.Radius
                    ))
                {
                    // Collision has occurred
                    var colliderPair = new Tuple<Collider, int, Collider, int>(collider, i, other, j);

                    if (m_lastCollisions.BinarySearch(colliderPair, m_compyes) != -1) continue;
                    if (currentCollisions.BinarySearch(new Tuple<Collider, int, Collider, int>(other, j, collider, i), m_compyes) != -1) continue;

                    currentCollisions.Add(colliderPair);


                    if (collider.IsSolid && other.IsSolid)
                    {
                        // Distance between ball centers
                        float fDistance = Vector2.Distance(collider.Position, other.Position);

                        // Calculate displacement required
                        float fOverlap = 0.5f * (fDistance - collider.Radius - other.Radius);

                        // Displace Current Ball away from collision
                        collider.Parent.Position.X -= fOverlap * (collider.Position.X - other.Position.X) / fDistance;
                        collider.Parent.Position.Y -= fOverlap * (collider.Position.Y - other.Position.Y) / fDistance;

                        // Displace Target Ball away from collision
                        other.Parent.Position.X += fOverlap * (collider.Position.X - other.Position.X) / fDistance;
                        other.Parent.Position.Y += fOverlap * (collider.Position.Y - other.Position.Y) / fDistance;
                    }
                }

                j++;
            }

            i++;
        }

        foreach ((Collider b1, int _, Collider b2, int _) in currentCollisions)
        {
            if (b1.IsSolid && b2.IsSolid && (b1.Parent.TimeSinceSpawned > 512))
            {
                Vector2 v1Part1 = b1.Parent.Velocity;
                float v1Part2 = (2 * b2.m_mass) / (b1.m_mass + b2.m_mass);
                float v1Part3 = Vector2.Dot(b1.Parent.Velocity - b2.Parent.Velocity, b1.Position - b2.Position) /
                    ((b1.Position - b2.Position).Length() * (b1.Position - b2.Position).Length());
                Vector2 v1Part4 = (b1.Position - b2.Position);

                Vector2 newVel1 = v1Part1 - (v1Part2 * v1Part3 * v1Part4);
                b1.Parent.Velocity = newVel1;

                Vector2 v2Part1 = b2.Parent.Velocity;
                float v2Part2 = (2 * b1.m_mass) / (b1.m_mass + b2.m_mass);
                float v2Part3 = Vector2.Dot(b2.Parent.Velocity - b1.Parent.Velocity, b2.Position - b1.Position) /
                    ((b2.Position - b1.Position).Length() * (b2.Position - b1.Position).Length());
                Vector2 v2part4 = (b2.Position - b1.Position);

                Vector2 newVel2 = v2Part1 - (v2Part2 * v2Part3 * v2part4);
                b2.Parent.Velocity = newVel2;

                /*
                // Distance between balls
                float fDistance = Vector2.Distance(b1.Position, b2.Position);

                // Normal
                float nx = (b1.Position.X - b2.Position.X) / fDistance;
                float ny = (b1.Position.Y - b2.Position.Y) / fDistance;

                // Wikipedia Version - Maths is smarter than what I did before, and it works
                float kx = (b1.Parent.Velocity.X - b2.Parent.Velocity.X);
                float ky = (b1.Parent.Velocity.Y - b2.Parent.Velocity.Y);
                float p = 2.0f * (nx * kx + ny * ky) / (b1.m_mass + b2.m_mass);
                b1.Parent.Velocity.X += b1.Parent.Velocity.X - p * b2.m_mass * nx;
                b1.Parent.Velocity.Y += b1.Parent.Velocity.Y - p * b2.m_mass * ny;
                b2.Parent.Velocity.X += b2.Parent.Velocity.X + p * b1.m_mass * nx;
                b2.Parent.Velocity.Y += b2.Parent.Velocity.Y + p * b1.m_mass * ny;


                if (b1.Parent.Velocity.Length() > Entity.MaxSpeed)
                {
                    b1.Parent.Velocity.Normalize();
                    b1.Parent.Velocity *= Entity.MaxSpeed;
                }
                else if (b1.Parent.Velocity.Length() < -Entity.MaxSpeed)
                {
                    b1.Parent.Velocity.Normalize();
                    b1.Parent.Velocity *= -Entity.MaxSpeed;
                }

                if (b2.Parent.Velocity.Length() > Entity.MaxSpeed)
                {
                    b2.Parent.Velocity.Normalize();
                    b2.Parent.Velocity *= Entity.MaxSpeed;
                }
                else if (b2.Parent.Velocity.Length() < -Entity.MaxSpeed)
                {
                    b2.Parent.Velocity.Normalize();
                    b2.Parent.Velocity *= -Entity.MaxSpeed;
                }
            */
            }

            b1.Parent.OnCollision(b2);
            b2.Parent.OnCollision(b1);
        }

        m_lastCollisions = currentCollisions;
    }


    public void AddCollider(Collider collider)
    {
        Colliders.Add(collider);
    }

    public void RemoveCollider(Collider collider)
    {
        Colliders.Remove(collider);
    }
}