using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Networking;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class CustomNetworkManager : NetworkManager
{
    private float nextRefreshTime;
    public void StartHosting()
    {
        StartMatchMaker();
        matchMaker.CreateMatch("dupa", 4, true, "", "", "", 0, 0, OnMatchCreated);
        base.StartHost();
        GameObject.Find("Matchmaking").SetActive(false);
    }

    private void OnMatchCreated(bool success, string extendedinfo, MatchInfo responsedata)
    {
        base.StartHost(responsedata);
        RefreshMatches();
    }

    private void Update()
    {
        if (Time.time >= nextRefreshTime)
        {
            RefreshMatches();
        }
    }

    private void RefreshMatches()
    {
        nextRefreshTime = Time.time + 5f;
        if (matchMaker == null)
        {
            StartMatchMaker();
        }

        matchMaker.ListMatches(0, 10, "", true, 0, 0, HandleMatchListComplete);
    }

    private void HandleMatchListComplete(bool success, string extendedinfo, List<MatchInfoSnapshot> responsedata)
    {
        AvailableMatchesList.HandleNewMatchList(responsedata);
    }

    public void JoinGame(MatchInfoSnapshot match)
    {
        if(matchMaker == null)
            StartMatchMaker();

        matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, HandleJoinedMatch);
        GameObject.Find("Matchmaking").SetActive(false);
    }

    private void HandleJoinedMatch(bool success, string extendedinfo, MatchInfo responsedata)
    {
        StartClient(responsedata);
    }
}
