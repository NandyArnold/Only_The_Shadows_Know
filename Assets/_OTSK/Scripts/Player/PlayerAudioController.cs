// Create a new script: PlayerAudioController.cs
using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Audio Data")]
    // --- CHANGE: We now reference the central database instead of a single default surface. ---
    [SerializeField] private SurfaceDatabaseSO surfaceDatabase;
    [SerializeField] private AudioClip jumpGrunt;
    [SerializeField] private AudioClip runningJumpGrunt;
    [SerializeField] private AudioClip dodgeGrunt;
    [SerializeField] private AudioClip landSound;

    [Header("Settings")]
    [SerializeField] private Transform footstepPoint; // An empty GameObject at the player's feet
    [SerializeField] private float surfaceCheckRadius = 0.3f;
    [SerializeField] private float surfaceCheckDistance = 0.3f;
    [SerializeField] private LayerMask surfaceLayer; // Set this to your 'Ground' layer
 
    
    
    private void Start()
    {
        if (SoundEffectManager.Instance == null)
        {
            Debug.LogError("<color=red>DEBUG:</color> SoundEffectManager instance is MISSING! Make sure the manager is in your scene.");
        }
        else
        {
            Debug.Log("<color=green>DEBUG:</color> SoundEffectManager instance was found successfully.");
        }
    }

    private void OnEnable()
    {
        playerMovement.OnJump += PlayJumpSound;
        playerMovement.OnDodge += PlayDodgeSound;
        playerMovement.OnLand += PlayLandSound;
    }

    private void OnDisable()
    {
        playerMovement.OnJump -= PlayJumpSound;
        playerMovement.OnDodge -= PlayDodgeSound;
        playerMovement.OnLand -= PlayLandSound;
    }

    // This method is called by Animation Events. Its internal logic remains the same.
    public void PlayFootstepSound()
    {
        SurfaceAudioSO surfaceAudio = GetCurrentSurfaceAudio();
        LocomotionState state = playerMovement.CurrentLocomotionState;

        // --- MODIFIED SECTION ---

        // 1. Select the correct settings object based on the player's state.
        MovementSound soundSettings = null;
        switch (state)
        {
            case LocomotionState.Walk:
                soundSettings = surfaceAudio.walkSound;
                break;
            case LocomotionState.Run:
                soundSettings = surfaceAudio.runSound;
                break;
            case LocomotionState.CrouchWalk:
                soundSettings = surfaceAudio.crouchSound;
                break;
        }

        // Make sure we have settings to play.
        if (soundSettings == null) return;

        // 2. Select the audio clips to use.
        AudioClip[] clipsToPlay = soundSettings.clips;

        // 3. SPECIAL LOGIC: If the selected clips are empty (for running), fall back to the walk clips.
        if (clipsToPlay.Length == 0)
        {
            clipsToPlay = surfaceAudio.walkSound.clips;
        }

        // 4. Play the sound if we have any clips available.
        if (clipsToPlay.Length > 0)
        {
            AudioClip randomClip = clipsToPlay[Random.Range(0, clipsToPlay.Length)];

            // Use the volume and pitch from the originally selected settings (e.g., runSound's volume/pitch).
            float randomVolume = Random.Range(soundSettings.minVolume, soundSettings.maxVolume);
            float randomPitch = Random.Range(soundSettings.minPitch, soundSettings.maxPitch);

            SoundEffectManager.Instance.PlaySoundAtPoint(randomClip, footstepPoint.position, randomVolume, randomPitch);
        }
    }

    // --- CHANGE: This method now finds the SurfaceType enum and uses it to query the database. ---
    private SurfaceAudioSO GetCurrentSurfaceAudio()
    {
        SurfaceType currentSurfaceType = SurfaceType.Default;

        // --- CHANGE: Replaced the simple Raycast with a more robust SphereCast ---
        if (Physics.SphereCast(footstepPoint.position, surfaceCheckRadius, Vector3.down, out RaycastHit hit, surfaceCheckDistance, surfaceLayer))
        {
            if (hit.collider.TryGetComponent<SurfaceIdentifier>(out var identifier))
            {
                currentSurfaceType = identifier.surfaceType;
            }
        }

        return surfaceDatabase.GetAudioData(currentSurfaceType);
    }

    private void PlayJumpSound()
    {
        // --- DEBUG: Check if the clip is assigned before trying to play it ---
        AudioClip clipToPlay = (playerMovement.CurrentLocomotionState == LocomotionState.Run) ? runningJumpGrunt : jumpGrunt;
        if (clipToPlay == null)
        {
            Debug.LogWarning("<color=orange>DEBUG:</color> Attempting to play jump sound, but the AudioClip is NULL. Check the Inspector.");
            return;
        }
        Debug.Log($"<color=cyan>DEBUG:</color> PlayJumpSound is calling the manager with clip: {clipToPlay.name}");
        SoundEffectManager.Instance.PlaySoundAtPoint(clipToPlay, transform.position);
    }

    private void PlayDodgeSound()
    {
        // --- DEBUG: Check if the clip is assigned ---
        if (dodgeGrunt == null)
        {
            Debug.LogWarning("<color=orange>DEBUG:</color> Attempting to play dodge sound, but the AudioClip is NULL. Check the Inspector.");
            return;
        }
        Debug.Log($"<color=cyan>DEBUG:</color> PlayDodgeSound is calling the manager with clip: {dodgeGrunt.name}");
        SoundEffectManager.Instance.PlaySoundAtPoint(dodgeGrunt, transform.position);
    }

    private void PlayLandSound()
    {
        // --- DEBUG: Check if the clip is assigned ---
        if (landSound == null)
        {
            Debug.LogWarning("<color=orange>DEBUG:</color> Attempting to play land sound, but the AudioClip is NULL. Check the Inspector.");
            return;
        }
        Debug.Log($"<color=cyan>DEBUG:</color> PlayLandSound is calling the manager with clip: {landSound.name}");
        SoundEffectManager.Instance.PlaySoundAtPoint(landSound, transform.position);
    }
}