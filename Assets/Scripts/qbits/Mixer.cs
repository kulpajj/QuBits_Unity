using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mixer 
{
    // if its an important value for sculpting, put it here; other scripts come here to get these values
    // this organization makes it much easier to change values in the system on the fly for tweaking/composing/sculpting
    // this includes variables related to DSP directly or to timing and probability
    // naming convention: other scripts name these variables in like fashion, however, replacing the first prefix of the var name with "mx", 
    // e.g. qbit_qtype0_bufferNamePrefix in the QbitMovement script is mx_qtype0_bufferNamePrefix; 
    // e.g. igniter_falling_bufferName in the IgniterMovement script is mx_falling_bufferName
    // in those scripts, given the mx prefix, we know which values are defined here

    // ****
    // qbits
    public float     qbit_velocitymagAsAmplitude_multiplier = 10f; //<--- velocityMagnitude "magnified" and mapped to ampLocal for qtype 0, 1, 2
    public string    qbit_qtype0_bufferNamePrefix = "wood_dark_rhythmic.";
    public float     qbit_qtype0_bufferPlayPos = 0f;
    public float     qbit_qtype0_ampGlobal = .1f;
    public float     qbit_qtype0_transposition = 1f;
    public string    qbit_qtype1Repel_bufferName = "qbit_rollypolly.1";
    public float     qbit_qtype1Repel_ampGlobal = 12f;
    public Vector2   qbit_qtype1Repel_transpRange_high = new Vector2( 1f, 1.2f );
    public Vector2   qbit_qtype1Repel_transpRange_mid = new Vector2( .85f, 1.1f );
    public Vector2   qbit_qtype1Repel_transpRange_low = new Vector2( .85f, .94f );
    public Vector2   qbit_qtype1Brownian_transpRange_high = new Vector2( .8f, .9f );
    public Vector2   qbit_qtype1Brownian_transpRange_mid = new Vector2( .7f, .85f );
    public float     qbit_qtype1Brownian_transpRange_low_max = .85f;
    public Vector2   qbit_qtype1Brownian_transpRange_low_minRandom = new Vector2( .45f, .65f );
    public Vector2   qbit_qtype1Brownian_transp_durTilNextRange = new Vector2( 3f, 10f );
    public string    qbit_qtype1Brownian_bufferPrefix = "qbit_rollypolly";
    public float     qbit_qtype1Brownian_ampGlobal_begin = 26f;
    public float     qbit_qtype1Brownian_ampGlobal_lowerAndRougher = 160f;
    public string    qbit_qtype2_bufferNamePrefix = "wood_pitch_legato.";
    public float     qbit_qtype2_bufferPlayPos = 0f;
    public float     qbit_qtype2_ampGlobal = 10f;
    public float     qbit_qtype2_transposition = 1.3f;
    public int       qbit_frozen_sameBufferCountMax = 1; // <-- 1 means just play once
    public string    qbit_frozen_bufferPrefix_woodDark = "wood_dark_legato";
    public string    qbit_frozen_bufferPrefix_tubesLow = "frozenTube_anim";
    public Vector2   qbit_frozen_bufferTranspositionRange_woodDark = new Vector2( .55f, .8f );
    public Vector2   qbit_frozen_bufferTranspositionRange_tubesLow = new Vector2( .9f, 1f );
    public float[]   qbit_frozen_localAmpLouds_begin = new float[8] { .1f, .2f, .3f, .4f, .7f, 1f, 2f, 2.5f };
    public float[]   qbit_frozen_localAmpQuiets = new float[5] { .002f, .005f, .007f, .008f, .009f };
        // a weight must correspond to each localAmpLoud
        // height of the tube is scaled from localAmp; only the loudest amp will touch the ceiling; so think of the last val in this list as the prob of touching the ceiling
    public List<int> qbit_frozen_localAmpLouds_weightedProbs_beginSpin = new List<int>{ 1, 1, 1, 1, 1, 1, 1, 5 };
    public float     qbit_frozen_ampGlobal_begin = .19f;
    public float     qbit_frozen_ampGlobal_lowerAndRougher = .6f;
        // height of filament scaled from amplitude
    public float     qbit_filament_YscaleMax_begin = .8f;
    public float     qbit_filament_YscaleMax_tallerTubes = 1.5f;
        // here we want the tubes to touch the ceiling
        // we don't know the height of the tree, and the scale is contingent on the scale of the parent qbit, so can't base this on ceilingHeight; just trial and error for this value:
    public float     qbit_filament_YscaleMax_beginSpin = 2.8f;
        // 3 main sections of the orbit phrase: transToOrbiting, orbiting, transToFrozen - enum in qbits script reflects this
    public float     qbit_orbit_transToOrbiting_ampGlobal_begin = .065f;
    public float     qbit_orbit_transToOrbiting_ampGlobal_lowerAndRougher = .08f;
    public string    qbit_orbit_transition_bufferPrefix = "trans_frozenAndOrbiting_tube"; // <-- buffers named trans_frozenAndOrbiting_tubeN_short, or _mid, or _long
    public List<int> qbit_orbit_transToOrbiting_probShortMidLong    = new List<int> { 0, 0, 10 }; //<-- weights are ints and in order as short, mid, long
    public List<int> qbit_orbit_transToOrbiting_sameTubeCountMaxRange_begin        = new List<int> { 2, 4 };
    public List<int> qbit_orbit_transToOrbiting_sameTubeCountMaxRange_beginCeiling = new List<int> { 2, 5 };
    public List<int> qbit_orbit_transToOrbiting_sameTubeCountMaxRange_tallerTubes    = new List<int> { 3, 6 };
    public int       qbit_orbit_frozen_and_transToOrbiting_xfadeDur = 100;
    public Vector2   qbit_orbit_userLetGoTooSoon_xfadeToFrozen_phaseRange = new Vector3( .5f, 0f ); // <-- here, phase runs in reverse; xfade to frozen as the phase is from this val to this val
    public float     qbit_orbit_orbiting_ampGlobal_begin = .035f;
    public float     qbit_orbit_orbiting_ampGlobal_lowerAndRougher = .055f;
    public string    qbit_orbit_orbiting_bufferPrefix = "orbiting_tube";
    public List<int> qbit_orbit_orbiting_probShortLong = new List<int> { 2, 5 }; //<-- weights are ints and in order as short, long
    public Vector2   qbit_orbit_orbiting_durRangeShort = new Vector2( 16f, 21f );
    public Vector2   qbit_orbit_orbiting_durRangeLong  = new Vector2( 23f, 30f );
        // 2xfading grooves in max converts this signal speed range to cents to add to the globalTransp
        // thus value 1f here can be thought of as the sound of the global transposition, and what you enter here is relative to that
        // we will lerp between these destinations:
    public Vector2   qbit_orbit_orbiting_transpLocal_destRange = new Vector2( .94f, 1.06f );
        // how long to lerp between destinations:
    public Vector2   qbit_orbit_orbiting_transpLocal_durRange  = new Vector2( 1f, 4f );
    public float     qbit_orbit_transToFrozen_ampGlobal = .05f;
    // these xfades are contingent on the durs of the main sections above:
    public Vector2   qbit_orbit_transitionXfadeOut_proportionOfTotalOrbitingDurRange = new Vector2( .1f, .2f );
    public Vector2   qbit_orbit_orbitingXfadeOut_proportionOfTotalOrbitingDurRange   = new Vector2( .1f, .2f );

    // ****
    // igniters
    public float   igniter_prob = .0037f;
    public float   igniter_destroyWhileFalling_prob_begin = .7f;
    public float   igniter_destroyWhileFalling_prob_beginCeiling = .5f;
    public string  igniter_falling_bufferName              = "igniterFalling.1";
    public Vector2 igniter_falling_delay_timeRange    = new Vector2( 20, 80 );
    public Vector2 igniter_falling_delay_lineDurRange = new Vector2( 300, 2000 );
    public Vector2 igniter_falling_ampGlobalRange_begin        = new Vector2( .0004f, .0017f );
    public Vector2 igniter_falling_ampGlobalRange_beginCeiling = new Vector2( .0014f, .003f );
    public Vector2 igniter_falling_ampGlobalRange_beginSpin    = new Vector2( .0024f, .004f );
    public string  igniter_curving_casetopRolly_bufferName = "igniterCurving.1";
    public string  igniter_curving_figure8s_1_bufferName   = "igniterCurving.2";
    public string  igniter_curving_figure8s_2_bufferName   = "igniterCurving.3";
                   // casetopTranspRange constantly changing - inside the poly and switches between high range and low range, repeat
                   // the poly also determines for itself whether to open gate and play each figure8s curving file / but casetopRolly always plays ( igniterCurving.1 )
    public Vector2 igniter_curving_casetopRolly_delay_timeRange = new Vector2( 20, 80 );
    public Vector2 igniter_curving_figure8s_1_delay_timeRange   = new Vector2( 20, 80 );
    public Vector2 igniter_curving_figure8s_2_delay_timeRange   = new Vector2( 20, 80 );
    public Vector2 igniter_curving_casetopRolly_delay_lineDurRange = new Vector2( 300, 2000 );
    public Vector2 igniter_curving_figure8s_1_delay_lineDurRange   = new Vector2( 300, 2000 );
    public Vector2 igniter_curving_figure8s_2_delay_lineDurRange   = new Vector2( 300, 2000 );
    public Vector2 igniter_curving_ampGlobalRange_begin            = new Vector2( .014f, .021f );
    public Vector2 igniter_curving_ampGlobalRange_beginCeiling     = new Vector2( .02f, .027f );
    public Vector2 igniter_curving_ampGlobalRange_beginSpin        = new Vector2( .03f, .037f );

    // ****
    // background
    // we scale exposure to localAmp, 0. - 1.
    // louderEvents always start at 0. exposure and go to 8.
    // globalAmpRange scales just how loud that ends up being
    // the ampGlobal in the upsideDown is set below in the upsideDown values
    public Vector2 bg_softerEvents_exposureRange_grow = new Vector2( 6f, 8f );
    public Vector2 bg_softerEvents_exposureRange_fade = new Vector2( 0f, .5f );
    public Vector2 bg_change_durationRange_grow = new Vector2( 5f, 15f );
    public Vector2 bg_change_durationRange_fade = new Vector2( 5f, 15f );
    public Vector2 bg_stayPut_durationRange_grown = new Vector2( 3f, 5f );
    public Vector2 bg_stayPut_durationRange_faded = new Vector2( 8f, 15f );
    public float   bg_tubes_softerEvents_ampGlobal = .003f;
    public float   bg_noise_softerEvents_ampGlobal = .003f;
    public float   bg_louderEventProb_begin = 0f;
    public float   bg_louderEventProb_beginCeiling = .47f;
                   // louderEvents change the globalAmp
    public Vector2 bg_louderEvents_ampGlobalRange_begin     = new Vector2( .017f, .04f );
    public Vector2 bg_louderEvents_ampGlobalRange_beginSpin = new Vector2( .025f, .043f );

    // ****
    // voids static, shakeIt, upsideDownReveal
    // params for random timing and density of tinyFlurries, sparse, igniterEvents are still in the mesh script for now 
    public float   void_static_oneClick_trailProb = .18f;
    public Vector2 void_static_oneClick_ampRange_trailDelay = new Vector2( 7f, 9f );
    public Vector2 void_static_oneClick_ampRange_sparse = new Vector2( .03f, .08f );
    public Vector2 void_static_oneClick_ampRange_tinyFlurry = new Vector2( .06f, .1f );
    public float   void_static_tinyFlurry_probability = .02f;
    public float   void_static_igniterEvent_ampGlobal = 1.6f;
    public Vector2 void_static_onDurRange  = new Vector2( 8f, 14f );
    public Vector2 void_static_offDurRange = new Vector2( 5f, 10f );
    public float   void_edgeMoving_ampGlobal = .07f; // <-- todo delete
    public float   void_shakeIt_ampGlobal = .07f;
    public Vector2 void_upsideDownReveal_on_longer_durRange   = new Vector2( 8f, 11f );
    public Vector2 void_upsideDownReveal_on_shorter_durRange  = new Vector2( 4f, 6f );
    public float   void_upsideDownReveal_prob_onLonger  = .5f;
    public Vector2 void_upsideDownReveal_off_longer_durRange  = new Vector2( 10f, 30f );
    public Vector2 void_upsideDownReveal_off_shorter_durRange = new Vector2( 2f, 6f );
    public float   void_upsideDownReveal_prob_offLonger = .8f;
    public string  void_upsideDownReveal_bufferPrefix = "upsideDownReveal";
    public float   void_upsideDownReveal_ampGlobal_begin = 1.4f;
    public float   void_upsideDownReveal_ampGlobal_beginSpin = 1.1f;
    public Vector2 void_upsideDownReveal_transpRange = new Vector2( .85f, 1.25f ); // goes to groove~ sig~

    // ****
    // geysers
    public float   geyser_ampGlobal = .45f;
        // in beginning, just don't want it to reach ceiling before it is killed, so minimize speed
    public Vector2 geyser_startSpeedRange_begin = new Vector2( .8f, 1f );
    public Vector2 geyser_startSpeedRange_beginCeiling = new Vector2( 1.0f, 1.4f );

    public Vector2 geyser_geyserDuration_toAddMoreParticles_range = new Vector2( 1.5f, 4.5f );
        // the following 3 params work in conjunction
    public Vector2 geyser_durationBetweenGeysers_range_1void = new Vector2( 7.0f, 25.0f );
        // until globalEvolutionState = lowerAndRougher, the makeDiscsModel only changes harmony when all lightDiscs are not busy
        // then, with more voids open, we need to increase the time between geysers to increase the chance there are no lightDiscs being hit by geysers, so the harmony can change
        // after the first open void, for every additional open void, the following params add an additional N seconds to min and max values of Vector2 geyser_durationBetweenGeysers_range_1void
            // e.g. if 3 voids, new Vector2( geyser_durationBetweenGeysers_range_1void[0] + secAddedToMin * 2, geyser_durationBetweenGeysers_range_1void[1] + secAddedToMax * 2 )   
    public float   geyser_durationBetweenGeysers_secAddedToMin_perAdditionalVoid = 2.5f;
    public float   geyser_durationBetweenGeysers_secAddedToMax_perAdditionalVoid = 3.5f;
    // scale particleRate to lfo dynamics:
    public Vector2 geyser_particlesRateRange = new Vector2( 2.3f, 12.0f );
    public Vector2 geyser_lfoDynamicsRange = new Vector2( .3f, .45f );
    public Vector2 geyser_stopB4Ceiling_durationUntilStopRange = new Vector2( 3.5f, 5.0f );
    public float   geyser_stopB4CeilingProb_begin = 1f;
    public float   geyser_stopB4CeilingProb_beginCeiling = .3f;

    // ****
    // lightDiscs
        // model script - the model generates the basic fund and partials and the disc script 
        // animates that by adding extra pitches for shimmer or spin sounds ( proportion or critical band )
    public float   model_fund_amp = 1f;
    public float   model_fund_decay = .8f;
    public Vector2 model_partials_ampsRange = new Vector2( .0001f, .0005f );
    public float   model_partials_decay = 1f;
    public Vector2 model_harmonyDurRange_lowerAndRougher = new Vector2( 6f, 10f );
        // lightDiscsMovement script
    public float   disc_lightFlareEvent_ampGlobal_begin = .6f;
    public float   disc_lightFlareEvent_ampGlobal_beginSpin = 1.1f;
            // lowerAndRougher ampGlobal set per harmony in the ScoreDiscs, but if too loud, can be globally reduced with these params:
    public float   disc_lightFlareEvent_ampGlobal_scaleValuesInScore_lowerAndRougher = .45f;
    public float   disc_lightSpinEvent_ampGlobal_scaleValuesInScore_lowerAndRougher  = .35f;
    public Vector2 disc_lightFlareEvent_durationRangePerSwell_begin = new Vector2( 3f, 7f );
    public float   disc_lightSpinEvent_prob_begin = 0;
    public float   disc_lightSpinEvent_prob_beginSpin = .6f;
    public float   disc_lightSpinEvent_attack_ampGlobal    = .04f;
    public float   disc_lightSpinEvent_attack_prob         = .85f;
    public float   disc_lightSpinEvent_resonance_ampGlobal = .55f;
    // scaled from spin rate:
    public Vector2 disc_lightSpinEvent_fund_percentOfCriticalBandRange = new Vector2( 0.005f, .085f );
        // ^^ for uppers, for each progression, we'll use the lightFlare shimmer range from the score
    public Vector2 disc_flare_randAmp_softerRange = new Vector2( .01f, .05f );
    public Vector2 disc_flare_randAmp_louderRange = new Vector2( .8f, 1.0f );
    public Vector2 disc_spin_randAmp_softerRange  = new Vector2( .0001f, .0005f ); // <-- applies to upper partials
    public Vector2 disc_spin_randAmp_louderRange  = new Vector2( 1f, 1f ); // <-- applies to fundamental
    public Vector2 disc_randDecay_fasterRange     = new Vector2( 2.0f, 6.0f );
    public Vector2 disc_randDecay_slowerRange     = new Vector2( .2f, 1.3f );
        // lowerAndRougher flares - growAndFade values are for ampLocal; scale intensity from there - whereas above its the opposite
    public int     disc_flare_lowerAndRougher_maxNumFlares = 9; // <-- to prevent CPU overload
    public float   disc_flare_lowerAndRougher_growAndFade_probInitStayPut     = .5f;
    public Vector2 disc_flare_lowerAndRougher_growAndFade_targetFadeRange     = new Vector2( .3f, .5f );
    public Vector2 disc_flare_lowerAndRougher_growAndFade_targetGrowRange     = new Vector2( .6f, 1f );
    public Vector2 disc_flare_lowerAndRougher_growAndFade_durToFadeRange      = new Vector2( .5f, 4f );
    public Vector2 disc_flare_lowerAndRougher_growAndFade_durToGrowRange      = new Vector2( .3f, 4f );
    public Vector2 disc_flare_lowerAndRougher_growAndFade_durToStayFadedRange = new Vector2( 1f, 2f );
    public Vector2 disc_flare_lowerAndRougher_growAndFade_durToStayGrownRange = new Vector2( 4f, 6f );

    // ****
    // upsideDown && rightsideUp
    public float   upsideDown_ampGlobal_begin = 4f;
    public float   upsideDown_ampGlobal_lowerAndRougher = 1.5f;
        // the threshold away from the floorCenter that demarcates what is distant vs close
        // some params below change when crossing this thresh, others only scale on one side or the other ( distant or close ), and others scale from the entire distance away from center regardless of this threshold
        // thus how far away this distance is effects when we perceive an aural change of distant vs close sounds when zooming with the mouse
    public float   upsideDown_distantOrClose_distFromCenter = 8f;
    /* design for distant vs close upsideDown params:
         stutter:
            repeatprob     = just switch when go from distant to close: line message - constantly runs between values for variety       
            shiftamt       = just switch when go from distant to close
            dropoutprob    = a constant value while distant ( 0 ), and scaled when close from distantOrClose_distFromCenter to floor center
            feedback       = constantly scaled from distant to floor center close
         all reverb params = just switch, but 1 setting for distant and for close it constantly switches between two
         filter freq       = constantly scaled from distant to floor center close 
         all flange params = for both distant and close - constant values that don't change for either       
    */
    public float   upsideDown_flange_rate  = .1f;
    public float   upsideDown_flange_depth = 30f;
    public Vector2 upsideDown_distant_stutter_repeatprobRange = new Vector2( .09f, .35f );
    public float   upsideDown_distant_stutter_shiftamt = 1.01f; // 1 is no pitch shifting
    public float   upsideDown_distant_stutter_feedback = 1.7f;
    public float   upsideDown_distant_stutter_dropoutprob = 0f;
    public float   upsideDown_distant_reverb_roomsize = 0f;
    public float   upsideDown_distant_reverb_decay = 127;
    public float   upsideDown_distant_reverb_damping = 127;
    public float   upsideDown_distant_reverb_diffusion = 0f;
    public float   upsideDown_distant_filter_freq = 100f;
    public Vector2 upsideDown_close_stutter_repeatprobRange = new Vector2( .09f, .55f );
    public float   upsideDown_close_stutter_shiftamt = 1.025f;
    public float   upsideDown_close_stutter_feedback = 0f;
    public float   upsideDown_close_stutter_dropoutprob = .2f; 
        // each reverb setting corresponds to an index of a vector2
        // I think only the first one is used?
    public Vector2 upsideDown_close_2reverbs_roomsize  = new Vector2( 65f, 0f );
    public Vector2 upsideDown_close_2reverbs_decay     = new Vector2( 0f, 127f );
    public Vector2 upsideDown_close_2reverbs_damping   = new Vector2( 0f, 127f );
    public Vector2 upsideDown_close_2reverbs_diffusion = new Vector2( 127f, 0f );
    public float   upsideDown_close_filter_freq = 847f;
        // when rotating around in the upside down...
        // in 1/2 steps deviation from recorded pitch level
    public Vector2 upsideDown_transpRange = new Vector2( -1.25f, 1.25f );
        // responsible for how severely the rotation rate transposes the sound / how "quickly" you transpose while rotating
    public Vector2 upsideDown_rotation_velocityY_scaledToTranspDeltaRange = new Vector2( -.04f, .04f );
    public Vector2 upsideDown_downsamp_intervalRange = new Vector2( 12f, 12.5f );
    public float   upsideDown_distToVoidCentroid_delayOn = 4f; // <-- if further away, no delay w feedback
        // max engine is set up to detect amp being too high; and to then bring delay back to the bottom of this range for safety:
    public float   upsideDown_delay_ampGlobal = .005f; // <-- this feedback is powerful stuff, so amp should be low
    public float   upsideDown_delay_feedbackMultMin = .84f;
    public Vector2 upsideDown_delay_feedbackMultMaxRangeLow  = new Vector2( 1.3f, 2.0f ); // <-- every time the delay is turned back on, we get a new max so have variation with the feedback explosion
    public Vector2 upsideDown_delay_feedbackMultMaxRangeHigh = new Vector2( 2.5f, 4.5f ); 
    public float   upsideDown_delay_feedbackMult_probRangeLow = .7f;
    public float   upsideDown_delay_feedbackMult_ampLimiterThresh = 0.0015f; // <-- determines how far the feedback can go before sets the multiplier back to something reasonable - limits the explosion
    public float   upsideDown_bg_ampGlobal = .1f; // mixed quietly with upsideDown but without the audio effects of upsideDown 
    public float   rightsideUp_distToVoidCentroid_distort = 6.5f; // <-- less than this, begins to warp spacetime
    public float   rightsideUp_distToVoidCentroid_suck = 3f; // <-- less than this, begins to suck camera into void
    public float   rightsideUp_approachingVoid_stutter_shiftamt_maxDeviateFrom1 = .01f;
    public Vector2 rightsideUp_approachingVoid_stutter_feedbackRange = new Vector2( 1f, 1.82f );
    public Vector2 rightsideUp_approachingVoid_stutter_repeatprobRange = new Vector2( .09f, .35f ); // sent on start; oscillatess between values by line
    public Vector2 rightsideUp_approachingVoid_reverb_roomsizeRange  = new Vector2( 0f, 110f );
    public Vector2 rightsideUp_approachingVoid_reverb_decayRange     = new Vector2( 0f, 110f );
    public Vector2 rightsideUp_approachingVoid_reverb_dampingRange   = new Vector2( 0f, 110f );
    public Vector2 rightsideUp_approachingVoid_reverb_diffusionRange = new Vector2( 0f, 110f );
}
