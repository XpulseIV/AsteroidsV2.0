using System;
using System.Collections.Generic;
using System.Linq;
using AstralAssault.Source.Graphics;
using AstralAssault.Source.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AstralAssault.Source.Game.GameState
{
    public class GameOverState : GameState, IKeyboardPressedEventListener
    {
        private readonly Boolean _newHighScore;
        private readonly Int64 _timeEntered;
        private Texture2D _gameOverText;
        private Texture2D _restartPrompt;
        private Boolean _showNewHighScore;
        private Int64 _lastToggle;

        public GameOverState(Game1 root) : base(root) {
            InputEventSource.KeyboardPressedEvent += this.OnKeyboardPressedEvent;
            this._timeEntered = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            if (this.Root.Score <= this.Root.HighScore) return;

            this.Root.HighScore = this.Root.Score;
            this._newHighScore = true;
        }

        public override void Enter() {
            this._gameOverText = AssetManager.Load<Texture2D>("GameOver");
            this._restartPrompt = AssetManager.Load<Texture2D>("Restart");
        }

        public override void Exit() {
            InputEventSource.KeyboardPressedEvent -= this.OnKeyboardPressedEvent;
        }

        public override List<DrawTask> GetDrawTasks() {
            Vector2 textPosition = new(
                (Single)Math.Round(Game1.GameWidth / 2d),
                (Single)Math.Round(Game1.GameHeight / 3d));

            Vector2 promptPosition = new(
                (Single)Math.Round(Game1.GameWidth / 2D),
                (Single)Math.Round(Game1.GameHeight / 2D));

            List<DrawTask> drawTasks = new();

            DrawTask gameOverText = new(this._gameOverText,
                textPosition,
                0,
                LayerDepth.HUD,
                new List<IDrawTaskEffect>());

            drawTasks.Add(gameOverText);

            Int64 timeNow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            if (timeNow - this._timeEntered > 1000) {
                DrawTask restartPrompt = new(this._restartPrompt,
                    promptPosition,
                    0,
                    LayerDepth.HUD,
                    new List<IDrawTaskEffect>());

                drawTasks.Add(restartPrompt);
            }

            Int32 score = (Int32)this.Lerp(0, this.Root.Score, MathF.Min((timeNow - this._timeEntered) / 800F, 1));
            String scoreText = $"Score: {score}";
            Int32 textX = 240 - $"Score: {this.Root.Score}".Length * 4;
            List<DrawTask> scoreTasks = scoreText.CreateDrawTasks(new Vector2(textX, 150), Color.White, LayerDepth.HUD);
            drawTasks.AddRange(scoreTasks);

            if (!this._newHighScore) {
                String highScoreText = $"High score: {this.Root.HighScore}";
                Int32 highScoreX = 240 - highScoreText.Length * 4;
                List<DrawTask> highScoreTasks =
                    highScoreText.CreateDrawTasks(new Vector2(highScoreX, 170), Color.White, LayerDepth.HUD);
                drawTasks.AddRange(highScoreTasks);

                return drawTasks;
            }

            Int64 timeSinceToggle = timeNow - this._lastToggle;
            if (timeSinceToggle > 500) {
                this._showNewHighScore = !this._showNewHighScore;
                this._lastToggle = timeNow;
            }

            if (!this._showNewHighScore) return drawTasks;

            String newHighScoreText = "New high score!";
            Int32 newHighScoreX = 240 - newHighScoreText.Length * 4;
            List<DrawTask> newHighScoreTasks =
                newHighScoreText.CreateDrawTasks(new Vector2(newHighScoreX, 170), Color.White, LayerDepth.HUD);
            drawTasks.AddRange(newHighScoreTasks);

            return drawTasks;
        }

        public override void Update(Single deltaTime) { }

        public void OnKeyboardPressedEvent(Object sender, KeyboardEventArgs e) {
            if (e.Keys.Contains(Keys.F)) return;

            Int64 timeNow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if (timeNow - this._timeEntered < 1000) {
                return;
            }

            Jukebox.PlaySound("RestartGame");
            this.Root.GameStateMachine.ChangeState(new GameplayState(this.Root));
        }

        private Single Lerp(Single firstFloat, Single secondFloat, Single by) {
            return firstFloat * (1 - by) + secondFloat * by;
        }
    }
}