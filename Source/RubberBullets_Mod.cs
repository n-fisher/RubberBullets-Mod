using Verse;
using Harmony;
using RimWorld;
using UnityEngine;
using HugsLib;
using System;
using HugsLib.Settings;
using System.Reflection;

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
            HarmonyInstance.Create(ModId).PatchAll(Assembly.GetExecutingAssembly());

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
            Messages.Message("Rubber bullets turned " + (UsingRubberBullets ? "on" : "off") + ".", MessageTypeDefOf.SilentInput);
        }

        [HarmonyPatch(typeof(Thing), "TakeDamage")]
        class RubberBullet
        {
            [HarmonyPrefix]
            static void Prefix(Thing __instance, ref DamageInfo dinfo)
            {
                try {
                    if (__instance == null || dinfo.Def == null || dinfo.Instigator == null || __instance.Position == null
                        || dinfo.Weapon.Verbs == null || dinfo.Weapon.Verbs.Count == 0) {
                        return;
                    }
                    if (Instance.UsingRubberBullets && dinfo.Instigator.Faction.IsPlayer && dinfo.Def == DamageDefOf.Bullet) {
                        float distance = IntVec3Utility.DistanceTo(dinfo.Instigator.Position, __instance.Position);
                        float range = dinfo.Weapon.Verbs[0].range;
                        float damageScalingByDistance = 0.5f * distance / range;
                        dinfo = new DamageInfo(DamageDefOf.Blunt, dinfo.Amount, 0, dinfo.Angle, dinfo.Instigator, dinfo.HitPart, dinfo.Weapon, dinfo.Category, dinfo.IntendedTarget);
                        dinfo.SetAmount((float) Math.Round(dinfo.Amount - dinfo.Amount * damageScalingByDistance));
                    }
                } catch (Exception e) { Log.Error(e.ToStringSafe()); } //catch melee NullReference exceptions
            }
        }
    }
}
