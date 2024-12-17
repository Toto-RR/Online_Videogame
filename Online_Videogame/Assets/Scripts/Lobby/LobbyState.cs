using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

[System.Serializable]
public class LobbyState
{
    public List<LobbyPlayerData> Players = new List<LobbyPlayerData>();

    public void AddPlayer(string playerId, string playerName)
    {
        if (!Players.Exists(p => p.PlayerId == playerId))
        {
            Players.Add(new LobbyPlayerData(playerId, playerName));
            Debug.Log($"Player {playerName} joined the lobby.");
        }
    }

    public void SetPlayerReady(string playerId)
    {
        var player = Players.Find(p => p.PlayerId == playerId);
        if (player != null)
        {
            player.IsReady = true;
            Debug.Log($"Player {player.PlayerName} is ready.");
        }
    }

    public bool AreAllPlayersReady()
    {
        foreach (var player in Players)
        {
            if (!player.IsReady) return false;
        }
        return true;
    }
}
