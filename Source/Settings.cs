using RimWorld;
using Verse;

namespace RubberBulletsMod {
    public class Settings : ModSettings {
        public static bool usingRubberBullets = false;

        public static void Toggle() {
            usingRubberBullets = !usingRubberBullets;
            Messages.Message("Rubber bullets turned " + (usingRubberBullets ? "on" : "off"),  MessageTypeDefOf.SilentInput);
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref usingRubberBullets, RubberBulletsMod.LabelNoSpaces, false);
        }
    }
}
