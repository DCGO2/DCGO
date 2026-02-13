using System;
using System.Collections;
using System.Collections.Generic;

//Amethyst Mandala
namespace DCGO.CardEffects.ST22
{
    public class ST22_10 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region WTFS/Security Shared

            bool SharedIsOpponentDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, SharedIsOpponentDigimon))
                {
                    Permanent selectedPermament = null;

                    #region Select Permament

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, SharedIsOpponentDigimon));
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: SharedIsOpponentDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermament = permanent;
                        yield return null;
                    }

                    selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to -9K DP", "The opponent is selecting 1 digimon to -9K DP");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    #endregion

                    if (selectedPermament != null) yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.ChangeDigimonDP(selectedPermament, -9000, EffectDuration.UntilEachTurnEnd, activateClass));
                }

            }

            #endregion

            #region When trashed from security

            if (timing == EffectTiming.OnDiscardSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("-9K DP to 1 opponent digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When effects trash this card from the security stack, 1 of your opponent's Digimon gets -9000 DP for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnTrashSelfSecurity(hashtable, cardEffect => cardEffect != null, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card)
                        && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, SharedIsOpponentDigimon);
                }
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("-9K DP to 1 opponent digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, hash => SharedActivateCoroutine(hash, activateClass), -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[Security] 1 of your opponent's Digimon gets -9000 DP for the turn.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card)
                        && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, SharedIsOpponentDigimon);
                }
            }

            #endregion

            #region Security - All Turns

            if (timing == EffectTiming.WhenRemoveField)
            {
                List<Permanent> removedPermanents = new List<Permanent>();

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing this card, 1 digimon doesn't leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[Security] [All Turns] When any of your Digimon with [Renamon], [Kyubimon], [Taomon] or [Sakuyamon] in their names would leave the battle area other than by battle, by trashing this card, 1 of those Digimon doesn't leave.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistInSecurity(card, false)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, CanSelectPermamentCondition)
                        && !CardEffectCommons.IsByBattle(hashtable);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistInSecurity(card, false);
                }

                bool CanSelectPermamentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.ContainsCardName("Renamon") ||
                            permanent.TopCard.ContainsCardName("Kyubimon") ||
                            permanent.TopCard.ContainsCardName("Taomon") ||
                            permanent.TopCard.ContainsCardName("Sakuyamon"));
                }

                bool CanSelectPermament(Permanent permanent)
                {
                    return removedPermanents.Contains(permanent);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    #region Trash Card from security

                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(card.Owner, card, activateClass).DestroySecurity());

                    #endregion

                    #region Prevent 1 Digimon Removal

                    Permanent selectedPermanent = null;
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermamentCondition));

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermamentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to prevent removal.", "The opponent is selecting 1 Digimon to prevent removal.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    if (selectedPermanent != null)
                    {
                        selectedPermanent.HideDeleteEffect();
                        selectedPermanent.HideHandBounceEffect();
                        selectedPermanent.HideDeckBounceEffect();
                        selectedPermanent.HideWillRemoveFieldEffect();

                        selectedPermanent.DestroyingEffect = null;
                        selectedPermanent.HandBounceEffect = null;
                        selectedPermanent.LibraryBounceEffect = null;
                        selectedPermanent.willBeRemoveField = false;
                    }

                    #endregion
                }
            }

            #endregion

            #region Main

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1. Then, place this card face up as the bottom security card.", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[Main] <Draw 1>. Then, place this card face up as the bottom security card.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);


                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    #region Draw 1
                    if (card.Owner.LibraryCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    }
                    #endregion

                    #region Place in security
                    if (card.Owner.CanAddSecurity(activateClass))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(
                            card, toTop: false, faceUp: true));

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                            .CreateRecoveryEffect(card.Owner));

                        yield return ContinuousController.instance.StartCoroutine(new IAddSecurity(card).AddSecurity());
                    }
                    #endregion
                }
            }

            #endregion

            return cardEffects;
        }
    }
}