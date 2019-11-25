using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Tobias Stroedicke

public class Colosseum : MonoBehaviour {

    private static Colosseum m_colosseum;
    public static Colosseum Get
    {
        get
        {
            if (m_colosseum == null)
            {
                m_colosseum = FindObjectOfType<Colosseum>();
            }
            return m_colosseum;
        }
    }
}
