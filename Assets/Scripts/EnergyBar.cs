using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyBar : MonoBehaviour
{
	public Player TrackedPlayer;
	private Slider slider;
    
    void Start()
    {
        slider = GetComponent<Slider>();
    }

    
    void Update()
    {
        // slider.value = TrackedPlayer.Energy;
		slider.enabled = slider.value > 0;
    }
}
