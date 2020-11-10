using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;


/*
 * 
 *  Move the camera
 * 
*/
public class CameraController : MonoBehaviour
{
    private float speed;
    private float sensitivity;
    private Camera cam;
    private Vector3 anchorPoint;
    private Vector3 anchorRot;
    private bool qwerty;

    void Awake()
    {
        speed = 20f;
        sensitivity = 0.2f;
        qwerty = true;
        cam = GetComponent<Camera>();
    }
    
    void LateUpdate()
    {
        // Translation
        Vector3 move = Vector3.zero;
        if (qwerty)
        {
            if (Input.GetKey(KeyCode.W))
                move += Vector3.forward * speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.A))
                move -= Vector3.right * speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.Q))
                move -= Vector3.up * speed * Time.deltaTime;
        }
        else
        {
            if (Input.GetKey(KeyCode.Z))
                move += Vector3.forward * speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.Q))
                move -= Vector3.right * speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.A))
                move -= Vector3.up * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
            move -= Vector3.forward * speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.D))
            move += Vector3.right * speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.E))
            move += Vector3.up * speed * Time.deltaTime;
        transform.Translate(move);

        // Rotation
        if (Input.GetMouseButtonDown(1))
        {
            anchorPoint = new Vector3(Input.mousePosition.y, -Input.mousePosition.x);
            anchorRot = transform.eulerAngles;
        }
        if (Input.GetMouseButton(1))
        {
            Vector3 rot = anchorRot;
            Vector3 dif = anchorPoint - new Vector3(Input.mousePosition.y, -Input.mousePosition.x);
            rot += dif * sensitivity;
            transform.eulerAngles = rot;
        }
    }

    public void UpdateKeyboard(int val)
    {
        qwerty = val == 0;
    }
    public bool IsQwerty()
    {
        return qwerty;
    }

    public void SetSpeed(string newSpeed)
    {
        speed = float.Parse(newSpeed, CultureInfo.InvariantCulture.NumberFormat);
    }
    public float GetSpeed()
    {
        return speed;
    }

    public void SetSensitivity(string newSensitivity)
    {
        sensitivity = float.Parse(newSensitivity, CultureInfo.InvariantCulture.NumberFormat);
    }
    public float GetSensitivity()
    {
        return sensitivity;
    }
}
