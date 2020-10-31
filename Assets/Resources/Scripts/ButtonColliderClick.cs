using UnityEngine;
using UnityEngine.InputSystem;

public abstract class ButtonColliderClick : ColliderClick
{
    public abstract override void OnClick();

    public override void OnEnter()
    {
        gameObject.transform.Translate(new Vector3(0, 0, -30));
    }

    public override void OnExit()
    {
        gameObject.transform.Translate(new Vector3(0, 0, 30));
    }
}
