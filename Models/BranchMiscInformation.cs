namespace Spider_QAMS.Models
{
    public class BranchMiscInformation
    {
        public long SiteID { get; set; }
        public int? NoOfCleaners { get; set; }
        public int? FrequencyOfDailyMailingService { get; set; }
        public string? ElectricSupply { get; set; }
        public string? WaterSupply { get; set; }
        public DateTime? BranchOpenDate { get; set; }
        public int? TellersCounter { get; set; }
        public int? NoOfSalesManagerOffices { get; set; }
        public bool ExistVIPSection { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public int? NoOfRenovationRetouchTime { get; set; }
        public bool LeasedOwBuilding { get; set; }
        public int? NoOfTeaBoys { get; set; }
        public int? FrequencyOfMonthlyCleaningService { get; set; }
        public string? DrainSewerage { get; set; }
        public bool CentralAC { get; set; }
        public bool SplitAC { get; set; }
        public bool WindowAC { get; set; }
        public int? CashCounterType { get; set; }
        public int? NoOfTellerCounters { get; set; }
        public int? NoOfAffluentRelationshipManagerOffices { get; set; }
        public bool SeperateVIPSection { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public DateTime? RenovationRetouchDate { get; set; }
        public int? NoOfTCRMachines { get; set; }
        public int? NoOfTotem { get; set; }
    }
}
