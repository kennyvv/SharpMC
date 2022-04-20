using SharpMC.Players;
using SharpMC.Util;

namespace SharpMC.World.Generators
{
    public interface IWorldGenerator
    {
        ChunkColumn GenerateChunkColumn(ChunkCoordinates coordinates);

        PlayerLocation GetSpawnPoint();
    }
}