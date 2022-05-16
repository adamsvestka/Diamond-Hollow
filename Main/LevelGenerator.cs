using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public class LevelGenerator
    {
        private record struct Segment(string[] Data, string Next, int Difficulty);

        public DiamondHollowGame Game;
        public Level Level;
        public string RootDirectory;

        private int Height;
        private string NextConnector;
        private readonly Dictionary<string, List<Segment>> Segments;

        private float NextCheckpoint;

        public LevelGenerator(DiamondHollowGame game, Level level, string path)
        {
            Game = game;
            Level = level;
            RootDirectory = Path.Combine(Game.Content.RootDirectory, path);
            Height = 0;
            Segments = new();

            foreach (var file in Directory.GetFiles(RootDirectory))
            {
                string[] data = File.ReadAllLines(file);
                string[] parts = Path.GetFileNameWithoutExtension(file).Split('_');
                if (!Segments.ContainsKey(parts[1])) Segments[parts[1]] = new();
                Segments[parts[1]].Add(new Segment(data, parts[2], 0));
            }
            NextConnector = "a";
            ResetCheckpoint(0);
        }

        private void SpawnDiamonds(params (float x, float y)[] diamonds)
        {
            foreach (var (x, y) in diamonds)
            {
                Level.CollectiblesController.SpawnDiamond(new Vector2(x + 0.5f, y + 0.5f).FromGrid().ToPoint());
            }
        }

        private void ResetCheckpoint(int y) => NextCheckpoint = y + (1 + Game.Random() * (float)Math.Sqrt(Level.Difficulty)) * 50;

        public void LoadNext(ref TileType[,] oldGrid)
        {
            var segment = Game.Choice(Segments[NextConnector]);
            string[] data = segment.Data;
            if (Game.Chance(0.5f)) data = data.Select(e => new String(e.ToCharArray().Reverse().ToArray())).ToArray();

            // if (oldGrid.Length > 0) return;
            // data = File.ReadAllLines(Path.Combine(Game.Content.RootDirectory, "Levels/Level2.txt"));

            int Width = data[0].Length;
            TileType[,] grid = new TileType[Height + data.Length, Width];

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    grid[y, x] = oldGrid[y, x];

            for (int y = Height; y < Height + data.Length; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var tile = (TileType)data[Height + data.Length - y - 1][x];
                    Point pos = new Vector2(x + 0.5f, y + 0.5f).FromGrid().ToPoint();
                    grid[y, x] = TileType.Empty;
                    switch (tile)
                    {
                        case TileType.Wall:
                            grid[y, x] = tile;
                            break;
                        case TileType.Spawnpoint:
                            Level.Spawnpoint = new Vector2(x + 0.5f, y + 0.5f).FromGrid().ToPoint();
                            Level.Player.Position = Level.Spawnpoint - Level.Player.Size.Half();
                            break;
                        case TileType.SingleGem:
                            SpawnDiamonds((x, y));
                            break;
                        case TileType.DounbleGem:
                            SpawnDiamonds((x - 0.5f, y), (x + 0.5f, y));
                            break;
                        case TileType.TripleGem:
                            SpawnDiamonds((x, y - 1), (x, y), (x, y + 1));
                            break;
                        case TileType.QuadrupleGem:
                            SpawnDiamonds((x - 1, y - 1), (x - 1, y + 1), (x + 1, y - 1), (x + 1, y + 1));
                            break;
                        case TileType.Slime:
                        case TileType.Slime2 when Level.Difficulty > 4:
                            Level.EnemyController.SpawnSlime(pos);
                            break;
                        case TileType.WallShooter:
                        case TileType.WallShooter2 when Level.Difficulty > 5:
                            bool right = grid[y, x - 1] == TileType.Wall;
                            int wx = right ? x * Game.TileSize + WallShooter.Size.X / 2 : (x + 1) * Game.TileSize - WallShooter.Size.X / 2;
                            Level.EnemyController.SpawnWallShooter(new Point(wx, (int)((y + 0.5f) * Game.TileSize)), right);
                            break;
                        case TileType.CeilingShooter when Level.Difficulty > 2:
                        case TileType.CeilingShooter2 when Level.Difficulty > 6:
                            int cy = (y + 1) * Game.TileSize - CeilingShooter.Size.Y / 2;
                            Level.EnemyController.SpawnCeilingShooter(new Point((int)((x + 0.5f) * Game.TileSize), cy));
                            break;
                        case TileType.Spider when Level.Difficulty > 3:
                        case TileType.Spider2 when Level.Difficulty > 7:
                            int zy = (y + 1) * Game.TileSize - Spider.Size.Y / 2;
                            Level.EnemyController.SpawnSpider(new Point((int)((x + 0.5f) * Game.TileSize), zy));
                            break;
                        case TileType.Bird:
                        case TileType.Bird2 when Level.Difficulty > 4:
                            Level.EnemyController.SpawnBird(pos);
                            break;
                        case TileType.LargeHeart when Game.Chance(0.2f):
                            Level.CollectiblesController.SpawnLargeHeart(pos);
                            break;
                        case TileType.Nuke when Game.Chance(0.4f):
                            Level.CollectiblesController.SpawnNuke(pos);
                            break;
                        case TileType.Checkpoint when y > NextCheckpoint:
                            Level.CollectiblesController.SpawnRespawnAnchor(pos);
                            ResetCheckpoint(y);
                            break;

                        case TileType.Empty:
                        default:
                            grid[y, x] = TileType.Empty;
                            break;
                    }
                }
            }

            NextConnector = segment.Next;
            Height += data.Length;
            oldGrid = grid;
        }
    }
}