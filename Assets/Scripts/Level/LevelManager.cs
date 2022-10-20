using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Level
{
    [System.Serializable]
    public struct LevelRules
    {
        public GameObject[] AvailableCars;
        public CarSpawn[] CarSpawns;

        [HideInInspector]
        public int levelIndex;

        public CarSpawn GetRandomSpawn() => CarSpawns[Random.Range(0, CarSpawns.Length)];

        public GameObject GetRandomCar() => AvailableCars[Random.Range(0, AvailableCars.Length)];
    }
    
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private LevelRules rules;

        public int GetLevelIndex()
        {
            rules.levelIndex = transform.GetSiblingIndex();
            return rules.levelIndex;
        }

        public LevelRules GetRules() => rules;

        [ExecuteInEditMode]
        public void FindAllCarSpawns()
        {
            int childCount = transform.childCount;

            List<CarSpawn> carSpawns = new List<CarSpawn>();

            for (int i = 0; i < childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (child.GetComponent<CarSpawn>())
                    carSpawns.Add(child.GetComponent<CarSpawn>());
            }

            rules.CarSpawns = carSpawns.ToArray();
        }
    }
}
