using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class VictoryZone : MonoBehaviour
{
    [SerializeField] private string victorySceneName = "Victory";
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            SceneTransitioner.Instance.LoadScene(victorySceneName);
    }
}