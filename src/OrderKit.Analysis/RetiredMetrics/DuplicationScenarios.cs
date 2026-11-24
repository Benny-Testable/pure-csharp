using System;
using System.Collections.Generic;

namespace OrderKit.RetiredMetrics
{
    /// <summary>
    /// Code Quality Auditing / Code Duplication: Refactoring Opportunity Detection,
    /// Risk-Based Testing Prioritization, Maintainability Testing (retired by TP-6263).
    /// Tool: jscpd-cs. Two production clones of the same 25-line block that differ only
    /// in a constant; one is tested and one is not, so duplication risk is uneven.
    /// </summary>
    public static class DuplicationScenarios
    {
        public static decimal DomesticInvoice(IList<decimal> lines, decimal discountPercent)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            decimal subtotal = 0m;
            foreach (decimal line in lines)
            {
                if (line < 0m) throw new ArgumentException("negative line");
                subtotal += line;
            }
            decimal discount = subtotal * discountPercent / 100m;
            if (discount > subtotal) discount = subtotal;
            decimal taxable = subtotal - discount;
            decimal tax = Math.Round(taxable * 0.18m, 2);
            decimal total = taxable + tax;
            if (total < 0m) total = 0m;
            return Math.Round(total, 2);
        }

        public static decimal ExportInvoice(IList<decimal> lines, decimal discountPercent)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            decimal subtotal = 0m;
            foreach (decimal line in lines)
            {
                if (line < 0m) throw new ArgumentException("negative line");
                subtotal += line;
            }
            decimal discount = subtotal * discountPercent / 100m;
            if (discount > subtotal) discount = subtotal;
            decimal taxable = subtotal - discount;
            decimal tax = Math.Round(taxable * 0.00m, 2);
            decimal total = taxable + tax;
            if (total < 0m) total = 0m;
            return Math.Round(total, 2);
        }
    }
}
