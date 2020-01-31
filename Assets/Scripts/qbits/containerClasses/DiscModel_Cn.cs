using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscModel_Cn
{
    // this container class is for each lightDiscMovement script to receive its freq, amp, decays, and transp
    // the MakeDiscsModel script makes a List<DiscModel_Cn>, and each lightDiscMovement script randomly selects one of these from the list

    public List<float> freqs;
    public List<float> amps;
    public List<float> decays;
        // values directly from the score, merely passed on by the MakeDiscsModel script:
    public DiscModelParams_Cn.ModelTransposition modelTransposition;
    public float ampGlobal;
}
