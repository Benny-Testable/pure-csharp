using System.Collections.Generic;
using System.Linq;

namespace OrderKit.Analysis
{
    /// <summary>A parcel to route. Immutable.</summary>
    public sealed record Parcel(int Id, double Weight, string Region, bool Fragile = false, bool Oversize = false);

    /// <summary>
    /// Nested-decision fixture: decisions at depth four and five, deeper than
    /// anything else in the solution. NestedPathsSampleTests takes the careful,
    /// heavy and standard lanes and leaves express-fragile and freight untaken, so
    /// path and branch coverage here land strictly between 0 and 100.
    /// </summary>
    public static class NestedPathsSample
    {
        public static IList<(string Lane, int Id)> RouteParcels(
            IEnumerable<Parcel> parcels, IEnumerable<string> regions, bool expressOnly = false)
        {
            var lanes = new List<(string, int)>();
            var regionList = regions.ToList();
            foreach (var parcel in parcels)                                 // depth 1
            {
                if (parcel.Weight > 0)                                      // depth 2
                {
                    foreach (var region in regionList)                      // depth 3
                    {
                        if (parcel.Region == region)                        // depth 4
                        {
                            if (parcel.Fragile)                             // depth 5
                            {
                                lanes.Add((expressOnly ? "express-fragile" : "careful", parcel.Id));
                            }
                            else if (parcel.Weight > 20)
                            {
                                lanes.Add((parcel.Oversize ? "freight" : "heavy", parcel.Id));
                            }
                            else
                            {
                                lanes.Add(("standard", parcel.Id));
                            }
                        }
                    }
                }
            }

            return lanes;
        }
    }

    /// <summary>
    /// Lane totals built on RouteParcels: gives the nested fixture a downstream
    /// caller, so a change to it has dependent code for change-impact analysis.
    /// </summary>
    public static class LaneSummary
    {
        public static IDictionary<string, int> LaneCounts(
            IEnumerable<Parcel> parcels, IEnumerable<string> regions, bool expressOnly = false) =>
            NestedPathsSample.RouteParcels(parcels, regions, expressOnly)
                .GroupBy(l => l.Lane)
                .ToDictionary(g => g.Key, g => g.Count());
    }
}
