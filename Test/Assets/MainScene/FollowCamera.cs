using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour 
{

    public GameObject camera;

    public Globals globals;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (globals.external){
            transform.position = new Vector3(0,0,1f);
        }else{
            transform.position = camera.transform.position;
        }
        
    }
}
