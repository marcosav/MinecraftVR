using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public readonly int chunkRenderDist = 6;
    public readonly int chunkKeepDist = 3;

    public GameObject terrainChunk;
    public Transform player;

    private FastNoiseLite noise;

    private readonly Dictionary<ChunkPos, TerrainChunk> chunks = new Dictionary<ChunkPos, TerrainChunk>();
    private readonly Queue<TerrainChunk> chunkPool = new Queue<TerrainChunk>();

    private readonly ConcurrentQueue<ChunkBuildJob> computeChunkQueue = new ConcurrentQueue<ChunkBuildJob>();
    private readonly ConcurrentQueue<ChunkBuildJob> buildChunkQueue = new ConcurrentQueue<ChunkBuildJob>();

    private ChunkPos curChunk = new ChunkPos(-1, -1);

    void Start()
    {
        noise = new FastNoiseLite(Random.Range(0, 100000));
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        Task.Factory.StartNew(BuildTask);
    }

    private void Update() => LoadChunks();

    void BuildTask()
    {
        while (true)
        {
            ChunkBuildJob job;

            if (!computeChunkQueue.TryDequeue(out job))
                continue;

            float height;

            for (int x = 0; x < TerrainChunk.CHUNK_SIZE + 2; x++)
                for (int z = 0; z < TerrainChunk.CHUNK_SIZE + 2; z++)
                {
                    height = ComputeHeightAt(job.pos.x + x - 1, job.pos.z + z - 1);
                    for (int y = 0; y < TerrainChunk.CHUNK_HEIGHT; y++)
                        job.chunk.blocks[x, y, z] = height < y ? 0 : 1;
                }

            /*for (int x = 0; x < TerrainChunk.CHUNK_SIZE + 2; x++)
                for (int z = 0; z < TerrainChunk.CHUNK_SIZE + 2; z++)
                    for (int y = 0; y < TerrainChunk.CHUNK_HEIGHT; y++)
                        job.chunk.blocks[x, y, z] = GetBlockType(job.pos.x + x - 1, y, job.pos.z + z - 1);*/

            buildChunkQueue.Enqueue(job);
        }
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
        ChunkPos playerChunk = GetChunkFromLocation(player.position);

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
        ChunkBuildJob job;
        while (buildChunkQueue.TryDequeue(out job))
        {
            job.chunk.BuildMesh();
            chunks[job.pos] = job.chunk;

            job.chunk.gameObject.SetActive(true);

            yield return null;
        }
    }

    private ChunkPos GetChunkFromLocation(Vector3 loc)
    {
        int x = Mathf.FloorToInt(loc.x / TerrainChunk.CHUNK_SIZE) * TerrainChunk.CHUNK_SIZE;
        int z = Mathf.FloorToInt(loc.z / TerrainChunk.CHUNK_SIZE) * TerrainChunk.CHUNK_SIZE;

        return new ChunkPos(x, z);
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