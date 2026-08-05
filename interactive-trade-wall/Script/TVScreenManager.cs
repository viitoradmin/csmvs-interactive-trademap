using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class TVScreenManager : MonoBehaviour
{

    [SerializeField] GameObject landingScreenParent;
    [SerializeField] GameObject detailedScreenParent;
    [SerializeField] GameObject mainScreenRoutesParent;
    //[SerializeField] GameObject ImportRoute;
    //[SerializeField] GameObject ExportRoute;
    [SerializeField] private GameObject itemRouteDetailsParent;
    [SerializeField] private GameObject[] itemRoutes = new GameObject[8];
    [SerializeField] TVScreenBreathingFX tvScreenBreathingFX;

    [SerializeField] private List<RouteCityLists> SourceCityLists;
    [SerializeField] private List<RouteCityLists> DestinationCityLists;
    [SerializeField] private List<RouteCityLists> middleCityLists;
    [Serializable]
    public class RouteCityLists
    {
        public string materialName;  
       public List<string> cityName = new List<string>();
    }
    
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
       // StartCoroutine( itemRoutes[selectedItemIndex].GetComponent<MaterialsEffectManger>().RevealCityOnebyOne());

        StartCoroutine(ManageCityReveal(selectedItemIndex));
    }

    IEnumerator ManageCityReveal(int selectedItemIndex)
    {
        for (int i = 0; i < SourceCityLists[selectedItemIndex].cityName.Count; i++)
        {
            itemRoutes[selectedItemIndex].GetComponent<MaterialsEffectManger>().FocusCityByName(SourceCityLists[selectedItemIndex].cityName[i]);
        }

        yield return new WaitForSeconds(1f);
        
        for (int i = 0; i < middleCityLists[selectedItemIndex].cityName.Count; i++)
        {
            itemRoutes[selectedItemIndex].GetComponent<MaterialsEffectManger>().FocusCityByName(middleCityLists[selectedItemIndex].cityName[i]);
        }
        yield return new WaitForSeconds(1f);
        
        for (int i = 0; i < DestinationCityLists[selectedItemIndex].cityName.Count; i++)
        {
            itemRoutes[selectedItemIndex].GetComponent<MaterialsEffectManger>().FocusCityByName(DestinationCityLists[selectedItemIndex].cityName[i]);
        }
    }
    
    [SerializeField] CityLabelEffect cityLabelEffect;
    public void ShowDetailedScreen(int selectedItemIndex)
    {
        landingScreenParent.SetActive(false);
        detailedScreenParent.SetActive(true);
        ManageItemDetailsObjects(selectedItemIndex);
        mainScreenRoutesParent.SetActive(false);
        tvScreenBreathingFX.DisableBreathing();
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
                       // Debug.Log("Camera Animation Completed");
                       mainScreenRoutesParent.SetActive(true);
                       tvScreenBreathingFX.EnableBreathing();
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
