using UnityEngine;
using System;

[CreateAssetMenu(fileName = "AttackDictionary", menuName = "Quirky-Dirt/AttackDictionary", order = 0)]
public class AttackDictionary : ScriptableObject {
    [SerializeField] private AttackData[] attackList;

    public AttackData GetAttack(int index) {
        return attackList[index];
    }
}

[Serializable] public struct AttackData {
    [SerializeField] private int animationID;
    public int AnimationID {
        get {return animationID;}
        private set {animationID = value;}
    }
    [SerializeField] private bool isCharged;
        public bool IsCharged {
        get {return isCharged;}
        private set {isCharged = value;}
    }
    [SerializeField] private HitData data;
        public HitData Data {
        get {return data;}
        private set {data = value;}
    }

}