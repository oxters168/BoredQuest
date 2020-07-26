using UnityEngine;
using TMPro;
using Mirror;

public class JigsawStateMachine : MonoBehaviour
{
    public enum GameState { none, gameSetup, inGame, }
    private GameState currentState = GameState.none;

    public GameObject gameSetupPanel;
    public GameObject inGamePanel;

    public Vector2Int maxPieceCount = new Vector2Int(12, 12);
    public TMP_InputField columnsField, rowsField;
    private string prevColumnsValue, prevRowsValue;

    //private JigsawGame jigsawGame;

    void Start()
    {
        //jigsawGame = FindObjectOfType<JigsawGame>();
        gameSetupPanel.SetActive(false);
        inGamePanel.SetActive(false);
    }
    void Update()
    {
        var currentColumnsValue = columnsField.text;
        if (currentColumnsValue != prevColumnsValue)
            OnColumnsValueChanged();

        var currentRowsValue = rowsField.text;
        if (currentRowsValue != prevRowsValue)
            OnRowsValueChanged();

        prevColumnsValue = currentColumnsValue;
        prevRowsValue = currentRowsValue;
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
        else if (currentState == GameState.inGame && state != currentState && jigsawGame != null)
            jigsawGame.DestroyJigsawPuzzle();
        
        gameSetupPanel.SetActive(state == GameState.gameSetup);
        inGamePanel.SetActive(state == GameState.inGame);

        currentState = state;
    }
    public GameState GetState()
    {
        return currentState;
    }

    private void OnColumnsValueChanged()
    {
        int columns = 0;
        try
        {
            columns = System.Convert.ToInt32(columnsField.text);
        }
        catch {}
        SetColumnsValue(columns);
    }
    private void OnRowsValueChanged()
    {
        int rows = 0;
        try
        {
            rows = System.Convert.ToInt32(rowsField.text);
        }
        catch {}
        SetRowsValue(rows);
    }

    private void SetColumnsValue(int columns)
    {
        columns = Mathf.Clamp(columns, 2, maxPieceCount.x);
        var jigsawGame = FindObjectOfType<JigsawGameSync>();
        if (jigsawGame != null)
            jigsawGame.CmdSetColumnsValue(columns);
    }
    private void SetRowsValue(int rows)
    {
        rows = Mathf.Clamp(rows, 2, maxPieceCount.y);
        var jigsawGame = FindObjectOfType<JigsawGameSync>();
        if (jigsawGame != null)
            jigsawGame.CmdSetRowsValue(rows);
    }
    private void LoadJigsaw()
    {
        var jigsawGame = FindObjectOfType<JigsawGameSync>();
        if (jigsawGame != null)
            jigsawGame.CmdLoadJigsawPuzzle();
        else
            Debug.LogError("Jigsaw game does not exist?!");
    }
}
