using UnityEngine;
using Nakama;
using System.Collections.Generic;

public class NakamaClient : MonoBehaviour
{
    public static NakamaClient singleton;
    public static bool connectedMaster { get { return singleton.session.Created; } }
    public TMPro.TMP_InputField matchIdField;
    public TMPro.TMP_InputField displayNameField;

    private Client client;
    private ISession session;
    private ISocket socket;

    private string matchId;
    private IMatch match;

    void Awake()
    {
        singleton = this;
    }
    async void Start()
    {
        //Connect to master server
        client = new Client("http", "192.168.2.131", 7350, "defaultkey");

        //Authenticate user and receive session object
        var deviceId = PlayerPrefs.GetString("nakama.deviceid");
        if (string.IsNullOrEmpty(deviceId)) {
            deviceId = SystemInfo.deviceUniqueIdentifier;
            PlayerPrefs.SetString("nakama.deviceid", deviceId); // cache device id.
        }
        session = await client.AuthenticateDeviceAsync(deviceId);
        Debug.LogFormat("New user: {0}, {1}", session.Created, session);

        //Get current display name from account
        var account = await client.GetAccountAsync(session);
        var user = account.User;
        Debug.LogFormat("User id '{0}' username '{1}'", user.Id, user.Username);
        Debug.LogFormat("User wallet: '{0}'", account.Wallet);
        displayNameField.text = user.DisplayName;

        //Create socket to communicate with the server
        socket = client.NewSocket();
        socket.Connected += () =>
        {
            Debug.Log("Socket connected.");
        };
        socket.Closed += () =>
        {
            Debug.Log("Socket closed.");
        };
        await socket.ConnectAsync(session);

        //Subscribe to match presence event
        var connectedOpponents = new List<IUserPresence>(2);
        socket.ReceivedMatchPresence += presenceEvent =>
        {
            foreach (var presence in presenceEvent.Leaves)
            {
                connectedOpponents.Remove(presence);
            }
            connectedOpponents.AddRange(presenceEvent.Joins);
            // Remove yourself from connected opponents.
            // if (match != null && match.Presences != null)
            //     foreach (var self in match.Presences)
            //         if (self != null && connectedOpponents.Contains(self))
            //             connectedOpponents.Remove(self);
            Debug.LogFormat("Connected opponents: [{0}]", string.Join(",\n  ", connectedOpponents));
        };
    }

    public async void JoinRoom()
    {
        matchId = matchIdField.text;
        match = await socket.JoinMatchAsync(matchId);
        foreach (var presence in match.Presences)
        {
            Debug.LogFormat("User id '{0}' name '{1}'.", presence.UserId, presence.Username);
        }
    }
    public async void CreateRoom()
    {
        match = await socket.CreateMatchAsync();
        Debug.LogFormat("New match with id '{0}'.", match.Id);
    }
    public async void SetDisplayName()
    {
        string displayName = displayNameField.text;
        await client.UpdateAccountAsync(session, null, displayName, null, null, null);
    }
}
