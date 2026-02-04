using UnityEngine.Events;
using ViitorCloud.API;
using ViitorCloud.API.StandardTemplates;
using static API;
using static InteractiveTradeWallDataSO;

public class APICall:PersistentLazySingleton<APICall> {
    public Server serverType;
    public void RequestTradeRouteData(string form,UnityAction<APIResponse<Root>> callbackOnSuccess,UnityAction<string> callbackOnFail) {
        ServerCommunication.Instance.SendRequestGet(form,
            callbackOnSuccess,callbackOnFail);
    }
    public void RequestLogin(string form,string url,UnityAction<APIResponse<LoginResponse>> callbackOnSuccess,UnityAction<string> callbackOnFail) {
        ServerCommunication.Instance.SendRequestPost(form,url,
            callbackOnSuccess,callbackOnFail);
    }
}//APICall class end.
