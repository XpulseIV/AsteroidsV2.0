using System;
using System.Collections.Generic;
using System.Diagnostics;
using AsteroidsV2._0;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

internal sealed class PixelRenderer
{
    private GraphicsDevice _graphicsDevice;
    private readonly Texture2D _pixel;
    private readonly Color[] _pixelData;
    private readonly SpriteBatch _spriteBatch;

    private readonly Game1 _root;

    private RenderTarget2D _renderTarget;

    public const int RenderWidth = (int)Width.Quarter;
    public const int RenderHeight = (int)Height.Quarter;
    private Matrix _scale;
    public float ScaleX;
    public float ScaleY;

    private enum Width
    {
        Full = 1920,
        Half = 960,
        Quarter = 480,
        Eighth = 240,
        Sixteenth = 120,
        ThirtySecond = 60,
        SixtyFourth = 30
    }

    private enum Height
    {
        Full = 1080,
        Half = 540,
        Quarter = 270,
        Eighth = 135,
        Sixteenth = 67,
        ThirtySecond = 34,
        SixtyFourth = 17
    }

    public PixelRenderer(GraphicsDevice graphicsDevice, Game1 root)
    {
        this._root = root;

        this._graphicsDevice = graphicsDevice;

        this._pixelData = new Color[PixelRenderer.RenderWidth * PixelRenderer.RenderHeight];
        for (int i = 0; i < this._pixelData.Length; i++) this._pixelData[i] = Color.White;

        this._pixel = new(graphicsDevice, PixelRenderer.RenderWidth, PixelRenderer.RenderHeight);
        this._pixel.SetData(this._pixelData);

        this._spriteBatch = new(graphicsDevice);
    }

    public void SetupWindow()
    {
        this._root.Window.Title = "Asteroids but better";

        this._root.Graphics.PreferredBackBufferWidth = (int)Width.Half;
        this._root.Graphics.PreferredBackBufferHeight = (int)Height.Half;
        _root.Graphics.ApplyChanges();

        ScaleX = _root.Graphics.PreferredBackBufferWidth / (float)PixelRenderer.RenderWidth;
        ScaleY = _root.Graphics.PreferredBackBufferHeight / (float)PixelRenderer.RenderHeight;
        _scale = Matrix.CreateScale(new Vector3(ScaleX, ScaleY, 1));

        _root.Graphics.SynchronizeWithVerticalRetrace = false;
        _root.IsFixedTimeStep = false;

        this._renderTarget = new(
            this._root.GraphicsDevice,
            this._root.GraphicsDevice.PresentationParameters.BackBufferWidth,
            this._root.GraphicsDevice.PresentationParameters.BackBufferHeight,
            false,
            this._root.GraphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24);
    }

    public void DrawLine(int x1, int y1, int x2, int y2, Color c)
    {
        int x, y, dx, dy, dx1, dy1, px, py, xe, ye, i;
        dx = x2 - x1; dy = y2 - y1;
        dx1 = Math.Abs(dx); dy1 = Math.Abs(dy);
        px = 2 * dy1 - dx1;	py = 2 * dx1 - dy1;
        if (dy1 <= dx1)
        {
            if (dx >= 0)
            { x = x1; y = y1; xe = x2; }
            else
            { x = x2; y = y2; xe = x1;}

            this.DrawPixel(x, y, c);

            for (i = 0; x<xe; i++)
            {
                x = x + 1;
                if (px<0)
                    px = px + 2 * dy1;
                else
                {
                    if ((dx<0 && dy<0) || (dx>0 && dy>0)) y = y + 1; else y = y - 1;
                    px = px + 2 * (dy1 - dx1);
                }

                this.DrawPixel(x, y, c);
            }
        }
        else
        {
            if (dy >= 0)
            { x = x1; y = y1; ye = y2; }
            else
            { x = x2; y = y2; ye = y1; }

            this.DrawPixel(x, y, c);

            for (i = 0; y<ye; i++)
            {
                y = y + 1;
                if (py <= 0)
                    py = py + 2 * dx1;
                else
                {
                    if ((dx<0 && dy<0) || (dx>0 && dy>0)) x = x + 1; else x = x - 1;
                    py = py + 2 * (dx1 - dy1);
                }

                this.DrawPixel(x, y, c);
            }
        }
    }

    public void DrawTriangle(int x1, int y1, int x2, int y2, int x3, int y3, Color c)
    {
        this.DrawLine(x1, y1, x2, y2, c);
        this.DrawLine(x2, y2, x3, y3, c);
        this.DrawLine(x3, y3, x1, y1, c);
    }

    public static Vector2 Wrap(Vector2 position)
    {
        float x = position.X;
        float y = position.Y;

        if (x < 0) x = RenderWidth + x % RenderWidth;
        if (x >= RenderWidth) x %= RenderWidth;

        if (y < 0) y = RenderHeight + y % RenderHeight;
        if (y >= RenderHeight) y %= RenderHeight;

        return new(x, y);
    }

    public void DrawPixel(int x, int y, Color color)
    {
        Vector2 wrapedCoords = PixelRenderer.Wrap(new(x, y));

        x = (int)wrapedCoords.X;
        y = (int)wrapedCoords.Y;

        int index = y * this._pixel.Width + x;
        this._pixelData[index] = color;
    }

    public void DrawPixel(Vector2 pos, Color color)
    {
        DrawPixel((int)pos.X, (int)pos.Y, color);
    }

    public void DrawWireFrameModel(List<Vector2> vecModelCoordinates, float x, float y, float r, float s, Color col)
    {
        // Create translated model vector of coordinate pairs
        List<Vector2> vecTransformedCoordinates = new List<Vector2>();
        int verts = vecModelCoordinates.Count;
        vecTransformedCoordinates.AddRange(vecModelCoordinates);

        Matrix translation = Matrix.CreateTranslation(x, y, 0);
        Matrix rotation = Matrix.CreateRotationZ(r);
        Matrix scale = Matrix.CreateScale(s);

        Matrix world = Matrix.Identity;
        world *= rotation;
        world *= scale;
        world *= translation;


        for (int i = 0; i < verts; i++)
        {
            vecTransformedCoordinates[i] = Vector2.Transform(vecModelCoordinates[i], world);
        }


        // Draw Closed Polygon
        for (int i = 0; i < verts + 1; i++)
        {
            int j = (i + 1);

            DrawLine((int)vecTransformedCoordinates[i % verts].X, (int)vecTransformedCoordinates[i % verts].Y,
                (int)vecTransformedCoordinates[j % verts].X, (int)vecTransformedCoordinates[j % verts].Y, col);
        }
    }

    public void Clear(Color color)
    {
        for (int i = 0; i < this._pixelData.Length; i++) this._pixelData[i] = color;
    }

    public void DrawPixels()
    {
        this._pixel.SetData(this._pixelData);

        this._root.GraphicsDevice.SetRenderTarget(this._renderTarget);

        this._spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap);
        this._spriteBatch.Draw(this._pixel, Vector2.Zero, Color.White);
        this._spriteBatch.End();

        _root.GraphicsDevice.SetRenderTarget(null);

        _spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointWrap,
            null, null, null, _scale);

        _spriteBatch.Draw(
            _renderTarget,
            new Rectangle(0, 0, this._root.GraphicsDevice.Viewport.Width, this._root.GraphicsDevice.Viewport.Height),
            Color.White);
        _spriteBatch.End();
    }
}