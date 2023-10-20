using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class VFXManager : NetworkSingleton<VFXManager>
{
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private GameObject vfxPrefab;
    [SerializeField] private List<AnimationClip> vfxAnims;
    [Command(requiresAuthority = false)] public void CreateFloatingText(string text, Color color, Vector3 pos) {
        SendFloatingText(text, color, pos);
    }
    [ClientRpc] private void SendFloatingText(string text, Color color, Vector3 pos) {
        GameObject prefab = Instantiate(floatingTextPrefab, pos, Quaternion.identity);

        var tmp = prefab.GetComponent<TextMeshPro>();
        tmp.text = text;
        tmp.color = color;
    }

    public void CreateVFX(VFXType type, Vector3 pos, Quaternion rotation, float durationOverride = -1, string layerOverride = null) {
        ServerSendVFX(type, pos, rotation, durationOverride, layerOverride);
    }

    [Command(requiresAuthority = false)] private void ServerSendVFX(VFXType type, Vector3 pos, Quaternion rotation, float durationOverride, string layerOverride) {
        SendVFX(type, pos, rotation, durationOverride, layerOverride);
    }

    [ClientRpc] private void SendVFX(VFXType type, Vector3 pos, Quaternion rotation, float durationOverride, string layerOverride) {
        GameObject vfx = Instantiate(vfxPrefab, pos, rotation);

        Animator animator = vfx.GetComponent<Animator>();
        AnimatorOverrideController animController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animController["particle"] = vfxAnims[(int)type-1];
        animator.runtimeAnimatorController = animController;

        vfx.GetComponent<DestroyAfterDelay>().Init(vfxAnims[(int)type-1].length - 1/60f);
        if (durationOverride != -1) {
            vfx.GetComponent<DestroyAfterDelay>().Init(durationOverride);
        }

        if (layerOverride != null)
            vfx.GetComponent<SpriteRenderer>().sortingLayerName = layerOverride;
    }


}

public enum VFXType {
    NONE, VFX_SHOOT, VFX_ARROW, VFX_HIT
}
