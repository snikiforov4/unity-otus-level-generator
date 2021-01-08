using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public enum EMob
    {
        None, CharacterNPC,
        Projectile,
    }

    public static class ComponentsExt
    {
        public static void MoveTo(this Component i, Component to, float speed)
        {
            i.transform.position += (to.transform.position - i.transform.position).normalized
                        * speed * Time.deltaTime;
        }

        public static Transform GetPrefab(this EMob m)
        {
            return Resources.Load<Transform>(m.ToString());
        }

        public static T AddComponent<T>(this Component component) where T : Component
        {
            return component.gameObject.AddComponent<T>();
        }

        public static T GetAddComponent<T, TIn>(this TIn component)
            where T : Component
            where TIn : Transform
        {
            var t = component.GetComponent<T>();
            if (t == null) t = component.gameObject.AddComponent<T>();
            return t;
        }

        public static void Subscribe()
        {
            var npc = new NPCController();
            npc.OnDie += (n) => { Debug.Log("OnDie: " + n); };
            //npc.OnDie(npc);
        }
    }

    public class NPCController : Character
    {
        private INPC _npcLoigc;

        public INPC NpcLoigc { get => _npcLoigc; set => _npcLoigc = value; }

        public event Action<NPCController> OnDie;

        #region Logic
        private void Awake()
        {
            //var r = gameObject.AddComponent<Rigidbody2D>();
            this.AddComponent<Rigidbody2D>();
            NpcLoigc = new NPCMoveToPlayer();
            //transform.GetAddComponent<Rigidbody2D, Transform>();
        }

        public override void Start()
        {
            InitComponents();
        }

        private void OnDestroy()
        {
            OnDie?.Invoke(this);
        }

        private void Update()
        {
            OnDie?.Invoke(this);
            NpcLoigc.PersonLogic(this, Character.I);
        }
        #endregion
    }

    public interface INPC
    {
        void PersonLogic(NPCController i, Character player);
    }

    internal class NPCMoveTudaSuda : INPC
    {
        public void PersonLogic(NPCController i, Character player)
        {
            Debug.Log("Логика поведения");
        }
    }

    internal class NPCMoveToPlayer : INPC
    {
        private enum EState { None, Idle, Move, Attack }

        private const int Speed = 10;
        private EState State;
        private float AttackTime;

        public void PersonLogic(NPCController i, Character player)
        {
            switch (State)
            {
                case EState.None:
                    State = EState.Idle;
                    break;
                case EState.Idle:
                    Debug.Log("Стоим на месте: " + State);
                    if (player != null) State = EState.Move;
                    break;
                case EState.Move:
                    if (player == null) State = EState.Idle;
                    else if (Vector3.Distance(player.transform.position, i.transform.position) < 3)
                        State = EState.Attack;
                    else i.MoveTo(player, Speed);
                    Debug.Log("Бежим: " + State);
                    break;
                case EState.Attack:
                    if(AttackTime > Time.time) return;
                    AttackTime = Time.time + 1;
                    i.StartCoroutine(ProjectileMove());
                    IEnumerator ProjectileMove()
                    {
                        var p = EMob.Projectile.GetPrefab();
                        var projectile = GameObject.Instantiate(p, i.transform.position, Quaternion.identity, null);
                        for (float t = 0; t < 50; t++)
                        {
                            t += Time.deltaTime;
                            projectile.MoveTo(player, 20);
                            yield return null;
                            if (Vector3.Distance(player.transform.position, i.transform.position) < 1)
                                Debug.Log("Game over");
                        }
                        GameObject.Destroy(projectile.gameObject);
                    }
                    break;
                default:
                    Debug.LogError("Неизвестное состояние: " + State);
                    break;
            }
        }
    }
}
