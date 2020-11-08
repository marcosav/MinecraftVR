using System;
using UnityEngine;

public class Terraforming : MonoBehaviour
{

    private const long DELAY = 200;

    public MinecraftVR controls;

    public Transform player;
    public LayerMask groundLayer;
    public readonly float interactDist = 4;

    private TerrainGenerator terrainGenerator;
    private Inventory inventory;

    private bool used;
    private long last = -1;

    void Start()
    {
        terrainGenerator = GetComponent<TerrainGenerator>();
        inventory = player.GetComponent<Inventory>();
    }

    protected void Awake() => controls = new MinecraftVR();
    protected void OnEnable() => controls.Enable();
    protected void OnDisable() => controls.Disable();

    void Update()
    {
        bool place = controls.Player.Place.ReadValue<float>() == 1;
        bool mine = controls.Player.Mine.ReadValue<float>() == 1;

        bool clicking = mine || place;

        if (clicking && (DateTimeOffset.Now.ToUnixTimeMilliseconds() - last >= DELAY || !used))
        {

            var c = PlayerLook.GetRotation();
            Vector3 dir = c * Vector3.forward;

            if (Physics.Raycast(Camera.main.transform.position, dir, out RaycastHit hit, interactDist, groundLayer))
            {
                used = true;

                if (place)
                    dir *= -1;

                Vector3 target = hit.point + dir * .01f;

                if (place && CannotPlace(target, player.position))
                    return;

                ChunkPos pos = terrainGenerator.GetChunkPosition(target);
                TerrainChunk tc = terrainGenerator.GetChunkAt(pos);

                int bix = Mathf.FloorToInt(target.x) - pos.x + 1;
                int biy = Mathf.FloorToInt(target.y);
                int biz = Mathf.FloorToInt(target.z) - pos.z + 1;

                if (mine)
                {
                    if (CanBreak(tc.Blocks[bix, biy, biz]))
                        tc.Blocks[bix, biy, biz] = BlockType.Air;
                }
                else
                {
                    tc.Blocks[bix, biy, biz] = inventory.Current;
                }

                last = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                tc.BuildMesh();
            }
        }

        if (!clicking)
            used = false;
    }

    private bool CannotPlace(Vector3 target, Vector3 playerPos)
    {
        return IsSameBlock(target, playerPos) || IsSameBlock(target, playerPos + new Vector3(0, 1, 0));
    }

    private bool IsSameBlock(Vector3 a, Vector3 b)
    {
        return Math.Floor(a.x) == Math.Floor(b.x) &&
            Math.Floor(a.y) == Math.Floor(b.y) &&
            Math.Floor(a.z) == Math.Floor(b.z);
    }

    private bool CanBreak(BlockType type)
    {
        return type != BlockType.Baserock;
    }
}