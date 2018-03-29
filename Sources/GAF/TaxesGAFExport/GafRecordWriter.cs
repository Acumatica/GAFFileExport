using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TaxesGAFExport.CreatorsForDocs.Builders;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport
{
	public class GafRecordWriter
	{
		private const string ElementsDelimiter = "|";

		private readonly IGAFRepository _gafRepository;

		public GafRecordWriter(IGAFRepository gafRepository)
		{
			_gafRepository = gafRepository;
		}

		public void WriteCompanyRecord(CompanyRecord companyRecord, StringBuilder stringBuilder)
		{
			Write(stringBuilder, "C");
			Write(stringBuilder, companyRecord.CompanyName);
			Write(stringBuilder, companyRecord.CompanyBRN);
			Write(stringBuilder, companyRecord.CompanyGSTNumber);
			Write(stringBuilder, companyRecord.PeriodStartDate);
			Write(stringBuilder, companyRecord.PeriodEndDate);
			Write(stringBuilder, companyRecord.FileCreationDate);
			Write(stringBuilder, companyRecord.ProductVersion);
			WriteLast(stringBuilder, companyRecord.GAFVersion);
		}

		public void WritePurchaseRecord(PurchaseRecord purchaseRecord, StringBuilder stringBuilder)
		{
			var dataListToWrite = BuildDocumentRecordsDataListToWrite(purchaseRecord);

			dataListToWrite.Add(0, "P");
			dataListToWrite.Add(10, purchaseRecord.SupplierName);
			dataListToWrite.Add(20, purchaseRecord.SupplierBRN);
			dataListToWrite.Add(50, purchaseRecord.ImportDeclarationNumber);

			Write(stringBuilder, dataListToWrite.Values);
		}

		public void WriteSupplyRecord(SupplyRecord supplyRecord, StringBuilder stringBuilder)
		{
			var dataListToWrite = BuildDocumentRecordsDataListToWrite(supplyRecord);

			dataListToWrite.Add(0, "S");
			dataListToWrite.Add(10, supplyRecord.CustomerName);
			dataListToWrite.Add(20, supplyRecord.CustomerBRN);
			dataListToWrite.Add(110, supplyRecord.Country);

			Write(stringBuilder, dataListToWrite.Values);
		}

		private SortedList<int, object> BuildDocumentRecordsDataListToWrite(GAFRecordBase gafRecord)
		{
			var list = new SortedList<int, object>
			{
				{30, gafRecord.InvoiceDate},
				{40, gafRecord.InvoiceNumber},
				{60, gafRecord.LineNumber},
				{70, gafRecord.ProductDescription},
				{80, RoundAmount(gafRecord.Amount)},
				{90, RoundAmount(gafRecord.GSTAmount)},
				{100, gafRecord.TaxCode},
				{120, gafRecord.ForeignCurrencyCode},
				{130, RoundAmount(gafRecord.ForeignCurrencyAmount, gafRecord.ForeignCurrencyCode)},
				{140, RoundAmount(gafRecord.ForeignCurrencyAmountGST, gafRecord.ForeignCurrencyCode)}
			};

			return list;
		}

		public void WriteLedgerRecord(LedgerRecord ledgerRecord, StringBuilder stringBuilder)
		{
			Write(stringBuilder, "L");
			Write(stringBuilder, ledgerRecord.TransactionDate);
			Write(stringBuilder, ledgerRecord.AccountID);
			Write(stringBuilder, ledgerRecord.AccountName);
			Write(stringBuilder, ledgerRecord.TransactionDescription);
			Write(stringBuilder, ledgerRecord.Name);
			Write(stringBuilder, ledgerRecord.TransactionID);
			Write(stringBuilder, ledgerRecord.SourceDocumentID);
			Write(stringBuilder, ledgerRecord.SourceType);
			Write(stringBuilder, RoundAmount(ledgerRecord.DebitAmount));
			Write(stringBuilder, RoundAmount(ledgerRecord.CreditAmount));
			WriteLast(stringBuilder, RoundAmount(ledgerRecord.BalanceAmount));
		}

		public void WriteFooterRecord(FooterRecord aggregationFooterRecord, StringBuilder stringBuilder)
		{
			Write(stringBuilder, "F");

			Write(stringBuilder, aggregationFooterRecord.PurchaseRecordsCount);
			Write(stringBuilder, RoundAmount(aggregationFooterRecord.PurchaseAmountSum));
			Write(stringBuilder, RoundAmount(aggregationFooterRecord.PurchaseGSTAmountSum));

			Write(stringBuilder, aggregationFooterRecord.SupplyRecordsCount);
			Write(stringBuilder, RoundAmount(aggregationFooterRecord.SupplyAmountSum));
			Write(stringBuilder, RoundAmount(aggregationFooterRecord.SupplyGSTAmountSum));

			Write(stringBuilder, aggregationFooterRecord.LedgerRecordsCount);
			Write(stringBuilder, RoundAmount(aggregationFooterRecord.DebitSum));
			Write(stringBuilder, RoundAmount(aggregationFooterRecord.CreditSum));
			WriteLast(stringBuilder, RoundAmount(aggregationFooterRecord.BalanceSum));
		}

		private decimal RoundAmount(decimal amt, string currencyCode = null)
		{
			var curyID = currencyCode == GafRecordBuilderBase.ForeignCurrencyCodeForBaseCuryID || currencyCode == null
							? _gafRepository.GetCompany().BaseCuryID
							: currencyCode;

			var curyList = _gafRepository.GetCurrencyList(curyID);

			return Math.Round(amt, (int)curyList.DecimalPlaces, MidpointRounding.AwayFromZero);
		}

		public void Write(StringBuilder sb, object value)
		{
			if (value is DateTime)
			{
				value = ((DateTime)value).ToString("dd/MM/yyyy");
			}
			else if (value is decimal)
			{
				value = ((decimal)value).ToString(CultureInfo.InvariantCulture);
			}
			sb.Append(value);
			sb.Append(ElementsDelimiter);
		}

		public void WriteLast(StringBuilder sb, object obj)
		{
			Write(sb, obj);

			sb.AppendLine();
		}

		public void Write(StringBuilder sb, IEnumerable<object> values)
		{
			foreach (var value in values)
			{
				Write(sb, value);
			}
			sb.AppendLine();
		}
	}
}
