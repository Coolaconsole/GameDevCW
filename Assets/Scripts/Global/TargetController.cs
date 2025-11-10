using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TargetController : MonoBehaviour
{
    public List<string> targetableTags;

    public List<GameObject> possibleTargets = new List<GameObject>();

    public GameObject currentTarget;

    private void OnTriggerEnter(Collider other)
    {
        if (targetableTags.Contains(other.gameObject.tag))
            possibleTargets.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (targetableTags.Contains(other.gameObject.tag) && other.gameObject != null)
        {
            if (currentTarget.Equals(other.gameObject))
                currentTarget = null;
            possibleTargets.Remove(other.gameObject);
        }
    }

    public void UpdateTarget()
    {
        GameObject bestTarget = null;
        float bestDistance = Mathf.Infinity;
        int bestPriority = int.MaxValue;

        Vector3 selfPos = transform.position;

        foreach (var t in possibleTargets)
        {
            if (t == null) continue;  // clean up destroyed objects

            int priority = targetableTags.IndexOf(t.tag);

            float distance = Vector3.Distance(selfPos, t.transform.position);

            // choose target by priority first, then distance
            if (priority < bestPriority || (priority == bestPriority && distance < bestDistance))
            {
                bestPriority = priority;
                bestDistance = distance;
                bestTarget = t;
            }
        }

        currentTarget = bestTarget;
    }
}
