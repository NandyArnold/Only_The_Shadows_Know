using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class ArrowProjectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 5f;

    [Header("Impact Settings")]
    [SerializeField] private SurfaceAudioDataSO surfaceAudioData;
    [SerializeField] private ImpactNoiseDataSO impactNoiseData;

    private Rigidbody _rb;
    private bool _hasHit = false;
    private GameObject _owner;
    private List<DamageInstance> _damageProfile; // Will be given its data by the BowSO
    private float _initialSpeed;                 // Will be given its data by the BowSO
  

    private bool _isReadyToFire = false;
    private TrailRenderer _trailRenderer;

   
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
    public void Initialize(GameObject owner, List<DamageInstance> damageProfile, float speed)
    {
        //Debug.Log($"(8) Arrow (ID: {GetInstanceID()}) is being INITIALIZED with speed {speed}.");
        _owner = owner;
        _damageProfile = damageProfile;
        _initialSpeed = speed;
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
        SurfaceType surfaceType = SurfaceType.Default;
        if (collision.collider.TryGetComponent<SurfaceIdentifier>(out var surface))
        {
            surfaceType = surface.surfaceType;
        }

        // Get the correct sound data
        var soundData = surfaceAudioData.GetSound(surfaceType);
        SoundEffectManager.Instance.PlaySoundAtPoint(soundData.audioClip, transform.position);

        // Get the correct noise data and generate the in-game noise
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
       
    }
}
