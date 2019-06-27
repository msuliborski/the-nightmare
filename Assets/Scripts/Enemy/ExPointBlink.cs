using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExPointBlink : MonoBehaviour
{

    private SpriteRenderer _spriteRenderer;
    public SpriteRenderer SpriteRenderer { get { return _spriteRenderer; } }
    private Color _prepareColor = new Color(255f, 140f, 0f);
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
        _spriteRenderer.color = _baseColor;
        _blink = StartCoroutine(Blink());
    }

    public void StopBlink()
    {
        _spriteRenderer.color = _prepareColor;
        StopCoroutine(_blink);
    }
}
