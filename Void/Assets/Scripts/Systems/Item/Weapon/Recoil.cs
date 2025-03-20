using UnityEngine;

public class Recoil : MonoBehaviour
{
    public float recoilAmount = 2f; // Vertical recoil amount
    public float recoilSpeed = 10f; // Speed of recoil
    public float returnSpeed = 20f; // Speed of returning to rest
    public float cameraRecoilMultiplier = 0.5f; // How much the camera moves compared to the gun

    private Vector3 gunTargetRotation;
    private Vector3 gunCurrentRotation;
    private Vector3 cameraTargetRotation;
    private Vector3 cameraCurrentRotation;

    void Update()
    {
        // Smoothly apply recoil to the gun
        gunTargetRotation = Vector3.Lerp(gunTargetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        gunCurrentRotation = Vector3.Slerp(gunCurrentRotation, gunTargetRotation, recoilSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(gunCurrentRotation);

        // Smoothly apply recoil to the camera
        cameraTargetRotation = Vector3.Lerp(cameraTargetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        cameraCurrentRotation = Vector3.Slerp(cameraCurrentRotation, cameraTargetRotation, recoilSpeed * Time.deltaTime);
        Camera.main.transform.localRotation = Quaternion.Euler(cameraCurrentRotation);
    }

    public void ApplyRecoil()
    {
        // Apply recoil to the gun
        gunTargetRotation += new Vector3(-recoilAmount, Random.Range(-recoilAmount, recoilAmount), 0);

        // Apply a smaller recoil to the camera
        cameraTargetRotation += new Vector3(-recoilAmount * cameraRecoilMultiplier, 0, 0);
    }
}
