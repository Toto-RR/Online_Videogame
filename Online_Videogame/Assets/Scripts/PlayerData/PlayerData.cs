using System;
using UnityEngine;

public enum CommandType
{
    JOIN,
    MOVE,
    SHOOT,
    DIE,
    DISCONNECTED
}

[Serializable]
public class PlayerData
{
    public string PlayerId;        // Player ID
    public string PlayerName;      // Player Name
    public Vector3 Position;       // Position
    public Quaternion Rotation;    // Rotation
    public float Health;             // Health points 
    public int Energy;             // Energy
    public int AmmoCount;          // Ammo
    public float Damage;             // Damage

    public string TargetPlayerId;
    public CommandType Command;         // Commands (JOIN, MOVE, SHOOT, etc.)
}
