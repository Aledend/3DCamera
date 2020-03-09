using UnityEngine;

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

    

    

    private float targetCameraDistance = 0f;

    private Vector3 targetPosCamBoomStart = Vector3.zero;
    private Vector3 originMoveTo = Vector3.zero;
    private float snapDistance = 0.005f;
    private float moveToDistance = 0f;
    private float lerpSpeed = 0f;


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
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        
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
            }
            if(Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
            {
                cameraBoomStart.transform.rotation *= Quaternion.AngleAxis(-Input.GetAxis("Mouse Y"), Vector3.right);
                transform.rotation *= Quaternion.AngleAxis(Input.GetAxis("Mouse X"), Vector3.up);
            }



            //moveToDistance = (targetPosCamBoomStart - cameraBoomStart.transform.localPosition).magnitude;

            //if (moveToDistance > snapDistance)
            //{
            //    cameraBoomStart.transform.localPosition += (targetPosCamBoomStart - cameraBoomStart.transform.localPosition) * lerpSpeed;
            //}
            //else
            //{
            //    cameraBoomStart.transform.localPosition = targetPosCamBoomStart;
            //}
            Vector3 currentVelocity = Vector3.zero;
            cameraBoomStart.transform.localPosition = Vector3.SmoothDamp(cameraBoomStart.transform.localPosition, targetPosCamBoomStart, ref currentVelocity, lerpSpeed); ;

            //targetPosCamBoomStart = new Vector3(
            //    cameraDistance < maxDistance ? Mathf.Pow((1 - (cameraDistance - minDistance) / (maxDistance - minDistance)), 6) * -originMoveTo.x : 0,
            //    targetPosCamBoomStart.y,
            //    targetPosCamBoomStart.z
            //    );

            //targetPosCamBoomStart = new Vector3(
            //    cameraDistance < Mathf.PI + 1 ? Mathf.Pow(Mathf.Cos(0.5f * cameraDistance - 0.5f), 3) * -originMoveTo.x : 0,
            //    targetPosCamBoomStart.y,
            //    targetPosCamBoomStart.z
            //    );

            targetPosCamBoomStart = new Vector3(
                cameraDistance < centerCameraDistance ? Mathf.Pow(Mathf.Cos((Mathf.PI/((centerCameraDistance-1)*2)) * cameraDistance - (Mathf.PI/((centerCameraDistance-1)*2))), centerCameraArcFactor) * -originMoveTo.x : 0,
                targetPosCamBoomStart.y,
                targetPosCamBoomStart.z
                );





            if (objectToFollow)
            {
                transform.position = new Vector3(
                    Mathf.Lerp(transform.position.x, objectToFollow.transform.position.x, lerpSpeed),
                    Mathf.Lerp(transform.position.y, objectToFollow.transform.position.y, lerpSpeed * 5f),
                    Mathf.Lerp(transform.position.z, objectToFollow.transform.position.z, lerpSpeed)
                    );

                transform.position = Vector3.Lerp(transform.position, objectToFollow.transform.position, lerpSpeed);
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
        centerCameraDistance = centerCameraDistance <= minDistance ? minDistance + 0.0001f : centerCameraDistance;
    }
}
