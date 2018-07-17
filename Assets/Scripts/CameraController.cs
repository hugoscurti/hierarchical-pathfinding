using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float ZoomSpeed;
    public float MovementSpeed;

    private float defaultCamSize;
    private Vector3 defaultCamPos;


    public void ResetDefaultPosition(int mapWidth, int mapHeight)
    {
        //Adjust camera with respect to the map's size
        defaultCamPos = new Vector3(mapWidth / 2f, -mapHeight / 2f, -mapWidth);

        //Set cam size w.r.t the biggest dimension
        if (mapWidth > mapHeight)
            defaultCamSize = (mapWidth / ((float)Camera.main.pixelWidth / Camera.main.pixelHeight)) / 2f + 3;
        else
            defaultCamSize = mapHeight / 2f + 3;

        Camera.main.transform.position = defaultCamPos;
        Camera.main.orthographicSize = defaultCamSize;
    }


    public void HandleCameraControls()
    {
        HandleZoom();
        HandleCameraMove();
        HandleCameraReset();
    }


    private void HandleCameraReset()
    {
        if (Input.GetButtonDown("Reset"))
        {
            Camera.main.transform.position = defaultCamPos;
            Camera.main.orthographicSize = defaultCamSize;
        }
    }

    private void HandleCameraMove()
    {
        if (Input.GetMouseButton(1))
        {
            Camera.main.transform.position += Vector3.left * Input.GetAxis("Mouse X") * MovementSpeed;
            Camera.main.transform.position += Vector3.down * Input.GetAxis("Mouse Y") * MovementSpeed;
        }
    }

    private void HandleZoom()
    {
        float delta = Input.GetAxis("Mouse ScrollWheel");
        if (delta != 0)
        {
            Camera.main.orthographicSize += ZoomSpeed * -delta;

            //Bound minimum camera size
            if (Camera.main.orthographicSize < 0.1f) Camera.main.orthographicSize = 0.1f;
        }
    }

}
