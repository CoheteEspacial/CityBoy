using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using static TurretScript;

public class MissionEndUI : MonoBehaviour
{
    public Button nextButton;

    private List<TMP_Dropdown> dropdowns = new();

    void Start()
    {
        gameObject.SetActive(false);
        nextButton.onClick.AddListener(ApplyChangesAndContinue);
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
