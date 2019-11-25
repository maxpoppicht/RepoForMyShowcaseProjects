using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Tobias Stroedicke

public class Valkyrie : MonoBehaviour {
#if UNITY_EDITOR
    [TagSelector]
#endif
    public string m_ignoreCollisiontag;
	// Use this for initialization
	void Start ()
    {
        // get gameobjects in scene with tag

        GameObject[] gos = GameObject.FindGameObjectsWithTag(m_ignoreCollisiontag);
        foreach (GameObject go in gos)
        {
            Collider col = go.GetComponent<Collider>();
            if (col == null) continue;
            Physics.IgnoreCollision(this.GetComponent<Collider>(), col, true);
        }

        gos = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject go in gos)
        {
            Collider col = go.GetComponent<Collider>();
            if (col == null) continue;
            Physics.IgnoreCollision(this.GetComponent<Collider>(), col, true);
        }

        gos = GameObject.FindGameObjectsWithTag("Hitable");
        foreach (GameObject go in gos)
        {
            Collider col = go.GetComponent<Collider>();
            if (col == null) continue;
            Physics.IgnoreCollision(this.GetComponent<Collider>(), col, true);
        }
    }
}
