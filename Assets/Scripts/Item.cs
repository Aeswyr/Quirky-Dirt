using System;
using UnityEngine;

[Serializable] public struct Item
{
    [SerializeField] private string name;
    public int iconID;
    public ItemType[] types;
    public int[] attackIDs;

}


public enum ItemType {
    NONE, MAINHAND, OFFHAND, ARMOR, TOOL, TRINKET
}
