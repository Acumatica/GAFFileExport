using System;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Moq;
using PX.Data;
using PX.Objects.Common.Extensions;
using PX.Objects.Common.Tools;
using PX.Objects.GAF.Tests.DataContexts;
using PX.Objects.GL;
using PX.Objects.Tests.AP.DataContexts;
using PX.Objects.Tests.AR.DataContexts;
using PX.Objects.Tests.Common.DataContexts;
using PX.Objects.Tests.GL.DataContexts;
using PX.Objects.Tests.Infrastucture.ApprovalTests;
using PX.Objects.Tests.TX.DataContexts;
using TaxesGAFExport;
using Xunit;

namespace PX.Objects.GAF.Tests.Ledger
{
	public class GLGAFLedgerRecordsCreatorTests: GAFTestsBase
	{
		private AccountDataContext _accountDataContext;
		private GAFPeriodDataContext _gafPeriodDataContext;
		private TaxPeriodDataContext _taxPeriodDataContext;
		private BranchDataContext _branchDataContext;
		private FinPeriodDataContext _finPeriodDataContext;

		private GLGAFLedgerRecordsCreator _glgafLedgerRecordsCreator;

		public GLGAFLedgerRecordsCreatorTests()
		{
			_accountDataContext = GetService<AccountDataContext>();
			_gafPeriodDataContext = GetService<GAFPeriodDataContext>();
			_taxPeriodDataContext = GetService<TaxPeriodDataContext>();
			_branchDataContext = GetService<BranchDataContext>();
			_finPeriodDataContext = GetService<FinPeriodDataContext>();

			_glgafLedgerRecordsCreator = new GLGAFLedgerRecordsCreator(GAFRepository);

			GAFRepositoryMock.Setup(repo =>	repo.GetTaxPeriodByKey(_gafPeriodDataContext.GAFPeriod.BranchID, _gafPeriodDataContext.GAFPeriod.TaxAgencyID, _gafPeriodDataContext.GAFPeriod.TaxPeriodID))
								.Returns(_taxPeriodDataContext.TaxPeriod);

			GAFRepositoryMock.Setup(repo => repo.GetFinPeriodsInInterval(_taxPeriodDataContext.TaxPeriod.StartDate, _taxPeriodDataContext.TaxPeriod.EndDate))
								.Returns(_finPeriodDataContext.FinPeriod201503.SingleToArray());

			GAFRepositoryMock.Setup(repo => repo.GetBranchByID(_branchDataContext.Branch.BranchID))
								.Returns(_branchDataContext.Branch);

			GAFRepositoryMock.Setup(repo => repo.FindLastYearNotAdjustmentPeriod(It.IsAny<string>()))
								.Returns(new FinPeriod() {FinPeriodID = "201512"});
		}

		#region Test_CreateLedgerRecords_Account_Record_Creation

		[Theory]
		[InlineData(AccountType.Liability,	5, 1, 2, -4)]
		[InlineData(AccountType.Income,		4, 1, 2, -3)]
		[InlineData(AccountType.Asset,		3, 1, 2, 4)]
		[InlineData(AccountType.Expense,	6, 2, 4, 8)]
		public void Test_CreateLedgerRecords_Account_Record_Creation_From_GLHistory(string accountType, decimal accountYtdBalance, decimal ptdDebit, decimal ptdCredit, decimal expectedBegBalance)
		{
			//Arrange
			Test_CreateLedgerRecords_Account_Record_Creation_Base(accountType,
				_finPeriodDataContext.FinPeriod201503.FinPeriodID,
				accountYtdBalance,
				expectedBegBalance,
				ptdDebit,
				ptdCredit);
		}

		[Fact]
		public void Test_CreateLedgerRecords_Account_Record_Creation_From_GLHistory_With_Zero_Balance()
		{
			Test_CreateLedgerRecords_Account_Record_Creation_Base(AccountType.Liability, _finPeriodDataContext.FinPeriod201502.FinPeriodID, 0, 0, 0);
		}

		[Fact]
		public void Test_CreateLedgerRecords_Account_Record_Creation_Without_GLHistory()
		{
			Test_CreateLedgerRecords_Account_Record_Creation_Base("", null, 0, 0, 0);
		}

		private void Test_CreateLedgerRecords_Account_Record_Creation_Base(string accountType, string finPeriodID, decimal accountYtdBalance, decimal expectedBeginBalance, decimal? ptdDebit = null, decimal? ptdCredit = null)
		{
			//Arrange
			SetupAccountsWithGLHistory(accountYtdBalance, finPeriodID, accountType, ptdDebit, ptdCredit);

			GAFRepositoryMock.Setup(repo => repo.GetPostedGLTrans(_branchDataContext.Branch.BranchID,
																	_branchDataContext.Branch.LedgerID,
																	_accountDataContext.AssetAccount.AccountID,
																	_finPeriodDataContext.FinPeriod201503.FinPeriodID))
								.Returns(new GLTran[] {});

			//Action
			var ledgerRecord = _glgafLedgerRecordsCreator.CreateLedgerRecords(_gafPeriodDataContext.GAFPeriod)
															.Single();

			//Assert
			Assert.Equal(_accountDataContext.AssetAccount.AccountCD, ledgerRecord.AccountID);
			Assert.Equal(_accountDataContext.AssetAccount.Description, ledgerRecord.AccountName);
			Assert.Equal("OPENING BALANCE", ledgerRecord.TransactionDescription);
			Assert.Equal(_finPeriodDataContext.FinPeriod201503.StartDate, ledgerRecord.TransactionDate);
			Assert.Equal(0, ledgerRecord.DebitAmount);
			Assert.Equal(0, ledgerRecord.CreditAmount);
			Assert.Equal(expectedBeginBalance, ledgerRecord.BalanceAmount);

			Assert.Null(ledgerRecord.Name);
			Assert.Null(ledgerRecord.TransactionID);
			Assert.Null(ledgerRecord.SourceDocumentID);
			Assert.Null(ledgerRecord.SourceType);
		}

		#endregion

		[Theory]
		[InlineData(BatchModule.AR, CustomerDataContext.CustomerID, ContactDataContext.CustomerLocationFullName)]
		[InlineData(BatchModule.SO, CustomerDataContext.CustomerID, ContactDataContext.CustomerLocationFullName)]
		[InlineData(BatchModule.AP, VendorDataContext.VendorID, ContactDataContext.VendorLocationFullName)]
		[InlineData(BatchModule.PO, VendorDataContext.VendorID, ContactDataContext.VendorLocationFullName)]
		[InlineData(BatchModule.GL, null, null)]
		public void Test_CreateLedgerRecords_GLTran_Record_Creation(string module, int? contragentID, string expectedContragentName)
		{
			//Arrange
			const decimal accountBegBalance = 5;

			SetupAccountsWithGLHistory(accountBegBalance, _finPeriodDataContext.FinPeriod201503.FinPeriodID);

			var glTran = new GLTran()
			{
				DebitAmt = 5,
				CreditAmt = 1,
				TranDesc = "TranDesc",
				TranDate = new DateTime(2015, 1, 3),
				BatchNbr = "BN0001",
				RefNbr = "REF0001",
				Module = module,
				ReferenceID = contragentID
			};

			GAFRepositoryMock.Setup(repo => repo.GetPostedGLTrans(_branchDataContext.Branch.BranchID,
																	_branchDataContext.Branch.LedgerID,
																	_accountDataContext.AssetAccount.AccountID,
																	_finPeriodDataContext.FinPeriod201503.FinPeriodID))
								.Returns(glTran.SingleToArray());

			//Action
			var ledgerRecord = _glgafLedgerRecordsCreator.CreateLedgerRecords(_gafPeriodDataContext.GAFPeriod)
				.Last();

			//Assert
			Assert.Equal(glTran.TranDate, ledgerRecord.TransactionDate);
			Assert.Equal(_accountDataContext.AssetAccount.AccountCD, ledgerRecord.AccountID);
			Assert.Equal(_accountDataContext.AssetAccount.Description, ledgerRecord.AccountName);
			Assert.Equal(glTran.TranDesc, ledgerRecord.TransactionDescription);
			Assert.Equal(expectedContragentName, ledgerRecord.Name);
			Assert.Equal(glTran.BatchNbr, ledgerRecord.TransactionID);
			Assert.Equal(glTran.RefNbr, ledgerRecord.SourceDocumentID);
			Assert.Equal(glTran.Module, ledgerRecord.SourceType);

			Assert.Equal(glTran.DebitAmt, ledgerRecord.DebitAmount);
			Assert.Equal(glTran.CreditAmt, ledgerRecord.CreditAmount);
			Assert.Equal(accountBegBalance + (ledgerRecord.DebitAmount - ledgerRecord.CreditAmount), ledgerRecord.BalanceAmount);
		}

		[Fact]
		[UseReporter(typeof(DiffReporter))]
		[UseApprovalFileName("__CreateLedgerRecords_With_Two_Accounts_And_Two_Trans")]
		public void Test_CreateLedgerRecords_With_Two_Accounts_And_Two_Trans()
		{
			//Arrange
			var accountHistoryData = new []
			{
				new PXResult<Account, GLHistory, AH>(_accountDataContext.AssetAccount,
					new GLHistory()
					{
						PtdCredit = 1,
						PtdDebit = 3
					},
					new AH()
					{
						FinPeriodID = _finPeriodDataContext.FinPeriod201503.FinPeriodID,
						YtdBalance = 6
					}),
				new PXResult<Account, GLHistory, AH>(_accountDataContext.LiablityAccount,
					new GLHistory()
					{
						PtdCredit = 9,
						PtdDebit = 2
					},
					new AH()
					{
						FinPeriodID = _finPeriodDataContext.FinPeriod201503.FinPeriodID,
						YtdBalance = 15
					})
			};

			GAFRepositoryMock.Setup(repo => repo.GetAccountsWithDataToCalcBeginBalancesExcludingYTDNetIncAcc(_branchDataContext.Branch.BranchID,
																														_branchDataContext.Branch.LedgerID,
																														_finPeriodDataContext.FinPeriod201503.FinPeriodID))
								.Returns(accountHistoryData);

			var glTrans = new GLTran[] { new GLTran()
			{
				AccountID = _accountDataContext.AssetAccount.AccountID,
				DebitAmt = 5,
				CreditAmt = 1,
				TranDesc = "TranDesc1",
				TranDate = new DateTime(2015, 1, 1),
				BatchNbr = "BN0001",
				RefNbr = "REF0001",
				Module = BatchModule.AP,
				ReferenceID = VendorDataContext.VendorID
			},
			new GLTran()
			{
				AccountID = _accountDataContext.AssetAccount.AccountID,
				DebitAmt = 6,
				CreditAmt = 3,
				TranDesc = "TranDesc2",
				TranDate = new DateTime(2015, 1, 2),
				BatchNbr = "BN0002",
				RefNbr = "REF0002",
				Module = BatchModule.AR,
				ReferenceID = CustomerDataContext.CustomerID
			},
			new GLTran()
			{
				AccountID = _accountDataContext.LiablityAccount.AccountID,
				DebitAmt = 9,
				CreditAmt = 15,
				TranDesc = "TranDesc3",
				TranDate = new DateTime(2015, 1, 3),
				BatchNbr = "BN0003",
				RefNbr = "REF0003",
				Module = BatchModule.SO,
				ReferenceID = CustomerDataContext.CustomerID
			},
			new GLTran()
			{
				AccountID = _accountDataContext.LiablityAccount.AccountID,
				DebitAmt = 20,
				CreditAmt = 7,
				TranDesc = "TranDesc4",
				TranDate = new DateTime(2015, 1, 4),
				BatchNbr = "BN0004",
				RefNbr = "REF0004",
				Module = BatchModule.PO,
				ReferenceID = VendorDataContext.VendorID
			}};

			GAFRepositoryMock.Setup(repo => repo.GetPostedGLTrans(_branchDataContext.Branch.BranchID, 
																	_branchDataContext.Branch.LedgerID,
																	It.IsAny<int?>(),
																	_finPeriodDataContext.FinPeriod201503.FinPeriodID))
								.Returns<int?, int?, int?, string>((b, l, accountID, fp) => glTrans.Where(glTran => glTran.AccountID == accountID));

			//Action
			var ledgerRecords = _glgafLedgerRecordsCreator.CreateLedgerRecords(_gafPeriodDataContext.GAFPeriod);

			//Assert
			Approvals.VerifyAll(ledgerRecords, "ledgerRecords", record => record.Dump());
		}

		[Theory]
		[InlineData(0)]
		[InlineData(5)]
		public void Test_CreateLedgerRecords_That_Total_Of_Balance_Is_Calculated(decimal accountBeginBalance)
		{
			//Arrange
			SetupAccountsWithGLHistory(accountBeginBalance, _finPeriodDataContext.FinPeriod201503.FinPeriodID);

			var glTrans = new GLTran[]
			{
				new GLTran()
				{
					TranDate = new DateTime(),
					DebitAmt = 5,
					CreditAmt = 1,
				},
				new GLTran()
				{
					TranDate = new DateTime(),
					DebitAmt = 10,
					CreditAmt = 2,
				}
			};

			GAFRepositoryMock.Setup(repo => repo.GetPostedGLTrans(_branchDataContext.Branch.BranchID,
																	_branchDataContext.Branch.LedgerID,
																	_accountDataContext.AssetAccount.AccountID,
																	_finPeriodDataContext.FinPeriod201503.FinPeriodID))
								.Returns(glTrans);

			//Action
			var ledgerRecords = _glgafLedgerRecordsCreator.CreateLedgerRecords(_gafPeriodDataContext.GAFPeriod);

			//Assert
			var tranRecordStartIndex = accountBeginBalance == 0
														? 0
														: 1;

			var tranRecord1 = ledgerRecords[tranRecordStartIndex];
			var tranRecord2 = ledgerRecords[tranRecordStartIndex + 1];

			Assert.Equal(accountBeginBalance + (tranRecord1.DebitAmount - tranRecord1.CreditAmount), tranRecord1.BalanceAmount);
			Assert.Equal(tranRecord1.BalanceAmount + (tranRecord2.DebitAmount - tranRecord2.CreditAmount),
				tranRecord2.BalanceAmount);
		}

		[Fact]
		[UseReporter(typeof(DiffReporter))]
		[UseApprovalFileName("__CreateLedgerRecords__Adjustment_Period__")]
		public void Test_CreateLedgerRecords_That_Adjustment_Period_Is_Joined_To_Last_Year_Period_And_Is_Exported()
		{
			//Arrange
			var accountHistoryPairs = new[]
			{
				new PXResult<Account, GLHistory, AH>(_accountDataContext.AssetAccount,
					new GLHistory()
					{
						FinPeriodID = _finPeriodDataContext.FinPeriod201503.FinPeriodID
					},
					new AH()),
				new PXResult<Account, GLHistory, AH>(_accountDataContext.AssetAccount,
					new GLHistory()
					{
						FinPeriodID = _finPeriodDataContext.FinPeriod201504.FinPeriodID
					},
					new AH())
			};

			GAFRepositoryMock.Setup(repo => repo.GetAccountsWithDataToCalcBeginBalancesExcludingYTDNetIncAcc(_branchDataContext.Branch.BranchID, 
																											_branchDataContext.Branch.LedgerID,
																											It.IsAny<string>()))
								.Returns<int?, int?, string>((b, l, finPeriodID) => accountHistoryPairs.Where(pair => ((GLHistory)pair).FinPeriodID == finPeriodID));

			GAFRepositoryMock.Setup(repo => repo.FindLastYearNotAdjustmentPeriod(_finPeriodDataContext.FinPeriod201503.FinYear))
								.Returns(_finPeriodDataContext.FinPeriod201503);

			GAFRepositoryMock.Setup(repo => repo.GetAdjustmentFinPeriods(_finPeriodDataContext.FinPeriod201503.FinYear))
								.Returns(_finPeriodDataContext.FinPeriod201504.SingleToArray());

			var glTrans = new GLTran[]
			{
				new GLTran()
				{
					AccountID = _accountDataContext.AssetAccount.AccountID,
					DebitAmt = 5,
					CreditAmt = 1,
					TranDesc = "TranDesc1",
					TranDate = new DateTime(2015, 1, 1),
					BatchNbr = "BN0001",
					RefNbr = "REF0001",
					Module = BatchModule.AP,
					ReferenceID = VendorDataContext.VendorID,
					FinPeriodID = _finPeriodDataContext.FinPeriod201503.FinPeriodID
				},
				new GLTran()
				{
					AccountID = _accountDataContext.AssetAccount.AccountID,
					DebitAmt = 6,
					CreditAmt = 3,
					TranDesc = "TranDesc2",
					TranDate = new DateTime(2015, 1, 2),
					BatchNbr = "BN0002",
					RefNbr = "REF0002",
					Module = BatchModule.AR,
					ReferenceID = CustomerDataContext.CustomerID,
					FinPeriodID = _finPeriodDataContext.FinPeriod201504.FinPeriodID
				}
			};

			GAFRepositoryMock.Setup(repo => repo.GetPostedGLTrans(_branchDataContext.Branch.BranchID, 
																	_branchDataContext.Branch.LedgerID,
																	_accountDataContext.AssetAccount.AccountID,
																	It.IsAny<string>()))
								.Returns<int?, int?, int?, string>((b, l, a, finPeriodID) => glTrans.Where(glTran => glTran.FinPeriodID == finPeriodID));

			//Action
			var ledgerRecords = _glgafLedgerRecordsCreator.CreateLedgerRecords(_gafPeriodDataContext.GAFPeriod);

			//Assert
			Approvals.VerifyAll(ledgerRecords, "ledgerRecords", record => record.Dump());
		}

		private void SetupAccountsWithGLHistory(decimal accountYtdBalance, string resultFinPeriodID, string accountType = null, decimal? ptdDebit = null, decimal? ptdCredit = null)
		{
			var account = _accountDataContext.CreateAssetAccount();
			account.Type = accountType ?? account.Type;

			GAFRepositoryMock.Setup(repo => repo.GetAccountsWithDataToCalcBeginBalancesExcludingYTDNetIncAcc(_branchDataContext.Branch.BranchID, 
																											_branchDataContext.Branch.LedgerID,
																											_finPeriodDataContext.FinPeriod201503.FinPeriodID))
							.Returns(new PXResult<Account, GLHistory, AH>(account,
																		new GLHistory()
																		{
																			PtdDebit = ptdDebit,
																			PtdCredit = ptdCredit
																		},
																		new AH()
																		{
																			FinPeriodID = resultFinPeriodID,
																			YtdBalance = accountYtdBalance
																		})
																	.SingleToArray());
		}
	}
}
