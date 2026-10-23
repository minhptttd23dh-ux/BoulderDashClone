using Godot;
using System.Collections.Generic;

namespace BoulderDash
{
    // Camera and the main _Draw(): HUD, hint box, toasts and end screens.
    public partial class Game
    {
        private void ComputeView()
        {
            if (_showFullMap)
            {
                // Fit the whole level on screen.
                _ts = Mathf.Floor(Mathf.Min(ViewW / (float)_width, ViewH / (float)_height));
                _origin = new Vector2((ViewW - _ts * _width) / 2f, HudHeight + (ViewH - _ts * _height) / 2f);
            }
            else
            {
                // Zoomed in, camera follows the player and stops at the level edges.
                _ts = ZoomTile;
                float levelW = _width * _ts;
                float levelH = _height * _ts;
                Vector2 pc = (_playerVisual + new Vector2(0.5f, 0.5f)) * _ts;
                float ox = levelW <= ViewW ? (ViewW - levelW) / 2f : Mathf.Clamp(ViewW / 2f - pc.X, ViewW - levelW, 0f);
                float oy = levelH <= ViewH ? (ViewH - levelH) / 2f : Mathf.Clamp(ViewH / 2f - pc.Y, ViewH - levelH, 0f);
                _origin = new Vector2(ox, HudHeight + oy);
            }
        }

        // Drawing (placeholder hell theme)

        private static readonly Color HudColor = new Color(0.16f, 0.04f, 0.04f);
        private static readonly Color GoldText = new Color(1f, 0.82f, 0.45f);

        public override void _Draw()
        {
            if (_grid == null)
            {
                return;
            }

            ComputeView();
            Font font = _font;
            Font titleFont = _titleFont;

            DrawBackground();

            Vector2 shakeOff = _shake > 0.01f
                ? new Vector2(_rng.RandfRange(-1f, 1f), _rng.RandfRange(-1f, 1f)) * _shake * 12f
                : Vector2.Zero;
            DrawSetTransform(shakeOff);
            DrawTowerFloor();

            float pad = _ts;
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (_grid[x, y] == Tile.Void)
                    {
                        continue;
                    }
                    Vector2 pos = _origin + new Vector2(x, y) * _ts + _offset[x, y] * _ts;
                    if (_wobbling[x, y])
                    {
                        pos += new Vector2(Mathf.Sin(_time * 60f) * _ts * 0.06f, 0);
                    }
                    if (pos.X < -pad || pos.X > ViewW || pos.Y < HudHeight - pad || pos.Y > HudHeight + ViewH)
                    {
                        continue;
                    }
                    DrawTile(_grid[x, y], new Rect2(pos, new Vector2(_ts, _ts)), x, y);
                }
            }

            foreach (DriftFx fx in _fx)
            {
                float k = fx.Age / 0.8f;
                Vector2 fc = _origin + (new Vector2(fx.Cell.X, fx.Cell.Y) + new Vector2(0.5f, 0.5f)) * _ts;
                DrawArc(fc, _ts * (0.2f + 0.7f * k), 0, Mathf.Tau, 24, new Color(1f, 0.8f, 0.4f, 1f - k), 3f);
            }

            foreach (Enemy e in _enemies)
            {
                if (_grid[e.X, e.Y] == Tile.Bush)
                {
                    continue;   // hidden in a bush
                }
                DrawEnemy(new Rect2(_origin + e.Visual * _ts, new Vector2(_ts, _ts)));
            }

            Rect2 playerRect = new Rect2(_origin + _playerVisual * _ts, new Vector2(_ts, _ts));
            DrawPlayer(playerRect);
            if (_grid[_player.X, _player.Y] == Tile.Bush)
            {
                // Bush leaves in front of the sheep while it walks through.
                if (!TryDrawTexture("bush", playerRect, new Color(1f, 1f, 1f, 0.6f)))
                {
                    DrawBush(playerRect, 0.6f, 0f);
                }
            }

            if (_flipTimer > 0)
            {
                // Charge-up: glowing rings close in on the sheep.
                float k = 1f - _flipTimer / FlipDelay;   // 0 -> 1
                Vector2 pc = _origin + (_playerVisual + new Vector2(0.5f, 0.5f)) * _ts;
                Color ringColor = _pending == Power.Burst ? new Color(1f, 0.4f, 0.1f, 0.3f + 0.6f * k) : new Color(1f, 0.9f, 0.5f, 0.3f + 0.6f * k);
                DrawArc(pc, _ts * (1.4f - 0.9f * k), 0, Mathf.Tau, 32, ringColor, 4f);
                DrawArc(pc, _ts * (1.0f - 0.6f * k), 0, Mathf.Tau, 32, new Color(1f, 1f, 1f, 0.2f + 0.6f * k), 2f);
            }

            DrawSetTransform(Vector2.Zero);
            if (_heaven > 0.01f)
            {
                DrawHeavenGlow();
            }

            if (_flash > 0f)
            {
                DrawRect(new Rect2(0, HudHeight, ViewW, ViewH), new Color(1f, 1f, 1f, _flash * 0.85f));
            }

            // HUD is drawn last so it covers any tiles that scroll under it.
            DrawRect(new Rect2(0, 0, ViewW, HudHeight), HudColor);
            DrawRect(new Rect2(0, HudHeight - 2, ViewW, 2), new Color(0.75f, 0.15f, 0.08f));
            string hud = "Level " + (_levelIndex + 1) + "    Souls " + _diamonds + "/" + _needed
                         + (_killsNeeded > 0 ? "    Skeletons " + _kills + "/" + _killsNeeded : "")
                         + "    Score " + _score + "    Time " + Mathf.CeilToInt(_timeLeft)
                         + "    Burst " + _charges + (_gravity < 0 ? "    Gravity UP" : "")
                         + (_keys > 0 ? "    Keys " + _keys : "");
            DrawString(titleFont, new Vector2(14, 28), hud, HorizontalAlignment.Left, -1, 24, GoldText);
            string hint = (_showFullMap ? "M: back to zoom" : "M: full map") + "   SPACE at statue: gravity   E: burst   F11: fullscreen";
            DrawString(titleFont, new Vector2(0, 28), hint, HorizontalAlignment.Right, ViewW - 14, 20, new Color(0.8f, 0.6f, 0.5f));

            if (!_showFullMap)
            {
                DrawMiniMap();
            }

            if (_hint != "" && !_hintHidden && _state == State.Playing && (_tutorial || _levelClock < 8f))
            {
                // The box is sized from the real text height, so long hints are never cut off.
                const int hintSize = 18;
                float textWidth = ViewW - 76;
                Vector2 textSize = font.GetMultilineStringSize(_hint, HorizontalAlignment.Left, textWidth, hintSize);
                float titleBlock = 44;
                float boxH = titleBlock + textSize.Y + 22;
                Rect2 box = new Rect2(20, HudHeight + 10, ViewW - 40, boxH);
                DrawRect(box, new Color(0, 0, 0, 0.8f));
                DrawRect(box, new Color(0.75f, 0.15f, 0.08f), false, 2f);
                DrawString(titleFont, new Vector2(38, box.Position.Y + 34), _title, HorizontalAlignment.Left, -1, 30, GoldText);
                DrawString(font, new Vector2(box.Position.X, box.Position.Y + 28), "Click to hide", HorizontalAlignment.Right, box.Size.X - 18, 15, new Color(0.7f, 0.6f, 0.55f));
                DrawMultilineString(font, new Vector2(38, box.Position.Y + titleBlock + font.GetAscent(hintSize)), _hint, HorizontalAlignment.Left, textWidth, hintSize, -1, new Color(0.95f, 0.9f, 0.85f));
            }

            if (_toastTime > 0f)
            {
                float a = Mathf.Min(1f, _toastTime);
                float ty = HudHeight + ViewH - 40;
                DrawRect(new Rect2(ViewW / 2f - 320, ty - 30, 640, 44), new Color(0, 0, 0, 0.75f * a));
                DrawString(titleFont, new Vector2(0, ty), _toast, HorizontalAlignment.Center, ViewW, 26, new Color(1f, 0.82f, 0.45f, a));
            }

            if (_state != State.Playing && _state != State.Story)
            {
                float fade = Mathf.Clamp(_stateTime / 0.7f, 0f, 1f);
                float tfade = Mathf.Clamp((_stateTime - 0.4f) / 0.6f, 0f, 1f);
                string sub = "";
                switch (_state)
                {
                    case State.Dead: sub = "Press R to try again"; break;
                    case State.LevelComplete: sub = "Press Enter for the next level"; break;
                    case State.AllClear: sub = "Score " + _score + "    Press R to play again"; break;
                }
                float midY = HudHeight + ViewH / 2f;
                DrawRect(new Rect2(0, HudHeight, ViewW, ViewH), new Color(0, 0, 0, 0.35f * fade));
                DrawRect(new Rect2(0, midY - 62, ViewW, 124), new Color(0, 0, 0, 0.85f * fade));
                DrawRect(new Rect2(0, midY - 62, ViewW, 2), new Color(0.75f, 0.15f, 0.08f, fade));
                DrawRect(new Rect2(0, midY + 60, ViewW, 2), new Color(0.75f, 0.15f, 0.08f, fade));
                DrawString(titleFont, new Vector2(0, midY - 2), _endLine, HorizontalAlignment.Center, ViewW, 38, new Color(GoldText.R, GoldText.G, GoldText.B, tfade));
                DrawString(font, new Vector2(0, midY + 38), sub, HorizontalAlignment.Center, ViewW, 20, new Color(0.85f, 0.75f, 0.7f, tfade));
            }

            if (_state == State.Story)
            {
                DrawStory();
            }
        }

    }
}
