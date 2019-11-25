using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// Tobias Stroedicke

namespace Assets.Scripts.Weapon
{
    public abstract class AGun : AWeapon
    {
        public override MainWeaponType GetMainWeapon { get { return MainWeaponType.GUN; } }

        public bool IsReloading { get; protected set; }

        /// <summary>amount of max Ammo the player can have</summary>
        public abstract int MaxAmmo { get; }

        private int currentAmmo;
        /// <summary>amount of current Ammo the player can have</summary>
        public int CurrentAmmo
        {
            get { { return currentAmmo; } }
            set { { currentAmmo = (value < 0) ? 0 : value; m_Player.AmmoTextBox.text = AmmoText; }
            }
        }

        protected abstract int AmmoPerShot { get; }

        /// <summary>The time when player start reload</summary>
        protected float reloadStart;

        /// <summary>This value gets divided with <see cref="ReloadTime"/> when player is chaser (1 is normal)</summary>
        public abstract float ChaserReloadTime { get; }
        /// <summary>The time need to reload the weapon</summary>
        public abstract float ReloadTime { get; }

        /// <summary>Text for UI</summary>
        public override string AmmoText
        {
            get { return CurrentAmmo + " / " + MaxAmmo; }
        }

        public override bool Shoot()
        {
            if (!base.Shoot()) return false;
            // return if player is reloading
            if (IsReloading)
                return false;
            if (m_Player.AmmoTextBox.gameObject.activeSelf == false)
            {
                return false;
            }

            // check if player is allowed to shoot
            if (CurrentAmmo <= 0 || IsReloading)
                return false;

            CurrentAmmo -= AmmoPerShot;
            lastShot = Time.time;
            ShootSound.PlayOneShot(ShootSound.clip);

            return true;
        }
        protected override void Update()
        {
            if (!m_Player.isLocalPlayer) return;
            base.Update();

            float newReloadTime = -1f;
            if (m_Player.IsChaser)
                newReloadTime = ChaserReloadTime;
            else
                newReloadTime = ReloadTime;

            if (IsReloading)
            {
                if (Time.time - reloadStart >= newReloadTime)
                {
                    CurrentAmmo = MaxAmmo;
                    IsReloading = false;
                    ReloadEnd();
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                reloadStart = Time.time;
                IsReloading = true;
                ReloadStart();
            }
        }

        protected void ReloadStart()
        {
            m_Player.AmmoTextBox.text = "Reloading";
        }

        protected void ReloadEnd()
        {
            SetAmmoText();
        }

        protected override void Start()
        {
            base.Start();
            CurrentAmmo = MaxAmmo;
            ShootSound = GetComponent<AudioSource>();
        }

        /// <summary>
        /// Reset Ammo
        /// </summary>
        public override void ResetAmmo()
        {
            if (!m_Player.isLocalPlayer) return;
            if (IsReloading)
                reloadStart = 0;
            else
                CurrentAmmo = MaxAmmo;
        }

        public override void SetAmmoText()
        {
            m_Player.AmmoTextBox.text = AmmoText;
        }

        public AudioSource ShootSound { get; private set; }

        
    }
}
