using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TweetController : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Rigidbody rigidbody;
    private List<Collider> collisions = new List<Collider>();
    
    public void SetTweetText(string txt)
    {
        this.text.text += txt;
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        collisions.Add(otherCollider);
        // Calculate Angle Between the collision point and the player
        Vector3 dir = otherCollider.transform.position - transform.position;
        // We then get the opposite (-Vector3) and normalize it
        dir = -dir.normalized;
        // And finally we add force in the direction of dir and multiply it by force. 
        // This will push back the player
        rigidbody.AddForce(dir*5f);
    }

    private void OnTriggerExit(Collider otherCollider)
    {
        collisions.Remove(otherCollider);
        if(collisions.Count == 0)
            rigidbody.velocity = Vector3.zero;
    }
}
