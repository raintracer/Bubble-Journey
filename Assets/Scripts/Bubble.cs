using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{

    private GameObject GO;
    private LineRenderer LR;
    private const int SEGMENT_COUNT = 10;
    private float Thickness = 0.03f;
    public float radius = 1;

    void Awake()
    {

        GO = gameObject;
        LR = GO.GetComponent<LineRenderer>();
        LR.positionCount = SEGMENT_COUNT + 2;
        LR.useWorldSpace = false;
        LR.startColor = Color.cyan;
        LR.endColor = Color.cyan;
        LR.material = GameAssets.Material.SpriteDefault;
        LR.startWidth = Thickness;
        LR.endWidth = Thickness;
        LR.numCornerVertices = 5;

    }

    public void SetRadius(float _Radius)
    {
        radius = _Radius;
        SetCircle();
    }

    void SetCircle()
    {
        float x;
        float y;
        float angle = 30f;

        for (int i = 0; i < (SEGMENT_COUNT + 2); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            LR.SetPosition(i, new Vector3(x, y, 0));

            angle += (360f / (float)SEGMENT_COUNT);
        }
    }
}
