using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour {

    public bool devMode;

    public Wave[] waves;
    public Enemy enemy;

    public bool allowCamp;

    LivingEntity playerEntity;
    Transform playerT;

    Wave currentWave;
    public int currentWaveNumber;

    int enemiesRemainingToSpawn;
    int enemiesRemainingAlive;
    float nextSpawnTime;

    MapGenerator map;

    float timeBetweenCampingChecks = 2;
    float campThreshholdDistance = 1.5f;
    float nextCampCheckTime;
    Vector3 campPositionOld;
    bool isCamping;

    bool isDisabled;

    public event System.Action<int> OnNewWave;
	
	void Start () {
        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.transform;

        nextCampCheckTime = timeBetweenCampingChecks + Time.time;
        campPositionOld = playerT.position;
        playerEntity.OnDeath += OnPlayerDeath;

        map = FindObjectOfType<MapGenerator>();
        NextWave();
	}
	
	
	void Update () {
        if (!isDisabled)
        {
            if (Time.time > nextCampCheckTime)
            {
                nextCampCheckTime = Time.time + timeBetweenCampingChecks;
                if (!allowCamp)
                {
                    isCamping = (Vector3.Distance(playerT.position, campPositionOld)) < campThreshholdDistance;
                    campPositionOld = playerT.position;
                }             
            }

            if ((enemiesRemainingToSpawn > 0 || currentWave.infinite) && Time.time > nextSpawnTime)
            {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

                StartCoroutine("SpawnEnemy");
            }
        }

        if (devMode)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StopCoroutine("SpawnEnemy");
                foreach (Enemy enemy in FindObjectsOfType<Enemy>())
                {
                    GameObject.Destroy(enemy.gameObject);
                }
                NextWave();
            }
        }

	}

    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1;
        float tileFlashSpeed = 4;
        Transform spawnTile = map.GetRandomOpenTile();
        if (!allowCamp)
        {
            if (isCamping)
            {
                spawnTile = map.GetTileFromPosition(playerT.position);
            }
        }
        Material tileMat = spawnTile.GetComponent<Renderer>().material;
        Color intialColor = map.tilePrefab.GetComponent<Renderer>().sharedMaterial.color;
        Color flashColor = Color.red;
        float spawnTimer = 0;

        while (spawnTimer < spawnDelay)
        {

            tileMat.color = Color.Lerp(intialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up , Quaternion.identity) as Enemy;
        spawnedEnemy.GetComponent<Renderer>().material = currentWave.skinMaterial;
        spawnedEnemy.deathEffectMat = currentWave.deathEffect;
        spawnedEnemy.score = currentWave.score;
        spawnedEnemy.moneyReward = currentWave.money;
        spawnedEnemy.OnDeath += OnEnemyDeath;
        spawnedEnemy.startingHealth = currentWave.health;
        spawnedEnemy.damage = currentWave.damage;
        spawnedEnemy.attackDistanceThreshold = currentWave.attackDistanceThreshHold;
        spawnedEnemy.timeBetweenAttacks = currentWave.timeBetweenAttacks;
    }

    void OnPlayerDeath()
    {
        isDisabled = true;
    }

    void OnEnemyDeath()
    {
        enemiesRemainingAlive--;

        if (enemiesRemainingAlive == 0)
        {
            LvlComplete();
            playerEntity.GetComponent<Player>().currentScore = playerEntity.GetComponent<Player>().score;
            playerEntity.GetComponent<Player>().currentMoney = playerEntity.GetComponent<Player>().money;
        }
    }

    public void ResetPlayerPosition()
    {
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;
    }

    public void LvlComplete()
    {
        FindObjectOfType<GameUI>().OnLvlComplete();
    }

    public void NextWave()
    {
        currentWaveNumber ++;
        
        if (currentWaveNumber - 1 < waves.Length)
        {
            currentWave = waves[currentWaveNumber - 1];

            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;

            if (OnNewWave != null)
            {
                OnNewWave(currentWaveNumber);
            }
            ResetPlayerPosition();
            isCamping = false;
        }
    }

    [System.Serializable]
    public class Wave
    {
        public bool infinite;
        public int enemyCount;
        public float timeBetweenSpawns;
        public int score;
        public int money;
        public float health;
        public float damage;
        public float timeBetweenAttacks;
        public float attackDistanceThreshHold;

        public Material skinMaterial;
        public Material deathEffect;
    }

}
