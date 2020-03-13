using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class CameraController : MonoBehaviour
{
    [SerializeField]
    GameObject objectToFollow;

    [SerializeField]
    GameObject cameraBoomStart;

    [SerializeField]
    GameObject cameraBoomEnd;

    [SerializeField]
    GameObject cameraObject;

    [SerializeField]
    Camera playerCamera;

    [SerializeField]
    [MinAttribute(0)]
    float cameraDistance = 0f, maxDistance = 5f, minDistance = 1f, centerCameraDistance = 1f, centerCameraArcFactor = 3f, avoidanceRadius = 0.15f;

    [SerializeField]
    List<string> cameraBoomIgnoreTags;


    private float targetCameraDistance = 0f;
    private float targetCameraDistanceCached = 0f;

    private Vector3 targetPosCamBoomStart = Vector3.zero;
    private Vector3 overriddenTargetPosCamBoomStart = Vector3.zero;
    private Vector3 originMoveTo = Vector3.zero;
    private float lerpSpeed = 0f;
    private bool cameraDistanceOverridden = false, overrideMinDistance = false;
    private float cameraBoomHitDelta = 1f;
    private float currentStartCheckAvoidanceRadius, currentEndCheckAvoidanceRadius;
    private bool zoomable = true;
    private float minXRotation = -50f, maxXRotation = 50f;


    //Remove
    Vector3 HitPosition = Vector3.zero;


    [SerializeField]
    GameObject rightHandSidePos = null, leftHandSidePos = null;

    private void Awake()
    {

        if (!cameraBoomStart)
        {
            cameraBoomStart = new GameObject("Camera Boom Start");
            cameraBoomStart.transform.parent = transform;
        }
        if (!cameraBoomEnd)
        {
            cameraBoomEnd = new GameObject("Camera Boom End");
            cameraBoomEnd.transform.parent = cameraBoomStart.transform;
        }
        if (!cameraObject)
        {
            cameraObject = new GameObject("Viewport");
            cameraObject.transform.parent = cameraBoomEnd.transform;
        }
        if (!playerCamera)
        {
            playerCamera = cameraObject.AddComponent(typeof(Camera)) as Camera;
            playerCamera.nearClipPlane = 0.01f;
        }
        if (!leftHandSidePos)
        {
            leftHandSidePos = new GameObject("Left Hand Side Pos");
            leftHandSidePos.transform.parent = transform;
            leftHandSidePos.transform.localPosition = new Vector3(-0.75f, 0.87f, 0);
        }
        if (!rightHandSidePos)
        {
            rightHandSidePos = new GameObject("Right Hand Side Pos");
            rightHandSidePos.transform.parent = transform;
            rightHandSidePos.transform.localPosition = new Vector3(0.75f, 0.87f, 0);
        }

        if (targetPosCamBoomStart == Vector3.zero)
        {
            targetPosCamBoomStart = rightHandSidePos.transform.localPosition;
            originMoveTo = leftHandSidePos.transform.localPosition;
            cameraBoomStart.transform.localPosition = rightHandSidePos.transform.localPosition;
        }
        currentStartCheckAvoidanceRadius = avoidanceRadius;
        currentEndCheckAvoidanceRadius = avoidanceRadius;
        targetCameraDistance = targetCameraDistanceCached = cameraDistance;
    }

    void FixedUpdate()
    {
        Vector3 startpoint = cameraBoomStart.transform.position;
        Vector3 endpoint = cameraBoomEnd.transform.position;

        //Debug.DrawLine(startpoint, startpoint + (endpoint - startpoint), Color.red);
        //Debug.DrawLine(endpoint, endpoint + Mathf.Sin(Mathf.Acos(Vector3.Dot((endpoint - startpoint).normalized, -cameraBoomStart.transform.forward))) * (endpoint - startpoint).magnitude * -cameraBoomStart.transform.up, Color.blue);
        //Debug.DrawLine(startpoint, startpoint + (Vector3.Dot((endpoint - startpoint).normalized, -cameraBoomStart.transform.forward)) * (endpoint - startpoint).magnitude * -cameraBoomStart.transform.forward, Color.blue);
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isPlaying)
        {
            // check input
            lerpSpeed = Time.deltaTime * 5f;

            if (Input.GetKeyDown(KeyCode.E))
            {
                targetPosCamBoomStart = rightHandSidePos.transform.localPosition;
                originMoveTo = leftHandSidePos.transform.localPosition;
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                targetPosCamBoomStart = leftHandSidePos.transform.localPosition;
                originMoveTo = rightHandSidePos.transform.localPosition;
            }
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (zoomable && scrollInput != 0)
            {
                targetCameraDistance += -scrollInput * Mathf.Abs(scrollInput) * 80f;
                targetCameraDistance = Mathf.Clamp(targetCameraDistance, minDistance, maxDistance);
                cameraDistanceOverridden = false;
                targetCameraDistanceCached = cameraDistance;
            }
            if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
            {
                cameraBoomStart.transform.rotation *= Quaternion.AngleAxis(-Input.GetAxis("Mouse Y"), Vector3.right);
                transform.rotation *= Quaternion.AngleAxis(Input.GetAxis("Mouse X"), Vector3.up);

                Vector3 camboomstartEuler = cameraBoomStart.transform.rotation.eulerAngles;
                if (camboomstartEuler.x > maxXRotation && camboomstartEuler.x < 180f)
                {
                    camboomstartEuler.x = maxXRotation;
                }
                else if (camboomstartEuler.x < 360 + minXRotation && camboomstartEuler.x > 180f)
                {
                    camboomstartEuler.x = minXRotation;
                }
                cameraBoomStart.transform.rotation = Quaternion.Euler(camboomstartEuler);

            }
            zoomable = true;
            overrideMinDistance = false;


            if (objectToFollow)
            {
                transform.position = new Vector3(
                    Mathf.Lerp(transform.position.x, objectToFollow.transform.position.x, lerpSpeed),
                    Mathf.Lerp(transform.position.y, objectToFollow.transform.position.y, lerpSpeed * 5f),
                    Mathf.Lerp(transform.position.z, objectToFollow.transform.position.z, lerpSpeed)
                    );

                transform.position = Vector3.Lerp(transform.position, objectToFollow.transform.position, lerpSpeed);
            }






            //make cameraboom start go collide




            cameraBoomStart.transform.localPosition = Vector3.Lerp(cameraBoomStart.transform.localPosition, overriddenTargetPosCamBoomStart, lerpSpeed);

            float dist = ((cameraBoomEnd.transform.position) - cameraBoomStart.transform.position).magnitude;
            float actualCamDistance = (Vector3.Dot((cameraBoomEnd.transform.position - cameraBoomStart.transform.position).normalized, -cameraBoomStart.transform.forward)) * dist;

            targetPosCamBoomStart = new Vector3(
                actualCamDistance < centerCameraDistance ? Mathf.Pow(Mathf.Cos((Mathf.PI / ((centerCameraDistance - 1) * 2)) * actualCamDistance - (Mathf.PI / ((centerCameraDistance - 1) * 2))), centerCameraArcFactor) * -originMoveTo.x : 0,
                originMoveTo.y,
                originMoveTo.z
                );


            Vector3 startpoint = transform.position;
            Vector3 endpoint = cameraBoomStart.transform.position;

            Vector3 camBoomStartDelta = Vector3.zero;
            bool hitImportant = false;
            RaycastHit[] camBoomStartHits = Physics.SphereCastAll(startpoint, currentStartCheckAvoidanceRadius * 3f, (endpoint - startpoint), (endpoint - startpoint).magnitude);
            if (camBoomStartHits.Length > 0)
            {
                foreach (RaycastHit hit in camBoomStartHits)
                {
                    if (!cameraBoomIgnoreTags.Contains(hit.transform.tag))
                    {
                        hitImportant = true;
                        HitPosition = hit.point + hit.normal;
                        cameraBoomStart.transform.position = (hit.point + hit.normal * currentStartCheckAvoidanceRadius * 2.8f);
                        overriddenTargetPosCamBoomStart = cameraBoomStart.transform.position - transform.position;
                        //cameraDistance = actualCamDistance;
                        overrideMinDistance = true;
                        Vector3 temp = (targetPosCamBoomStart - cameraBoomStart.transform.localPosition);
                        camBoomStartDelta = temp.x * transform.right + temp.y * transform.up + temp.z * transform.forward;

                        currentStartCheckAvoidanceRadius = avoidanceRadius * 1.1f;
                        break;
                    }
                }
            }
            if (!hitImportant)
            {
                currentStartCheckAvoidanceRadius = avoidanceRadius;
                cameraBoomStart.transform.localPosition = overriddenTargetPosCamBoomStart = targetPosCamBoomStart;
            }



            cameraDistance = Mathf.Lerp(cameraDistance, targetCameraDistance, lerpSpeed);



            


            //make cameraboomend go collide



            cameraBoomEnd.transform.localPosition = new Vector3(
                0,
                (cameraDistance < 3 ? (Mathf.Sin(cameraDistance * 0.5f) * Mathf.Pow(cameraDistance, 3)) / 111.5f : cameraDistance * 0.25f - 0.5085f),
                -cameraDistance
                );


            startpoint = transform.position;
            endpoint = cameraBoomStart.transform.position + cameraBoomStart.transform.up * (maxDistance * 0.25f - 0.5085f) - cameraBoomStart.transform.forward * maxDistance + camBoomStartDelta;


            RaycastHit[] hits = Physics.SphereCastAll(startpoint, currentEndCheckAvoidanceRadius, (endpoint - startpoint).normalized, targetCameraDistanceCached);// (cameraBoomEnd.transform.position - startpoint).magnitude);
            RaycastHit nearestHit = new RaycastHit();
            float nearestHitLength = float.MaxValue;
            hitImportant = false;
            foreach (RaycastHit hit in hits)
            {
                if (!cameraBoomIgnoreTags.Contains(hit.transform.tag))
                {
                    hitImportant = true;
                    float distance = ((hit.point + hit.normal * currentEndCheckAvoidanceRadius) - startpoint).magnitude;
                    if (distance < nearestHitLength)
                    {
                        nearestHit = hit;
                        nearestHitLength = distance;
                    }
                }
            }
            if (hitImportant)
            {
                HitPosition = cameraBoomEnd.transform.position = nearestHit.point + nearestHit.normal * currentEndCheckAvoidanceRadius;

                dist = ((cameraBoomEnd.transform.position) - cameraBoomStart.transform.position).magnitude;
                cameraDistance = (Vector3.Dot((cameraBoomEnd.transform.position - cameraBoomStart.transform.position).normalized, -cameraBoomStart.transform.forward)) * dist;

                currentEndCheckAvoidanceRadius = avoidanceRadius * 1.1f;
                overrideMinDistance = true;
                if (!cameraDistanceOverridden)
                {
                    cameraDistanceOverridden = true;
                }
                zoomable = false;
            }

            if (!hitImportant)
            {
                cameraBoomEnd.transform.localPosition += camBoomStartDelta;
                currentEndCheckAvoidanceRadius = avoidanceRadius;
                if (cameraDistanceOverridden)
                {
                    targetCameraDistance = targetCameraDistanceCached;
                    cameraDistanceOverridden = false;
                }
            }

            if (overrideMinDistance)
            {
                cameraDistance = Mathf.Clamp(cameraDistance, Mathf.Epsilon, maxDistance);
                targetCameraDistance = Mathf.Clamp(targetCameraDistance, Mathf.Epsilon, maxDistance);
            }
            else
            {
                cameraDistance = Mathf.Clamp(cameraDistance, minDistance, maxDistance);
                targetCameraDistance = Mathf.Clamp(targetCameraDistance, minDistance, maxDistance);
            }
        }
    }

    private void OnValidate()
    {
        centerCameraDistance = centerCameraDistance <= minDistance ? minDistance + Mathf.Epsilon : centerCameraDistance;
    }
}
