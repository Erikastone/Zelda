using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    public Dray dray;
    private SphereCollider colDray;
    private void Start()
    {
        GameObject go = GameObject.Find("Dray");
        dray = GetComponent<Dray>();
        colDray = GetComponent<SphereCollider>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Dray")
        {
            var index = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(index + 1);
        }
    }
}
