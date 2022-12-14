using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    /// <summary>
    /// The map is has a set width and infinite height.
    /// It is made from elementary building segments that are stacked on top of each other and span the entire width.
    /// This class takes care of loading the segments from files, appending them to the map and spawing enemies, collectibles and other objects.
    /// </summary>
    public class LevelGenerator
    {
        /// <summary>
        /// A segment is a 2D array of characters and can specify which segment group can follow (not currently used beyond the first segment).
        /// </summary>
        private struct Segment
        {
            /// <summary>The segment data.</summary>
            /// <seealso cref="DiamondHollow.TileType"/>
            public string[] Data;
            /// <summary>The segment group that can follow this segment.</summary>
            public string Next;

            /// <summary>
            /// Creates a new segment.
            /// </summary>
            /// <param name="data">The segment data.</param>
            /// <param name="next">The segment group that can follow this segment.</param>
            public Segment(string[] data, string next)
            {
                Data = data;
                Next = next;
            }
        }

        /// <summary>
        /// The game this level generator is associated with.
        /// </summary>
        public DiamondHollowGame Game;
        /// <summary>
        /// The level this level generator is associated with.
        /// </summary>
        public Level Level;
        /// <summary>
        /// The level segment file root directory.
        /// </summary>
        public string RootDirectory;

        /// <summary>
        /// The current map height.
        /// </summary>
        private int Height;
        /// <summary>
        /// The id of the following segment group.
        /// </summary>
        private string NextConnector;
        /// <summary>
        /// A dictionary of segment groups.
        /// </summary>
        private readonly Dictionary<string, List<Segment>> Segments;

        /// <summary>
        /// The distance to the next checkpoint.
        /// </summary>
        private float NextCheckpoint;

        /// <summary>
        /// Create a new level generator.
        /// </summary>
        /// <param name="game">The game this level generator is associated with.</param>
        /// <param name="level">The level this level generator is associated with.</param>
        /// <param name="path">The path to the level segment file root directory.</param>
        /// <remarks>
        /// Level segment filenames follow the pattern <c>"[name]\_[before]\_[after].txt"</c>
        /// - <c>[name]</c> is a name for the map creator (not used)
        /// - <c>[before]</c> is an identifier which has to match the <c>[after]</c> identifier of the segment that comes before it
        /// </remarks>
        public LevelGenerator(DiamondHollowGame game, Level level, string path)
        {
            Game = game;
            Level = level;
            RootDirectory = Path.Combine(Game.Content.RootDirectory, path);     // Path to level segments
            Height = 0;
            Segments = new();

            foreach (var file in Directory.GetFiles(RootDirectory))
            {
                // Level segment filenames follow the pattern "<name>_<before>_<after>.txt"
                //     <name> is a name for the map creator (not used)
                //     <before> is an identifier which has to match the <after> identifier of the segment that comes before it
                string[] data = File.ReadAllLines(file);
                string[] parts = Path.GetFileNameWithoutExtension(file).Split('_');
                if (!Segments.ContainsKey(parts[1])) Segments[parts[1]] = new();
                Segments[parts[1]].Add(new Segment(data, parts[2]));
            }
            NextConnector = "a";    // The first segment will have a before identifier of "a"
            ResetCheckpoint(0);
        }

        private void SpawnDiamonds(params (float x, float y)[] diamonds)
        {
            foreach (var (x, y) in diamonds)
            {
                Level.CollectiblesController.SpawnDiamond(new Vector2(x + 0.5f, y + 0.5f).FromGrid().ToPoint());
            }
        }

        /// <summary>
        /// Level components contain pacements for potential checkpoints but not all of them will be used.
        /// As the difficulty increases, the distance between checkpoints increases.
        /// This method calculates the minimum distance to the next checkpoint.
        /// </summary>
        /// <param name="y">The height of the map.</param>
        private void ResetCheckpoint(int y) => NextCheckpoint = y + (1 + Game.Random() * (float)Math.Sqrt(Level.Difficulty)) * 50;

        /// <summary>
        /// Load a random level segment and process it.
        /// </summary>
        /// <param name="grid">The grid to append the segment to.</param>
        /// <remarks>
        /// Individual tiles are evaluated here, platforms are placed and enemies/collectibles are spawned.
        /// Some enemies only appear after a certain difficulty is reached, sometimes more of the same enemy appear even later on.
        /// Some collectibles only have a certain chance of spawning.
        /// </remarks>
        public void LoadNext(ref TileType[,] grid)
        {
            // Choose a segment
            var segment = Game.Choice(Segments[NextConnector]);
            string[] data = segment.Data;
            if (Game.Chance(0.5f)) data = data.Select(e => new String(e.ToCharArray().Reverse().ToArray())).ToArray();

            int Width = data[0].Length;
            Helpers.ResizeArray(ref grid, Height + data.Length, Width);     // Fast extend of the map

            for (int y = Height; y < Height + data.Length; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    // Individual tiles are evaluated here, platforms are placed and enemies/collectibles are spawned
                    // Some enemies only appear after a certain difficulty is reached, sometimes more of the same enemy appear even later on
                    // Some collectibles only have a certain chance of spawning
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
                        case TileType.DoubleGem:
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
                        case TileType.Nuke when Game.Chance(0.5f * Level.Modifier):
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
        }
    }
}