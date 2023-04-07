using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AsteroidsV2._0;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

internal sealed class Renderer
{
    private GraphicsDevice _graphicsDevice;
    private readonly Texture2D _screenOfPixels;
    private readonly Color[] _pixelData;
    private readonly SpriteBatch _spriteBatch;

    private readonly Game1 _root;

    private RenderTarget2D _renderTarget;

    public const int RenderWidth = (int)Width.Quarter;
    public const int RenderHeight = (int)Height.Quarter;
    private Matrix _scale;
    private float _scaleX;
    private float _scaleY;

    private Color[] _pixelFont;
    private Texture2D fontThing;
    private readonly List<Rectangle> _letters = new();

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

    public Renderer(GraphicsDevice graphicsDevice, Game1 root)
    {
        this._root = root;

        this._graphicsDevice = graphicsDevice;

        this._pixelData = new Color[Renderer.RenderWidth * Renderer.RenderHeight];

        this._screenOfPixels = new(graphicsDevice, Renderer.RenderWidth, Renderer.RenderHeight);

        this.SetupWindow();
        this.SetupFont();

        this._spriteBatch = new(graphicsDevice);
    }

    private void SetupFont()
    {
        /*
        string data = string.Empty;
        data += "?Q`0001oOch0o01o@F40o0<AGD4090LAGD<090@A7ch0?00O7Q`0600>00000000";
        data += "O000000nOT0063Qo4d8>?7a14Gno94AA4gno94AaOT0>o3`oO400o7QN00000400";
        data += "Of80001oOg<7O7moBGT7O7lABET024@aBEd714AiOdl717a_=TH013Q>00000000";
        data += "720D000V?V5oB3Q_HdUoE7a9@DdDE4A9@DmoE4A;Hg]oM4Aj8S4D84@`00000000";
        data += "OaPT1000Oa`^13P1@AI[?g`1@A=[OdAoHgljA4Ao?WlBA7l1710007l100000000";
        data += "ObM6000oOfMV?3QoBDD`O7a0BDDH@5A0BDD<@5A0BGeVO5ao@CQR?5Po00000000";
        data += "Oc``000?Ogij70PO2D]??0Ph2DUM@7i`2DTg@7lh2GUj?0TO0C1870T?00000000";
        data += "70<4001o?P<7?1QoHg43O;`h@GT0@:@LB@d0>:@hN@L0@?aoN@<0O7ao0000?000";
        data += "OcH0001SOglLA7mg24TnK7ln24US>0PL24U140PnOgl0>7QgOcH0K71S0000A000";
        data += "00H00000@Dm1S007@DUSg00?OdTnH7YhOfTL<7Yh@Cl0700?@Ah0300700000000";
        data += "<008001QL00ZA41a@6HnI<1i@FHLM81M@@0LG81?O`0nC?Y7?`0ZA7Y300080000";
        data += "O`082000Oh0827mo6>Hn?Wmo?6HnMb11MP08@C11H`08@FP0@@0004@000000000";
        data += "00P00001Oab00003OcKP0006@6=PMgl<@440MglH@000000`@000001P00000000";
        data += "Ob@8@@00Ob@8@Ga13R@8Mga172@8?PAo3R@827QoOb@820@0O`0007`0000007P0";
        data += "O`000P08Od400g`<3V=P0G`673IP0`@3>1`00P@6O`P00g`<O`000GP800000000";
        data += "?P9PL020O`<`N3R0@E4HC7b0@ET<ATB0@@l6C4B0O`H3N7b0?P01L3R000000020";
        */

        this._pixelFont = new Color[177 * 782];
        fontThing = _root.Content.Load<Texture2D>("aseprite_font");
        fontThing.GetData(_pixelFont);

        for (int y = 1; y < 782; y += 11)
        {
            for (int x = 1; x < 176; x += 11)
            {
                int index = (y * 177) + x;
                int len = 0;

                while (_pixelFont[index] != new Color(255, 255, 255, 255))
                {
                    len++;
                    index++;
                }

                Rectangle charBounds = new(x, y, len, 7);

                this._letters.Add(charBounds);
            }
        }
        /*
        int px = 0, py = 0;
        for (int b = 0; b < 1024; b += 4)
        {
            uint sym1 = (uint)data[b + 0] - 48;
            uint sym2 = (uint)data[b + 1] - 48;
            uint sym3 = (uint)data[b + 2] - 48;
            uint sym4 = (uint)data[b + 3] - 48;
            uint r = sym1 << 18 | sym2 << 12 | sym3 << 6 | sym4;

            for (int i = 0; i < 24; i++)
            {
                int k = (r & (1 << i)) != 0 ? 255 : 0;
                this._pixelFont[py * 128 + px] = new(k, k, k, k);
                if (++py == 48)
                {
                    px++;
                    py = 0;
                }
            }
        }
        */
    }

    private void SetupWindow()
    {
        this._root.Window.Title = "Asteroids but better";

        this._root.Graphics.PreferredBackBufferWidth = (int)Width.Half;
        this._root.Graphics.PreferredBackBufferHeight = (int)Height.Half;
        this._root.Graphics.ApplyChanges();

        this._scaleX = this._root.Graphics.PreferredBackBufferWidth / (float)Renderer.RenderWidth;
        this._scaleY = this._root.Graphics.PreferredBackBufferHeight / (float)Renderer.RenderHeight;
        this._scale = Matrix.CreateScale(new Vector3(this._scaleX, this._scaleY, 1));

        this._root.Graphics.SynchronizeWithVerticalRetrace = true;
        this._root.IsFixedTimeStep = true;

        this._renderTarget = new(
            this._root.GraphicsDevice,
            this._root.GraphicsDevice.PresentationParameters.BackBufferWidth,
            this._root.GraphicsDevice.PresentationParameters.BackBufferHeight,
            false,
            this._root.GraphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24);
    }

    public void DrawString(int x, int y, string sText, Color col, int scale)
    {
        int sx = 0;
        int sy = 0;

        foreach (char c in sText)
        {
            switch (c)
            {
                case '\n':
                    sx = 0;
                    sy += 7 * scale;

                    break;

                case '\t':
                    sx += 8 * 4 * scale;

                    break;

                default:
                {
                    int indexInLetters = (c - 32);

                    Rectangle letterBoundingBox = this._letters[indexInLetters];

                    var charColorArray = new Color[letterBoundingBox.Width * letterBoundingBox.Height];

                    for (int charY = 0; charY < letterBoundingBox.Height; charY++)
                    {
                        for (int charX = 0; charX < letterBoundingBox.Width; charX++)
                        {
                            charColorArray[charY * letterBoundingBox.Width + charX] = this._pixelFont[
                                (letterBoundingBox.Y + charY) * 177 + (letterBoundingBox.X + charX)];
                        }
                    }

                    if (scale > 1)
                    {
                    }
                    else
                    {
                        for (int yOffset = 0; yOffset < letterBoundingBox.Height; yOffset++)
                            for (int xOffset = 0; xOffset < letterBoundingBox.Width; xOffset++)
                                if (charColorArray[yOffset * letterBoundingBox.Width + xOffset] == new Color(0, 0, 0, 255))
                                    this.DrawPixel(x + sx + xOffset, y + sy + yOffset, col);
                    }
                    sx += letterBoundingBox.Width * scale;

                    break;
                }
            }
        }
    }

    public void DrawWireFrameModel(List<Vector2> vecModelCoordinates, float x, float y, float r, float s, Color col)
    {
        // Create translated model vector of coordinate pairs
        var vecTransformedCoordinates = new List<Vector2>();
        int verts = vecModelCoordinates.Count;
        vecTransformedCoordinates.AddRange(vecModelCoordinates);

        Matrix translation = Matrix.CreateTranslation(x, y, 0);
        Matrix rotation = Matrix.CreateRotationZ(r);
        Matrix scale = Matrix.CreateScale(s);

        Matrix world = Matrix.Identity;
        world *= rotation;
        world *= scale;
        world *= translation;


        for (int i = 0; i < verts; i++) vecTransformedCoordinates[i] = Vector2.Transform(vecModelCoordinates[i], world);


        // Draw Closed Polygon
        for (int i = 0; i < verts + 1; i++)
        {
            int j = (i + 1);

            this.DrawLine((int)vecTransformedCoordinates[i % verts].X, (int)vecTransformedCoordinates[i % verts].Y,
                (int)vecTransformedCoordinates[j % verts].X, (int)vecTransformedCoordinates[j % verts].Y, col);
        }
    }

    public void DrawLine(int x1, int y1, int x2, int y2, Color c)
    {
        int x, y, dx, dy, dx1, dy1, px, py, xe, ye, i;
        dx = x2 - x1; dy = y2 - y1;
        dx1 = Math.Abs(dx); dy1 = Math.Abs(dy);
        px = 2 * dy1 - dx1; py = 2 * dx1 - dy1;
        if (dy1 <= dx1)
        {
            if (dx >= 0)
            {
                x = x1;
                y = y1;
                xe = x2;
            }
            else
            {
                x = x2;
                y = y2;
                xe = x1;
            }

            this.DrawPixel(x, y, c);

            for (i = 0; x < xe; i++)
            {
                x = x + 1;
                if (px < 0)
                    px = px + 2 * dy1;
                else
                {
                    if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) y = y + 1;
                    else y = y - 1;
                    px = px + 2 * (dy1 - dx1);
                }

                this.DrawPixel(x, y, c);
            }
        }
        else
        {
            if (dy >= 0)
            {
                x = x1;
                y = y1;
                ye = y2;
            }
            else
            {
                x = x2;
                y = y2;
                ye = y1;
            }

            this.DrawPixel(x, y, c);

            for (i = 0; y < ye; i++)
            {
                y = y + 1;
                if (py <= 0)
                    py = py + 2 * dx1;
                else
                {
                    if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) x = x + 1;
                    else x = x - 1;
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

    public void DrawPixel(int x, int y, Color color)
    {
        Vector2 wrapedCoords = ExtensionClass.Wrap(new(x, y), Renderer.RenderWidth, Renderer.RenderHeight);

        x = (int)wrapedCoords.X;
        y = (int)wrapedCoords.Y;

        int index = y * this._screenOfPixels.Width + x;
        this._pixelData[index] = color;
    }

    public void DrawPixel(Vector2 pos, Color color)
    {
        this.DrawPixel((int)pos.X, (int)pos.Y, color);
    }

    public void PixelPass()
    {
        this._screenOfPixels.SetData(this._pixelData);

        this._root.GraphicsDevice.SetRenderTarget(this._renderTarget);

        this._spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap);
        this._spriteBatch.Draw(this._screenOfPixels, Vector2.Zero, Color.White);
        this._spriteBatch.End();

        this._root.GraphicsDevice.SetRenderTarget(null);

        this._spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointWrap,
            null, null, null, this._scale);

        this._spriteBatch.Draw(this._renderTarget,
            new Rectangle(0, 0, this._root.GraphicsDevice.Viewport.Width, this._root.GraphicsDevice.Viewport.Height),
            Color.White);
        this._spriteBatch.End();
    }

    public void Clear(Color color)
    {
        for (int i = 0; i < this._pixelData.Length; i++) this._pixelData[i] = color;
    }
}