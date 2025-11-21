using UnityEngine;

public class MultiDisplayBootstrap : MonoBehaviour
{

    void Start()
    {
        for(int i=1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }
    }


}
