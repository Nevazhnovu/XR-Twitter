using UnityEngine;

[CreateAssetMenu(fileName = "TweetGeo", menuName = "ScriptableObjects/SpawnTweetGeo", order = 1)]
public class TweetGeo : ScriptableObject
{
    public string city;
    [SerializeField] private Vector2 geo;
    public float x => geo.x; // latitude 
    public float y => geo.y; // longitude 
    public float bearing; //angle from camera
    public bool isTweetPlaced = false;
}