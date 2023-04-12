﻿#region
using System;
using System.Collections.Generic;
using Asteroids2.Source.Entity.Components;
using Asteroids2.Source.Game;
using Microsoft.Xna.Framework;
#endregion

namespace Asteroids2.Source.Entity;

public class CollisionSystem
{
    private readonly Func<float, float, float, float, float, float, bool> m_doCirclesOverlap
        = static (x1, y1, r1, x2, y2, r2) => MathF.Abs
            ((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2)) <= ((r1 + r2) * (r1 + r2));

    private List<Tuple<Collider, Collider>> m_lastCollisions = new List<Tuple<Collider, Collider>>();
    public List<Collider> Colliders { get; } = new List<Collider>();

    public void OnUpdate(UpdateEventArgs e)
    {
        var currentCollisions = new List<Tuple<Collider, Collider>>();

        for (int i = 0; i < Colliders.Count; i++)
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
                var colliderPair = new Tuple<Collider, Collider>(Colliders[i], Colliders[j]);
                currentCollisions.Add(colliderPair);

                if (m_lastCollisions.Contains(colliderPair)) continue;

                if (Colliders[i].IsSolid && Colliders[j].IsSolid && (Colliders[i].Parent.TimeSinceSpawned > 512))
                {
                    Collider b1 = Colliders[i];
                    Collider b2 = Colliders[j];

                    // Distance between balls
                    float fDistance = MathF.Sqrt((b1.Position.X - b2.Position.X)*(b1.Position.X - b2.Position.X) + (b1.Position.Y - b2.Position.Y)*(b1.Position.Y - b2.Position.Y));

                    // Normal
                    float nx = (b2.Position.X - b1.Position.X) / fDistance;
                    float ny = (b2.Position.Y - b1.Position.Y) / fDistance;

                    // Wikipedia Version - Maths is smarter but same
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