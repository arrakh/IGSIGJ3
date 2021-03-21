using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Cinemachine;

public class GameManager : PhotonSingleton<GameManager>
{
    public List<Transform> spawnpoints = new List<Transform>();
    public UnityEngine.UI.Text tempText;
    public InputManager inputManager;
    public CinemachineVirtualCamera vCam;
    public bool isCamShaking = false;

    private float currentShakeTimer = 0f;

    public override void OnEnable()
    {
        base.OnEnable();

        CountdownTimer.OnCountdownTimerHasExpired += OnCountdownTimerHasExpired;
    }

    public override void OnDisable()
    {
        base.OnDisable();

        CountdownTimer.OnCountdownTimerHasExpired -= OnCountdownTimerHasExpired;
    }

    private void OnCountdownTimerHasExpired()
    {
        StartGame();
    }

    private void Start()
    {
        Hashtable properties = new Hashtable
        {
            {GameConstants.PLAYER_HAS_LOADED_LEVEL, true }
        };
        var succeed = PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
        Debug.Log("Start Func! Set Custom Properties Succeeded: " + succeed);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.Log("Discconected! " + cause);
        SceneManager.LoadScene("S_Lobby");
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        Debug.Log("Left Room!");
        PhotonNetwork.Disconnect();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            //Put stuff here in case there's a function only the master client called
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        CheckEndOfGame();
    }

    private void StartGame()
    {
        Debug.Log("Game Started...");

        //TODO: figure out if rejoin

        Transform spawn = spawnpoints[PhotonNetwork.LocalPlayer.GetPlayerNumber()];

        PlayerController pc = PhotonNetwork.Instantiate("PlayerNetworkPrefab", spawn.position, Quaternion.identity).GetComponent<PlayerController>();

        Debug.Log("Player Controller is Null: " + pc == null);

        inputManager.currentPlayer = pc;
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(GameConstants.PLAYER_LIVES))
        {
            CheckEndOfGame();
            Debug.Log("Properties contains PLAYER_LIVES");
            return;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }


        //Will only be run by master client only at this point
        int startTimestamp;
        bool startTimeIsSet = CountdownTimer.TryGetStartTime(out startTimestamp);

        if (changedProps.ContainsKey(GameConstants.PLAYER_HAS_LOADED_LEVEL))
        {
            if (CheckAllPlayerHasLoaded())
            {
                if (!startTimeIsSet)
                {
                    CountdownTimer.SetStartTime();
                }
            }
            else
            {
                //Waiting for player
                Debug.Log("Not all player is loaded! waiting for players...");
            }
        }
    }

    private bool CheckAllPlayerHasLoaded()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            object playerLoadedLevel;

            if(player.CustomProperties.TryGetValue(GameConstants.PLAYER_HAS_LOADED_LEVEL, out playerLoadedLevel))
            {
                if((bool)playerLoadedLevel)
                {
                    Debug.Log(player.NickName + " has loaded!");
                    continue;
                }
            }

            Debug.Log(player.NickName + " has not loaded!");

            return false;
        }

        Debug.Log("All player has loaded!");

        return true;
    }

    private void CheckEndOfGame()
    {
        Debug.Log("<color=green>Checking End Of Game</color>");
        List<Player> survivors = new List<Player>();

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            object lives;
            if (p.CustomProperties.TryGetValue(GameConstants.PLAYER_LIVES, out lives))
            {
                if ((int)lives > 1)
                {
                    Debug.Log("Player " + p.NickName + " Lives!");
                    survivors.Add(p);
                }
            }
        }

        Debug.Log("Survivors left: " + survivors.Count);

        if (survivors.Count <= 1)
        {
            Debug.Log("<color=green>Game Ended! " + survivors[0].NickName + " wins!</color>");
            if (PhotonNetwork.IsMasterClient)
            {
                StopAllCoroutines();
            }

            StartCoroutine(VictoryCoroutine(survivors[0]));

        }
    }

    private IEnumerator VictoryCoroutine(Player winner)
    {
        tempText.text = winner.NickName + " wins!";

        yield return new WaitForSeconds(3f);

        PhotonNetwork.LeaveRoom();
    }

    public IEnumerator StartShake(float duration, float strength = 1f)
    {
        isCamShaking = true;

        var noise = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_FrequencyGain = strength;

        float timer = duration;

        while (timer > 0)
        {
            yield return new WaitForEndOfFrame();

            noise.m_AmplitudeGain = timer * strength;

            timer -= Time.deltaTime;
        }

        noise.m_AmplitudeGain = 0f;

        isCamShaking = false;
    }
}
