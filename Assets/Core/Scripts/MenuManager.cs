using UnityEngine;
using Barebones.MasterServer;
using TMPro;
using System.Linq;

public class MenuManager : MonoBehaviour
{
    private NetworkUIBridge msfBridge { get { if (_msfBridge == null) _msfBridge = FindObjectOfType<NetworkUIBridge>(); return _msfBridge; } }
    private NetworkUIBridge _msfBridge;

    private JigsawGame jigsawGame { get { if (_jigsawGame == null) _jigsawGame = FindObjectOfType<JigsawGame>(); return _jigsawGame; } }
    private JigsawGame _jigsawGame;

    public Vector2Int maxPieceCount = new Vector2Int(12, 12);
    public TMP_InputField columnsField, rowsField;
    public float updateInfoTime = 3;

    [Space(10)]
    public GameObject msfScreen;
    public GameObject selfHostScreen;
    public GameObject loadingScreen;

    [Space(10)]
    public TMPro.TMP_InputField roomNameField;
    public TMPro.TextMeshProUGUI playersLabel;
    public TMPro.TextMeshProUGUI roomNameLabel;
    public UnityEngine.UI.Selectable[] enabledOnlyIfConnected;

    [Space(10)]
    public GameObject gameSetupPanel;
    public GameObject inGamePanel;
    public GameObject roomCodePanel;


    private string prevColumnsValue, prevRowsValue;

    private int prevColumns, prevRows;
    private float prevUpdateInfoTime = float.MinValue;

    void Update()
    {
        SetInteractables(Msf.Connection.IsConnected);
        UpdateMenus();
        UpdateOnScreenInfo();

        CheckInputChanges();
        CheckJigsawChanges();
    }

    private void SetInteractables(bool value)
    {
        foreach (var mb in enabledOnlyIfConnected)
            mb.interactable = value;
    }
    private void UpdateOnScreenInfo()
    {
        if (msfBridge.connectedToGameServer && msfBridge.gameInfo != null)
        {
            if (Time.time - prevUpdateInfoTime > updateInfoTime)
            {
                Msf.Client.Matchmaker.FindGames((games) => { Debug.Log("Refreshed games list"); var refreshedInfo = games.FirstOrDefault(game => game.Id == msfBridge.gameInfo.Id); if (refreshedInfo != null) { Debug.Log("Refreshed game info"); msfBridge.gameInfo = refreshedInfo; } });
                //I'll try to figure out later why player count isn't updating
                prevUpdateInfoTime = Time.time;
            }
            playersLabel.text = msfBridge.gameInfo.OnlinePlayers + "/" + msfBridge.gameInfo.MaxPlayers;
            roomNameLabel.text = "Room code: " + msfBridge.gameInfo.Name;
        }
    }
    private void UpdateMenus()
    {
        bool msfScreenActive = !msfBridge.creatingRoom && !msfBridge.connectingToRoom && !msfBridge.connectingToGameServer && !msfBridge.connectedToGameServer;
        bool loadingScreenActive = msfBridge.creatingRoom || msfBridge.connectingToRoom || msfBridge.connectingToGameServer;
        bool gameSetupScreenActive = !msfScreenActive && !loadingScreenActive;

        bool puzzleInProgress = false;
        if (jigsawGame != null)
            puzzleInProgress = jigsawGame.isLoading || jigsawGame.isLoaded;

        msfScreen.SetActive(!puzzleInProgress && msfScreenActive);
        loadingScreen.SetActive(!puzzleInProgress && loadingScreenActive);

        gameSetupPanel.SetActive(!puzzleInProgress && gameSetupScreenActive);
        inGamePanel.SetActive(puzzleInProgress);
        roomCodePanel.SetActive(msfBridge.connectedToGameServer);
    }

    private void CheckJigsawChanges()
    {
        if (jigsawGame != null)
        {
            if (prevColumns != jigsawGame.puzzlePieceCount.x)
                columnsField.text = jigsawGame.puzzlePieceCount.x.ToString();
            if (prevRows != jigsawGame.puzzlePieceCount.y)
                rowsField.text = jigsawGame.puzzlePieceCount.y.ToString();

            prevColumns = jigsawGame.puzzlePieceCount.x;
            prevRows = jigsawGame.puzzlePieceCount.y;
        }
    }

    private void CheckInputChanges()
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
    public void LoadJigsaw()
    {
        var jigsawGame = FindObjectOfType<JigsawGameSync>();
        if (jigsawGame != null)
            jigsawGame.CmdLoadJigsawPuzzle();
        else
            Debug.LogError("Jigsaw game does not exist?!");
    }
    public void UnloadJigsaw()
    {
        var jigsawGame = FindObjectOfType<JigsawGameSync>();
        if (jigsawGame != null)
            jigsawGame.CmdDestroyJigsawPuzzle();
        else
            Debug.LogError("Jigsaw game does not exist?!");
    }
}
