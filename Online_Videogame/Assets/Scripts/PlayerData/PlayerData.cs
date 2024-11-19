using System;
using UnityEngine;

[Serializable]
public class BaseMessage
{
    public string MessageType;
}

[Serializable]
public class PlayerData : BaseMessage
{
    public string PlayerId;        // Player ID
    public string PlayerName;      // Player Name
    public Vector3 Position;       // Position
    public Quaternion Rotation;    // Rotation
    public int Health;             // Health points 
    public int Energy;             // Energy
    public int AmmoCount;          // Ammo

    public string Command;         // Commands (JOIN, MOVE, SHOOT, etc.)
}
