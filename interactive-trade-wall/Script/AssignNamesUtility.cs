using UnityEngine;
using TMPro;
public class AssignNamesUtility : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [ContextMenu("AssignNames")]
    public void AssignNames()
    {
        TMP_Text text = transform.GetChild(1).gameObject.GetComponent<TMP_Text>();
        text.text = gameObject.name;
    }
    
}
