using UnityEngine;

public class JigBoundaryCollider : MonoBehaviour
{
    public enum BoundarySide { none, top, bottom, left, right }

    public BoundarySide boundarySide;
    public JigPieceBehaviour jigPiece;
    public Vector3 boxCenter;
    public Vector3 boxSize;

    private UnityHelpers.CollisionListener collisionListener;

    void Start()
    {
        var boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        boxCollider.center = boxCenter;
        boxCollider.size = boxSize;

        var rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;

        //jigPiece = GetComponentInParent<JigPieceBehaviour>();
        collisionListener = gameObject.AddComponent<UnityHelpers.CollisionListener>();
        collisionListener.OnTriggerStayEvent += jigPiece.OnBoundaryTriggerStay;
    }
    void OnEnable()
    {
        //Debug.Log("Collision listener is null: " + (collisionListener == null) + " Jig piece is null: " + (jigPiece == null));
        //collisionListener.OnTriggerStayEvent += jigPiece.OnBoundaryTriggerStay;
    }
    void OnDisable()
    {
        //collisionListener.OnTriggerStayEvent -= jigPiece.OnBoundaryTriggerStay;
    }
}
