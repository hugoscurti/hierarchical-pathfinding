using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour {

    //Store callback to call it when button is clicked
    Action callback;

    public Text text;

	// Use this for initialization
	void Start () {
        gameObject.SetActive(false);
	}

    public void Show(string text, Action callBack)
    {
        this.callback = callBack;
        this.text.text = text;

        gameObject.SetActive(true);
    }

    public void OnHide()
    {
        gameObject.SetActive(false);
        if (callback != null)
            callback();
    }
}
