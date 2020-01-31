using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnOff_Cn
{
    // container class for on/off data
    public bool on; // true for entire time its on; false for entire time its off
    public bool onClick; // true for one frame the moment it switches to on
    public bool offClick; // true for one frame the moment it switches to off
    public float onDur;
    public float offDur;
    [ Range( 0f, 1f ) ]
    public float fadeOutPhase; // <--- normalized
}
