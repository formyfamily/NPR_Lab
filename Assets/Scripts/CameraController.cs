
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GameObject gameObject;
    float x1;
    float x2;
    float x3;
    float x4;

    void Start()
    {
        gameObject = GameObject.Find("Main Camera");

        // Make the rigid body not change rotation  
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
    }

    // Update is called once per frame  
    void Update()
    {
        //空格键抬升高度  
        if (Input.GetKey(KeyCode.Space))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
        }

        //w键前进  
        if (Input.GetKey(KeyCode.A))
        {
            this.gameObject.transform.Translate(new Vector3(-3 * Time.deltaTime, 0, 0));
        }
        //s键后退  
        if (Input.GetKey(KeyCode.D))
        {
            this.gameObject.transform.Translate(new Vector3(3 * Time.deltaTime, 0, 0));
        }
        //a键后退  
        if (Input.GetKey(KeyCode.W))
        {
            this.gameObject.transform.Translate(new Vector3(0, 3 * Time.deltaTime, 0));
        }
        //d键后退  
        if (Input.GetKey(KeyCode.S))
        {
            this.gameObject.transform.Translate(new Vector3(0, -3 * Time.deltaTime, 0));
        }
        //a键后退  
        if (Input.GetKey(KeyCode.Q))
        {
            this.gameObject.transform.Translate(new Vector3(0, 0, -5 * Time.deltaTime));
        }
        //d键后退  
        if (Input.GetKey(KeyCode.E))
        {
            this.gameObject.transform.Translate(new Vector3(0, 0, 5 * Time.deltaTime));
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (axes == RotationAxes.MouseXAndY)
            {
                float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;

                rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
                rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

                transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
            }
            else if (axes == RotationAxes.MouseX)
            {
                transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
            }
            else
            {
                rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
                rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

                transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
            }
        }

    }

    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 3F;
    public float sensitivityY = 3F;

    public float minimumX = -360F;
    public float maximumX = 360F;

    public float minimumY = -90F;
    public float maximumY = 90F;

    float rotationY = 0F;
}