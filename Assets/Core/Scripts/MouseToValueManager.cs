using UnityEngine;
using UnityHelpers;

public class MouseToValueManager : MonoBehaviour
{
    [RequireInterface(typeof(IValueManager))]
    public GameObject affectedObject;
    private IValueManager valueManager;
    public Camera viewingCamera;

    private bool firstFrame;
    private Vector2 prevMousePos;

    void Start()
    {
        firstFrame = true;
        valueManager = affectedObject.GetComponent<IValueManager>();
        //Cursor.lockState = CursorLockMode.Confined;
    }
    void OnEnable()
    {
        firstFrame = true;
    }

    void Update()
    {
        /*if (!firstFrame)
        {
            float deltaX = Input.mousePosition.x - prevMousePos.x;
            float deltaY = Input.mousePosition.y - prevMousePos.y;
            valueManager.SetAxis("Horizontal", deltaX);
            valueManager.SetAxis("Vertical", deltaY);
        }*/
        Vector3 projectedPoint = viewingCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.y));
        valueManager.SetAxis("Horizontal", projectedPoint.x);
        valueManager.SetAxis("Vertical", projectedPoint.z);
        valueManager.SetToggle("Grab", Input.GetMouseButton(0));

        prevMousePos = Input.mousePosition;
        firstFrame = false;
    }
}
