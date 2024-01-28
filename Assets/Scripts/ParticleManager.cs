using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }

    private Transform _trans;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one ParticleManager instances in scene!");
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        _trans = transform;
    }

    public void SpawnParticleWithLookDirection(GameObject prefab, Vector3 position, Vector3 lookDirection)
    {
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection, Vector3.forward);
        GameObject particle = GameObject.Instantiate(prefab, position, lookRotation, _trans);
    }
}
