using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// Tobias Stroedicke

namespace Assets.Scripts.Weapon
{
    class Katana : ASword
    {
        public override float ActiveDuration { get { return 1.0f; } }
        public override WeaponName GetWeaponName { get { return WeaponName.KATANA; } }

        public override float ShootWaitTime { get { return 0.8f; } }
        public override float ChaserWaitTime { get { return ShootWaitTime / 3; } }

        public override bool HasRapidFire { get { return false; } }

        public override bool Shoot()
        {
            return base.Shoot();
        }

        protected override void OnTriggerStay(Collider other)
        {
            base.OnTriggerStay(other);
        }

        protected override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
        }
    }
}
