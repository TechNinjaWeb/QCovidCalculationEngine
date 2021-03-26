﻿// QCovid® Calculation Engine is Copyright © 2020 Oxford University Innovation Limited.
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 
// PLEASE NOTE:
// In its compiled form, QCovid@ Calculation Engine is a Class I Medical Device and
// is covered by the Medical Device Regulations 2002 (as amended).
// 
// Modification of the source code and subsequently placing that modified code on the market
// may make that person/entity a legal manufacturer of a medical device and so
// subject to the requirements listed in Medical Device Regulations 2002 (as amended).
// 
// Failure to comply with these regulations (for example, failure to comply with the relevant
// registration requirements or failure to meet the relevant essential requirements)
// may result in prosecution and a penalty of an unlimited fine and/or 6 months’ imprisonment.
// 
// This source code version of QCovid® Calculation Engine is provided as is, and
// has not been certified for clinical use, and must not be used for supporting or informing clinical decision-making.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using QCovid.RiskCalculator.Risk.Input;

namespace QCovid.RiskCalculator.CodeMapping.Internal.CodeGroupMappings
{
    // <summary>
    // A mapping between codes to a trigger a bool flag to true, if code instances
    // appear at least a given number of times of a given period
    // </summary>
    internal class NInstancesInPeriodBoolMapping<TParent> : BoolMapping<TParent>
        where TParent : class
    {
        private readonly int _minOccurrences;
        private readonly int _lookBackMonths;

        public NInstancesInPeriodBoolMapping(IReadOnlyList<int> codeGroupIds, int minOccurrences, int lookBackMonths, Expression<Func<ClinicalInformation, TParent>> parentExpression, Expression<Func<TParent, bool>> childExpression) 
            : base(codeGroupIds, parentExpression, childExpression)
        {
            _minOccurrences = minOccurrences;
            _lookBackMonths = lookBackMonths;
        }

        public override string ToStringProcessingRules() => $"True if present at least {_minOccurrences} times in the preceding {_lookBackMonths} months";

        protected override void Process_Inner(RiskInput riskInput, IReadOnlyList<CodeGroupInstance> recognisedInstances, Date processingReferenceDate)
        {
            DateTime earliestDate = processingReferenceDate.Value.AddMonths(-_lookBackMonths);
            IEnumerable<CodeGroupInstance> instancesWithinPeriod = recognisedInstances.Where(cgi => cgi.Date.Value >= earliestDate);

            if (instancesWithinPeriod.Count() >= _minOccurrences)
                SetValue(riskInput);
        }
    }
}
