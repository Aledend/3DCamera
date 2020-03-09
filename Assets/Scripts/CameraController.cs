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
    float cameraDistance = 0f, maxDistance = 5f, minDistance = 1f, centerCameraDistance = 1f, centerCameraArcFactor = 3f;

    [SerializeField]
    List<string> cameraBoomIgnoreTags;

    

    private float targetCameraDistance = 0f;
    private float targetCameraDistanceCached = 0f;

    private Vector3 targetPosCamBoomStart = Vector3.zero;
    private Vector3 originMoveTo = Vector3.zero;
    private float snapDistance = 0.005f;
    private float moveToDistance = 0f;
    private float lerpSpeed = 0f;
    private bool cameraDistanceOverridden = false;
    private float cameraBoomHitDelta = 1f;


    [SerializeField]
    Vector3 rightHandSidePos = Vector3.zero, leftHandSidePos = Vector3.zero;

    private void Awake()
    {
        if (!Application.isPlaying)
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
            }
        }

        if (targetPosCamBoomStart == Vector3.zero)
        {
            targetPosCamBoomStart = rightHandSidePos;
            originMoveTo = leftHandSidePos;
            cameraBoomStart.transform.localPosition = rightHandSidePos;
        }
        targetCameraDistance = cameraDistance;

        Debug.Log(playerCamera.orthographicSize);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        Vector3 startpoint = cameraBoomStart.transform.position;
        Vector3 endpoint = cameraBoomEnd.transform.position;
        
        //RaycastHit[] hits = Physics.RaycastAll(startpoint, startpoint + (endpoint - startpoint), (endpoint - startpoint).magnitude);
        //Debug.Log(Vector3.Dot((endpoint - startpoint).normalized, -cameraBoomStart.transform.forward));
        Debug.DrawLine(startpoint, startpoint + (endpoint - startpoint), Color.red);
        Debug.DrawLine(endpoint, endpoint + Mathf.Sin(Mathf.Acos(Vector3.Dot((endpoint - startpoint).normalized, -cameraBoomStart.transform.forward))) * (endpoint - startpoint).magnitude * -cameraBoomStart.transform.up, Color.blue);
        Debug.DrawLine(startpoint, startpoint + (Vector3.Dot((endpoint - startpoint).normalized, -cameraBoomStart.transform.forward)) * (endpoint - startpoint).magnitude * -cameraBoomStart.transform.forward, Color.blue);
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isPlaying)
        {
            lerpSpeed = Time.deltaTime * 5f;

            if (Input.GetKeyDown(KeyCode.E))
            {
                targetPosCamBoomStart = rightHandSidePos;
                originMoveTo = leftHandSidePos;
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                targetPosCamBoomStart = leftHandSidePos;
                originMoveTo = rightHandSidePos;
            }
            if(Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                targetCameraDistance += -(Input.GetAxis("Mouse ScrollWheel") * Mathf.Abs(Input.GetAxis("Mouse ScrollWheel"))) * 80f;
                targetCameraDistance = Mathf.Clamp(targetCameraDistance, minDistance, maxDistance);
                cameraDistanceOverridden = false;
            }
            if(Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
            {
                cameraBoomStart.transform.rotation *= Quaternion.AngleAxis(-Input.GetAxis("Mouse Y"), Vector3.right);
                transform.rotation *= Quaternion.AngleAxis(Input.GetAxis("Mouse X"), Vector3.up);
            }

            //RaycastHit[] boomStartHits = Physics.SphereCastAll(transform.position + cameraBoomStart.transform.position, 0.1f, cameraBoomStart.transform.right, 0.1f);
  
            //if(boomStartHits.Length > 0)
            //{
            //    Vector3 normal = Vector3.zero;
            //    foreach (RaycastHit hit in boomStartHits)
            //    {
                    
            //        Debug.Log(hit.transform.name);
            //        cameraBoomStart.transform.position = hit.point + hit.normal * 0.1f;
            //        break;
            //        //normal += hit.normal;
            //    }
            //    //normal = normal.normalized;
            //    //targetPosCamBoomStart = 
            //} 
            ////if (Physics.SphereCast(targetPosCamBoomStart, 2f, cameraBoomStart.transform.right, out boomStartHit, 2f))
            ////{
            ////    targetPosCamBoomStart = boomStartHit.point + boomStartHit.normal * 2.1f;
            ////}
            //else
            //{
                
            //}


            targetPosCamBoomStart = new Vector3(
                cameraDistance < centerCameraDistance ? Mathf.Pow(Mathf.Cos((Mathf.PI / ((centerCameraDistance - 1) * 2)) * cameraDistance - (Mathf.PI / ((centerCameraDistance - 1) * 2))), centerCameraArcFactor) * -originMoveTo.x : 0,
                targetPosCamBoomStart.y,
                targetPosCamBoomStart.z
                );

            Vector3 currentVelocity = Vector3.zero;
            cameraBoomStart.transform.localPosition = Vector3.SmoothDamp(cameraBoomStart.transform.localPosition, targetPosCamBoomStart, ref currentVelocity, lerpSpeed);



            if (objectToFollow)
            {
                transform.position = new Vector3(
                    Mathf.Lerp(transform.position.x, objectToFollow.transform.position.x, lerpSpeed),
                    Mathf.Lerp(transform.position.y, objectToFollow.transform.position.y, lerpSpeed * 5f),
                    Mathf.Lerp(transform.position.z, objectToFollow.transform.position.z, lerpSpeed)
                    );

                transform.position = Vector3.Lerp(transform.position, objectToFollow.transform.position, lerpSpeed);
            }

            Vector3 startpoint = cameraBoomStart.transform.position;
            Vector3 endpoint = cameraBoomEnd.transform.position;

            //RaycastHit[] hits = Physics.RaycastAll(startpoint, (endpoint - startpoint), (endpoint - startpoint).magnitude + 1);

            RaycastHit[] hits = Physics.SphereCastAll(startpoint, 0.01f, (endpoint - startpoint).normalized, (endpoint - startpoint).magnitude + 1);


            if (hits.Length > 0)
            {
                RaycastHit closestHit = new RaycastHit();
                float closestValue = Mathf.Infinity;
                bool changePos = false;
                foreach (RaycastHit hit in hits)
                {
                    if (!cameraBoomIgnoreTags.Contains(hit.transform.tag))
                    {
                        Debug.Log(hit.transform.name);
                        float dist = ((hit.point + hit.normal * 0.01f) - startpoint).magnitude;
                        if(dist < closestValue)
                        {
                            closestHit = hit;
                            closestValue = dist;
                            changePos = true;
                        } 
                    }
                }
                if (changePos)
                {
                    if (!cameraDistanceOverridden)
                    {
                        cameraDistanceOverridden = true;
                        targetCameraDistanceCached = cameraDistance;
                    }
                    
                    float dist = ((closestHit.point + closestHit.normal * 0.01f) - startpoint).magnitude;
                    cameraDistance = targetCameraDistance = (Vector3.Dot(((closestHit.point + closestHit.normal * 0.01f) - startpoint).normalized, -cameraBoomStart.transform.forward)) * dist;
                    cameraDistance = Mathf.Clamp(cameraDistance, Mathf.Epsilon, maxDistance);
                    targetCameraDistance = Mathf.Clamp(targetCameraDistance, Mathf.Epsilon, maxDistance);
                }
                else
                {
                    if (cameraDistanceOverridden)
                    {
                        cameraDistanceOverridden = false;
                        cameraDistance = targetCameraDistance = targetCameraDistanceCached > targetCameraDistance ? targetCameraDistanceCached : cameraDistance;
                    }
                }

            }
            else
            {
                if (cameraDistanceOverridden)
                {
                    cameraDistanceOverridden = false;
                    targetCameraDistance = targetCameraDistanceCached > targetCameraDistance ? targetCameraDistanceCached : cameraDistance;
                }
                cameraDistance = Mathf.Clamp(cameraDistance, minDistance, maxDistance);
                targetCameraDistance = Mathf.Clamp(targetCameraDistance, minDistance, maxDistance);
            }

            

            cameraDistance = Mathf.Lerp(cameraDistance, targetCameraDistance, lerpSpeed);

            cameraBoomEnd.transform.localPosition = new Vector3(
                0,
                cameraDistance < 3 ? (Mathf.Sin(cameraDistance * 0.5f) * Mathf.Pow(cameraDistance, 3)) / 111.5f : cameraDistance * 0.25f - 0.5085f,
                -cameraDistance
                );

            
        }
    }

    private void OnValidate()
    {
        centerCameraDistance = centerCameraDistance <= minDistance ? minDistance + Mathf.Epsilon : centerCameraDistance;
    }

    
}
