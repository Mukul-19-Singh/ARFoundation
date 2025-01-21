using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using System.Collections.Generic;

public class ARManager : MonoBehaviour
{
    public GameObject cubePrefab;
    private ARRaycastManager arRaycastManager;
    private Camera arCamera;

    void Start()
    {
        arRaycastManager = FindFirstObjectByType<ARRaycastManager>();
        arCamera = Camera.main;
    }


    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPosition = Input.GetTouch(0).position;
            List<ARRaycastHit> hits = new List<ARRaycastHit>();

            if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.Planes))
            {
                Pose hitPose = hits[0].pose;

                GameObject newCube = Instantiate(cubePrefab, hitPose.position, Quaternion.identity);

                newCube.GetComponent<Renderer>().material.color = Random.ColorHSV();

                StartCoroutine(CaptureImageAfterFrames(10, newCube));
            }
        }
    }

    IEnumerator CaptureImageAfterFrames(int frames, GameObject cube)
    {
        for (int i = 0; i < frames; i++)
        {
            yield return null;
        }

        CaptureImage(cube.transform.position);
    }

    void CaptureImage(Vector3 cubePosition)
    {
        var cameraManager = arCamera.GetComponent<ARCameraManager>();
        if (cameraManager == null || !cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            Debug.LogError("Failed to capture AR image.");
            return;
        }

        var texture = ConvertImageToTexture2D(image);
        image.Dispose();

        Debug.Log("Image Captured!");
   
        byte[] imageBytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.persistentDataPath + "/CapturedImage.png", imageBytes);
    }

    Texture2D ConvertImageToTexture2D(XRCpuImage image)
    {
        XRCpuImage.ConversionParams conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(image.width, image.height),
            outputFormat = TextureFormat.RGBA32,
            transformation = XRCpuImage.Transformation.None
        };

        Texture2D texture = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
        var rawTextureData = texture.GetRawTextureData<byte>();
        image.Convert(conversionParams, rawTextureData);
        texture.Apply();
        return texture;
    }
}
