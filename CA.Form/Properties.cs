namespace CA.Form
{
    public class CaProperties
    {
        public int CellSize { get; set; } = 4;
        public int ViewPortSize { get; set; } = 32;
        public int ChunkSize { get; set; } = 8;
        public int MovementFactor { get; set; } = 32;
        public string SaveFileFolder { get; set; } = @"images";
        public int RockPercentage { get; set; } = 50;
        public int Generations { get; set; } = 5;
        public int WorldSeed { get; set; } = 42;
    }
}
