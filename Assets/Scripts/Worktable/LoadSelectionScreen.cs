using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class LoadSelectionScreen : MonoBehaviour {

    public GameObject modelPrefab;

    private float topZ = 0.0767f;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PopulateScreen()
    {
        DirectoryInfo dir = new DirectoryInfo(string.Format("{0}/Models/Saved", Application.dataPath));
        FileInfo[] info = dir.GetFiles("*.obj");
        //string text = "";
        int i = 0;
        foreach (FileInfo f in info)
        {

            var go = Instantiate(modelPrefab);
            
            string[] temp = f.ToString().Split('\\');
            go.GetComponentInChildren<Text>().text = temp[temp.Length - 1];
            go.transform.SetParent(this.transform);
            
            go.transform.localPosition = new Vector3(0f, topZ-i*0.03f, 0f);
            go.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

            i++;
            //text += String.Format(">>  {0}\n", temp[temp.Length - 1]);
            //Debug.Log(temp[temp.Length - 1]);
            //break;
        }
        //Text ImportText = GetComponentInChildren<Text>();
        //ImportText.text = text;
        //ImportText.fontSize = 50;
    }
}
