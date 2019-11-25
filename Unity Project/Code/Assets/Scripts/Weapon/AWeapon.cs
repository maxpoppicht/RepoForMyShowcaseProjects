using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Diagnostics;
using UnityEngine.UI;
using UnityEngine.Networking;

// Tobias Stroedicke

public abstract class AWeapon : MonoBehaviour
{
    /// <summary>Type of weapon</summary>
    [Flags]
    public enum MainWeaponType
    {
        GUN = 1 << 1,
        SWORD = 1 << 101
    }

    /// <summary>Name of weapon</summary>
    [Flags]
    public enum WeaponName
    {
        MACHINEGUN = 1 << 1,
        KATANA = 1 << 101
    }

    /// <summary>Player of weapon</summary>
    [SerializeField]
    protected PlayerEntity m_Player;

    /// <summary>Get Main weapon type</summary>
    public abstract MainWeaponType GetMainWeapon { get; }

    /// <summary>Get the weapon.</summary>
    /// <value><see cref="WeaponName"/></value>
    public abstract WeaponName GetWeaponName { get; }

    /// <summary>Time of last shot</summary>
    protected float lastShot;

    public abstract string AmmoText { get; }

    /// <summary>Time player has to wait before next shot can be fired when player is chaser</summary>
    public abstract float ChaserWaitTime { get; }

    /// <summary>Time player has to wait before next shot can be fired when player is not chaser</summary>
    public abstract float ShootWaitTime { get; }
    
    /// <summary>Damage of Weapon. Uses <see cref="WeaponDamage.Damage(WeaponName)"/></summary>
    public float Damage { get { return WeaponDamage.Damage(GetWeaponName); } }

    /// <summary>Return true if you can hold down the button</summary>
    public abstract bool HasRapidFire { get; }

    public virtual bool Shoot()
    {
        float f = -1;
        if (m_Player.IsChaser)
            f = ChaserWaitTime;
        else
            f = ShootWaitTime;

        // if wait time is higher than times between last shot and this shot
        if (Time.time - lastShot < f)
            return false;
        else
            return true;
    }

    protected virtual void Update()
    {

    }

    /// <summary>
    /// Reset Ammo for Guns. 
    /// </summary>
    public virtual void ResetAmmo()
    {
    }

    public abstract void SetAmmoText();

    protected virtual void Start() { }
}
