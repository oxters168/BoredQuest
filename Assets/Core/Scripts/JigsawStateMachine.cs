using UnityEngine;
using TMPro;

public class JigsawStateMachine : MonoBehaviour
{
    public enum GameState { none, gameSetup, inGame, }
    private GameState currentState = GameState.none;

    private JigsawGame jigsawGame { get { if (_jigsawGame == null) _jigsawGame = FindObjectOfType<JigsawGame>(); return _jigsawGame; } }
    private JigsawGame _jigsawGame;

    public void StartGame()
    {
        SetState(GameState.inGame);
    }
    public void EndGame()
    {
        SetState(GameState.gameSetup);
    }
    public void SetState(GameState state)
    {
        //var jigsawGame = FindObjectOfType<JigsawGame>();
        if (state == GameState.inGame && state != currentState)
            ;//LoadJigsaw();
        else if (currentState == GameState.inGame && state != currentState && jigsawGame != null)
            jigsawGame.DestroyJigsawPuzzle();
        
        currentState = state;
    }
    public GameState GetState()
    {
        return currentState;
    }

}
