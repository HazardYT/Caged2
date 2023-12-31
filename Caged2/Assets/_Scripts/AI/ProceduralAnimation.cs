using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralAnimation : MonoBehaviour
{
    /* Some useful functions we may need *////////////////

    static Vector3[] CastOnSurface(Vector3 point, float halfRange, Vector3 up)
    {
        Vector3[] res = new Vector3[2];
        Ray ray = new(new Vector3(point.x, point.y + halfRange, point.z), -up);

        if (Physics.Raycast(ray, out RaycastHit hit, 2f * halfRange))
        {
            res[0] = hit.point;
            res[1] = hit.normal;
        }
        else
        {
            res[0] = point;
        }
        return res;
    }

    /*************************************/

    public Transform leftFootTarget;
    public Transform rightFootTarget;
    public Transform leftFootTargetRig;
    public Transform rightFootTargetRig;
    public Transform pivot;
    public Transform scaler;

    public float smoothness = 2f;
    public float stepHeight = 0.2f;
    public float stepLength = 1f;
    public float angularSpeed = 0.1f;
    public float velocityMultiplier = 80f;
    public float bounceAmplitude = 0.05f;
    public bool running = false;
    public float LerpSpeed = 5f; // Added LerpSpeed variable

    private Vector3 lastBodyPos;
    private Vector3 initBodyPos;

    private Vector3 velocity;
    private Vector3 lastVelocity;

    // Start is called before the first frame update
    void Start()
    {
        lastBodyPos = transform.position;
        initBodyPos = transform.localPosition;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        velocity = transform.position - lastBodyPos;
        velocity *= velocityMultiplier;
        velocity = (velocity + smoothness * lastVelocity) / (smoothness + 1f);

        if (velocity.magnitude < 0.000025f * velocityMultiplier)
            velocity = lastVelocity;
        lastVelocity = velocity;

        int sign = Vector3.Dot(velocity.normalized, transform.forward) < 0 ? -1 : 1;

        scaler.localScale = new Vector3(scaler.localScale.x, stepHeight * 2f * velocity.magnitude, stepLength * velocity.magnitude);
        scaler.rotation = Quaternion.LookRotation(sign * velocity.normalized, Vector3.up);
        pivot.Rotate(Vector3.right, sign * angularSpeed, Space.Self);

        Vector3 desiredPositionLeft = leftFootTarget.position;
        Vector3 desiredPositionRight = rightFootTarget.position;

        Vector3[] posNormLeft = CastOnSurface(desiredPositionLeft, 2f, Vector3.up);
        if (posNormLeft[0].y > desiredPositionLeft.y)
        {
            leftFootTargetRig.position = Vector3.Lerp(leftFootTargetRig.position, posNormLeft[0], LerpSpeed * Time.deltaTime);
        }
        else
        {
            leftFootTargetRig.position = Vector3.Lerp(leftFootTargetRig.position, desiredPositionLeft, LerpSpeed * Time.deltaTime);
        }
        if (posNormLeft[1] != Vector3.zero)
        {
            leftFootTargetRig.rotation = Quaternion.Lerp(leftFootTargetRig.rotation, Quaternion.LookRotation(sign * velocity.normalized, posNormLeft[1]), LerpSpeed * Time.deltaTime);
        }

        Vector3[] posNormRight = CastOnSurface(desiredPositionRight, 2f, Vector3.up);
        if (posNormRight[0].y > desiredPositionRight.y)
        {
            rightFootTargetRig.position = Vector3.Lerp(rightFootTargetRig.position, posNormRight[0], LerpSpeed * Time.deltaTime);
        }
        else
        {
            rightFootTargetRig.position = Vector3.Lerp(rightFootTargetRig.position, desiredPositionRight, LerpSpeed * Time.deltaTime);
        }
        if (posNormRight[1] != Vector3.zero)
        {
            rightFootTargetRig.rotation = Quaternion.Lerp(rightFootTargetRig.rotation, Quaternion.LookRotation(sign * velocity.normalized, posNormRight[1]), LerpSpeed * Time.deltaTime);
        }

        float feetDistance = Mathf.Clamp01(Mathf.Abs(leftFootTargetRig.localPosition.z - rightFootTargetRig.localPosition.z) / (stepLength / 4f));

        float heightReduction = running ? bounceAmplitude - bounceAmplitude * Mathf.Clamp01(velocity.magnitude) * feetDistance : bounceAmplitude * Mathf.Clamp01(velocity.magnitude) * feetDistance;
        transform.localPosition = Vector3.Lerp(transform.localPosition, initBodyPos - heightReduction * Vector3.up, LerpSpeed * Time.deltaTime);
        scaler.localPosition = Vector3.Lerp(scaler.localPosition, new Vector3(0f, heightReduction, 0f), LerpSpeed * Time.deltaTime);

        lastBodyPos = transform.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(leftFootTarget.position, 0.2f);
        Gizmos.DrawWireSphere(rightFootTarget.position, 0.2f);
    }
}