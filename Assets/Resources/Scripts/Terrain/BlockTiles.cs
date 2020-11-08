using System.Collections.Generic;
using UnityEngine;

public class BlockTiles
{
    private readonly Tile top, side, bottom;

    public TilePos topPos, sidePos, bottomPos;

    private BlockTiles(Tile tile) : this(tile, tile, tile) { }

    private BlockTiles(Tile top, Tile side, Tile bottom)
    {
        this.top = top;
        this.side = side;
        this.bottom = bottom;

        GetPositions();
    }

    private void GetPositions()
    {
        topPos = TilePos.tiles[top];
        sidePos = TilePos.tiles[side];
        bottomPos = TilePos.tiles[bottom];
    }

    public static Dictionary<BlockType, BlockTiles> find = new Dictionary<BlockType, BlockTiles>(){
        { BlockType.Grass, new BlockTiles(Tile.Grass, Tile.GrassSide, Tile.Dirt) },
        { BlockType.Dirt, new BlockTiles(Tile.Dirt) },
        { BlockType.Stone, new BlockTiles(Tile.Stone) },
        { BlockType.Log, new BlockTiles(Tile.Log, Tile.LogSide, Tile.Log) },
        { BlockType.Leaves, new BlockTiles(Tile.Leaves) },
        { BlockType.Baserock, new BlockTiles(Tile.Baserock) },
        { BlockType.Sand, new BlockTiles(Tile.Sand) },
        { BlockType.Cobblestone, new BlockTiles(Tile.Cobblestone) },
        { BlockType.Gravel, new BlockTiles(Tile.Gravel) },
    };
}

public class TilePos
{
    private readonly Vector2[] uvs;

    private TilePos(int xPos, int yPos)
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