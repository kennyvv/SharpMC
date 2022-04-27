﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Numerics;
using SharpMC.API.Entities;

namespace SharpMC.World.Common
{
    public abstract class WorldProviderBase
    {
        public IEnumerable<IChunkColumn> GenerateChunks(int viewDistance,
            List<Tuple<int, int>> chunksUsed, IPlayer player)
        {
            lock (chunksUsed)
            {
                var newOrders = new Dictionary<Tuple<int, int>, double>();
                var radiusSquared = viewDistance / Math.PI;
                var radius = Math.Ceiling(Math.Sqrt(radiusSquared));
                var centerX = (int) player.KnownPosition.X >> 4;
                var centerZ = (int) player.KnownPosition.Z >> 4;

                for (var x = -radius; x <= radius; ++x)
                {
                    for (var z = -radius; z <= radius; ++z)
                    {
                        var distance = x * x + z * z;
                        if (distance > radiusSquared)
                        {
                            continue;
                        }
                        var chunkX = (int) (x + centerX);
                        var chunkZ = (int) (z + centerZ);
                        var index = new Tuple<int, int>(chunkX, chunkZ);
                        newOrders[index] = distance;
                    }
                }

                if (newOrders.Count > viewDistance)
                {
                    foreach (var pair in newOrders.OrderByDescending(pair => pair.Value))
                    {
                        if (newOrders.Count <= viewDistance) break;
                        newOrders.Remove(pair.Key);
                    }
                }

                foreach (var chunkKey in chunksUsed.ToArray())
                {
                    if (!newOrders.ContainsKey(chunkKey))
                    {
                        chunksUsed.Remove(chunkKey);
                        new Task(() => player.UnloadChunk(chunkKey.Item1, chunkKey.Item2)).Start();
                    }
                }

                foreach (var pair in newOrders.OrderBy(pair => pair.Value))
                {
                    if (chunksUsed.Contains(pair.Key)) continue;

                    var chunk = GenerateChunkColumn(new Vector2(pair.Key.Item1, pair.Key.Item2));
                    chunksUsed.Add(pair.Key);

                    yield return chunk;
                }
            }
        }

        protected abstract IChunkColumn GenerateChunkColumn(Vector2 vector);
    }
}