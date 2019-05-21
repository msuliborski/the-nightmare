
using UnityEngine;

public class GridPoint : MonoBehaviour
{
    [SerializeField] private Sprite _red;
    private Sprite _green;
    [SerializeField] private SpriteRenderer _sprite;
    
    [SerializeField] private bool _buildable = true;
    public bool Buildable
    {
        get { return _buildable; }
        set
        {
            _buildable = value;
            changeSprite(value);
        }
    }

    void Start()
    {
        _red = (Sprite)Resources.Load("red", typeof(Sprite));
        _green = (Sprite)Resources.Load("green", typeof(Sprite));
    }

    public void setSpriteRenderer(GameObject gridRender)
    {
        _sprite = gridRender.GetComponent<SpriteRenderer>();
    }

    public void changeSprite(bool isGreen)
    {
        Debug.Log("Changing colour");
        if (isGreen)
            _sprite.sprite = _green;
        else
        {
            Debug.Log("Changing to red");
            _sprite.sprite = _red;
        }
            
    }
}
