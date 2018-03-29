<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX506000.aspx.cs" Inherits="Page_TX506000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="TaxesGAFExport.GAFGenerationProcess" PrimaryView="GAFPeriodView">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="GAFPeriodView" FilesIndicator="True"  NoteIndicator="True">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True"/>
            <px:PXSegmentMask ID="edBranchID" runat="server" DataField="BranchID" CommitChanges="True"/>
            <px:PXSegmentMask ID="edTaxAgencyID" runat="server" DataField="TaxAgencyID" CommitChanges="True"/>
            <px:PXSelector ID="edTaxPeriodID" runat="server" DataField="TaxPeriodID"  CommitChanges="True" AutoRefresh="True"/>
            <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" />
            <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDateUI" />
		</Template>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXFormView>
</asp:Content>