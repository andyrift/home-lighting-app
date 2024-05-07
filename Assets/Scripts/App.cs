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
    private float _valueHot = 0f;
    private float _valueCold = 0f;
    private TMP_Text[] _vitals = new TMP_Text[4];

    private TMP_Text _numberText;
    private TMP_Text _outputText;
    private TMP_InputField _inputField;

    private BluetoothManager bluetoothManager;

    private string _hcMAC = "";

    // Is set to execute after default time so everything starts before the app,
    // so the app can init everything after that in custom order
    private void Start() 
    {
        _value = 0f;

        GameObject slider = GameObject.Find("MainSlider");
        slider.GetComponent<Slider>().SetValueWithoutNotify(_value);

        _numberText = GameObject.Find("NumberField").GetComponent<TMP_Text>();
        _outputText = GameObject.Find("OutputField").GetComponent<TMP_Text>();
        UpdateOutput();

        _inputField = GameObject.Find("InputField").GetComponent<TMP_InputField>();

        for (int i = 0; i < _vitals.Length; i++) {
            _vitals[i] = GameObject.Find("Vital" + (i+1).ToString()).GetComponent<TMP_Text>();
        }

        PrintSliderValues();

        bluetoothManager = GameObject.Find("BluetoothManager").GetComponent<BluetoothManager>();
        bluetoothManager.InitBluetooth();

        _vitals[2].text = "Not connected";
        bluetoothManager.ReadDataEvent += (string data) =>
        {
            bluetoothManager.Toast("Recieved: " + data);
        };

        bluetoothManager.ConnectionStatusEvent += (string status) =>
        {
            if (status == "connected")
            {
                _vitals[2].text = "Connected";
            }
            else
            {
                _vitals[2].text = "Could not connect";
            }
        };

        FindHC();
        ConnectToHc();
        SendHot();
        SendCold();
    }

    private void FindHC() 
    {
        string[] devices = bluetoothManager.GetPairedDevices();

        foreach (var device in devices)
        {
            var split = device.Split("+");
            if (split.Length != 2) continue;
            if (split[split.Length - 1] == "HC-06")
            {
                _hcMAC = split[0];
                break;
            }
        }
    }

    private void ConnectToHc()
    {
        if (_hcMAC.Length > 0)
        {
            bluetoothManager.StartConnection(_hcMAC);
        } else {
            bluetoothManager.Toast("Device not found");
            _vitals[2].text = "Could not connect";
        }
    }

    public void HandleSliderChange(float value) 
    {
        _value = value;
        UpdateOutput();
    }

    public void Send()
    {
        bluetoothManager.WriteData(_inputField.text + "\n");
    }

    private void SendHot()
    {
        byte val = (byte)(_valueHot * 255);
        bluetoothManager.WriteData("hv" + val.ToString() + "\n");
    }
    private void SendCold()
    {
        byte val = (byte)(_valueCold * 255);
        bluetoothManager.WriteData("cv" + val.ToString() + "\n");
    }

    public void PrintSliderValues()
    {
        int val = (int)(_valueHot * 255);
        _vitals[0].text = "Hot Value: " + val.ToString();
        val = (int)(_valueCold * 255);
        _vitals[1].text = "Cold Value: " + val.ToString();
    }

    public void HandleHotSliderChange(float value)
    {
        _valueHot = value;
        PrintSliderValues();
        SendHot();
    }

    public void HandleColdSliderChange(float value)
    {
        _valueCold = value;
        PrintSliderValues();
        SendCold();
    }

    private void UpdateOutput()
    {
        int val = (int)(_value * 255);
        _numberText.text = val.ToString();
    }

    public void GetLocations()
    {
        StartCoroutine(GetText());
    }

    public void GetDevices()
    {
        string[] devices = bluetoothManager.GetPairedDevices();

        for (int i = 0; i < devices.Length; i++) {
            devices[i] += "\r\n";
        }

        _outputText.text = string.Concat(devices);
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
        }
    }
}
