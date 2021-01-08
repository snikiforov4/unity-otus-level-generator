
using Assets.Scripts;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject Prefab;
    public float Delay = 1.0f;
    private float TimeLeft;

    private void Start()
    {
        TimeLeft = Delay;
    }

    private void Update()
    {
        TimeLeft -= Time.deltaTime;
        if (TimeLeft < 0.0f)
        {
            TimeLeft = Delay;
            //Instantiate(Prefab, transform.position, transform.rotation);

            //var prefab = Resources.Load<NPCController>("CharacterNPC");
            var mob = Instantiate(EMob.CharacterNPC.GetPrefab(),
                transform.position + new Vector3(Random.Range(0, 10), 0),
                transform.rotation).GetComponent<NPCController>();

            if (Random.value > 0.5f)
                mob.NpcLoigc = new NPCMoveToPlayer();
            else mob.NpcLoigc = new NPCMoveTudaSuda();
        }
    }
}
