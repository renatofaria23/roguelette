using UnityEngine;
using UnityEngine.UI;

public class RouletteSlot : MonoBehaviour
{

    public Image sliceImage;

    public void Setup(SliceData data, float angle)
    {

        if (sliceImage == null)
        {
            Debug.LogError("RouletteSlot is missing the Slice Image component reference!");
            return;
        }

        sliceImage.material = null;

        sliceImage.color = data.displayColor;

        sliceImage.fillAmount = angle / 360f;
    }
}