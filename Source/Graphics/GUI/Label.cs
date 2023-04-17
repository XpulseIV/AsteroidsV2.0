using Microsoft.Xna.Framework;

namespace Asteroids2.Source.Graphics.GUI;

// Label Control - represents a simple text label
internal class Label : BaseControl
{
    // Position of label
    public Vector2 vPos;

    // Size of label
    public Vector2 vSize;

    // Text displayed on label
    public string sText;

    // Show a border?
    public bool bHasBorder = false;

    // Show a background?
    public bool bHasBackground = false;

    // Text alignment
    public enum Alignment { Left, Centre, Right };
    public Alignment nAlign = Alignment.Centre;

    // Constructor
    public Label(Manager manager, string text, Vector2 pos, Vector2 size) : base(manager)
    {
        // Associate with a Manager
        m_manager = manager;

        // Set properties
        vPos = pos;
        vSize = size;
        sText = text;
    }

    // Override Update method
    public override void Update(PixelRenderer pr, TextRenderer tr) { }

    public override void Draw(PixelRenderer pr, TextRenderer tr)
    {
        if (!bVisible) return;

        if (bHasBackground) pr.FillRect(vPos + new Vector2(1, 1), vSize - new Vector2(2, 2), m_manager.colNormal);

        if (bHasBorder) pr.DrawRect(vPos, vSize - new Vector2(1, 1), m_manager.colBorder);

        Vector2 vText = tr.GetTextSizeProp(sText);

        switch (nAlign)
        {
        case Alignment.Left:
            tr.DrawStringProp
                (new Vector2(vPos.X + 2.0f, vPos.Y + (vSize.Y - vText.Y) * 0.5f), sText, m_manager.colText, 1);

            break;
        case Alignment.Centre:
            tr.DrawStringProp(vPos + (vSize - vText) * 0.5f, sText, m_manager.colText, 1);

            break;
        case Alignment.Right:
            tr.DrawStringProp
            (
                new Vector2(vPos.X + vSize.X - vText.X - 2.0f, vPos.Y + (vSize.Y - vText.Y) * 0.5f), sText,
                m_manager.colText, 1
            );

            break;
        }
    }
}