using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Asteroids2.Source.Graphics.GUI;

// A Manager acts as a convenient grouping of controls
public class Manager
{
    public Manager() { }

    public void AddControl(BaseControl control)
    {
        // Add a gui element derived from BaseControl to this manager
        m_vControls.Add(control);
    }

    public void Update(PixelRenderer pr, TextRenderer tr)
    {
        // Update all controls that this manager operates
        foreach (BaseControl control in m_vControls) control.Update(pr, tr);
    }

    public void Draw(PixelRenderer pr, TextRenderer tr)
    {
        // Draw as "sprite" all controls that this manager operates
        foreach (BaseControl control in m_vControls) control.Draw(pr, tr);
    }

    // Theme attributes
    public Color colNormal = Color.DarkBlue;
    public Color colHover = Color.Blue;
    public Color colClick = Color.Cyan;
    public Color colDisable = Color.DarkGray;
    public Color colBorder = Color.White;
    public Color colText = Color.White;
    public float fHoverSpeedOn = 10.0f;
    public float fHoverSpeedOff = 4.0f;
    public float fGrabRad = 8.0f;

    public void CopyThemeFrom(Manager manager)
    {
        // Copy all theme attributes from a different manager object
        colNormal = manager.colNormal;
        colHover = manager.colHover;
        colClick = manager.colClick;
        colDisable = manager.colDisable;
        colBorder = manager.colBorder;
        colText = manager.colText;
        fHoverSpeedOn = manager.fHoverSpeedOn;
        fHoverSpeedOff = manager.fHoverSpeedOff;
        fGrabRad = manager.fGrabRad;
    }

    private List<BaseControl> m_vControls = new List<BaseControl>(); // Container
}