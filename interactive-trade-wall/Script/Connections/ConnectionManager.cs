using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class ConnectionManager : MonoBehaviourPunCallbacks
{
    public static ConnectionManager Instance { get; private set; }

    #region Public Fields
    public const byte OnMaterialButtonClickEventCode = 1;
    public const byte OnImportRouteButtonClickEventCode = 2;
    public const byte OnExportRouteButtonClickEventCode = 3;
    public const byte OnBackButtonClickEventCode = 4;

    #endregion 

    #region Private Fields
    string gameVersion = "1";

    #endregion

    #region MonobehaviorCallBacks

    
    void Awake()
    {
        //PhotonNetwork.AutomaticallySyncScene = true;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Connect();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MaterialClickedinBook(0,0);
        }
    }
    #endregion

    #region  Public Methods

    public void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {

        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }

    public void MaterialClickedinBook(int bookMarkid,int itemId)
    {
        object[] content = new object[] { bookMarkid,itemId }; // Array contains the target position and the IDs of the selected units
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        PhotonNetwork.RaiseEvent(OnMaterialButtonClickEventCode, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void RaiseEventForRouteClick(int value, bool _isClicked) // 1= Import , 2 = Export
    {

        bool content = _isClicked;
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(value == 1 ? OnImportRouteButtonClickEventCode : OnExportRouteButtonClickEventCode, content,
                                                        raiseEventOptions, SendOptions.SendReliable);
    }

    public void RaiseEventForBackButtonClick()
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(OnBackButtonClickEventCode, null, raiseEventOptions, SendOptions.SendReliable);
    }

    #endregion




    #region MonoBehaviourPunCallbacks Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");

        PhotonNetwork.JoinOrCreateRoom("BookRoom", new RoomOptions { MaxPlayers = 2 }, TypedLobby.Default);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Room Joined");
    }
    #endregion


   
}
