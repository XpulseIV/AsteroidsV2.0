using System;
using Microsoft.Xna.Framework;

namespace AsteroidsV2._0
{
    public static class ExtensionClass
    {
        public static Color RandomColour()
        {
            return new(new Random().Next(0, 256), new Random().Next(0, 256), new Random().Next(0, 256));
        }

        public static Vector2 Wrap(Vector2 position, int RenderWidth, int RenderHeight)
        {
            float x = position.X;
            float y = position.Y;

            if (x < 0) x = RenderWidth + x % RenderWidth;
            if (x >= RenderWidth) x %= RenderWidth;

            if (y < 0) y = RenderHeight + y % RenderHeight;
            if (y >= RenderHeight) y %= RenderHeight;

            return new(x, y);
        }
    }
}