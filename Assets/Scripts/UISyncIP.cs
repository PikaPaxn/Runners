using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;

public class UISyncIP : MonoBehaviour
{
    MyNetworkManager _networkManager;
    TMP_InputField _inputField;

    // Start is called before the first frame update
    void Start()
    {
        _inputField = GetComponent<TMP_InputField>();
        _networkManager = MyNetworkManager.singleton;
        _inputField.text = _networkManager.networkAddress;
    }

    public void UpdateNetworkAddress(string address) => _networkManager.networkAddress = address;
}
