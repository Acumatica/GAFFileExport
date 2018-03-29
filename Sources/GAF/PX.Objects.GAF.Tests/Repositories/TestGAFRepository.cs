using System;
using System.Collections.Generic;
using System.Linq;
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
using PX.Objects.Tests.AP.DataContexts;
using PX.Objects.Tests.AR.DataContexts;
using PX.Objects.Tests.Common.DataContexts;
using PX.Objects.Tests.SO.DataContexts;
using PX.Objects.Tests.TX.DataContexts;
using PX.Objects.TX;
using TaxesGAFExport.Data;
using TaxesGAFExport.Repositories;
using Version = PX.SM.Version;

namespace PX.Objects.GAF.Tests.Repositories
{
	public class TestGAFRepository : IGAFRepository
	{
		private readonly TaxDataContext _taxDataContext;
		private readonly VendorDataContext _vendorDataContext;
		private readonly LocationDataContext _locationDataContext;
		private readonly ContactDataContext _contactDataContext;
		private readonly CompanyDataContext _companyDataContext;
		private readonly CustomerDataContext _customerDataContext;
		private readonly ARAddressDataContext _arAddressDataContext;
		private readonly CountryDataContext _countryDataContext;
		private readonly SOAddressDataContext _soAddressDataContext;

		public TestGAFRepository(TaxDataContext taxDataContext,
									VendorDataContext vendorDataContext,
									LocationDataContext locationDataContext,
									ContactDataContext contactDataContext,
									CompanyDataContext companyDataContext,
									CustomerDataContext customerDataContext,
									ARAddressDataContext arAddressDataContext,
									CountryDataContext countryDataContext,
									SOAddressDataContext soAddressDataContext)
		{
			_taxDataContext = taxDataContext;
			_vendorDataContext = vendorDataContext;
			_locationDataContext = locationDataContext;
			_contactDataContext = contactDataContext;
			_companyDataContext = companyDataContext;
			_customerDataContext = customerDataContext;
			_arAddressDataContext = arAddressDataContext;
			_countryDataContext = countryDataContext;
			_soAddressDataContext = soAddressDataContext;
		}

		public virtual IEnumerable<DocumentID> GetReportedDocuments(int? branchID, int? taxAgency, string taxPeriodID)
		{
			throw new NotImplementedException();
		}

		public virtual TaxPeriod GetTaxPeriodByKey(int? branchID, int? taxAgencyID, string taxPeriodID)
		{
			throw new NotImplementedException();
		}

		public int? GetMaxGAFMajorVersion(GAFPeriod gafPeriod)
		{
			throw new NotImplementedException();
		}

		public virtual Branch GetBranchByID(int? branchID)
		{
			throw new NotImplementedException();
		}

		public virtual BAccount GetBAccountByID(int? baccountID)
		{
			throw new NotImplementedException();
		}

		public virtual Contact GetContact(int? contactID)
		{
			return _contactDataContext.Contacts[contactID];
		}

		public virtual Version GetAcumaticaVersion()
		{
			throw new NotImplementedException();
		}

		public virtual GAFPeriod FindGAFPeriodByKey(int? branchID, int? taxAgencyID, string taxPeriodID)
		{
			throw new NotImplementedException();
		}

		public virtual IEnumerable<TaxTran> GetReportedTaxTransForDocuments(string module, string docType, string[] refNbrs, int? taxAgencyID, string taxPeriodID)
		{
			throw new NotImplementedException();
		}

		public virtual IEnumerable<PXResult<TaxTran, APRegister>> GetReportedTaxTransWithAdjdDocumentForAPPayments(string docType, string[] refNbrs, int? taxAgencyID,
			string taxPeriodID)
		{
			throw new NotImplementedException();
		}

		public virtual IEnumerable<PXResult<APTran, APTax>> GetReportedAPTransWithAPTaxesForTaxCalcedOnItem(string docType, string[] refNbrs, int? taxAgencyID, string taxPeriodID)
		{
			throw new NotImplementedException();
		}

		public virtual IEnumerable<Tax> GetTaxesByIDs(string[] taxIDs)
		{
			return _taxDataContext.Taxes.Values.Where(tax => taxIDs.Contains(tax.TaxID));
		}

		public virtual IEnumerable<TaxCategoryDet> GetTaxCategoryDetsForTaxIDs(string[] taxIDs)
		{
			throw new NotImplementedException();
		}

		public virtual IEnumerable<APRegister> GetAPRegistersByIDs(string docType, string[] refNbrs)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<AP.APInvoice> GetAPInvoicesByIDs(string docType, string[] refNbrs)
		{
			throw new NotImplementedException();
		}

		public virtual Location GetLocation(int? baccountID, int? locationID)
		{
			return
				_locationDataContext.Locations.Single(
					location => location.BAccountID == baccountID && location.LocationID == locationID);
		}

		public virtual Contact GetContactByID(int? contactID)
		{
			return _contactDataContext.Contacts[contactID];
		}

		public virtual IEnumerable<Vendor> GetVendorsByIDs(int?[] vendorIDs)
		{
			return _vendorDataContext.Vendors.Values.Where(vendor => vendorIDs.Contains(vendor.BAccountID));
		}

		public virtual Company GetCompany()
		{
			return _companyDataContext.Company;
		}

		public virtual IEnumerable<PXResult<APRegister, TaxTran>> GetReportedTaxAPRegistersWithTaxTransForDocuments(string docType, string[] refNbrs, int? taxAgencyID)
		{
			throw new NotImplementedException();
		}

		public virtual IEnumerable<PXResult<ARTran, ARTax>> GetReportedARTransWithARTaxesForTaxCalcedOnItem(string docType, string[] refNbrs, int? taxAgencyID, string taxPeriodID)
		{
			throw new NotImplementedException();
		}

		public virtual IEnumerable<ARInvoice> GetARInvoicesByIDs(string docType, string[] refNbrs)
		{
			throw new NotImplementedException();
		}

		public virtual IEnumerable<Customer> GetCustomersByIDs(int?[] customerIDs)
		{
			return _customerDataContext.Customers.Values.Where(customer => customerIDs.Contains(customer.BAccountID));
		}

		public virtual ARAddress GetARAddress(int? addressID)
		{
			return _arAddressDataContext.Addresses[addressID];
		}

		public virtual SOAddress GetSOAddress(int? addressID)
		{
			return _soAddressDataContext.Addresses[addressID];
		}

		public Country GetCountry(string countryID)
		{
			return _countryDataContext.Countries[countryID];
		}

		public virtual IEnumerable<SOInvoice> GetSOInvoices(string docType, string[] refNbrs)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<PXResult<CASplit, CATax>> GetReportedCASplitsWithCATaxesForTaxCalcedOnItem(string adjTranType, string[] adjRefNbrs, int? taxAgencyID,
			string taxPeriodID)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<CAAdj> GetCAAdjsByIDs(string adjTranType, string[] adjRefNbrs)
		{
			throw new NotImplementedException();
		}

		public FinPeriod FindFinPeriodWithStartDate(DateTime? startDate)
		{
			throw new NotImplementedException();
		}

		public FinPeriod FindFinPeriodWithEndDate(DateTime? endDate)
		{
			throw new NotImplementedException();
		}

		public bool CheckDoNotExistUnreleasedAPRegistersWithAPTransWithBranchWithFinPeriodLessOrEqual(int? branchID, string finPeriod)
		{
			throw new NotImplementedException();
		}

		public bool CheckDoNotExistUnreleasedARRegistersWithARTransWithBranchWithFinPeriodLessOrEqual(int? branchID, string finPeriod)
		{
			throw new NotImplementedException();
		}

		public bool CheckDoNotExistUnreleasedCAAdjsWithCASplitsWithBranchWithFinPeriodLessOrEqual(int? branchID, string finPeriod)
		{
			throw new NotImplementedException();
		}

		public bool CheckDoNotExistUnreleasedOrUnpostedGLTransWithBranchWithFinPeriodLessOrEqual(int? branchID, string finPeriod)
		{
			throw new NotImplementedException();
		}

		public virtual IEnumerable<TaxAdjustment> GetTaxAdjustments(string docType, string[] refNbr)
		{
			throw new NotImplementedException();
		}

		public virtual IEnumerable<TaxTran> GetTaxTransForDocuments(string module, string[] docTypes, string[] refNbr, string taxPeriodID)
		{
			throw new NotImplementedException();
		}

		public virtual CurrencyList GetCurrencyList(string curyID)
		{
			throw new NotImplementedException();
		}

		public virtual IEnumerable<FinPeriod> GetFinPeriodsInInterval(DateTime? fromDate, DateTime? tillDate)
		{
			throw new NotImplementedException();
		}

		public virtual IEnumerable<FinPeriod> GetAdjustmentFinPeriods(string year)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Query to get data for calculation of the begin balances of accounts.
		/// </summary>
		/// <returns>Account;
		/// GLHistory: total of ptdDebit and ptdCredit of specified period;
		/// AH: total of finYtdBalance for last period activity period.</returns>
		public virtual IEnumerable<PXResult<Account, GLHistory, AH>>
			GetAccountsWithDataToCalcBeginBalancesExcludingYTDNetIncAcc(int? branchID, int? ledgerID,
				string finPeriodID)
		{
			throw new NotImplementedException();
		}

		public virtual IEnumerable<GLTran> GetPostedGLTrans(int? branchID, int? ledgerID, int? accountID, string finPeriodID)
		{
			throw new NotImplementedException();
		}

		public virtual FinPeriod FindLastYearNotAdjustmentPeriod(string finYear)
		{
			throw new NotImplementedException();
		}

		public virtual IEnumerable<PXResult<GLTran, CurrencyInfo>> GetTaxableGLTransWithCuryInfoGroupedByDocumentAttrAndTaxCategory(int? branchID, string[] batchNbrs)
		{
			throw new NotImplementedException();
		}

		public void ClearCaches()
		{
			
		}
	}
}
