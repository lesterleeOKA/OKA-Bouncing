using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SortRoad : MonoBehaviour
{
    public Direction direction = Direction.toRight;
    public int orderLayer = 0;
    public GameObject movingObjectPrefab; // Reference to the MovingObject prefab
    public int maxMovingItems = 2; // Maximum number of moving items allowed
    private Image roadHintImage;
    private Canvas canvas;
    private MovingObject[] movingItems;

    public enum Direction { none, toLeft, toRight};

    // Start is called before the first frame update
    void Start()
    {
        if(this.canvas == null)
        {
            this.canvas = this.GetComponent<Canvas>();
            if(this.canvas != null ) {   
               this.canvas.sortingOrder = this.orderLayer;
            }
        }

        if(this.roadHintImage == null)
        {
            this.roadHintImage = this.GetComponent<Image>();
            this.showRoadHint(false);
        }

        if(this.direction == Direction.none) return;

        // Initialize the moving items array
        this.movingItems = new MovingObject[this.maxMovingItems];
        bool toLeft = this.direction== Direction.toLeft;
        // Instantiate MovingObject prefabs up to the maximum limit
        for (int i = 0; i < this.maxMovingItems; i++)
        {
            GameObject movingItemObject = Instantiate(this.movingObjectPrefab, this.transform);
            this.movingItems[i] = movingItemObject.GetComponent<MovingObject>();
            if (this.movingItems[i] != null)
            {
                this.movingItems[i].startPosX = toLeft ? 1550f : -1550f;
                this.movingItems[i].transform.localScale = new Vector3(toLeft? -1f : 1f, 1f, 1f);
                this.movingItems[i].SortLayer = this.orderLayer + 1;
            }
        }
    }

    public void startMovingItems(int roadId)
    {
        StartCoroutine(this.delayNextItem(3f, roadId));
    }

    private IEnumerator delayNextItem(float delay = 1f, int roadId = -1)
    {
        foreach (var movingItem in this.movingItems)
        {
            if (movingItem != null)
            {
                movingItem.StartNewMovement(roadId);
                yield return new WaitForSeconds(delay);
            }
        }

    }

    public void setOrder(int newOrder)
    {
        if(this.canvas != null)
        {
            this.orderLayer = newOrder;
            this.canvas.sortingOrder = this.orderLayer;
        }
    }

    public void showRoadHint(bool status)
    {
        if(this.roadHintImage != null)
        {
            this.roadHintImage.enabled = status;
        }
    }
}
