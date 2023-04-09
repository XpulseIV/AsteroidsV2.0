using Microsoft.Xna.Framework;

namespace Vector2Extensions;

internal static class Vector2Ext
{
    /// <summary>
    /// Given a vector, get the vector perpendicular to that vec
    /// </summary>
    /// <param name="myVector"></param>
    /// <returns></returns>
    internal static Vector2 Perp(this Vector2 myVector) => new Vector2(-myVector.Y, myVector.X);
}