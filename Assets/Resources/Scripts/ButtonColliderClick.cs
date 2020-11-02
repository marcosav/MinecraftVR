using UnityEngine;

public abstract class ButtonColliderClick : ColliderClick
{
    private const float DELTA = 6f;

    public abstract override void OnClick();

    public override void OnEnter()
    {
        gameObject.transform.Translate(new Vector3(0, 0, -DELTA));
    }

    public override void OnExit()
    {
        gameObject.transform.Translate(new Vector3(0, 0, DELTA));
    }
}
