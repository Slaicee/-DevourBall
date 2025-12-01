using System.Collections.Generic;
using UnityEngine;

public class EatableManager : MonoBehaviour
{
    public static List<Eatable> All = new List<Eatable>();

    void Awake()
    {
        All.Clear();
        All.AddRange(FindObjectsOfType<Eatable>());
    }
}
