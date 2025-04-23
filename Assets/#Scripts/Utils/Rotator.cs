using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private Vector3 _axis = Vector3.forward * 60f;
    [SerializeField] private Space _space = Space.Self;

    private void Update()
    {
        transform.Rotate(_axis * Time.deltaTime, _space);
    }
}
