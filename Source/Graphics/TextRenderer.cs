using System.Collections.Generic;
using Asteroids2.Source.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Asteroids2.Source.Graphics;

public class TextRenderer
{
    private Game1 m_root;

    private Color[] m_pixelFont;
    private Texture2D m_fontThing;
    private readonly List<Rectangle> m_letters = new List<Rectangle>();

    public void Init(Game1 root)
    {
        m_root = root;

        m_pixelFont = new Color[177 * 782];
        m_fontThing = m_root.Content.Load<Texture2D>("aseprite_font");
        m_fontThing.GetData(m_pixelFont);

        for (int y = 1; y < 782; y += 11)
        {
            for (int x = 1; x < 177; x += 11)
            {
                int index = (y * 177) + x;
                int len = 0;

                while (m_pixelFont[index] != new Color(255, 255, 255, 255))
                {
                    len++;
                    index++;
                }

                Rectangle charBounds = new Rectangle(x, y, len, 7);

                m_letters.Add(charBounds);
            }
        }
    }

    public void Draw(string input,
        Vector2 position,
        Color color, int scale)
    {
        DrawString((int)position.X, (int)position.Y, input, color, scale);
    }

    public void DrawString(int x, int y, string sText, Color col, int scale)
    {
        int sx = 0;
        int sy = 0;

        foreach (char c in sText)
        {
            switch (c)
            {
            case '\n':
                sx = 0;
                sy += 7 * scale;

                break;

            case '\t':
                sx += 8 * 4 * scale;

                break;

            default:
            {
                int indexInLetters = (c - 32);

                Rectangle letterBoundingBox = m_letters[indexInLetters];

                var charColorArray = new Color[letterBoundingBox.Width * letterBoundingBox.Height];

                if (c == '!')
                {
                    sy -= 1;
                }

                for (int charY = 0; charY < letterBoundingBox.Height; charY++)
                {
                    for (int charX = 0; charX < letterBoundingBox.Width; charX++)
                    {
                        charColorArray[charY * letterBoundingBox.Width + charX] = m_pixelFont[
                            (letterBoundingBox.Y + charY) * 177 + (letterBoundingBox.X + charX)];
                    }
                }

                if (scale > 1)
                {
                    for (int yOffset = 0; yOffset < letterBoundingBox.Height; yOffset++)
                    {
                        for (int xOffset = 0; xOffset < letterBoundingBox.Width; xOffset++)
                        {
                            if (charColorArray[yOffset * letterBoundingBox.Width + xOffset] ==
                                new Color(0, 0, 0, 255))
                            {
                                for (int yss = 0; yss < scale; yss++)
                                {
                                    for (int xss = 0; xss < scale; xss++)
                                    {
                                        m_root.PixelRenderer.DrawPixel(x + sx + (xOffset * scale) + xss,
                                            y + sy + (yOffset * scale) + yss, col);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int yOffset = 0; yOffset < letterBoundingBox.Height; yOffset++)
                    {
                        for (int xOffset = 0; xOffset < letterBoundingBox.Width; xOffset++)
                        {
                            if (charColorArray[yOffset * letterBoundingBox.Width + xOffset] == new Color(0, 0, 0, 255))
                            {
                                m_root.PixelRenderer.DrawPixel(x + sx + xOffset, y + sy + yOffset, col);
                            }
                        }
                    }
                }

                if (c == '!')
                {
                    sy += 1;
                }

                sx += letterBoundingBox.Width * scale;

                break;
            }
            }
        }
    }
}