using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ToolBar : MonoBehaviour
{
    #region Variables
    private float _scrollInput = 0;
    private VoxelWorldGenerator _inputActions;
    private PlayerInput _playerInput;

    private World _world;
    public Player Player;
    public RectTransform Highlight;
    public ItemSlot[] ItemSlots;

    private int _slotIndex = 0;
    #endregion

    private void Start()
    {
        _world = GameObject.FindGameObjectWithTag("World").GetComponent<World>();
        _inputActions = new VoxelWorldGenerator();
        _inputActions.Enable();
        foreach (ItemSlot slot in ItemSlots)
        {
            slot.Icon.sprite = _world.VoxelTypes[slot.ItemID].icon;
            slot.Icon.enabled = true;
        }
        Player.SelectedBlockIndex = ItemSlots[_slotIndex].ItemID;
    }

    private void Update()
    {
        _scrollInput = _inputActions.Player.Scroll.ReadValue<float>();
        if (_scrollInput != 0)
        {
            if (_scrollInput > 0)
            {
                _slotIndex--;
            }
            else
            {
                _slotIndex++;
            }
            if (_slotIndex > (byte)ItemSlots.Length - 1)
            {
                _slotIndex = 0;
            }
            else if (_slotIndex < 0)
            {
                _slotIndex = (byte)(ItemSlots.Length - 1);
            }
            Highlight.position = ItemSlots[_slotIndex].Icon.transform.position;
            Player.SelectedBlockIndex = ItemSlots[_slotIndex].ItemID;
        }
    }
}

[System.Serializable]
public class ItemSlot
{
    public byte ItemID;
    public Image Icon;
}
