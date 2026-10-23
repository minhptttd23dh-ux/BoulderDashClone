using Godot;
using System.Collections.Generic;

namespace BoulderDash
{
    // Falling boulders and souls, Heaven powers
    public partial class Game
    {
        private bool IsRound(Tile t)
        {
            return t == Tile.Boulder || t == Tile.Diamond;
        }

        private bool PlayerAt(int x, int y)
        {
            return _player.X == x && _player.Y == y;
        }

        private bool CanRollTo(int x, int y)
        {
            int by = y + _gravity;
            if (!InBounds(x, y) || !InBounds(x, by)) return false;
            if (_grid[x, y] != Tile.Empty || _grid[x, by] != Tile.Empty) return false;
            if (PlayerAt(x, y) || PlayerAt(x, by)) return false;
            if (EnemyAt(x, y) != null || EnemyAt(x, by) != null) return false;
            return true;
        }

        private void MoveObject(int fromX, int fromY, int toX, int toY)
        {
            _grid[toX, toY] = _grid[fromX, fromY];
            _grid[fromX, fromY] = Tile.Empty;
            _falling[toX, toY] = true;
            _falling[fromX, fromY] = false;
            _moved[toX, toY] = true;
            _offset[toX, toY] = new Vector2(fromX - toX, fromY - toY);
            _offset[fromX, fromY] = Vector2.Zero;
        }

        private void Toast(string text)
        {
            _toast = text;
            _toastTime = 2.2f;
        }

        // SPACE next to an unused Heaven statue starts the gravity flip. Each statue works once.
        private void UseStatue()
        {
            if (_flipTimer > 0)
            {
                return;
            }

            foreach (Vector2I d in Dirs)
            {
                Vector2I n = _player + d;
                if (InBounds(n.X, n.Y) && _grid[n.X, n.Y] == Tile.Statue)
                {
                    _grid[n.X, n.Y] = Tile.StatueUsed;
                    _pending = Power.Flip;
                    _flipTimer = FlipDelay;
                    return;
                }
            }

            bool spent = false;
            foreach (Vector2I d in Dirs)
            {
                Vector2I n = _player + d;
                if (InBounds(n.X, n.Y) && _grid[n.X, n.Y] == Tile.StatueUsed)
                {
                    spent = true;
                }
            }
            Toast(spent ? "This statue has already been used" : "Stand next to a Heaven statue and press SPACE");
        }

        // E or right-click: spend a burst charge. The charge-up runs first, then everything around the sheep breaks.
        private void StartBurst()
        {
            if (_flipTimer > 0)
            {
                return;
            }
            if (_charges <= 0)
            {
                Toast("No burst charges left");
                return;
            }
            _charges--;
            _pending = Power.Burst;
            _flipTimer = FlipDelay;
        }

        // Smashes everything breakable in the 3x3 area around the sheep: dirt, rocks, bushes and skeletons.
        // Walls, doors, keys, souls and the gate stay. Rocks above the hole may fall afterwards!
        private void DoBurst()
        {
            _shake = Mathf.Max(_shake, 0.6f);
            _flash = 0.7f;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int x = _player.X + dx;
                    int y = _player.Y + dy;
                    if ((dx == 0 && dy == 0) || !InBounds(x, y))
                    {
                        continue;
                    }

                    Tile t = _grid[x, y];
                    bool broke = false;
                    if (t == Tile.Dirt || t == Tile.Boulder || t == Tile.Bush)
                    {
                        _grid[x, y] = Tile.Empty;
                        _offset[x, y] = Vector2.Zero;
                        broke = true;
                    }

                    Enemy en = EnemyAt(x, y);
                    if (en != null)
                    {
                        _enemies.Remove(en);
                        _kills++;
                        _score += 25;
                        if (_grid[x, y] == Tile.Empty)
                        {
                            _grid[x, y] = Tile.Diamond;
                        }
                        broke = true;
                    }

                    if (broke)
                    {
                        _fx.Add(new DriftFx { Cell = new Vector2I(x, y) });
                    }
                }
            }
            System.Array.Clear(_falling, 0, _falling.Length);
            _physicsTimer = PhysicsStepTime;
        }

        private void DoFlip()
        {
            _shake = Mathf.Max(_shake, 0.45f);
            _gravity = -_gravity;
            _flash = 1f;
            // Everything starts from rest, so rocks that were mid-fall do not crush anyone by surprise.
            System.Array.Clear(_falling, 0, _falling.Length);
            _physicsTimer = PhysicsStepTime;   // react right away
        }

        private void PhysicsStep()
        {
            System.Array.Clear(_moved, 0, _moved.Length);
            System.Array.Clear(_wobbling, 0, _wobbling.Length);
            int dy = _gravity;

            // Process the objects closest to the "floor" first so a whole column can fall one step per tick.
            int yStart = dy > 0 ? _height - 2 : 1;
            int yEnd = dy > 0 ? -1 : _height;
            int yStep = dy > 0 ? -1 : 1;

            for (int y = yStart; y != yEnd; y += yStep)
            {
                for (int x = 0; x < _width; x++)
                {
                    Tile t = _grid[x, y];
                    if (!IsRound(t) || _moved[x, y])
                    {
                        continue;
                    }

                    int by = y + dy;
                    Tile below = _grid[x, by];

                    if (PlayerAt(x, by))
                    {
                        // A falling object crushes the player. A resting one is safe only while the
                        // player keeps moving: stand still too long and it wobbles, then drops.
                        if (_falling[x, y])
                        {
                            _state = State.Dead;
                        }
                        else if (_idle >= WobbleStart)
                        {
                            _wobbling[x, y] = true;
                            if (_idle >= WobbleStart + WobbleDuration)
                            {
                                _state = State.Dead;
                            }
                        }
                        _falling[x, y] = false;
                        continue;
                    }

                    Enemy victim = EnemyAt(x, by);
                    if (victim != null)
                    {
                        if (_falling[x, y])
                        {
                            // A falling rock turns the skeleton into a soul.
                            _enemies.Remove(victim);
                            _kills++;
                            _score += 25;
                            _grid[x, by] = Tile.Diamond;
                        }
                        _falling[x, y] = false;
                        continue;
                    }

                    if (below == Tile.Empty || below == Tile.Bush)
                    {
                        // Falling into a bush tears it down.
                        MoveObject(x, y, x, by);
                        continue;
                    }

                    _falling[x, y] = false;

                    // Roll off the top of other round objects.
                    if (IsRound(below))
                    {
                        // Roll to whichever side is free. If both sides are free, pick one at random;
                        // if one side is blocked, use the other.
                        bool canLeft = CanRollTo(x - 1, y);
                        bool canRight = CanRollTo(x + 1, y);
                        if (canLeft && canRight)
                        {
                            if (_rng.Randf() < 0.5f) canRight = false; else canLeft = false;
                        }
                        if (canLeft)
                        {
                            MoveObject(x, y, x - 1, y);
                            _falling[x - 1, y] = false;
                        }
                        else if (canRight)
                        {
                            MoveObject(x, y, x + 1, y);
                            _falling[x + 1, y] = false;
                        }
                    }
                }
            }

            // Many rocks falling at once shake the whole screen.
            int rocks = 0;
            for (int ry = 0; ry < _height; ry++)
            {
                for (int rx = 0; rx < _width; rx++)
                {
                    if (_grid[rx, ry] == Tile.Boulder && _falling[rx, ry])
                    {
                        rocks++;
                    }
                }
            }
            if (rocks >= 4)
            {
                _shake = Mathf.Max(_shake, Mathf.Min(1f, 0.3f + rocks * 0.08f));
            }
        }

    }
}
