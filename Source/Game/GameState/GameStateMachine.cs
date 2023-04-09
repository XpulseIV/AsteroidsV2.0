using System.Collections.Generic;
using Asteroids2.Source.Graphics;

namespace Asteroids2.Source.Game.GameState;

public class GameStateMachine
{
    private GameState m_currentState;

    public GameStateMachine(GameState initialState)
    {
        m_currentState = initialState;
        m_currentState.Enter();
    }

    public void ChangeState(GameState newState)
    {
        m_currentState?.Exit();
        m_currentState = newState;
        m_currentState.Enter();
    }

    public void Draw() => m_currentState.Draw();
}