using UnityEngine;

public class PickUp : MonoBehaviour
{
    public enum eType { key, health, grappler }

    public static float COLLIDER_DELAY = 0.5f;

    [Header("Set in I")]
    public eType itemType;
    // Awake() � Activate() ������������ ��������� �� 0,5 �������
    private void Awake()
    {
        GetComponent<Collider>().enabled = false;
        Invoke("Activate", COLLIDER_DELAY);
    }
    private void Activate()
    {
        GetComponent<Collider>().enabled = true;
    }
}
