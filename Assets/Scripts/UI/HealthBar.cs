using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private TextMeshProUGUI percentage;
    private RectTransform full;
    public PlayerManager player;
    public bool playerEnabled;
    private float ratio;

    void Start()
    {
        percentage = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        full = transform.GetChild(1).GetComponent<RectTransform>();
    }

    void Update()
    {
        if (playerEnabled)
        {
            ratio = player._currentHealth / player._maxHealth;
            percentage.text = (int)(ratio * 100) + "%";
            full.localScale = new Vector3(ratio, full.localScale.y, full.localScale.z);
        }
    }
}