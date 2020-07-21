using UnityEngine;
using UnityHelpers;
public class Movement2D : MonoBehaviour, IValueManager
{
    public float movementMultiplier = 0.01f;
    public Grabber grabber;

    [Space(10)]
    public ValuesVault valuesVault;

    void Update()
    {
        //transform.position += new Vector3(GetAxis("Horizontal"), transform.position.y, GetAxis("Vertical")) * movementMultiplier;
        transform.position = new Vector3(GetAxis("Horizontal"), transform.position.y, GetAxis("Vertical"));
        grabber.grab = GetToggle("Grab");
    }

    public float GetAxis(string name)
    {
        return valuesVault.GetValue(name).GetAxis();
    }
    public Vector3 GetDirection(string name)
    {
        return valuesVault.GetValue(name).GetDirection();
    }
    public Quaternion GetOrientation(string name)
    {
        return valuesVault.GetValue(name).GetOrientation();
    }
    public Vector3 GetPoint(string name)
    {
        return valuesVault.GetValue(name).GetPoint();
    }
    public bool GetToggle(string name)
    {
        return valuesVault.GetValue(name).GetToggle();
    }
    public void SetAxis(string name, float value)
    {
        valuesVault.GetValue(name).SetAxis(value);
    }
    public void SetDirection(string name, Vector3 value)
    {
        valuesVault.GetValue(name).SetDirection(value);
    }
    public void SetOrientation(string name, Quaternion value)
    {
        valuesVault.GetValue(name).SetOrientation(value);
    }
    public void SetPoint(string name, Vector3 value)
    {
        valuesVault.GetValue(name).SetPoint(value);
    }
    public void SetToggle(string name, bool value)
    {
        valuesVault.GetValue(name).SetToggle(value);
    }
}
