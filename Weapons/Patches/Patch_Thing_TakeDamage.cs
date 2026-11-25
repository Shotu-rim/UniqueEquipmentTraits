using HarmonyLib;
using System;
using UniqueEquipmentTraits.Core;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Records melee contact before armor mitigation and
    /// preserves existing post-armor damage tracking.
    /// Nested damage calls do not contribute additional
    /// pre-armor contact damage.
    /// </summary>
    [HarmonyPatch(typeof(Thing), nameof(Thing.TakeDamage))]
    internal static class Patch_Thing_TakeDamage
    {
        internal sealed class ContactFrame
        {
            internal readonly MeleeAttackContext Context;
            internal readonly ContactFrame Previous;

            internal ContactFrame(
                MeleeAttackContext context,
                ContactFrame previous)
            {
                Context = context;
                Previous = previous;
            }
        }

        [ThreadStatic]
        private static ContactFrame activeContact;

        [HarmonyPrefix]
        private static void Prefix(
            Thing __instance,
            DamageInfo dinfo,
            out ContactFrame __state)
        {
            __state = null;

            MeleeAttackContext context =
                Patch_Verb_MeleeAttackDamage.CurrentContext;

            if (context == null ||
                context.ContactObservationClosed ||
                context.Attacker == null ||
                context.Victim == null ||
                context.Weapon == null)
                return;

            if (__instance != context.Victim ||
                dinfo.Instigator != context.Attacker ||
                dinfo.Amount <= 0f)
                return;

            bool isRootContact = true;

            ContactFrame previous = activeContact;

            while (previous != null)
            {
                if (ReferenceEquals(
                    previous.Context, context))
                {
                    isRootContact = false;
                    break;
                }

                previous = previous.Previous;
            }

            ContactFrame frame =
                new ContactFrame(
                    context,
                    activeContact);

            activeContact = frame;
            __state = frame;

            if (!isRootContact)
                return;

            context.RegisterContactDamage(dinfo.Amount);
            context.RegisterContactPart(dinfo.HitPart);
        }

        [HarmonyPostfix]
        private static void Postfix(
            Thing __instance,
            DamageInfo dinfo,
            DamageWorker.DamageResult __result,
            ContactFrame __state)
        {
            MeleeAttackContext context =
                Patch_Verb_MeleeAttackDamage.CurrentContext;

            if (context == null ||
                context.Attacker == null ||
                context.Victim == null ||
                __instance != context.Victim ||
                dinfo.Instigator != context.Attacker ||
                __result == null)
                return;

            // Contact parts include fully absorbed hits.
            if (__state != null &&
                ReferenceEquals(__state.Context, context))
            {
                RegisterContactParts(context, __result);
            }

            // Preserve existing post-armor damage tracking.
            if (__result.totalDamageDealt <= 0f)
                return;

            context.RegisterDamage(
                __result.totalDamageDealt);

            RegisterHitParts(context, __result);
        }

        private static void RegisterContactParts(
            MeleeAttackContext context,
            DamageWorker.DamageResult result)
        {
            if (result.parts != null)
            {
                foreach (BodyPartRecord part in result.parts)
                {
                    context.RegisterContactPart(part);
                }
            }

            if (result.hediffs != null)
            {
                foreach (Hediff hediff in result.hediffs)
                {
                    if (hediff != null)
                    {
                        context.RegisterContactPart(
                            hediff.Part);
                    }
                }
            }
        }

        private static void RegisterHitParts(
            MeleeAttackContext context,
            DamageWorker.DamageResult result)
        {
            if (result.parts != null)
            {
                foreach (BodyPartRecord part in result.parts)
                {
                    context.RegisterHitPart(part);
                }
            }

            if (result.hediffs != null)
            {
                foreach (Hediff hediff in result.hediffs)
                {
                    if (hediff != null)
                    {
                        context.RegisterHitPart(
                            hediff.Part);
                    }
                }
            }
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(
            Exception __exception,
            ContactFrame __state)
        {
            if (__state != null &&
                ReferenceEquals(activeContact, __state))
            {
                activeContact = __state.Previous;
            }

            return __exception;
        }
    }
}