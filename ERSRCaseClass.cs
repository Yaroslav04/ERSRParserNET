using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERSRParserNET
{
    public class ERSRCaseClass : ERSRClass
    {
        public string Content {get; set; }
        public string CriminalNumber{ get; set; }

        public ERSRCaseClass(ERSRClass _ersr)
        {
            Id = _ersr.Id;
            DecisionType = _ersr.DecisionType;
            DecisionDate = _ersr.DecisionDate;
            PublicDate = _ersr.PublicDate;
            JudiciaryType = _ersr.JudiciaryType;
            Case = _ersr.Case;
            Court = _ersr.Court;
            Judge = _ersr.Judge;
            URL = _ersr.URL;

            Content = "";
            CriminalNumber = "";
        }
    }
}
