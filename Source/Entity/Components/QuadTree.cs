using System;
using System.Collections.Generic;
using AstralAssault.Source.Game;
using AstralAssault.Source.Game.GameState;
using Microsoft.Xna.Framework;

// from https://gamedev.stackexchange.com/questions/131133/monogames-most-efficient-way-of-checking-intersections-between-multiple-objects
namespace AstralAssault.Source.Entity.Components
{
    public class CollisionPair
    {
        public Entity Entity1 { get; }
        public Entity Entity2 { get; }

        public CollisionPair(Entity entity1, Entity entity2) {
            this.Entity1 = entity1;
            this.Entity2 = entity2;
        }
    }

    public class QuadTree
    {
        private GameplayState _gameplayState;

        private readonly int m_maxObjects = 4;
        private readonly int m_maxLevels = 7;

        private int m_level;
        private List<Entity> m_objects; // convert to your object type
        private List<Entity> m_returnObjects;
        private List<Entity> m_nearby;
        private Rectangle m_bounds;
        private QuadTree[] m_nodes;


        public QuadTree(int pLevel, Rectangle pBounds, GameplayState GameplayState) {
            this.m_level = pLevel;
            this.m_objects = new List<Entity>();
            this.m_bounds = pBounds;
            this.m_nodes = new QuadTree[4];
            this.m_returnObjects = new List<Entity>();
            this.m_nearby = new List<Entity>();
            this._gameplayState = GameplayState;
        }

        public void Clear() {
            this.m_objects.Clear();

            for (int i = 0; i < this.m_nodes.Length; i++) {
                if (this.m_nodes[i] != null) {
                    this.m_nodes[i].Clear();
                    this.m_nodes[i] = null;
                }
            }
        }

        private void Split() {
            int subWidth = this.m_bounds.Width / 2;
            int subHeight = this.m_bounds.Height / 2;
            int x = this.m_bounds.X;
            int y = this.m_bounds.Y;

            this.m_nodes[0] = new QuadTree(this.m_level + 1, new Rectangle(x + subWidth, y, subWidth, subHeight),
                this._gameplayState);
            this.m_nodes[1] = new QuadTree(this.m_level + 1, new Rectangle(x, y, subWidth, subHeight), this._gameplayState);
            this.m_nodes[2] = new QuadTree(this.m_level + 1, new Rectangle(x, y + subHeight, subWidth, subHeight),
                this._gameplayState);
            this.m_nodes[3] = new QuadTree(this.m_level + 1, new Rectangle(x + subWidth, y + subHeight, subWidth, subHeight),
                this._gameplayState);
        }

        // Determine which node the object belongs to. -1 means
        // object cannot completely fit within a child node and is part
        // of the parent node

        private int GetIndex(Entity pRect) {
            int index = -1;
            double verticalMidpoint = this.m_bounds.X + this.m_bounds.Width / 2;
            double horizontalMidpoint = this.m_bounds.Y + this.m_bounds.Height / 2;

            // Object can completely fit within the top quadrants
            bool topQuadrant = (pRect.Bounds.Y < horizontalMidpoint) &&
                               ((pRect.Bounds.Y + pRect.Bounds.Height) < horizontalMidpoint);

            // Object can completely fit within the bottom quadrants
            bool bottomQuadrant = pRect.Bounds.Y > horizontalMidpoint;

            // Object can completely fit within the left quadrants
            if ((pRect.Bounds.X < verticalMidpoint) && ((this.m_bounds.X + pRect.Bounds.Width) < verticalMidpoint)) {
                if (topQuadrant) index = 1;
                else if (bottomQuadrant) index = 2;
            }

            // Object can completely fit within the right quadrants
            else if (pRect.Bounds.X > verticalMidpoint) {
                if (topQuadrant) index = 0;
                else if (bottomQuadrant) index = 3;
            }

            return index;
        }

        public void Insert(Entity pRect) {
            if (this.m_nodes[0] != null) {
                int index = this.GetIndex(pRect);

                if (index != -1) {
                    this.m_nodes[index].Insert(pRect);

                    return;
                }
            }

            this.m_objects.Add(pRect);

            if ((this.m_objects.Count > this.m_maxObjects) && (this.m_level < this.m_maxLevels)) {
                if (this.m_nodes[0] == null) this.Split();

                int i = 0;

                while (i < this.m_objects.Count) {
                    int index = this.GetIndex(this.m_objects[i]);

                    if (index != -1) {
                        this.m_nodes[index].Insert(this.m_objects[i]);
                        this.m_objects.RemoveAt(i);
                    }
                    else i++;
                }
            }
        }


        //Return all objects that could collide with the given object (recursive)
        public void Retrieve(List<Entity> returnedObjs, Entity obj) {
            if (this.m_nodes[0] != null) {
                int index = this.GetIndex(obj);

                if (index != -1) this.m_nodes[index].Retrieve(returnedObjs, obj);
                else
                    for (int i = 0; i < this.m_nodes.Length; i++)
                        this.m_nodes[i].Retrieve(returnedObjs, obj);
            }

            returnedObjs.AddRange(this.m_objects);
        }

        public void Draw(Game1 root) {
            foreach (QuadTree node in this.m_nodes) {
                if (node != null) {
                    node.Draw(root);
                    root.Renderer.DrawRect
                        (node.m_bounds.Left - 1, node.m_bounds.Top - 1, node.m_bounds.Width, 1, Color.White);

                    root.Renderer.DrawRect
                        (node.m_bounds.Left - 1, node.m_bounds.Top - 1, 1, node.m_bounds.Height, Color.White);
                }
            }
        }

        private Func<Vector2, float, Vector2, float, bool> m_doCirclesOverlap
            = static (p1, r1, p2, r2) => MathF.Abs
                ((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y)) <= ((r1 + r2) * (r1 + r2));

        public void Update(float deltaTime, List<Entity> entities) {
            this.Clear();

            foreach (Entity ent in entities) this.Insert(ent);

            var collisions = new List<CollisionPair>();

            for (int i = 0; i < entities.Count; i++) {
                this.m_nearby.Clear();
                this.m_returnObjects.Clear();
                this.Retrieve(this.m_returnObjects, entities[i]);

                for (int x = 0; x < this.m_returnObjects.Count; x++) {
                    // Don't check distance with self
                    if (entities[i] == this.m_returnObjects[x]) continue;

                    if (!this.m_doCirclesOverlap
                        (
                            entities[i].Position, entities[i].Bounds.Width, this.m_returnObjects[x].Position,
                            this.m_returnObjects[x].Bounds.Width
                        )) continue;

                    // Store collisions for later processing
                    collisions.Add(new CollisionPair(entities[i], this.m_returnObjects[x]));
                }
            }

            foreach (CollisionPair collisionPair in collisions) {
                if (collisionPair.Entity1.IsSolid && collisionPair.Entity2.IsSolid && (collisionPair.Entity1.TimeSinceSpawned > 512)) {
                    // Distance between ball centers
                    float distance = Vector2.Distance(collisionPair.Entity1.Position, collisionPair.Entity2.Position);

                    // Calculate displacement required
                    float fOverlap = 0.5f * (distance - collisionPair.Entity1.Bounds.Width - collisionPair.Entity2.Bounds.Width);

                    collisionPair.Entity1.Position
                        -= fOverlap * (collisionPair.Entity1.Position - collisionPair.Entity2.Position) / distance;
                    collisionPair.Entity2.Position
                        += fOverlap * (collisionPair.Entity1.Position - collisionPair.Entity2.Position) / distance;

                    // Unknown gathering
                    var v1 = collisionPair.Entity1.Velocity;
                    var v2 = collisionPair.Entity2.Velocity;
                    var m1 = collisionPair.Entity1.mass;
                    var m2 = collisionPair.Entity2.mass;
                    var x1 = collisionPair.Entity1.Position;
                    var x2 = collisionPair.Entity2.Position;

                    Vector2 v1Prime = v1 - (2 * m2 / (m1 + m2)) * Vector2.Dot(v1 - v2, x1 - x2) / (x1 - x2).LengthSquared() * (x1 - x2);
                    Vector2 v2Prime = v2 - (2 * m1 / (m1 + m2)) * Vector2.Dot(v2 - v1, x2 - x1) / (x2 - x1).LengthSquared() * (x2 - x1);

                    collisionPair.Entity1.Velocity = v1Prime;
                    collisionPair.Entity2.Velocity = v2Prime;
                }

                collisionPair.Entity1.OnCollision(collisionPair.Entity2);
                collisionPair.Entity2.OnCollision(collisionPair.Entity1);
            }
        }
    }
}