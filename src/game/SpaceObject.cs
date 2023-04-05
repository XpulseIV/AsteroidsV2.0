using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AsteroidsV2._0
{
    public class SpaceObject
    {
        public int _size;

        public Vector2 _pos;

        public Vector2 _dPos;

        public float _angle;

        public List<Vector2> _objModel;

        public SpaceObject(List<Vector2> model, Vector2 pos, Vector2 deltaPos, float angle, int size)
        {
            this._objModel = model;
            this._pos = pos;
            this._dPos = deltaPos;
            this._angle = angle;
            this._size = size;
        }
    }
}