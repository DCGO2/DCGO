using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSnake : MonoBehaviour {
    float tick = 0;
    float rotZ = 0;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        tick += Time.deltaTime;
        rotZ += 0.02f;

        Vector3 pos = transform.position;

        pos.y = Mathf.Sin(tick / 10f) * 8f;

        transform.rotation = Quaternion.Euler(0 , Mathf.Sin(tick/5f) /5f * Mathf.Rad2Deg ,rotZ);
        transform.position = pos;
	}
}
