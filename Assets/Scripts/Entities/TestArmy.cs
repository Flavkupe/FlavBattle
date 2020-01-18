using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestArmy : IArmy
{
    public string ID => Guid.NewGuid().ToString();

    public Formation Formation { get; set; } = new Formation();

    public FactionData Faction { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
