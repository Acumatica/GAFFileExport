using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.Common.Documents;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.SO;
using PX.Objects.TX;
using TaxesGAFExport.Data;
using Branch = PX.Objects.GL.Branch;
using BAccount = PX.Objects.CR.BAccount;
using Contact = PX.Objects.CR.Contact;
using Version = PX.SM.Version;

namespace TaxesGAFExport.Repositories
{
	public interface IGAFRepository
	{
		/// <summary>
		/// Gets documents which has been reported in tax period.
		/// </summary>
		IEnumerable<DocumentID> GetReportedDocuments(int? branchID, int? taxAgency, string taxPeriodID);

		TaxPeriod GetTaxPeriodByKey(int? branchID, int? taxAgencyID, string taxPeriodID);

		int? GetMaxGAFMajorVersion(GAFPeriod gafPeriod);

		Branch GetBranchByID(int? branchID);

		BAccount GetBAccountByID(int? baccountID);

		Contact GetContact(int? contactID);

		Version GetAcumaticaVersion();

		GAFPeriod FindGAFPeriodByKey(int? branchID, int? taxAgencyID, string taxPeriodID);

		IEnumerable<TaxTran> GetReportedTaxTransForDocuments(string module, string docType, string[] refNbrs, int? taxAgencyID, string taxPeriodID);

		IEnumerable<PXResult<TaxTran, APRegister>> GetReportedTaxTransWithAdjdDocumentForAPPayments(string docType, string[] refNbrs,
			int? taxAgencyID, string taxPeriodID);

		IEnumerable<PXResult<APTran, APTax>> GetReportedAPTransWithAPTaxesForTaxCalcedOnItem(string docType, string[] refNbrs,
			int? taxAgencyID, string taxPeriodID);

		IEnumerable<Tax> GetTaxesByIDs(string[] taxIDs);

		IEnumerable<TaxCategoryDet> GetTaxCategoryDetsForTaxIDs(string[] taxIDs);
			
		IEnumerable<APRegister> GetAPRegistersByIDs(string docType, string[] refNbrs);

		IEnumerable<APInvoice> GetAPInvoicesByIDs(string docType, string[] refNbrs);

		Location GetLocation(int? baccountID, int? locationID);

		Contact GetContactByID(int? contactID);

		IEnumerable<Vendor> GetVendorsByIDs(int?[] vendorIDs);

		Company GetCompany();

		IEnumerable<PXResult<APRegister, TaxTran>> GetReportedTaxAPRegistersWithTaxTransForDocuments(string docType, string[] refNbrs, int? taxAgencyID);

		IEnumerable<PXResult<ARTran, ARTax>> GetReportedARTransWithARTaxesForTaxCalcedOnItem(string docType, string[] refNbrs,
			int? taxAgencyID, string taxPeriodID);

		IEnumerable<ARInvoice> GetARInvoicesByIDs(string docType, string[] refNbrs);

		IEnumerable<Customer> GetCustomersByIDs(int?[] customerIDs);

		ARAddress GetARAddress(int? addressID);

		SOAddress GetSOAddress(int? addressID);

		Country GetCountry(string countryID);

		IEnumerable<SOInvoice> GetSOInvoices(string docType, string[] refNbrs);

		IEnumerable<PXResult<CASplit, CATax>> GetReportedCASplitsWithCATaxesForTaxCalcedOnItem(string adjTranType, string[] adjRefNbrs,
			int? taxAgencyID, string taxPeriodID);

		IEnumerable<CAAdj> GetCAAdjsByIDs(string adjTranType, string[] adjRefNbrs);

		FinPeriod FindFinPeriodWithStartDate(DateTime? startDate);

		FinPeriod FindFinPeriodWithEndDate(DateTime? endDate);

		bool CheckDoNotExistUnreleasedAPRegistersWithAPTransWithBranchWithFinPeriodLessOrEqual(int? branchID, string finPeriod);

		bool CheckDoNotExistUnreleasedARRegistersWithARTransWithBranchWithFinPeriodLessOrEqual(int? branchID, string finPeriod);

		bool CheckDoNotExistUnreleasedCAAdjsWithCASplitsWithBranchWithFinPeriodLessOrEqual(int? branchID, string finPeriod);

		bool CheckDoNotExistUnreleasedOrUnpostedGLTransWithBranchWithFinPeriodLessOrEqual(int? branchID, string finPeriod);

		IEnumerable<TaxAdjustment> GetTaxAdjustments(string docType, string[] refNbr);

		IEnumerable<TaxTran> GetTaxTransForDocuments(string module, string[] docTypes, string[] refNbr, string taxPeriodID);

		CurrencyList GetCurrencyList(string curyID);

		IEnumerable<FinPeriod> GetFinPeriodsInInterval(DateTime? fromDate, DateTime? tillDate);

		IEnumerable<FinPeriod> GetAdjustmentFinPeriods(string year);

		/// <summary>
		/// Query to get data for calculation of the beginning balances of accounts.
		/// </summary>
		/// <returns>Account;
		/// GLHistory: total of ptdDebit and ptdCredit of specified period;
		/// AH: total of finYtdBalance for last period activity period.</returns>
		IEnumerable<PXResult<Account, GLHistory, AH>>
			GetAccountsWithDataToCalcBeginBalancesExcludingYTDNetIncAcc(int? branchID, int? ledgerID,
				string finPeriodID);

		IEnumerable<GLTran> GetPostedGLTrans(int? branchID, int? ledgerID, int? accountID, string finPeriodID);

		FinPeriod FindLastYearNotAdjustmentPeriod(string finYear);

		IEnumerable<PXResult<GLTran, CurrencyInfo>> GetTaxableGLTransWithCuryInfoGroupedByDocumentAttrAndTaxCategory(int? branchID, string[] batchNbrs);

		void ClearCaches();
	}
}