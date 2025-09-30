using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameEventWithBool OnPlayerDied;
    [SerializeField] GameObject failPanel;
    private void OnEnable()
    {
        OnPlayerDied.OnEventRaised += DisplayStatus;
    }

    private void DisplayStatus(bool obj)
    {
        if (!obj) { failPanel.SetActive(true); }
    }

    private void OnDisable()
    {
        OnPlayerDied.OnEventRaised += DisplayStatus;
    }

}
