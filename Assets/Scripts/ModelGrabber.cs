using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ModelGrabber : MonoBehaviour
{
    [Header("Controller Settings")] [SerializeField]
    private XRNode rightControllerNode;

    [SerializeField] private XRNode leftControllerNode;
    [SerializeField] private LayerMask grabbableLayer;
    [SerializeField] private GameObject rightControllerObject;

    [Header("Player Movement Settings")] [SerializeField]
    private DynamicMoveProvider moveProvider;

    [SerializeField] private SnapTurnProvider snapProvider;

    [Header("Grab Settings")] [SerializeField]
    private float rotationSpeed = 100f;

    [SerializeField] private float scaleSpeed = 0.5f;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float grabDistance = 10f;


    private InputDevice rightController;
    private InputDevice leftController;

    private GameObject grabbedObject;
    private Transform originalParent;
    private bool isGrabbing;


    private float playerMoveSpeed;


    private Transform rightControllerTransform;

    private void Awake()
    {
        if (rightControllerObject != null)
            rightControllerTransform = rightControllerObject.transform;
        else
            Debug.LogError("RightControllerObject is not assigned!");
    }

    private void Start()
    {
        rightController = InputDevices.GetDeviceAtXRNode(rightControllerNode);
        leftController = InputDevices.GetDeviceAtXRNode(leftControllerNode);
        playerMoveSpeed = moveProvider.moveSpeed;
    }

    private void Update()
    {
        RefreshInputDevices();


        if (rightController.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed))
        {
            if (triggerPressed && !isGrabbing)
            {
                TryGrabObject();
                TogglePlayerMovement(false);
            }
            else if (!triggerPressed && isGrabbing)
            {
                ReleaseObject();
                TogglePlayerMovement(true);
            }
        }

        if (isGrabbing && grabbedObject != null)
        {
            ProcessGrabbedObjectManipulation();
        }
    }

    private void RefreshInputDevices()
    {
        if (!rightController.isValid)
            rightController = InputDevices.GetDeviceAtXRNode(rightControllerNode);

        if (!leftController.isValid)
            leftController = InputDevices.GetDeviceAtXRNode(leftControllerNode);
    }

    private void TryGrabObject()
    {
        if (rightControllerTransform == null)
            return;

        if (Physics.Raycast(rightControllerTransform.position, rightControllerTransform.forward, out RaycastHit hit,
                grabDistance, grabbableLayer))
        {
            grabbedObject = hit.collider.gameObject;
            originalParent = grabbedObject.transform.parent;
            grabbedObject.transform.SetParent(rightControllerTransform);
            isGrabbing = true;
        }
    }

    private void ReleaseObject()
    {
        if (grabbedObject != null)
        {
            grabbedObject.transform.SetParent(originalParent);
            grabbedObject = null;
            isGrabbing = false;
        }
    }

    private void ProcessGrabbedObjectManipulation()
    {
        RotateGrabbedObject();
        ScaleGrabbedObject();
        MoveGrabbedObject();
    }

    private void RotateGrabbedObject()
    {
        if (rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rightStickInput))
        {
            float rotationAmount = rightStickInput.x * rotationSpeed * Time.deltaTime;
            grabbedObject.transform.Rotate(Vector3.up, rotationAmount, Space.World);
        }
    }

    private void ScaleGrabbedObject()
    {
        if (leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 leftStickInput))
        {
            float scaleChange = leftStickInput.y * scaleSpeed * Time.deltaTime;

            Vector3 newScale = grabbedObject.transform.localScale + Vector3.one * scaleChange;
            grabbedObject.transform.localScale = Vector3.Max(newScale, Vector3.one * 0.1f);
        }
    }


    private void MoveGrabbedObject()
    {
        if (rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool isAPressed) && isAPressed)
        {
            grabbedObject.transform.position += rightControllerTransform.forward * moveSpeed * Time.deltaTime;
        }

        if (rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool isBPressed) && isBPressed)
        {
            grabbedObject.transform.position -= rightControllerTransform.forward * moveSpeed * Time.deltaTime;
        }
    }

    private void TogglePlayerMovement(bool isMovementEnabled)
    {
        moveProvider.moveSpeed = isMovementEnabled ? playerMoveSpeed : 0;
        snapProvider.enableTurnLeftRight = isMovementEnabled;
        snapProvider.enableTurnAround = isMovementEnabled;
    }
}