using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSpaceMovement : MonoBehaviour
{

	public float movementSpeed = 0.1f;
    public float rotationSpeed = 0.5f;

	public string key = "turntable_";

	public bool shiftPressed = false;
    
    private Vector3 cameraRotation;

	void Start()
	{
		Vector3 pos = Vector3.zero;
		pos.x = PlayerPrefs.GetFloat(key+"posx",0);
		pos.y = PlayerPrefs.GetFloat(key+"posy",0);
		pos.z = PlayerPrefs.GetFloat(key+"posz",0);
		transform.position = pos;
        Vector3 rot = Vector3.zero;
        rot.x = PlayerPrefs.GetFloat(key + "rotx", 0);
        rot.y = PlayerPrefs.GetFloat(key + "roty", 0);
        rot.z = PlayerPrefs.GetFloat(key + "rotz", 0);
        cameraRotation = rot;
	}
	
	private void OnDestroy()
	{
		var newPos = transform.position;
        var newRot = transform.rotation.eulerAngles;
		PlayerPrefs.SetFloat(key+"posx",newPos.x);
		PlayerPrefs.SetFloat(key+"posy",newPos.y);
		PlayerPrefs.SetFloat(key+"posz",newPos.z);
        PlayerPrefs.SetFloat(key + "rotx", newRot.x);
        PlayerPrefs.SetFloat(key + "roty", newRot.y);
        PlayerPrefs.SetFloat(key + "rotz", newRot.z);
        PlayerPrefs.Save();
	}

	// Update is called once per frame
	void Update ()
	{
		bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		if (shift != shiftPressed) return;
		Vector3 delta = Vector3.zero;
		if (Input.GetKey(KeyCode.W))
		{
			delta.z += 1;
		}
		if (Input.GetKey(KeyCode.S))
		{
			delta.z -= 1;
		}
		if (Input.GetKey(KeyCode.A))
		{
			delta.x -= 1;
		}
		if (Input.GetKey(KeyCode.D))
		{
			delta.x += 1;
		}
		if (Input.GetKey(KeyCode.Q))
		{
			delta.y -= 1;
		}
		if (Input.GetKey(KeyCode.E))
		{
			delta.y += 1;
		}
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            cameraRotation.y -= rotationSpeed;
            //transform.Rotate(new Vector3(0f, -rotationSpeed, 0f), Space.World);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            cameraRotation.y += rotationSpeed;
            //transform.Rotate(new Vector3(0f, rotationSpeed, 0f), Space.World);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            cameraRotation.x += rotationSpeed;
            //transform.Rotate(new Vector3(rotationSpeed, 0f, 0f), Space.World);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            cameraRotation.x -= rotationSpeed;
            //transform.Rotate(new Vector3(-rotationSpeed, 0f, 0f), Space.World);
        }

        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        transform.Rotate(new Vector3(cameraRotation.x, 0f, 0f), relativeTo: Space.World);
        transform.Rotate(new Vector3(0f, cameraRotation.y, 0f), relativeTo: Space.World);
        

        delta = delta * Time.deltaTime * movementSpeed;
        transform.Translate(delta, relativeTo: Space.Self);
		//if (delta.sqrMagnitude > 0)
		//{
		//	var newPos = transform.position + delta;
		//	transform.position = newPos;
		//}
		if (Input.GetKey(KeyCode.T))
		{
			// reset
			transform.position = Vector3.zero;
		}
	}
}
