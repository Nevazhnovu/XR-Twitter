using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class Geo : MonoBehaviour
{
    [SerializeField] private Text location;
    [SerializeField] private Text status;
    [SerializeField] private Image compass;
    [SerializeField] private Camera arCamera;
    [SerializeField] private GameObject prefab;
    [SerializeField] private List<TweetGeo> geos;
        
    private bool myGeoFound = false;
    private Vector2 myPos = Vector2.zero;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(3f);

        while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            Permission.RequestUserPermission(Permission.CoarseLocation);
            yield return new WaitForSeconds(5f);
        }

        location.text = "Checking permissions";
        // Check if the user has location service enabled.
        if (!Input.location.isEnabledByUser) yield break;

        
        location.text = "Starting location service";
        // Starts the location service.
        Input.location.Start();       
        Input.compass.enabled = true;

        location.text = "Detecting your geo";
        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
            location.text += ".";
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            location.text = "Timed out";
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            location.text = "Unable to determine device location :(" ;
            yield break;
        }
        else
        {
            // If the connection succeeded, this retrieves the device's current location and displays it in the Console window.
            location.text = "Location: " + Input.location.lastData.latitude + " | " + Input.location.lastData.longitude + " | " + Input.location.lastData.altitude + " | " + Input.location.lastData.horizontalAccuracy + " | " + Input.location.lastData.timestamp;
            float latitude = Input.location.lastData.latitude;
            float longitude = Input.location.lastData.longitude;
            myPos = new Vector2(latitude, longitude);
            myGeoFound = true;
        }

        // Stops the location service if there is no need to query location updates continuously.
        Input.location.Stop();
    }

    void Update()
    {
        if (!myGeoFound) return;
        status.text = "myPos != Vector2.zero";
        foreach (var geo in geos)
        {
            geo.bearing = AngleFromCoordinate(myPos.x, myPos.y, geo.x, geo.y);
            status.text = "bearing = " + geo.bearing;
            compass.transform.rotation = Quaternion.Slerp(compass.transform.rotation,
                Quaternion.Euler(0, 0, Input.compass.trueHeading + geo.bearing), Time.deltaTime);
            if (!geo.isTweetPlaced) PutTweetMidAir(geo);
        }
    }

    private float AngleFromCoordinate(float lat1, float long1, float lat2, float long2)
    {
        lat1 *= Mathf.Deg2Rad;
        lat2 *= Mathf.Deg2Rad;
        long1 *= Mathf.Deg2Rad;
        long2 *= Mathf.Deg2Rad;
 
        float dLon = long2 - long1;
        float y = Mathf.Sin(dLon) * Mathf.Cos(lat2); 
        float x = (Mathf.Cos(lat1) * Mathf.Sin(lat2)) - (Mathf.Sin(lat1) * Mathf.Cos(lat2) * Mathf.Cos(dLon));
        float brng = Mathf.Atan2(y, x); 
        brng = Mathf.Rad2Deg* brng; 
        brng = (brng + 360) % 360; 
        brng = 360 - brng;
        return brng;
    }

    private void PutTweetMidAir(TweetGeo geo)
    {
        var bearing = Input.compass.trueHeading + geo.bearing;
        geo.isTweetPlaced = true;
        var targetPosition = arCamera.transform.position
                             // + arCamera.transform.forward * 0.6f // Place it 60cm in front of the camera
                             + arCamera.transform.forward * 1f // Place it 100cm in front of the camera
                             + arCamera.transform.up * 0.5f; // move it "up" 50cm perpendicular to the view direction
        var tweet = Instantiate(prefab, targetPosition, Quaternion.identity);
        tweet.transform.RotateAround(Vector3.zero, Vector3.up, -bearing);
        tweet.GetComponent<TweetController>().SetTweetText(geo.city);
        // tweet.transform.LookAt(arCamera.transform);
        // targetObject.position = Vector3.MoveTowards(currentPosition, targetPosition, step * Time.deltaTime);
    }
}
