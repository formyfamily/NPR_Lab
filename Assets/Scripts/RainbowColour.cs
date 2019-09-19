using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RainbowColour : MonoBehaviour {

    Renderer rend;
    Material material;
    Mesh mesh;

    // Use this for initialization
    void Start () {
        rend = GetComponent<Renderer>();
        material = rend.material;
        material.SetColor("_Colour", Color.magenta);
        mesh = GetComponent<Mesh>();
        Unwrapping.GenerateSecondaryUVSet(mesh);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
