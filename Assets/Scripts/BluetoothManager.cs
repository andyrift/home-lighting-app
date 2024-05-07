using UnityEngine.Android;
using UnityEngine;

public class BluetoothManager : MonoBehaviour
{
    private bool isConnected;

    private static AndroidJavaClass unity3dbluetoothplugin;
    private static AndroidJavaObject BluetoothConnector;

    // creating an instance of the bluetooth class from the plugin 
    public void InitBluetooth()
    {
        isConnected = false;

        if (Application.platform != RuntimePlatform.Android)
            return;

        if (!Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_ADMIN")
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH")
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN")
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_ADVERTISE")
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT"))
        {

            Permission.RequestUserPermissions(new string[] {
                            "android.permission.BLUETOOTH_ADMIN",
                            "android.permission.BLUETOOTH",
                            "android.permission.BLUETOOTH_SCAN",
                            "android.permission.BLUETOOTH_ADVERTISE",
                            "android.permission.BLUETOOTH_CONNECT"
                    });

        }

        unity3dbluetoothplugin = new AndroidJavaClass("com.example.unity3dbluetoothplugin.BluetoothConnector");
        BluetoothConnector = unity3dbluetoothplugin.CallStatic<AndroidJavaObject>("getInstance");
    }
    
    public void StartScanDevices()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        BluetoothConnector.CallStatic("StartScanDevices");
    }

    public void StopScanDevices()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        BluetoothConnector.CallStatic("StopScanDevices");
    }

    // This function will be called by Java class to update the scan status,
    // DO NOT CHANGE ITS NAME OR IT WILL NOT BE FOUND BY THE JAVA CLASS
    public void ScanStatus(string status)
    {
        Toast("Scan Status: " + status);
    }

    // This function will be called by Java class whenever a new device is found,
    // and delivers the new devices as a string data="MAC+NAME"
    // DO NOT CHANGE ITS NAME OR IT WILL NOT BE FOUND BY THE JAVA CLASS
    public void NewDeviceFound(string data)
    {
        
    }

    // Get paired devices from BT settings
    public string[] GetPairedDevices()
    {
        if (Application.platform != RuntimePlatform.Android)
            return new string[0];

        // This function when called returns an array of PairedDevices as "MAC+Name" for each device found
        string[] data = BluetoothConnector.CallStatic<string[]>("GetPairedDevices");

        return data;
    }

    // Start BT connect using device MAC address
    public void StartConnection(string MACaddr)
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        BluetoothConnector.CallStatic("StartConnection", MACaddr.ToUpper());
    }

    // Stop BT connetion
    public void StopConnection()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        if (isConnected)
            BluetoothConnector.CallStatic("StopConnection");
    }

    
    public delegate void ConnectionStatusHandler(string status);
    public event ConnectionStatusHandler ConnectionStatusEvent;

    // This function will be called by Java class to update BT connection status,
    // DO NOT CHANGE ITS NAME OR IT WILL NOT BE FOUND BY THE JAVA CLASS
    public void ConnectionStatus(string status)
    {
        Toast("Connection Status: " + status);
        isConnected = status == "connected";
        ConnectionStatusEvent.Invoke(status);
    }

    public delegate void ReadDataHandler(string data);
    public event ReadDataHandler ReadDataEvent;
    // This function will be called by Java class whenever BT data is received,
    // DO NOT CHANGE ITS NAME OR IT WILL NOT BE FOUND BY THE JAVA CLASS
    public void ReadData(string data)
    {
        Debug.Log("BT Stream: " + data);
        ReadDataEvent.Invoke(data);
    }

    // Write data to the connected BT device
    public void WriteData(string data)
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        if (isConnected)
            BluetoothConnector.CallStatic("WriteData", data);
    }

    // This function will be called by Java class to send Log messages,
    // DO NOT CHANGE ITS NAME OR IT WILL NOT BE FOUND BY THE JAVA CLASS
    public void ReadLog(string data)
    {
        Debug.Log(data);
    }


    // Function to display an Android Toast message
    public void Toast(string data)
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        BluetoothConnector.CallStatic("Toast", data);
    }
}
