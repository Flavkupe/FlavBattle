using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IArmy
{
    string ID { get; }

    Formation Formation { get; }
}
