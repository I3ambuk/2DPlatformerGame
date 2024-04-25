using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class VarationGenerator : MonoBehaviour
{
    public List<GameObject> variations = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        int randInt = Random.Range(0,variations.Count);
        Instantiate(variations[randInt], transform.position, Quaternion.identity).transform.parent = transform;
    }
}
