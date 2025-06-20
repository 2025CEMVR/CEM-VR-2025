using UnityEngine;

public class Billboard2D : MonoBehaviour
{
    public Transform cameraTransform;
    public float enableDistance = 20f;
    private Quaternion originalRotation;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        originalRotation = transform.rotation;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, cameraTransform.position);

        if (distance >= enableDistance)
        {
            Vector3 lookDir = cameraTransform.position - transform.position;
            lookDir.y = 0; // Prevents tilting up/down
            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }
        else
        {
            transform.rotation = originalRotation;
        }
    }
}
