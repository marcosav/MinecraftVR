using UnityEngine;

public class ContinueButton : ButtonColliderClick
{
    public override void OnClick() => GetComponentInParent<GameMenu>().Toggle();
}
