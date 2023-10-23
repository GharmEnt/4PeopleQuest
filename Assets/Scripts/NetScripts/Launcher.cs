using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button findRoomButton;
    [SerializeField] private Button nickNameButton;

    [SerializeField] private InputField nickNameIF;
    [SerializeField] private InputField createRoomIF;
    [SerializeField] private InputField findRoomIF;


    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("подключаемся к серверу");
    }

    private void Update()
    {
        nickNameButton.interactable = string.IsNullOrEmpty(nickNameIF.text) ? false : true;
        createRoomButton.interactable = string.IsNullOrEmpty(nickNameIF.text) && string.IsNullOrEmpty(createRoomIF.text) ? false : true;
        findRoomButton.interactable = string.IsNullOrEmpty(nickNameIF.text) && string.IsNullOrEmpty(findRoomIF.text) ? false : true;
    }

    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 3;
        PhotonNetwork.CreateRoom(createRoomIF.text, roomOptions, TypedLobby.Default);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(findRoomIF.text);
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void NickNameSave()
    {
        PhotonNetwork.NickName = nickNameIF.text;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Ure joined room");
        PhotonNetwork.LoadLevel(1);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Ure not joined room");
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room created successful");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Room isnt created");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Ure connected to server");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Ure connected to lobby");
    }
}
