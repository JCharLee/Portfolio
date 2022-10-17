using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public enum Type { Melee, Range, Armor, Potion, Etc }

    public Type type;
    public string name;
    public Sprite img;
    public int value;
}