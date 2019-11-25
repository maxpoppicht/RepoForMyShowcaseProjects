using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

// Tobias Stroedicke

public static class WeaponDamage
{
	public static float Damage(AWeapon.WeaponName _weaponType)
    {
        switch (_weaponType)
        {
            case AWeapon.WeaponName.MACHINEGUN:
                return 7.1f;

            case AWeapon.WeaponName.KATANA:
                return 32.4f;
            // broken things
            default:
                throw new System.NotImplementedException(_weaponType + " is not valid");
        }
    }
}
