using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class RoomInfoUI : MonoBehaviour
{
    public TMP_Text RoomNameText;
    public TMP_Text RoomPlayersText;
    public Button JoinRoomButton;

    private string roomName;

    public void Start()
    {
        JoinRoomButton.onClick.AddListener(() =>
        {
            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.LeaveLobby();
            }

            PhotonNetwork.JoinRoom(roomName);
        });
    }

    public void Initialize(string name, byte currentPlayers, byte maxPlayers)
    {
        roomName = name;

        RoomNameText.text = name;
        RoomPlayersText.text = currentPlayers + " / " + maxPlayers;
    }
}
