using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static Queue<VoxelMod> MakeTree(Vector3 position, int minTrunkHeight, int maxTrunkHeight, NoiseSettings settings)
    {
        Queue<VoxelMod> queue = new();
        int height = (int)(maxTrunkHeight * MultyOctaveNoise.GetTreePlacementOctavePerlin(position.x, position.y, settings));
        if(height < minTrunkHeight)
            height = minTrunkHeight;

        for(int i = 1; i < height; i++)
        {
            queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), 12));
        }
        queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + height, position.z), 15));
        return queue;
    }
}
