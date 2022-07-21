using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ERSRParserNET
{
    public class ERSRClass
    {
        public string Id { get; set; }
        public string DecisionType { get; set; }
        public DateTime DecisionDate { get; set; }
        public DateTime PublicDate { get; set; }
        public string JudiciaryType { get; set; }
        public string Case { get; set; }
        public string Court { get; set; }
        public string Judge { get; set; }

        public ERSRClass()
        {
            Id = "";
            DecisionType = "";
            DecisionDate = DateTime.MinValue;
            PublicDate = DateTime.MinValue;
            JudiciaryType = "";
            Case = "";
            Court = "";
            Judge = "";
        }

        public bool Equals(ERSRClass ersr)
        {
            //Check whether the compared object is null.  
            if (Object.ReferenceEquals(ersr, null)) return false;

            //Check whether the compared object references the same data.  
            if (Object.ReferenceEquals(this, ersr)) return true;

            //Check whether the UserDetails' properties are equal.  
            return Id.Equals(ersr.Id);
        }

        // If Equals() returns true for a pair of objects   
        // then GetHashCode() must return the same value for these objects.  

        public override int GetHashCode()
        {

            //Get hash code for the UserName field if it is not null.  
            int hashN = Id == null ? 0 : Id.GetHashCode();

            //Calculate the hash code for the GPOPolicy.  
            return hashN;
        }
    }
}
