using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public Tile top, side, bottom;

    public TilePos topPos, sidePos, bottomPos;

    public Block(Tile tile) : this(tile, tile, tile) { }

    public Block(Tile top, Tile side, Tile bottom)
    {
        this.top = top;
        this.side = side;
        this.bottom = bottom;

        GetPositions();
    }

    void GetPositions()
    {
        topPos = TilePos.tiles[top];
        sidePos = TilePos.tiles[side];
        bottomPos = TilePos.tiles[bottom];
    }

    public static Dictionary<BlockType, Block> blocks = new Dictionary<BlockType, Block>(){
        { BlockType.Grass, new Block(Tile.Grass, Tile.GrassSide, Tile.Dirt) },
        { BlockType.Dirt, new Block(Tile.Dirt) },
        { BlockType.Stone, new Block(Tile.Stone) },
        { BlockType.Log, new Block(Tile.Log, Tile.LogSide, Tile.Log) },
        { BlockType.Leaves, new Block(Tile.Leaves) },
        { BlockType.Baserock, new Block(Tile.Baserock) },
        { BlockType.Sand, new Block(Tile.Sand) },
        { BlockType.Cobblestone, new Block(Tile.Cobblestone) },
        { BlockType.Gravel, new Block(Tile.Gravel) },
    };
}

public class TilePos
{
    private readonly Vector2[] uvs;

    public TilePos(int xPos, int yPos)
    {
        uvs = new Vector2[]
        {
            new Vector2(xPos / 16f + .001f, yPos / 16f + .001f),
            new Vector2(xPos / 16f + .001f, (yPos + 1) / 16f - .001f),
            new Vector2((xPos + 1) / 16f - .001f, (yPos + 1) / 16f - .001f),
            new Vector2((xPos + 1) / 16f - .001f, yPos / 16f + .001f),
        };
    }

    public Vector2[] GetUVs() => uvs;

    public static Dictionary<Tile, TilePos> tiles = new Dictionary<Tile, TilePos>()
    {
        { Tile.Grass, new TilePos(0, 1) },
        { Tile.Stone, new TilePos(1, 1) },
        { Tile.Dirt, new TilePos(2, 1) },
        { Tile.GrassSide, new TilePos(3, 1) },
        { Tile.LogSide, new TilePos(4, 1) },
        { Tile.Log, new TilePos(5, 1) },
        { Tile.Cobblestone, new TilePos(0, 0) },
        { Tile.Baserock, new TilePos(1, 0) },
        { Tile.Sand, new TilePos(2, 0) },
        { Tile.Gravel, new TilePos(3, 0) },
        { Tile.Leaves, new TilePos(4, 0) },
    };
}

public enum Tile
{
    Grass, Stone, Dirt, GrassSide, LogSide, Log,
    Cobblestone, Baserock, Sand, Gravel, Leaves
}