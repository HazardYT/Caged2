using UnityEngine;

public class ProceduralHandAnimation : MonoBehaviour
{
    [SerializeField] private Transform _leftHand;
    [SerializeField] private Transform _rightHand;

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

    void Update()
    {
        HandleHand(_leftHand, _leftHandTarget, _handOverlapPositions[0].position);
        HandleHand(_rightHand, _rightHandTarget, _handOverlapPositions[1].position);
    }

    void HandleHand(Transform hand, Transform target, Vector3 location)
    {
        Collider[] hitColliders = new Collider[1];
        int numColliders = Physics.OverlapSphereNonAlloc(location, _checkDistance, hitColliders, _wallLayerMask);

        if (numColliders > 0)
        {
            Collider hitCollider = hitColliders[0];

            if (Physics.Raycast(location, hitCollider.transform.position - location, out RaycastHit hit, _wallLayerMask))
            {
                Debug.DrawLine(hand.position, hit.point, Color.red);
                UpdateHandTarget(hand, target, hit);
            }
        }
        else
        {
            // No collider found, move the target back to the original position and rotation
            MoveHandTargetToOriginal(target);
        }
    }
    void UpdateHandTarget(Transform hand, Transform target, RaycastHit hit)
    {
        Quaternion rotation = Quaternion.LookRotation(-hit.normal , hand.up);

        target.SetPositionAndRotation(Vector3.Lerp(target.position, hit.point, _armMoveSpeed * Time.deltaTime),
            Quaternion.Lerp(target.rotation, rotation, _armRotationSpeed * Time.deltaTime));

        Transform[] whatFinger = _leftHandTarget == target ? _leftFingers : _rightFingers;
        foreach (Transform finger in whatFinger)
        {

            if (Physics.Raycast(finger.position, hit.point, out RaycastHit fingerHit, _checkDistance, _wallLayerMask))
            {
                Quaternion fingerRotation = Quaternion.LookRotation(-fingerHit.normal.normalized + fingerHit.point, finger.up);
                finger.rotation = Quaternion.Lerp(finger.rotation, fingerRotation, _fingerBendSpeed * Time.deltaTime);
            }
            else
            {
                // Reset finger rotation when no hit is detected
                finger.localRotation = Quaternion.identity;
            }
        }
    }



    void MoveHandTargetToOriginal(Transform target)
    {
        if (target == _leftHandTarget && target.localPosition != _originalLeftPosition)
        {
            target.SetLocalPositionAndRotation(Vector3.Lerp(target.localPosition, _originalLeftPosition, _armMoveSpeed * Time.deltaTime), 
            Quaternion.Lerp(target.localRotation, _originalLeftRotation, _armRotationSpeed * Time.deltaTime));
        }
        else if (target == _rightHandTarget && target.localPosition != _originalRightPosition)
        {
            target.SetLocalPositionAndRotation(Vector3.Lerp(target.localPosition, _originalRightPosition, _armMoveSpeed * Time.deltaTime), 
            Quaternion.Lerp(target.localRotation, _originalRightRotation, _armRotationSpeed * Time.deltaTime));
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(_handOverlapPositions[0].position, _checkDistance);
        Gizmos.DrawWireSphere(_handOverlapPositions[1].position, _checkDistance);
    }
}
