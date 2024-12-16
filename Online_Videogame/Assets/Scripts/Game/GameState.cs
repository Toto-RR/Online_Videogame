using System;
using System.Collections.Generic;

[Serializable]
public class GameState
{
    public enum ServerState
    {
        LOBBY,
        IN_GAME,
    }
    public ServerState currentState = ServerState.LOBBY;
    public List<PlayerData> Players = new List<PlayerData>();

    public GameState(ServerState state, List<PlayerData> players)
    {
        currentState = state;
        Players = players;
    }
}