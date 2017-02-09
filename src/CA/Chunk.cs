using System;
using System.CodeDom;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace CA
{
    public class Chunk
    {
        public static int MaxId;
        public int Id { get; }

        public int SizeX { get; }
        public int SizeY { get; }
        public int Generations { get; }
        public int Threshold { get; }
        public int PosX { get; }
        public int PosY { get; }
        public double RockPercentage { get; }

        public int[,] Cells { get; set; }
        public int[,] Final { get; set; }
        public int[,] Noise { get; set; }
        public int[,] Temp { get; set; }
        public ChunkType Type { get; set; }

        public World World { get; }
        public bool IsFinal { get; set; }

        public Chunk(World world, int x, int y, int gen, int threshold, int posX, int posY, double rockPercentage)
        {
            Id = MaxId++;
            Trace.WriteLine($"Created chunk [{Id}]: ({posX}, {posY})");
            World = world;
            SizeX = x;
            SizeY = y;
            Generations = gen;
            Threshold = threshold;
            PosX = posX;
            PosY = posY;
            RockPercentage = rockPercentage;

            Cells = GenerateNoise();
            Noise = GenerateNoise();
            Type = ChunkType.Edge;
        }


        /// <summary>
        /// Return and array of random cell values based on position of chunk in world.
        /// </summary>
        public int[,] GenerateNoise()
        {
            var key = World.GetChunkKey(PosX, PosY);
            var seed = key.GetHashCode() + World.WorldSeed;
            var rnd = new Random(seed);
            var noise = new int[SizeX,SizeY];

            for(var xCell = 0; xCell < SizeX; xCell++)
            {
                for (var yCell = 0; yCell < SizeY; yCell++)
                {
                    noise[xCell, yCell] = rnd.NextDouble() < RockPercentage ? 0 : 1;
                }
            }

            // Demo - cut a cross road into the noise.
            //const int width = 1;

            //var xOffset = SizeX / 2 - width / 2;
            //var yOffset = SizeY / 2 - width / 2;

            //// Cut x-axis.
            //for (var x = 0; x < SizeX; x++)
            //    for (var y = yOffset; y < yOffset + width; y++)
            //        noise[x, y] = 0;

            //// Cut y-axis.
            //for (var y = 0; y < SizeY; y++)
            //    for (var x = xOffset; x < xOffset + width; x++)
            //        noise[x, y] = 0;

            return noise;
        } 

        /// <summary>
        /// Given a world coordinate, returns that cell from Cells array.
        /// </summary>
        public int GetAbsoluteCell(int x, int y)
        {
            var localX = x - (PosX*SizeX);
            var localY = y - (PosY*SizeY);
            
            return Cells[localX, localY];
        }

        /// <summary>
        /// Given a world coordinate, returns the cell from Final array.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int GetAbsoluteFinalCell(int x, int y)
        {
            var localX = x - (PosX * SizeX);
            var localY = y - (PosY * SizeY);

            return Final[localX, localY];
        }
        
        /// <summary>
        /// Evolve each cell in the chunk and store them in the Temp property.
        /// </summary>
        public void EvolveChunk(int generation)
        {
            // Ensure chunk only passes through as many evolutions as required
            if (generation < Generations)
            {
                Temp = new int[SizeX, SizeY];

                foreach (var x in Enumerable.Range(0, SizeX))
                {
                    foreach (var y in Enumerable.Range(0, SizeY))
                    {
                        Temp[x, y] = EvolveCell(x + (PosX * SizeX), y + (PosY * SizeY));
                    }
                }
            }
        }
        
        /// <summary>
        /// Swaps the evolved Temp property with the Cells property of the chunk.
        /// </summary>
        public void SwapEvolvedChunk()
        {
            Cells = Temp;
        }

        /// <summary>
        /// Based on the value of itself and its 8 neighbor cells, returns the evolved value of a cell.
        /// </summary>
        public int EvolveCell(int x, int y)
        {
            var count = 0;
            for (var xCell = -1; xCell <= 1; xCell++)
            {
                for (var yCell = -1; yCell <= 1; yCell++)
                {
                    count += World.GetCell(xCell + x, yCell + y);
                }
            }

            return count / Threshold;
        }

        /// <summary>
        /// Marks the chunk as being fully evolved, and copies the contents of its Cells property into its Final property.
        /// </summary>
        public void MarkAsFinal()
        {
            IsFinal = true;
            Final = new int[SizeX, SizeY];
            foreach (var x in Enumerable.Range(0, SizeX))
            {
                foreach (var y in Enumerable.Range(0, SizeY))
                {
                    Final[x, y] = Cells[x, y];
                }
            }
        }

        /// <summary>
        /// Type of chunks. Can be either inside, meaning its cells are fully evolved and are allowed to 
        /// be returned in a view, or can be edge, meaning its cells must first go through evolution.
        /// </summary>
        public enum ChunkType
        {
            Edge, Inside
        }
    }
}
