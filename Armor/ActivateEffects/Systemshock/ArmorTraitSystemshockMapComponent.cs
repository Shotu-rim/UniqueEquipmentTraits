
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public class ArmorTraitSystemshockMapComponent
        : MapComponent
    {
        private Dictionary<
            Explosion,
            ArmorTraitSystemshockExplosionData> explosions =
            new Dictionary<
                Explosion,
                ArmorTraitSystemshockExplosionData>();

        private List<Explosion> explosionKeys;

        private List<ArmorTraitSystemshockExplosionData>
            explosionValues;

        public ArmorTraitSystemshockMapComponent(Map map)
            : base(map)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(
                ref explosions,
                "UET_SystemshockExplosions",
                LookMode.Reference,
                LookMode.Deep,
                ref explosionKeys,
                ref explosionValues);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (explosions == null)
                {
                    explosions = new Dictionary<
                        Explosion,
                        ArmorTraitSystemshockExplosionData>();
                }

                explosionKeys = null;
                explosionValues = null;
            }
        }

        public void RegisterExplosion(
            Explosion explosion,
            ArmorTraitSystemshockExplosionData data)
        {
            if (explosion == null || data == null)
                return;

            explosions[explosion] = data;
        }

        public bool TryGetExplosionData(
            Explosion explosion,
            out ArmorTraitSystemshockExplosionData data)
        {
            if (explosion == null)
            {
                data = null;
                return false;
            }

            return explosions.TryGetValue(
                explosion,
                out data);
        }

        public void RemoveExplosion(Explosion explosion)
        {
            if (explosion == null)
                return;

            explosions.Remove(explosion);
        }
    }
}
