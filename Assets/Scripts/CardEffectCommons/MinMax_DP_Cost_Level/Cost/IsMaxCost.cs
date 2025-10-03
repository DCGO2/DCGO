using System.Collections.Generic;
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

    public static List<Permanent> GetNonMaxCostPermanents(Player owner, bool digimonOnly = true)
    {
        var candidates = digimonOnly
            ? owner.GetBattleAreaDigimons()
            : owner.GetBattleAreaPermanents().Where(p => p.IsDigimon || p.IsTamer);

        var list = candidates
            .Where(x => x.TopCard != null && x.TopCard.HasPlayCost)
            .ToList();

        if (list.Count == 0) return new List<Permanent>();

        var maxCost = list.Max(p => p.TopCard.GetCostItself);

        return list
            .Where(p => p.TopCard.GetCostItself < maxCost)
            .ToList();
    }
}