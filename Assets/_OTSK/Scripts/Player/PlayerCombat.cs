using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Unity.Cinemachine;
using System.Net.Sockets;

public class PlayerCombat : MonoBehaviour
{
    // Public properties for other scripts to access
    public CinemachineBrain Brain { get; private set; }
    public AimTargetController AimTargetController { get; private set; }
    public PlayerAnimationController PlayerAnimationController => playerAnimationController;
    public Transform FirePoint => firePoint;
    public bool IsFocused => _isFocused;
    public bool IsAiming => _isAiming;
    public WeaponSO CurrentWeapon => _currentWeapon;
    public NoiseSettingsSO NoiseSettings => noiseSettings;
    public PlayerStats PlayerStats => playerStats;
    public Transform HandSocketR => handSocketR;
    public Transform HandSocketL => handSocketL;

    [Header("Component References")]
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PlayerAnimationController playerAnimationController;
    [SerializeField] private WeaponManager weaponManager;
  
    [SerializeField] private PlayerSkillController playerSkillController;

    [Header("Data References")]
    [SerializeField] private NoiseSettingsSO noiseSettings;
    [SerializeField] private PlayerStats playerStats;

    [Header("Combat Points")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform handSocketR;
    [SerializeField] private Transform handSocketL;
    [SerializeField] private Transform backSocket;
    [SerializeField] private Transform hipSocketL;
    [SerializeField] private Transform hipSocketR;

    [Header("Weapon Settings")]
    [SerializeField] private List<WeaponSO> availableWeapons;

    public event Action<bool> OnAimStateChanged;
    public event Action<bool> OnFocusStateChanged;
    public event Action<WeaponSO> OnWeaponSwitched;
    public event Action OnBowFire;
    public event Action OnFocusedShotFired;

    private WeaponSO _currentWeapon;
    private DaggerAnimation _daggerAnimation;
    private BowAnimation _bowAnimation;
    private AnimancyAnimation _animancyAnimation;
    private bool _isAiming = false;
    private bool _isFocused = false;
     private Rig ikRig;
    private Coroutine _rigWeightCoroutine;
    private float _lastAttackTime;
    private Enemy _potentialDeathZoneTarget;

    private void Awake()
    {
        // Get component references
        playerInputHandler = GetComponent<PlayerInputHandler>();
        playerAnimationController = GetComponent<PlayerAnimationController>();
        weaponManager = GetComponent<WeaponManager>();
       
        playerSkillController = GetComponent<PlayerSkillController>();
        _daggerAnimation = GetComponent<DaggerAnimation>();
        _bowAnimation = GetComponent<BowAnimation>();
        _animancyAnimation = GetComponent<AnimancyAnimation>();
        ikRig = GetComponentInChildren<Rig>();
        Brain = Camera.main.GetComponent<CinemachineBrain>();

        playerStats = GetComponent<PlayerStats>();
        if (GameManager.Instance != null)
        {
            AimTargetController = GameManager.Instance.AimTargetController;
        }





        if (_daggerAnimation != null)
        {
            //Debug.Log("Dagger Animation found in PlayerCombat.");
        }
        else
        {
            Debug.LogError("Dagger Animation NOT found in PlayerCombat. Please assign it in the inspector.", this);
        }

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
            playerInputHandler.OnTertiaryAttackInput -= HandleTertiaryAttack;
            playerInputHandler.OnWeapon1Input -= HandleWeapon1Switch;
            playerInputHandler.OnWeapon2Input -= HandleWeapon2Switch;
            playerInputHandler.OnWeapon3Input -= HandleWeapon3Switch;
        }
    }

    private void Start()
    {
        
        //// We initialize the WeaponManager in Start to ensure all other Awake methods have run.
        if (weaponManager != null && availableWeapons != null && availableWeapons.Count > 0)
        {
            // Tell the manager about all our weapons and which one is the default.
            weaponManager.Initialize(handSocketR, handSocketL, backSocket, hipSocketL, hipSocketR, availableWeapons);

            // Also set this script's current weapon data.
            //_currentWeapon = availableWeapons[0];
        }

        if (availableWeapons != null && availableWeapons.Count > 0)
        {
            // Switch to the first weapon in the list by default.
            SwitchWeapon(availableWeapons[0]);
        }
        else
        {
            Debug.LogWarning("No available weapons found. Please assign at least one weapon in the inspector.", this);
        }
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatStart += EnableInputs;
            CombatManager.Instance.OnCombatEnd += DisableInputs;
        }



    }

    private void Update()
    {
        // NEW: This block will handle the continuous target searching
        if (_currentWeapon is AnimancySO animancyWeapon)
        {
            UpdateDeathZoneTargeting(animancyWeapon);
        }
        else if (_potentialDeathZoneTarget != null)
        {
            // If we switch away from Animancy, clear the old target
            _potentialDeathZoneTarget.GetComponentInChildren<EnemyUIController>()?.SetDeathZoneMarkActive(false);
            _potentialDeathZoneTarget = null;
        }
    }


    private void OnDestroy()
    {
        //DisableInputs();
    }

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
        UpdateRigWeight(newWeapon.usesAimIK);

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
        if (_currentWeapon == null) return;
        if (Time.time < _lastAttackTime + _currentWeapon.timeBetweenAttacks) return;

        // This is now simple. It just tells the weapon to perform its primary attack.
        // The weapon itself will figure out if it's a focused shot or not.
        if (_currentWeapon is BowSO && _isFocused)
        {
            // Tell the weapon to perform its "Secondary" (focused) attack
            _currentWeapon.SecondaryAttack(this);

            // Fire the event for the camera recoil
            OnFocusedShotFired?.Invoke();
        }
        else
        {
            // Otherwise, perform a normal primary attack for any weapon
            _currentWeapon.PrimaryAttack(this);
        }

        // Fire the general "bow fired" event for visuals like the hand arrow
        if (_currentWeapon is BowSO)
        {
            OnBowFire?.Invoke();
        }

        // Update the cooldown timer
        _lastAttackTime = Time.time;
    }

    private void HandleSecondaryAttack()
    {
        if (_currentWeapon is BowSO && _isAiming)
        {
            // This is the toggle logic. It sets the focus state to the opposite of what it currently is.
            SetFocusState(!_isFocused);
        }
        // Case 2: The weapon is a Dagger or Animancy (or any other standard melee/magic weapon).
        else if (_currentWeapon is DaggerSO || _currentWeapon is AnimancySO)
        {
            // This is the standard attack logic.
            if (Time.time < _lastAttackTime + _currentWeapon.timeBetweenAttacks) return;

            _currentWeapon.SecondaryAttack(this);
            _lastAttackTime = Time.time;
        }

    }

    private void HandleSecondaryAttackPress()
    {
       
    }

    // This handler is for RELEASING A HOLD (for the Bow)
    private void HandleSecondaryAttackRelease()
    {
        
    }

    private void HandleTertiaryAttack()
    {
        // Check if the currently equipped weapon is Animancy
        if (_currentWeapon is AnimancySO)
        {
            // Ask this script's own method if there's a valid target
            Enemy target = GetPotentialDeathZoneTarget();
            if (target != null)
            {
                // If there is a target, play the animation...
                playerAnimationController.TriggerTertiaryAttack();
                // ...and tell the PlayerSkillController to activate the skill on that target.
                playerSkillController.TryActivateSkill(SkillIdentifier.DeathZone, target);
            }
            else
            {
                // No valid target, so do nothing.
                Debug.Log("Death Zone: No valid target in range.");
            }
        }
    }

    // Using named methods for weapon switching to ensure stable unsubscription
    private void HandleWeapon1Switch() => SwitchWeaponByIndex(0);
    private void HandleWeapon2Switch() => SwitchWeaponByIndex(1);
    private void HandleWeapon3Switch() => SwitchWeaponByIndex(2);
    
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

    public Coroutine RunSkillCoroutine(IEnumerator routine)
    {
        return StartCoroutine(routine);
    }

    private void EnableInputs()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnPrimaryAttackInput += HandlePrimaryAttack;
            playerInputHandler.OnSecondaryAttackInput += HandleSecondaryAttack;
            playerInputHandler.OnSecondaryAttackPressed += HandleSecondaryAttackPress;
            playerInputHandler.OnSecondaryAttackReleased += HandleSecondaryAttackRelease;
            playerInputHandler.OnTertiaryAttackInput += HandleTertiaryAttack;
            playerInputHandler.OnWeapon1Input += HandleWeapon1Switch;
            playerInputHandler.OnWeapon2Input += HandleWeapon2Switch;
            playerInputHandler.OnWeapon3Input += HandleWeapon3Switch;
        }
    }
    private void DisableInputs()
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

    private void UpdateDeathZoneTargeting(AnimancySO animancyWeapon)
    {
        Enemy newTarget = animancyWeapon.FindClosestTarget(this.gameObject);

        // If the target has changed (or is new/lost)
        if (newTarget != _potentialDeathZoneTarget)
        {
            // Turn off the mark on the old target
            if (_potentialDeathZoneTarget != null)
            {
                _potentialDeathZoneTarget.GetComponentInChildren<EnemyUIController>()?.SetDeathZoneMarkActive(false);
            }

            // Turn on the mark on the new target
            if (newTarget != null)
            {
                newTarget.GetComponentInChildren<EnemyUIController>()?.SetDeathZoneMarkActive(true);
            }

            _potentialDeathZoneTarget = newTarget;
        }
    }

    public Enemy GetPotentialDeathZoneTarget()
    {
        return _potentialDeathZoneTarget;
    }
}