using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RoutineRunner : MonoBehaviour
{
    public void Run(Routine routine)
    {
        routine.Then(() => Destroy(this.gameObject));
        StartCoroutine(routine);
    }
}
