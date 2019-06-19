using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExPointBlink : MonoBehaviour
{

    private SpriteRenderer _spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(Blink());
    }


    private IEnumerator Blink()
    {
        yield return new WaitForSeconds(0.5f);
        _spriteRenderer.enabled = false;
        yield return new WaitForSeconds(0.5f);
        _spriteRenderer.enabled = true;
        StartCoroutine(Blink());
    }
}
