using Godot;
using System.Collections.Generic;

namespace BoulderDash
{
    // The per frame loop, the story intro and the end screen text.
    public partial class Game
    {
        public override void _Process(double delta)
        {
            float dt = (float)delta;

            UpdateVisuals(dt);

            if (_state != State.Playing)
            {
                return;
            }

            _levelClock += dt;
            _idle += dt;
            _timeLeft -= dt;
            if (_timeLeft <= 0)
            {
                _timeLeft = 0;
                _state = State.Dead;
            }

            if (_flipTimer > 0)
            {
                _flipTimer -= dt;
                if (_flipTimer <= 0)
                {
                    if (_pending == Power.Flip)
                    {
                        DoFlip();
                    }
                    else
                    {
                        DoBurst();
                    }
                }
            }

            _playerTimer += dt;
            if (_playerTimer >= PlayerStepTime && _flipTimer <= 0)
            {
                Vector2I dir = ReadDirection();
                if (dir != Vector2I.Zero)
                {
                    Vector2I before = _player;
                    TryMovePlayer(dir);
                    if (_player != before)
                    {
                        _idle = 0;
                    }
                    _playerTimer = 0;
                }
            }

            _physicsTimer += dt;
            if (_physicsTimer >= PhysicsStepTime)
            {
                _physicsTimer = 0;
                PhysicsStep();
            }

            if (_driftSeconds > 0)
            {
                _driftTimer += dt;
                if (_driftTimer >= _driftSeconds)
                {
                    _driftTimer = 0;
                    DriftSoul();
                }
            }

            _enemyTimer += dt;
            if (_enemyTimer >= EnemyStepTime)
            {
                _enemyTimer = 0;
                EnemyStep();
            }
        }

        private void AdvanceStory()
        {
            string text = StoryPages[_storyPage][1];
            if (_storyChars < text.Length)
            {
                _storyChars = text.Length;   // first press finishes the line
                return;
            }
            _storyPage++;
            _storyChars = 0;
            if (_storyPage >= StoryPages.Length)
            {
                FinishStory();
            }
        }

        private void FinishStory()
        {
            _state = State.Playing;
            _levelClock = 0;
        }

        private void PickEndLine()
        {
            switch (_state)
            {
                case State.Dead:
                    _endLine = _timeLeft <= 0f ? "Time ran out. The gates stay shut." : DeathLines[_rng.RandiRange(0, DeathLines.Length - 1)];
                    break;
                case State.LevelComplete:
                    _endLine = "A soul gate opens. Heaven draws nearer.";
                    break;
                case State.AllClear:
                    _endLine = "The last soul is free. Your penance is paid.";
                    break;
            }
        }

        // Slides the drawn positions toward the real grid positions so movement looks smooth, and animates the background embers. Redraws every frame.
        private void UpdateVisuals(float dt)
        {
            _time += dt;

            if (_state == State.Story)
            {
                _storyChars += dt * 40f;
            }
            if (_state == State.Playing || _state == State.Story)
            {
                _stateTime = 0f;
            }
            else
            {
                if (_stateTime <= 0f)
                {
                    PickEndLine();
                }
                _stateTime += dt;
            }
            _shake = Mathf.MoveToward(_shake, 0f, dt * 1.5f);

            Vector2 target = new Vector2(_player.X, _player.Y);
            if (_playerVisual != target)
            {
                // A bit faster than the step time so the glide finishes before the next step.
                _playerVisual = _playerVisual.MoveToward(target, dt * 1.3f / PlayerStepTime);
            }

            _heaven = Mathf.MoveToward(_heaven, _gravity < 0 ? 1f : 0f, dt * 2f);
            _flash = Mathf.MoveToward(_flash, 0f, dt * 2.5f);
            if (_toastTime > 0f)
            {
                _toastTime -= dt;
            }

            foreach (DriftFx fx in _fx)
            {
                fx.Age += dt;
            }
            _fx.RemoveAll(f => f.Age > 0.8f);

            foreach (Enemy e in _enemies)
            {
                Vector2 et = new Vector2(e.X, e.Y);
                if (e.Visual != et)
                {
                    e.Visual = e.Visual.MoveToward(et, dt * 1.3f / EnemyStepTime);
                }
            }

            float slide = dt / PhysicsStepTime;
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (_bushShake[x, y] > 0f)
                    {
                        _bushShake[x, y] -= dt;
                    }
                    if (_offset[x, y] != Vector2.Zero)
                    {
                        _offset[x, y] = _offset[x, y].MoveToward(Vector2.Zero, slide);
                    }
                }
            }

            for (int i = 0; i < EmberCount; i++)
            {
                _embers[i].Y -= _emberSpeed[i] * dt;
                _embers[i].X += Mathf.Sin(_time * 2f + i) * 10f * dt;
                if (_embers[i].Y < HudHeight)
                {
                    _embers[i] = new Vector2(_rng.RandfRange(0, ViewW), HudHeight + ViewH);
                    _emberSpeed[i] = _rng.RandfRange(15f, 55f);
                }
            }

            QueueRedraw();
        }

    }
}
