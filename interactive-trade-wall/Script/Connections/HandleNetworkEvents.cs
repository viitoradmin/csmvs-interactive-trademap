using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
public class HandleNetworkEvents : MonoBehaviour , IOnEventCallback
{
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    void IOnEventCallback.OnEvent(EventData photonEvent)
    {
        //OnEvent(photonEvent);
        byte eventCode = photonEvent.Code;
        Debug.Log("Event code recieved" + eventCode + ">" + photonEvent.CustomData);

        if (photonEvent.Code == 1)
        {
            object[] data = (object[])photonEvent.CustomData;
            int bookmarkId = (int)data[0];
            int itemId = (int)data[1];
            Debug.Log("bookmark" + bookmarkId + "item id" + itemId);
           // TVScreenManager.Instance.ShowDetailedScreen();
        }

        if (photonEvent.Code == 2)
        {
            Debug.Log((bool)photonEvent.CustomData);
          //  TVScreenManager.Instance.ToggleImportRouteDisplay((bool)photonEvent.CustomData);
        }

        if (photonEvent.Code == 3)
        {
            Debug.Log((bool)photonEvent.CustomData);
            //TVScreenManager.Instance.ToggleExportRouteDisplay((bool)photonEvent.CustomData);
        }

        if (photonEvent.Code == 4)
        {
            Debug.Log("back button clicked in Kiosk");
            TVScreenManager.Instance.ShowMainScreen();
        }

    }
}
