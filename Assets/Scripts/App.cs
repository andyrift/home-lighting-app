using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class App : MonoBehaviour
{
    private int _valueHot = 0;
    private int _valueCold = 0;
    private TMP_Text[] _vitals = new TMP_Text[4];

    private TMP_Text _outputText;
    private TMP_InputField _inputField;

    private BluetoothManager bluetoothManager;

    private string _hcMAC = "";

    private bool _connecting = false;

    private bool _waitingReconnect = false;
    private float _reconnectTimer = 0f;
    private float _timeToWait = 3f;

    // Is set to execute after default time so everything starts before the app,
    // so the app can init everything after that in custom order
    private void Start() 
    {
        _outputText = GameObject.Find("OutputField").GetComponent<TMP_Text>();

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
            if (data.StartsWith("hcv")) 
            {
                string[] vals = data.Substring(3, data.Length - 3).Split(',');
                _valueHot = int.Parse(vals[0]);
                _valueCold = int.Parse(vals[1]);
                GameObject.Find("HotSlider").GetComponent<Slider>().SetValueWithoutNotify(_valueHot);
                GameObject.Find("ColdSlider").GetComponent<Slider>().SetValueWithoutNotify(_valueCold);
            } 
            else if (data.StartsWith("tl"))
            {
                string location = data.Substring(2, data.Length - 2);
                GameObject.Find("LocationInput").GetComponent<TMP_InputField>().text = location;
            }
            else if (data.StartsWith("wifi"))
            {
                string[] wifi = data.Substring(4, data.Length - 4).Split('+');

                GameObject.Find("ssidInput").GetComponent<TMP_InputField>().text = wifi[0];
                GameObject.Find("passwordInput").GetComponent<TMP_InputField>().text = wifi[1];
            }
            else bluetoothManager.Toast("Recieved: " + data);
        };

        bluetoothManager.ConnectionStatusEvent += (string status) =>
        {
            _vitals[3].text = status;
            if (status == "connected")
            {
                _connecting = false;
                _vitals[2].text = "Connected";
                Refresh();
                // bluetoothManager.Toast("Successfully connected!");
            }
            else if (status == "connecting")
            {
                _vitals[2].text = "Connecting...";
            }
            else
            {
                _connecting = false;
                _vitals[2].text = "Could not connect";
                _waitingReconnect = true;
            }
        };

        TryConnect(true);
    }

    public void Refresh()
    {
        bluetoothManager.WriteData("gv\n");
        bluetoothManager.WriteData("gtl\n");
        bluetoothManager.WriteData("gwifi\n");
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

    private void ConnectToHc(bool toast = false)
    {
        if (_hcMAC.Length > 0)
        {
            bluetoothManager.StartConnection(_hcMAC);
        } 
        else 
        {
            _connecting = false;
            if (toast)
                bluetoothManager.Toast("Device not found");
            _vitals[2].text = "Device not found";
            _waitingReconnect = true;
        }
    }

    public void TryConnect(bool toast = false)
    {
        if (!_connecting) {
            _connecting = true;
            _vitals[2].text = "Connecting...";
            FindHC();
            ConnectToHc(toast);
        } else {
            bluetoothManager.Toast("Already Connecting");
        }
    }

    public void SubmitWIFI()
    {
        string ssid = GameObject.Find("ssidInput").GetComponent<TMP_InputField>().text;
        string password = GameObject.Find("passwordInput").GetComponent<TMP_InputField>().text;
        bluetoothManager.WriteData("wifi" + ssid + "+" + password);
    }

    public void SubmitLocation()
    {
        string location = GameObject.Find("LocationInput").GetComponent<TMP_InputField>().text;
        bluetoothManager.WriteData("tl" + location);
    }

    public void Send()
    {
        bluetoothManager.WriteData(_inputField.text + "\n");
    }

    private void SendHot()
    {
        bluetoothManager.WriteData("hv" + _valueHot.ToString() + "\n");
    }

    private void SendCold()
    {
        bluetoothManager.WriteData("cv" + _valueCold.ToString() + "\n");
    }

    public void PrintSliderValues()
    {
        _vitals[0].text = "Hot Value: " + _valueHot.ToString();
        _vitals[1].text = "Cold Value: " + _valueCold.ToString();
    }

    public void HandleHotSliderChange(float value)
    {
        _valueHot = (int)value;
        PrintSliderValues();
        SendHot();
    }

    public void HandleColdSliderChange(float value)
    {
        _valueCold = (int)value;
        PrintSliderValues();
        SendCold();
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

    private void Update() {
        if (_waitingReconnect) {
            _reconnectTimer += Time.deltaTime;
            if (_reconnectTimer > _timeToWait) {
                _reconnectTimer = 0f;
                _waitingReconnect = false;
                TryConnect();
            }
        }
    }
}
