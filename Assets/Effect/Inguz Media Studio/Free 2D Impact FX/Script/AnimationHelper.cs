using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InguzPings
{

public class AnimationHelper : MonoBehaviour {
	public bool INFINITE_LOOP = true; //If true, then duration is ignored and goes on and on.
	public float duration = 5.5f; //Fixed duration of the vfx.
	public bool DESTROY_ON_END = true;
	public GameObject[] all_particles;
	private float start_time = -100.0f;
	private bool GO = true;


	void Start () 
	{
		start_time = Time.time;	
	}


	void Update () 
	{
		if (!INFINITE_LOOP && duration + start_time <= Time.time && GO)
			STOP_VFX();
	}


	public void STOP_VFX()
	{
		GO = false;
		for(int i = 0; i < all_particles.Length; i++)
	        all_particles[i].GetComponent<ParticleSystem>().Stop();
				
		if (DESTROY_ON_END) 
			Destroy (gameObject, 1.0f); //A little time before destroying, to let remaining particles to die first. It looks better.
	}


	public void PLAY_VFX()
	{
		for(int i = 0; i < all_particles.Length; i++)
	        all_particles[i].GetComponent<ParticleSystem>().Play();
		
		start_time = Time.time;
		GO = true;
	}

	
}

}