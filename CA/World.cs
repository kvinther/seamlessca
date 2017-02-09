using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace CA
{
    public class World
    {
        public int CellSize { get; set; } = 5;
        public int ChunkSizeX { get; set; } = 30;
        public int ChunkSizeY { get; set; } = 30;
        public int Generations { get; set; } = 5;
        public int Threshold { get; set; } = 5;
        public double RockPercentage { get; set; } = 0.5;
        public int MovementFactor { get; set; } = 5;
        public int ViewPortSize { get; set; } = 20;
        public int WorldSeed { get; set; } = 42;

        public Dictionary<string, Chunk> Chunks { get; }

        public World()
        {
            Chunks = new Dictionary<string, Chunk>();
        }

        public ViewPort RenderViewPort(Point location)
        {
            var viewPort = new ViewPort(location.X, location.Y, location.X + ViewPortSize - 1, location.Y + ViewPortSize - 1);
            GetView(viewPort);
            return viewPort;
        }

        static void Main(string[] args)
        {
            ConsoleKeyInfo key;

            var location = new Point(0, 0);
            var world = new World();
            var drawGrid = false;

            do
            {
                Console.Clear();
                int xMin = location.X - 0,
                    xMax = location.X + world.ChunkSizeX - 1,
                    yMin = location.Y - 0,
                    yMax = location.Y + world.ChunkSizeY - 1;

                var viewPort = new ViewPort(xMin, yMin, xMax, yMax);

                var view = world.GetView(viewPort);
                for (var x = 0; x < (1 + xMax - xMin); x++)
                {
                    for (var y = 0; y < (1 + yMax - yMin); y++)
                    {
                        var cell = view[y, x];
                        var color = Console.ForegroundColor;
                        Console.ForegroundColor = cell == 1
                            ? ConsoleColor.DarkBlue
                            : ConsoleColor.White;
                        Console.Write("█");
                        Console.ForegroundColor = color;
                    }
                    Console.WriteLine();
                }

                world.DrawImage(viewPort, drawGrid);

                Console.WriteLine("Number of chunks: " + world.Chunks.Keys.Count);
                Console.WriteLine($"Location: ({location.X}, {location.Y})");
                Console.WriteLine("R: reset\tG: grid on/off\tQ: quit");
                key = Console.ReadKey();
                if (key.Key == ConsoleKey.LeftArrow)
                    location.X = location.X - world.MovementFactor;
                if (key.Key == ConsoleKey.RightArrow)
                    location.X = location.X + world.MovementFactor;
                if (key.Key == ConsoleKey.UpArrow)
                    location.Y = location.Y - world.MovementFactor;
                if (key.Key == ConsoleKey.DownArrow)
                    location.Y = location.Y + world.MovementFactor;
                if (key.Key == ConsoleKey.R)
                    world = new World();
                if (key.Key == ConsoleKey.G)
                    drawGrid = !drawGrid;

            } while (key.Key != ConsoleKey.Q);
        }

        /// <summary>
        /// Given a viewport, creates an image of the world and saves it to disc.
        /// </summary>
        public Image DrawImage(ViewPort viewPort,
            bool drawEdges = true,
            bool drawGrid = false,
            bool drawView = true,
            bool drawEdgesAsNoise = false,
            bool drawFinalAsNoise = false,
            bool colorizeInsideChunks = true)
        {
            var chunks = Chunks.Values.ToList();

            if (!drawEdges)
                chunks = chunks.Where(x => x.Type != Chunk.ChunkType.Edge).ToList();

            // Determine image size.
            var xMin = chunks.Min(x => x.PosX);
            var xMax = chunks.Max(x => x.PosX);
            var xDim = (1 + xMax - xMin) * ChunkSizeX * CellSize;
            var yMin = chunks.Min(x => x.PosY);
            var yMax = chunks.Max(x => x.PosY);
            var yDim = (1 + yMax - yMin) * ChunkSizeX * CellSize;

            var image = new Bitmap(xDim, yDim);

            using (var g = Graphics.FromImage(image))
            {
                foreach (var chunk in chunks)
                {
                    var chunkXRelative = chunk.PosX - xMin;
                    var xTopLeft = chunkXRelative * ChunkSizeX * CellSize;

                    var chunkYRelative = chunk.PosY - yMin;
                    var yTopLeft = chunkYRelative * ChunkSizeY * CellSize;

                    var solidColor = Brushes.DarkGray;
                    var openColor = Brushes.DimGray;

                    for (var x = 0; x < chunk.SizeX; x++)
                    {
                        for (var y = 0; y < chunk.SizeY; y++)
                        {
                            Brush brush;
                            int block;
                            if (chunk.IsFinal)
                            {
                                block = drawFinalAsNoise
                                    ? chunk.Noise[x, y]
                                    : chunk.Final[x, y];
                                brush = block == 1
                                    ? (colorizeInsideChunks ? Brushes.ForestGreen : solidColor)
                                    : (colorizeInsideChunks ? Brushes.SaddleBrown : openColor);
                            }
                            else
                            {
                                block = drawEdgesAsNoise
                                    ? chunk.Noise[x, y]
                                    : chunk.Cells[x, y];
                                brush = block == 1
                                    ? solidColor
                                    : openColor;
                            }

                            g.FillRectangle(brush, xTopLeft + x * CellSize, yTopLeft + y * CellSize, CellSize, CellSize);
                        }
                    }
                }

                if (drawGrid)
                {
                    foreach (var chunk in chunks.OrderBy(x => x.Type))
                    {
                        var chunkXRelative = chunk.PosX - xMin;
                        var xTopLeft = chunkXRelative * ChunkSizeX * CellSize;

                        var chunkYRelative = chunk.PosY - yMin;
                        var yTopLeft = chunkYRelative * ChunkSizeY * CellSize;

                        var color = chunk.Type == Chunk.ChunkType.Edge
                            ? Color.Red
                            : Color.Yellow;

                        var pen = new Pen(color, 1.5f);

                        g.DrawRectangle(pen, xTopLeft, yTopLeft, ChunkSizeX * CellSize - 1, ChunkSizeY * CellSize - 1);
                    }
                }


                // Draw viewport.
                if (drawView)
                {
                    // Calculate viewport
                    // Determine which chunks are (partially or completely) within the bounding box of the view
                    var xTranslation = Math.Abs(Math.Min(0, xMin) * ChunkSizeX * CellSize);
                    var yTranslation = Math.Abs(Math.Min(0, yMin) * ChunkSizeY * CellSize);
                    Console.WriteLine($"viewport: {viewPort}");
                    viewPort.Grow(CellSize, CellSize);
                    Console.WriteLine($"viewport-grown: {viewPort}");
                    viewPort.Translate(xTranslation, yTranslation);
                    Console.WriteLine($"viewport-translated: {viewPort}");

                    var rect = viewPort.GetRectangle();
                    g.DrawRectangle(new Pen(Color.Fuchsia, 2*CellSize), rect);
                }
            }

            return image;
        }

        /// <summary>
        /// Given a set of world coordinates, returns an array of cells. Evolves and adds chunks as necessary.
        /// </summary>
        private int[,] GetView(ViewPort viewPort)
        {
            // Determine which chunks are (partially or completely) within the bounding box of the view
            var lower = ConvertCellWorldCoordinateToChunkCoordinate(viewPort.XMin, viewPort.YMin);
            var upper = ConvertCellWorldCoordinateToChunkCoordinate(viewPort.XMax, viewPort.YMax);

            // If chunks overlapped by view are not 'inside', they are going to get evolved
            var evolveList = new List<Chunk>();
            for (var posX = lower.X; posX <= upper.X; posX++)
            {
                for (var posY = lower.Y; posY <= upper.Y; posY++)
                {
                    // Get chunk, if it does not exist: create it
                    var key = GetChunkKey(posX, posY);
                    Chunk chunk;
                    if (!Chunks.TryGetValue(key, out chunk))
                    {
                        chunk = new Chunk(this, ChunkSizeX, ChunkSizeY, Generations, Threshold, posX, posY, RockPercentage);
                        Chunks[key] = chunk;
                    }

                    if (chunk.Type == Chunk.ChunkType.Edge)
                    {
                        chunk.Type = Chunk.ChunkType.Inside;
                        evolveList.Add(chunk);
                        // Get the neighboring chunks
                        var added = AddAdjacentChunks(posX, posY);
                        evolveList.AddRange(added);
                    }
                }
            }

            // Clean up list (remove duplicates)
            evolveList = evolveList.Distinct().ToList();

            if (evolveList.Count > 0)
            {
                Trace.WriteLine($"EvolveList.Count: {evolveList.Count}");
            }

            RunEvolution(evolveList);

            // Mark 'inside' chunks as final
            foreach (var chunk in Chunks.Values)
            {
                if (chunk.Type == Chunk.ChunkType.Inside && !chunk.IsFinal)
                {
                    chunk.MarkAsFinal();
                }
            }

            // Determine size of view, create array
            var viewSizeX = viewPort.XMax - viewPort.XMin + 1;
            var viewSizeY = viewPort.YMax - viewPort.YMin + 1;
            var view = new int[viewSizeX, viewSizeY];

            // Copy cells from chunks into array
            for (var x = viewPort.XMin; x <= viewPort.XMax; x++)
            {
                for (var y = viewPort.YMin; y <= viewPort.YMax; y++)
                {
                    view[x - viewPort.XMin, y - viewPort.YMin] = GetFinalCell(x, y);
                }
            }

            return view;
        }

        /// <summary>
        /// Evolves a list of chunks, storing the result in their Cells array.
        /// </summary>
        /// <param name="evolveList"></param>
        private void RunEvolution(List<Chunk> evolveList)
        {
            int maxGenerations = 0;

            // Initialize Cells array with random noise
            foreach (var chunk in evolveList)
            {
                chunk.Cells = chunk.GenerateNoise();
                maxGenerations = Math.Max(maxGenerations, chunk.Generations);
            }

            // Evolve edge chunks
            int generation = 0;
            while (generation < maxGenerations)
            {
                // Evolve chunks one generation and store in its local Temp array
                foreach (var chunk in evolveList)
                {
                    chunk.EvolveChunk(generation);
                }

                // Swap local temp into Cells property
                foreach (var chunk in evolveList)
                {
                    chunk.SwapEvolvedChunk();
                }

                generation++;
            }
        }

        /// <summary>
        /// Given a set of chunk coordinates, creates up to 8 'edge' chunks surrounding it and adds them to the World.
        /// </summary>
        private List<Chunk> AddAdjacentChunks(int xChunk, int yChunk)
        {
            var list = new List<Chunk>();

            for (var x = xChunk - 1; x <= xChunk + 1; x++)
            {
                for (var y = yChunk - 1; y <= yChunk + 1; y++)
                {
                    // Add chunk to list. If it does not exist, create it and then add it.
                    var key = GetChunkKey(x, y);
                    if (!Chunks.ContainsKey(key))
                    {
                        Chunks.Add(key, new Chunk(this, ChunkSizeX, ChunkSizeY, Generations, Threshold, x, y, RockPercentage));
                    }
                    list.Add(Chunks[key]);
                }
            }

            return list;
        }

        /// <summary>
        /// Given a world coordinate, returns that block. If world coordinate outside chunks, return 1.
        /// </summary>
        public int GetCell(int x, int y)
        {
            var chunk = GetChunkFromWorldCellCoordinate(x, y);

            // Always return "1" if we ask out of bounds.
            if (chunk == null)
            {
                return 1;
            }
            return chunk.GetAbsoluteCell(x, y);
        }

        /// <summary>
        /// Returns the cell value of a world coordinate, or if the coordinate is outside the generated world, return the value 1.
        /// </summary>
        public int GetFinalCell(int x, int y)
        {
            var chunk = GetChunkFromWorldCellCoordinate(x, y);
            return chunk.GetAbsoluteFinalCell(x, y);
        }

        /// <summary>
        /// Given a world coordinate, returns the chunk in which that coordinate resides, or null of no such chunk exists.
        /// </summary>
        public Chunk GetChunkFromWorldCellCoordinate(int x, int y)
        {
            var chunkCoordinate = ConvertCellWorldCoordinateToChunkCoordinate(x, y);

            var key = GetChunkKey(chunkCoordinate.X, chunkCoordinate.Y);

            if (!Chunks.ContainsKey(key))
                return null;

            return Chunks[key];
        }

        /// <summary>
        /// Given the world coordinate of a cell, return the coordinate of the chunk in which that cell resides.
        /// </summary>
        public Point ConvertCellWorldCoordinateToChunkCoordinate(int x, int y)
        {
            var xChunk = x < 0
                ? (x + 1) / ChunkSizeX - 1
                : x / ChunkSizeX;

            var yChunk = y < 0
                ? (y + 1) / ChunkSizeY - 1
                : y / ChunkSizeY;


            return new Point(xChunk, yChunk);
        }

        /// <summary>
        /// Creates a string of the binary X/Y numbers of a coordinate.
        /// </summary>
        public static string GetChunkKey(int x, int y)
        {
            return $"({x}, {y})";
        }
    }
}
