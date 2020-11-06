using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class TerrainPersistor : MonoBehaviour
{

    public const string FOLDER_NAME = "World";
    public const string WORLD_PROPERTIES = "info.properties";
    private const int CACHE_SIZE = 222;

    private string path = FOLDER_NAME;
    private string infoFilePath;

    private int seed = -1;

    private readonly LinkedList<ChunkPos> cache = new LinkedList<ChunkPos>();

    private readonly Dictionary<ChunkPos, BlockDistribution> chunkBlocks = new Dictionary<ChunkPos, BlockDistribution>();

    public void Init()
    {
#if !UNITY_EDITOR
        path = Application.persistentDataPath + "/" + FOLDER_NAME;
#endif

        infoFilePath = path + "/" + WORLD_PROPERTIES;

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        InitPropertiesFile();

        Task.Factory.StartNew(() =>
        {
            while (true)
            {
                Thread.Sleep(60000);
                Save();
            }
        });
    }

    public int GetSeed()
    {
        if (seed == -1)
            seed = UnityEngine.Random.Range(0, int.MaxValue);

        return seed;
    }

    private void InitPropertiesFile()
    {
        if (!File.Exists(infoFilePath))
        {
            File.WriteAllLines(infoFilePath, new string[] { "Seed=" + GetSeed() });
        }
        else
        {
            var props = new Dictionary<string, string>();
            foreach (var row in File.ReadAllLines(infoFilePath))
                props.Add(row.Split('=')[0], string.Join("=", row.Split('=').Skip(1).ToArray()));

            try
            {
                seed = int.Parse(props["Seed"]);
            }
            catch
            {
                seed = GetSeed();
            }
        }
    }

    private string GetChunkFile(ChunkPos pos) => String.Format("chunk{0}_{1}.data", pos.x, pos.z);

    public BlockDistribution GetBlocksFor(ChunkPos pos, Func<ChunkPos, BlockDistribution> Generator)
    {
        BlockDistribution blocks;

        if (!chunkBlocks.ContainsKey(pos))
        {
            var read = Read(GetChunkFile(pos));

            if (read == null)
            {
                blocks = Generator(pos);
                SaveChunk(pos, blocks);
            }
            else
                blocks = BytesToBlocks(read.bytes);

            CacheChunk(pos, blocks);
        }
        else
        {
            UpdateCached(pos);
            blocks = chunkBlocks[pos];
        }

        return blocks;
    }

    private void UpdateCached(ChunkPos pos)
    {
        cache.Remove(pos);
        cache.AddFirst(pos);
    }

    private void CacheChunk(ChunkPos pos, BlockDistribution blocks)
    {
        cache.AddFirst(pos);
        chunkBlocks.Add(pos, blocks);

        if (cache.Count > CACHE_SIZE)
        {
            SaveChunk(pos, chunkBlocks[pos]);

            var toRemove = cache.Last.Value;
            chunkBlocks.Remove(toRemove);
            cache.RemoveLast();
        }
    }

    public void Save()
    {
        foreach (var c in chunkBlocks)
            SaveChunk(c.Key, c.Value);
    }

    private void SaveChunk(ChunkPos pos, BlockDistribution blocks)
    {
        if (blocks == null)
            throw new ApplicationException("Chunk not loaded");

        if (!blocks.Dirty)
            return;

        blocks.Dirty = false;
        Save(GetChunkFile(pos), BlocksToBytes(blocks));
    }

    private Action<Stream> BlocksToBytes(BlockDistribution blocks) => str =>
        {
            foreach (BlockType b in blocks)
            {
                str.WriteByte((byte)b);
            }
            str.Flush();
        };

    private BlockDistribution BytesToBlocks(byte[] bytes)
    {
        BlockDistribution blocks = new BlockDistribution();

        long i = 0;

        for (int x = 1; x < BlockDistribution.SIDE - 1; x++)
            for (int z = 1; z < BlockDistribution.SIDE - 1; z++)
                for (int y = 0; y < BlockDistribution.HEIGHT; y++)
                    blocks[x, y, z] = (BlockType)bytes[i++];

        return blocks;
    }

    private void Save(string filename, Action<Stream> Converter)
    {
        using (var output = File.OpenWrite(path + "/" + filename))
            Converter(output);
    }

    private ByteArray Read(string filename)
    {
        string finalName = path + "/" + filename;
        if (File.Exists(finalName))
            return new ByteArray(File.ReadAllBytes(finalName));
        else
            return null;
    }
}

/// <summary>
/// Byte array wrapper because I can't use nullables.
/// </summary>
internal class ByteArray
{

    internal byte[] bytes;

    internal ByteArray(byte[] bytes)
    {
        this.bytes = bytes;
    }
}
