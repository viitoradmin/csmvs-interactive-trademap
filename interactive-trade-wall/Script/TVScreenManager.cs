using System;
using UnityEngine;

public class TVScreenManager : MonoBehaviour
{

    [SerializeField] GameObject landingScreenParent;
    [SerializeField] GameObject detailedScreenParent;

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
    }

    public void ShowDetailedScreen()
    {
        landingScreenParent.SetActive(false);
        detailedScreenParent.SetActive(true);

        FocusMesopotamia();
    }
    public void ToggleImportRouteDisplay(bool _value)
    {
        ImportRoute.SetActive(_value);
    }

    public void ToggleExportRouteDisplay(bool _value)
    {
        ExportRoute.SetActive(_value);
    }
    
    public MapCameraFocus mapCam;
public BoxCollider2D indiaRegion, mesopotamiaRegion;

public void FocusIndia() => mapCam.FocusOn(indiaRegion);
public void FocusMesopotamia() => mapCam.FocusOn(mesopotamiaRegion);
public void FocusAll() => mapCam.FocusFullMap();
}
