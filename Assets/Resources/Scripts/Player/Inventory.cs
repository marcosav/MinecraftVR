using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{

    private const float DISPLAY_TIME = 3 * 1000;
    private const long DELAY = 250;

    public Text usingTX;

    public MinecraftVR controls;

    public BlockType Current { get; private set; } = BlockType.Cobblestone;
    private int currentIndex;

    private int blockTypeSize;

    private long last = -1;

    void Start()
    {
        currentIndex = (int)Current;
        blockTypeSize = Enum.GetNames(typeof(BlockType)).Length;

        usingTX.gameObject.SetActive(false);
    }

    protected void Awake() => controls = new MinecraftVR();
    protected void OnEnable() => controls.Enable();
    protected void OnDisable() => controls.Disable();

    void Update()
    {
        bool previous = controls.Player.PreviousItem.ReadValue<float>() == 1;
        bool next = controls.Player.NextItem.ReadValue<float>() == 1;

        if (previous || next)
            MoveItem(next);

        if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - last >= DISPLAY_TIME)
            usingTX.gameObject.SetActive(false);
    }

    public void MoveItem(bool next)
    {
        if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - last < DELAY)
            return;

        last = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        usingTX.gameObject.SetActive(true);

        if (next)
            currentIndex++;
        else
            currentIndex--;

        Current = (BlockType)Math.Abs(currentIndex % blockTypeSize);

        usingTX.text = Current == 0 ? "Empty" : Block.Name[Current];
    }
}
