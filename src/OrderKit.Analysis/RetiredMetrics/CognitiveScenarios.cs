using System.Collections.Generic;

namespace OrderKit.RetiredMetrics
{
    /// <summary>
    /// Readability / Maintainability / Cognitive Complexity: Risk Detection, Code Review
    /// Support, Testing Effort Prioritization, Code Understandability Analysis
    /// (retired by TP-6263). Tool: sonar-cs S3776 (cognitive complexity), lizard.
    /// Deep nesting, mixed boolean operators, break/continue and recursion each add
    /// cognitive increments; this method is well above the S3776 limit of 15.
    /// </summary>
    public static class CognitiveScenarios
    {
        public static List<string> ReconcileBatch(IList<Dictionary<string, int>> batches, int tolerance, bool strict)
        {
            var issues = new List<string>();
            for (int b = 0; b < batches.Count; b++)
            {
                Dictionary<string, int> batch = batches[b];
                if (batch == null) continue;
                foreach (KeyValuePair<string, int> line in batch)
                {
                    if (line.Value < 0)
                    {
                        if (strict || line.Value < -tolerance)
                        {
                            issues.Add("negative:" + line.Key);
                            if (issues.Count > 50) break;
                        }
                        else if (line.Key.StartsWith("ADJ") && !strict)
                        {
                            continue;
                        }
                    }
                    else if (line.Value > 10000)
                    {
                        while (issues.Count > 0 && issues[issues.Count - 1].StartsWith("large"))
                        {
                            issues.RemoveAt(issues.Count - 1);
                        }
                        issues.Add(line.Value > 100000 && strict ? "huge:" + line.Key : "large:" + line.Key);
                    }
                    else
                    {
                        switch (line.Key.Length)
                        {
                            case 0: issues.Add("empty-key"); break;
                            case 1: if (strict) issues.Add("short-key:" + line.Key); break;
                            default: break;
                        }
                    }
                }
            }
            return issues.Count > 0 && strict ? Dedupe(issues, 0) : issues;
        }

        private static List<string> Dedupe(List<string> items, int index)
        {
            if (index >= items.Count) return items;
            if (items.IndexOf(items[index]) != index) items.RemoveAt(index);
            else index++;
            return Dedupe(items, index);
        }
    }
}
