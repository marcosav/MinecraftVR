using System;
using System.Collections.Generic;

public enum BlockType : byte
{

    Air = 0,
    Stone = 1,
    Grass = 2,
    Dirt = 3,
    Baserock = 4,
    Log = 5,
    Leaves = 6,
    Sand = 7,
    Gravel = 8,
    Cobblestone = 9
}

public class Block
{
    private static Dictionary<BlockType, string> Init()
    {
        _name = new Dictionary<BlockType, string>();
        foreach (var n in Enum.GetNames(typeof(BlockType)))
            _name.Add((BlockType)Enum.Parse(typeof(BlockType), n), n);
        return _name;
    }

    private static Dictionary<BlockType, string> _name;

    public static Dictionary<BlockType, string> Name { get => _name ?? Init(); set { } }
}