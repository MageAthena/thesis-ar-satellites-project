using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthController : MonoBehaviour
{

    public Globals globals;

    float earthRad = 6370.0f;

    public GameObject earthObject;

    // Start is called before the first frame update
    void Start()
    {
        
        earthObject.transform.Rotate(90.0f - globals.lat, 0.0f, 0.0f, Space.Self);
        earthObject.transform.Rotate(0.0f, 90.0f + globals.lng, 0.0f, Space.Self);
        earthObject.transform.Rotate(0.0f, 90.0f, 0.0f, Space.World);

    }

    // Update is called once per frame
    void Update()
    {

        earthObject.SetActive(globals.external);

        float modRad = earthRad / globals.externalScale;
        earthObject.transform.position = new Vector3(0, -modRad,1f);
        earthObject.transform.localScale = Vector3.one * modRad * 2.0f;

    

    }
}
