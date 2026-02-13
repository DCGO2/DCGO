using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region "Target effect is negated" effect.
public interface IDisableCardEffect
{
    bool IsDisabled(ICardEffect cardEffect);
}
#endregion

#region "Target permanent can not be selected by the effect" effect.
public interface ICanNotSelectBySkillEffect
{
    bool CanNotSelectBySkill(Permanent permanent, ICardEffect cardEffect);
}
#endregion

#region "Target card cannot be played" effect.
public interface ICanNotPlayCardEffect
{
    bool CanNotPlay(CardSource cardSource);
}
#endregion

#region "Ignore target card's color requirement" effect.
public interface IIgnoreColorConditionEffect
{
    bool IgnoreColorCondition(CardSource cardSource);
}
#endregion

#region "Target permanent gains Iceclad" effect
public interface IIcecladEffect
{
    bool HasIceclad(Permanent permanent);
}
#endregion

#region "Target permanent gains Rush" effect
public interface IRushEffect
{
    bool HasRush(Permanent permanent);
}
#endregion

#region "Target permanent gains Reboot" effect
public interface IRebootEffect
{
    bool HasReboot(Permanent permanent);
}
#endregion

#region "Target permanent gains Alliance" effect
public interface IAllianceEffect
{
    bool HasAlliance(Permanent permanent);
}
#endregion

#region "Target permanent gains Scapegoat" effect
public interface IScapegoatEffect
{
    bool HasScapegoat(Permanent permanent);
}
#endregion

#region "Treat target permanent as Digimon" effect
public interface ITreatAsDigimonEffect
{
    bool IsDigimon(Permanent permanent);
}
#endregion

#region "Change target card's colors" effect
public interface IChangeCardColorEffect
{
    List<CardColor> GetCardColors(List<CardColor> CardColors, CardSource cardSource);
}
#endregion

#region "Change target card's origin colors" effect
public interface IChangeBaseCardColorEffect
{
    List<CardColor> GetBaseCardColors(List<CardColor> BaseCardColors, CardSource cardSource);
}
#endregion

#region "Target card gains additional effects" effect
public interface IAddSkillEffect
{
    List<ICardEffect> GetCardEffect(CardSource card, List<ICardEffect> GetCardEffects, EffectTiming timing);
}
#endregion

#region "Target card cannot be affected by effects" effect
public interface ICanNotAffectedEffect
{
    bool CanNotAffect(CardSource cardSource, ICardEffect cardEffect);
}
#endregion

#region "Target card cannot be trashed from digivolution cards" effect
public interface ICanNotTrashFromDigivolutionCardsEffect
{
    bool CanNotTrashFromDigivolutionCards(CardSource cardSource, ICardEffect cardEffect);
}
#endregion

#region "Change target permanent's Security Attack" effect
public interface IChangeSAttackEffect
{
    int GetSAttack(int SAttack, Permanent permanent, int invertValue);
    CalculateOrder isUpDown();
    bool PermanentCondition(Permanent permanent);
}
#endregion

#region "Change target permanent's Link Max" effect
public interface IChangeLinkMaxEffect
{
    int GetLinkMax(int linkMax, Permanent permanent, int invertValue);
    CalculateOrder isUpDown();
    bool PermanentCondition(Permanent permanent);
}
#endregion

#region "Invert target permanent's Security Attack" effect
public interface IInvertSAttackEffect
{
    int InversionValue(Permanent permanent, int invertValue);
    bool PermanentCondition(Permanent permanent);
}
#endregion

#region "Change target card's card names" effect
public interface IChangeCardNamesEffect
{
    List<string> ChangeCardNames(List<string> CardNames, CardSource cardSource);
}
#endregion

#region "Change target card's origin card names" effect
public interface IChangeBaseCardNameEffect
{
    List<string> ChangeBaseCardNames(List<string> BaseCardNames, CardSource cardSource);
}
#endregion

#region "Change target card's card names for DigiXros" effect
public interface IChangeCardNamesForDigiXrosEffect
{
    List<string> ChangeCardNamesForDigiXros(List<string> CardNames, CardSource cardSource);
}
#endregion

#region "Change target card's card level for Assembly" effect
public interface IChangeCardLevelForAssemblyEffect
{
    List<int> ChangeCardLevelForAssembly(List<int> levels, CardSource cardSource);
}
#endregion

#region "Change target card's traits" effect
public interface IChangeTraitsEffect
{
    List<string> ChangTraits(List<string> Traits, CardSource cardSource);
}
#endregion

#region "Target permanent cannot unsuspend" effect
public interface ICanNotUnsuspendEffect
{
    bool CanNotUnsuspend(Permanent permanent);
}
#endregion

#region "Target permanent cannot suspend" effect
public interface ICanNotSuspendEffect
{
    bool CanNotSuspend(Permanent permanent);
}
#endregion

#region "Target permanent cannot return to hand" effect
public interface ICannotReturnToHandEffect
{
    bool CannotReturnToHand(Permanent permanent, ICardEffect cardEffect);
}
#endregion

#region "Target permanent cannot return to deck" effect
public interface ICannotReturnToLibraryEffect
{
    bool CannotReturnToLibrary(Permanent permanent, ICardEffect cardEffect);
}
#endregion

#region "Target permanent cannot affected by DP minus effect" effect
public interface IImmuneFromDPMinusEffect
{
    bool ImmuneFromDPMinus(Permanent permanent, ICardEffect cardEffect);
}
#endregion

#region "Target permanent cannot affected by De-Digivolve" effect
public interface IImmuneFromDeDigivolveEffect
{
    bool ImmuneDeDigivolve(Permanent permanent);
}
#endregion

#region "Target permanent cannot have its stack cards trashed" effect
public interface IImmuneFromStackTrashingEffect
{
    bool ImmuneStackTrashing(Permanent permanent, ICardEffect effect);
}
#endregion

#region "Target permanent does not have DP" effect
public interface IDontHaveDPEffect
{
    bool DontHaveDP(Permanent permanent);
}
#endregion

#region "Change target permanent's DP" effect
public interface IChangeDPEffect
{
    int GetDP(int DP, Permanent permanent);
    bool PermanentCondition(Permanent permanent);
    bool IsUpDown();
    bool IsMinusDP();
}
#endregion

#region "Change target permanent's origin DP" effect
public interface IChangeBaseDPEffect
{
    int GetDP(int DP, Permanent permanent);
    bool PermanentCondition(Permanent permanent);
    bool IsUpDown();
    bool IsMinusDP();
}
#endregion

#region "Change target card's DP" effect
public interface IChangeCardDPEffect
{
    int GetDP(int DP, CardSource cardSource);
    bool CardCondition(CardSource cardSource);
    bool IsUpDown();
    bool IsMinusDP();
}
#endregion

#region "Change target card's cost" effect
public interface IChangeCostEffect
{
    int GetCost(int cost, CardSource cardSource, SelectCardEffect.Root root, List<Permanent> targetPermanents);
    bool CardCondition(CardSource cardSource);
    bool IsUpDown();
    bool IsCheckAvailability();
    bool IsChangePayingCost();
}
#endregion

#region "Change the maximum DP of DP-based deletion effect" effect
public interface IChangeDPDeleteEffectMaxDPEffect
{
    int GetMaxDP(int maxDP, ICardEffect cardEffect);
}
#endregion

#region "Change target permanent's level" effect
public interface IChangePermanentLevelEffect
{
    int GetPermanentLevel(int level, Permanent permanent);
}
#endregion

#region "Change target card's level" effect
public interface IChangeCardLevelEffect
{
    int GetCardLevel(int level, CardSource cardSource);
}
#endregion

#region "Target attacking permanent can attack to target defending permanent" effect
public interface ICanAttackTargetDefendingPermanentEffect
{
    bool CanAttackTargetDefendingPermanent(Permanent Attacker, Permanent Defender, ICardEffect cardEffect);
}
#endregion

#region "Target attacking permanent cannot attack to target defending permanent" effect
public interface ICanNotAttackTargetDefendingPermanentEffect
{
    bool CanNotAttackTargetDefendingPermanent(Permanent Attacker, Permanent Defender);
}
#endregion

#region "Target Security Digimon card does not battle" effect
public interface IDontBattleSecurityDigimonEffect
{
    bool DontBattleSecurityDigimon(CardSource cardSource);
}
#endregion

#region "Target defending permanent cannot block target attacking permanent" effect"
public interface ICannotBlockEffect
{
    bool CannotBlock(Permanent AttackingPermanent, Permanent BlockingPermanent);
}
#endregion

#region "Target permanent gets additional digivolution condition" effect
public interface IAddDigivolutionRequirementEffect
{
    int GetEvoCost(Permanent permanent, CardSource cardSource, bool checkAvailability);
}
#endregion

#region "Target player cannot gain Memory by effects" effect
public interface ICannotAddMemoryEffect
{
    bool cannotAddMemory(Player player, ICardEffect cardEffect);
}
#endregion

#region "Target player cannot reduce digivolution costs" effect
public interface ICannotReduceCostEffect
{
    bool CannotReduceCost(Player player, List<Permanent> targetPermanents, CardSource cardSource);
}
#endregion

#region "Target player cannot ignore digivolution condition" effect
public interface ICannotIgnoreDigivolutionConditionEffect
{
    bool cannotIgnoreDigivolutionCondition(Player player, Permanent targetPermanent, CardSource cardSource);
}
#endregion

#region "Target player cannot add Security" effect
public interface ICannotAddSecurityEffect
{
    bool cannotAddSecurity(Player player, ICardEffect cardEffect);
}
#endregion

#region "Target Digimon can be suspended by Digisorption" effect
public interface ICanSuspendByDigisorptionEffect
{
    bool canSuspendDigisorption(Permanent permanent, ICardEffect cardEffect);
    bool isCheckAvailability();
}
#endregion

#region "Target permanent cannot digivolve into target card" effect"
public interface ICanNotDigivolveEffect
{
    bool CanNotEvolve(Permanent permanent, CardSource cardSource);
}
#endregion

#region "Target card cannot be played as a new permanent" effect
public interface ICanNotPutFieldEffect
{
    bool CanNotPutField(CardSource cardSource, ICardEffect cardEffect);
}
#endregion

#region "Target card cannot be moved" effect
public interface ICanNotMoveEffect
{
    bool CanNotMove(CardSource cardSource, ICardEffect cardEffect);
}
#endregion

#region "Target card gains DNA digivolution conditions" effect
public interface IAddJogressConditionEffect
{
    JogressCondition GetJogressCondition(CardSource cardSource);
}
#endregion

#region "Target card gains DigiXros conditions" effect
public interface IAddDigiXrosConditionEffect
{
    DigiXrosCondition GetDigiXrosCondition(CardSource cardSource);
}
#endregion

#region "Target card gains Assembly conditions" effect
public interface IAddAssemblyConditionEffect
{
    AssemblyCondition GetAssemblyCondition(CardSource cardSource);
}
#endregion

#region "Target card gains Burst digivolution conditions" effect
public interface IAddBurstDigivolutionConditionEffect
{
    BurstDigivolutionCondition GetBurstDigivolutionCondition(CardSource cardSource);
}
#endregion

#region "Target card gains Link conditions" effect
public interface IAddLinkConditionEffect
{
    LinkCondition GetLinkCondition(CardSource cardSource);
}
#endregion

#region "Target card gains App Fusion digivolution conditions" effect
public interface IAddAppFusionConditionEffect
{
    AppFusionCondition GetAppFusionCondition(CardSource cardSource);
}
#endregion

#region "Add the maximum number of cards that can be selected from trash in DigiXros" effect
public interface IAddMaxTrashCountDigiXrosEffect
{
    int GetMaxTrashCount(CardSource cardSource);
}
#endregion

#region "Add the maximum number of cards that can be selected from under Tamer in DigiXros" effect
public interface IAddMaxUnderTamerCountDigiXrosEffect
{
    int getMaxUnderTamerCount(CardSource cardSource);
}
#endregion

#region "Target card be selected in DigiXros" effect
public interface ICanSelectDigiXrosEffect
{
    bool CanSelect(CardSource cardSource, Permanent permanent);
}
#endregion

#region "Target card be selected in Assembly" effect
public interface ICanSelectAssemblyEffect
{
    bool CanSelect(CardSource cardSource, Permanent permanent);
}
#endregion

#region "Target permanent's attack target cannot be switched" effect"
public interface ICanNotSwitchAttackTargetEffect
{
    bool CanNotBeSwitchAttackTarget(Permanent permanent);
}
#endregion

#region "Target permanent cannot be deleted" effect"
public interface ICanNotBeDestroyedEffect
{
    bool CanNotBeDestroyed(Permanent permanent);
}
#endregion

#region "Target permanent cannot be deleted by battle" effect"
public interface ICanNotBeDestroyedByBattleEffect
{
    bool CanNotBeDestroyedByBattle(Permanent permanent, Permanent AttackingPermanent, Permanent DefendingPermanent, CardSource DefendingCard);
    bool PermanentCondition(Permanent permanent);
}
#endregion

#region "Target permanent cannot be deleted by effect" effect"
public interface ICanNotBeDestroyedBySkillEffect
{
    bool CanNotBeDestroyedBySkill(Permanent permanent, ICardEffect cardEffect);
}
#endregion

#region "Target permanent cannot be removed" effect"
public interface ICanNotBeRemovedEffect
{
    bool CanNotBeRemoved(Permanent permanent);
}
#endregion

#region "Target permanent gains Blocker" effect
public interface IBlockerEffect
{
    bool IsBlocker(Permanent permanent);
}
#endregion

#region "Add levels for DNA digivolution" effect
public interface IAddJogressLevelsEffect
{
    List<int> GetJogressLevels(CardSource cardSource, Permanent permanent);
}
#endregion

#region "Add names for DNA digivolution" effect
public interface IAddDNANamesEffect
{
    List<string> GetDNANames(CardSource cardSource, Permanent permanent);
}
#endregion

#region "Change the min memory to end turn" effect
public interface IChangeEndTurnMinMemoryEffect
{
    int GetMinMemory(int minMemory);
}
#endregion

#region "Vortex may attack Players" effect
public interface IVortexCanAttackPlayersEffect
{
    bool VortexCanAttackPlayersPermanent(Permanent Attacker);
}
#endregion