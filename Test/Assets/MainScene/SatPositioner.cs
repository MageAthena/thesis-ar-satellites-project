using One_Sgp4;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using System.IO;
using UnityEngine;

public class SatPositioner : MonoBehaviour
{
    public GameObject Sats;
    public GameObject BaseSat;
    public MeshRenderer BaseSatMesh;
    public TextAsset tleTextAsset;
    private ArrayList satObjects;
    private List<Tle> tleList;
    private ArrayList validList;
    private ArrayList subsetList;
    private ArrayList lastPositions;
    private ArrayList nextPositions;

    public Globals globals;

    private int numSats = 0;

    private int numCats = 10;
    private ArrayList lastUpdates;

    private float timeStep = 300.0f; // Time step in seconds

    

    One_Sgp4.Coordinate observer;

   
    void Start ()
    {

        numSats = 0;

        string path = Application.persistentDataPath + "tleData.txt";
        string tleText = tleTextAsset.text;
        string[] tleLines = Regex.Split ( tleText, "\n|\r|\r\n" );
        File.WriteAllText(path, "");
        StreamWriter writer = new StreamWriter(path, true);
        for ( int i=0; i < tleLines.Length; i++ ) {
            string valueLine = tleLines[i];
            if (valueLine != ""){
                writer.WriteLine(valueLine);
            }
        }
        writer.Close();
        if (System.IO.File.Exists(path))
        {
            tleList = ParserTLE.ParseFile(path);
            numSats = tleList.Count;
            print("TLE List Retrieved, with ("+numSats+") sats");
        }
       
        satObjects = new ArrayList();
        validList = new ArrayList();
        subsetList = new ArrayList();
        lastPositions = new ArrayList();
        nextPositions = new ArrayList();
        BaseSat.SetActive(true);
        int subsetSats = 0;
        for (int i = 0; i < numSats; i++)
        {
            GameObject sat = Instantiate(BaseSat);
            sat.transform.SetParent(Sats.transform);
            sat.name = tleList[i].satName;

            MeshRenderer m_MeshRender = sat.transform.GetComponent<MeshRenderer>();
            if (sat.name.Contains("ISS (ZARYA)")){
                m_MeshRender.material.color = new Color(1.0f, 0.0f, 1.0f);
            }else if (sat.name.Contains("STARLINK")){
                m_MeshRender.material.color = new Color(0.0f, 0.0f, 0.75f);
            }else if (sat.name.Contains("NAVSTAR")){
                m_MeshRender.material.color = new Color(0.0f, 0.0f, 1.0f);
            }else if (sat.name.Contains("GALILEO")){
                m_MeshRender.material.color = new Color(1.0f, 0.75f, 0.0f);
            }else if (sat.name.Contains("GLONASS") || sat.name.Contains("MOLNIYA")){
                m_MeshRender.material.color = new Color(0.75f, 0.0f, 1.0f);
            }else if (sat.name.Contains("O3B")){
                m_MeshRender.material.color = new Color(0.0f, 1.0f, 0.0f);
            }else{
                m_MeshRender.material.color = new Color(0.5f, 0.0f, 0.0f);
            }


            if (
                sat.name.Contains("ISS (ZARYA)") || 
                sat.name.Contains("IRIDIUM") ||
                sat.name.Contains("MOLNIYA") ||
                sat.name.Contains("O3B") || 
                sat.name.Contains("OPTUS") || 
                sat.name.Contains("BEIDOU") || 
                sat.name.Contains("INTELSAT") ||
                sat.name.Contains("IMAGE") || 
                sat.name.Contains("POLAR") || 
                sat.name.Contains("SPIRALE")
            ){
                subsetList.Add(true);
                subsetSats += 1;
                print(sat.name);
            }else{
                subsetList.Add(false);
            }
           

            satObjects.Add(sat);
            validList.Add(true);
            lastPositions.Add(new Vector3(0,0,0));
            nextPositions.Add(new Vector3(0,0,0));
        }
        print("Subset established, with ("+subsetSats+"/"+numSats+") sats");
        BaseSat.SetActive(false);

        lastUpdates = new ArrayList();
        for (int i = 0; i < numCats; i++)
        {
            float staggerTime = timeStep * ((float) i) / ((float) numCats);
            DateTime updateTime = globals.currentTime.AddSeconds(-timeStep-staggerTime);
            lastUpdates.Add(updateTime);
        }

        float lat = globals.lat;
        float lng = globals.lng;
        observer = new Coordinate(lat, lng, 0);
    }

    // Update is called once per frame
    void Update()
    {
        DateTime currentTime = globals.currentTime;

        // Determine staggered updates
        ArrayList updateFracs = new ArrayList();
        ArrayList positionUpdates = new ArrayList();
        for (int i = 0; i < numCats; i++)
        {
            bool updatePositions = false;
            DateTime lastUpdate = (DateTime) lastUpdates[i];
            float updateFrac = ((float) (currentTime - lastUpdate).TotalSeconds) / timeStep;
            if (updateFrac > 1 || globals.updateAllSats){
                updatePositions = true;
                while (updateFrac > 1){
                    lastUpdate = lastUpdate.AddSeconds(timeStep);
                    updateFrac = updateFrac - 1.0f;
                }
                lastUpdates[i] = lastUpdate;
                //print("Updating index: "+i);
            }

            updateFracs.Add(updateFrac);
            positionUpdates.Add(updatePositions);
        }
        globals.updateAllSats = false;
        

        for (int i = 0; i < numSats; i++)
        {
            int catI = i % numCats;

            GameObject sat = (GameObject) satObjects[i];
            
            if ((bool) validList[i] && ((bool) subsetList[i] || !globals.satSubset)){
                if ((bool) positionUpdates[catI]){
                    DateTime baseTime = (DateTime) lastUpdates[catI];
                    EpochTime lastTime = new EpochTime(baseTime);
                    EpochTime nextTime = new EpochTime(baseTime.AddSeconds(timeStep));
                    try {
                        updateSatPositions(i, lastTime, nextTime);
                    }
                    catch (Exception e) {
                        print("Satellite orbit invalid: "+sat.name);
                        validList[i] = false;
                    }   
                    
                }
                

                // Get linear interpolation
                float updateFrac = (float) updateFracs[catI];
                Vector3 lastPos = sphericalToCartesian((Vector3) lastPositions[i]);
                Vector3 nextPos = sphericalToCartesian((Vector3) nextPositions[i]);
                Vector3 scenePos = (1 - updateFrac) * lastPos + updateFrac * nextPos;

                // Normalise magnitude to make it look better
                float mag = (1 - updateFrac) * lastPos.magnitude + updateFrac * nextPos.magnitude;
                scenePos = scenePos.normalized * mag;


                sat.transform.position = scenePos + sat.transform.parent.position;
                //sat.active = alt > 0;
            }else{
                sat.active = false;
            }
        }
    }

    void updateSatPositions(int i, EpochTime lastTime, EpochTime nextTime){
            Tle tle = tleList[i];
            One_Sgp4.Sgp4 sgp4Propagator = new Sgp4(tle, Sgp4.wgsConstant.WGS_84);
            sgp4Propagator.runSgp4Cal(lastTime, nextTime, timeStep / 60.0f);
            List<One_Sgp4.Sgp4Data> resultDataList = new List<Sgp4Data>();
            resultDataList = sgp4Propagator.getResults();
            
            One_Sgp4.Point3d sphericalCurrent = One_Sgp4.SatFunctions.calcSphericalCoordinate(observer, lastTime, resultDataList[0]);
            One_Sgp4.Point3d sphericalNext = One_Sgp4.SatFunctions.calcSphericalCoordinate(observer, nextTime, resultDataList[1]);

            lastPositions[i] = sphericalToVector(sphericalCurrent);
            nextPositions[i] = sphericalToVector(sphericalNext);
    }

    Vector3 sphericalToVector(One_Sgp4.Point3d spherical){
        return new Vector3(
            (float) spherical.x,
            (float) spherical.y,
            (float) spherical.z
        );
    }

    Vector3 sphericalToCartesian(Vector3 spherical){
        float range = (float) spherical.x;
        float azi = Mathf.Deg2Rad * (float) spherical.y;
        float alt = Mathf.Deg2Rad * (float) spherical.z;
        Vector3 scenePos = new Vector3(
            Mathf.Cos(alt) * Mathf.Cos(azi), 
            Mathf.Sin(alt), 
            Mathf.Cos(alt) * Mathf.Sin(azi)
        );

        //float scale = Mathf.Log(range / 200, 10);
        //float scale = Mathf.Sqrt(range / 1000);
        //float scale = Mathf.Pow(range / 7500.0f, 0.425f);
        float scale = 0.3f + (range - 200.0f) / 21250.0f;
        if (globals.external){
            scale = range / globals.externalScale;
        }
        scenePos = scenePos * scale;

        return scenePos;
    }
}
