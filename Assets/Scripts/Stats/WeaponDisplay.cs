using System;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.Combat
{
    public class WeaponDisplay : MonoBehaviour
    {
        Fighter fighter;
        WeaponConfig weapon;

        private void Awake()
        {
            fighter = GameObject.FindWithTag("Player").GetComponent<Fighter>();
        }

        private void Update()
        {
            GetComponent<Text>().text = String.Format("{0:0}", fighter.weaponDurability );
        }
    }
}
