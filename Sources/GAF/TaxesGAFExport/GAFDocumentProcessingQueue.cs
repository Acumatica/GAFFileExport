using System;
using System.Collections.Generic;
using PX.Objects.Common.Documents;

namespace TaxesGAFExport
{
	public class GAFDocumentProcessingQueue
	{
		private const int MaxCountToTake = 500;

		private readonly List<DocumentIDGroup> _documentGroups;

		private int _currentDocumentGroupIndex;

		private int _currentDocumentRefNbrIndex;

		public GAFDocumentProcessingQueue(List<DocumentIDGroup> documentGroups)
		{
			_documentGroups = documentGroups;

			_currentDocumentGroupIndex = 0;
			_currentDocumentRefNbrIndex = 0;
		}

		public DocumentIDGroup GetNextDocumentGroup()
		{
			if (_currentDocumentGroupIndex >= _documentGroups.Count)
				return null;

			var currentGroup = _documentGroups[_currentDocumentGroupIndex];

			var remainingCount = currentGroup.RefNbrs.Count - _currentDocumentRefNbrIndex;

			var countToTake = Math.Min(remainingCount, MaxCountToTake);

			var documentGroup = new DocumentIDGroup()
			{
				Module = currentGroup.Module,
				DocumentTypes = currentGroup.DocumentTypes,
				RefNbrs = currentGroup.RefNbrs.GetRange(_currentDocumentRefNbrIndex, countToTake)
			};

			_currentDocumentRefNbrIndex += countToTake;

			if (_currentDocumentRefNbrIndex == currentGroup.RefNbrs.Count)
			{
				_currentDocumentGroupIndex += 1;
				_currentDocumentRefNbrIndex = 0;
			}

			return documentGroup;
		}
	}
}
