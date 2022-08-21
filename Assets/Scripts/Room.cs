using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Room", menuName = "ProcGen/Room", order = 100)]
public class Room : ScriptableObject
{
    public GameObject Prefab;
    public float radius;
    public float CanopyRadius;
}
