using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoredArmy : IArmy
{
    public string ID { get; private set; }

    public Formation Formation { get; private set; }

    public StoredArmy(string id, Formation formation)
    {
        ID = id;
        Formation = formation;
    }

    public StoredArmy()
    {
        ID = new Guid().ToString();
        Formation = new Formation();
    }
}
