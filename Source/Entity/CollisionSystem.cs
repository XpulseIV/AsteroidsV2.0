using System;
using System.Collections.Generic;
using Asteroids2.Source.Entity.Components;
using Asteroids2.Source.Game;
using Microsoft.Xna.Framework;

namespace Asteroids2.Source.Entity;

public class CollisionSystem
{
    public List<Collider> Colliders { get; } = new List<Collider>();
    private List<Tuple<Collider, Collider>> m_lastCollisions = new List<Tuple<Collider, Collider>>();

    private readonly Func<float, float, float, float, float, float, bool> m_doCirclesOverlap
        = static (x1, y1, r1, x2, y2, r2) =>
        {
            return MathF.Abs((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2)) <= ((r1 + r2) * (r1 + r2));
        };

    public void OnUpdate(UpdateEventArgs e)
    {
        List<Tuple<Collider, Collider>> currentCollisions = new List<Tuple<Collider, Collider>>();

        for (int i = 0; i < (Colliders.Count - 1); i++)
        {
            for (int j = 0; j < Colliders.Count; j++)
            {
                if (Colliders[i] == Colliders[j]) continue;

                if (!m_doCirclesOverlap
                    (
                        Colliders[i].Position.X, Colliders[i].Position.Y, Colliders[i].Radius, Colliders[j].Position.X,
                        Colliders[j].Position.Y,
                        Colliders[j].Radius
                    )) continue;

                // Collision has occured
                Tuple<Collider, Collider> colliderPair = new Tuple<Collider, Collider>(Colliders[i], Colliders[j]);
                currentCollisions.Add(colliderPair);

                if (m_lastCollisions.Contains(colliderPair)) continue;

                if (Colliders[i].IsSolid && Colliders[j].IsSolid && (Colliders[i].Parent.TimeSinceSpawned > 512))
                {
                    // Distance between balls
                    float fDistance = Vector2.Distance(Colliders[i].Position, Colliders[j].Position);

                    // Normal
                    Vector2 n = (Colliders[j].Position - Colliders[i].Position) / fDistance;

                    // Wikipedia Version - Maths is smarter than what I did before
                    Vector2 k = (Colliders[i].Parent.Velocity - Colliders[j].Parent.Velocity);
                    float p = 2.0f * (n.X * k.X + n.Y * k.Y) / (Colliders[i].m_mass + Colliders[j].m_mass);

                    // Do stuff with collision response
                    Colliders[i].Parent.Velocity += -p * Colliders[j].m_mass * n;
                    Colliders[j].Parent.Velocity += p * Colliders[i].m_mass * n;
                }

                Colliders[i].Parent.OnCollision(Colliders[j]);
                Colliders[j].Parent.OnCollision(Colliders[i]);
            }
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