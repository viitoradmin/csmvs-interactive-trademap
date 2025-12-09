using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialsEffectManger : MonoBehaviour
{


/// <summary>
/// Manages all CityLabelEffect components under LocationNamesParent
/// for this material/route.
/// Attach this to 'Material1-Details'.
/// </summary>

    [Header("Hierarchy Reference")]
    [Tooltip("Parent that contains all location objects with CityLabelEffect.")]
    [SerializeField] private Transform locationNamesParent;

    // Cached list of labels (no searching every frame)
    private CityLabelEffect[] _cityLabels;

    void Awake()
    {
        if (locationNamesParent == null)
        {
            Debug.LogError($"[{name}] LocationNamesParent is not assigned.");
            return;
        }

        // Get all CityLabelEffect components under the parent (including inactive)
        _cityLabels = locationNamesParent.GetComponentsInChildren<CityLabelEffect>(true);

        if (_cityLabels == null || _cityLabels.Length == 0)
        {
            Debug.LogWarning($"[{name}] No CityLabelEffect components found under {locationNamesParent.name}.");
        }
    }

    // --- PUBLIC API ---

    /// <summary>
    /// Play reveal on all cities (e.g., when route is first shown).
    /// </summary>
    public void RevealAllCities()
    {
        if (_cityLabels == null) return;

        foreach (var label in _cityLabels)
        {
            if (label != null)
                label.PlayReveal();
        }
    }

   public IEnumerator RevealCityOnebyOne()
    {
        if(_cityLabels == null || _cityLabels.Length == 0) yield break;

        foreach (var label in _cityLabels)
        {
            if(label != null)
                label.PlayReveal();
            yield return new WaitForSeconds(2f);
        }
        
        
    }
    
    /// <summary>
    /// Set all city labels to idle state (dimmed but visible).
    /// </summary>
    public void SetAllCitiesIdle()
    {
        if (_cityLabels == null) return;

        foreach (var label in _cityLabels)
        {
            if (label != null)
                label.SetIdle();
        }
    }

    /// <summary>
    /// Activate a specific city by index (0-based, based on hierarchy order).
    /// Others go to idle.
    /// </summary>
    public void FocusCityByIndex(int index)
    {
        if (_cityLabels == null || _cityLabels.Length == 0) return;

        for (int i = 0; i < _cityLabels.Length; i++)
        {
            var label = _cityLabels[i];
            if (label == null) continue;

            if (i == index)
                label.PlayReveal();
            else
                label.SetIdle();
        }
    }

    /// <summary>
    /// Activate a specific city by GameObject name (Location1_Parent, Dholavira, etc.).
    /// </summary>
    public void FocusCityByName(string cityObjectName)
    {
        if (_cityLabels == null || string.IsNullOrEmpty(cityObjectName)) return;

        foreach (var label in _cityLabels)
        {
            if (label == null) continue;

            bool isTarget = label.gameObject.name == cityObjectName;
            if (isTarget)
                label.PlayReveal();
            // else
            //     label.SetIdle();
        }
    }

}
