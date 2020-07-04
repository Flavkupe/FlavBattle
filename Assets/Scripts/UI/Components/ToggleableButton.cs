using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Button))]
public class ToggleableButton : MonoBehaviour
{
    [Tooltip("Background that is enabled when this is selected and disabled when unselected.")]
    [Required]
    public GameObject SelectedBackground;

    public event EventHandler OnClicked;

    public Button Button => GetComponent<Button>();

    // Start is called before the first frame update
    void Awake()
    {
        SelectedBackground.Hide();

        Button.onClick.AddListener(() => OnClicked?.Invoke(this, new EventArgs()));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleSelected(bool selected)
    {
        if (selected)
        {
            SelectedBackground.Show();
        }
        else
        {
            SelectedBackground.Hide();
        }
    }
}
