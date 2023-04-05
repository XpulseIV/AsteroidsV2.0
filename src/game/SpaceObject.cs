using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AsteroidsV2._0
{
    internal sealed class SpaceObject
    {
        private Game1 _root;

        public int Size;
        public Vector2 Pos;
        public Vector2 DPos;
        public float Angle;
        public List<Vector2> ObjModel;

        public SpaceObject(Game1 rooty, List<Vector2> model, Vector2 pos, Vector2 deltaPos, float angle, int size)
        {
            this._root = rooty;

            this.ObjModel = model;
            this.Pos = pos;
            this.DPos = deltaPos;
            this.Angle = angle;
            this.Size = size;
        }

        public void Update(float elapsedTime)
        {
            this.Pos += this.DPos * elapsedTime;

            this.Pos = PixelRenderer.Wrap(this.Pos);
        }

        public void Draw(Color color, bool singlePixel)
        {
            switch (singlePixel)
            {
                case true:
                    this._root._pixelRenderer.DrawPixel(this.Pos, color);
                    break;
                case false:
                    this._root._pixelRenderer.DrawWireFrameModel(this.ObjModel, this.Pos.X, this.Pos.Y, this.Angle, this.Size, color);
                    break;
            }
        }
    }
}