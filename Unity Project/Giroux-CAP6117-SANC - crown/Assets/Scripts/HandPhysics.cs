using UnityEngine;

/// <summary>
/// This class controls the hands physics.
/// </summary>
public class HandPhysics : MonoBehaviour
{
    /// <summary>
    /// Holds a reference to the target hand.
    /// </summary>
    public Transform target;
    /// <summary>
    /// Holds a reference to the rigidbody of the hand.
    /// </summary>
    private Rigidbody rb;
    /// <summary>
    /// Holds a reference to all the hand colliders.
    /// </summary>
    private Collider[] handColliders;

    /// <summary>
    /// Sets the rotation of the rigidbody to the controller position.
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // set it to the controller position
        rb.position = target.position;
        rb.rotation = target.rotation;
        // get the colliders
        handColliders = GetComponentsInChildren<Collider>();
    }

    /// <summary>
    /// Enable the hand colliders.
    /// </summary>
    public void EnableHandColliders()
    {
        foreach (var item in handColliders)
        {
            item.enabled = true;
        }
    }

    /// <summary>
    /// Enable the hand colliders.
    /// </summary>
    public void DisableHandColliders()
    {
        foreach (var item in handColliders)
        {
            item.enabled = false;
        }
    }

    private bool IsVector3Valid(Vector3 v)
{
    return !(float.IsNaN(v.x) || float.IsInfinity(v.x) ||
             float.IsNaN(v.y) || float.IsInfinity(v.y) ||
             float.IsNaN(v.z) || float.IsInfinity(v.z));
}

    /// <summary>
    /// Adjusts the position and rotation of the hand.
    /// </summary>
    void FixedUpdate()
    {
        // position velocity
        rb.velocity = (target.position - transform.position) / Time.fixedDeltaTime;

        // rotation velocity
        Quaternion rotationDifference = target.rotation * Quaternion.Inverse(transform.rotation);
        rotationDifference.ToAngleAxis(out float angleInDegree, out Vector3 rotationAxis);

        // Edge case: angle ≈ 0 can return a broken axis (NaN or Infinity)
        if (angleInDegree > 0.01f && IsVector3Valid(rotationAxis))
        {
            Vector3 rotationDifferenceInDegree = angleInDegree * rotationAxis;
            Vector3 angularVelocity = (rotationDifferenceInDegree * Mathf.Deg2Rad / Time.fixedDeltaTime);

            if (IsVector3Valid(angularVelocity))
            {
                rb.angularVelocity = angularVelocity;
            }
            else
            {
                Debug.LogWarning("Invalid angular velocity calculated: " + angularVelocity);
            }
        }
        else
        {
            // Skip angular velocity update to avoid applying NaNs
            rb.angularVelocity = Vector3.zero;
        }
    }

}
