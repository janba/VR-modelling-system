using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTest : MonoBehaviour
{

	public GameObject targetObject;
	public Collider surface;

	public Vector3 initialClick;
	public Quaternion initialRotation;
	
	public GameObject fromGO;
	public GameObject toGO;
	
	// Update is called once per frame
	void Update ()
	{
		Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit = new RaycastHit();
		Debug.DrawRay(r.origin, r.direction, Color.magenta);
		bool didHit = Physics.Raycast(r, out hit);
		if (Input.GetMouseButtonDown(0))
		{
			if (didHit)
			{
				initialClick = hit.point;
				fromGO.transform.position = initialClick;
				initialClick.y = 0;
				initialRotation = targetObject.transform.rotation;

			}
		} else if (Input.GetMouseButton(0))
		{
			if (didHit)
			{
				Vector3 point = hit.point;
				toGO.transform.position = point;
				point.y = 0;
				Debug.DrawRay(Vector3.zero, initialClick, Color.green);
				Debug.DrawRay(Vector3.zero, point, Color.green);
				
				targetObject.transform.rotation = initialRotation *
				                                  (Quaternion.Inverse(Quaternion.LookRotation(initialClick)) *
				                                   Quaternion.LookRotation(point));
			}
		}
	}
}
