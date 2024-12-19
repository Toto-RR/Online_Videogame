using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

[System.Serializable]
public class LobbyState
{
    public List<LobbyPlayerData> Players = new List<LobbyPlayerData>();
    public bool isGameStarted = false; // Nuevo campo para indicar si el juego ha comenzado

    public void AddPlayer(LobbyPlayerData playerData)
    {
        if (!Players.Exists(p => p.PlayerId == playerData.PlayerId))
        {
            Players.Add(new LobbyPlayerData(playerData.PlayerId, playerData.PlayerName));
            LobbyManager.Instance.UpdateLobbyUI(Players);
        }
    }

    public void SetPlayerReady(string playerId, Color playerColor)
    {
        var player = Players.Find(p => p.PlayerId == playerId);
        if (player != null)
        {
            player.IsReady = true;
            player.PlayerColor = playerColor;
            Debug.Log($"Player {player.PlayerName} está Ready con color {player.PlayerColor}");

            LobbyManager.Instance.UpdateLobbyUI(Players);
        }
    }


    public bool AreAllPlayersReady()
    {
        foreach (var player in Players)
        {
            if (!player.IsReady)
            {
                Debug.Log(player.PlayerName + " is not ready");
                return false;
            }
        }
        return true;
    }
}
