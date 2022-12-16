using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent (typeof (PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity {

    public float moveSpeed = 5;
    public int score;
    public int money;
    public int currentScore = 0;
    public int currentMoney = 0;

    public Crosshairs crosshairs;
    public TMP_Text scoretxt;
    public TMP_Text moneytxt;
    public TMP_Text healthtxt;
    public TMP_Text ammotxt;
    public Slider healthBar;
    public Light pointlight;

    Camera viewCamera;
    PlayerController controller;
    GunController gunController;
    Gun gunToEquip;

    Color ogFresnelColor;
    Color fresnelColor;


    protected override void Start () {
        DisplayState();
        base.Start();
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;

        fresnelColor = GetComponent<MeshRenderer>().sharedMaterial.GetColor("_FresnelColor");
        ogFresnelColor = fresnelColor;
    }
	
	void Update () {
        // Movement Input
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move(moveVelocity);

        // Look Input
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            //Debug.DrawLine(ray.origin, point, Color.red);
            controller.LookAt(point);
            crosshairs.transform.position = point;
            crosshairs.DetectTarget(ray);
            //print((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).magnitude);
            if ((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > Mathf.Pow(2.3f, 2f))
            {
                gunController.Aim(point);
            }
        }
        // Weapon Input
        if (Input.GetMouseButton(0))
        {
            gunController.OnTriggerHold();
        }
        if (Input.GetMouseButtonUp(0))
        {
            gunController.OnTriggerRelease();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            gunController.Reload();
        }

        // Weapon Switch
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            gunToEquip = gunController.pistol;
            gunController.GunToEquip(gunToEquip);
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            gunToEquip = gunController.sawedOff;
            gunController.GunToEquip(gunToEquip);
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            gunToEquip = gunController.smg;
            gunController.GunToEquip(gunToEquip);
        }
        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            gunToEquip = gunController.shotgun;
            gunController.GunToEquip(gunToEquip);
        }
        if (Input.GetKeyUp(KeyCode.Alpha5))
        {
            gunToEquip = gunController.carbine;
            gunController.GunToEquip(gunToEquip);
        }
        if (Input.GetKeyUp(KeyCode.Alpha6))
        {
            gunToEquip = gunController.assault;
            gunController.GunToEquip(gunToEquip);
        }

        healthBar.value = health/100;
        healthtxt.text = health.ToString();

	}

    public override void Die()
    {
        healthBar.value = 0;
        healthtxt.text = "0";
        AudioManager.instance.PlaySound("Player Death", transform.position);
        GetComponent<MeshRenderer>().sharedMaterial.SetColor("_FresnelColor", ogFresnelColor);
        base.Die();
    }

    public void DisplayState()
    {
        scoretxt.text = currentScore.ToString();
        moneytxt.text = currentMoney.ToString();
    }
    public void LightControl(float rate)
    {
        if (pointlight.intensity > 1)
        {
            pointlight.intensity += rate * Time.deltaTime;
        }

        float intensity = (fresnelColor.r + fresnelColor.g + fresnelColor.b) / 3;
        if (intensity > 0.5)
        {
            float newIntensity = intensity + rate * Time.deltaTime;
            float factor = newIntensity / intensity;
            fresnelColor = new Color(fresnelColor.r * factor, fresnelColor.g * factor, fresnelColor.b * factor, fresnelColor.a);
            GetComponent<MeshRenderer>().sharedMaterial.SetColor("_FresnelColor", fresnelColor);
        }
    }
}
