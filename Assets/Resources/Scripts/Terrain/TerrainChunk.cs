using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockDistribution
{

    public const int SIDE = TerrainChunk.CHUNK_SIZE + 2;
    public const int HEIGHT = TerrainChunk.CHUNK_HEIGHT;

    public bool Dirty { get; set; } = false;

    private readonly BlockType[,,] blocks = new BlockType[SIDE, HEIGHT, SIDE];

    public BlockType GetAt(int x, int y, int z) => blocks[x, y, z];

    public BlockType this[int x, int y, int z]
    {
        get { return GetAt(x, y, z); }
        set { Dirty = true; blocks[x, y, z] = value; }
    }

    public IEnumerator<BlockType> GetEnumerator()
    {
        for (int x = 1; x < TerrainChunk.CHUNK_SIZE + 1; x++)
            for (int z = 1; z < TerrainChunk.CHUNK_SIZE + 1; z++)
                for (int y = 0; y < TerrainChunk.CHUNK_HEIGHT; y++)
                    yield return GetAt(x, y, z);
    }
}

public class TerrainChunk : MonoBehaviour
{
    public const int CHUNK_SIZE = 16;
    public const int CHUNK_HEIGHT = 128;

    public BlockDistribution Blocks { get; set; } = null;

    public void BuildMesh()
    {
        Mesh mesh = new Mesh();

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        for (int x = 1; x < CHUNK_SIZE + 1; x++)
            for (int z = 1; z < CHUNK_SIZE + 1; z++)
                for (int y = 0; y < CHUNK_HEIGHT; y++)
                {
                    if (Blocks[x, y, z] != BlockType.Air)
                    {
                        Vector3 blockPos = new Vector3(x - 1, y, z - 1);
                        int numFaces = 0;
                        //no land above, build top face
                        if (y < CHUNK_HEIGHT - 1 && Blocks[x, y + 1, z] == 0)
                        {
                            verts.Add(blockPos + new Vector3(0, 1, 0));
                            verts.Add(blockPos + new Vector3(0, 1, 1));
                            verts.Add(blockPos + new Vector3(1, 1, 1));
                            verts.Add(blockPos + new Vector3(1, 1, 0));
                            numFaces++;
                        }

                        //bottom
                        if (y > 0 && Blocks[x, y - 1, z] == 0)
                        {
                            verts.Add(blockPos + new Vector3(0, 0, 0));
                            verts.Add(blockPos + new Vector3(1, 0, 0));
                            verts.Add(blockPos + new Vector3(1, 0, 1));
                            verts.Add(blockPos + new Vector3(0, 0, 1));
                            numFaces++;
                        }

                        //front
                        if (Blocks[x, y, z - 1] == 0)
                        {
                            verts.Add(blockPos + new Vector3(0, 0, 0));
                            verts.Add(blockPos + new Vector3(0, 1, 0));
                            verts.Add(blockPos + new Vector3(1, 1, 0));
                            verts.Add(blockPos + new Vector3(1, 0, 0));
                            numFaces++;
                        }

                        //right
                        if (Blocks[x + 1, y, z] == 0)
                        {
                            verts.Add(blockPos + new Vector3(1, 0, 0));
                            verts.Add(blockPos + new Vector3(1, 1, 0));
                            verts.Add(blockPos + new Vector3(1, 1, 1));
                            verts.Add(blockPos + new Vector3(1, 0, 1));
                            numFaces++;
                        }

                        //back
                        if (Blocks[x, y, z + 1] == 0)
                        {
                            verts.Add(blockPos + new Vector3(1, 0, 1));
                            verts.Add(blockPos + new Vector3(1, 1, 1));
                            verts.Add(blockPos + new Vector3(0, 1, 1));
                            verts.Add(blockPos + new Vector3(0, 0, 1));
                            numFaces++;
                        }

                        //left
                        if (Blocks[x - 1, y, z] == 0)
                        {
                            verts.Add(blockPos + new Vector3(0, 0, 1));
                            verts.Add(blockPos + new Vector3(0, 1, 1));
                            verts.Add(blockPos + new Vector3(0, 1, 0));
                            verts.Add(blockPos + new Vector3(0, 0, 0));
                            numFaces++;
                        }

                        int tl = verts.Count - 4 * numFaces;
                        for (int i = 0; i < numFaces; i++)
                        {
                            tris.AddRange(new int[] {
                                tl + i * 4,
                                tl + i * 4 + 1,
                                tl + i * 4 + 2,
                                tl + i * 4,
                                tl + i * 4 + 2,
                                tl + i * 4 + 3
                            });
                        }
                    }
                }

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();

        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
