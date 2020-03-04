using Verse;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using System;
using System.Reflection;

namespace RubberBulletsMod {
    public class RubberBulletsMod : Mod {
        public readonly static string Label = "Using Rubber Bullets";
        public readonly static string LabelNoSpaces = "UsingRubberBullets";
        public static KeyBindingDef ToggleRubberBullets = KeyBindingDef.Named("Toggle Rubber Bullets");
        private readonly string tooltip = "Enables rubber bullets.These do blunt damage and have less damage at range than standard bullets do. (Can toggle with \"\\\")";
        
        public RubberBulletsMod(ModContentPack content) : base(content) {
            Harmony harmony = new Harmony("fyarn.RubberBulletsMod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect SectionRect) {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(SectionRect);
            listingStandard.CheckboxLabeled(Label, ref Settings.usingRubberBullets, tooltip);
            listingStandard.End();
            base.DoSettingsWindowContents(SectionRect);
        }


        public override string SettingsCategory() {
            return LabelNoSpaces;
        }
        
        // Harmony Prefix
        [StaticConstructorOnStartup]
        public class RubberBulletsModPatch {
            static RubberBulletsModPatch() {
                Harmony harmony = new Harmony("fyarn.RubberBulletsMod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }

            [HarmonyPatch(typeof(Thing), "TakeDamage")]
            class RubberBullet {
                [HarmonyPrefix]
                static void Prefix(Thing __instance, ref DamageInfo dinfo) {
                    try {
                        if (__instance == null ||
                            dinfo.Def == null ||
                            dinfo.Instigator == null ||
                            __instance.Position == null ||
                            dinfo.Weapon.Verbs == null ||
                            dinfo.Weapon.Verbs.Count == 0) {
                            return;
                        }

                        if (Settings.usingRubberBullets &&
                            dinfo.Instigator.Faction.IsPlayer &&
                            dinfo.Def == DamageDefOf.Bullet) {
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
}
