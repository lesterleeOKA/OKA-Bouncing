using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Cell : MonoBehaviour
{
    public int playerId = -1;
    public TextMeshProUGUI content;
    private CanvasGroup cellImage = null;
    public Sprite[] cellSprites;
    public Color32 defaultColor = Color.black;
    public Color32 selectedColor = Color.white;
    public int row;
    public int col;
    public bool isSelected = false;
    public int cellId = -1;

    public void SetTextContent(string letter="", Color _color = default, Sprite gridSprite = null)
    {
        if(gridSprite != null) this.cellSprites[0] = gridSprite;
        if (this.cellImage == null) 
            this.cellImage = this.GetComponent<CanvasGroup>();

        //this.SetButtonColor(_color);
        //this.cellImage.sprite = this.cellSprites[0];

        if (this.content != null) {
            this.SetTextStatus(true);
            this.content.text = letter;
            this.content.color = this.defaultColor;
        }
        this.isSelected = !string.IsNullOrEmpty(letter) ? true : false;

        if (string.IsNullOrEmpty(letter))
        {
            this.setCellStatus(false);
        }
        else
        {
            this.setCellStatus(true);
        }
    }

    public void SetTextStatus(bool show, float duration=0.5f)
    {
        this.transform.DOScale(show ? 1f : 0f, duration).SetEase(Ease.InOutSine);
        this.isSelected = show ? true : false;
    }

    public void SetTextColor(Color _color = default)
    {
        if (this.content != null)
        {
            if (_color != default(Color))
                this.content.color = _color;
            else
                this.content.color = Color.black;
        }
    }

    public void setCellStatus(bool show=false)
    {
        if(this.cellImage != null)
        {
            this.cellImage.alpha = show? 1f:0f;
        }
    }

    public void setCellEnterColor(bool show = false)
    {
        if (this.cellImage != null && show)
        {
            this.cellImage.alpha = show ? 1f : 0f;
            this.cellImage.GetComponent<Image>().color = show ? Color.yellow : Color.white;
        }
    }

}
