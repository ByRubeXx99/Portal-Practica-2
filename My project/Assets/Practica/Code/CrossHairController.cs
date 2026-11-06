using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [Header("UI Reference")]
    public Image crosshairImage;

    [Header("Crosshair States")]
    public Sprite emptyCrosshair;
    public Sprite blueCrosshair;
    public Sprite orangeCrosshair;
    public Sprite bothCrosshair;

    public void UpdateCrosshair(bool hasBluePortal, bool hasOrangePortal)
    {
        if (hasBluePortal && hasOrangePortal)
        {
            crosshairImage.sprite = bothCrosshair;
        }
        else if (hasBluePortal)
        {
            crosshairImage.sprite = blueCrosshair;
        }
        else if (hasOrangePortal)
        {
            crosshairImage.sprite = orangeCrosshair;
        }
        else
        {
            crosshairImage.sprite = emptyCrosshair;
        }
    }
}