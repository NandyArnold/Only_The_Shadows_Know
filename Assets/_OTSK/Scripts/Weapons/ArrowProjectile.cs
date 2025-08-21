using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class ArrowProjectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 5f;

    [Header("Impact Settings")]
    [SerializeField] private SurfaceAudioSO surfaceAudio;
    [SerializeField] private ImpactNoiseDataSO impactNoiseData;

    private Rigidbody _rb;
    private bool _hasHit = false;
    private GameObject _owner;
    private List<DamageInstance> _damageProfile; // Will be given its data by the BowSO
    private float _initialSpeed;                 // Will be given its data by the BowSO
  

    private bool _isReadyToFire = false;
    private TrailRenderer _trailRenderer;

    private BowSO _bowData;
    private void Awake()
    {
        
        _rb = GetComponent<Rigidbody>();
        GetComponent<BoxCollider>().isTrigger = false;
        _rb.useGravity = true;
        _trailRenderer = GetComponent<TrailRenderer>();
    }
    private void OnEnable()
    {
        // Clear any old trail positions from the last time this object was used
        if (_trailRenderer != null)
        {
            _trailRenderer.Clear();
        }
    }
    // This is the method the BowSO will call to give the arrow its stats.
    public void Initialize(GameObject owner, List<DamageInstance> damageProfile, float speed, BowSO bowData)
    {
        //Debug.Log($"(8) Arrow (ID: {GetInstanceID()}) is being INITIALIZED with speed {speed}.");
        _owner = owner;
        _damageProfile = damageProfile;
        _initialSpeed = speed;
        _bowData = bowData;
        _isReadyToFire = true;
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (_isReadyToFire)
        {
            _rb.linearVelocity = transform.forward * _initialSpeed;
            _isReadyToFire = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasHit) return;
        _hasHit = true;
        //if (_rb.isKinematic) return;

        // The very first thing we do on ANY collision is stop the arrow's physics.
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;
        transform.SetParent(collision.transform, true);
        // ----------NOISE AND SOUND EFFECTS---------

        // 1. Determine the surface type just ONCE and store it in a variable.
        SurfaceType surfaceType = SurfaceType.Default;
        if (collision.collider.TryGetComponent<SurfaceIdentifier>(out var surface))
        {
            surfaceType = surface.surfaceType;
        }

        // 2. Play the impact SOUND using the determined surface type.
        // This logic relies on the _bowData reference we set up.
        if (_bowData != null)
        {
            _bowData.GetImpactSound(surfaceType).Play(transform);
        }

        // 3. Generate the in-game NOISE using the SAME surface type.
        float noiseIntensity = impactNoiseData.GetNoiseIntensity(surfaceType);
        NoiseManager.Instance.GenerateNoise(transform.position, noiseIntensity, this.gameObject);


        //-------------------------------------------


        if (collision.gameObject.TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth))
        {
            // Use the damage profile it was given at the start.
            if (_damageProfile != null)
            {
                enemyHealth.TakeDamage(_damageProfile, _owner);
            }
        }
        else if (collision.collider.TryGetComponent<Destructible>(out var destructible))
        {
            // Use the same damage profile the arrow was created with
            destructible.TakeDamage(_damageProfile, _owner);
        }

    }
}
