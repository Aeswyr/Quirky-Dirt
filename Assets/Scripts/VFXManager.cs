using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class VFXManager : NetworkSingleton<VFXManager>
{
    [SerializeField] private GameObject floatingTextPrefab;
    [Command(requiresAuthority = false)] public void CreateFloatingText(string text, Color color, Vector3 pos) {
        SendFloatingText(text, color, pos);
    }
    [ClientRpc] private void SendFloatingText(string text, Color color, Vector3 pos) {
        GameObject prefab = Instantiate(floatingTextPrefab, pos, Quaternion.identity);

        var tmp = prefab.GetComponent<TextMeshPro>();
        tmp.text = text;
        tmp.color = color;
    }
}
