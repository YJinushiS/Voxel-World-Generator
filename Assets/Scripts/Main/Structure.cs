using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static Queue<VoxelMod> MakeTree(Vector3 position, int minTrunkHeight, int maxTrunkHeight, NoiseSettings settings)
    {
        Queue<VoxelMod> queue = new();
        int height = (int)(maxTrunkHeight * MultyOctaveNoise.GetTreePlacementOctavePerlin(position.x, position.y, settings));
        if (height < minTrunkHeight)
            height = minTrunkHeight;
        float radius = height / 10;
        float divider = 1 / (radius*2);

        for (int y = 1; y < height; y++)
        {
            for (int x = -(int)(radius/(y*divider)); x <= (int)(radius / (y * divider)); x++)
            {
                for (int z = -(int)(radius / (y *divider)); z <= (int)(radius / (y * divider)); z++)
                {
                    if (Vector3.Distance(new Vector3(position.x + x, position.y + y, position.z + z), new Vector3(position.x, position.y + y, position.z)) <= (float)radius)
                        queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + y, position.z + z), 12));
                }
            }
        }
        queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + height, position.z), 15));
        return queue;
    }
}
