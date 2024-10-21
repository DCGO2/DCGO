using System.Collections;
using System.Collections.Generic;

public partial class CardEffectCommons
{
    static bool CanSelectSourceCardCondition(CardSource source, CardColor color, int level, ICardEffect activateClass)
    {
        return source.IsDigimon &&
               source.CardColors.Contains(color) &&
               source.HasLevel && source.Level == level &&
               CanPlayAsNewPermanent(cardSource: source, payCost: false, cardEffect: activateClass);
    }

    #region Can activate [Decode]

    public static bool CanActivateDecode(CardColor color, int level, CardSource cardSource, ICardEffect activateClass)
    {
        return IsExistOnBattleAreaDigimon(cardSource) &&
               cardSource.PermanentOfThisCard().DigivolutionCards.Some(
                   source => CanSelectSourceCardCondition(source, color, level, activateClass));
    }

    #endregion

    #region Effect process of [Decode]

    public static IEnumerator DecodeProcess(CardColor color, int level, CardSource cardSource, ICardEffect activateClass)
    {
        List<CardSource> selectedCards = new List<CardSource>();

        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

        selectCardEffect.SetUp(
            canTargetCondition: source => CanSelectSourceCardCondition(source, color, level, activateClass),
            canTargetCondition_ByPreSelecetedList: null,
            canEndSelectCondition: null,
            canNoSelect: () => true,
            selectCardCoroutine: SelectCardCoroutine,
            afterSelectCardCoroutine: null,
            message: "Select 1 digivolution card to play.",
            maxCount: 1,
            canEndNotMax: false,
            isShowOpponent: true,
            mode: SelectCardEffect.Mode.Custom,
            root: SelectCardEffect.Root.Custom,
            customRootCardList: cardSource.PermanentOfThisCard().DigivolutionCards,
            canLookReverseCard: true,
            selectPlayer: cardSource.Owner,
            cardEffect: activateClass);

        selectCardEffect.SetUpCustomMessage(
            "Select 1 digivolution card to play.",
            "The opponent is selecting 1 digivolution card to play.");
        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

        IEnumerator SelectCardCoroutine(CardSource source)
        {
            selectedCards.Add(source);

            yield return null;
        }

        yield return ContinuousController.instance.StartCoroutine(
            PlayPermanentCards(
                cardSources: selectedCards,
                activateClass: activateClass,
                payCost: false,
                isTapped: false,
                root: SelectCardEffect.Root.DigivolutionCards,
                activateETB: true));
    }

    #endregion

    #region Target 1 Digimon gains [Decode]

    public static IEnumerator GainDecode(CardColor color, int level, Permanent targetPermanent, EffectDuration effectDuration,
        ICardEffect activateClass)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool CanUseCondition()
        {
            return IsPermanentExistsOnBattleArea(targetPermanent) &&
                   !targetPermanent.TopCard.CanNotBeAffected(activateClass);
        }

        ActivateClass decode = CardEffectFactory.DecodeEffect(
            color: color,
            level: level,
            targetPermanent: targetPermanent,
            isInheritedEffect: false,
            condition: CanUseCondition,
            rootCardEffect: activateClass,
            targetPermanent.TopCard);

        AddEffectToPermanent(
            targetPermanent: targetPermanent,
            effectDuration: effectDuration,
            card: card,
            cardEffect: decode,
            timing: EffectTiming.OnAllyAttack);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                .CreateBuffEffect(targetPermanent));
        }
    }

    #endregion
}