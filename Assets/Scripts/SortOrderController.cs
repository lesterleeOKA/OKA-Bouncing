using UnityEngine;

public class SortOrderController : MonoBehaviour
{
    public static SortOrderController Instance = null;
    public SortRoad[] roads;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    public void startMovingObjects()
    {
        for(int i=0; i< this.roads.Length; i++)
        {
            if (this.roads[i] != null && this.roads[i].direction != SortRoad.Direction.none)
            {
                this.roads[i].startMovingItems(i-1);
            }
        }
    }
}
