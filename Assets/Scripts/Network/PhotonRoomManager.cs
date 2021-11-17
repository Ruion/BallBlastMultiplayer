using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonRoomManager : MonoBehaviourPunCallbacks
{
    public int maxPlayers = 2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnConnectedToMaster()
    {
        // create or join room
        RoomOptions roomOptions = new RoomOptions() { MaxPlayers = (byte)maxPlayers };
        PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: roomOptions);
    }

    public override void OnJoinedRoom()
    {
        // spawn player object
        GameObject newPlayer = PhotonNetwork.Instantiate("Player", new Vector3(0, 0.9f, 0), Quaternion.identity);
        newPlayer.name = "Player_" + PhotonNetwork.LocalPlayer.ActorNumber.ToString();
    }
}
