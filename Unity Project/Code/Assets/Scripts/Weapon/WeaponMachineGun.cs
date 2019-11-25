using Assets.Scripts.Weapon;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Tobias Stroedicke

public class WeaponMachineGun : AGun
{

    public override WeaponName GetWeaponName { get { return WeaponName.MACHINEGUN; } }

    public override float ShootWaitTime { get { return 0.05f; } }
    public override float ChaserWaitTime { get { return ShootWaitTime / 2.3f; } }

    public override bool HasRapidFire { get { return true; } }

    public override int MaxAmmo { get { return 30; } }

    public override float ReloadTime { get { return 1f; } }

    public override float ChaserReloadTime { get { return ReloadTime / 3; } }

    protected override int AmmoPerShot { get { return 1; } }

    public override bool Shoot()
    {
        return base.Shoot();
    }

    protected override void Update()
    {
        base.Update();
    }
}
