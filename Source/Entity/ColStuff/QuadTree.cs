using System.Collections.Generic;
using System.Diagnostics;
using Asteroids2.Source.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Asteroids2.Source.Entity.ColStuff
{
    // from https://gamedev.stackexchange.com/questions/131133/monogames-most-efficient-way-of-checking-intersections-between-multiple-objects
    public class QuadTree
    {
        private readonly int MAX_OBJECTS = 4;
        private readonly int MAX_LEVELS = 7;

        private int level;
        private List<Entity> objects; // convert to your object type
        private List<Entity> returnObjects;
        private List<Entity> nearby;
        private Rectangle bounds;
        private QuadTree[] nodes;


        public QuadTree(int pLevel, Rectangle pBounds)
        {
            level = pLevel;
            objects = new List<Entity>();
            bounds = pBounds;
            nodes = new QuadTree[4];
            returnObjects = new List<Entity>();
            nearby = new List<Entity>();
        }

        public void Clear()
        {
            objects.Clear();

            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] != null)
                {
                    nodes[i].Clear();
                    nodes[i] = null;
                }
            }
        }


        private void Split()
        {
            int subWidth = bounds.Width / 2;
            int subHeight = bounds.Height / 2;
            int x = bounds.X;
            int y = bounds.Y;

            nodes[0] = new QuadTree(level + 1, new Rectangle(x + subWidth, y, subWidth, subHeight));
            nodes[1] = new QuadTree(level + 1, new Rectangle(x, y, subWidth, subHeight));
            nodes[2] = new QuadTree(level + 1, new Rectangle(x, y + subHeight, subWidth, subHeight));
            nodes[3] = new QuadTree(level + 1, new Rectangle(x + subWidth, y + subHeight, subWidth, subHeight));
        }


        // Determine which node the object belongs to. -1 means
        // object cannot completely fit within a child node and is part
        // of the parent node

        private int GetIndex(Entity pRect)
        {
            int index = -1;
            double verticalMidpoint = bounds.X + (bounds.Width / 2);
            double horizontalMidpoint = bounds.Y + (bounds.Height / 2);

            // Object can completely fit within the top quadrants
            bool topQuadrant = (pRect.Bounds.Y < horizontalMidpoint &&
                pRect.Bounds.Y + pRect.Bounds.Height < horizontalMidpoint);

            // Object can completely fit within the bottom quadrants
            bool bottomQuadrant = (pRect.Bounds.Y > horizontalMidpoint);

            // Object can completely fit within the left quadrants
            if (pRect.Bounds.X < verticalMidpoint && bounds.X + pRect.Bounds.Width < verticalMidpoint)
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
            if (nodes[0] != null)
            {
                int index = GetIndex(pRect);

                if (index != -1)
                {
                    nodes[index].Insert(pRect);

                    return;
                }
            }

            objects.Add(pRect);

            if (objects.Count > MAX_OBJECTS && level < MAX_LEVELS)
            {
                if (nodes[0] == null)
                {
                    Split();
                }

                int i = 0;

                while (i < objects.Count)
                {
                    int index = GetIndex(objects[i]);

                    if (index != -1)
                    {
                        nodes[index].Insert(objects[i]);
                        objects.RemoveAt(i);
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
            if (nodes[0] != null)
            {
                var index = GetIndex(obj);

                if (index != -1)
                {
                    nodes[index].Retrieve(returnedObjs, obj);
                }
                else
                {
                    for (int i = 0; i < nodes.Length; i++)
                    {
                        nodes[i].Retrieve(returnedObjs, obj);
                    }
                }
            }

            returnedObjs.AddRange(objects);
        }

        public void Draw(Game1 root)
        {
            foreach (QuadTree node in nodes)
            {
                if (node != null)
                {
                    node.Draw(root);
                    root.PixelRenderer.DrawRect
                        (node.bounds.Left - 1, node.bounds.Top - 1, node.bounds.Width, 1, Color.White);

                    root.PixelRenderer.DrawRect
                        (node.bounds.Left - 1, node.bounds.Top - 1, 1, node.bounds.Height, Color.White);
                }
            }
        }

        public void Update(GameTime gameTime, List<Entity> entities)
        {
            Clear();

            foreach (Entity ent in entities)
            {
                Insert(ent);
            }

            for (int i = 0; i < entities.Count; i++)
            {
                nearby.Clear();
                returnObjects.Clear();
                Retrieve(returnObjects, entities[i]);

                for (int x = 0; x < returnObjects.Count; x++)
                {
                    // Don't check distance with self
                    if (entities[i] == returnObjects[x]) continue;

                    if (entities[i].Bounds.X < returnObjects[x].Bounds.X + returnObjects[x].Bounds.Width &&
                        entities[i].Bounds.X + entities[i].Bounds.Width > returnObjects[x].Bounds.X &&
                        entities[i].Bounds.Y < returnObjects[x].Bounds.Y + returnObjects[x].Bounds.Height &&
                        entities[i].Bounds.Y + entities[i].Bounds.Height > returnObjects[x].Bounds.Y)
                    {
                        entities[i].OnCollision(returnObjects[x]);
                    }
                }
            }
        }
    }
}