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

        [HarmonyPatch(typeof(Verb), "WarmupComplete")]
        class RubberBullet
        {
            [HarmonyPrefix]
            public static void Impact_Patch(Verb __instance)
            {
                try
                {
                    if (__instance.CasterPawn == null || __instance.CasterPawn.Faction == null || __instance.verbProps == null) return;
                    if (RubberBullets_Mod.Instance.UsingRubberBullets && __instance.CasterPawn.Faction.Equals(Faction.OfPlayer))
                    {
                        __instance.verbProps.projectileDef.projectile.damageDef = DamageDefOf.Blunt;
                        __instance.verbProps.projectileDef.projectile.damageDef.label = "Rubber Bullet";
                    }
                }
                catch (Exception e)
                {
                    Messages.Message("RubberBullets exception logged. You shouldn't see this so bug the dev!", MessageSound.Silent);
                }
            }
        }
    }
}
