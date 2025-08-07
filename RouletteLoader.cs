using UnityEngine;

public class RouletteLoader : MonoBehaviour

{

    [Tooltip("The RouletteWheel script to control.")]

    public RouletteWheel rouletteWheel;

    void Start()

    {

    }

    public void OnPlayerClick()

    {

        if (rouletteWheel != null)

        {

            rouletteWheel.Spin();

        }

        else

        {

            Debug.LogError("RouletteWheel reference not set on RouletteLoader.");

        }

    }

}