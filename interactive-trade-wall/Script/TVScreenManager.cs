using System;
using UnityEngine;

public class TVScreenManager : MonoBehaviour
{

    [SerializeField] GameObject landingScreenParent;
    [SerializeField] GameObject detailedScreenParent;
    [SerializeField] GameObject mainScreenRoutesParent;
    [SerializeField] GameObject ImportRoute;
    [SerializeField] GameObject ExportRoute;

    public static TVScreenManager Instance { get; private set; }
    void Awake()
    {
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
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            FocusMesopotamia();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            FocusAll();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {

            mapCam.MovetoActual(new Vector2(0f, -59.4f), 5.4f,
                   () =>
                   {
                       Debug.Log("Camera Animation Completed");
                       mainScreenRoutesParent.SetActive(true);
                   });

        }   
    }

    public void ShowDetailedScreen()
    {
        landingScreenParent.SetActive(false);
        detailedScreenParent.SetActive(true);
        mainScreenRoutesParent.SetActive(false);
        FocusMesopotamia();
    }

    public void ShowMainScreen()
    {
        detailedScreenParent.SetActive(false);
        landingScreenParent.SetActive(true);
        ToggleExportRouteDisplay(true);
        ToggleImportRouteDisplay(true);
        
        mapCam.MovetoActual(new Vector2(0f, -59.4f), 5.4f,
                   () =>
                   {
                       Debug.Log("Camera Animation Completed");
                       mainScreenRoutesParent.SetActive(true);
                   });
    }
    public void ToggleImportRouteDisplay(bool _value)
    {
        ImportRoute.SetActive(_value);
    }

    public void ToggleExportRouteDisplay(bool _value)
    {
        ExportRoute.SetActive(_value);
    }
    
    //------------------------------MAP Moving Code---------------
    public MapCameraFocus mapCam;
    public BoxCollider2D indiaRegion, mesopotamiaRegion , worldRegion;

    public void FocusIndia() => mapCam.FocusOn(indiaRegion);
    public void FocusMesopotamia() => mapCam.FocusOn(mesopotamiaRegion);

    public void FocuWorldMap() => mapCam.FocusOn(worldRegion);
    public void FocusAll() => mapCam.FocusFullMap();
}
