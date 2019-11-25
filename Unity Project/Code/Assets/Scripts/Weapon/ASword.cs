using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// Tobias Stroedicke

namespace Assets.Scripts.Weapon
{
    public abstract class ASword : AWeapon
    {
        public readonly List<GameObject> alreadyhit = new List<GameObject>();

        public Material m_hitMaterial;

        public Material m_idleMaterial;

        public abstract float ActiveDuration { get; }

        public float ActivationTime { get; private set; }

        public virtual bool Activated { get; private set; }

        public override MainWeaponType GetMainWeapon { get { return MainWeaponType.SWORD; } }

        public override string AmmoText { get { return "∞ / ∞"; } }

        public override bool Shoot()
        {
            if (!base.Shoot()) return false;

            // if wait time is higher than times between last shot and this shot
            if (Time.time - lastShot < ShootWaitTime + ActiveDuration)
                return false;

            lastShot = Time.time;
            Activated = true;
            ActivationTime = Time.time;
            gameObject.GetComponent<Renderer>().material = m_hitMaterial;
            return true;

        }

        protected override void Start()
        {
            base.Start();
            HitSound = GetComponent<AudioSource>();
        }
        protected override void Update()
        {
            base.Update();

            if (Activated)
            {
                if (Time.time - ActivationTime > ActiveDuration)
                {
                    Activated = false;
                    alreadyhit.Clear();
                    gameObject.GetComponent<Renderer>().material = m_idleMaterial;
                }
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag != m_Player.DamageTag) return;
            DealDamage(other.gameObject.GetComponentInParent<PlayerEntity>());
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            if (other.gameObject.tag != m_Player.DamageTag) return;
            DealDamage(other.gameObject.GetComponentInParent<PlayerEntity>());
        }

        /// <summary>
        /// calculate if player is allowed to deal damage
        /// </summary>
        /// <param name="_hit"></param>
        public void DealDamage(PlayerEntity _hit)
        {
            // return if is not player
            if (!m_Player.isLocalPlayer) return;

            // check if weapon is activated
            if (Activated)
            {
                // check if player was hit
                if (_hit.gameObject.tag != m_Player.DamageTag &&
                    _hit.gameObject.tag != "Player" &&
                    _hit.gameObject.tag != m_Player.ValkyrieTag) return;

                // check if hit player was already hit
                if (alreadyhit.Contains(_hit.gameObject)) return;

                // play hit sound
                HitSound.PlayOneShot(HitSound.clip);

                // player gets damage
                m_Player.CmdSword(GetWeaponName, _hit.gameObject, m_Player.transform.forward);
                alreadyhit.Add(_hit.gameObject);
            }

         }

        /// <summary>
        /// Does Nothing
        /// </summary>
        public override void ResetAmmo()
        {
            base.ResetAmmo();
        }

        public override void SetAmmoText()
        {
            m_Player.AmmoTextBox.text = AmmoText;
        }

        public AudioSource HitSound { get; private set; }
    }
}
