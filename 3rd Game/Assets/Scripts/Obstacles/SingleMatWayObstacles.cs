using System.Threading.Tasks;
using UnityEngine;

public class SingleMatWayObstacles : MonoBehaviour, IObsTypes
{
    [field: SerializeField]
    public ObsTypes obsType { get; set; }

    public Material NeededMat;

    void Start()
    {
        int chosen = Random.Range(0, transform.childCount);
        int j = StaticData.ChooseMat(NeededMat);

        for(int i = 0; i < transform.childCount; i++)
        {
            if(i == chosen)
            {
                transform.GetChild(chosen).GetComponent<MeshRenderer>().material = NeededMat;
            }
            else
            {
                transform.GetChild(i).GetComponent<MeshRenderer>().material = StaticData.Materials[j];
            }
        }
    }   
}
