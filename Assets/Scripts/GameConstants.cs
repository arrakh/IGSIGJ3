using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConstants
{
    public const float PLAYER_RESPAWN_TIME = 2.0f;

    public const int PLAYER_MAX_POINT = 0;

    public const string PLAYER_LIVES = "PlayerLives";
    public const string PLAYER_READY = "IsPlayerReady";
    public const string PLAYER_HAS_LOADED_LEVEL = "PlayerHasLoadedLevel";

    public static Color GetColor(int colorChoice)
    {
        switch (colorChoice)
        {
            case 0: return Color.red;
            case 1: return Color.blue;
            case 2: return Color.green;
            case 3: return Color.yellow;
            case 4: return Color.cyan;
            case 5: return Color.grey;
            case 6: return Color.magenta;
            case 7: return Color.white;
        }

        return Color.black;
    }
}
