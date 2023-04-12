#region
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Asteroids2.Source.Game;
using Asteroids2.Source.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Asteroids2.Source.Graphics;

public class PixelRenderer
{
    public readonly Color[] m_pixelData;

    private readonly int m_width;
    private readonly int m_height;

    private readonly Game1 m_root;

    private readonly Texture2D m_screenTexture;

    public PixelRenderer(Game1 root, int width, int height)
    {
        m_width = width;
        m_height = height;

        m_root = root;
        m_screenTexture = new Texture2D(m_root.GraphicsDevice, width, height);

        m_pixelData = new Color[width * height];
    }

    internal Texture2D GetPixelScreen()
    {
        m_screenTexture.SetData(m_pixelData);

        return m_screenTexture;
    }

    public void DrawCircle(int x, int y, int radius, Color c, byte mask)
    {
        if (radius > 0)
        {
            int x0 = 0;
            int y0 = radius;
            int d = 3 - 2 * radius;

            while (y0 >= x0) // only formulate 1/8 of circle
            {
                // Draw even octants
                if ((mask & 0x01) != 0) DrawPixel(x + x0, y - y0, c); // Q6 - upper right right
                if ((mask & 0x04) != 0) DrawPixel(x + y0, y + x0, c); // Q4 - lower lower right
                if ((mask & 0x10) != 0) DrawPixel(x - x0, y + y0, c); // Q2 - lower left left
                if ((mask & 0x40) != 0) DrawPixel(x - y0, y - x0, c); // Q0 - upper upper left

                if ((x0 != 0) && (x0 != y0))
                {
                    if ((mask & 0x02) != 0) DrawPixel(x + y0, y - x0, c); // Q7 - upper upper right
                    if ((mask & 0x08) != 0) DrawPixel(x + x0, y + y0, c); // Q5 - lower right right
                    if ((mask & 0x20) != 0) DrawPixel(x - y0, y + x0, c); // Q3 - lower lower left
                    if ((mask & 0x80) != 0) DrawPixel(x - x0, y - y0, c); // Q1 - upper left left
                }

                if (d < 0) d += 4 * x0++ + 6;
                else d += 4 * (x0++ - y0--) + 10;
            }
        }
        else
            DrawPixel(x, y, c);
    }

    public void DrawPixel(int x, int y, Color color)
    {
        Vector2 wrapedCoords = new Vector2(x, y).Wrap(m_width, m_height);

        x = (int)wrapedCoords.X;
        y = (int)wrapedCoords.Y;

        int index = y * m_screenTexture.Width + x;
        m_pixelData[index] = color;
    }

    public void DrawPixel(Vector2 pos, Color color)
    {
        DrawPixel((int)pos.X, (int)pos.Y, color);
    }

    public void DrawRect(int x, int y, int w, int h, Color c)
    {
        DrawLine(x, y, x + w, y, c);
        DrawLine(x + w, y, x + w, y + h, c);
        DrawLine(x + w, y + h, x, y + h, c);
        DrawLine(x, y + h, x, y, c);
    }

    public void FillRect(int x, int y, int w, int h, Color c)
    {
        int x2 = x + w;
        int y2 = y + h;

        for (int i = x; i < x2; i++)
        {
            for (int j = y; j < y2; j++)
                DrawPixel(i, j, c);
        }
    }

    public void DrawLine(int x1, int y1, int x2, int y2, Color c)
    {
        int x, y, dx, dy, dx1, dy1, px, py, xe, ye, i;
        dx = x2 - x1;
        dy = y2 - y1;
        dx1 = Math.Abs(dx);
        dy1 = Math.Abs(dy);
        px = 2 * dy1 - dx1;
        py = 2 * dx1 - dy1;

        if (dy1 <= dx1)
        {
            if (dx >= 0)
            {
                x = x1;
                y = y1;
                xe = x2;
            }
            else
            {
                x = x2;
                y = y2;
                xe = x1;
            }

            DrawPixel(x, y, c);

            for (i = 0; x < xe; i++)
            {
                x = x + 1;

                if (px < 0)
                    px = px + 2 * dy1;
                else
                {
                    if (((dx < 0) && (dy < 0)) || ((dx > 0) && (dy > 0))) y = y + 1;
                    else y = y - 1;
                    px = px + 2 * (dy1 - dx1);
                }

                DrawPixel(x, y, c);
            }
        }
        else
        {
            if (dy >= 0)
            {
                x = x1;
                y = y1;
                ye = y2;
            }
            else
            {
                x = x2;
                y = y2;
                ye = y1;
            }

            DrawPixel(x, y, c);

            for (i = 0; y < ye; i++)
            {
                y = y + 1;

                if (py <= 0)
                    py = py + 2 * dx1;
                else
                {
                    if (((dx < 0) && (dy < 0)) || ((dx > 0) && (dy > 0))) x = x + 1;
                    else x = x - 1;
                    py = py + 2 * (dx1 - dy1);
                }

                DrawPixel(x, y, c);
            }
        }
    }

    public void DrawWireFrameModel(List<Vector2> vecModelCoordinates, float x, float y, float r, float s, Color col)
    {
        // Create translated model vector of coordinate pairs
        var vecTransformedCoordinates = new List<Vector2>();
        int verts = vecModelCoordinates.Count;

        Matrix translation = Matrix.CreateTranslation(x, y, 0);
        Matrix rotation = Matrix.CreateRotationZ(r);
        Matrix scale = Matrix.CreateScale(s);

        Matrix world = Matrix.Identity;
        world *= rotation;
        world *= scale;
        world *= translation;


        for (int i = 0; i < verts; i++) vecTransformedCoordinates.Add(Vector2.Transform(vecModelCoordinates[i], world));


        // Draw Closed Polygon
        for (int i = 0; i < (verts + 1); i++)
        {
            int j = i + 1;

            DrawLine
            (
                (int)vecTransformedCoordinates[i % verts].X, (int)vecTransformedCoordinates[i % verts].Y,
                (int)vecTransformedCoordinates[j % verts].X, (int)vecTransformedCoordinates[j % verts].Y, col
            );
        }
    }

    public void ClearSimd(Color color)
    {
        if (Avx2.IsSupported && (m_pixelData.Length >= 8))
        {
            var colorVector = Vector256.Create(color.PackedValue);
            int numVectors = m_pixelData.Length / 8;

            unsafe
            {
                fixed (Color* pixels = &m_pixelData[0])
                {
                    var pixelVectors = (Vector256<uint>*)pixels;

                    for (int i = 0; i < numVectors; i++) Avx.Store((uint*)(pixelVectors + i), colorVector);
                }
            }
        }
    }
}