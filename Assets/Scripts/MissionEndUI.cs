using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using static TurretScript;

public class MissionEndUI : MonoBehaviour
{
    public GameObject panel;
    public GameObject turretDropdownPrefab;
    public Transform dropdownContainer;
    public Button nextButton;

    private List<TMP_Dropdown> dropdowns = new();

    void Start()
    {
        panel.SetActive(false);
        nextButton.onClick.AddListener(ApplyChangesAndContinue);
    }

    public void Show(List<TurretType> currentTurrets)
    {
        panel.SetActive(true);

        foreach (Transform child in dropdownContainer)
            Destroy(child.gameObject);

        dropdowns.Clear();

        foreach (var turret in currentTurrets)
        {
            var dropdownGO = Instantiate(turretDropdownPrefab, dropdownContainer);
            var dropdown = dropdownGO.GetComponent<TMP_Dropdown>();
            dropdown.options = new List<TMP_Dropdown.OptionData>();

            foreach (var type in System.Enum.GetValues(typeof(TurretType)))
                dropdown.options.Add(new TMP_Dropdown.OptionData(type.ToString()));

            dropdown.value = (int)turret;
            dropdowns.Add(dropdown);
        }
    }

    private void ApplyChangesAndContinue()
    {
        Player.Instance.turretTypes.Clear();

        foreach (var dd in dropdowns)
            Player.Instance.turretTypes.Add((TurretType)dd.value);

        Player.Instance.SaveState();
        GameManager.Instance.StartNextPhase(); // Reloads scene for now
    }
}
