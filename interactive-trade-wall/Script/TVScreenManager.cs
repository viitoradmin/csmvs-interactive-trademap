using System;
using UnityEngine;

public class TVScreenManager : MonoBehaviour
{

    [SerializeField] GameObject landingScreenParent;
    [SerializeField] GameObject detailedScreenParent;
    [SerializeField] GameObject mainScreenRoutesParent;
    //[SerializeField] GameObject ImportRoute;
    //[SerializeField] GameObject ExportRoute;
    [SerializeField] private GameObject itemRouteDetailsParent;
    [SerializeField] private GameObject[] itemRoutes = new GameObject[8];
    [SerializeField] CameraBreathingMotion cameraBreathingMotion;
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

    void ManageItemDetailsObjects(int selectedItemIndex)
    {
        itemRouteDetailsParent.SetActive(true);
        for (int i = 0; i < itemRoutes.Length; i++)
        {
            itemRoutes[i].SetActive(false);
        }
        itemRoutes[selectedItemIndex].SetActive(true);
        StartCoroutine( itemRoutes[selectedItemIndex].GetComponent<MaterialsEffectManger>().RevealCityOnebyOne());
    }
    [SerializeField] CityLabelEffect cityLabelEffect;
    public void ShowDetailedScreen(int selectedItemIndex)
    {
        landingScreenParent.SetActive(false);
        detailedScreenParent.SetActive(true);
        ManageItemDetailsObjects(selectedItemIndex);
        mainScreenRoutesParent.SetActive(false);
        cameraBreathingMotion.DisableBreathing();
        //cityLabelEffect.PlayReveal();
        FocusMesopotamia();
    }

    public void ShowMainScreen()
    {
        detailedScreenParent.SetActive(false);
        landingScreenParent.SetActive(true);
        // ToggleExportRouteDisplay(true);
        // ToggleImportRouteDisplay(true);
        
        // Below value is as per the its parent position in world.
        mapCam.MovetoActual(new Vector2(91.3f, -59.4f), 5f,
                   () =>
                   {
                       Debug.Log("Camera Animation Completed");
                       mainScreenRoutesParent.SetActive(true);
                       cameraBreathingMotion.EnableBreathing();
                   });
    }
    // public void ToggleImportRouteDisplay(bool _value)
    // {
    //     ImportRoute.SetActive(_value);
    // }
    //
    // public void ToggleExportRouteDisplay(bool _value)
    // {
    //     ExportRoute.SetActive(_value);
    // }
    
    //------------------------------MAP Moving Code---------------
    public MapCameraFocus mapCam;
    public BoxCollider2D indiaRegion, mesopotamiaRegion , worldRegion;

    public void FocusIndia() => mapCam.FocusOn(indiaRegion);
    public void FocusMesopotamia() => mapCam.FocusOn(mesopotamiaRegion);

    public void FocuWorldMap() => mapCam.FocusOn(worldRegion);
    public void FocusAll() => mapCam.FocusFullMap();
}
