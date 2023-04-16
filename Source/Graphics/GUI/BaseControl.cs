namespace Asteroids2.Source.Graphics.GUI;

// Virtual base class for all controls
public abstract class BaseControl
{
    public BaseControl(Manager manager)
    {
        m_manager = manager;

        manager.AddControl(this);
    }

    public bool bVisible = true; // Sets whether or not the control is interactive/displayed

    public bool bPressed = false; // True on single frame control begins being manipulated
    public bool bHeld = false; // True on all frames control is under user manipulation
    public bool bReleased = false; // True on single frame control ceases being manipulated

    // Switches the control on/off
    public void Enable(bool bEnable)
    {
        bVisible = bEnable;
    }

    // Updates the control's behavior
    public abstract void Update(PixelRenderer pr, TextRenderer tr);

    // Draws the control using "sprite" based CPU operations
    public abstract void Draw(PixelRenderer pr, TextRenderer tr);

    protected Manager m_manager; // Controls are related to a manager, where the theme resides and control groups can be implemented
    protected State m_state = State.Normal; // All controls exist in one of four states
    protected float m_fTransition = 0.0f; // To add a "swish" to things, controls can fade between states

    protected enum State { Disabled, Normal, Hover, Click } // Disabled - Greyed out and not interactive, Normal - interactive and operational, Hover - currently under the user's mouse focus, Click - user is interacting with the control
}
