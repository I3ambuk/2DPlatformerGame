using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MovementController
{
    public class UIScript : MonoBehaviour
    {
        [SerializeField] StaminaControl stamina_control;
        VisualElement staminaBar;
        private void OnEnable()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            staminaBar = root.Q<VisualElement>("Stamina");
        }

        private void FixedUpdate()
        {
            staminaBar.style.width = new StyleLength(Length.Percent(100 * stamina_control.stamina / stamina_control.maxStamina));
        }
    }
}
