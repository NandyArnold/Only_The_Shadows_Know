// Create this new script: UISoundsSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "UI_Sounds", menuName = "Only The Shadows Know/Audio/UI Sounds")]
public class UISoundsSO : ScriptableObject
{
    [Header("Core Feedback")]
    public AudioClip hover;
    public AudioClip clickConfirm;
    public AudioClip clickBack;
    public AudioClip error;

    [Header("Transitions & Notifications")]
    public AudioClip menuOpen;
    public AudioClip menuClose;
    public AudioClip toggle;
    public AudioClip newObjective;
}