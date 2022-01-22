using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]

public class WallEditorScript : MonoBehaviour
{

#if UNITY_EDITOR

    void Update()
    {

        if (Application.isEditor && !Application.isPlaying)
        {

            float xscale = (int)(transform.localScale.x / 12) * 12f;
            float yscale = (int)(transform.localScale.y / 12) * 12f;
            transform.localScale = new Vector3(xscale, yscale, 1);
        
        }

    }

#endif

}


