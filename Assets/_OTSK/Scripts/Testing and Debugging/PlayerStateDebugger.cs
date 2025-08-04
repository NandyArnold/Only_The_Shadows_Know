using UnityEngine;

public class PlayerStateDebugger : MonoBehaviour
{
    private void Start()
    {
        // On start, find the HUDManager and register this player's combat component with it.
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.RegisterPlayerForDebugging(GetComponent<PlayerCombat>());
        }
    }
}