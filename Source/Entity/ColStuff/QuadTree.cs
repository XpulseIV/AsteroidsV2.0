using System;
using System.Collections.Generic;
using System.Diagnostics;
using Asteroids2.Source.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Asteroids2.Source.Entity.ColStuff
{
    // from https://gamedev.stackexchange.com/questions/131133/monogames-most-efficient-way-of-checking-intersections-between-multiple-objects
    public class QuadTree
    {
        // Use 0, 1 does not really work
        private int m_collisionResponeThing = 0;

        private readonly int m_maxObjects = 4;
        private readonly int m_maxLevels = 7;

        private int m_level;
        private List<Entity> m_objects; // convert to your object type
        private List<Entity> m_returnObjects;
        private List<Entity> m_nearby;
        private Rectangle m_bounds;
        private QuadTree[] m_nodes;


        public QuadTree(int pLevel, Rectangle pBounds)
        {
            m_level = pLevel;
            m_objects = new List<Entity>();
            m_bounds = pBounds;
            m_nodes = new QuadTree[4];
            m_returnObjects = new List<Entity>();
            m_nearby = new List<Entity>();
        }

        public void Clear()
        {
            m_objects.Clear();

            for (int i = 0; i < m_nodes.Length; i++)
            {
                if (m_nodes[i] != null)
                {
                    m_nodes[i].Clear();
                    m_nodes[i] = null;
                }
            }
        }


        private void Split()
        {
            int subWidth = m_bounds.Width / 2;
            int subHeight = m_bounds.Height / 2;
            int x = m_bounds.X;
            int y = m_bounds.Y;

            m_nodes[0] = new QuadTree(m_level + 1, new Rectangle(x + subWidth, y, subWidth, subHeight));
            m_nodes[1] = new QuadTree(m_level + 1, new Rectangle(x, y, subWidth, subHeight));
            m_nodes[2] = new QuadTree(m_level + 1, new Rectangle(x, y + subHeight, subWidth, subHeight));
            m_nodes[3] = new QuadTree(m_level + 1, new Rectangle(x + subWidth, y + subHeight, subWidth, subHeight));
        }


        // Determine which node the object belongs to. -1 means
        // object cannot completely fit within a child node and is part
        // of the parent node

        private int GetIndex(Entity pRect)
        {
            int index = -1;
            double verticalMidpoint = m_bounds.X + (m_bounds.Width / 2);
            double horizontalMidpoint = m_bounds.Y + (m_bounds.Height / 2);

            // Object can completely fit within the top quadrants
            bool topQuadrant = (pRect.Bounds.Y < horizontalMidpoint &&
                pRect.Bounds.Y + pRect.Bounds.Height < horizontalMidpoint);

            // Object can completely fit within the bottom quadrants
            bool bottomQuadrant = (pRect.Bounds.Y > horizontalMidpoint);

            // Object can completely fit within the left quadrants
            if (pRect.Bounds.X < verticalMidpoint && m_bounds.X + pRect.Bounds.Width < verticalMidpoint)
            {
                if (topQuadrant)
                {
                    index = 1;
                }
                else if (bottomQuadrant)
                {
                    index = 2;
                }
            }

            // Object can completely fit within the right quadrants
            else if (pRect.Bounds.X > verticalMidpoint)
            {
                if (topQuadrant)
                {
                    index = 0;
                }
                else if (bottomQuadrant)
                {
                    index = 3;
                }
            }

            return index;
        }

        public void Insert(Entity pRect)
        {
            if (m_nodes[0] != null)
            {
                int index = GetIndex(pRect);

                if (index != -1)
                {
                    m_nodes[index].Insert(pRect);

                    return;
                }
            }

            m_objects.Add(pRect);

            if (m_objects.Count > m_maxObjects && m_level < m_maxLevels)
            {
                if (m_nodes[0] == null)
                {
                    Split();
                }

                int i = 0;

                while (i < m_objects.Count)
                {
                    int index = GetIndex(m_objects[i]);

                    if (index != -1)
                    {
                        m_nodes[index].Insert(m_objects[i]);
                        m_objects.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }


        //Return all objects that could collide with the given object (recursive)
        public void Retrieve(List<Entity> returnedObjs, Entity obj)
        {
            if (m_nodes[0] != null)
            {
                int index = GetIndex(obj);

                if (index != -1)
                {
                    m_nodes[index].Retrieve(returnedObjs, obj);
                }
                else
                {
                    for (int i = 0; i < m_nodes.Length; i++)
                    {
                        m_nodes[i].Retrieve(returnedObjs, obj);
                    }
                }
            }

            returnedObjs.AddRange(m_objects);
        }

        public void Draw(Game1 root)
        {
            foreach (QuadTree node in m_nodes)
            {
                if (node != null)
                {
                    node.Draw(root);
                    root.PixelRenderer.DrawRect
                        (node.m_bounds.Left - 1, node.m_bounds.Top - 1, node.m_bounds.Width, 1, Color.White);

                    root.PixelRenderer.DrawRect
                        (node.m_bounds.Left - 1, node.m_bounds.Top - 1, 1, node.m_bounds.Height, Color.White);
                }
            }
        }

        private Func<Vector2, float, Vector2, float, bool> m_doCirclesOverlap
            = static (p1, r1, p2, r2) => MathF.Abs
                ((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y)) <= ((r1 + r2) * (r1 + r2));

        public void Update(GameTime gameTime, List<Entity> entities)
        {
            Clear();

            foreach (Entity ent in entities)
            {
                Insert(ent);
            }

            for (int i = 0; i < entities.Count; i++)
            {
                m_nearby.Clear();
                m_returnObjects.Clear();
                Retrieve(m_returnObjects, entities[i]);

                for (int x = 0; x < m_returnObjects.Count; x++)
                {
                    // Don't check distance with self
                    if (entities[i] == m_returnObjects[x]) continue;

                    if (!m_doCirclesOverlap
                        (
                            entities[i].Position, entities[i].Bounds.Width, m_returnObjects[x].Position,
                            m_returnObjects[x].Bounds.Width
                        )) continue;

                    if (entities[i].IsSolid && m_returnObjects[x].IsSolid && (entities[i].TimeSinceSpawned > 512))
                    {
                        // Distance between ball centers
                        float distance = Vector2.Distance(entities[i].Position, m_returnObjects[x].Position);

                        // Calculate displacement required
                        float fOverlap = 0.5f * (distance - entities[i].Bounds.Width - m_returnObjects[x].Bounds.Width);

                        entities[i].Position
                            -= fOverlap * (entities[i].Position - m_returnObjects[x].Position) / distance;
                        m_returnObjects[x].Position
                            += fOverlap * (entities[i].Position - m_returnObjects[x].Position) / distance;

                        switch (m_collisionResponeThing)
                        {
                        case 0:
                        {
                            // Unknown gathering
                            float v11 = entities[i].Velocity.Length();
                            float v21 = m_returnObjects[x].Velocity.Length();

                            int m11 = entities[i].Mass;
                            int m21 = m_returnObjects[x].Mass;

                            float angle11 = entities[i].Velocity.ToAngle();
                            float angle21 = m_returnObjects[x].Velocity.ToAngle();

                            float phi1 = angle11 - angle21;

                            // Equation solving
                            float v1Fx1
                                = ((v11 * MathF.Cos(angle11 - phi1) * (m11 - m21) + 2 * m21 * v21 * MathF.Cos(angle21 - phi1)) /
                                    (m11 + m21)) * MathF.Cos
                                    (phi1) + v11 * MathF.Sin(angle11 - phi1) * MathF.Cos(phi1 + (MathF.PI / 2));

                            float v1Fy1
                                = ((v11 * MathF.Cos(angle11 - phi1) * (m11 - m21) + 2 * m21 * v21 * MathF.Cos(angle21 - phi1)) /
                                    (m11 + m21)) * MathF.Sin
                                    (phi1) + v11 * MathF.Sin(angle11 - phi1) * MathF.Sin(phi1 + (MathF.PI / 2));


                            float v2Fx1
                                = ((v21 * MathF.Cos(angle21 - phi1) * (m21 - m11) + 2 * m11 * v11 * MathF.Cos(angle11 - phi1)) /
                                    (m21 + m11)) * MathF.Cos
                                    (phi1) + v21 * MathF.Sin(angle21 - phi1) * MathF.Cos(phi1 + (MathF.PI / 2));

                            float v2Fy1
                                = ((v21 * MathF.Cos(angle21 - phi1) * (m21 - m11) + 2 * m11 * v11 * MathF.Cos(angle11 - phi1)) /
                                    (m21 + m11)) * MathF.Sin
                                    (phi1) + v21 * MathF.Sin(angle21 - phi1) * MathF.Sin(phi1 + (MathF.PI / 2));

                            entities[i].Velocity = new Vector2(v1Fx1, v1Fy1);
                            m_returnObjects[x].Velocity = new Vector2(v2Fx1, v2Fy1);

                            break;
                        }

                        case 1:
                            Vector2 v12 = entities[i].Velocity;
                            Vector2 v22 = m_returnObjects[x].Velocity;
                            int m12 = entities[i].Mass;
                            int m22 = m_returnObjects[x].Mass;

                            Vector2 x1 = entities[i].Position;
                            Vector2 x2 = m_returnObjects[x].Position;

                            int e1P2 = (2 * m22) / (m12 + m22);
                            int e2P2 = (2 * m12) / (m12 + m22);

                            float e1P3 = (Vector2.Dot(v12 - v22, x1 - x2)) / (x1 - x2).LengthSquared();
                            float e2P3 = (Vector2.Dot(v22 - v12, x2 - x1)) / (x2 - x1).LengthSquared();

                            Vector2 e1P4 = x1 - x2;
                            Vector2 e2P4 = x2 - x1;

                            Vector2 e1P5 = v12 - e1P2 * e1P3 * e1P4;
                            Vector2 e2P5 = v22 - e2P2 * e2P3 * e2P4;

                            entities[i].Velocity = e1P5;
                            m_returnObjects[x].Velocity = e2P5;

                            break;
                        }
                    }


                    entities[i].OnCollision(m_returnObjects[x]);
                    m_returnObjects[x].OnCollision(entities[i]);
                }
            }
        }
    }
}