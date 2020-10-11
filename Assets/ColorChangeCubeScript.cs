using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

// public class ColorChangeCubeScript : MonoBehaviour
public class ColorChangeCubeScript : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject colorChangeCube;
    private const byte COLOR_CHANGE_EVENT = 0;
    string gameVersion = "1";
    bool isConnecting;
    private static byte maxPlayersPerRoom = 4;
    RoomOptions roomOptions;
    private string defaultRoomName = "defaultExerciseRoom1";

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        roomOptions = new RoomOptions { MaxPlayers = maxPlayersPerRoom };
    }
    void Start()
    {   // connect method
        Connect();
    }

    public void Connect()
    {
        Debug.Log("Connect()ing");
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Connect() IsConnected: " + PhotonNetwork.IsConnected);
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            Debug.Log("Connect() IsConnected: " + PhotonNetwork.IsConnected);
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;

            PhotonNetwork.JoinRandomRoom();
            // JoinRandomRoom failed.
            // Client is on NameServer (must be Master Server for matchmaking) 
            // but not ready for operations (State: ConnectingToNameServer). 
            // Wait for callback: OnJoinedLobby or OnConnectedToMaster.

        }
        Debug.Log("Connect()ed");
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
        if (isConnecting)
        {
            // _joinOrCreateRoom();
        }
    }

    private void _joinOrCreateRoom()
    {
        Debug.Log("JoinOrCreateRoom " + defaultRoomName);
        PhotonNetwork.JoinOrCreateRoom(defaultRoomName, roomOptions, TypedLobby.Default);
        isConnecting = false;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
        isConnecting = false;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("---PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
        PhotonNetwork.CreateRoom(defaultRoomName, roomOptions);
    }

    public override void OnErrorInfo(ErrorInfo errorInfo)
    {
        Debug.Log("OneErrorInfo: " + errorInfo);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
    }


    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }

    private void NetworkingClient_EventReceived(EventData obj)
    {
        if (obj.Code == COLOR_CHANGE_EVENT)
        {
            Debug.Log("Received event: COLOR_CHANGE_EVENT");
            object[] datas = (object[])obj.CustomData;

            float r = (float)datas[0];
            float g = (float)datas[1];
            float b = (float)datas[2];

            setCubeColor(r, g, b);
        }
    }

    private void setCubeColor(float r, float g, float b)
    {
        //Get the Renderer component from the new cube
        var cubeRenderer = colorChangeCube.GetComponent<Renderer>();

        //Call SetColor using the shader property name "_Color" and setting the color to red
        cubeRenderer.material.SetColor("_Color", new Color(r, g, b, 1f));
    }

    void Update()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("PhotonNetwork.IsConnected: " + PhotonNetwork.IsConnected);
        }

        if (isConnecting)
        {
            if (photonView.IsMine && Input.GetKey(KeyCode.Space)) // or GetKeyDown
            {
                float r = Random.Range(0f, 1f);
                float g = Random.Range(0f, 1f);
                float b = Random.Range(0f, 1f);

                // cant specify incoming objects
                object[] datas = new object[] { r, g, b }; // base.photonView.ViewID,
                PhotonNetwork.RaiseEvent(COLOR_CHANGE_EVENT, datas, RaiseEventOptions.Default, SendOptions.SendUnreliable);
            }
            if (!base.photonView.IsMine && Input.GetKey(KeyCode.Space)) // or GetKeyDown
            {
                Debug.Log("not mine");
            }
        }

    }
}
