using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DropTable", menuName = "Quirky-Dirt/DropTable", order = 0)]
public class DropTable : ScriptableObject {
    [SerializeField] private List<RollTable> rolls;
    public int[] RollDrops() {
        List<int> drops = new();
        foreach (var roll in rolls)
            RollOnTable(roll, in drops);
        return drops.ToArray();
    }

    private void RollOnTable(RollTable table, in List<int> drops) {
        int totalWeight = table.noDropWeight;
        foreach (var drop in table.itemWeights)
            totalWeight += drop.weight;

        int roll = UnityEngine.Random.Range(0, totalWeight);
        int weightTally = table.noDropWeight;
        if (roll < weightTally)
            return;

        foreach (var drop in table.itemWeights) {
            weightTally += drop.weight;

            if (roll < weightTally) {
                drops.Add(drop.itemID);
                return;
            }
        }
    }

    [Serializable]
    struct RollTable {
        public int noDropWeight;
        public List<ItemRoll> itemWeights;
    }

    [Serializable]
    struct ItemRoll {
        public int itemID;
        public int weight;
    }
}
