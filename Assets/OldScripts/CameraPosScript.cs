using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosScript : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float threshold;

    private Vector3 velocity = Vector3.zero;
    private Vector2 otherVel = Vector2.zero;
    public float timeToSmoothDamp = 0.5f;
    public float timeToSmoothDampM = 0.25f;
    public float cameraSizeChangeTo = 7;
    public cameraMode camMode;
    public Transform target;
    private Camera cam;
    Vector3 targetPos;
    GameObject runeViewer;
    bool lookedAtPlayer = false;
    public AnimationCurve screenShakeAnimationCurve;
    Vector3 noScreenShakePos = Vector3.zero;
    Vector2 shakeAmountVector;

    public void Awake()
    {
        //runeViewer = GameObject.Find("RuneViewer");
        cam = GetComponent<Camera>();
        noScreenShakePos = transform.position;
    }

    public enum cameraMode
    {
        mouseAndPlayer,
        justPlayer,
        justTarget

    }

    public IEnumerator shakeTheScreen(float shakeAmount, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            startPos = targetPos;
            elapsedTime += Time.deltaTime;
            float strength = screenShakeAnimationCurve.Evaluate(elapsedTime / duration);
            float strengthASAmount = strength * shakeAmount;
            Vector2 randomCircle = Random.insideUnitCircle;
            shakeAmountVector = new Vector3(randomCircle.x * strengthASAmount, randomCircle.y * strengthASAmount, -10);
            yield return null;
        }
        shakeAmountVector = Vector2.zero;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (cam.orthographicSize != cameraSizeChangeTo)
        {
            Vector2 camSize = new Vector2(cam.orthographicSize, 0);
            camSize = Vector2.SmoothDamp(camSize, new Vector2(cameraSizeChangeTo, 0), ref otherVel, timeToSmoothDamp);
            cam.orthographicSize = camSize.x;
        }
        if (camMode == cameraMode.mouseAndPlayer)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var allowedPosClamped = mousePos - player.position;

            allowedPosClamped = Vector3.ClampMagnitude(allowedPosClamped, threshold);
            targetPos = new Vector3(player.position.x + allowedPosClamped.x, player.position.y + allowedPosClamped.y, -10);
            noScreenShakePos = Vector3.SmoothDamp(noScreenShakePos, targetPos, ref velocity, timeToSmoothDampM);
            transform.position = Vector3.SmoothDamp(noScreenShakePos, targetPos, ref velocity, timeToSmoothDampM);
        }
        else if (camMode == cameraMode.justTarget)
        {
            targetPos = new Vector3(target.position.x, target.position.y, -10);
            noScreenShakePos = Vector3.SmoothDamp(noScreenShakePos, targetPos, ref velocity, timeToSmoothDamp);
            transform.position = Vector3.SmoothDamp(noScreenShakePos, targetPos, ref velocity, timeToSmoothDamp);
        }
        else if (camMode == cameraMode.justPlayer)
        {
            targetPos = new Vector3(player.position.x, player.position.y, -10);
            noScreenShakePos = Vector3.SmoothDamp(noScreenShakePos, targetPos, ref velocity, timeToSmoothDampM);
            transform.position = Vector3.SmoothDamp(noScreenShakePos, targetPos, ref velocity, timeToSmoothDampM);
        }
        transform.position = new Vector3(transform.position.x + shakeAmountVector.x, transform.position.y + shakeAmountVector.y, -10);
    }
}
