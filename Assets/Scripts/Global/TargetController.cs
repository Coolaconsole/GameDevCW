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
        {
            Projectile pc = GetComponentInParent<Projectile>();
            if (pc) pc.target = other.gameObject;
            possibleTargets.Add(other.gameObject);
        }
    }

    private void Update()
    {
        /*
        for (int i = possibleTargets.Count - 1; i >= 0; i--)
        {
            GameObject target = possibleTargets[i];
            if (Vector3.Distance(transform.position, target.transform.position) > GetComponent<SphereCollider>().radius*3)
            {
                possibleTargets.RemoveAt(i);
                if (currentTarget == target)
                    currentTarget = null;
            }
        }*/
    }

    private void OnTriggerExit(Collider other)
    {
        if (targetableTags.Contains(other.gameObject.tag) && currentTarget != null)
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
