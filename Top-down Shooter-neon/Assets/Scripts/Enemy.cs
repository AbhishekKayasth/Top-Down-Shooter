using UnityEngine;
using System.Collections;

[RequireComponent (typeof (UnityEngine.AI.NavMeshAgent))]
public class Enemy : LivingEntity {

	public enum State {Idle, Chasing, Attacking};
	State currentState;

	public ParticleSystem deathEffect;
    public Material deathEffectMat;

    public Material skinMaterial;
    public Material attackMaterial;

    public float damage = 1;
	public float damageLightRate = -3f;
    public int score;
    public int moneyReward;

	UnityEngine.AI.NavMeshAgent pathfinder;
	Transform target;
	LivingEntity targetEntity;

	public float attackDistanceThreshold = .25f;
	public float timeBetweenAttacks = 1;


	float nextAttackTime;
	float myCollisionRadius;
	float targetCollisionRadius;

	bool hasTarget;

	void Awake() {
		pathfinder = GetComponent<UnityEngine.AI.NavMeshAgent> ();
		
		if (GameObject.FindGameObjectWithTag ("Player") != null) {
			hasTarget = true;
			
			target = GameObject.FindGameObjectWithTag ("Player").transform;
			targetEntity = target.GetComponent<LivingEntity> ();
			
			myCollisionRadius = GetComponent<CapsuleCollider> ().radius;
			targetCollisionRadius = target.GetComponent<CapsuleCollider> ().radius;
		}
	}
	
	protected override void Start () {
		base.Start ();
        skinMaterial = transform.GetComponent<Renderer>().material;
		if (hasTarget) {
			currentState = State.Chasing;
			targetEntity.OnDeath += OnTargetDeath;

			StartCoroutine (UpdatePath ());
		}
	}

	public override void TakeHit (float _damage, Vector3 hitPoint, Vector3 hitDirection)
	{
		AudioManager.instance.PlaySound ("Impact", transform.position);
		if (_damage >= health) 
        {
			AudioManager.instance.PlaySound ("Enemy Death", transform.position);
            deathEffect.GetComponent<ParticleSystemRenderer>().material = deathEffectMat;
			Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, deathEffect.startLifetime);
                    target.GetComponent<Player>().currentScore += score;
                    target.GetComponent<Player>().currentMoney += moneyReward;
                    target.GetComponent<Player>().DisplayState();
		}
		base.TakeHit (_damage, hitPoint, hitDirection);
	}

	void OnTargetDeath() {
		hasTarget = false;
		currentState = State.Idle;
	}

	void Update () 
    {

		if (hasTarget) 
        {
			if (Time.time > nextAttackTime) 
            {
				float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;
				if (sqrDstToTarget < Mathf.Pow (attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2)) 
                {
					nextAttackTime = Time.time + timeBetweenAttacks;
					AudioManager.instance.PlaySound ("Enemy Attack", transform.position);
					StartCoroutine (Attack ());
				}

			}
		}

	}

	IEnumerator Attack() {

		currentState = State.Attacking;
		pathfinder.enabled = false;

		Vector3 originalPosition = transform.position;
		Vector3 dirToTarget = (target.position - transform.position).normalized;
		Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);

		float attackSpeed = 3;
		float percent = 0;

        transform.GetComponent<Renderer>().material = attackMaterial;
		bool hasAppliedDamage = false;

		while (percent <= 1) {

			if (percent >= .5f && !hasAppliedDamage && targetEntity != null) {
				hasAppliedDamage = true;
				targetEntity.TakeDamage(damage);
				targetEntity.GetComponent<Player>().LightControl(damageLightRate);
			}

			percent += Time.deltaTime * attackSpeed;
			float interpolation = (-Mathf.Pow(percent,2) + percent) * 4;
			transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

			yield return null;
		}
        transform.GetComponent<Renderer>().material = skinMaterial;
		currentState = State.Chasing;
		pathfinder.enabled = true;
	}

	IEnumerator UpdatePath() {
		float refreshRate = .25f;

		while (hasTarget) {
			if (currentState == State.Chasing) {
				Vector3 dirToTarget = (target.position - transform.position).normalized;
				Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold/2);
				if (!dead) {
					pathfinder.SetDestination (targetPosition);
				}
			}
			yield return new WaitForSeconds(refreshRate);
		}
	}
}