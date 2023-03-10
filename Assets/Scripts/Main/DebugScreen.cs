using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugScreen : MonoBehaviour
{
    World _world;
    TMP_Text _text;

    float _frameRate;
    float _timer;

    int _halfWorldSizeInVoxels;
    int _halfWorldSizeInChunks;

    private void Start()
    {
        _world = GameObject.FindGameObjectWithTag("World").GetComponent<World>();
        _text = GetComponent<TMP_Text>();

        _halfWorldSizeInVoxels = (int)((VoxelData.WorldWidthInVoxels*VoxelData.VoxelSize) / 2);
        _halfWorldSizeInChunks = VoxelData.WorldWidthInChunks / 2;
    }
    private void Update()
    {
        string debugText = "YJS GameDev Voxel Game";
        debugText += "\n";
        debugText += _frameRate + " - FPS";
        debugText += "\n\n";
        debugText += "XYZ: " + (Mathf.FloorToInt(_world.Player.transform.position.x)-_halfWorldSizeInVoxels)+ " / " + Mathf.FloorToInt(_world.Player.transform.position.y) + " / " + (Mathf.FloorToInt(_world.Player.transform.position.z) - _halfWorldSizeInVoxels);
        debugText += "\n";
        debugText += "Chunk: " + (_world.PlayerChunkCoord.X-_halfWorldSizeInChunks) + " / " + (_world.PlayerChunkCoord.Z- _halfWorldSizeInChunks);

        _text.text = debugText;
        if(_timer > 1f)
        {
            _frameRate = (int)(1f / Time.unscaledDeltaTime);
            _timer = 0f;
        }
        else
        {
            _timer += Time.deltaTime;
        }
    }
}
