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
using PX.Objects.TX;
using TaxesGAFExport.Data;
using BAccount = PX.Objects.CR.BAccount;
using Branch = PX.Objects.GL.Branch;
using Contact = PX.Objects.CR.Contact;
using Version = PX.SM.Version;

namespace TaxesGAFExport.Repositories
{
	public class GAFRepository : IGAFRepository
	{
		public class GAFRepositoryGraph : PXGraph<GAFRepositoryGraph>
		{

		}

		private readonly PXGraph _graph;

		public GAFRepository(PXGraph graph = null)
		{
			_graph = graph ?? PXGraph.CreateInstance<GAFRepositoryGraph>();
		}

		/// <summary>
		/// Gets documents which has been reported in the tax period.
		/// </summary>
		public IEnumerable<DocumentID> GetReportedDocuments(int? branchID, int? taxAgency, string taxPeriodID)
		{
			var childBranchIds = PXAccess.GetChildBranchIDs(GetBranchByID(branchID).BranchCD);

			return PXSelectGroupBy<TaxTran,
									Where<TaxTran.branchID, In<Required<TaxTran.branchID>>,
											And<TaxTran.vendorID, Equal<Required<TaxTran.vendorID>>,
											And<TaxTran.taxPeriodID , Equal<Required<TaxTran.taxPeriodID>>,
											And<TaxTran.voided, Equal<False>,
											And<TaxTran.released, Equal<True>>>>>>,
									Aggregate<GroupBy<TaxTran.refNbr,
												GroupBy<TaxTran.tranType>>>>
									.Select(_graph, childBranchIds, taxAgency, taxPeriodID)
									.Select(row =>
									{
										var taxTran = (TaxTran)row;

										return new DocumentID()
										{
											DocType = taxTran.TranType,
											RefNbr = taxTran.RefNbr,
											Module = taxTran.Module
										};
									});
		}

		public TaxPeriod GetTaxPeriodByKey(int? branchID, int? taxAgencyID, string taxPeriodID)
		{
			return TaxYearMaint.GetTaxPeriodByKey(_graph, branchID, taxAgencyID, taxPeriodID);
		}

		public int? GetMaxGAFMajorVersion(GAFPeriod gafPeriod)
		{
			var gafPeriodWithMaxMajorVersion = (GAFPeriod)PXSelectGroupBy<GAFPeriod,
													Where<GAFPeriod.branchID, Equal<Required<GAFPeriod.branchID>>,
														And<GAFPeriod.taxAgencyID, Equal<Required<GAFPeriod.taxAgencyID>>>>,
													Aggregate<Max<GAFPeriod.gafMajorVersion>>>
													.Select(_graph, gafPeriod.BranchID, gafPeriod.TaxAgencyID);

			return gafPeriodWithMaxMajorVersion.GAFMajorVersion;
		}

		public BAccount GetBAccountByID(int? baccountID)
		{
            var baccount = (BAccount)PXSelect<BAccountR,
                                            Where<BAccountR.bAccountID, Equal<Required<BAccountR.bAccountID>>>>
											.Select(_graph, baccountID);

		    if (baccount == null)
		        throw new PXException(PX.Objects.Common.Messages.EntityWithIDDoesNotExist,
		            EntityHelper.GetFriendlyEntityName(typeof (BAccount)), baccountID);

			return baccount;
		}

		public Contact GetContact(int? contactID)
		{
			 var contact = (Contact)PXSelectReadonly<Contact, 
												Where<Contact.contactID, Equal<Required<BAccount.defContactID>>>>
												.Select(_graph, contactID);

			if (contact == null)
				throw new PXException(PX.Objects.Common.Messages.EntityWithIDDoesNotExist,
					EntityHelper.GetFriendlyEntityName(typeof(Contact)), contactID);

			return contact;
		}

		public Version GetAcumaticaVersion()
		{
			return (Version) PXSelect<Version>.Select(_graph);
		}

		public Branch GetBranchByID(int? branchID)
		{
			var branch = (Branch)PXSelectReadonly<Branch,
											Where<Branch.branchID, Equal<Required<Branch.branchID>>>>
											.Select(_graph, branchID);

		    if (branch == null)
		        throw new PXException(PX.Objects.Common.Messages.EntityWithIDDoesNotExist,
		            EntityHelper.GetFriendlyEntityName(typeof (Branch)), branchID);

			return branch;
		}

		public GAFPeriod FindGAFPeriodByKey(int? branchID, int? taxAgencyID, string taxPeriodID)
		{
			return (GAFPeriod)PXSelect<GAFPeriod,
											Where<GAFPeriod.branchID, Equal<Required<GAFPeriod.branchID>>,
													And<GAFPeriod.taxAgencyID, Equal<Required<GAFPeriod.taxAgencyID>>,
													And<GAFPeriod.taxPeriodID, Equal<Required<GAFPeriod.taxPeriodID>>>>>>
											.Select(_graph, branchID, taxAgencyID, taxPeriodID);
		}

		public IEnumerable<TaxTran> GetReportedTaxTransForDocuments(string module, string docType, string[] refNbrs, int? taxAgencyID, string taxPeriodID)
		{
			return PXSelect<TaxTran,
								Where<TaxTran.module, Equal<Required<TaxTran.module>>,
										And<TaxTran.tranType, Equal<Required<TaxTran.tranType>>,
										And <TaxTran.refNbr, In<Required<TaxTran.refNbr>>,
										And<TaxTran.vendorID, Equal<Required<TaxTran.vendorID>>,
										And<TaxTran.taxPeriodID, Equal<Required<TaxTran.taxPeriodID>>,
										And<TaxTran.released, Equal<True>,
										And<TaxTran.voided, Equal<False>>>>>>>>>
								.Select(_graph, module, docType, refNbrs, taxAgencyID, taxPeriodID)
								.RowCast<TaxTran>();
		}

		public IEnumerable<PXResult<TaxTran, APRegister>> GetReportedTaxTransWithAdjdDocumentForAPPayments(string docType, string[] refNbrs, int? taxAgencyID, string taxPeriodID)
		{
			return PXSelectJoin<TaxTran,
								InnerJoin<APRegister,
									On<TaxTran.adjdDocType, Equal<APRegister.docType>,
										And<TaxTran.adjdRefNbr, Equal<APRegister.refNbr>>>>,
								Where<TaxTran.tranType, Equal<Required<TaxTran.tranType>>,
										And<TaxTran.refNbr, In<Required<TaxTran.refNbr>>,
										And<TaxTran.vendorID, Equal<Required<TaxTran.vendorID>>,
										And<TaxTran.taxPeriodID, Equal<Required<TaxTran.taxPeriodID>>,
										And<TaxTran.voided, Equal<False>,
										And<TaxTran.released, Equal<True>,
										And<TaxTran.module, Equal<BatchModule.moduleAP>>>>>>>>>
								.Select(_graph, docType, refNbrs, taxAgencyID, taxPeriodID)
								.Cast<PXResult<TaxTran, APRegister>>();
		}

		public IEnumerable<PXResult<APTran, APTax>> GetReportedAPTransWithAPTaxesForTaxCalcedOnItem(string docType, string[] refNbrs, int? taxAgencyID, string taxPeriodID)
		{
			return PXSelectJoin<APTran,
									InnerJoin<APTax,
										On<APTran.tranType, Equal<APTax.tranType>,
											And<APTran.refNbr, Equal<APTax.refNbr>,
											And<APTran.lineNbr, Equal<APTax.lineNbr>>>>,
									InnerJoin<Tax,
										On<APTax.taxID, Equal<Tax.taxID>>,
									InnerJoin<TaxTran,
										On<TaxTran.taxID, Equal<APTax.taxID>,
											And<TaxTran.tranType, Equal<APTax.tranType>,
											And<TaxTran.refNbr, Equal<APTax.refNbr>>>>>>>,
									Where<APTran.tranType, Equal<Required<APTran.tranType>>,
											And<APTran.refNbr, In<Required<APTran.refNbr>>,
											And<Tax.taxVendorID, Equal<Required<Tax.taxVendorID>>,
											And<Tax.taxCalcType, Equal<CSTaxCalcType.item>,
											And<TaxTran.taxPeriodID, Equal<Required<TaxTran.taxPeriodID>>,
											And<TaxTran.module, Equal<BatchModule.moduleAP>,
											And<TaxTran.voided, Equal<False>,
											And<TaxTran.released, Equal<True>>>>>>>>>>
									.Select(_graph, docType, refNbrs, taxAgencyID, taxPeriodID)
									.Cast<PXResult<APTran, APTax>>();
		}

		public IEnumerable<Tax> GetTaxesByIDs(string[] taxIDs)
		{
			return PXSelect<Tax,
							Where<Tax.taxID, In<Required<Tax.taxID>>>>
							.Select(_graph, (object)taxIDs)
							.RowCast<Tax>();
		}

		public IEnumerable<TaxCategoryDet> GetTaxCategoryDetsForTaxIDs(string[] taxIDs)
		{
			return PXSelect<TaxCategoryDet,
								Where<TaxCategoryDet.taxID, In<Required<TaxCategoryDet.taxID>>>>
								.Select(_graph, (object)taxIDs)
								.RowCast<TaxCategoryDet>();
		}

		public IEnumerable<APRegister> GetAPRegistersByIDs(string docType, string[] refNbrs)
		{
			return PXSelect<PX.Objects.AP.Standalone.APRegister,
								Where<PX.Objects.AP.Standalone.APRegister.docType, Equal<Required<PX.Objects.AP.Standalone.APRegister.docType>>,
										And<PX.Objects.AP.Standalone.APRegister.refNbr, In<Required<PX.Objects.AP.Standalone.APRegister.refNbr>>>>>
								.Select(_graph, docType, refNbrs)
								.Select(row => (APRegister)row);
		}

		public IEnumerable<APInvoice> GetAPInvoicesByIDs(string docType, string[] refNbrs)
		{
			return PXSelect<APInvoice,
								Where<APInvoice.docType, Equal<Required<APInvoice.docType>>,
										And<APInvoice.refNbr, In<Required<APInvoice.refNbr>>>>>
								.Select(_graph, docType, refNbrs)
								.RowCast<APInvoice>();
		}

		public Location GetLocation(int? baccountID, int? locationID)
		{
			var location = (Location)PXSelect<Location,
								Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
										And<Location.locationID, Equal<Required<Location.locationID>>>>>
								.Select(_graph, baccountID, locationID);

			if (location == null)
				throw new PXException(PX.Objects.Common.Messages.EntityWithIDDoesNotExist,
					EntityHelper.GetFriendlyEntityName(typeof(Location)), Location.GetKeyImage(baccountID, locationID));

			return location;
		}

		public Contact GetContactByID(int? contactID)
		{
			var contact = (Contact)PXSelect<Contact,
								Where<Contact.contactID, Equal<Required<Contact.contactID>>>>
								.Select(_graph, contactID);

			if (contact == null)
				throw new PXException(PX.Objects.Common.Messages.EntityWithIDDoesNotExist,
					EntityHelper.GetFriendlyEntityName(typeof(Contact)), contactID);

			return contact;
		}

		public IEnumerable<Vendor> GetVendorsByIDs(int?[] vendorIDs)
		{
			return PXSelect<Vendor, 
								Where<Vendor.bAccountID, In<Required<Vendor.bAccountID>>>>
								.Select(_graph, vendorIDs)
								.RowCast<Vendor>();
		}

		public Company GetCompany()
		{
			return PXSelect<Company>.Select(_graph);
		}

		public IEnumerable<PXResult<APRegister, TaxTran>> GetReportedTaxAPRegistersWithTaxTransForDocuments(string docType, string[] refNbrs, int? taxAgencyID)
		{
			return PXSelectJoin<APRegister,
								InnerJoin<TaxTran,
									On<APRegister.docType, Equal<TaxTran.tranType>,
										And<APRegister.refNbr, Equal<TaxTran.refNbr>>>>,
								Where<APRegister.origModule, Equal<BatchModule.moduleTX>,
										And<TaxTran.origTranType, Equal<Required<TaxTran.origTranType>>,
										And<TaxTran.origRefNbr, In<Required<TaxTran.origRefNbr>>,
										And<TaxTran.vendorID, Equal<Required<TaxTran.vendorID>>,
										And<TaxTran.voided, Equal<False>,
										And<TaxTran.released, Equal<True>>>>>>>>
								  .Select(_graph, docType, refNbrs, taxAgencyID)
								.Cast<PXResult<APRegister, TaxTran>>();
									
		}

		public IEnumerable<PXResult<ARTran, ARTax>> GetReportedARTransWithARTaxesForTaxCalcedOnItem(string docType, string[] refNbrs, int? taxAgencyID, string taxPeriodID)
		{
			return PXSelectJoin<ARTran,
									InnerJoin<ARTax,
										On<ARTran.tranType, Equal<ARTax.tranType>,
											And<ARTran.refNbr, Equal<ARTax.refNbr>,
											And<ARTran.lineNbr, Equal<ARTax.lineNbr>>>>,
									InnerJoin<Tax,
										On<ARTax.taxID, Equal<Tax.taxID>>,
									InnerJoin<TaxTran,
										On<TaxTran.taxID, Equal<ARTax.taxID>,
											And<TaxTran.tranType, Equal<ARTax.tranType>,
											And<TaxTran.refNbr, Equal<ARTax.refNbr>>>>>>>,
									Where<ARTran.tranType, Equal<Required<ARTran.tranType>>,
											And<ARTran.refNbr, In<Required<ARTran.refNbr>>,
											And<Tax.taxVendorID, Equal<Required<Tax.taxVendorID>>,
											And<Tax.taxCalcType, Equal<CSTaxCalcType.item>,
											And<TaxTran.taxPeriodID, Equal<Required<TaxTran.taxPeriodID>>,
											And<TaxTran.module, Equal<BatchModule.moduleAR>,
											And<TaxTran.voided, Equal<False>,
											And<TaxTran.released, Equal<True>>>>>>>>>>
									.Select(_graph, docType, refNbrs, taxAgencyID, taxPeriodID)
									.Cast<PXResult<ARTran, ARTax>>();
		}

		public IEnumerable<ARInvoice> GetARInvoicesByIDs(string docType, string[] refNbrs)
		{
			return PXSelect<ARInvoice,
								Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>,
										And<ARInvoice.refNbr, In<Required<ARInvoice.refNbr>>>>>
								.Select(_graph, docType, refNbrs)
								.RowCast<ARInvoice>();
		}

		public IEnumerable<Customer> GetCustomersByIDs(int?[] customerIDs)
		{
			return PXSelect<Customer,
								Where<Customer.bAccountID, In<Required<Customer.bAccountID>>>>
								.Select(_graph, customerIDs)
								.RowCast<Customer>();
		}

		public ARAddress GetARAddress(int? addressID)
		{
			var arAddress = (ARAddress)PXSelect<ARAddress,
								Where<ARAddress.addressID, Equal<Required<ARAddress.addressID>>>>
								.Select(_graph, addressID);

			if (arAddress == null)
				throw new PXException(PX.Objects.Common.Messages.EntityWithIDDoesNotExist,
					EntityHelper.GetFriendlyEntityName(typeof(ARAddress)), addressID);

			return arAddress;
		}

		public SOAddress GetSOAddress(int? addressID)
		{
			var address = (SOAddress)PXSelect<SOAddress,
								Where<SOAddress.addressID, Equal<Required<SOAddress.addressID>>>>
								.Select(_graph, addressID);

			if (address == null)
				throw new PXException(PX.Objects.Common.Messages.EntityWithIDDoesNotExist,
					EntityHelper.GetFriendlyEntityName(typeof(SOAddress)), addressID);

			return address;
		}

		public Country GetCountry(string countryID)
		{
			var country = (Country)PXSelect<Country,
												Where<Country.countryID, Equal<Required<Country.countryID>>>>
												.Select(_graph, countryID);

			if (country == null)
				throw new PXException(PX.Objects.Common.Messages.EntityWithIDDoesNotExist,
					EntityHelper.GetFriendlyEntityName(typeof(ARAddress)), countryID);

			return country;
		}

		public IEnumerable<SOInvoice> GetSOInvoices(string docType, string[] refNbrs)
		{
			return PXSelect<SOInvoice,
								Where<SOInvoice.docType, Equal<Required<SOInvoice.docType>>,
										And<SOInvoice.refNbr, In<Required<SOInvoice.refNbr>>>>>
								.Select(_graph, docType, refNbrs)
								.RowCast<SOInvoice>();
		}

		public IEnumerable<PXResult<CASplit, CATax>> GetReportedCASplitsWithCATaxesForTaxCalcedOnItem(string adjTranType, string[] adjRefNbrs, int? taxAgencyID, string taxPeriodID)
		{
			return PXSelectJoin<CASplit,
									InnerJoin<CATax,
										On<CASplit.adjTranType, Equal<CATax.adjTranType>,
											And<CASplit.adjRefNbr, Equal<CATax.adjRefNbr>,
											And<CASplit.lineNbr, Equal<CATax.lineNbr>>>>,
									InnerJoin<Tax,
										On<CATax.taxID, Equal<Tax.taxID>>,
									InnerJoin<TaxTran,
										On<TaxTran.taxID, Equal<CATax.taxID>,
											And<TaxTran.tranType, Equal<CATax.adjTranType>,
											And<TaxTran.refNbr, Equal<CATax.adjRefNbr>>>>>>>,
									Where<CASplit.adjTranType, Equal<Required<CASplit.adjTranType>>,
											And<CASplit.adjRefNbr, In<Required<CASplit.adjRefNbr>>,
											And<Tax.taxVendorID, Equal<Required<Tax.taxVendorID>>,
											And<Tax.taxCalcType, Equal<CSTaxCalcType.item>,
											And<TaxTran.taxPeriodID, Equal<Required<TaxTran.taxPeriodID>>,
											And<TaxTran.module, Equal<BatchModule.moduleCA>,
											And<TaxTran.voided, Equal<False>,
											And<TaxTran.released, Equal<True>>>>>>>>>>
									.Select(_graph, adjTranType, adjRefNbrs, taxAgencyID, taxPeriodID)
									.Cast<PXResult<CASplit, CATax>>();
		}

		public IEnumerable<CAAdj> GetCAAdjsByIDs(string adjTranType, string[] adjRefNbrs)
		{
			return PXSelect<CAAdj,
								Where<CAAdj.adjTranType, Equal<Required<CAAdj.adjTranType>>,
										And<CAAdj.adjRefNbr, In<Required<CAAdj.adjRefNbr>>>>>
								.Select(_graph, adjTranType, adjRefNbrs)
								.RowCast<CAAdj>();
		}

		public FinPeriod FindFinPeriodWithStartDate(DateTime? startDate)
		{
			return FinPeriodIDAttribute.FindFinPeriodWithStartDate(_graph, startDate);
		}

		public FinPeriod FindFinPeriodWithEndDate(DateTime? endDate)
		{
			return FinPeriodIDAttribute.FindFinPeriodWithEndDate(_graph, endDate);
		}

		public bool CheckDoNotExistUnreleasedAPRegistersWithAPTransWithBranchWithFinPeriodLessOrEqual(int? branchID, string finPeriod)
		{
			var childBranchIds = PXAccess.GetChildBranchIDs(GetBranchByID(branchID).BranchCD);

			var query = new PXSelectJoin<APRegister,
											LeftJoin<APTran,
												On<APRegister.docType, Equal<APTran.tranType>,
													And<APRegister.refNbr, Equal<APTran.refNbr>>>>, 
											Where<APRegister.released, Equal<False>,
												And<APRegister.finPeriodID, LessEqual<Required<APRegister.finPeriodID>>,
												And<Where<APRegister.branchID, In<Required<APRegister.branchID>>,
															Or<APTran.branchID, In<Required<APTran.branchID>>>>>>>>(_graph);

			var apRegister = query.SelectSingle(finPeriod, childBranchIds, childBranchIds);

			return apRegister == null;
		}

		public bool CheckDoNotExistUnreleasedARRegistersWithARTransWithBranchWithFinPeriodLessOrEqual(int? branchID, string finPeriod)
		{
			var childBranchIds = PXAccess.GetChildBranchIDs(GetBranchByID(branchID).BranchCD);

			var query = new PXSelectJoin<ARRegister,
											LeftJoin<ARTran,
												On<ARRegister.docType, Equal<ARTran.tranType>,
													And<ARRegister.refNbr, Equal<ARTran.refNbr>>>>,
											Where<ARRegister.released, Equal<False>,
												And<ARRegister.finPeriodID, LessEqual<Required<ARRegister.finPeriodID>>,
												And<Where<ARRegister.branchID, In<Required<ARRegister.branchID>>,
															Or<ARTran.branchID, In<Required<ARTran.branchID>>>>>>>>(_graph);

			var arRegister = query.SelectSingle(finPeriod, childBranchIds, childBranchIds);

			return arRegister == null;
		}

		public bool CheckDoNotExistUnreleasedCAAdjsWithCASplitsWithBranchWithFinPeriodLessOrEqual(int? branchID, string finPeriod)
		{
			var childBranchIds = PXAccess.GetChildBranchIDs(GetBranchByID(branchID).BranchCD);

			var query = new PXSelectJoin<CAAdj,
											LeftJoin<CASplit,
												On<CAAdj.adjTranType, Equal<CASplit.adjTranType>,
													And<CAAdj.adjRefNbr, Equal<CASplit.adjRefNbr>>>>,
											Where<CAAdj.released, Equal<False>,
												And<CAAdj.finPeriodID, LessEqual<Required<CAAdj.finPeriodID>>,
												And<Where<CAAdj.branchID, In<Required<CAAdj.branchID>>,
															Or<CASplit.branchID, In<Required<CASplit.branchID>>>>>>>>(_graph);

			var caAdj = query.SelectSingle(finPeriod, childBranchIds, childBranchIds);

			return caAdj == null;
		}

		public bool CheckDoNotExistUnreleasedOrUnpostedGLTransWithBranchWithFinPeriodLessOrEqual(int? branchID, string finPeriod)
		{
			var childBranchIds = PXAccess.GetChildBranchIDs(GetBranchByID(branchID).BranchCD);

			var query = new PXSelect<GLTran,
								Where<Where2<Where<GLTran.released, Equal<False>,
												Or<GLTran.posted, Equal<False>>>,
											And<GLTran.finPeriodID, LessEqual<Required<Batch.finPeriodID>>,
											And<GLTran.branchID, In<Required<GLTran.branchID>>>>>>>(_graph);

			var tran = query.SelectSingle(finPeriod, childBranchIds);

			return tran == null;
		}

		public IEnumerable<TaxAdjustment> GetTaxAdjustments(string docType, string[] refNbrs)
		{
			return PXSelect<TaxAdjustment,
									Where<TaxAdjustment.docType, Equal<Required<TaxAdjustment.docType>>,
										And<TaxAdjustment.refNbr, In<Required<TaxAdjustment.refNbr>>>>>
									.Select(_graph, docType, refNbrs)
									.RowCast<TaxAdjustment>();
		}

		public IEnumerable<TaxTran> GetTaxTransForDocuments(string module, string[] docTypes, string[] refNbr, string taxPeriodID)
		{
			return PXSelect<TaxTran,
								Where<TaxTran.module, Equal<Required<TaxTran.module>>,
										And<TaxTran.tranType, In<Required<TaxTran.tranType>>,
										And<TaxTran.refNbr, In<Required<TaxTran.refNbr>>>>>>
								.Select(_graph, module, docTypes, refNbr)
								.RowCast<TaxTran>();
		}

		public CurrencyList GetCurrencyList(string curyID)
		{
			var currencyList = (CurrencyList)PXSelect<CurrencyList,
												Where<CurrencyList.curyID, Equal<Required<CurrencyList.curyID>>>>
												.Select(_graph, curyID);

			if (currencyList == null)
				throw new PXException(PX.Objects.Common.Messages.EntityWithIDDoesNotExist,
					EntityHelper.GetFriendlyEntityName(typeof(CurrencyList)), curyID);

			return currencyList;
		}

		public IEnumerable<FinPeriod> GetFinPeriodsInInterval(DateTime? fromDate, DateTime? tillDate)
		{
			return FinPeriodIDAttribute.GetFinPeriodsInInterval(_graph, fromDate, tillDate);
		}

		public IEnumerable<FinPeriod> GetAdjustmentFinPeriods(string year)
		{
			return FinPeriodIDAttribute.GetAdjustmentFinPeriods(_graph, year);
		}

		/// <summary>
		/// Query to get data for calculation of the begin balances of accounts.
		/// </summary>
		/// <returns>Account;
		/// GLHistory: total of ptdDebit and ptdCredit of specified period;
		/// AH: total of finYtdBalance for last period activity period.</returns>
		public IEnumerable<PXResult<Account, GLHistory, AH>> GetAccountsWithDataToCalcBeginBalancesExcludingYTDNetIncAcc(int? branchID, int? ledgerID, string finPeriodID)
		{
			var glSetup = GetGLSetup();
			var childBranchIds = PXAccess.GetChildBranchIDs(GetBranchByID(branchID).BranchCD);

			return PXSelectJoinGroupBy<Account,
								LeftJoin<GLHistoryByPeriod,
									On<GLHistoryByPeriod.branchID, In<Required<GLHistoryByPeriod.branchID>>,
										And<GLHistoryByPeriod.ledgerID, Equal<Required<GLHistoryByPeriod.ledgerID>>,
										And<Account.accountID, Equal<GLHistoryByPeriod.accountID>,
										And<GLHistoryByPeriod.finPeriodID, Equal<Required<GLHistoryByPeriod.finPeriodID>>,
										And<Where2<Where<Account.type, Equal<AccountType.asset>,
														Or<Account.type, Equal<AccountType.liability>>>,
													Or<Where<GLHistoryByPeriod.lastActivityPeriod, GreaterEqual<Required<GLHistoryByPeriod.lastActivityPeriod>>,// greater than the first period of a year
															And<Where<Account.type, Equal<AccountType.expense>,
															Or<Account.type, Equal<AccountType.income>>>>>>>>>>>>,
								//to get saldo in selected period
								LeftJoin<GLHistory,
									On<GLHistoryByPeriod.branchID, Equal<GLHistory.branchID>,
										And<GLHistoryByPeriod.ledgerID, Equal<GLHistory.ledgerID>,
										And<GLHistoryByPeriod.accountID, Equal<GLHistory.accountID>,
										And<GLHistoryByPeriod.subID, Equal<GLHistory.subID>,
										And<GLHistoryByPeriod.finPeriodID, Equal<GLHistory.finPeriodID>>>>>>,
								//to get ending balance for last activity period
								LeftJoin<AH, 
									On<GLHistoryByPeriod.ledgerID, Equal<AH.ledgerID>,
										And<GLHistoryByPeriod.branchID, Equal<AH.branchID>,
										And<GLHistoryByPeriod.accountID, Equal<AH.accountID>,
										And<GLHistoryByPeriod.subID, Equal<AH.subID>,
										And<GLHistoryByPeriod.lastActivityPeriod, Equal<AH.finPeriodID>>>>>>>>>, 
								Where<Account.accountID, NotEqual<Required<Account.accountID>>>,// not YTD Net Income Account>,
								Aggregate<
									Sum<AH.finYtdBalance,
									Sum<GLHistory.finPtdCredit,
									Sum<GLHistory.finPtdDebit,
									GroupBy<Account.accountID>>>>>>
								.Select(_graph, childBranchIds, 
												ledgerID, 
												finPeriodID,
												FinPeriodIDAttribute.GetFirstFinPeriodIDOfYear(finPeriodID),
												glSetup.YtdNetIncAccountID)
								.Cast<PXResult<Account, GLHistoryByPeriod, GLHistory, AH>>()
								.Select(row => new PXResult<Account, GLHistory, AH>((Account)row, (GLHistory)row, (AH)row));
		}

		public IEnumerable<GLTran> GetPostedGLTrans(int? branchID, int? ledgerID, int? accountID, string finPeriodID)
		{
			var childBranchIds = PXAccess.GetChildBranchIDs(GetBranchByID(branchID).BranchCD);

			return PXSelect<GLTran,
								Where<GLTran.branchID, In<Required<GLTran.branchID>>,
										And<GLTran.ledgerID, Equal<Required<GLTran.ledgerID>>,
										And<GLTran.accountID, Equal<Required<GLTran.accountID>>,
										And<GLTran.finPeriodID, Equal<Required<GLTran.finPeriodID>>,
										And<GLTran.posted, Equal<True>>>>>>>
								.Select(_graph, childBranchIds, ledgerID, accountID, finPeriodID)
								.RowCast<GLTran>();
		}

		public FinPeriod FindLastYearNotAdjustmentPeriod(string finYear)
		{
			return FinPeriodIDAttribute.FindLastYearNotAdjustmentPeriod(_graph, finYear);
		}

		public IEnumerable<PXResult<GLTran, CurrencyInfo>> GetTaxableGLTransWithCuryInfoGroupedByDocumentAttrAndTaxCategory(int? branchID, string[] batchNbrs)
		{
			var childBranchIds = PXAccess.GetChildBranchIDs(GetBranchByID(branchID).BranchCD);

			return PXSelectJoinGroupBy<GLTran,
									InnerJoin<CurrencyInfo,
										On<GLTran.curyInfoID, Equal<CurrencyInfo.curyInfoID>>>, 
									Where<GLTran.branchID, In<Required<GLTran.branchID>>,
											And<GLTran.batchNbr, In<Required<GLTran.batchNbr>>,
											And<GLTran.taxCategoryID, IsNotNull,
											And<GLTran.module, Equal<BatchModule.moduleGL>>>>>,
									Aggregate<GroupBy<GLTran.branchID,
												GroupBy<GLTran.batchNbr,
												GroupBy<GLTran.refNbr,
												GroupBy<GLTran.taxCategoryID>>>>>>
									.Select(_graph, childBranchIds, batchNbrs)
									.Cast<PXResult<GLTran, CurrencyInfo>>();
		}

		public void ClearCaches()
		{
			_graph.Clear();
		}

		private GLSetup GetGLSetup()
		{
			return PXSetup<GLSetup>.Select(_graph);
		}
	}
}
