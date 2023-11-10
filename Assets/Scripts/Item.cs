using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Item
{
    public int iconID;
    public ItemType[] types;

}


public enum ItemType {
    NONE, MAINHAND, OFFHAND, ARMOR, TOOL, TRINKET
}
