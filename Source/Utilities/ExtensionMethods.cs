#region
using System;
using Microsoft.Xna.Framework;
#endregion

namespace Asteroids2.Source.Utilities;

public static class ExtensionMethods
{
    internal static int Mod(this int x, int y) => (Math.Abs(x * y) + x) % y;

    internal static Vector2 RotateVector(this Vector2 inVector, float rotation) =>
        Vector2.Transform(inVector, Matrix.CreateRotationZ(rotation));

    public static Vector2 Wrap(this Vector2 position, int RenderWidth, int RenderHeight)
    {
        float x = position.X;
        float y = position.Y;

        if (x < 0) x = RenderWidth + x % RenderWidth;
        if (x >= RenderWidth) x %= RenderWidth;

        if (y < 0) y = RenderHeight + y % RenderHeight;
        if (y >= RenderHeight) y %= RenderHeight;

        return new Vector2(x, y);
    }

    public static Color toColor(this int ic)
    {
        byte r = (byte)(ic >> 24);
        byte g = (byte)(ic >> 16);
        byte b = (byte)(ic >> 8);

        return new Color((float)r, g, b, 255);
    }

    public static int twoDToOneD((int, int, int) hmmm) => hmmm.Item1 + hmmm.Item2 * hmmm.Item3;
}