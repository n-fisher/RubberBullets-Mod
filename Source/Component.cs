using Verse;
using UnityEngine;

namespace RubberBulletsMod {
    public class Component : GameComponent {
        internal KeyBindingDef toggleKey;
        public Component(Game game) => toggleKey = KeyBindingDef.Named(RubberBulletsMod.LabelNoSpaces);

        public override void GameComponentOnGUI() {
            if (Event.current?.type == EventType.KeyDown) {
                if (toggleKey.KeyDownEvent) {
                    Settings.Toggle();
                    Event.current.Use();
                }
            }
        }
    }
}