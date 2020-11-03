using System;
using UnityEngine;

public class Terraforming : MonoBehaviour
{

    public MinecraftVR controls;

    public Transform player;
    public LayerMask groundLayer;
    public readonly float interactDist = 4;

    private readonly long delay = 200;

    private TerrainGenerator terrainGenerator;

    private bool used;
    private long last = -1;

    void Start()
    {
        terrainGenerator = GetComponent<TerrainGenerator>();
    }

    protected void Awake()
    {
        controls = new MinecraftVR();
    }

    protected void OnEnable()
    {
        controls.Enable();
    }

    protected void OnDisable()
    {
        controls.Disable();
    }

    void Update()
    {
        bool place = controls.Player.Place.ReadValue<float>() == 1;
        bool mine = controls.Player.Mine.ReadValue<float>() == 1;

        bool clicking = mine || place;

        if (clicking && (DateTimeOffset.Now.ToUnixTimeMilliseconds() - last >= delay || !used))
        {

#if UNITY_EDITOR
            Vector3 dir = Camera.main.transform.forward;
#else
            var c = GvrVRHelpers.GetHeadRotation();
            Vector3 dir = c * Vector3.forward;
#endif

            if (Physics.Raycast(Camera.main.transform.position, dir, out RaycastHit hit, interactDist, groundLayer))
            {
                used = true;

                if (place)
                    dir *= -1;

                Vector3 target = hit.point + dir * .01f;

                if (place && IsSameBlock(target, player.position))
                    return;

                ChunkPos pos = terrainGenerator.GetChunkPosition(target);
                TerrainChunk tc = terrainGenerator.GetChunkAt(pos);

                int bix = Mathf.FloorToInt(target.x) - pos.x + 1;
                int biy = Mathf.FloorToInt(target.y);
                int biz = Mathf.FloorToInt(target.z) - pos.z + 1;

                if (mine)
                    tc.Blocks[bix, biy, biz] = BlockType.Air;
                else
                {
                    tc.Blocks[bix, biy, biz] = BlockType.Grass;
                }

                last = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                tc.BuildMesh();
            }
        }

        if (!clicking)
            used = false;
    }

    private bool IsSameBlock(Vector3 a, Vector3 b)
    {
        return Math.Floor(a.x) == Math.Floor(b.x) &&
        Math.Floor(a.y) == Math.Floor(b.y) &&
        Math.Floor(a.z) == Math.Floor(b.z);
    }
}