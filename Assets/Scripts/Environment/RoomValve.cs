using System.Collections;
using UnityEngine;


public class RoomValve : MonoBehaviour , IInteractable , ISaveable
{

    [Header("Valve Settings")]

    public float RotationSpeed = 50f;

    private Transform level;
    private GameObject room;
    private bool isRotating = false;
    


    private void Start()
    {
        level = LevelManager.Instance.LevelRoot;
        room = transform.parent.gameObject;
    }

    private void ActivateRoom()
    {
        if (isRotating) return;

        
        StartCoroutine(RotateSequence());

    }

    private IEnumerator RotateSequence()
    {
        isRotating = true;


        float elapsedTime = 0f;
        float Duration = 90f / RotationSpeed;
        Quaternion startRot = level.transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, 0, 90);

        while (elapsedTime < Duration)
        {
            level.transform.rotation = Quaternion.Slerp(startRot, endRot, elapsedTime / Duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        level.rotation = endRot;

        isRotating = false;

    }

    public void Interact()
    {
        ActivateRoom();
    }

    public object CaptureState() => LevelManager.Instance.LevelRoot.rotation;

    public void RestoreState(object state)
    {
        LevelManager.Instance.LevelRoot.rotation = (Quaternion)state;
    }
}
