using UnityEngine;

namespace Arena
{
    public interface IPlayer
    {
        void StartMove();
        void StopMove();
        void StartAttack();
        void StopAttack();
        void SetActive();
        void Hit();

        GameObject GetGameObject();
        string GetStringName();
    }
}