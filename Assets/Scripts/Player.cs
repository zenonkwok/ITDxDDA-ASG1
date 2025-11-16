using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

[Serializable]
public class Player
{
    public string playerId;
    public string name;
    public int score;

    public Player(string id, string name)
    {
        playerId = id;
        this.name = name;
    }
}
