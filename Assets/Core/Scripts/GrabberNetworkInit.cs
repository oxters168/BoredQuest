using UnityEngine;
using System.ComponentModel;

public class GrabberNetworkInit : MonoBehaviour//NetworkBehaviour
{
    /*[Tooltip("Set to true if moves come from owner client, set to false if moves always come from server")]
    public bool clientAuthority;
    private float lastClientSendTime = float.MinValue;
    // Is this a client with authority over this transform?
    // This component could be on the player object or any object that has been assigned authority to this client.
    bool IsClientWithAuthority => hasAuthority && clientAuthority;
    public float changeTolerance = 0.01f;
    
    private Movement2D Movement { get { if (_movement == null) _movement = GetComponent<Movement2D>(); return _movement; } }
    private Movement2D _movement;
    private float horizontal;
    private float prevHorizontal;
    private float vertical;
    private float prevVertical;
    private bool grab;
    private bool prevGrab;

    void Start()
    {
        //movement = GetComponent<Movement2D>();
        if (hasAuthority)
        {
            var mouseToValue = GetComponent<MouseToValueManager>();
            mouseToValue.enabled = true;
        }
        else if (isServer)
        {
            var grabber = GetComponent<UnityHelpers.Grabber>();
            grabber.enabled = true;
        }
    }

    void Update()
    {
        // if server then always sync to others.
        if (isServer)
        {
            // just use OnSerialize via SetDirtyBit only sync when position
            // changed. set dirty bits 0 or 1
            SetDirtyBit(HasChanged() ? 1UL : 0UL);
        }

        // no 'else if' since host mode would be both
        if (isClient)
        {
            // send to server if we have local authority (and aren't the server)
            // -> only if connectionToServer has been initialized yet too
            if (!isServer && IsClientWithAuthority)
            {
                // check only each 'syncInterval'
                if (Time.time - lastClientSendTime >= syncInterval)
                {
                    if (HasChanged())
                    {
                        // serialize
                        // local position/rotation for VR support
                        using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
                        {
                            SerializeIntoWriter(writer, Movement.GetAxis("Horizontal"), Movement.GetAxis("Vertical"), Movement.GetToggle("Grab"));

                            // send to server
                            CmdClientToServerSync(writer.ToArray());
                        }
                    }
                    lastClientSendTime = Time.time;
                }
            }

            // apply interpolation on client for all players
            // unless this client has authority over the object. could be
            // himself or another object that he was assigned authority over
            if (!IsClientWithAuthority)
            {
                ApplyValues();
                // received one yet? (initialized?)
                // if (goal != null)
                // {
                //     // teleport or interpolate
                //     if (NeedsTeleport())
                //     {
                //         // local position/rotation for VR support
                //         ApplyPositionRotationScale(goal.localPosition, goal.localRotation, goal.localScale);

                //         // reset data points so we don't keep interpolating
                //         start = null;
                //         goal = null;
                //     }
                //     else
                //     {
                //         // local position/rotation for VR support
                //         ApplyPositionRotationScale(InterpolatePosition(start, goal, targetComponent.transform.localPosition),
                //                                     InterpolateRotation(start, goal, targetComponent.transform.localRotation),
                //                                     InterpolateScale(start, goal, targetComponent.transform.localScale));
                //     }
                // }
            }
        }
    }

    // moved since last time we checked it?
    bool HasChanged()
    {
        var horizontal = Movement.GetAxis("Horizontal");
        var vertical = Movement.GetAxis("Vertical");
        var grab = Movement.GetToggle("Grab");

        // moved or rotated or scaled?
        // local position/rotation/scale for VR support
        bool horizontalChanged = Mathf.Abs(prevHorizontal - horizontal) > changeTolerance;
        bool verticalChanged = Mathf.Abs(prevVertical - vertical) > changeTolerance;
        bool changes = horizontalChanged || verticalChanged || (grab != prevGrab);

        // save last for next frame to compare
        // (only if change was detected. otherwise slow moving objects might
        //  never sync because of C#'s float comparison tolerance. see also:
        //  https://github.com/vis2k/Mirror/pull/428)
        if (changes)
        {
            // local position/rotation for VR support
            prevHorizontal = horizontal;
            prevVertical = vertical;
            prevGrab = grab;
        }
        return changes;
    }

    public override bool OnSerialize(NetworkWriter writer, bool initialState)
    {
        SerializeIntoWriter(writer, Movement.GetAxis("Horizontal"), Movement.GetAxis("Vertical"), Movement.GetToggle("Grab"));
        return true;
    }
    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
        DeserializeFromReader(reader);
    }

    // serialization is needed by OnSerialize and by manual sending from authority
    // public only for tests
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SerializeIntoWriter(NetworkWriter writer, float horizontalw, float verticalw, bool grabw)
    {
        // serialize position, rotation, scale
        // note: we do NOT compress rotation.
        //       we are CPU constrained, not bandwidth constrained.
        //       the code needs to WORK for the next 5-10 years of development.
        writer.WriteSingle(horizontalw);
        writer.WriteSingle(verticalw);
        writer.WriteBoolean(grabw);
    }
    private void DeserializeFromReader(NetworkReader reader)
    {
        horizontal = reader.ReadSingle();
        vertical = reader.ReadSingle();
        grab = reader.ReadBoolean();
    }
    private void ApplyValues()
    {
        Movement.SetAxis("Horizontal", horizontal);
        Movement.SetAxis("Vertical", vertical);
        Movement.SetToggle("Grab", grab);
    }

    // local authority client sends sync message to server for broadcasting
    [Command]
    void CmdClientToServerSync(byte[] payload)
    {
        // Ignore messages from client if not in client authority mode
        if (!clientAuthority)
            return;

        // deserialize payload
        using (PooledNetworkReader networkReader = NetworkReaderPool.GetReader(payload))
            DeserializeFromReader(networkReader);

        // server-only mode does no interpolation to save computations,
        // but let's set the position directly
        if (isServer && !isClient)
            ApplyValues();

        // set dirty so that OnSerialize broadcasts it
        SetDirtyBit(1UL);
    }*/
}
