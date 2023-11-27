using System.Collections.Generic;
using AstralAssault.Source.Graphics;

namespace AstralAssault.Source.Game.GameState
{
    public class GameStateMachine
    {
        public GameState _currentState;

        public GameStateMachine(GameState initialState) {
            this._currentState = initialState;
            this._currentState.Enter();
        }

        public void Update(float deltaTime) {
            this._currentState.Update(deltaTime);
        }

        public List<DrawTask> GetDrawTasks() {
            return this._currentState.GetDrawTasks();
        }

        public void ChangeState(GameState newState) {
            this._currentState?.Exit();
            this._currentState = newState;
            this._currentState.Enter();
        }
    }
}