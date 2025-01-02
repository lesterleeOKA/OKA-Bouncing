using System;
using UnityEngine;
using UnityEngine.UI;

public class BloodController : MonoBehaviour
{
    public Image[] bloods;
    public Sprite[] bloodSprites;
    public int totalBload;
    // Start is called before the first frame update
    void Start()
    {
        this.setBloods(true);
    }

    public void setBloods(bool init = true, Action onCompleted = null)
    {
        if (!init)
        {
            if (this.totalBload > 0)
            {
                this.totalBload -= 1;
                if (this.bloods[this.totalBload] != null)
                {
                    this.bloods[this.totalBload].sprite = this.bloodSprites[0];
                }
            }
            else
            {
                onCompleted?.Invoke();
            }
        }
        else
        {
            this.totalBload = this.bloods.Length;
            for (int i=0; i<this.bloods.Length; i++)
            {
                if (this.bloods[i] != null)
                {
                    this.bloods[i].sprite = this.bloodSprites[1];
                }
            }
        }
       
    }
}
