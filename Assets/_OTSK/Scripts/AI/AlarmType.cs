public enum AlarmType
{
    None,               // This enemy type never sounds an alarm
    GoToPanel,          // This enemy runs to the nearest physical panel
    SignalFromPosition  // This enemy raises a "magical" or radio alarm from its current spot
}