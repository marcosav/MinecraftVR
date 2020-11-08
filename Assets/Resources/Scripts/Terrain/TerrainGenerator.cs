using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public const int chunkRenderDist = 6;
    public const int chunkKeepDist = 3;

    private TerrainPersistor persistor;

    public GameObject terrainChunk;
    public Transform player;

    private FastNoiseLite noise;

    private readonly Dictionary<ChunkPos, TerrainChunk> chunks = new Dictionary<ChunkPos, TerrainChunk>();
    private readonly Queue<TerrainChunk> chunkPool = new Queue<TerrainChunk>();

    private readonly ConcurrentQueue<ChunkBuildJob> computeChunkQueue = new ConcurrentQueue<ChunkBuildJob>();
    private readonly ConcurrentQueue<ChunkBuildJob> buildChunkQueue = new ConcurrentQueue<ChunkBuildJob>();

    private ChunkPos curChunk = new ChunkPos(-1, -1);

    private long built;

    void Start()
    {
        persistor = GetComponent<TerrainPersistor>();
        persistor.Init();

        noise = new FastNoiseLite(persistor.GetSeed());
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        Task.Factory.StartNew(BuildTask);

        player.position = persistor.PlayerPosition;
        StartCoroutine(SpawnTask());
    }

    private IEnumerator SpawnTask()
    {
        while (built < Math.Pow(chunkRenderDist * 2 + 1, 2))
            yield return new WaitForSeconds(.5f);

        //yield return new WaitForSeconds(.5f);

        player.GetComponent<PlayerBehavior>().Enable(true);
    }

    private void Update() => LoadChunks();

    void BuildTask()
    {
        while (true)
        {
            try
            {
                if (!computeChunkQueue.TryDequeue(out ChunkBuildJob job))
                    continue;

                job.chunk.Blocks = persistor.GetBlocksFor(job.pos, GenerateBlockDistribution);

                buildChunkQueue.Enqueue(job);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
            }
        }
    }

    private BlockDistribution GenerateBlockDistribution(ChunkPos pos)
    {
        var rnd = new System.Random(persistor.GetSeed() - pos.x * 31 - pos.z * 23);
        double height;
        BlockDistribution blocks = new BlockDistribution();

        bool hasTree = false;

        for (int x = 0; x < TerrainChunk.CHUNK_SIZE + 2; x++)
            for (int z = 0; z < TerrainChunk.CHUNK_SIZE + 2; z++)
            {
                height = Math.Floor(ComputeHeightAt(pos.x + x - 1, pos.z + z - 1));
                for (int y = 0; y < TerrainChunk.CHUNK_HEIGHT; y++)
                    blocks[x, y, z] = GetForHeight(y, height);

                if (!hasTree && x > 4 && x < 12 && z > 4 && z < 12)
                {
                    int r = rnd.Next(1, 115 + (int)Math.Pow(height, height / 65.0));
                    if (r == 2)
                    {
                        hasTree = true;
                        GenerateTree(rnd, blocks, x, (int)height + 1, z);
                    }
                }
            }

        return blocks;
    }

    private BlockType GetForHeight(int y, double height)
    {
        if (y > height)
            return BlockType.Air;

        if (y == 0)
            return BlockType.Baserock;

        if (height == y)
            return BlockType.Grass;

        if (y > height - 5)
            return BlockType.Dirt;

        return BlockType.Stone;
    }

    private void GenerateTree(System.Random r, BlockDistribution blocks, int x, int y, int z)
    {
        int t = r.Next(2, 5);
        int h = r.Next(4, 6);
        int w = r.Next(1, 4);
        int w2 = r.Next(1, 4);

        int hh = w + w2 >= 4 ? 1 : 0;

        for (int k = y + 2 + hh; k <= y + h + hh; k++)
        {
            if (k == y + h)
            {
                w2 -= Math.Min(1, r.Next(3));
                w -= r.Next(2);

            }
            else if (k == y + h - 1 && r.Next(3) < 2)
            {
                w2 -= Math.Max(0, r.Next(-1, 2));
                w -= Math.Max(0, r.Next(-1, 2));
            }

            for (int i = x - w2; i <= x + w; i++)
                for (int j = z - w; j <= z + w2; j++)
                    blocks[i, k, j] = BlockType.Leaves;
        }

        for (int i = 0; i < t + hh; i++)
            blocks[x - 1, y + i, z] = BlockType.Log;
    }

    void BuildChunk(ChunkPos pos)
    {
        TerrainChunk chunk;

        if (chunkPool.Count > 0)
        {
            chunk = chunkPool.Dequeue();
            chunk.transform.position = new Vector3(pos.x, 0, pos.z);
        }
        else
        {
            GameObject chunkObj = Instantiate(terrainChunk, new Vector3(pos.x, 0, pos.z), Quaternion.identity);
            chunk = chunkObj.GetComponent<TerrainChunk>();
        }

        computeChunkQueue.Enqueue(new ChunkBuildJob(pos, chunk));
    }

    void LoadChunks()
    {
        var pp = player.position;
        persistor.PlayerPosition = pp;
        ChunkPos playerChunk = GetChunkPosition(pp);

        if (!curChunk.Equals(playerChunk))
        {
            curChunk.x = playerChunk.x;
            curChunk.z = playerChunk.z;

            List<ChunkPos> toGenerate = new List<ChunkPos>();

            for (int i = playerChunk.x - TerrainChunk.CHUNK_SIZE * chunkRenderDist;
                i <= playerChunk.x + TerrainChunk.CHUNK_SIZE * chunkRenderDist;
                i += TerrainChunk.CHUNK_SIZE)

                for (int j = playerChunk.z - TerrainChunk.CHUNK_SIZE * chunkRenderDist;
                    j <= playerChunk.z + TerrainChunk.CHUNK_SIZE * chunkRenderDist;
                    j += TerrainChunk.CHUNK_SIZE)
                {
                    ChunkPos cp = new ChunkPos(i, j);
                    if (!chunks.ContainsKey(cp))
                        toGenerate.Add(cp);
                }

            List<ChunkPos> toDestroy = new List<ChunkPos>();

            foreach (ChunkPos cp in chunks.Keys)
                if (Mathf.Abs(playerChunk.x - cp.x) > TerrainChunk.CHUNK_SIZE * (chunkRenderDist + chunkKeepDist) ||
                    Mathf.Abs(playerChunk.z - cp.z) > TerrainChunk.CHUNK_SIZE * (chunkRenderDist + chunkKeepDist)
                )
                    toDestroy.Add(cp);

            toGenerate.RemoveAll(cp => Mathf.Abs(playerChunk.x - cp.x) > TerrainChunk.CHUNK_SIZE * (chunkRenderDist + 1) || Mathf.Abs(playerChunk.z - cp.z) > TerrainChunk.CHUNK_SIZE * (chunkRenderDist + 1));

            foreach (ChunkPos cp in toDestroy)
            {
                chunks[cp].gameObject.SetActive(false);
                chunkPool.Enqueue(chunks[cp]);
                chunks.Remove(cp);
            }

            toGenerate.ForEach(BuildChunk);
        }

        if (!buildChunkQueue.IsEmpty)
            StartCoroutine(DelayBuildChunks());
    }

    IEnumerator DelayBuildChunks()
    {
        while (buildChunkQueue.TryDequeue(out ChunkBuildJob job))
        {
            job.chunk.BuildMesh();
            chunks[job.pos] = job.chunk;

            job.chunk.gameObject.SetActive(true);

            built++;

            yield return null;
        }
    }

    public ChunkPos GetChunkPosition(Vector3 loc)
    {
        int x = Mathf.FloorToInt(loc.x / TerrainChunk.CHUNK_SIZE) * TerrainChunk.CHUNK_SIZE;
        int z = Mathf.FloorToInt(loc.z / TerrainChunk.CHUNK_SIZE) * TerrainChunk.CHUNK_SIZE;

        return new ChunkPos(x, z);
    }

    public TerrainChunk GetChunkAt(ChunkPos loc)
    {
        return chunks[loc];
    }

    float ComputeHeightAt(int x, int z)
    {
        float n1 = noise.GetNoise(x * .8f, z * .8f) * 10;
        float n2 = noise.GetNoise(x * 3f, z * 3f) * 10 * (noise.GetNoise(x * .3f, z * .3f) + .5f);

        float heightMap = n1 + n2;

        return TerrainChunk.CHUNK_HEIGHT * .5f + heightMap;
    }
}

public struct ChunkPos : System.IEquatable<ChunkPos>
{
    public int x, z;

    public ChunkPos(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public bool Equals(ChunkPos c) => c.x == x && c.z == z;
}

public struct ChunkBuildJob
{
    public readonly ChunkPos pos;
    public readonly TerrainChunk chunk;

    public ChunkBuildJob(ChunkPos pos, TerrainChunk chunk)
    {
        this.chunk = chunk;
        this.pos = pos;
    }
}