// CastMode.cs
public enum CastMode
{
    // For skills that happen immediately upon key press.
    Instant,
    // For skills that require a special targeting mode before activation.
    Targeted,
    // For skills that require the key to be held down (we can implement this later).
    Channel
}