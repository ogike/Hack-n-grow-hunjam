using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }

    public float particlePosZ = 1.0f;

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
        Vector3 rotatedDirectionFor2D = Quaternion.Euler(0, 0, 90) * lookDirection;
        Quaternion lookRotation = Quaternion.LookRotation(Vector3.forward, rotatedDirectionFor2D);
        position.z = particlePosZ;
        GameObject particle = GameObject.Instantiate(prefab, position, lookRotation, _trans);
    }

    public void SpawnParticleWithParentAndDuration(GameObject prefab, Transform parent, float duration)
    {
        GameObject particle = GameObject.Instantiate(prefab, parent);

        //nullchechking is expensive so be caredul to use ParticleSystem here lol
        ParticleSystem particleSystem = particle.GetComponent<ParticleSystem>();
        particleSystem.Stop();
        
        ParticleSystem.MainModule main = particleSystem.main; 
        main.duration = duration;
        
        particleSystem.Play();
        
    }
}
