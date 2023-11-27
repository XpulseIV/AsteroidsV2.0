using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstralAssault.Source.Graphics
{
    public struct DrawTask
    {
        public Texture2D Texture { get; }
        public Rectangle Source { get; }
        public Rectangle Destination { get; }
        public Single Rotation { get; }
        public LayerDepth LayerDepth { get; }
        public EffectContainer EffectContainer = new();
        public Color Color { get; }
        public Vector2 Origin { get; }

        public DrawTask(
            Texture2D texture,
            Rectangle source,
            Rectangle destination,
            Single rotation,
            LayerDepth layerDepth,
            List<IDrawTaskEffect> effects,
            Color color) {
            this.Texture = texture;
            this.Source = source;
            this.Destination = destination;
            this.Rotation = rotation;
            this.LayerDepth = layerDepth;
            this.EffectContainer = new(effects);
            this.Color = color;
            this.Origin = new Vector2(
                (Single)Math.Round(this.Source.Width / 2D),
                (Single)Math.Round(this.Source.Height / 2D));
        }

        public DrawTask(
            Texture2D texture,
            Rectangle source,
            Vector2 position,
            Single rotation,
            LayerDepth layerDepth,
            List<IDrawTaskEffect> effects,
            Color color,
            Vector2 origin) {
            this.Texture = texture;
            this.Source = source;
            this.Destination = new Rectangle(
                (Int32)position.X,
                (Int32)position.Y,
                source.Width,
                source.Height);
            this.Rotation = rotation;
            this.LayerDepth = layerDepth;
            this.EffectContainer = new(effects);
            this.Color = color;
            this.Origin = origin;
        }

        public DrawTask(
            Texture2D texture,
            Vector2 position,
            Single rotation,
            LayerDepth layerDepth,
            List<IDrawTaskEffect> effects,
            Color color,
            Vector2 origin) {
            this.Texture = texture;
            this.Source = new Rectangle(
                0,
                0,
                texture.Width,
                texture.Height);
            this.Destination = new Rectangle(
                (Int32)position.X,
                (Int32)position.Y, this.Source.Width, this.Source.Height);
            this.Rotation = rotation;
            this.LayerDepth = layerDepth;
            this.EffectContainer = new(effects);
            this.Color = color;
            this.Origin = origin;
        }

        public DrawTask(
            Texture2D texture,
            Rectangle source,
            Vector2 position,
            Single rotation,
            LayerDepth layerDepth,
            List<IDrawTaskEffect> effects) {
            this.Texture = texture;
            this.Source = source;
            this.Destination = new Rectangle(
                (Int32)position.X,
                (Int32)position.Y,
                source.Width,
                source.Height);
            this.Rotation = rotation;
            this.LayerDepth = layerDepth;
            this.EffectContainer = new(effects);
            this.Color = Color.White;
            this.Origin = new Vector2(
                (Single)Math.Round(this.Source.Width / 2D),
                (Single)Math.Round(this.Source.Height / 2D));
        }

        public DrawTask(
            Texture2D texture,
            Vector2 position,
            Single rotation,
            LayerDepth layerDepth,
            List<IDrawTaskEffect> effects) {
            this.Texture = texture;
            this.Source = new Rectangle(
                0,
                0,
                texture.Width,
                texture.Height);
            this.Destination = new Rectangle(
                (Int32)position.X,
                (Int32)position.Y, this.Source.Width, this.Source.Height);
            this.Rotation = rotation;
            this.LayerDepth = layerDepth;
            this.EffectContainer = new(effects);
            this.Color = Color.White;
            this.Origin = new Vector2(
                (Single)Math.Round(this.Source.Width / 2D),
                (Single)Math.Round(this.Source.Height / 2D));
        }
    }
}