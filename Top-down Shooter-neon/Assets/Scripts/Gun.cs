using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {

    public enum FireMode { Auto, Burst, Single };
    public FireMode fireMode;

    public Transform[] projectileSpawnPoint;
    public Projectile projectile;
    public Material projectileMat;
    public float msBetweenShots = 100;
    public float gunDamage = 1;
    public float muzzleVelocity = 35;
    public int burstCount;
    public int projectilesPerMag;
    public int projectleRemainingInMag;
    public float reloadTime = .3f;

    [Header("Recoil")]
    public Vector2 kickMinMax = new Vector2(3, 5);
    public Vector2 recoilAngMinMax = new Vector2(.05f, .2f);
    public float recoilMoveSettleTime = .1f;
    public float recoilRotationSettleTime = .1f;

    [Header("Effects")]
    public Material tracerMat;
    public Material shellMat;
    public Transform shell;
    public Transform shellEjection;
    public AudioClip shootAudio;
    public AudioClip reloadAudio;

    MuzzelFlash muzzelFlash;
    float nextShotTime;

    bool triggerReleasedSinceLastShot = true;
    
    [HideInInspector]
    public bool dissolved = false;
    
    int shotsRemaingInBurst;
    bool isReloading;

    Vector3 recoilSmoothDampVelocity;
    float recoilRotSmoothDampVelocity;
    float recoilAngle;

    void Start()
    {
        muzzelFlash = GetComponent<MuzzelFlash>();
        shotsRemaingInBurst = burstCount;
        projectleRemainingInMag = projectilesPerMag;
        FindObjectOfType<Player>().ammotxt.text = (projectleRemainingInMag / burstCount).ToString();
    }

    void LateUpdate()
    {
        FindObjectOfType<Player>().ammotxt.text = (projectleRemainingInMag / burstCount).ToString();
        if (!isReloading && projectleRemainingInMag == 0)
        {
            Reload();
        }

        if (!isReloading)
        {
            //animate recoil
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilMoveSettleTime);
            recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotSmoothDampVelocity, recoilRotationSettleTime);
            transform.localEulerAngles = transform.localEulerAngles = Vector3.left * recoilAngle;
        }
    }

    void Shoot()
    {
        if (!isReloading && Time.time > nextShotTime && projectleRemainingInMag > 0)
        {
            if (fireMode == FireMode.Burst)
            {
                if (shotsRemaingInBurst == 0)
                {
                    return;
                }
                shotsRemaingInBurst--;
            }
            else if (fireMode == FireMode.Single)
            {
                if (!triggerReleasedSinceLastShot)
                {
                    return;
                }
            }

            for (int i = 0; i < projectileSpawnPoint.Length; i++)
            {
                if (projectleRemainingInMag == 0)
                {
                    break;
                }
                projectleRemainingInMag--;
                nextShotTime = Time.time + msBetweenShots / 1000;
                Projectile newProjectile = Instantiate(projectile, projectileSpawnPoint[i].position, projectileSpawnPoint[i].rotation) as Projectile;
                if (projectileMat != null)
                {
                    newProjectile.GetComponent<Renderer>().material = projectileMat;
                }
                if (tracerMat != null)
                {
                    newProjectile.GetComponent<TrailRenderer>().material = tracerMat;
                }
                newProjectile.damage = gunDamage;
                newProjectile.SetSpeed(muzzleVelocity);
            }

            Transform newShell = Instantiate(shell, shellEjection.position, shellEjection.rotation);
            if (shellMat != null)
            {
                newShell.GetComponent<Renderer>().material = shellMat;
            }
            muzzelFlash.Activate();
            transform.localPosition -= Vector3.forward * Random.Range(kickMinMax.x,kickMinMax.y);
            recoilAngle += Random.Range(recoilAngMinMax.x, recoilAngMinMax.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);

            AudioManager.instance.PlaySound(shootAudio, transform.position);
        }
    }

    public void Reload()
    {
        if (!isReloading && projectleRemainingInMag != projectilesPerMag)
        {
            StartCoroutine(AnimateReload());
            AudioManager.instance.PlaySound(reloadAudio, transform.position);
        }
    }

    IEnumerator AnimateReload()
    {
        isReloading = true;
        yield return new WaitForSeconds(.2f);

        float reloadSpeed = 1f / reloadTime;
        float percent = 0;
        Vector3 initialRot = transform.localEulerAngles;
        float maxReloadingAngle = 60;

        while (percent < 1)
        {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            float reloadAngle = Mathf.Lerp(0, maxReloadingAngle, interpolation);
            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;

            yield return null;
        }

        isReloading = false;
        projectleRemainingInMag = projectilesPerMag;;
    }

    public void Aim(Vector3 aimPoint)
    {
        if (!isReloading)
        {
            transform.LookAt(aimPoint);
        }
    }

    public void OnTriggerHold()
    {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease()
    {
        triggerReleasedSinceLastShot = true;
        shotsRemaingInBurst = burstCount;

    }

}
