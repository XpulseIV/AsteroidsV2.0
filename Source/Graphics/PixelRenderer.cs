#region

using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using AstralAssault.Source.Game;
using AstralAssault.Source.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace AstralAssault.Source.Graphics
{
    public class PixelRenderer
    {
        public readonly Color[] m_pixelData;

        private readonly Game1 m_root;

        private readonly Texture2D m_screenTexture;

        public Int32 Width;
        public Int32 Height;

        public PixelRenderer(Game1 root, Int32 width, Int32 height) {
            this.m_root = root;

            this.Width = width;
            this.Height = height;

            this.m_screenTexture = new Texture2D(this.m_root.GraphicsDevice, width, height);

            this.m_pixelData = new Color[width * height];
        }

        internal Texture2D GetPixelScreen() {
            this.m_screenTexture.SetData(this.m_pixelData);

            return this.m_screenTexture;
        }

        public void DrawCircle(Int32 x, Int32 y, Int32 radius, Color c, Byte mask) {
            if (radius > 0) {
                Int32 x0 = 0;
                Int32 y0 = radius;
                Int32 d = 3 - 2 * radius;

                while (y0 >= x0) // only formulate 1/8 of circle
                {
                    // Draw even octants
                    if ((mask & 0x01) != 0) this.DrawPixel(x + x0, y - y0, c); // Q6 - upper right right
                    if ((mask & 0x04) != 0) this.DrawPixel(x + y0, y + x0, c); // Q4 - lower lower right
                    if ((mask & 0x10) != 0) this.DrawPixel(x - x0, y + y0, c); // Q2 - lower left left
                    if ((mask & 0x40) != 0) this.DrawPixel(x - y0, y - x0, c); // Q0 - upper upper left

                    if ((x0 != 0) && (x0 != y0)) {
                        if ((mask & 0x02) != 0) this.DrawPixel(x + y0, y - x0, c); // Q7 - upper upper right
                        if ((mask & 0x08) != 0) this.DrawPixel(x + x0, y + y0, c); // Q5 - lower right right
                        if ((mask & 0x20) != 0) this.DrawPixel(x - y0, y + x0, c); // Q3 - lower lower left
                        if ((mask & 0x80) != 0) this.DrawPixel(x - x0, y - y0, c); // Q1 - upper left left
                    }

                    if (d < 0) d += 4 * x0++ + 6;
                    else d += 4 * (x0++ - y0--) + 10;
                }
            }
            else
                this.DrawPixel(x, y, c);
        }

        public void DrawPixel(Int32 x, Int32 y, Color color) {
            Vector2 wrapedCoords = new Vector2(x, y).Wrap(Game1.GameWidth, Game1.GameHeight);

            x = (Int32)wrapedCoords.X;
            y = (Int32)wrapedCoords.Y;

            Int32 index = y * this.m_screenTexture.Width + x;

            this.m_pixelData[index] = color;
        }

        public void DrawPixel(Vector2 pos, Color color) {
            this.DrawPixel((Int32)pos.X, (Int32)pos.Y, color);
        }

        public void DrawRect(Int32 x, Int32 y, Int32 w, Int32 h, Color c) {
            this.DrawLine(x, y, x + w, y, c);
            this.DrawLine(x + w, y, x + w, y + h, c);
            this.DrawLine(x + w, y + h, x, y + h, c);
            this.DrawLine(x, y + h, x, y, c);
        }

        public void DrawRect(Vector2 p, Vector2 s, Color c) {
            this.DrawRect((Int32)p.X, (Int32)p.Y, (Int32)s.X, (Int32)s.Y, c);
        }

        public void FillRect(Int32 x, Int32 y, Int32 w, Int32 h, Color c) {
            Int32 x2 = x + w;
            Int32 y2 = y + h;

            for (Int32 i = x; i < x2; i++) {
                for (Int32 j = y; j < y2; j++) this.DrawPixel(i, j, c);
            }
        }

        public void FillRect(Vector2 p, Vector2 s, Color c) {
            this.FillRect((Int32)p.X, (Int32)p.Y, (Int32)s.X, (Int32)s.Y, c);
        }

        public void DrawLine(Int32 x1, Int32 y1, Int32 x2, Int32 y2, Color c) {
            Int32 x, y, dx, dy, dx1, dy1, px, py, xe, ye, i;
            dx = x2 - x1;
            dy = y2 - y1;
            dx1 = Math.Abs(dx);
            dy1 = Math.Abs(dy);
            px = 2 * dy1 - dx1;
            py = 2 * dx1 - dy1;

            if (dy1 <= dx1) {
                if (dx >= 0) {
                    x = x1;
                    y = y1;
                    xe = x2;
                }
                else {
                    x = x2;
                    y = y2;
                    xe = x1;
                }

                this.DrawPixel(x, y, c);

                for (i = 0; x < xe; i++) {
                    x = x + 1;

                    if (px < 0)
                        px = px + 2 * dy1;
                    else {
                        if (((dx < 0) && (dy < 0)) || ((dx > 0) && (dy > 0))) y = y + 1;
                        else y = y - 1;
                        px = px + 2 * (dy1 - dx1);
                    }

                    this.DrawPixel(x, y, c);
                }
            }
            else {
                if (dy >= 0) {
                    x = x1;
                    y = y1;
                    ye = y2;
                }
                else {
                    x = x2;
                    y = y2;
                    ye = y1;
                }

                this.DrawPixel(x, y, c);

                for (i = 0; y < ye; i++) {
                    y = y + 1;

                    if (py <= 0)
                        py = py + 2 * dx1;
                    else {
                        if (((dx < 0) && (dy < 0)) || ((dx > 0) && (dy > 0))) x = x + 1;
                        else x = x - 1;
                        py = py + 2 * (dx1 - dy1);
                    }

                    this.DrawPixel(x, y, c);
                }
            }
        }

        public void DrawWireFrameModel(List<Vector2> vecModelCoordinates, Single x, Single y, Single r, Single s,
            Color col) {
            // Create translated model vector of coordinate pairs
            var vecTransformedCoordinates = new List<Vector2>();
            Int32 verts = vecModelCoordinates.Count;

            Matrix translation = Matrix.CreateTranslation(x, y, 0);
            Matrix rotation = Matrix.CreateRotationZ(r);
            Matrix scale = Matrix.CreateScale(s);

            Matrix world = Matrix.Identity;
            world *= rotation;
            world *= scale;
            world *= translation;


            for (Int32 i = 0; i < verts; i++)
                vecTransformedCoordinates.Add(Vector2.Transform(vecModelCoordinates[i], world));


            // Draw Closed Polygon
            for (Int32 i = 0; i < (verts + 1); i++) {
                Int32 j = i + 1;

                this.DrawLine
                (
                    (Int32)vecTransformedCoordinates[i % verts].X, (Int32)vecTransformedCoordinates[i % verts].Y,
                    (Int32)vecTransformedCoordinates[j % verts].X, (Int32)vecTransformedCoordinates[j % verts].Y, col
                );
            }
        }

        public void ClearSimd(Color color) {
            if (Avx2.IsSupported && (this.m_pixelData.Length >= 8)) {
                var colorVector = Vector256.Create(color.PackedValue);
                Int32 numVectors = this.m_pixelData.Length / 8;

                unsafe {
                    fixed (Color* pixels = &this.m_pixelData[0]) {
                        var pixelVectors = (Vector256<UInt32>*)pixels;

                        for (Int32 i = 0; i < numVectors; i++) Avx.Store((UInt32*)(pixelVectors + i), colorVector);
                    }
                }
            }
        }

        public void DrawPixel((Int32, Int32) coordinates, Color color) {
            this.DrawPixel(coordinates.Item1, coordinates.Item2, color);
        }

        public void DrawTriangle(Int32 x1, Int32 y1, Int32 x2, Int32 y2, Int32 x3, Int32 y3, Color color) {
            this.DrawLine(x1, y1, x2, y2, color);
            this.DrawLine(x2, y2, x3, y3, color);
            this.DrawLine(x3, y3, x1, y1, color);
        }
    }
}