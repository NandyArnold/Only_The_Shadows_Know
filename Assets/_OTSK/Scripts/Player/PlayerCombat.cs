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
    public Transform FirePoint => firePoint;
    public bool IsFocused => _isFocused;
    public bool IsAiming => _isAiming;
    public WeaponSO CurrentWeapon => _currentWeapon;
    public Transform HandSocketR => handSocketR;
    public Transform HandSocketL => handSocketL;

    [Header("Component References")]
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PlayerAnimationController playerAnimationController;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private Rig ikRig;
    [SerializeField] private MultiAimConstraint aimConstraint;
    [SerializeField] private PlayerSkillController playerSkillController;


    [Header("Socket Transforms")]
    [SerializeField] private Transform handSocketR;
    [SerializeField] private Transform handSocketL;

    [Header("Weapon Settings")]
    [SerializeField] private List<WeaponSO> availableWeapons;

    [Header("Combat Points")]
    [SerializeField] private Transform firePoint;

    [Header("Data References")]
    [SerializeField] private NoiseSettingsSO noiseSettings;
    [SerializeField] private PlayerHealthManaNoise playerHealthManaNoise;
    public NoiseSettingsSO NoiseSettings => noiseSettings;
    public PlayerHealthManaNoise HealthManaNoise => playerHealthManaNoise;



    public event Action<bool> OnAimStateChanged;
    public event Action<bool> OnFocusStateChanged;
    public event Action<WeaponSO> OnWeaponSwitched;
    public event Action OnBowFire;
    public event Action OnFocusedShotFired;

    private Coroutine _rigWeightCoroutine;

    private WeaponSO _currentWeapon;
    
    private DaggerAnimation _daggerAnimation;
    private BowAnimation _bowAnimation;
    private AnimancyAnimation _animancyAnimation;

    private bool _isAiming = false;
    private bool _isFocused = false;

    #region Standard Methods
    private void Awake()
    {
        // Get component references
        if (playerInputHandler == null) playerInputHandler = GetComponent<PlayerInputHandler>();
        if (playerAnimationController == null) playerAnimationController = GetComponent<PlayerAnimationController>();
        if (weaponManager == null) weaponManager = GetComponent<WeaponManager>();
        if (playerSkillController == null) playerSkillController = GetComponent<PlayerSkillController>();

        _daggerAnimation = GetComponent<DaggerAnimation>();
        _bowAnimation = GetComponent<BowAnimation>();
        _animancyAnimation = GetComponent<AnimancyAnimation>();

        if (ikRig == null)
            ikRig = GetComponentInChildren<Rig>();
        if (GameManager.Instance != null && GameManager.Instance.AimTarget != null)
        {
            // Create a source object array for the constraint
            var sourceList = new WeightedTransformArray();
            sourceList.Add(new WeightedTransform(GameManager.Instance.AimTarget, 1f));

            // Assign the source (the _AimTarget) to both constraints
            if (aimConstraint != null)
            {
                aimConstraint.data.sourceObjects = sourceList;
            }


            // Rebuild the rig with the new data
            var rigBuilder = GetComponent<RigBuilder>();
            if (rigBuilder != null) rigBuilder.Build();
        }
        else
        {
            Debug.LogError("PlayerCombat could not find AimTarget from GameManager to initialize IK rig!", this);
        }

        if (availableWeapons != null && availableWeapons.Count > 0)
        {
            SwitchWeapon(availableWeapons[0]);
        }
        if (noiseSettings == null) Debug.LogError("NoiseSettingsSO not assigned on PlayerCombat!");
        if (playerHealthManaNoise == null) playerHealthManaNoise = GetComponent<PlayerHealthManaNoise>();


    }

    private void OnEnable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnPrimaryAttackInput += HandlePrimaryAttack;
            playerInputHandler.OnSecondaryAttackInput += HandleSecondaryAttack;
            playerInputHandler.OnSecondaryAttackPressed += HandleSecondaryAttackPress;
            playerInputHandler.OnSecondaryAttackReleased += HandleSecondaryAttackRelease;
            playerInputHandler.OnTertiaryAttackInput += HandleTertiaryAttack;

            playerInputHandler.OnWeapon1Input += () => SwitchWeaponByIndex(0);
            playerInputHandler.OnWeapon2Input += () => SwitchWeaponByIndex(1);
            playerInputHandler.OnWeapon3Input += () => SwitchWeaponByIndex(2);
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
            playerInputHandler.OnTertiaryAttackInput -= HandleTertiaryAttack;

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
        weaponManager.EquipNewWeapon(newWeapon);

        // Determine if the weapon uses the IK aiming rig.
        bool usesAimIK = (newWeapon is BowSO || newWeapon is AnimancySO);
        UpdateRigWeight(usesAimIK);

        // --- SINGLE, CLEAN LOGIC BLOCK ---
        if (newWeapon is BowSO)
        {
            playerAnimationController.SetWeaponAnimation(_bowAnimation);
            playerAnimationController.SetWeaponType(2);
            SetAimState(true);
        }
        else if (newWeapon is DaggerSO)
        {
            playerAnimationController.SetWeaponAnimation(_daggerAnimation);
            playerAnimationController.SetWeaponType(1);
            SetAimState(false);
        }
        else if (newWeapon is AnimancySO)
        {
            
            playerAnimationController.SetWeaponAnimation(_animancyAnimation);
            playerAnimationController.SetWeaponType(3);
            SetAimState(false);
        }

    }

    private void SetAimState(bool isAiming)
    {
        
        _isAiming = isAiming;
        OnAimStateChanged?.Invoke(_isAiming);
        playerAnimationController.SetAimingState(_isAiming);
        if (!_isAiming && _isFocused)
        {
            SetFocusState(false);
        }
    }

    private void HandlePrimaryAttack()
    {

        if (_currentWeapon is BowSO && _isFocused)
        {
            _currentWeapon.SecondaryAttack(this);
            OnBowFire?.Invoke(); // Announce that a focused shot was fired
            OnFocusedShotFired?.Invoke(); // Notify that a focused shot was fired

        }
        else if (_currentWeapon is BowSO) // Added check for unfocused bow shot
        {
            _currentWeapon.PrimaryAttack(this);
            OnBowFire?.Invoke(); // Announce that an unfocused shot was fired
        }
        else
        {
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
        if (_currentWeapon is BowSO && _isAiming)
        {
            if (_isFocused)
            {
                SetFocusState(false);
            }
        }
    }

    private void HandleTertiaryAttack()
    {
        // If we are holding the Animancy weapon, this button activates the DeathZone skill.
        if (_currentWeapon is AnimancySO)
        {
            // We tell the skill controller to try and activate the skill by its ID.
            playerSkillController.TryActivateSkillByID(SkillIdentifier.DeathZone);
        }
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