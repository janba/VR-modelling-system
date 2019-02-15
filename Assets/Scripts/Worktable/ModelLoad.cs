using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModelLoad : MonoBehaviour {

    private bool isInteracting;

    //[SerializeField]
    private GameObject UIPanel;

    private bool fromFront;

    private bool canUse;

	// Use this for initialization
	void Start () {
        var component = GetComponent<MeshRenderer>();
        Material material = component.material;
        material.color = Color.grey;
        canUse = GetComponent<MeshRenderer>().isVisible;
        UIPanel = GameObject.FindGameObjectWithTag("UIScreenOwner");
        //Debug.Log(UIPanel.name);
    }

   
	
	// Update is called once per frame
	void Update () {
        canUse = GetComponent<MeshRenderer>().isVisible;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (canUse)
        {
            if (other.gameObject.name == "index-finger")
            {
                //Check if the trigger is entering the button from above
                if (other.transform.parent.name.StartsWith("hand") && !fromFront)
                {
                    //Debug.Log(other.transform.position.x);
                    //Debug.Log(transform.position.x);

                    //Debug.Log(other.transform.position.y);
                    //Debug.Log(transform.position.y);

                    //Debug.Log(other.transform.position.z);
                    //Debug.Log(transform.position);
                    if (other.transform.position.x > transform.position.x)
                    {
                        fromFront = true;
                        if (!isInteracting)
                        {
                            StartCoroutine(InitiateInteraction());
                        }

                    }
                    else
                    {
                        fromFront = false;
                    }
                }
            }
        
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.parent.name.StartsWith("hand"))
        {
            fromFront = false;
        }
    }

    IEnumerator InitiateInteraction()
    {
        isInteracting = true;
        var component = GetComponent<MeshRenderer>();
        Material material = component.material;
        material.color = Color.blue;

        ScreenController _screenController = UIPanel.GetComponent<ScreenController>();
        _screenController.LoadMesh(transform.parent.gameObject.GetComponentInChildren<Text>().text);
        //_screenController.OpenImportUI((false);
        //_screenController.ExportMesh("TestExport");

        //_screenController.disableScreen();

        byte[] noize = { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 };
        OVRHaptics.Channels[1].Preempt(new OVRHapticsClip(noize, 10));
        OVRHaptics.Channels[0].Preempt(new OVRHapticsClip(noize, 10));
        yield return new WaitForSeconds(0.3f);

        material.color = Color.grey;
        isInteracting = false;

    }
}
