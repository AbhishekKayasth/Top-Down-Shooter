using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
	
	#region Variables
    public Transform target;

    public float smmothSpeed = 10f;
    public Vector3 offset;
	#endregion
	
	#region Unity Methods
    void FixedUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPostion = target.position + offset;
            Vector3 smoothPosition = Vector3.Lerp(transform.position, desiredPostion, smmothSpeed * Time.deltaTime);
            transform.position = desiredPostion;
        }
    }

	#endregion

}
