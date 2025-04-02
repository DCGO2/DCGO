using System;
using System.Collections;
using System.Collections.Generic;

// Gaiamon
namespace DCGO.CardEffects.BT21
{
    public class BT21_101 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region App Fusion (Globemon & Charismon)

            // To Be Implemented

            #endregion

            #region Blocker

            if (timing == EffectTiming.None) cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));

            #endregion

            #region Link +1

            if (timing == EffectTiming.None) cardEffects.Add(CardEffectFactory.ChangeSelfLinkMaxStaticEffect(card.PermanentOfThisCard().LinkedMax + 1, false, card, null));

            #endregion

            #endregion

            #region Your Turn OPT

            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend, trash top sec", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT21_101_WhenLinked");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[Your Turn] [Once Per Turn] When your Digimon get linked, by unsuspending this Digimon, trash your opponent's top security card.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                    && CardEffectCommons.IsOwnerTurn(card)
                    && CardEffectCommons.CanTriggerWhenLinked(hashtable, PermanentCondition, cardSource => true)
                    && card.PermanentOfThisCard().IsSuspended
                    && CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard());

                bool PermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool unsuspended = false;
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    selectPermanentEffect.SetUp
                        (
                        card.Owner,
                        permanent => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent) && permanent == card.PermanentOfThisCard(),
                        null,
                        null,
                        1,
                        true,
                        false,
                        SelectPermanentCoroutine,
                        null,
                        SelectPermanentEffect.Mode.UnTap,
                        activateClass
                        );
                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        if (permanent.IsSuspended) unsuspended = true;
                        yield return null;
                    }
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    if (unsuspended)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                    }
                }
            }

            #endregion

            #region WD/WA Shared

            string SharedEffectDiscription()
                => "[When Digivolving] [When Attacking] You may link 1 Digimon card with the [Appmon] trait from your hand or this Digimon's digivolution cards to 1 of your Digimon without paying the cost.";
            bool SharedCanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable)
            {
            }

            #endregion

            return cardEffects;
        }
    }
}