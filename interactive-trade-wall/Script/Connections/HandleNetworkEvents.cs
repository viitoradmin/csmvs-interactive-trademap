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
            TVScreenManager.Instance.ShowDetailedScreen();
        }

        if (photonEvent.Code == 2)
        {
            Debug.Log((bool)photonEvent.CustomData);
            TVScreenManager.Instance.ToggleImportRouteDisplay((bool)photonEvent.CustomData);
        }

        if (photonEvent.Code == 3)
        {
            Debug.Log((bool)photonEvent.CustomData);
            TVScreenManager.Instance.ToggleExportRouteDisplay((bool)photonEvent.CustomData);
        }

    }
}
