using UnityEngine;
using TMPro;

public class JigsawStateMachine : MonoBehaviour
{
    public enum GameState { none, gameSetup, inGame, }
    private GameState currentState = GameState.none;

    public GameObject gameSetupPanel;
    public GameObject inGamePanel;

    public Vector2Int maxPieceCount = new Vector2Int(12, 12);
    public TMP_InputField columnsField, rowsField;

    //private JigsawGame jigsawGame;

    void Start()
    {
        //jigsawGame = FindObjectOfType<JigsawGame>();
        gameSetupPanel.SetActive(false);
        inGamePanel.SetActive(false);
    }

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
        var jigsawGame = FindObjectOfType<JigsawGame>();
        if (state == GameState.inGame && state != currentState)
            LoadJigsaw();
        else if (currentState == GameState.inGame && state != currentState)
            jigsawGame.DestroyJigsawPuzzle();
        
        gameSetupPanel.SetActive(state == GameState.gameSetup);
        inGamePanel.SetActive(state == GameState.inGame);

        currentState = state;
    }
    public GameState GetState()
    {
        return currentState;
    }

    private void LoadJigsaw()
    {
        int columns = 0;
        int rows = 0;
        try
        {
            columns = System.Convert.ToInt32(columnsField.text);
        }
        catch {}
        try
        {
            rows = System.Convert.ToInt32(rowsField.text);
        }
        catch {}
        columns = Mathf.Clamp(columns, 2, maxPieceCount.x);
        rows = Mathf.Clamp(rows, 2, maxPieceCount.y);

        var jigsawGame = FindObjectOfType<JigsawGame>();
        jigsawGame.seed = Random.Range(1337, 42070);
        jigsawGame.puzzlePieceCount = new Vector2Int(columns, rows);
        jigsawGame.LoadJigsawPuzzle();
    }
}
