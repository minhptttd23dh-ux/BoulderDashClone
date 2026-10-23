using Godot;
using System.Collections.Generic;

namespace BoulderDash
{
    // Drawing of single tiles and characters (placeholders or Art/ textures).
    public partial class Game
    {
        private bool TryDrawTexture(string name, Rect2 r, Color? modulate = null)
        {
            if (_textures.TryGetValue(name, out Texture2D tex))
            {
                DrawTextureRect(tex, r, false, modulate);
                return true;
            }
            return false;
        }

        private void DrawTile(Tile t, Rect2 r, int gx, int gy)
        {
            float s = r.Size.X;
            Vector2 c = r.Position + r.Size / 2f;

            switch (t)
            {
                case Tile.Dirt:
                    if (!TryDrawTexture("dirt", r))
                    {
                        // Scorched earth with a few glowing cracks.
                        DrawRect(r, new Color(0.3f, 0.1f, 0.08f));
                        DrawRect(r.Grow(-s * 0.08f), new Color(0.23f, 0.07f, 0.06f));
                        int h = (gx * 7 + gy * 13) % 5;
                        if (h == 0)
                        {
                            DrawLine(r.Position + new Vector2(s * 0.2f, s * 0.3f), r.Position + new Vector2(s * 0.45f, s * 0.5f), new Color(1f, 0.4f, 0.1f, 0.8f), 2f);
                            DrawLine(r.Position + new Vector2(s * 0.45f, s * 0.5f), r.Position + new Vector2(s * 0.4f, s * 0.75f), new Color(1f, 0.4f, 0.1f, 0.8f), 2f);
                        }
                        else if (h == 2)
                        {
                            DrawCircle(r.Position + new Vector2(s * 0.7f, s * 0.65f), s * 0.05f, new Color(1f, 0.35f, 0.1f, 0.8f));
                        }
                    }
                    break;

                case Tile.Wall:
                    if (!TryDrawTexture("wall", r))
                    {
                        // Obsidian block.
                        DrawRect(r, new Color(0.1f, 0.05f, 0.14f));
                        DrawRect(r.Grow(-s * 0.06f), new Color(0.2f, 0.1f, 0.27f));
                        DrawRect(new Rect2(r.Position + new Vector2(s * 0.12f, s * 0.12f), new Vector2(s * 0.3f, s * 0.1f)), new Color(0.4f, 0.22f, 0.5f));
                    }
                    break;

                case Tile.Boulder:
                    if (!TryDrawTexture("boulder", r))
                    {
                        // Charred rock with an ember glow.
                        DrawCircle(c, s * 0.47f, new Color(0.9f, 0.25f, 0.05f, 0.35f));
                        DrawCircle(c, s * 0.4f, new Color(0.22f, 0.2f, 0.2f));
                        DrawCircle(c + new Vector2(-s * 0.1f, -s * 0.1f), s * 0.13f, new Color(0.38f, 0.34f, 0.33f));
                        DrawLine(c + new Vector2(s * 0.05f, s * 0.05f), c + new Vector2(s * 0.22f, s * 0.2f), new Color(1f, 0.4f, 0.1f, 0.9f), 2f);
                    }
                    break;

                case Tile.Diamond:
                    if (!TryDrawTexture("diamond", r))
                    {
                        // Soul gem.
                        float pulse = 0.5f + 0.5f * Mathf.Sin(_time * 4f + gx + gy);
                        DrawCircle(c, s * (0.36f + 0.04f * pulse), new Color(1f, 0.5f, 0.1f, 0.25f));
                        Vector2[] outer =
                        {
                            c + new Vector2(0, -s * 0.38f), c + new Vector2(s * 0.3f, 0),
                            c + new Vector2(0, s * 0.38f), c + new Vector2(-s * 0.3f, 0),
                        };
                        DrawColoredPolygon(outer, new Color(1f, 0.35f, 0.1f));
                        Vector2[] inner =
                        {
                            c + new Vector2(0, -s * 0.2f), c + new Vector2(s * 0.14f, 0),
                            c + new Vector2(0, s * 0.2f), c + new Vector2(-s * 0.14f, 0),
                        };
                        DrawColoredPolygon(inner, new Color(1f, 0.85f, 0.4f));
                    }
                    break;

                case Tile.Statue:
                case Tile.StatueUsed:
                    bool used = t == Tile.StatueUsed;
                    if (!TryDrawTexture(used ? "statue_used" : "statue", r))
                    {
                        // Placeholder angel statue on a stone base.
                        float glow = used ? 0f : 0.5f + 0.5f * Mathf.Sin(_time * 3f + gx);
                        Color stone = used ? new Color(0.35f, 0.35f, 0.38f) : new Color(0.9f, 0.88f, 0.8f);
                        Color shade = used ? new Color(0.25f, 0.25f, 0.28f) : new Color(0.7f, 0.66f, 0.55f);
                        if (!used)
                        {
                            DrawCircle(c + new Vector2(0, -s * 0.12f), s * (0.42f + 0.05f * glow), new Color(1f, 0.9f, 0.5f, 0.25f));
                        }
                        DrawRect(new Rect2(r.Position + new Vector2(s * 0.15f, s * 0.78f), new Vector2(s * 0.7f, s * 0.17f)), shade);
                        DrawRect(new Rect2(r.Position + new Vector2(s * 0.33f, s * 0.36f), new Vector2(s * 0.34f, s * 0.44f)), stone);
                        DrawCircle(c + new Vector2(0, -s * 0.2f), s * 0.13f, stone);
                        DrawArc(c + new Vector2(0, -s * 0.34f), s * 0.12f, 0, Mathf.Tau, 20, used ? new Color(0.5f, 0.5f, 0.5f) : new Color(1f, 0.85f, 0.3f), 3f);
                        DrawColoredPolygon(new[] { c + new Vector2(-s * 0.1f, -s * 0.05f), c + new Vector2(-s * 0.4f, -s * 0.22f), c + new Vector2(-s * 0.28f, s * 0.15f) }, stone);
                        DrawColoredPolygon(new[] { c + new Vector2(s * 0.1f, -s * 0.05f), c + new Vector2(s * 0.4f, -s * 0.22f), c + new Vector2(s * 0.28f, s * 0.15f) }, stone);
                    }
                    break;

                case Tile.Bush:
                    float shake = _bushShake[gx, gy] > 0f ? Mathf.Sin(_time * 55f) * s * 0.08f : 0f;
                    if (!TryDrawTexture("bush", new Rect2(r.Position + new Vector2(shake, 0), r.Size)))
                    {
                        DrawBush(r, 1f, shake);
                    }
                    break;

                case Tile.Key:
                    if (!TryDrawTexture("key", r))
                    {
                        Color gold = new Color(1f, 0.85f, 0.25f);
                        DrawCircle(c + new Vector2(-s * 0.12f, -s * 0.1f), s * 0.14f, gold);
                        DrawCircle(c + new Vector2(-s * 0.12f, -s * 0.1f), s * 0.06f, new Color(0.1f, 0.05f, 0.05f));
                        DrawLine(c + new Vector2(-s * 0.02f, 0), c + new Vector2(s * 0.28f, s * 0.3f), gold, 4f);
                        DrawLine(c + new Vector2(s * 0.14f, s * 0.16f), c + new Vector2(s * 0.24f, s * 0.06f), gold, 4f);
                    }
                    break;

                case Tile.Door:
                    if (!TryDrawTexture("door", r))
                    {
                        DrawRect(r, new Color(0.15f, 0.1f, 0.05f));
                        DrawRect(r.Grow(-s * 0.08f), new Color(0.35f, 0.25f, 0.1f));
                        for (int i = 1; i <= 3; i++)
                        {
                            DrawLine(r.Position + new Vector2(s * 0.25f * i, s * 0.08f), r.Position + new Vector2(s * 0.25f * i, s * 0.92f), new Color(0.15f, 0.1f, 0.05f), 3f);
                        }
                        DrawCircle(c, s * 0.1f, new Color(1f, 0.85f, 0.25f));
                    }
                    break;

                case Tile.Exit:
                    bool open = GateOpen();
                    if (open)
                    {
                        if (!TryDrawTexture("exit", r))
                        {
                            // Glowing portal.
                            float pulse = 0.5f + 0.5f * Mathf.Sin(_time * 4f);
                            DrawCircle(c, s * 0.46f, new Color(0.6f, 0.1f, 0.8f));
                            DrawCircle(c, s * (0.28f + 0.06f * pulse), new Color(1f, 0.75f, 1f));
                            DrawArc(c, s * 0.46f, 0, Mathf.Tau, 32, new Color(1f, 0.5f, 1f), 3f);
                        }
                    }
                    else
                    {
                        if (!TryDrawTexture("exit_closed", r) && !TryDrawTexture("exit", r, new Color(0.35f, 0.35f, 0.35f)))
                        {
                            // Sealed gate.
                            DrawCircle(c, s * 0.46f, new Color(0.18f, 0.04f, 0.04f));
                            DrawArc(c, s * 0.46f, 0, Mathf.Tau, 32, new Color(0.5f, 0.1f, 0.1f), 3f);
                            DrawLine(c + new Vector2(-s * 0.2f, -s * 0.2f), c + new Vector2(s * 0.2f, s * 0.2f), new Color(0.5f, 0.1f, 0.1f), 3f);
                            DrawLine(c + new Vector2(s * 0.2f, -s * 0.2f), c + new Vector2(-s * 0.2f, s * 0.2f), new Color(0.5f, 0.1f, 0.1f), 3f);
                        }
                    }
                    break;
            }
        }

        // Placeholder hell bush: a clump of crimson and purple blades.
        private void DrawBush(Rect2 r, float alpha, float shake)
        {
            float s = r.Size.X;
            Vector2 p = r.Position + new Vector2(shake, 0);
            Color[] cols =
            {
                new Color(0.55f, 0.08f, 0.1f, alpha), new Color(0.78f, 0.15f, 0.25f, alpha),
                new Color(0.45f, 0.05f, 0.25f, alpha), new Color(0.88f, 0.28f, 0.15f, alpha),
                new Color(0.62f, 0.1f, 0.32f, alpha),
            };
            for (int i = 0; i < 5; i++)
            {
                float bx = p.X + s * (0.14f + 0.18f * i);
                float lean = (i - 2) * 0.06f * s;
                float top = p.Y + s * (0.2f + 0.1f * ((i * 7) % 3));
                Vector2[] blade =
                {
                    new Vector2(bx - s * 0.09f, p.Y + s * 0.95f),
                    new Vector2(bx + s * 0.09f, p.Y + s * 0.95f),
                    new Vector2(bx + lean, top),
                };
                DrawColoredPolygon(blade, cols[i]);
            }
        }

        private void DrawEnemy(Rect2 r)
        {
            if (TryDrawTexture("skeleton", r))
            {
                return;
            }

            // Placeholder skeleton: pale skull, dark eye sockets, ribs.
            float s = r.Size.X;
            Vector2 c = r.Position + r.Size / 2f;
            Color bone = new Color(0.92f, 0.9f, 0.8f);
            Color dark = new Color(0.1f, 0.05f, 0.05f);
            DrawCircle(c + new Vector2(0, -s * 0.12f), s * 0.24f, bone);
            DrawRect(new Rect2(c + new Vector2(-s * 0.12f, s * 0.04f), new Vector2(s * 0.24f, s * 0.1f)), bone);
            DrawCircle(c + new Vector2(-s * 0.09f, -s * 0.14f), s * 0.06f, dark);
            DrawCircle(c + new Vector2(s * 0.09f, -s * 0.14f), s * 0.06f, dark);
            DrawLine(c + new Vector2(0, s * 0.14f), c + new Vector2(0, s * 0.4f), bone, 3f);
            for (int i = 0; i < 3; i++)
            {
                float yy = s * (0.2f + 0.07f * i);
                DrawLine(c + new Vector2(-s * 0.16f, yy), c + new Vector2(s * 0.16f, yy), bone, 2f);
            }
        }

        private void DrawPlayer(Rect2 r)
        {
            if (!TryDrawTexture("player", r))
            {
                // Placeholder: the sheep
                float s = r.Size.X;
                Rect2 cube = r.Grow(-s * 0.15f);
                DrawRect(cube, Colors.White);
                DrawRect(cube, new Color(0.6f, 0.6f, 0.65f), false, 2f);
            }
        }
    }
}
