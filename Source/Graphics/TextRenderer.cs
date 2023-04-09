﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstralAssault;

public static class TextRenderer
{
    private static Texture2D m_font;

    private static Dictionary<char, int> m_dict;

    public static void Init()
    {
        m_font = AssetManager.Load<Texture2D>("font");
        m_dict = new Dictionary<char, int>
        {
            { 'A', 0 },
            { 'B', 1 },
            { 'C', 2 },
            { 'D', 3 },
            { 'E', 4 },
            { 'F', 5 },
            { 'G', 6 },
            { 'H', 7 },
            { 'I', 8 },
            { 'J', 9 },
            { 'K', 10 },
            { 'L', 11 },
            { 'M', 12 },
            { 'N', 13 },
            { 'O', 14 },
            { 'P', 15 },
            { 'Q', 16 },
            { 'R', 17 },
            { 'S', 18 },
            { 'T', 19 },
            { 'U', 20 },
            { 'V', 21 },
            { 'W', 22 },
            { 'X', 23 },
            { 'Y', 24 },
            { 'Z', 25 },
            { '0', 26 },
            { '1', 27 },
            { '2', 28 },
            { '3', 29 },
            { '4', 30 },
            { '5', 31 },
            { '6', 32 },
            { '7', 33 },
            { '8', 34 },
            { '9', 35 },
            { ':', 36 },
            { ';', 37 },
            { '.', 38 },
            { ',', 39 },
            { ' ', 40 }
        };
    }

    public static List<DrawTask> CreateDrawTasks(
        this string input,
        Vector2 position,
        Color color,
        LayerDepth layerDepth)
    {
        string text = input.ToUpper();
        List<DrawTask> drawTasks = new List<DrawTask>();

        for (int i = 0; i < text.Length; i++)
        {
            int x = m_dict[text[i]] % 6;
            int y = m_dict[text[i]] / 6;

            Rectangle source = new Rectangle(x * 10, y * 10, 10, 10);

            drawTasks.Add
            (
                new DrawTask
                (
                    m_font,
                    source,
                    new Vector2(position.X + i * 10, position.Y),
                    0,
                    layerDepth,
                    new List<IDrawTaskEffect>(),
                    color,
                    Vector2.Zero
                )
            );
        }

        return drawTasks;
    }
}