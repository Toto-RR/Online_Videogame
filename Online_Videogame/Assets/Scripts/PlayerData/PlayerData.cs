using System;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public string PlayerId;        // ID del player
    public string PlayerName;      // Nombre del player
    public Vector3 Position;       // Posici�n
    public Quaternion Rotation;    // Rotaci�n
    public string Command;         // Comandos (JOIN, MOVE, SHOOT, etc.)
}
