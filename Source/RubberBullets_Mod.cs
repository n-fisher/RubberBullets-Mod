using Verse;
using Harmony;
using RimWorld;
using UnityEngine;
using HugsLib;
using System;

namespace RubberBullets_Mod
{
    public class RubberBullets_Mod : ModBase
    {
        internal const string ModId = "RubberBullets_Mod";
        private bool usingRubberBullets;
        public static KeyBindingDef ToggleRubberBullets = KeyBindingDef.Named("ToggleRubberBullets");
        private static RubberBullets_Mod instance = null;

        private RubberBullets_Mod()
        {
            instance = this;
            usingRubberBullets = false;
        }

        public static RubberBullets_Mod Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new RubberBullets_Mod();
                }
                return instance;
            }
        }
        
        public bool UsingRubberBullets
        {
            get => usingRubberBullets;
            set => usingRubberBullets = value;
        }

        public override string ModIdentifier
        {
            get => ModId;
        }


        public override void OnGUI()
        {
            if (Event.current.type != EventType.KeyDown || Event.current.keyCode == KeyCode.None) return;
            
            if (ToggleRubberBullets.JustPressed)
            {
                toggleMod();
            }
        }

        public void toggleMod()
        {
            usingRubberBullets = !usingRubberBullets;
            Messages.Message("Rubber bullets turned " + (usingRubberBullets ? "on" : "off") + ".", MessageSound.Silent);
        }

        [HarmonyPatch(typeof(Thing), "TakeDamage")]
        class RubberBullet
        {
            [HarmonyPrefix]
            public static void Impact_Patch(Thing __instance, ref DamageInfo dinfo)
            {
                try
                {
                    DamageInfo d = new DamageInfo(DamageDefOf.Blunt, dinfo.Amount, dinfo.Angle, dinfo.Instigator, dinfo.ForceHitPart, dinfo.WeaponGear, dinfo.Category);
                    if (RubberBullets_Mod.Instance.UsingRubberBullets && dinfo.Instigator.Faction.IsPlayer)
                    {
                        dinfo = d;
                        float distance = IntVec3Utility.DistanceTo(dinfo.Instigator.Position, __instance.Position);
                        float range = dinfo.WeaponGear.Verbs[dinfo.WeaponGear.Verbs.Count - 1].range;
                        float damageScalingByDistance = 0.5f * distance / range;
                        dinfo.SetAmount((int)Math.Round(((float)dinfo.Amount) - ((float) dinfo.Amount) * damageScalingByDistance));
                        RubberBullets_Mod.Instance.Logger.Message("Bullet with range " + range + " at distance " + distance + " did " + dinfo.Amount + " damage.");
                    }
                }
                catch (Exception e)
                {
                    //Catches exceptions from not found factions
                }
            }
        }
    }
}
