using Godot;
using System.Collections.Generic;

namespace BoulderDash
{
    // Player movement and pushing rules.
    public partial class Game
    {
        private bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < _width && y < _height;
        }

        private void TryMovePlayer(Vector2I dir)
        {
            Vector2I target = _player + dir;
            if (!InBounds(target.X, target.Y))
            {
                return;
            }

            switch (_grid[target.X, target.Y])
            {
                case Tile.Empty:
                    _player = target;
                    break;

                case Tile.Dirt:
                    _grid[target.X, target.Y] = Tile.Empty;
                    _player = target;
                    break;

                case Tile.Bush:
                    _player = target;   // the sheep walks through bushes without destroying them
                    break;

                case Tile.Diamond:
                    _grid[target.X, target.Y] = Tile.Empty;
                    _falling[target.X, target.Y] = false;
                    _diamonds++;
                    _score += 10;
                    _player = target;
                    break;

                case Tile.Boulder:
                    // Boulders can only be pushed sideways into an empty tile.
                    if (dir.Y == 0)
                    {
                        Vector2I beyond = target + dir;
                        if (InBounds(beyond.X, beyond.Y) && _grid[beyond.X, beyond.Y] == Tile.Empty)
                        {
                            _grid[beyond.X, beyond.Y] = Tile.Boulder;
                            _grid[target.X, target.Y] = Tile.Empty;
                            _falling[target.X, target.Y] = false;
                            _offset[beyond.X, beyond.Y] = new Vector2(-dir.X, -dir.Y);
                            _offset[target.X, target.Y] = Vector2.Zero;
                            _player = target;
                        }
                    }
                    break;

                case Tile.Key:
                    _grid[target.X, target.Y] = Tile.Empty;
                    _keys++;
                    _score += 5;
                    _player = target;
                    break;

                case Tile.Door:
                    if (_keys > 0)
                    {
                        _keys--;
                        _grid[target.X, target.Y] = Tile.Empty;
                        _player = target;
                    }
                    break;

                case Tile.Exit:
                    if (GateOpen())
                    {
                        _player = target;
                        _score += Mathf.CeilToInt(_timeLeft);
                        _state = _levelIndex + 1 < Levels.All.Length ? State.LevelComplete : State.AllClear;
                    }
                    break;
            }

            if (_state == State.Playing && EnemyAt(_player.X, _player.Y) != null)
            {
                _state = State.Dead;
            }
        }

    }
}
