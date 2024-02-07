using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro _damageText;

    private Color _textColor;
    private float _dissapearTime;

    private void Awake()
    {
        _damageText = GetComponent<TextMeshPro>();
    }

    public void Setup(int damage)
    {
        _damageText.text = damage.ToString();
        _textColor = _damageText.color;
        _dissapearTime = 1.0f;
    }

    private void Update()
    {
        float moveYSpeed = 1.0f;
        transform.position += new Vector3(0f, moveYSpeed) * Time.deltaTime;

        _dissapearTime -= Time.deltaTime;
        if ( _dissapearTime < 0)
        {
            float dissaparSpeed = 2f;
            _textColor.a -= dissaparSpeed * Time.deltaTime;
            _damageText.color = _textColor;
            if (_textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
