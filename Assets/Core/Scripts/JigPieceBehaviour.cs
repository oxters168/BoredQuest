using UnityEngine;
using UnityHelpers;

public class JigPieceBehaviour : MonoBehaviour
{
    public GrabbableBase GrabbableSelf { get { if (_grabbableSelf == null) _grabbableSelf = GetComponentInParent<GrabbableBase>(); return _grabbableSelf; } }
    private GrabbableBase _grabbableSelf;
    public delegate void OnAttachAttemptHandler(JigBoundaryCollider caller, JigBoundaryCollider other);
    public event OnAttachAttemptHandler onAttachAttempt;

    private int prevGrabberCount;
    private bool justUngrabbed;

    //void Awake()
    //{
    //    grabbableSelf = GetComponent<IGrabbable>();
    //}
    void Update()
    {
        //var grabbableSelf = GetComponentInParent<GrabbableBase>();
        int currentGrabberCount = GrabbableSelf.GetGrabCount();
        justUngrabbed = currentGrabberCount == 0 && prevGrabberCount != currentGrabberCount;
        prevGrabberCount = currentGrabberCount;
    }

    public void OnBoundaryTriggerStay(TreeCollider.CollisionInfo colInfo)
    {
        //Debug.Log("Trigger staying");
        var otherJigBoundary = colInfo.collidedWith.GetComponent<JigBoundaryCollider>();
        var otherJigPiece = colInfo.collidedWith.GetComponentInParent<JigPieceBehaviour>();
        if (otherJigBoundary != null && otherJigPiece != null && justUngrabbed && otherJigPiece.GrabbableSelf.GetGrabCount() <= 0)
            onAttachAttempt?.Invoke(colInfo.sender.GetComponent<JigBoundaryCollider>(), otherJigBoundary);
    }
}
