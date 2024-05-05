using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class App : MonoBehaviour
{
    private float _value;
    private TMP_Text _numberText;
    private TMP_Text _outputText;

    private void Start() 
    {
        _value = 0f;

        GameObject slider = GameObject.Find("MainSlider");
        slider.GetComponent<UnityEngine.UI.Slider>().SetValueWithoutNotify(_value);

        _numberText = GameObject.Find("NumberField").GetComponent<TMP_Text>();
        _outputText = GameObject.Find("OutputField").GetComponent<TMP_Text>();
        UpdateOutput();

        StartCoroutine(GetText());
    }

    public void HandleSliderChange(float value) 
    {
        _value = value;
        UpdateOutput();
    }

    private void UpdateOutput()
    {
        int val = (int)(_value * 255);
        _numberText.text = val.ToString();
    }

    IEnumerator GetText()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://worldtimeapi.org/api/timezone.txt");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            string str = www.downloadHandler.text;
            _outputText.text = str;
            Debug.Log(str);
        }
    }
}
