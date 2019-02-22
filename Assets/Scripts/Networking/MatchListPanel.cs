using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Networking;
using UnityEngine;
using UnityEngine.Networking.Match;

public class MatchListPanel : MonoBehaviour
{
    [SerializeField] private JoinButton _joinButtonPrefab;
    
    private void Awake()
    {
        AvailableMatchesList.OnAvailableMatchesChange += AvailableMatchesList_OnAvailableMatchesChanged;
    }

    private void AvailableMatchesList_OnAvailableMatchesChanged(
        List<MatchInfoSnapshot> matches)
    {
        ClearExistingButtons();
        CreateJoinMatchButtons(matches);
    }

    private void ClearExistingButtons()
    {
        var buttons = GetComponentsInChildren<JoinButton>();
        foreach (var button in buttons)
        {
            Destroy(button.gameObject);
        }
    }

    private void CreateJoinMatchButtons(List<MatchInfoSnapshot> matches)
    {
        foreach (var match in matches)
        {
            var button = Instantiate(_joinButtonPrefab);
            button.Initialize(match, transform);
        }
    }
}
