using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{

    private GameObject GO;
    private BoxCollider2D BC;
    private LineRenderer LR;
    private const int SEGMENT_COUNT = 200;
    private float Thickness = 0.05f;
    public float radius;
    [SerializeField] public bool Free = true;
    Color ActiveColor;
    Vector2 SpawnPoint;
    bool _Respawning = false;
    const float RADIUS_FREE = 0.5f;
    const int UNATTACH_TRAVEL_FRAMES = 20;
    const int UNATTACH_DELAY_FRAMES = 10;

    public enum BubbleType { Jump, Land, Dash, Bounce, Win}
    [SerializeField] public BubbleType Type = BubbleType.Jump;

    static Color[] ActiveColors = { Color.cyan, Color.red, Color.yellow, Color.green, Color.magenta };

    void Awake()
    {

        GO = gameObject;
        GO.GetComponent<SpriteRenderer>().sprite = null;

        BC = GO.GetComponent<BoxCollider2D>();
        LR = GO.GetComponent<LineRenderer>();
        LR.positionCount = SEGMENT_COUNT + 2;
        LR.useWorldSpace = false;
        ActiveColor = ActiveColors[(int)Type];
        LR.startColor = ActiveColor;
        LR.endColor = ActiveColor;

        LR.material = GameAssets.Material.SpriteDefault;
        LR.startWidth = Thickness;
        LR.endWidth = Thickness;
        LR.numCornerVertices = 5;
        SpawnPoint = GO.transform.position;

        Free = true;
        SetRadius(RADIUS_FREE);
        UpdateColorByActive(true);

    }

    public void SetBubbleType(BubbleType _Type)
    {
        Type = _Type;
        ActiveColor = ActiveColors[(int)Type];
    }

    public void UpdateColorByActive(bool Active)
    {
        if (Active)
        {
            LR.startColor = ActiveColor;
            LR.endColor = ActiveColor;
        }
        else
        {
            LR.startColor = Color.grey;
            LR.endColor = Color.grey;
        }
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

    public void AttachToPlayer(float _Radius)
    {
        SetRadius(_Radius);
        Free = false;
        BC.enabled = false;
    }

    public void UnattachFromPlayer()
    {
        StartCoroutine(UnattachAnimation());
    }

    private IEnumerator UnattachAnimation()
    {
        // Get darker
        UpdateColorByActive(false);

        // Move towards spawn point and scale down radius smoothly over transition frames
        Vector3 StartPosition = gameObject.transform.position;
        Vector3 TargetPosition = SpawnPoint;
        Vector3 PositionChangePerFrame = (TargetPosition - StartPosition) / (float)(UNATTACH_TRAVEL_FRAMES);

        float StartRadius = radius;
        float TargetRadius = RADIUS_FREE;
        float RadiusChangePerFrame = (TargetRadius - StartRadius) / (float)(UNATTACH_TRAVEL_FRAMES);

        for (int i = 0; i < UNATTACH_TRAVEL_FRAMES; i++)
        {
            yield return new WaitForFixedUpdate();
            transform.position += PositionChangePerFrame;
            SetRadius(radius + RadiusChangePerFrame);
        }

        // Ensure values end exactly
        transform.position = TargetPosition;
        SetRadius(TargetRadius);

        // Wait for delay frames
        for (int i = 0; i < UNATTACH_DELAY_FRAMES; i++) yield return new WaitForFixedUpdate();

        // Reactivate
        Free = true;
        BC.enabled = true;

        // Brighten
        UpdateColorByActive(true);

    }


}
