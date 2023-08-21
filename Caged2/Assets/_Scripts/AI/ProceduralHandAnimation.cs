using UnityEngine;

public class ProceduralHandAnimation : MonoBehaviour
{
    [SerializeField] private Transform _leftHandTarget;
    [SerializeField] private Transform _rightHandTarget;
    [SerializeField] private Transform[] _handOverlapPositions;
    [SerializeField] private Transform[] _leftFingers;
    [SerializeField] private Transform[] _rightFingers;
    [SerializeField] private LayerMask _wallLayerMask;
    [SerializeField] private float _armMoveSpeed;
    [SerializeField] private float _armRotationSpeed;
    [SerializeField] private float _fingerBendSpeed;
    [SerializeField] private float _checkDistance;
    [SerializeField] private float _handOffset;

    // Store the original positions and rotations of the hand targets
    private Vector3 _originalLeftPosition;
    private Quaternion _originalLeftRotation;
    private Vector3 _originalRightPosition;
    private Quaternion _originalRightRotation;

    private void Start()
    {
        // Store the original positions and rotations
        _originalLeftPosition = _leftHandTarget.localPosition;
        _originalLeftRotation = _leftHandTarget.localRotation;
        _originalRightPosition = _rightHandTarget.localPosition;
        _originalRightRotation = _rightHandTarget.localRotation;
    }

    void LateUpdate()
    {
        HandleHand(_leftHandTarget, _handOverlapPositions[0].position);
        HandleHand(_rightHandTarget, _handOverlapPositions[1].position);
    }

void HandleHand(Transform target, Vector3 location)
{
    Collider[] hitColliders = new Collider[1];
    int numColliders = Physics.OverlapSphereNonAlloc(location, _checkDistance, hitColliders, _wallLayerMask);
    if (numColliders > 0)
    {
        Collider hitCollider = hitColliders[0];
        Vector3 raycastDirection = hitCollider.ClosestPoint(location) - location;
        if (Physics.Raycast(location, raycastDirection, out RaycastHit hit, _checkDistance, _wallLayerMask))
        {   
            Debug.DrawLine(target.position, hit.point, Color.red);
            UpdateHandTarget(target, hit);
        }
    }
    else MoveHandTargetToOriginal(target); // No collider found, move the target back to the original position and rotation

}

    void UpdateHandTarget(Transform target, RaycastHit hit)
    {
        Quaternion rotation = Quaternion.LookRotation(-hit.normal, target.up);
        Quaternion crookedCorrection = Quaternion.Euler(0f, _leftHandTarget == target ? -30f : 30f, 0f);
        rotation *= crookedCorrection;

         // Apply a small offset along the hit normal to prevent clipping through the surface
        Vector3 offsetPosition = hit.point + hit.normal * _handOffset;

        target.SetPositionAndRotation(Vector3.Lerp(target.position, offsetPosition, _armMoveSpeed * Time.deltaTime), 
        Quaternion.Lerp(target.rotation, rotation, 1 * _armRotationSpeed * Time.deltaTime));

        foreach (Transform finger in _leftHandTarget == target ? _leftFingers : _rightFingers)
        {
            Vector3 raycastDirection = hit.collider.ClosestPoint(hit.point) - hit.point;
            if (Physics.Raycast(finger.position, raycastDirection * _handOffset, out RaycastHit fingerHit, _wallLayerMask))
            {
                Quaternion fingerRotation = Quaternion.LookRotation(_leftHandTarget == target? fingerHit.normal : -fingerHit.normal, finger.up);
                finger.rotation = Quaternion.Lerp(finger.rotation, fingerRotation, 1 * _fingerBendSpeed * Time.deltaTime);
            }
            else finger.localRotation = Quaternion.identity; // Reset finger rotation when no hit is detected 
        }
    }



    void MoveHandTargetToOriginal(Transform target)
    {
        if (target == _leftHandTarget && target.localPosition != _originalLeftPosition)
        {
            target.SetLocalPositionAndRotation(Vector3.Lerp(target.localPosition, _originalLeftPosition, _armMoveSpeed * Time.deltaTime), 
            Quaternion.Lerp(target.localRotation, _originalLeftRotation, 1 * _armRotationSpeed * Time.deltaTime));
        }
        else if (target == _rightHandTarget && target.localPosition != _originalRightPosition)
        {
            target.SetLocalPositionAndRotation(Vector3.Lerp(target.localPosition, _originalRightPosition, _armMoveSpeed * Time.deltaTime), 
            Quaternion.Lerp(target.localRotation, _originalRightRotation, 1 * _armRotationSpeed * Time.deltaTime));
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(_handOverlapPositions[0].position, _checkDistance);
        Gizmos.DrawWireSphere(_handOverlapPositions[1].position, _checkDistance);
    }
}
