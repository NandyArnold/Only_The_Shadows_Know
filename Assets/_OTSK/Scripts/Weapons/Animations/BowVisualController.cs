// Create this new script, BowVisualsController.cs
using System.Collections;
using UnityEngine;

public class BowVisualsController : MonoBehaviour
{
    [SerializeField] private GameObject handArrow;
    [SerializeField] private float arrowRespawnDelay = 1.5f;

    private PlayerCombat _playerCombat;

    private void Awake()
    {
        _playerCombat = GetComponent<PlayerCombat>();
    }

    private void Start()
    {
        // On start, immediately check the currently equipped weapon to set the initial state.
        if (_playerCombat.CurrentWeapon != null)
        {
            HandleWeaponSwitch(_playerCombat.CurrentWeapon);
        }
    }
    private void OnEnable()
    {
        // Listen for when any bow fires
        BowSO.OnBowFired += HideArrowForShot;

        // Listen for when the player switches weapons
        _playerCombat.OnWeaponSwitched += HandleWeaponSwitch;
    }

    private void OnDisable()
    {
        BowSO.OnBowFired -= HideArrowForShot;
        _playerCombat.OnWeaponSwitched -= HandleWeaponSwitch;
    }

    private void HandleWeaponSwitch(WeaponSO newWeapon)
    {
        // Show the hand arrow only if a bow is equipped
        if (handArrow != null)
        {
            handArrow.SetActive(newWeapon is BowSO);
        }
    }

    private void HideArrowForShot()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RespawnArrowRoutine());
        }
    }

    private IEnumerator RespawnArrowRoutine()
    {
        handArrow.SetActive(false);
        yield return new WaitForSeconds(arrowRespawnDelay);
        handArrow.SetActive(true);
    }
}