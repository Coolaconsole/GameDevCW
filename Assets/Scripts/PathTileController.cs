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

        StartCoroutine(fadeInColor());
    }

    private System.Collections.IEnumerator fadeInColor()
    {
        Color initialColor = new Color(0.4213236f, 0.6886792f, 0.4125578f);
        Color targetColor = startColor;
        float time = 0f;

        // Set the initial color
        renderer.material.color = initialColor;

        while (time < 2.0f)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / 2.0f);
            renderer.material.color = Color.Lerp(initialColor, targetColor, t);
            yield return null;
        }

        renderer.material.color = targetColor;
    }


    public void updateActivity(float value)
    {
        activity += value;
        updateColor();

        

        if (activity > 1)
        {
            PathManager.Instance.splitPathAt(CoordinateManager.Instance.getNearestWorldPosCoordinate(transform.position));
        }
    }

    public void resetActivity()
    {
        activity = 0;
    }

    private void updateColor()
    {
        activity = Mathf.Clamp01(activity);

        Color brown = startColor;
        Color red = endColor;

        Color current = Color.Lerp(brown, red, activity);

        renderer.material.color = current;
    }
}
