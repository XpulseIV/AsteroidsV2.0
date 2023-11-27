using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace AstralAssault.Source.Graphics
{
    public struct ColorEffect : IDrawTaskEffect
    {
        public Vector4 Color { get; }

        public ColorEffect(Vector4 color) {
            this.Color = color;
        }
    }
}