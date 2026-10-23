using Godot;
using System.Collections.Generic;

namespace BoulderDash
{
    // Gate rules, drifting souls and skeleton movement.
    public partial class Game
    {

        // The gate opens when enough souls are collected and enough skeletons are defeated.
        private bool GateOpen()
        {
            return _diamonds >= _needed && _kills >= _killsNeeded;
        }

        // Every few seconds one soul that is far from the player vanishes and respawns buried in the dirt somewhere else, so waiting is never a safe plan. The total number of souls stays the same.
        private void DriftSoul()
        {
            List<Vector2I> souls = new List<Vector2I>();
            List<Vector2I> spots = new List<Vector2I>();

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int dist = Mathf.Abs(x - _player.X) + Mathf.Abs(y - _player.Y);
                    if (dist < 4)
                    {
                        continue;
                    }

                    if (_grid[x, y] == Tile.Diamond && !_falling[x, y] && _offset[x, y] == Vector2.Zero)
                    {
                        souls.Add(new Vector2I(x, y));
                    }
                    else if (_grid[x, y] == Tile.Dirt)
                    {
                        int by = y + _gravity;
                        if (InBounds(x, by) && _grid[x, by] != Tile.Empty)
                        {
                            spots.Add(new Vector2I(x, y));
                        }
                    }
                }
            }

            if (souls.Count == 0 || spots.Count == 0)
            {
                return;
            }

            Vector2I from = souls[(int)(_rng.Randi() % (uint)souls.Count)];
            Vector2I to = spots[(int)(_rng.Randi() % (uint)spots.Count)];
            _grid[from.X, from.Y] = Tile.Empty;
            _grid[to.X, to.Y] = Tile.Diamond;
            _fx.Add(new DriftFx { Cell = from });
            _fx.Add(new DriftFx { Cell = to });
        }

        //Skeletons
        private Enemy EnemyAt(int x, int y)
        {
            foreach (Enemy e in _enemies)
            {
                if (e.X == x && e.Y == y)
                {
                    return e;
                }
            }
            return null;
        }

        private bool EnemyCanEnter(int x, int y)
        {
            return InBounds(x, y) && (_grid[x, y] == Tile.Empty || _grid[x, y] == Tile.Bush) && EnemyAt(x, y) == null;
        }

        // Each skeleton hugs the wall on its left: it turns left if it can, otherwise goes straight, otherwise turns right. This makes it patrol tunnels and walk around loops.
        private void EnemyStep()
        {
            foreach (Enemy e in _enemies)
            {
                int left = (e.Dir + 3) % 4;
                Vector2I l = Dirs[left];
                Vector2I f = Dirs[e.Dir];

                if (EnemyCanEnter(e.X + l.X, e.Y + l.Y))
                {
                    e.Dir = left;
                    e.X += l.X;
                    e.Y += l.Y;
                }
                else if (EnemyCanEnter(e.X + f.X, e.Y + f.Y))
                {
                    e.X += f.X;
                    e.Y += f.Y;
                }
                else
                {
                    e.Dir = (e.Dir + 1) % 4;
                }

                if (_grid[e.X, e.Y] == Tile.Bush)
                {
                    _bushShake[e.X, e.Y] = 0.6f;   // the only clue that something is hiding in there
                }

                if (PlayerAt(e.X, e.Y))
                {
                    _state = State.Dead;
                }
            }
        }

    }
}
