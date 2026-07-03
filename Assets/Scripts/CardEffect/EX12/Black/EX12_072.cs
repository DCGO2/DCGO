using System.Collections;
using System.Collections.Generic;

// Metal Empire
namespace DCGO.CardEffects.EX12
{
    public class EX12_072 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Use Req
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("ME");
                }
            }
            #endregion

            #region All Turns Security
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsTraits("ME");
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(cardSource)
                        && cardSource.Owner == card.Owner
                        && cardSource == cardSource.PermanentOfThisCard().TopCard
                        && PermanentCondition(cardSource.PermanentOfThisCard());
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistInSecurity(card, false);
                }

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                {
                    if (_timing == EffectTiming.WhenRemoveField)
                    {
                        bool Condition()
                        {
                            return true;
                        }

                        cardEffects.Add(CardEffectFactory.GuardSelfEffect(false, cardSource, Condition));
                    }

                    return cardEffects;
                }

                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("Your Digimon gain Guard", CanUseCondition, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects, limitTiming: EffectTiming.WhenRemoveField);
                cardEffects.Add(addSkillClass);
            }
            #endregion

            #region Main Effect
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Replace your bottom sec with this face-up card", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] Add your bottom security card to the hand and place this card face up as the bottom security card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectFactory.ReplaceBottomSecurityWithFaceUpOptionEffect(card, activateClass));
                }
            }
            #endregion

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 lvl 5 or lower [ME] Digimon card from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                 => "[Security] You may play 1 level 5 or lower [ME] trait Digimon card from your hand without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                bool CanPlayCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel
                        && cardSource.Level <= 5
                        && cardSource.EqualsTraits("ME")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectHandEffect selectHandEffect2 = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect2.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanPlayCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.PlayForFree,
                        cardEffect: activateClass);

                    yield return StartCoroutine(selectHandEffect2.Activate());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
