using System;
using System.Collections.Generic;

namespace OrderKit.Analysis
{
    /// <summary>
    /// High-complexity fixture. ClassifyShipment is deliberately over a cyclomatic
    /// limit of 10 and a cognitive limit of 15, through real nested branch structure,
    /// so Lizard, unilyze and the Roslyn metrics analyzers all have an unambiguous
    /// outlier to report. SummariseBands is a milder second method, so the file is
    /// not a single data point.
    /// </summary>
    public static class ComplexitySample
    {
        public static (string Band, double Surcharge) ClassifyShipment(
            string region, string tier, double weightKg, bool hazardous, bool express,
            double customsValue, int destinationZone, string promo)
        {
            string band;
            double surcharge;
            if (weightKg <= 0)
            {
                return ("invalid", 0.0);
            }

            if (hazardous)
            {
                if (region == "EU")
                {
                    if (weightKg > 30) { band = "haz-heavy-eu"; surcharge = 48.0; }
                    else if (weightKg > 5) { band = "haz-mid-eu"; surcharge = 26.0; }
                    else { band = "haz-light-eu"; surcharge = 14.0; }
                }
                else if (region == "US")
                {
                    if (destinationZone >= 7) { band = "haz-remote-us"; surcharge = 55.0; }
                    else if (weightKg > 20) { band = "haz-heavy-us"; surcharge = 39.0; }
                    else { band = "haz-std-us"; surcharge = 21.0; }
                }
                else
                {
                    band = "haz-intl";
                    surcharge = 62.0;
                }
            }
            else if (express)
            {
                if (tier == "gold") surcharge = 9.0;
                else if (tier == "silver") surcharge = 13.0;
                else surcharge = 18.0;
                band = "express-" + (region ?? "xx").ToLowerInvariant();
            }
            else
            {
                if (weightKg > 50) { band = "freight"; surcharge = 34.0; }
                else if (weightKg > 10) { band = "parcel-heavy"; surcharge = 11.0; }
                else { band = "parcel"; surcharge = 4.5; }
            }

            if (customsValue > 1000 && region != "US") surcharge += 25.0;
            if (promo != null && promo.ToUpperInvariant() == "FREESHIP" && !hazardous) surcharge = 0.0;
            if (destinationZone > 6 && band != "freight") surcharge += 7.5;

            return (band, Math.Round(surcharge, 2));
        }

        public static IDictionary<string, double> SummariseBands(
            IEnumerable<(string Band, double Surcharge)> rows, double? minimum = null)
        {
            var totals = new Dictionary<string, double>();
            foreach (var (band, value) in rows)
            {
                if (minimum.HasValue && value < minimum.Value) continue;
                totals[band] = Math.Round(totals.GetValueOrDefault(band) + value, 2);
            }

            return totals;
        }
    }
}
