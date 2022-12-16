using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

[RequireComponent (typeof(NavMeshAgent))]
public class MyEnemy : LivingEntity
{
    public enum State { Idle, Chasing, Attacking };
    State currentState;

    public ParticleSystem deathEffect;

    FieldOfView fov;
    NavMeshAgent pathfinder;
    Transform target;
    LivingEntity targetEntity;
    Material skinMaterial;

    Color originalColor;

    float attackDistanceThreshhold = 1.5f;
    float timeBeetweenAttacks = 1;
    float damage = 1;

    float nextAttackTime;
    float myCollisionRadius;
    float targetCollisionRadius;

    bool hasTarget;

    void Awake()
    {
        pathfinder = GetComponent<NavMeshAgent>();

        fov = GetComponent<FieldOfView>();
    }

    void Start()
    {
        currentState = State.Idle;
    }
	
    public void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, float enemyHealth, Color skinColor)
    {
        pathfinder.speed = moveSpeed;

        if (hasTarget)
        {
            damage = Mathf.Ceil(targetEntity.startingHealth / hitsToKillPlayer);
        }
        startingHealth = enemyHealth;

        skinMaterial = GetComponent<Renderer>().material;
        skinMaterial.color = skinColor;
        originalColor = skinMaterial.color;
    }

    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        AudioManager.instance.PlaySound("Impact", transform.position);
        if (damage >= health)
        {
            AudioManager.instance.PlaySound("Enemy Death", transform.position);
            deathEffect.GetComponent<ParticleSystemRenderer>().material = skinMaterial;
           Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, deathEffect.startLifetime);
        }
        base.TakeHit(damage, hitPoint, hitDirection);
    }


    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }

    public void GetTarget(Transform visibleTarget)
    {
        target = visibleTarget;
    }
	
	
	void Update () {

        if (target == null)
        {
            fov.GetTarget();
        }

        if (target != null)
        {
            hasTarget = true;

            targetEntity = target.GetComponent<LivingEntity>();
            targetEntity.OnDeath += OnTargetDeath;

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
        }

        if (currentState != State.Chasing)
        {
            if (hasTarget)
            {
                currentState = State.Chasing;
                targetEntity.OnDeath += OnTargetDeath;

                StartCoroutine(UpdatePath());
            }
        }

        if (hasTarget == true)
        {
            if (Time.time > nextAttackTime)
            {

                float sqrDstoTarget = (target.position - transform.position).sqrMagnitude;

                if (sqrDstoTarget < Mathf.Pow(attackDistanceThreshhold + myCollisionRadius + targetCollisionRadius, 2))
                {
                    nextAttackTime = Time.time + timeBeetweenAttacks;
                    AudioManager.instance.PlaySound("Enemy Attack", transform.position);
                    currentState = State.Attacking;
                    StartCoroutine(Attack());
                }
            }
        }

	}

    IEnumerator Attack()
    {
        currentState = State.Attacking;
        pathfinder.enabled = false;

        Vector3 originalPosition = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);

        float percent = 0;
        float attackSpeed = 3;

        skinMaterial.color = Color.red;
        bool hasAppliedDamage = false;

        while (percent <= 1)
        {
            if (percent >= .5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }

            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent,2) + percent) * 4;
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);
            
            yield return null;
        }

        skinMaterial.color = originalColor;
        currentState = State.Chasing;
        pathfinder.enabled = true;
    }

    IEnumerator UpdatePath ()
    {
        float refreshRate = .25f;

        while (hasTarget)
        {
            if (currentState == State.Chasing)
            {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshhold/2);
                if (!dead)
                {
                    pathfinder.SetDestination(targetPosition);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }

}
