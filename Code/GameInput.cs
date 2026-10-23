using Godot;
using System.Collections.Generic;

namespace BoulderDash
{
    // Keyboard and mouse input.
    public partial class Game
    {
        public override void _UnhandledKeyInput(InputEvent @event)
        {
            if (@event is InputEventKey key && key.Pressed && !key.Echo)
            {
                if (_state == State.Story && key.Keycode != Key.F11)
                {
                    if (key.Keycode == Key.Escape)
                    {
                        FinishStory();
                    }
                    else if (key.Keycode == Key.Space || key.Keycode == Key.Enter || key.Keycode == Key.KpEnter)
                    {
                        AdvanceStory();
                    }
                    return;
                }

                if (key.Keycode == Key.R)
                {
                    if (_state == State.AllClear)
                    {
                        _score = 0;
                        LoadLevel(0);
                    }
                    else
                    {
                        LoadLevel(_levelIndex);
                    }
                }
                else if (key.Keycode == Key.Space && _state == State.Playing)
                {
                    UseStatue();
                }
                else if (key.Keycode == Key.E && _state == State.Playing)
                {
                    StartBurst();
                }
                else if (key.Keycode == Key.H)
                {
                    _hintHidden = !_hintHidden;
                }
                else if (key.Keycode == Key.F11)
                {
                    bool full = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;
                    DisplayServer.WindowSetMode(full ? DisplayServer.WindowMode.Windowed : DisplayServer.WindowMode.Fullscreen);
                }
                else if (key.Keycode == Key.M || key.Keycode == Key.Tab)
                {
                    _showFullMap = !_showFullMap;
                    QueueRedraw();
                }
                else if ((key.Keycode == Key.Enter || key.Keycode == Key.Space) && _state == State.LevelComplete)
                {
                    LoadLevel(_levelIndex + 1);
                }
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mb && mb.Pressed)
            {
                if (_state == State.Story)
                {
                    if (mb.ButtonIndex == MouseButton.Left)
                    {
                        AdvanceStory();
                    }
                    return;
                }
                if (mb.ButtonIndex == MouseButton.Left)
                {
                    _hintHidden = true;
                }
                else if (mb.ButtonIndex == MouseButton.Right && _state == State.Playing)
                {
                    StartBurst();
                }
            }
        }

        private Vector2I ReadDirection()
        {
            if (Input.IsKeyPressed(Key.Left) || Input.IsKeyPressed(Key.A)) return Vector2I.Left;
            if (Input.IsKeyPressed(Key.Right) || Input.IsKeyPressed(Key.D)) return Vector2I.Right;
            if (Input.IsKeyPressed(Key.Up) || Input.IsKeyPressed(Key.W)) return Vector2I.Up;
            if (Input.IsKeyPressed(Key.Down) || Input.IsKeyPressed(Key.S)) return Vector2I.Down;
            return Vector2I.Zero;
        }

    }
}
