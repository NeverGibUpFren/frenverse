using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UniversalActions : MonoBehaviour
{
    public UnityEvent start;


    void Start()
    {
        start.Invoke();
    }

}
