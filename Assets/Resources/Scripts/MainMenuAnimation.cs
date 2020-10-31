using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class MainMenuAnimation : MonoBehaviour
{
    private readonly float angularSpeed = .5f;
    private readonly Vector3 scale = new Vector3(25, 10);

    private Vector3 fixedPoint;
    private float currentAngle;

    void Start()
    {
        fixedPoint = transform.position;
    }

    void Update()
    {
        currentAngle += angularSpeed * Time.deltaTime;

        Vector3 offset = new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle));

        offset.Scale(scale);

        transform.position = fixedPoint + offset;
    }
}
