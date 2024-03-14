using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialColorFade : MonoBehaviour {
    public Material particleMaterial;
    public Material backgroundMaterial;
    float colorOffset = 0;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        colorOffset += Time.deltaTime* 0.01f;
        if (colorOffset > 1) colorOffset = 0;

        particleMaterial.SetColor("_TintColor", Color.HSVToRGB(colorOffset, 1, 1));
        backgroundMaterial.SetColor("_TintColor", Color.HSVToRGB(colorOffset, 1, 0.05f));
    }
}
