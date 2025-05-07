using System.Linq;


public partial class CardEffectCommons
{
    public static bool IsMaxCost(Permanent permanent, Player owner, bool IsDigimonOnly)
    {
        if (permanent == null) return false;
        if (permanent.TopCard == null) return false;
        if (permanent.TopCard.Owner != owner) return false;
        if (!IsPermanentExistsOnOwnerBattleArea(permanent, permanent.TopCard)) return false;
        if (!permanent.IsDigimon && !permanent.IsTamer) return false;
        if (!permanent.TopCard.HasPlayCost) return false;

        if (IsDigimonOnly)
        {
            if (!permanent.IsDigimon) return false;
            var costs = permanent.TopCard.Owner.GetBattleAreaDigimons()
                .Filter(x => x.TopCard.HasPlayCost)
                .Select(x => x.TopCard.GetCostItself).ToList();

            return costs.Count >= 1 && permanent.TopCard.GetCostItself == costs.Max();

        }
        else
        {
            var costs = permanent.TopCard.Owner.GetBattleAreaPermanents()
                .Filter(x => (x.IsDigimon || x.IsTamer) && x.TopCard.HasPlayCost)
                .Select(x => x.TopCard.GetCostItself).ToList();

            return costs.Count >= 1 && permanent.TopCard.GetCostItself == costs.Max();
        }
    }
}