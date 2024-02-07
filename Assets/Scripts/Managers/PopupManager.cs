using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [SerializeField] private GameObject _popupPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public DamagePopup CreateDamagePopup(Vector3 position, int damage)
    {
        GameObject popup = Instantiate(_popupPrefab, position, Quaternion.identity);

        DamagePopup damagePopup = popup.GetComponent<DamagePopup>();
        damagePopup.Setup(damage);

        return damagePopup;
    }
}
