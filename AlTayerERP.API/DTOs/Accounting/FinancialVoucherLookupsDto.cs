using System.Collections.Generic;

namespace AlTayerERP.API.DTOs.Accounting
{
    /// <summary>
    /// جميع القوائم المطلوبة لفتح شاشة السند المالي.
    /// </summary>
    public class FinancialVoucherLookupsDto
    {
        public List<BranchLookupDto> Branches { get; set; } = new();

        public List<VoucherTypeLookupDto> VoucherTypes { get; set; } = new();

        public List<VoucherStatusLookupDto> VoucherStatuses { get; set; } = new();

        public List<CashBoxLookupDto> CashBoxes { get; set; } = new();

        public List<CurrencyLookupDto> Currencies { get; set; } = new();

        public List<CostCenterLookupDto> CostCenters { get; set; } = new();

        public List<PaymentMethodLookupDto> PaymentMethods { get; set; } = new();

        public List<PartyLookupDto> Parties { get; set; } = new();

        public List<AccountLookupDto> Accounts { get; set; } = new();
    }

    public class BranchLookupDto
    {
        public int Branch_ID { get; set; }

        public string Branch_Name { get; set; } = string.Empty;
    }

    public class VoucherTypeLookupDto
    {
        public int Voucher_Type_ID { get; set; }

        public string Voucher_Type_Code { get; set; } = string.Empty;

        public string Voucher_Type_Name_AR { get; set; } = string.Empty;
    }

    public class VoucherStatusLookupDto
    {
        public int Voucher_Status_ID { get; set; }

        public string Voucher_Status_Code { get; set; } = string.Empty;

        public string Voucher_Status_Name_AR { get; set; } = string.Empty;
    }

    public class CashBoxLookupDto
    {
      //  public int Cash_Box_ID { get; set; }
        public string Cash_Box_ID { get; set; } = string.Empty;
        public string Cash_Box_Code { get; set; } = string.Empty;

        public string Cash_Box_Name { get; set; } = string.Empty;

        public string Account_ID { get; set; } = string.Empty;

        public int Branch_ID { get; set; }
    }

    public class CurrencyLookupDto
    {
        public int Currency_ID { get; set; }

        public string Currency_Code { get; set; } =
            string.Empty;

        public string Currency_Name_AR { get; set; } =
            string.Empty;

        public decimal Exchange_Rate { get; set; }

        public bool Is_Default { get; set; }

        public bool Is_Local { get; set; }
    }

    public class CostCenterLookupDto
    {
        public string Cost_Center_ID { get; set; } = string.Empty;

        public string Cost_Center_Code { get; set; } = string.Empty;

        public string Cost_Center_Name_AR { get; set; } = string.Empty;
    }

    public class PaymentMethodLookupDto
    {
        public int Payment_Method_ID { get; set; }

        public string Payment_Method_Code { get; set; } = string.Empty;

        public string Payment_Method_Name_AR { get; set; } = string.Empty;
    }

    public class PartyLookupDto
    {
        public string Party_ID { get; set; } = string.Empty;

        public string Party_Code { get; set; } = string.Empty;

        public string Party_Name_AR { get; set; } = string.Empty;
    }

    public class AccountLookupDto
    {
        public string Account_ID { get; set; } = string.Empty;

        public string Account_Code { get; set; } = string.Empty;

        public string Account_Name_AR { get; set; } = string.Empty;
    }
}