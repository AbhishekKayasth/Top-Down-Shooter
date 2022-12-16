using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour {

    public Transform weaponHold;
    public Material gunMat;

    public Material gunSkin;
    public Material bulletSkin;
    public Material shellSkin;
    public Material tracerSkin;

    public float dissolveRate = 1f;

    [Header("Guns")]
    public Gun pistol;
    public Gun sawedOff;
    public Gun smg;
    public Gun shotgun;
    public Gun carbine;
    public Gun assault;

    public Gun equippedGun;
    public Gun gunToEquip;
    string gunName;

    void Start()
    {
        gunName = "Pistol";
        EquipGun(pistol);
    }

    public void DeEquipGun()
    {
        equippedGun.enabled = false;
        StartCoroutine(Dissolve());
    }

    public void EquipGun(Gun _gunToEquip)
    {
        equippedGun = Instantiate(_gunToEquip, weaponHold.position, weaponHold.rotation) as Gun;
        equippedGun.name = gunName;
        gunToEquip = equippedGun;
        equippedGun.projectileMat = bulletSkin;
        equippedGun.shellMat = shellSkin;
        equippedGun.tracerMat = tracerSkin;
        equippedGun.transform.parent = weaponHold;
        equippedGun.enabled = false;
        StartCoroutine(UnDissolve());
    }

    public void GunToEquip(Gun gun)
    {
        if (equippedGun == null)
        {
            EquipGun(gun);
        }
        else if (gun.name != gunToEquip.name)
        {
            gunToEquip = gun;
            gunName = gunToEquip.name;
            DeEquipGun();
        }
    }

    public void OnTriggerHold()
    {
        if (equippedGun != null)
        {
            equippedGun.OnTriggerHold();
        }
    }

    public void OnTriggerRelease()
    {
        if (equippedGun != null)
        {
            equippedGun.OnTriggerRelease();
        }
    }

    public float GunHeight
    {
        get
        {
            return weaponHold.position.y;
        }
    }

    public void Aim(Vector3 aimPoint)
    {
        if (equippedGun != null)
        {
            equippedGun.Aim(aimPoint);
        }
    }

    public void Reload()
    {
        if (equippedGun != null)
        {
            equippedGun.Reload();
        }
    }

    IEnumerator Dissolve()
    {
        string dissolveFactor = "_DissolveFactor";
        float currDissolveFactor = gunMat.GetFloat(dissolveFactor);
        float minDissolveFactor = -1f;
        float maxDissolveFactor = 0.6f;

        while (currDissolveFactor < maxDissolveFactor)
        {
            currDissolveFactor += Time.deltaTime * dissolveRate;
            gunMat.SetFloat(dissolveFactor, currDissolveFactor);
            currDissolveFactor = gunMat.GetFloat(dissolveFactor);

            yield return null;
        }
        equippedGun.dissolved = true;
        Destroy(equippedGun.gameObject);
        EquipGun(gunToEquip);
    }

    IEnumerator UnDissolve()
    {
        float dissolveRate = 1f;

        string dissolveFactor = "_DissolveFactor";
        float currDissolveFactor = gunMat.GetFloat(dissolveFactor);

        float minDissolveFactor = -1f;
        //float maxDissolveFactor = 0.6f;

        while (currDissolveFactor > minDissolveFactor)
        {
            currDissolveFactor -= Time.deltaTime * dissolveRate;
            gunMat.SetFloat(dissolveFactor, currDissolveFactor);
            currDissolveFactor = gunMat.GetFloat(dissolveFactor);

            yield return null;
        }

        equippedGun.enabled = true;
    }

}
