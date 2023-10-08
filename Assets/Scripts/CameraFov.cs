using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script was downloaded from a tutorial from CodeMonkey: https://www.youtube.com/watch?v=twMkGTqyZvI

public class CameraFov : MonoBehaviour {

    private Camera playerCamera;
    private float targetFov;
    private float fov;

    private void Awake() {
        playerCamera = GetComponent<Camera>();
        targetFov = playerCamera.fieldOfView;
        fov = targetFov;
    }

    private void Update() {
        float fovSpeed = 4f;
        fov = Mathf.Lerp(fov, targetFov, Time.deltaTime * fovSpeed);
        playerCamera.fieldOfView = fov;
    }

    public void SetCameraFov(float targetFov) {
        this.targetFov = targetFov;
    }
}
