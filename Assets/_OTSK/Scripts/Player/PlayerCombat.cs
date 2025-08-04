// PlayerCombat.cs - REFACTORED
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerCombat : MonoBehaviour
{
    // Public properties for other scripts (like WeaponSO) to access.
    public PlayerAnimationController PlayerAnimationController => playerAnimationController;
    public Transform HandSocketR => handSocketR;
    public Transform HandSocketL => handSocketL;

    [Header("Component References")]
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PlayerAnimationController playerAnimationController;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private Rig ikRig;

    [Header("Socket Transforms")]
    [SerializeField] private Transform handSocketR;
    [SerializeField] private Transform handSocketL;

    [Header("Weapon Settings")]
    [SerializeField] private List<WeaponSO> availableWeapons;

    [Header("Combat Points")]
    [SerializeField] private Transform firePoint;
    public Transform FirePoint => firePoint;
    public bool IsFocused => _isFocused;

    public event Action<bool> OnAimStateChanged;
    public event Action<bool> OnFocusStateChanged;
    public event Action<WeaponSO> OnWeaponSwitched;

    private Coroutine _rigWeightCoroutine;

    private WeaponSO _currentWeapon;
    public WeaponSO CurrentWeapon => _currentWeapon;
    private DaggerAnimation _daggerAnimation;
    private BowAnimation _bowAnimation;

    private bool _isAiming = false;
    private bool _isFocused = false;

    #region Standard Methods
    private void Awake()
    {
        // Get component references
        if (playerInputHandler == null) playerInputHandler = GetComponent<PlayerInputHandler>();
        if (playerAnimationController == null) playerAnimationController = GetComponent<PlayerAnimationController>();
        if (weaponManager == null) weaponManager = GetComponent<WeaponManager>();

        _daggerAnimation = GetComponent<DaggerAnimation>();
        _bowAnimation = GetComponent<BowAnimation>();

        if (availableWeapons != null && availableWeapons.Count > 0)
        {
            SwitchWeapon(availableWeapons[0]);
        }
        if (ikRig == null)
            ikRig = GetComponentInChildren<Rig>();
    }

    private void OnEnable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnPrimaryAttackInput += HandlePrimaryAttack;
            playerInputHandler.OnSecondaryAttackInput += HandleSecondaryAttack;
            playerInputHandler.OnSecondaryAttackPressed += HandleSecondaryAttackPress;
            playerInputHandler.OnSecondaryAttackReleased += HandleSecondaryAttackRelease;


            playerInputHandler.OnWeapon1Input += HandleWeapon1Switch;
            playerInputHandler.OnWeapon2Input += HandleWeapon2Switch;
            playerInputHandler.OnWeapon3Input += HandleWeapon3Switch;
        }
    }

    private void OnDisable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnPrimaryAttackInput -= HandlePrimaryAttack;
            playerInputHandler.OnSecondaryAttackInput -= HandleSecondaryAttack;
            playerInputHandler.OnSecondaryAttackPressed -= HandleSecondaryAttackPress;
            playerInputHandler.OnSecondaryAttackReleased -= HandleSecondaryAttackRelease;


            playerInputHandler.OnWeapon1Input -= HandleWeapon1Switch;
            playerInputHandler.OnWeapon2Input -= HandleWeapon2Switch;
            playerInputHandler.OnWeapon3Input -= HandleWeapon3Switch;
        }
    }
    #endregion

    public void SwitchWeaponByIndex(int index)
    {
        if (index < 0 || index >= availableWeapons.Count) return;
        SwitchWeapon(availableWeapons[index]);
    }

    public void SwitchWeapon(WeaponSO newWeapon)
    {
        if (newWeapon == null || newWeapon == _currentWeapon) return;

        _currentWeapon = newWeapon;

        OnWeaponSwitched?.Invoke(_currentWeapon);
        // 1. Tell the WeaponManager to handle the visuals.
        weaponManager.EquipNewWeapon(newWeapon);

        // 2. Determine if the new weapon should use the IK aiming rig.
        bool usesAimIK = (newWeapon is BowSO || newWeapon is AnimancySO);
        UpdateRigWeight(usesAimIK);

        // 3. Set the correct animation component and aim state in one clean block.
        if (newWeapon is BowSO)
        {
            playerAnimationController.SetWeaponAnimation(_bowAnimation);
            SetAimState(true); // Enter aim stance
        }
        else 
        {
            if (_isAiming) SetAimState(false);
            if (_isFocused) SetFocusState(false);

            if (newWeapon is DaggerSO)
                {
                    playerAnimationController.SetWeaponAnimation(_daggerAnimation);
                    SetAimState(false); // Daggers are not in aim stance
                }
      
            if (newWeapon is AnimancySO)
            {
                // When we create AnimancyAnimation, we'll set it here.
                // playerAnimationController.SetWeaponAnimation(_animancyAnimation);
                SetAimState(false); // Animancy is not in aim stance
            }
        }
       
    }

    private void SetAimState(bool isAiming)
    {
        if (_isAiming == isAiming) return;
        _isAiming = isAiming;
        OnAimStateChanged?.Invoke(_isAiming);
        playerAnimationController.SetAimingState(_isAiming);
        if (!_isAiming && _isFocused) SetFocusState(false);
    }

    private void HandlePrimaryAttack()
    {
        if (_currentWeapon is BowSO && _isFocused)
        {
            _currentWeapon.SecondaryAttack(this);
        }
        else
        {
            // Otherwise, it's a normal primary attack (dagger slash, or bow hip-fire).
            _currentWeapon?.PrimaryAttack(this);
        }
    }

    private void HandleSecondaryAttack()
    {
        if (_currentWeapon is BowSO) return;
        _currentWeapon?.SecondaryAttack(this);
    }
    private void HandleSecondaryAttackPress()
    {
        if (_currentWeapon is BowSO && _isAiming) SetFocusState(true);
    }

    // This handler is for RELEASING A HOLD (for the Bow)
    private void HandleSecondaryAttackRelease()
    {
        if (_currentWeapon is BowSO && _isAiming) SetFocusState(false);
    }

    private void HandleWeapon1Switch() => SwitchWeapon(availableWeapons[0]);
         private void HandleWeapon2Switch() => SwitchWeapon(availableWeapons[1]);
    private void HandleWeapon3Switch() => SwitchWeapon(availableWeapons[2]);
    private void UpdateRigWeight(bool enable)
    {
        if (_rigWeightCoroutine != null)
        {
            StopCoroutine(_rigWeightCoroutine);
        }
        float targetWeight = enable ? 1f : 0f;
        _rigWeightCoroutine = StartCoroutine(FadeRigWeight(targetWeight, 0.25f));
    }

    // NEW: The coroutine that smoothly changes the weight over time.
    private IEnumerator FadeRigWeight(float targetWeight, float duration)
    {
        float startTime = Time.time;
        float startWeight = ikRig.weight;

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            ikRig.weight = Mathf.Lerp(startWeight, targetWeight, t);
            yield return null;
        }

        ikRig.weight = targetWeight;
    }
    private void SetFocusState(bool isFocused)
    {
        if (_isFocused == isFocused) return;
        _isFocused = isFocused;
        OnFocusStateChanged?.Invoke(_isFocused);
    }
}