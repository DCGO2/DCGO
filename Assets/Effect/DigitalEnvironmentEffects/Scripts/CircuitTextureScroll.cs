using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircuitTextureScroll : MonoBehaviour {

    ParticleSystem ps;

    Material particleMaterial;

    Vector2 partOffset;

    public float partTextureScrollSpeed = -1;

	// Use this for initialization
	void Start () {
        ps = GetComponent<ParticleSystem>();
        particleMaterial = ps.GetComponent<Renderer>().material;
        partOffset = particleMaterial.mainTextureOffset;
        
	}
	
	// Update is called once per frame
	void LateUpdate () {
        //Scroll texture according to speed set in the editor
        partOffset.y += partTextureScrollSpeed * Time.deltaTime;
        if (partOffset.y <= -1)
        {
            partOffset.y = 0;
        }
        particleMaterial.mainTextureOffset = partOffset;
    }
}
