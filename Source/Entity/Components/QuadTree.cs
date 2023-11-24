using System;
using System.Collections.Generic;
using AstralAssault;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

// from https://gamedev.stackexchange.com/questions/131133/monogames-most-efficient-way-of-checking-intersections-between-multiple-objects
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


    public QuadTree(int pLevel, Rectangle pBounds, GameplayState GameplayState)
    {
        m_level = pLevel;
        m_objects = new List<Entity>();
        m_bounds = pBounds;
        m_nodes = new QuadTree[4];
        m_returnObjects = new List<Entity>();
        m_nearby = new List<Entity>();
        this._gameplayState = GameplayState;
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

        m_nodes[0] = new QuadTree(m_level + 1, new Rectangle(x + subWidth, y, subWidth, subHeight), this._gameplayState);
        m_nodes[1] = new QuadTree(m_level + 1, new Rectangle(x, y, subWidth, subHeight), this._gameplayState);
        m_nodes[2] = new QuadTree(m_level + 1, new Rectangle(x, y + subHeight, subWidth, subHeight), this._gameplayState);
        m_nodes[3] = new QuadTree(m_level + 1, new Rectangle(x + subWidth, y + subHeight, subWidth, subHeight), this._gameplayState);
    }


    // Determine which node the object belongs to. -1 means
    // object cannot completely fit within a child node and is part
    // of the parent node

    private int GetIndex(Entity pRect)
    {
        int index = -1;
        double verticalMidpoint = m_bounds.X + m_bounds.Width / 2;
        double horizontalMidpoint = m_bounds.Y + m_bounds.Height / 2;

        // Object can completely fit within the top quadrants
        bool topQuadrant = (pRect.Bounds.Y < horizontalMidpoint) &&
            ((pRect.Bounds.Y + pRect.Bounds.Height) < horizontalMidpoint);

        // Object can completely fit within the bottom quadrants
        bool bottomQuadrant = pRect.Bounds.Y > horizontalMidpoint;

        // Object can completely fit within the left quadrants
        if ((pRect.Bounds.X < verticalMidpoint) && ((m_bounds.X + pRect.Bounds.Width) < verticalMidpoint))
        {
            if (topQuadrant) index = 1;
            else if (bottomQuadrant) index = 2;
        }

        // Object can completely fit within the right quadrants
        else if (pRect.Bounds.X > verticalMidpoint)
        {
            if (topQuadrant) index = 0;
            else if (bottomQuadrant) index = 3;
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

        if ((m_objects.Count > m_maxObjects) && (m_level < m_maxLevels))
        {
            if (m_nodes[0] == null) Split();

            int i = 0;

            while (i < m_objects.Count)
            {
                int index = GetIndex(m_objects[i]);

                if (index != -1)
                {
                    m_nodes[index].Insert(m_objects[i]);
                    m_objects.RemoveAt(i);
                }
                else i++;
            }
        }
    }


    //Return all objects that could collide with the given object (recursive)
    public void Retrieve(List<Entity> returnedObjs, Entity obj)
    {
        if (m_nodes[0] != null)
        {
            int index = GetIndex(obj);

            if (index != -1) m_nodes[index].Retrieve(returnedObjs, obj);
            else
                for (int i = 0; i < m_nodes.Length; i++)
                    m_nodes[i].Retrieve(returnedObjs, obj);
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
        Clear();
        
        foreach (Entity ent in entities) Insert(ent);

        for (int i = 0; i < entities.Count; i++) {
            m_nearby.Clear();
            m_returnObjects.Clear();
            Retrieve(m_returnObjects, entities[i]);

            for (int x = 0; x < m_returnObjects.Count; x++) {
                // Don't check distance with self
                if (entities[i] == m_returnObjects[x]) continue;

                if (!m_doCirclesOverlap
                    (
                        entities[i].Position, entities[i].Bounds.Width, m_returnObjects[x].Position,
                        m_returnObjects[x].Bounds.Width
                    )) continue;

                if (entities[i].IsSolid && m_returnObjects[x].IsSolid && (entities[i].TimeSinceSpawned > 512)) {
                    // Distance between ball centers
                    float distance = Vector2.Distance(entities[i].Position, m_returnObjects[x].Position);

                    // Calculate displacement required
                    float fOverlap = 0.5f * (distance - entities[i].Bounds.Width - m_returnObjects[x].Bounds.Width);

                    entities[i].Position
                        -= fOverlap * (entities[i].Position - m_returnObjects[x].Position) / distance;
                    m_returnObjects[x].Position
                        += fOverlap * (entities[i].Position - m_returnObjects[x].Position) / distance;
                }


                //entities[i].OnCollision(m_returnObjects[x]);
                //m_returnObjects[x].OnCollision(entities[i]);
            }
        }
    }
}