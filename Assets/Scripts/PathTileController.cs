using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PathTileController : MonoBehaviour
{
    // must have value 0-1
    public float activity;  // represents the footfall that this tile gets, increased by enemies walking and dying on it
    public Renderer renderer;
    public Color startColor = new Color(0.59f, 0.29f, 0.0f);
    public Color endColor = Color.red;

    private void Start()
    {
        renderer = GetComponent<MeshRenderer>();
    }

    public void updateActivity(float value)
    {
        activity += value;
    }

    public void resetActivity()
    {
        activity = 0;
    }

    private void Update()
    {
        activity = Mathf.Clamp01(activity);
      
        Color brown = startColor;
        Color red = endColor;

        Color current = Color.Lerp(brown, red, activity);

        renderer.material.color = current;
    }
}
