using UnityEngine;

public class DestroyParticle : MonoBehaviour
{
    private float delayTimer = 1;

    // Update is called once per frame
    private void Update()
    {
        delayTimer -= Time.deltaTime;
        if (delayTimer <= 0)
        {
            Destroy(gameObject);
        }
    }
}