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
            _root = rooty;

            ObjModel = model;
            Pos = pos;
            DPos = deltaPos;
            Angle = angle;
            Size = size;
        }

        public Vector2 MakeHeadingVector(float speed)
        {
            return -Vector2.Transform(Vector2.UnitY, Matrix.CreateRotationZ(Angle)) * speed;
        }

        public void Update(float elapsedTime)
        {
            Pos += DPos * elapsedTime;
            Pos = ExtensionClass.Wrap(Pos, Renderer.RenderWidth, Renderer.RenderHeight);
        }

        public void Draw(Color color, bool singlePixel)
        {
            switch (singlePixel)
            {
                case true:
                    _root.Renderer.DrawPixel(Pos, color);
                    break;
                case false:
                    _root.Renderer.DrawWireFrameModel(ObjModel, Pos.X, Pos.Y, Angle, Size, color);
                    break;
            }
        }
    }
}