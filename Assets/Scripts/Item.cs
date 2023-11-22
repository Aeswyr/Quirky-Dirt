using System;

[Serializable] public struct Item
{
    public int iconID;
    public ItemType[] types;
    public int[] attackIDs;

}


public enum ItemType {
    NONE, MAINHAND, OFFHAND, ARMOR, TOOL, TRINKET
}
