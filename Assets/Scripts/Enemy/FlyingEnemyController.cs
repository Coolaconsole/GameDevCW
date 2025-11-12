using UnityEngine;

public class FlyingEnemyController : EnemyController
{
    public float height = 10f;


    protected override void followPath ()
    {
        if (path.Count == 0) return;

        Vector3 targetPos = CoordinateManager.Instance.getCoordinateWorldPos(path[path.Count - 1]) + pathOffset;
        targetPos.y = height;
        //Rotate towards target
        Vector3 direction = (targetPos - transform.position).normalized;
        direction = Quaternion.Euler(0, -90, 0) * direction;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x,height,transform.position.z), moveSpeed * Time.deltaTime);
        //Rotate only component of enemy called "Body" using Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.1f);
        Transform child = transform.Find("Body");
        child.LookAt(child.position + direction);
    }
}
