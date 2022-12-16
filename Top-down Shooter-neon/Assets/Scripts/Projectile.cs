using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    public LayerMask collisionMask;
    float speed;
    public float damage = 1;
    float lifetime = 2;

    float skinWidth =.1f;

    void Start()
    {
        Destroy(gameObject, lifetime);
        Collider[] intialCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask);
        if (intialCollisions.Length > 0)
        {
            OnHitObject(intialCollisions[0], transform.position);
        }

    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

	void Update () {
        float moveDistance = Time.deltaTime * speed;
        CheckforCollisions(moveDistance);
        Vector3 moveVector = moveDistance * Vector3.forward;
        transform.Translate(moveVector);
	}

    void CheckforCollisions(float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide))
        {
            OnHitObject(hit.collider, hit.point);
        }
    }

    void OnHitObject(Collider c, Vector3 hitPoint)
    {
        Idamageable damegeableObject =c.GetComponent<Idamageable>();
        if (damegeableObject != null)
        {
            damegeableObject.TakeHit(damage, hitPoint, transform.forward);
            GameObject.Destroy(gameObject);
        }
        else if(c != null)
        {
            GameObject.Destroy(gameObject);
        }
    }

}
