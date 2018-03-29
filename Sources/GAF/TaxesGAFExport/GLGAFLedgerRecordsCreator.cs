using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.Common.Extensions;
using PX.Objects.GL;
using TaxesGAFExport.Data;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport
{
	/// <summary>
	/// Creates L record elements. Exports GL transactions and account balances.
	/// </summary>
	public class GLGAFLedgerRecordsCreator
	{
		private readonly IGAFRepository _gafRepository;

		public GLGAFLedgerRecordsCreator(IGAFRepository gafRepository)
		{
			_gafRepository = gafRepository;
		}

		/// <summary>
		/// Creates L record elements. Exports GL transactions and account balances.
		/// </summary>
		public IList<LedgerRecord> CreateLedgerRecords(GAFPeriod gafPeriod)
		{
			var taxPeriod = _gafRepository.GetTaxPeriodByKey(gafPeriod.BranchID, gafPeriod.TaxAgencyID, gafPeriod.TaxPeriodID);

			var finPeriods = _gafRepository.GetFinPeriodsInInterval(taxPeriod.StartDate, taxPeriod.EndDate)
				.ToList();

			var finYears = finPeriods.Select(finPeriod => finPeriod.FinYear)
										.Distinct();

			var lastYearPeriod = finYears.Select(finYear => _gafRepository.FindLastYearNotAdjustmentPeriod(finYear))
										.SingleOrDefault();

			if (lastYearPeriod != null && finPeriods.Select(finPeriod => finPeriod.FinPeriodID).Contains(lastYearPeriod.FinPeriodID))
			{
				finPeriods.AddRange(_gafRepository.GetAdjustmentFinPeriods(lastYearPeriod.FinYear));
			}

			var records = new List<LedgerRecord>();

			var branch = _gafRepository.GetBranchByID(gafPeriod.BranchID);

			var minFinPeriod = finPeriods.First();

			var accountsWithGLHistoryData = _gafRepository.GetAccountsWithDataToCalcBeginBalancesExcludingYTDNetIncAcc(branch.BranchID, branch.LedgerID, minFinPeriod.FinPeriodID);

			foreach (var accountWithGLHistoryData in accountsWithGLHistoryData)
			{
				var account = (Account)accountWithGLHistoryData;
				var glHistoryForLastActivityPeriod = (AH)accountWithGLHistoryData;
				var glHistoryForSelectedPeriod = (GLHistory)accountWithGLHistoryData;

				decimal? beginBalanceForPeriod = 0m;

				if (glHistoryForLastActivityPeriod.FinPeriodID != null)
				{
					var selectedPeriodPtdCreditTotal = glHistoryForSelectedPeriod.FinPtdCredit ?? 0m;
					var selectedPeriodPtdDebitTotal = glHistoryForSelectedPeriod.FinPtdDebit ?? 0m;
					var selectedPeriodPtdSaldo = AccountRules.CalcSaldo(account.Type, selectedPeriodPtdDebitTotal, selectedPeriodPtdCreditTotal);

					beginBalanceForPeriod = GetBalanceSign(account.Type) * (glHistoryForLastActivityPeriod.FinYtdBalance - selectedPeriodPtdSaldo);
				}

				var balanceRecord = BuildLedgerRecordForBalance(account, minFinPeriod.StartDate, beginBalanceForPeriod.Value);
				records.Add(balanceRecord);

				foreach (var finPeriod in finPeriods)
				{
					var glTrans = _gafRepository.GetPostedGLTrans(branch.BranchID, branch.LedgerID, account.AccountID, finPeriod.FinPeriodID);

					foreach (var glTran in glTrans)
					{
						var record = BuildLedgerRecordForTran(account, glTran, records.LastOrDefault());

						records.Add(record);
					}

					_gafRepository.ClearCaches();
				}
			}

			return records;
		}

		private LedgerRecord BuildLedgerRecordForBalance(Account account, DateTime? finPeriodStartDate, decimal beginBalanceForPeriod)
		{
			return new LedgerRecord()
			{
				AccountID = account.AccountCD,
				AccountName = account.Description,
				TransactionDescription = "OPENING BALANCE",
				TransactionDate = finPeriodStartDate.Value,
				DebitAmount = 0m,
				CreditAmount = 0m,
				BalanceAmount = beginBalanceForPeriod
			};
		}

		private LedgerRecord BuildLedgerRecordForTran(Account account, GLTran tran, LedgerRecord prevLedgerRecord)
		{
			var ledgerRecord = new LedgerRecord();

			ledgerRecord.AccountID = account.AccountCD;
			ledgerRecord.AccountName = account.Description;
			ledgerRecord.TransactionDate = tran.TranDate.Value;
			ledgerRecord.TransactionDescription = tran.TranDesc;
			ledgerRecord.Name = GetContragentCompanyNameIfApplicable(tran);
			ledgerRecord.TransactionID = tran.BatchNbr;
			ledgerRecord.SourceDocumentID = tran.RefNbr;
			ledgerRecord.SourceType = tran.Module;
			ledgerRecord.DebitAmount = tran.DebitAmt.Value;
			ledgerRecord.CreditAmount = tran.CreditAmt.Value;

			ledgerRecord.BalanceAmount = prevLedgerRecord.BalanceAmount + ledgerRecord.DebitAmount - ledgerRecord.CreditAmount;

			return ledgerRecord;
		}

		private decimal GetBalanceSign(string accountType)
		{
			return accountType == AccountType.Liability || accountType == AccountType.Income
				? -1
				: 1;
		}

		private string GetContragentCompanyNameIfApplicable(GLTran tran)
		{
			if (tran.ReferenceID != null)
			{
				int? defContactID;

				if (tran.Module == BatchModule.AP || tran.Module == BatchModule.PO)
				{
					var vendor = _gafRepository.GetVendorsByIDs(tran.ReferenceID.SingleToArray())
												.Single();

					defContactID = vendor.DefContactID;
				}
				else
				{
					var customer = _gafRepository.GetCustomersByIDs(tran.ReferenceID.SingleToArray())
													.Single();

					defContactID = customer.DefContactID;
				}

				var contact = _gafRepository.GetContact(defContactID);

				return contact.FullName;
			}

			return null;
		}
	}
}