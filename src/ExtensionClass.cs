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
    }
}