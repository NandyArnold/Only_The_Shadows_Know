// GameStates.cs

public enum GameState
{
    // For when the player is in a main menu or pause menu.
    Menu,
    // For when the player has direct control in the game world.
    Gameplay,
    // For when a cinematic is playing and UI/Player Input should be disabled.
    Cutscene,
    // For when the game is transitioning between scenes.
    Loading,
    // For when the game is saving data, such as player progress or world state.
    Saving,
    // For when the game is in a state where the player has lost, such as after dying.
    GameOver,
    // For when the player is viewing detailed information about an item or character.
    Details, 

}
