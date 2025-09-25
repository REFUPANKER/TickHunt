using UnityEngine;

public class HunterMovement : tMovement
{
    public Transform dartGun, hunterHead;
    public Vector3 camMargin;
    public Transform HeadRigPointer;
    public Vector3 headRigMargin;
    [Range(300, 1000)]
    public float mouseSens;
    public float verCamMax = 40, verCamMin = -40;
    private float fpsRx;
    void Update()
    {
        if (!CheckCanMove()) { return; }

        PerformStandartMovement(false);

        cam.transform.position = hunterHead.position + (hunterHead.forward * camMargin.z) + (hunterHead.up * camMargin.y);
        HeadRigPointer.transform.position = hunterHead.position + (cam.transform.forward * headRigMargin.z) + (cam.transform.up * headRigMargin.y);
        dartGun.rotation = Quaternion.LookRotation(cam.transform.forward);

        float mouseX = Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSens * Time.deltaTime;
        fpsRx -= mouseY;
        fpsRx = Mathf.Clamp(fpsRx, verCamMin, verCamMax);
        cam.transform.localRotation = Quaternion.Euler(fpsRx, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            animator.SetLayerWeight(1, 1);
        }
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            animator.SetLayerWeight(1, 0);
        }
    }
}
