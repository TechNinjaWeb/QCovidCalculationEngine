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
using QCovid.RiskCalculator.Exceptions.Internal;
using QCovid.RiskCalculator.Risk.Input;

namespace QCovid.RiskCalculator.CodeMapping.Internal.CodeGroupMappings
{
    internal class ChronicKidneyDiseaseMapping : EnumMapping<ClinicalInformation, ChronicKidneyDisease>
    {
        private const int DialysisLookBackMonths = 12;

        public ChronicKidneyDiseaseMapping(IReadOnlyList<(int[] ids, ChronicKidneyDisease value)> codeGroupIdValuePairs, Expression<Func<ClinicalInformation, ChronicKidneyDisease>> childExpression) 
            : base(codeGroupIdValuePairs, ri => ri.ClinicalInformation, childExpression)
        {
            const int expectedCodeCount = 5;

            if (codeGroupIdValuePairs.Count != expectedCodeCount)
                throw new InvalidCodeGroupMappingException($"There must be exactly {expectedCodeCount} mappings for name chronic kidney disease.");

            if (codeGroupIdValuePairs.Select(p => p.value).Distinct().Count() != expectedCodeCount)
                throw new InvalidCodeGroupMappingException("There should be distinct values only in the chronic kidney disease mappings.");
        }

        public override string ToStringProcessingRules() => $"{nameof(ChronicKidneyDisease.Ckd5WithTransplant)} if any match to that, or {ChronicKidneyDisease.Ckd5WithDialysis} if match in preceding {DialysisLookBackMonths} months, or CKD5, CKD4, CDK3 in that priority order if matches to those";

        protected override void Process_Inner(RiskInput riskInput, IReadOnlyList<CodeGroupInstance> recognisedCodeGroupInstances, Date processingReferenceDate)
        {
            
            if (recognisedCodeGroupInstances.Any(cgi => GetValueFromCodeGroupInstance(cgi) == ChronicKidneyDisease.Ckd5WithTransplant))
            {
                //If they have ever had a transplant, we use the transplant code
                SetValue(riskInput, ChronicKidneyDisease.Ckd5WithTransplant);
            }
            else if(recognisedCodeGroupInstances.Any(cgi => GetValueFromCodeGroupInstance(cgi) == ChronicKidneyDisease.Ckd5WithDialysis && cgi.Date.Value >= processingReferenceDate.Value.AddMonths(-DialysisLookBackMonths)))
            {
                //If they have had dialysis in the last 12 months, we use the dialysis code
                SetValue(riskInput, ChronicKidneyDisease.Ckd5WithDialysis);
            }
            else if (recognisedCodeGroupInstances.Any(cgi => GetValueFromCodeGroupInstance(cgi) == ChronicKidneyDisease.Ckd5WithNeitherDialysisNorTransplant))
            {
                SetValue(riskInput, ChronicKidneyDisease.Ckd5WithNeitherDialysisNorTransplant);
            }
            else if(recognisedCodeGroupInstances.Any(cgi => GetValueFromCodeGroupInstance(cgi) == ChronicKidneyDisease.Ckd4))
            {
                SetValue(riskInput, ChronicKidneyDisease.Ckd4);
            }
            else if (recognisedCodeGroupInstances.Any(cgi => GetValueFromCodeGroupInstance(cgi) == ChronicKidneyDisease.Ckd3))
            {
                SetValue(riskInput, ChronicKidneyDisease.Ckd3);
            }
            else if (recognisedCodeGroupInstances.All(cgi => GetValueFromCodeGroupInstance(cgi) == ChronicKidneyDisease.Ckd5WithDialysis && cgi.Date.Value < processingReferenceDate.Value.AddMonths(-DialysisLookBackMonths)))
            {
                // edge case - user only has expired dialysis codes
                SetValue(riskInput, ChronicKidneyDisease.None);
            }
            else
            {
                throw new InvalidCodeGroupMappingException("Unexpected case for chronic kidney disease mapping.");
            }
        }
    }
}
