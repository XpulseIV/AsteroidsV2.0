using System;
using System.Collections.Generic;
using Asteroids2.Source.Game;
using Asteroids2.Source.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Asteroids2.Source.Graphics;

public sealed class TextRenderer
{
    private Game1 m_root;

    private Color[] m_pixelFont;
    private Texture2D m_fontThing;

    private List<Vector2> m_fontSpacing;

    private const int TabSizeInSpaces = 4;

    public void Init(Game1 root)
    {
        m_root = root;

        string data = "";
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

        m_pixelFont = new Color[128 * 48];

        int px = 0, py = 0;

        for (int b = 0; b < 1024; b += 4)
        {
            int sym1 = data[b + 0] - 48;
            int sym2 = data[b + 1] - 48;
            int sym3 = data[b + 2] - 48;
            int sym4 = data[b + 3] - 48;
            int r = (sym1 << 18) | (sym2 << 12) | (sym3 << 6) | sym4;

            for (int i = 0; i < 24; i++)
            {
                int k = (r & (1 << i)) != 0 ? 255 : 0;
                m_pixelFont[ExtensionMethods.twoDToOneD((px, py, 128))] = new Color(k, k, k, k);

                if (++py == 48)
                {
                    px++;
                    py = 0;
                }
            }
        }

        byte[] spacing =
        {
            0x03, 0x25, 0x16, 0x08, 0x07, 0x08, 0x08, 0x04, 0x15, 0x15, 0x08, 0x07, 0x15, 0x07, 0x24, 0x08,
            0x08, 0x17, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x24, 0x15, 0x06, 0x07, 0x16, 0x17,
            0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x17, 0x08, 0x08, 0x17, 0x08, 0x08, 0x08,
            0x08, 0x08, 0x08, 0x08, 0x17, 0x08, 0x08, 0x08, 0x08, 0x17, 0x08, 0x15, 0x08, 0x15, 0x08, 0x08,
            0x24, 0x18, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x33, 0x17, 0x17, 0x33, 0x18, 0x17, 0x17,
            0x17, 0x17, 0x17, 0x17, 0x07, 0x17, 0x17, 0x18, 0x18, 0x17, 0x17, 0x07, 0x33, 0x07, 0x08, 0x00
        };

        m_fontSpacing = new List<Vector2>();

        foreach (byte c in spacing) m_fontSpacing.Add(new Vector2(c >> 4, c & 15));
    }

    public Vector2 GetTextSize(string s)
    {
        Vector2 size = new Vector2(0, 1);
        Vector2 pos = new Vector2(0, 1);

        foreach (char c in s)
        {
            switch (c)
            {
            case '\n':
                pos.Y++;
                pos.X = 0;

                break;
            case '\t':
                pos.X += TabSizeInSpaces;

                break;
            default:
                pos.X++;

                break;
            }

            size.X = MathF.Max(size.X, pos.X);
            size.Y = MathF.Max(size.Y, pos.Y);
        }

        return size * 8;
    }

    public void DrawString(Vector2 pos, string sText, Color col, int scale)
    {
        DrawString((int)pos.X, (int)pos.Y, sText, col, scale);
    }

    public void DrawString(int x, int y, string sText, Color col, int scale)
    {
        int sx = 0;
        int sy = 0;

        foreach (char c in sText)
        {
            if (c == '\n')
            {
                sx = 0;
                sy += 8 * scale;
            }
            else if (c == '\t') sx += 8 * TabSizeInSpaces * scale;
            else
            {
                int ox = (c - 32) % 16;
                int oy = (c - 32) / 16;

                if (scale > 1)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (m_pixelFont[ExtensionMethods.twoDToOneD((i + ox * 8, j + oy * 8, 128))].R > 0)
                            {
                                for (int iss = 0; iss < scale; iss++)
                                {
                                    for (int js = 0; js < scale; js++)
                                        m_root.PixelRenderer.DrawPixel
                                            (x + sx + i * scale + iss, y + sy + j * scale + js, col);
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (m_pixelFont[ExtensionMethods.twoDToOneD((i + ox * 8, j + oy * 8, 128))].R > 0)
                                m_root.PixelRenderer.DrawPixel(x + sx + i, y + sy + j, col);
                        }
                    }
                }

                sx += 8 * scale;
            }
        }
    }

    public Vector2 GetTextSizeProp(string s)
    {
        Vector2 size = new Vector2(0, 1);
        Vector2 pos = new Vector2(0, 1);

        foreach (char c in s)
        {
            switch (c)
            {
            case '\n':
                pos.Y += 1;
                pos.X = 0;

                break;
            case '\t':
                pos.X += TabSizeInSpaces * 8;

                break;
            default:
                pos.X += m_fontSpacing[c - 32].Y;

                break;
            }

            size.X = MathF.Max(size.X, pos.X);
            size.Y = MathF.Max(size.Y, pos.Y);
        }

        size.Y *= 8;

        return size;
    }

    public void DrawStringProp(Vector2 pos, string sText, Color col, int scale)
    {
        DrawStringProp((int)pos.X, (int)pos.Y, sText, col, scale);
    }

    public void DrawStringProp(int x, int y, string sText, Color col, int scale)
    {
        int sx = 0;
        int sy = 0;

        foreach (char c in sText)
        {
            if (c == '\n')
            {
                sx = 0;
                sy += 8 * scale;
            }
            else if (c == '\t') sx += 8 * TabSizeInSpaces * scale;
            else
            {
                int ox = (c - 32) % 16;
                int oy = (c - 32) / 16;

                if (scale > 1)
                {
                    for (int i = 0; i < m_fontSpacing[c - 32].Y; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (m_pixelFont[
                                    ExtensionMethods.twoDToOneD
                                        (((int)(i + ox * 8 + m_fontSpacing[c - 32].X), j + oy * 8, 128))].R > 0)
                            {
                                for (int iss = 0; iss < scale; iss++)
                                {
                                    for (int js = 0; js < scale; js++)
                                        m_root.PixelRenderer.DrawPixel
                                            (x + sx + i * scale + iss, y + sy + j * scale + js, col);
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < m_fontSpacing[c - 32].Y; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (m_pixelFont[
                                    ExtensionMethods.twoDToOneD
                                        (((int)(i + ox * 8 + m_fontSpacing[c - 32].X), j + oy * 8, 128))].R > 0)
                                m_root.PixelRenderer.DrawPixel(x + sx + i, y + sy + j, col);
                        }
                    }
                }

                sx += (int)m_fontSpacing[c - 32].Y * scale;
            }
        }
    }
}