public class API {
    public static string APIDevelopmentBaseURL = "https://api-csmvs.focalat.com/";
    public static string APIProductionBaseURL = "https://awg-api.csmvs.org.in/";

    public static string APILogin = APIBaseURL + "v1/login";
    public static string APIGetTradeRouteSO_En = APIBaseURL + "v1/unity/get-bookmarks?lang=english";
    public static string APIGetTradeRouteSO_Mr = APIBaseURL + "v1/unity/get-bookmarks?lang=marathi";

    public static string APIBaseURL {
        get {
            switch (APICall.Instance.serverType) {
                case Server.Live:
                return APIProductionBaseURL;
                case Server.Development:
                return APIDevelopmentBaseURL;
            }

            return APIProductionBaseURL;
        }
    }

    public enum Server {
        Live,
        Development,
    }
}
