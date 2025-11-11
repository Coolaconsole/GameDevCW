using UnityEngine;

public class NextPromptController : MonoBehaviour
{
    public float amplitude = 10f;
    public float frequency = 3f;
    private RectTransform rect;
    private Vector3 initialPos;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        initialPos = rect.localPosition;
    }

    void Update()
    {
        Vector3 pos = initialPos;
        pos.x += Mathf.Abs(Mathf.Sin(Time.time * frequency) * amplitude);
        rect.localPosition = pos;
    }
}
