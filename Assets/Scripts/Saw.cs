using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saw : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var character = collision.transform.GetComponentInParent<Character>();
        if (character != null)
            character.bloodStream.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var character = collision.transform.GetComponentInParent<Character>();
        if (character != null)
            character.bloodStream.SetActive(false);
    }
}
