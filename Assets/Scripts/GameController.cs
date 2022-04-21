using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GameController 
{
    private static GameController instance;
    
    public static GameController Instance
    { 
        get
        {
            if (instance != null)
            {
                instance = new GameController();
                instance.CheckPoints.AddRange(GameObject.FindGameObjectsWithTag("CheckPoints"));

            }
            return instance;
        } 
    }
    private List<GameObject> checkPoints = new List<GameObject>();
    public List<GameObject> CheckPoints { get { return checkPoints; } }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
