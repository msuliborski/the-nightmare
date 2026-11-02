using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExPointBlink : MonoBehaviour
{

    private SpriteRenderer _spriteRenderer;
    public SpriteRenderer SpriteRenderer { get { return _spriteRenderer; } }
    private Color _prepareColor = new Color(1f, 140f / 255f, 0f); // Color is 0..1; 255,140,0 rendered as white
    private Color _baseColor = Color.red;
    private Coroutine _blink;
    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.color = _prepareColor;
    }


    private IEnumerator Blink()
    {
        yield return new WaitForSeconds(0.5f);
        _spriteRenderer.enabled = false;
        yield return new WaitForSeconds(0.5f);
        _spriteRenderer.enabled = true;
        _blink = StartCoroutine(Blink());
    }

    public void  StartBlink()
    {
        if (_blink != null) StopCoroutine(_blink);
        _spriteRenderer.color = _baseColor;
        _blink = StartCoroutine(Blink());
    }

    public void StopBlink()
    {
        _spriteRenderer.color = _prepareColor;
        _spriteRenderer.enabled = true; // could be stopped in the "off" half of a blink
        if (_blink != null) StopCoroutine(_blink);
        _blink = null;
    }
}
