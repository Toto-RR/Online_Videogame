using System;

public enum LobbyCommandType
{
    JOIN_LOBBY,
    READY,
    START_GAME,
    LEAVE_LOBBY
}

[System.Serializable]
public class LobbyPlayerData
{
    public string PlayerId;
    public string PlayerName;
    public bool IsReady;
    public LobbyCommandType Command; // Nuevo campo

    public LobbyPlayerData(string playerId, string playerName, LobbyCommandType command = LobbyCommandType.JOIN_LOBBY)
    {
        PlayerId = playerId;
        PlayerName = playerName;
        IsReady = false;
        Command = command; // Valor por defecto: JOIN_LOBBY
    }
}

