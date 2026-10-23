using Godot;
using System.Collections.Generic;

namespace BoulderDash
{
    // Background, tower floor, heaven glow, story screen and minimap.
    public partial class Game
    {
        private void DrawBackground()
        {
            if (_textures.TryGetValue("background", out Texture2D bg))
            {
                // Slightly oversized and shifted with the camera, so the sky has some depth behind the tower.
                float px = Mathf.Clamp(_origin.X * 0.02f, -36f, 36f);
                float py = Mathf.Clamp((_origin.Y - HudHeight) * 0.02f, -20f, 20f);
                DrawTextureRect(bg, new Rect2(-40 + px, HudHeight - 24 + py, ViewW + 80, ViewH + 48), false);
            }
            else
            {
                // Dark sky that glows red toward the bottom, like a lava pit.
                const int bands = 16;
                float bandH = ViewH / (float)bands;
                for (int i = 0; i < bands; i++)
                {
                    float f = i / (float)(bands - 1);
                    Color hell = new Color(0.04f + 0.28f * f * f, 0.0f + 0.05f * f, 0.02f + 0.01f * f);
                    float g = 1f - f;
                    Color heaven = new Color(0.3f + 0.5f * g * g, 0.24f + 0.42f * g * g, 0.1f + 0.18f * g * g);
                    Color c = hell.Lerp(heaven, _heaven);
                    DrawRect(new Rect2(0, HudHeight + i * bandH, ViewW, bandH + 1), c);
                }
            }

            if (_heaven > 0.01f)
            {
                // Golden light pours in from above.
                const int gb = 10;
                float gh = ViewH / (float)gb;
                for (int i = 0; i < gb; i++)
                {
                    float f = i / (float)(gb - 1);
                    DrawRect(new Rect2(0, HudHeight + i * gh, ViewW, gh + 1), new Color(1f, 0.82f, 0.4f, (0.45f - 0.3f * f) * _heaven));
                }
                for (int i = 0; i < 7; i++)
                {
                    float x0 = ViewW * (i + 0.5f) / 7f + Mathf.Sin(_time * 0.5f + i * 1.3f) * 50f;
                    float lean = Mathf.Sin(_time * 0.3f + i) * 60f + 40f;
                    DrawColoredPolygon(new[]
                    {
                        new Vector2(x0 - 22, HudHeight), new Vector2(x0 + 22, HudHeight),
                        new Vector2(x0 + 70 + lean, HudHeight + ViewH), new Vector2(x0 - 70 + lean, HudHeight + ViewH),
                    }, new Color(1f, 0.95f, 0.65f, 0.13f * _heaven));
                }
            }

            // Embers
            for (int i = 0; i < EmberCount; i++)
            {
                float size = 1.5f + _emberSpeed[i] / 30f;
                float flicker = 0.5f + 0.4f * Mathf.Sin(_time * 5f + i * 1.7f);
                DrawCircle(_embers[i], size, new Color(1f, 0.45f + 0.4f * _heaven, 0.1f + 0.5f * _heaven, flicker));
            }
        }

        // The inside of the tower: a dark stone floor behind everything, plus battlements along the open top edges.
        private void DrawTowerFloor()
        {
            float pad = _ts;
            Color tint = new Color(1f, 1f, 1f).Lerp(new Color(1f, 0.85f, 0.6f), _heaven * 0.6f);
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (_grid[x, y] == Tile.Void)
                    {
                        continue;
                    }
                    Vector2 pos = _origin + new Vector2(x, y) * _ts;
                    if (pos.X < -pad || pos.X > ViewW || pos.Y < HudHeight - pad * 2 || pos.Y > HudHeight + ViewH)
                    {
                        continue;
                    }
                    Rect2 r = new Rect2(pos, new Vector2(_ts, _ts));
                    if (!TryDrawTexture("floor", r, tint))
                    {
                        bool alt = ((x + y) & 1) == 0;
                        DrawRect(r, alt ? new Color(0.12f, 0.07f, 0.15f) : new Color(0.10f, 0.06f, 0.13f));
                        DrawRect(r, new Color(0.06f, 0.03f, 0.08f), false, 1f);
                    }
                    if (_grid[x, y] == Tile.Wall && (y == 0 || _grid[x, y - 1] == Tile.Void))
                    {
                        DrawMerlons(r);
                    }
                }
            }
        }

        private void DrawMerlons(Rect2 r)
        {
            float s = r.Size.X;
            float mw = s * 0.34f;
            float mh = s * 0.26f;
            for (int i = 0; i < 2; i++)
            {
                float fx = 0.06f + i * 0.54f;
                Rect2 m = new Rect2(r.Position.X + s * fx, r.Position.Y - mh, mw, mh);
                if (_textures.TryGetValue("wall", out Texture2D wt))
                {
                    Rect2 src = new Rect2(wt.GetWidth() * fx, 0, wt.GetWidth() * 0.34f, wt.GetHeight() * 0.26f);
                    DrawTextureRectRegion(wt, m, src);
                }
                else
                {
                    DrawRect(m, new Color(0.2f, 0.12f, 0.3f));
                }
                DrawRect(m, new Color(0.07f, 0.04f, 0.1f), false, 2f);
            }
        }

        // Heaven Power: a soft golden wash and a glowing frame, strongest at the top where the light comes from.
        private void DrawHeavenGlow()
        {
            float h = _heaven;
            DrawRect(new Rect2(0, HudHeight, ViewW, ViewH), new Color(1f, 0.85f, 0.45f, 0.10f * h));
            for (int i = 0; i < 12; i++)
            {
                float k = i / 12f;
                float a = (1f - k) * (1f - k) * h;
                DrawRect(new Rect2(0, HudHeight + i * 14, ViewW, 14), new Color(1f, 0.9f, 0.55f, a * 0.32f));
                DrawRect(new Rect2(i * 12, HudHeight, 12, ViewH), new Color(1f, 0.9f, 0.55f, a * 0.14f));
                DrawRect(new Rect2(ViewW - (i + 1) * 12, HudHeight, 12, ViewH), new Color(1f, 0.9f, 0.55f, a * 0.14f));
            }
            // Drifting feathers of light
            for (int i = 0; i < 14; i++)
            {
                float fx = (i * 97 % 13) / 13f * ViewW + Mathf.Sin(_time + i) * 20f;
                float fy = HudHeight + ((_time * (20f + i * 3f) + i * 120f) % ViewH);
                DrawCircle(new Vector2(fx, fy), 2.5f, new Color(1f, 0.97f, 0.8f, 0.55f * h));
            }
        }

        // Opening cutscene: the voice from above sends the sheep to Hell.
        private void DrawStory()
        {
            Font font = _font;
            Font titleFont = _titleFont;
            string[] page = StoryPages[_storyPage];
            bool voice = page[0] == "V";
            float total = HudHeight + ViewH;

            DrawRect(new Rect2(0, 0, ViewW, total), new Color(0.02f, 0.0f, 0.03f, 0.94f));

            // The light of the voice
            Vector2 lc = new Vector2(ViewW / 2f, 190);
            float pulse = 1f + 0.06f * Mathf.Sin(_time * 2f);
            float lit = voice ? 1f : 0.35f;
            for (int i = 0; i < 9; i++)
            {
                float rr = (150f - i * 15f) * pulse;
                DrawCircle(lc, rr, new Color(1f, 0.85f, 0.45f, 0.05f * lit + i * 0.012f * lit));
            }
            for (int i = 0; i < 10; i++)
            {
                float ang = i / 10f * Mathf.Tau + _time * 0.1f;
                Vector2 d = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                DrawLine(lc + d * 70f, lc + d * (200f + 20f * Mathf.Sin(_time + i)), new Color(1f, 0.92f, 0.6f, 0.25f * lit), 3f);
            }
            DrawCircle(lc, 46f * pulse, new Color(1f, 0.97f, 0.85f, 0.9f * lit));

            // The sheep, small, bottom left
            Rect2 sheep = new Rect2(150, 285, 150, 150);
            if (voice)
            {
                DrawRect(sheep, new Color(0, 0, 0, 0.0f));
            }
            DrawPlayer(sheep);
            if (voice)
            {
                DrawRect(sheep, new Color(0.02f, 0.0f, 0.03f, 0.45f));
            }

            Rect2 box = new Rect2(80, total - 230, ViewW - 160, 190);
            DrawRect(box, new Color(0, 0, 0, 0.85f));
            DrawRect(box, voice ? new Color(1f, 0.85f, 0.45f) : new Color(0.75f, 0.15f, 0.08f), false, 2f);
            DrawString(titleFont, new Vector2(box.Position.X + 24, box.Position.Y + 42), voice ? "The Voice Above" : "Sheep", HorizontalAlignment.Left, -1, 30, GoldText);
            string visible = page[1].Substring(0, Mathf.Min(page[1].Length, Mathf.Max(0, (int)_storyChars)));
            const int textSize = 24;
            DrawMultilineString(font, new Vector2(box.Position.X + 24, box.Position.Y + 62 + font.GetAscent(textSize)), visible, HorizontalAlignment.Left, box.Size.X - 48, textSize, -1, new Color(0.95f, 0.9f, 0.85f));
            float blink = 0.5f + 0.5f * Mathf.Sin(_time * 4f);
            DrawString(font, new Vector2(box.Position.X, box.End.Y - 14), "Click or SPACE to continue    ESC to skip", HorizontalAlignment.Right, box.Size.X - 20, 15, new Color(0.8f, 0.7f, 0.6f, 0.45f + 0.5f * blink));
        }

        private void DrawMiniMap()
        {
            float mp = Mathf.Min(8f, Mathf.Floor(Mathf.Min(300f / _width, 200f / _height)));
            if (mp < 2f)
            {
                mp = 2f;
            }
            float mapW = _width * mp;
            float mapH = _height * mp;
            Vector2 topLeft = new Vector2(ViewW - mapW - 14, HudHeight + ViewH - mapH - 14);

            DrawRect(new Rect2(topLeft - new Vector2(4, 4), new Vector2(mapW + 8, mapH + 8)), new Color(0, 0, 0, 0.7f));
            DrawRect(new Rect2(topLeft - new Vector2(4, 4), new Vector2(mapW + 8, mapH + 8)), new Color(0.75f, 0.15f, 0.08f), false, 2f);

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    Tile t = _grid[x, y];
                    if (t == Tile.Empty || t == Tile.Void)
                    {
                        continue;
                    }
                    Color c = Colors.White;
                    switch (t)
                    {
                        case Tile.Dirt: c = new Color(0.4f, 0.14f, 0.1f); break;
                        case Tile.Wall: c = new Color(0.45f, 0.25f, 0.55f); break;
                        case Tile.Boulder: c = new Color(0.55f, 0.52f, 0.5f); break;
                        case Tile.Diamond: c = new Color(1f, 0.55f, 0.15f); break;
                        case Tile.Bush: c = new Color(0.6f, 0.15f, 0.3f); break;
                        case Tile.Statue: c = new Color(1f, 0.95f, 0.6f); break;
                        case Tile.StatueUsed: c = new Color(0.5f, 0.5f, 0.5f); break;
                        case Tile.Key: c = new Color(1f, 0.9f, 0.3f); break;
                        case Tile.Door: c = new Color(0.9f, 0.7f, 0.2f); break;
                        case Tile.Exit: c = GateOpen() ? new Color(0.9f, 0.3f, 1f) : new Color(0.5f, 0.1f, 0.1f); break;
                    }
                    DrawRect(new Rect2(topLeft + new Vector2(x * mp, y * mp), new Vector2(mp, mp)), c);
                }
            }

            // Area currently shown on the main screen
            Rect2 visible = new Rect2(-_origin.X / _ts, -(_origin.Y - HudHeight) / _ts, ViewW / _ts, ViewH / _ts);
            visible = visible.Intersection(new Rect2(0, 0, _width, _height));
            if (visible.Size.X > 0 && visible.Size.Y > 0)
            {
                DrawRect(new Rect2(topLeft + visible.Position * mp, visible.Size * mp), new Color(1f, 1f, 1f, 0.8f), false, 1f);
            }

            foreach (Enemy e in _enemies)
            {
                DrawRect(new Rect2(topLeft + new Vector2(e.X * mp, e.Y * mp), new Vector2(mp, mp)), new Color(1f, 0.1f, 0.1f));
            }

            // Player marker blinks
            if (Mathf.Sin(_time * 8f) > -0.3f)
            {
                DrawRect(new Rect2(topLeft + _playerVisual * mp - new Vector2(1, 1), new Vector2(mp + 2, mp + 2)), Colors.White);
            }
        }

    }
}
