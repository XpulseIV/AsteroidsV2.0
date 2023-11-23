using System;
using Microsoft.Xna.Framework;

namespace AstralAssault;

public static class ExtensionMethods
{
    public static Int32 Mod(this Int32 x, Int32 y) => (Math.Abs(x * y) + x) % y;

    public static Vector2 Wrap(this Vector2 position, Int32 renderWidth, Int32 renderHeight) {
        Single x = position.X;
        Single y = position.Y;

        if (x < 0) {
            x = renderWidth + (x % renderWidth);
        }

        if (x >= renderWidth) {
            x %= renderWidth;
        }

        if (y < 0) {
            y = renderHeight + (y % renderHeight);
        }

        if (y >= renderHeight) {
            y %= renderHeight;
        }

        return new Vector2(x, y);
    }

    public static Color toColor(this Int32 ic) {
        Byte r = (Byte)(ic >> 24);
        Byte g = (Byte)(ic >> 16);
        Byte b = (Byte)(ic >> 8);

        return new Color((Single)r, g, b, 255);
    }

    public static Int32 twoDToOneD((Int32, Int32) coordinates, Int32 rowWidth) => coordinates.Item1 + coordinates.Item2 * rowWidth;

    public static Int32 twoDToOneD((Int32, Int32, Int32) input) => input.Item1 + input.Item2 * input.Item3;

    public static (Int32, Int32) oneDToTwoD(Int32 index, Int32 rowWidth) {
        Int32 i = index % rowWidth;
        Int32 j = index / rowWidth;

        return (i, j);
    }

}