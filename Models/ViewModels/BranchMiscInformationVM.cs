using System.ComponentModel;

namespace Spider_QAMS.Models.ViewModels
{
    public class BranchMiscInformationVM
    {
        [DisplayName("Number of Cleaners")]
        public int? NoOfCleaners { get; set; }

        [DisplayName("Frequency of Daily Mailing Service")]
        public int? FrequencyOfDailyMailingService { get; set; }

        [DisplayName("Electric Supply")]
        public string? ElectricSupply { get; set; }

        [DisplayName("Water Supply")]
        public string? WaterSupply { get; set; }

        [DisplayName("Branch Open Date")]
        public DateTime? BranchOpenDate { get; set; }

        [DisplayName("Teller Counters")]
        public int? TellersCounter { get; set; }

        [DisplayName("Number of Sales Manager Offices")]
        public int? NoOfSalesManagerOffices { get; set; }
        [DisplayName("VIP Section Exists")]
        public bool ExistVIPSection { get; set; } = false;

        [DisplayName("Contract Start Date")]
        public DateTime? ContractStartDate { get; set; }

        [DisplayName("Number of Renovation Retouch Time")]
        public int? NoOfRenovationRetouchTime { get; set; }

        [DisplayName("Leased/Owned Building")]
        public bool LeasedOwBuilding { get; set; } = false;

        [DisplayName("Number of Tea Boys")]
        public int? NoOfTeaBoys { get; set; }

        [DisplayName("Frequency of Monthly Cleaning Service")]
        public int? FrequencyOfMonthlyCleaningService { get; set; }

        [DisplayName("Drain/Sewerage")]
        public string? DrainSewerage { get; set; }

        [DisplayName("Central AC")]
        public bool CentralAC { get; set; } = false;

        [DisplayName("Split AC")]
        public bool SplitAC { get; set; } = false;

        [DisplayName("Window AC")]
        public bool WindowAC { get; set; } = false;

        [DisplayName("Cash Counter Type")]
        public int? CashCounterType { get; set; }
        [DisplayName("Number of Teller Counters")]
        public int? NoOfTellerCounters { get; set; }

        [DisplayName("Number of Affluent Relationship Manager Offices")]
        public int? NoOfAffluentRelationshipManagerOffices { get; set; }

        [DisplayName("Separate VIP Section")]
        public bool SeparateVIPSection { get; set; } = false;

        [DisplayName("Contract End Date")]
        public DateTime? ContractEndDate { get; set; }

        [DisplayName("Renovation Retouch Date")]
        public DateTime? RenovationRetouchDate { get; set; }

        [DisplayName("Number of TCR Machines")]
        public int? NoOfTCRMachines { get; set; }

        [DisplayName("Number of Totems")]
        public int? NoOfTotem { get; set; }
    }
}
