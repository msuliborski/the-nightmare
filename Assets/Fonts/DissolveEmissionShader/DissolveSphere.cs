using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveSphere : MonoBehaviour {

    Material mat;
    
    private float _dissolveAmount = 0f;
   
    public float DissolveAmount { get => _dissolveAmount; }

    private void Start() {
        mat = GetComponent<Renderer>().material;
       
    }

    private void Update() {
       _dissolveAmount += Time.deltaTime / 4;
       mat.SetFloat("_DissolveAmount", DissolveAmount);
       
    }
}