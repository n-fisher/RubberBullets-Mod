using Verse;
using Harmony;
using RimWorld;
using UnityEngine;
using HugsLib;
using System;
using HugsLib.Settings;

namespace RubberBullets_Mod
{
    public class RubberBullets_Mod : ModBase
    {
        internal const string ModId = "RubberBullets_Mod";
        private SettingHandle<bool> usingRubberBullets;
        public static KeyBindingDef ToggleRubberBullets = KeyBindingDef.Named("ToggleRubberBullets");
        private static RubberBullets_Mod instance = null;

        private RubberBullets_Mod()
        {
            instance = this;
        }

        public override void DefsLoaded()
        {
            usingRubberBullets = Settings.GetHandle<bool>("usingRubberBullets", "Use rubber bullets?", "Enables rubber bullets. These do blunt damage and have less damage at range than standard bullets do. (Can toggle with \"\\\")", false);
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
            set
            {
                usingRubberBullets.Value = value;
                HugsLibController.SettingsManager.SaveChanges();
            }
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
            UsingRubberBullets = !UsingRubberBullets;
            Messages.Message("Rubber bullets turned " + (UsingRubberBullets ? "on" : "off") + ".", MessageSound.Silent);
        }

        [HarmonyPatch(typeof(Thing), "TakeDamage")]
        class RubberBullet
        {
            [HarmonyPrefix]
            public static void Impact_Patch(Thing __instance, ref DamageInfo dinfo)
            {
                try
                {
                    if (__instance == null || dinfo.Def == null || dinfo.Instigator == null || __instance.Position == null
                        || dinfo.WeaponGear.Verbs == null || dinfo.WeaponGear.Verbs.Count == 0)
                    {
                        return;
                    }
                    if (RubberBullets_Mod.Instance.UsingRubberBullets && dinfo.Instigator.Faction.IsPlayer && dinfo.Def == DamageDefOf.Bullet)
                    {
                        float distance = IntVec3Utility.DistanceTo(dinfo.Instigator.Position, __instance.Position);
                        float range = dinfo.WeaponGear.Verbs[0].range;
                        float damageScalingByDistance = 0.5f * distance / range;
                        dinfo = new DamageInfo(DamageDefOf.Blunt, dinfo.Amount, dinfo.Angle, dinfo.Instigator, dinfo.ForceHitPart, dinfo.WeaponGear, dinfo.Category);
                        dinfo.SetAmount((int)Math.Round(((float)dinfo.Amount) - ((float)dinfo.Amount) * damageScalingByDistance));
                        //RubberBullets_Mod.Instance.Logger.Message("Bullet with range " + range + " at distance " + distance + " did " + dinfo.Amount + " damage.");
                    }
                }
                catch (Exception e)
                {
                    //catch melee NullReference exceptions
                }
            }
        }
    }
}
