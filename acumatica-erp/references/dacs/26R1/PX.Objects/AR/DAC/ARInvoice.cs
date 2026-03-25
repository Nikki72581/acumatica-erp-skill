using System;
using System.Diagnostics;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.SQLTree;
using PX.Data.WorkflowAPI;
using PX.Objects.GL;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.TX;
using PX.Objects.CR;
using PX.TM;
using SOInvoiceEntry = PX.Objects.SO.SOInvoiceEntry;
using PX.Objects.PM;
using PX.Objects.CA;
using PX.Objects.Common.Attributes;
using PX.Objects.Common;
using PX.Objects.SO.Interfaces;
using PX.Objects.AR.Standalone;

namespace PX.Objects.AR
{
	public class ARInvoiceType : ARDocType
	{
		/// <summary>
		/// Specialized selector for ARInvoice RefNbr.<br/>
		/// By default, defines the following set of columns for the selector:<br/>
		/// ARInvoice.refNbr,ARInvoice.docDate, ARInvoice.finPeriodID,<br/>
		/// ARInvoice.customerID, ARInvoice.customerID_Customer_acctName,<br/>
		/// ARInvoice.customerLocationID, ARInvoice.curyID, ARInvoice.curyOrigDocAmt,<br/>
		/// ARInvoice.curyDocBal,ARInvoice.status, ARInvoice.dueDate, ARInvoice.invoiceNbr<br/>
		/// </summary>
		public class RefNbrAttribute : PXSelectorAttribute
		{
			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="SearchType">Must be IBqlSearch, returning ARInvoice.refNbr</param>
			public RefNbrAttribute(Type SearchType)
				: base(SearchType,
				typeof(ARRegister.refNbr),
				typeof(ARInvoice.invoiceNbr),
				typeof(ARRegister.docDate),
				typeof(ARRegister.finPeriodID),
				typeof(ARRegister.customerID),
				typeof(ARRegister.customerID_Customer_acctName),
				typeof(ARRegister.customerLocationID),
				typeof(ARRegister.curyID),
				typeof(ARRegister.curyOrigDocAmt),
				typeof(ARRegister.curyDocBal),
				typeof(ARRegister.status),
				typeof(ARRegister.dueDate))
			{
			}
		}

		/// <summary>
		/// Specialized selector for ARInvoice RefNbr.<br/>
		/// By default, defines the following set of columns for the selector:<br/>
		/// ARInvoice.refNbr,ARInvoice.docDate, ARInvoice.finPeriodID,<br/>
		/// ARInvoice.customerID, ARInvoice.customerID_Customer_acctName,<br/>
		/// ARInvoice.customerLocationID, ARInvoice.curyID, ARInvoice.curyOrigDocAmt,<br/>
		/// ARInvoice.curyDocBal,ARInvoice.status, ARInvoice.dueDate, ARInvoice.invoiceNbr<br/>
		/// </summary>
		public class AdjdRefNbrAttribute : PXSelectorAttribute
		{
			public AdjdRefNbrAttribute(Type searchType, params Type[] fieldList)
				: base(searchType, fieldList)
			{ }

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="SearchType">Must be IBqlSearch, returning ARInvoice.refNbr</param>
			public AdjdRefNbrAttribute(Type SearchType)
				: this(SearchType,
				typeof(ARRegister.branchID),
				typeof(ARRegister.refNbr),
				typeof(ARRegister.docDate),
				typeof(ARRegister.finPeriodID),
				typeof(ARRegister.customerID),
				typeof(ARRegister.customerLocationID),
				typeof(ARRegister.curyID),
				typeof(ARRegister.curyOrigDocAmt),
				typeof(ARRegister.curyDocBal),
				typeof(ARRegister.status),
				typeof(ARRegister.dueDate),
				typeof(ARAdjust.ARInvoice.invoiceNbr),
				typeof(ARRegister.docDesc))
			{
			}
			protected override bool IsReadDeletedSupported => false;
		}

		public class AdjdLineNbrAttribute : PXSelectorAttribute
		{
			public const int emptyLineNbrID = 0;

			public AdjdLineNbrAttribute()
				: base(typeof(Search2<ARTran.lineNbr,
				InnerJoin<ARInvoice, On<ARInvoice.docType, Equal<ARTran.tranType>,
					And<ARInvoice.refNbr, Equal<ARTran.refNbr>>>>,
				Where<ARTran.tranType, Equal<Optional<ARAdjust.adjdDocType>>,
					And<ARTran.refNbr, Equal<Optional<ARAdjust.adjdRefNbr>>>>>),
				typeof(ARTran.sortOrder),
				typeof(ARTran.inventoryID),
				typeof(ARTran.tranDesc),
				typeof(ARTran.projectID),
				typeof(ARTran.taskID),
				typeof(ARTran.costCodeID),
				typeof(ARTran.accountID),
				typeof(ARTran.curyTranBal))
			{
				SubstituteKey = typeof(ARTran.sortOrder);
				_UnconditionalSelect = new Search<ARTran.lineNbr,
					Where<ARTran.tranType, Equal<Current<ARAdjust.adjdDocType>>,
						And<ARTran.refNbr, Equal<Current<ARAdjust.adjdRefNbr>>,
						And<ARTran.lineNbr, Equal<Required<ARTran.lineNbr>>>>>>();
			}

			public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
			{
				if (Equals(e.NewValue, emptyLineNbrID) || Equals(e.NewValue, emptyLineNbrID.ToString()))
				{
					e.Cancel = true;
				}
				else
				{
					base.FieldVerifying(sender, e);
				}
			}

			public override void SubstituteKeyFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
			{
				if (Equals(e.NewValue, emptyLineNbrID) || Equals(e.NewValue, emptyLineNbrID.ToString()))
				{
					e.NewValue = emptyLineNbrID;
				}
				else
				{
					base.SubstituteKeyFieldUpdating(sender, e);
				}
			}

			public override void SubstituteKeyFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
			{
				if (!Equals(e.ReturnValue, emptyLineNbrID) && !Equals(e.ReturnValue, emptyLineNbrID.ToString()))
				{
					base.SubstituteKeyFieldSelecting(sender, e);
				}
				else
				{
					var keyInfo = getSubstituteKeyMask(sender);
					e.ReturnState = PXStringState.CreateInstance(e.ReturnState, keyInfo.Length, null, _FieldName, null, null, keyInfo.Mask, null, null, null, null);
					if (e.ReturnValue != null && e.ReturnValue.GetType() != typeof(string))
					{
						e.ReturnValue = e.ReturnValue.ToString();
					}
				}
			}

			public override void SubstituteKeyCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
			{
				if(e.Operation.Option() == PXDBOperation.SubselectForExport)
					e.Expr = new Column(FieldName, new SimpleTable(BqlTable));
			}
		}

		public class AdjdLineNbrRestrictor : PXRestrictorAttribute
		{
			public AdjdLineNbrRestrictor(Type where, string message, params Type[] pars)
				: base(where, message, pars)
			{
			}

			public override void Verify(PXCache sender, PXFieldVerifyingEventArgs e, bool IsErrorValueRequired)
			{
				int? value = e.NewValue as int?;
				if (value != null && value != AdjdLineNbrAttribute.emptyLineNbrID)
				{
					base.Verify(sender, e, IsErrorValueRequired);
				}
			}
		}

		/// <summary>
		/// Specialized for ARInvoices version of the <see cref="AutoNumberAttribute"/><br/>
		/// It defines how the new numbers are generated for the AR Invoice. <br/>
		/// References ARInvoice.docType and ARInvoice.docDate fields of the document,<br/>
		/// and also define a link between  numbering ID's defined in AR Setup and ARInvoice types:<br/>
		/// namely ARSetup.invoiceNumberingID - for ARInvoice,
		/// ARSetup.adjustmentNumberingID - for ARDebitMemo and ARCreditMemo<br/>
		/// ARSetup.finChargeNumberingID - for FinCharges <br/>
		/// ARSetup.paymentNumberingID - for CashSale and CashReturn <br/>
		/// </summary>
		public class NumberingAttribute : AutoNumberAttribute
		{
			private static string[] _DocTypes
			{
				get
				{
					return new string[] { Invoice, DebitMemo, CreditMemo, FinCharge, SmallCreditWO, CashSale, CashReturn, PrepaymentInvoice };
				}
			}

			private static Type[] _SetupFields
			{
				get
				{
					return new Type[]
					{
						typeof(ARSetup.invoiceNumberingID),
						typeof(ARSetup.debitAdjNumberingID),
						typeof(ARSetup.creditAdjNumberingID),
						null,
						null,
						typeof(ARSetup.invoiceNumberingID),
						typeof(ARSetup.invoiceNumberingID),
						typeof(ARSetup.prepaymentInvoiceNumberingID)
					};
				}
			}

			public static Type GetNumberingIDField(string docType)
			{
				foreach (var pair in _DocTypes.Zip(_SetupFields))
					if (pair.Item1 == docType)
						return pair.Item2;

				return null;
			}

			public NumberingAttribute()
				: base(typeof(ARInvoice.docType), typeof(ARInvoice.docDate), _DocTypes, _SetupFields)
			{
			}

			protected NumberingAttribute(Type doctypeField, Type dateField, string[] doctypeValues, Type[] setupFields)
				: base(doctypeField, dateField, doctypeValues, setupFields)
			{
			}
		}

		public new static readonly string[] Values = { Invoice, DebitMemo, CreditMemo, FinCharge, SmallCreditWO, PrepaymentInvoice };
		public new static readonly string[] Labels = { Messages.Invoice, Messages.DebitMemo, Messages.CreditMemo, Messages.FinCharge, Messages.SmallCreditWO, Messages.PrepaymentInvoice };

		public new class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(Values, Labels)
			{
			}
		}

		public class AdjdListAttribute : PXStringListAttribute
		{
			public AdjdListAttribute()
				: base(
				new string[] { Invoice, DebitMemo, FinCharge, PrepaymentInvoice },
				new string[] { Messages.Invoice, Messages.DebitMemo, Messages.FinCharge, Messages.PrepaymentInvoice }) { }
		}

		/// <summary>
		/// String list with DocType, valid for the tax adjustments.
		/// </summary>
		public class TaxAdjdListAttribute : PXStringListAttribute
		{
			public TaxAdjdListAttribute()
				: base(
				new string[] { Invoice, DebitMemo, CreditMemo, FinCharge },
				new string[] { Messages.Invoice, Messages.DebitMemo, Messages.CreditMemo, Messages.FinCharge })
			{ }
		}

		public class AppListAttribute : PO.PXStringListExtAttribute
		{
			public AppListAttribute()
				: base(
					new string[] {CreditMemo, Payment, Prepayment, PrepaymentInvoice },
					new string[] {Messages.CreditMemo, Messages.Payment, Messages.Prepayment, Messages.PrepaymentInvoice },
					ARDocType.Values,
					ARDocType.Labels
				)
			{
				this.ExclusiveValues = true;
			}
		}
		public class AdjListAttribute : PO.PXStringListExtAttribute
		{
			public AdjListAttribute()
				: base(
					new string[] {Invoice, DebitMemo, FinCharge, Refund, PrepaymentInvoice},
					new string[] {Messages.Invoice, Messages.DebitMemo, Messages.FinCharge, Messages.Refund, Messages.PrepaymentInvoice },
					ARDocType.Values,
					ARDocType.Labels
				)
			{
				this.ExclusiveValues = true;
			}
		}

		public static string DrCr(string DocType)
		{
			switch (DocType)
			{
				case Invoice:
				case DebitMemo:
				case FinCharge:
				case SmallCreditWO:
				case CashSale:
				case PrepaymentInvoice:
					return GL.DrCr.Credit;
				case CreditMemo:
				case CashReturn:
					return GL.DrCr.Debit;
				default:
					return null;
			}
		}
	}

	[Serializable]
	[PXHidden]
	public partial class ARChildInvoice : ARInvoice
	{
		#region Keys
		public new class PK : PrimaryKeyOf<ARChildInvoice>.By<docType, refNbr>
		{
			public static ARChildInvoice Find(PXGraph graph, string docType, string refNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, docType, refNbr, options);
		}
		#endregion

		#region DocType
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		#endregion
	}

	/// <summary>
	/// Represents the accounts receivable invoices, credit and debit memos, overdue charges and credit write-offs
	/// as well as the invoices created in the Sales Orders module (see <see cref="SO.SOInvoice"/>).
	/// The records of this type are created and edited on the Invoices and Memos (AR301000) form
	/// (which corresponds to the <see cref="ARInvoiceEntry"/> graph).
	/// The SO invoices are created and edited through the Invoices (SO303000) form
	/// (which corresponds to the <see cref="SOInvoiceEntry"/> graph).
	/// </summary>
	[System.SerializableAttribute()]
	[PXTable()]
	[PXSubstitute(GraphType = typeof(ARInvoiceEntry))]
	[CRCacheIndependentPrimaryGraphList(new Type[] {
		typeof(SO.SOInvoiceEntry),
		typeof(ARCashSaleEntry),
		typeof(ARInvoiceEntry)
	},
		new Type[] {
		typeof(Select<ARInvoice,
			Where<ARInvoice.docType, Equal<Current<ARInvoice.docType>>,
				And<ARInvoice.refNbr, Equal<Current<ARInvoice.refNbr>>,
				And<ARInvoice.origModule, Equal<GL.BatchModule.moduleSO>,
				And<ARInvoice.released, Equal<False>>>>>>),
		typeof(Select<ARCashSale,
			Where<ARCashSale.docType, Equal<Current<ARRegister.docType>>,
			And<ARCashSale.refNbr, Equal<Current<ARRegister.refNbr>>>>>),
		typeof(Select<ARInvoice,
			Where<ARInvoice.docType, Equal<Current<ARInvoice.docType>>,
			And<ARInvoice.refNbr, Equal<Current<ARInvoice.refNbr>>>>>)
		})]
	[PXCacheName(Messages.ARInvoice)]
	[PXGroupMask(typeof(InnerJoinSingleTable<Customer, On<Customer.bAccountID, Equal<ARInvoice.customerID>, And<Match<Customer, Current<AccessInfo.userName>>>>>))]
	[DebuggerDisplay("DocType = {DocType}, RefNbr = {RefNbr}")]
	public partial class ARInvoice : ARRegister, CM.IInvoice, ICreatePaymentDocument
	{
		#region Events
		public new class Events : PXEntityEvent<ARInvoice>.Container<Events>
		{
			public PXEntityEvent<ARInvoice> ReleaseDocument;
			public PXEntityEvent<ARInvoice> OpenDocument;
			public PXEntityEvent<ARInvoice> CloseDocument;
			public PXEntityEvent<ARInvoice> VoidDocument;
			public PXEntityEvent<ARInvoice> CancelDocument;
			public PXEntityEvent<ARInvoice, CRQuote> ARInvoiceCreatedFromQuote;
			public PXEntityEvent<ARInvoice> ARInvoiceDeleted;
			public PXEntityEvent<ARInvoice> ProcessingCompleted;
		}
		#endregion

		#region Keys
		public new class PK : PrimaryKeyOf<ARInvoice>.By<docType, refNbr>
		{
			public static ARInvoice Find(PXGraph graph, string docType, string refNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, docType, refNbr, options);
		}

		public new static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<ARInvoice>.By<branchID> { }
			public class Customer : AR.Customer.PK.ForeignKeyOf<ARInvoice>.By<customerID> { }
			public class CustomerLocation : CR.Location.PK.ForeignKeyOf<ARInvoice>.By<customerID, customerLocationID> { }
			public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<ARInvoice>.By<curyInfoID> { }
			public class Currency : CM.Currency.PK.ForeignKeyOf<ARInvoice>.By<curyID> { }
			public class ARAccount : GL.Account.PK.ForeignKeyOf<ARInvoice>.By<aRAccountID> { }
			public class ARSubaccount : GL.Sub.PK.ForeignKeyOf<ARInvoice>.By<aRSubID> { }
			public class Schedule : GL.Schedule.PK.ForeignKeyOf<ARInvoice>.By<scheduleID> { }
			public class BillAddress : AR.ARAddress.PK.ForeignKeyOf<ARInvoice>.By<billAddressID> { }
			public class BillContact : AR.ARContact.PK.ForeignKeyOf<ARInvoice>.By<billContactID> { }
			public class ShipAddress : AR.ARAddress.PK.ForeignKeyOf<ARInvoice>.By<shipAddressID> { }
			public class ShipContact : AR.ARContact.PK.ForeignKeyOf<ARInvoice>.By<shipContactID> { }
			public class Terms : CS.Terms.PK.ForeignKeyOf<ARInvoice>.By<termsID> { }
			public class TaxZone : TX.TaxZone.PK.ForeignKeyOf<ARInvoice>.By<taxZoneID> { }
			public class CashAccount : CA.CashAccount.PK.ForeignKeyOf<ARInvoice>.By<cashAccountID> { }
			public class PaymentMethod : CA.PaymentMethod.PK.ForeignKeyOf<ARInvoice>.By<paymentMethodID> { }
		}
		#endregion

		#region Selected
		public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		#endregion
		#region DocType
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

		/// <summary>
		/// The type of the document.
		/// This field is a part of the compound key of the document.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="ARInvoiceType.ListAttribute"/>.
		/// </value>
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[ARInvoiceType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		[PXFieldDescription]
		public override String DocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		/// <summary>
		/// The reference number of the document.
		/// This field is a part of the compound key of the document.
		/// </summary>
		/// <value>
		/// For most document types, the reference number is generated automatically from the corresponding
		/// <see cref="Numbering">numbering sequence</see>, which is specified in the <see cref="ARSetup">Accounts Receivable module preferences</see>.
		/// </value>
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[ARInvoiceType.RefNbr(typeof(Search2<Standalone.ARRegisterAlias.refNbr,
			InnerJoinSingleTable<ARInvoice, On<ARInvoice.docType, Equal<Standalone.ARRegisterAlias.docType>,
				And<ARInvoice.refNbr, Equal<Standalone.ARRegisterAlias.refNbr>>>,
			InnerJoinSingleTable<Customer, On<Standalone.ARRegisterAlias.customerID, Equal<Customer.bAccountID>>>>,
			Where<Standalone.ARRegisterAlias.docType, Equal<Optional<ARInvoice.docType>>,
				And2<Where<Standalone.ARRegisterAlias.origModule, Equal<BatchModule.moduleAR>,
					Or<Standalone.ARRegisterAlias.origModule, Equal<BatchModule.moduleEP>,
					Or<Standalone.ARRegisterAlias.released, Equal<True>>>>,
				And<Match<Customer, Current<AccessInfo.userName>>>>>,
			OrderBy<Desc<Standalone.ARRegisterAlias.refNbr>>>), Filterable = true, IsPrimaryViewCompatible = true)]
		[ARInvoiceType.Numbering()]
		[ARInvoiceNbr()]
		[PXReferentialIntegrityCheck]
		[PXFieldDescription]
		public override String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
        #endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		#endregion
        #region TranPeriodID
        public new abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
		#endregion
		#region OrigModule
		public new abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }
		#endregion
		#region CustomerID
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

		/// <summary>
		/// The identifier of the <see cref="Customer"/> record associated with the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="BAccount.BAccountID"/> field.
		/// </value>
		[CustomerActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), Filterable = true, TabOrder = 2)]
		[PXDefault()]
		public override Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region CustomerID_Customer_acctName
		public new abstract class customerID_Customer_acctName : PX.Data.BQL.BqlString.Field<customerID_Customer_acctName> { }
		#endregion
		#region CustomerLocationID
		public new abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		/// <summary>
		/// The identifier of the <see cref="Branch">branch</see> to which the document belongs.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Branch.BranchID"/> field.
		/// </value>
		[Branch(typeof(AccessInfo.branchID), IsDetail = false, TabOrder = 0)]
		[PXFormula(typeof(Switch<
								Case<Where<PendingValue<ARInvoice.branchID>, IsPending>, Null,
								Case<Where<ARInvoice.customerLocationID, IsNotNull,
										And<Selector<ARInvoice.customerLocationID, Location.cBranchID>, IsNotNull>>,
									Selector<ARInvoice.customerLocationID, Location.cBranchID>,
								Case<Where<Current2<ARInvoice.branchID>, IsNotNull>,
									Current2<ARInvoice.branchID>>>>,
								Current<AccessInfo.branchID>>))]
		public override Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region CuryID
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		#endregion
		#region BillAddressID
		public abstract class billAddressID : PX.Data.BQL.BqlInt.Field<billAddressID> { }
		protected Int32? _BillAddressID;

		/// <summary>
		/// The identifier of the <see cref="ARAddress">Billing Address object</see>, associated with the customer.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="ARAddress.AddressID"/> field.
		/// </value>
		[PXDBInt()]
		[ARAddress(typeof(Select2<Customer,
			InnerJoin<CR.Standalone.Location, On<CR.Standalone.Location.bAccountID, Equal<Customer.bAccountID>, And<CR.Standalone.Location.locationID, Equal<Customer.defLocationID>>>,
			InnerJoin<Address, On<Address.bAccountID, Equal<Customer.bAccountID>, And<Address.addressID, Equal<Customer.defBillAddressID>>>,
			LeftJoin<ARAddress, On<ARAddress.customerID, Equal<Address.bAccountID>, And<ARAddress.customerAddressID, Equal<Address.addressID>, And<ARAddress.revisionID, Equal<Address.revisionID>, And<ARAddress.isDefaultBillAddress, Equal<True>>>>>>>>,
			Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>>))]
		public virtual Int32? BillAddressID
		{
			get
			{
				return this._BillAddressID;
			}
			set
			{
				this._BillAddressID = value;
			}
		}
		#endregion
		#region BillContactID
		public abstract class billContactID : PX.Data.BQL.BqlInt.Field<billContactID> { }

		/// <summary>
		/// The identifier of the <see cref="ARContact">Billing Contact object</see>, associated with the customer.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="ARContact.ContactID"/> field.
		/// </value>
		[PXDBInt]
		[PXSelector(typeof(ARContact.contactID), ValidateValue = false)]    //Attribute for showing contact email field on Automatic Notifications screen in the list of availible emails for
																			//Invoices and Memos screen. Relies on the work of platform, which uses PXSelector to compose email list
		[PXUIField(DisplayName = "Billing Contact", Visible = false)]		//Attribute for displaying user friendly contact email field on Automatic Notifications screen in the list of availible emails.
		[ARContact(typeof(Select2<Customer,
							InnerJoin<
									  CR.Standalone.Location, On<CR.Standalone.Location.bAccountID, Equal<Customer.bAccountID>,
								  And<CR.Standalone.Location.locationID, Equal<Customer.defLocationID>>>,
							InnerJoin<
									  Contact, On<Contact.bAccountID, Equal<Customer.bAccountID>,
								  And<Contact.contactID, Equal<Customer.defBillContactID>>>,
							LeftJoin<
									 ARContact, On<ARContact.customerID, Equal<Contact.bAccountID>,
								 And<ARContact.customerContactID, Equal<Contact.contactID>,
								 And<ARContact.revisionID, Equal<Contact.revisionID>,
								 And<ARContact.isDefaultContact, Equal<True>>>>>>>>,
			Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>>))]
		public virtual int? BillContactID
		{
			get;
			set;
		}
		#endregion
		#region MultiShipAddress
		public abstract class multiShipAddress : PX.Data.BQL.BqlBool.Field<multiShipAddress>
        {
		}
		/// <summary>
		/// The flag indicating that there are multiple shipments or orders with different addresses included in the invoice.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Multiple Ship-To Addresses", Enabled = false)]
		public virtual bool? MultiShipAddress
		{
			get;
			set;
		}
		#endregion
		#region ShipAddressID
		public abstract class shipAddressID : PX.Data.BQL.BqlInt.Field<shipAddressID> { }

		/// <summary>
		/// The identifier of the <see cref="ARAddress">Shipping Address object</see>, associated with the customer.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="ARAddress.AddressID"/> field.
		/// </value>
		[PXDBInt]
		[ARShippingAddress(typeof(Select2<Customer,
			InnerJoin<CR.Standalone.Location, On<CR.Standalone.Location.bAccountID, Equal<Customer.bAccountID>,
				And<CR.Standalone.Location.locationID, Equal<Current<ARInvoice.customerLocationID>>>>,
			InnerJoin<Address, On<Address.bAccountID, Equal<Customer.bAccountID>,
				And<Address.addressID, Equal<Location.defAddressID>>>,
			LeftJoin<ARShippingAddress, On<ARShippingAddress.customerID, Equal<Address.bAccountID>,
				And<ARShippingAddress.customerAddressID, Equal<Address.addressID>,
				And<ARShippingAddress.revisionID, Equal<Address.revisionID>,
				And<ARShippingAddress.isDefaultBillAddress, Equal<True>>>>>>>>,
			Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>>))]
		public virtual int? ShipAddressID
		{
			get;
			set;
		}
		#endregion
		#region ShipContactID
		public abstract class shipContactID : PX.Data.BQL.BqlInt.Field<shipContactID> { }

		/// <summary>
		/// The identifier of the <see cref="ARContact">Shipping Contact object</see>, associated with the customer.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="ARContact.ContactID"/> field.
		/// </value>
		[PXDBInt]
		[PXSelector(typeof(ARShippingContact.contactID), ValidateValue = false)]
		[PXUIField(DisplayName = "Shipping Contact", Visible = false)]
		[ARShippingContact(typeof(Select2<Customer,
			InnerJoin<CR.Standalone.Location, On<CR.Standalone.Location.bAccountID, Equal<Customer.bAccountID>,
				And<CR.Standalone.Location.locationID, Equal<Current<ARInvoice.customerLocationID>>>>,
			InnerJoin<Contact, On<Contact.bAccountID, Equal<Customer.bAccountID>,
				And<Contact.contactID, Equal<Location.defContactID>>>,
			LeftJoin<ARShippingContact, On<ARShippingContact.customerID, Equal<Contact.bAccountID>,
				And<ARShippingContact.customerContactID, Equal<Contact.contactID>,
				And<ARShippingContact.revisionID, Equal<Contact.revisionID>,
				And<ARShippingContact.isDefaultContact, Equal<True>>>>>>>>,
			Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>>))]
		public virtual int? ShipContactID
		{
			get;
			set;
		}
		#endregion
		#region ARAccountID
		public new abstract class aRAccountID : PX.Data.BQL.BqlInt.Field<aRAccountID> { }
		#endregion
		#region ARSubID
		public new abstract class aRSubID : PX.Data.BQL.BqlInt.Field<aRSubID> { }
		#endregion
		#region TermsID
		public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }

		/// <summary>
		/// The identifier of the <see cref="Terms">Credit Terms</see> object associated with the document.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="Customer.TermsID">credit terms</see> that are selected for the <see cref="CustomerID">customer</see>.
		/// Corresponds to the <see cref="Terms.TermsID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<Customer.termsID,
			Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>,
				And2<Where<Current<ARInvoice.docType>, NotEqual<ARDocType.creditMemo>>,
					Or<Where<Current<ARSetup.termsInCreditMemos>, Equal<True>>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.Visible)]
		[ARTermsSelector]
		[Terms(typeof(ARInvoice.docDate), typeof(ARInvoice.dueDate), typeof(ARInvoice.discDate), typeof(ARInvoice.curyOrigDocAmt), typeof(ARInvoice.curyOrigDiscAmt), typeof(ARInvoice.curyTaxTotal), typeof(ARInvoice.branchID))]
		[PXReferentialIntegrityCheck(CheckPoint = CheckPoint.BeforePersisting)]
		[PXForeignReference(typeof(Field<termsID>.IsRelatedTo<Terms.termsID>))]
        public virtual string TermsID
		{
			get;
			set;
		}
		#endregion
		#region DueDate
		public new abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }

		/// <summary>
		/// The due date of the document.
		/// </summary>
		[PXDBDate()]
		[PXUIField(DisplayName = "Due Date", Visibility = PXUIVisibility.SelectorVisible)]
		public override DateTime? DueDate
		{
			get
			{
				return this._DueDate;
			}
			set
			{
				this._DueDate = value;
			}
		}
		#endregion
		#region DiscDate
		public abstract class discDate : PX.Data.BQL.BqlDateTime.Field<discDate> { }
		protected DateTime? _DiscDate;

		/// <summary>
		/// The date when the cash discount can be taken in accordance with the <see cref="ARInvoice.TermsID">credit terms</see>.
		/// </summary>
		[PXDBDate()]
		[PXUIField(DisplayName = "Cash Discount Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DiscDate
		{
			get
			{
				return this._DiscDate;
			}
			set
			{
				this._DiscDate = value;
			}
		}
		#endregion
		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
		protected String _InvoiceNbr;

		/// <summary>
		/// The original reference number or ID assigned by the customer to the customer document.
		/// </summary>
		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "Customer Order Nbr.", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
		public virtual String InvoiceNbr
		{
			get
			{
				return this._InvoiceNbr;
			}
			set
			{
				this._InvoiceNbr = value;
			}
		}
		#endregion
		#region InvoiceDate
		public abstract class invoiceDate : PX.Data.BQL.BqlDateTime.Field<invoiceDate> { }
		protected DateTime? _InvoiceDate;

		/// <summary>
		/// The original date assigned by the customer to the customer document.
		/// </summary>
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "Customer Ref. Date")]
		public virtual DateTime? InvoiceDate
		{
			get
			{
				return this._InvoiceDate;
			}
			set
			{
				this._InvoiceDate = value;
			}
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		protected String _TaxZoneID;

		/// <summary>
		/// The identifier of the <see cref="TaxZone"/> associated with the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="TaxZone.TaxZoneID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Customer Tax Zone", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
		[PXFormula(typeof(Default<ARInvoice.customerLocationID>))]
		public virtual String TaxZoneID
		{
			get
			{
				return this._TaxZoneID;
			}
			set
			{
				this._TaxZoneID = value;
			}
		}
		#endregion
		#region TaxCalcMode
		public new abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }
		#endregion
		#region ExternalTaxExemptionNumber
		public abstract class externalTaxExemptionNumber : PX.Data.BQL.BqlString.Field<externalTaxExemptionNumber> { }

		/// <summary>
		/// The tax exemption number for reporting purposes. The field is used if the system is integrated with External Tax Calculation
		/// and the <see cref="FeaturesSet.AvalaraTax">External Tax Calculation Integration</see> feature is enabled.
		/// </summary>
		[PXDefault(typeof(Search<Location.cAvalaraExemptionNumber,
			Where<Location.bAccountID, Equal<Current<ARInvoice.customerID>>,
				And<Location.locationID, Equal<Current<ARInvoice.customerLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Exemption Number")]
		public virtual string ExternalTaxExemptionNumber { get; set; }
		#endregion
		#region AvalaraCustomerUsageType
		public abstract class avalaraCustomerUsageType : PX.Data.BQL.BqlString.Field<avalaraCustomerUsageType> { }
		protected String _AvalaraCustomerUsageType;

		/// <summary>
		/// The customer entity type for reporting purposes. The field is used if the system is integrated with External Tax Calculation
		/// and the <see cref="FeaturesSet.AvalaraTax">External Tax Calculation Integration</see> feature is enabled.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref = "TXAvalaraCustomerUsageType.ListAttribute" />.
		/// Defaults to the <see cref="Location.CAvalaraCustomerUsageType">customer entity type</see>
		/// that is specified for the <see cref="ARInvoice.customerLocationID">location of the customer</see>.
		/// </value>
		[PXDefault(
			TXAvalaraCustomerUsageType.Default,
			typeof(Search<Location.cAvalaraCustomerUsageType,
				Where<Location.bAccountID, Equal<Current<ARInvoice.customerID>>,
					And<Location.locationID, Equal<Current<ARInvoice.customerLocationID>>>>>))]
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Tax Exemption Type")]
		[TX.TXAvalaraCustomerUsageType.List]
		public virtual String AvalaraCustomerUsageType
		{
			get
			{
				return this._AvalaraCustomerUsageType;
			}
			set
			{
				this._AvalaraCustomerUsageType = value;
			}
		}
		#endregion
		#region DocDate
		public new abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		#endregion
		#region MasterRefNbr
		public abstract class masterRefNbr : PX.Data.BQL.BqlString.Field<masterRefNbr> { }
		protected String _MasterRefNbr;

		/// <summary>
		/// For the document representing one of several installments this field stores the <see cref="RefNbr"/>
		/// of the master document - the one, to which the installment belongs.
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		public virtual String MasterRefNbr
		{
			get
			{
				return this._MasterRefNbr;
			}
			set
			{
				this._MasterRefNbr = value;
			}
		}
		#endregion
		#region InstallmentCntr
		public abstract class installmentCntr : PX.Data.BQL.BqlShort.Field<installmentCntr> { }
		protected short? _InstallmentCntr;

		/// <summary>
		/// The counter of <see cref="TermsInstallment">installments</see> associated with the document.
		/// </summary>
		[PXDBShort()]
		public virtual short? InstallmentCntr
		{
			get
			{
				return this._InstallmentCntr;
			}
			set
			{
				this._InstallmentCntr = value;
			}
		}
		#endregion
		#region InstallmentNbr
		public abstract class installmentNbr : PX.Data.BQL.BqlShort.Field<installmentNbr> { }
		protected Int16? _InstallmentNbr;

		/// <summary>
		/// For the document representing one of several installments this field stores the number of the installment.
		/// </summary>
		[PXDBShort()]
		public virtual Int16? InstallmentNbr
		{
			get
			{
				return this._InstallmentNbr;
			}
			set
			{
				this._InstallmentNbr = value;
			}
		}
		#endregion
		#region OrigDocType
		public new abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
		#endregion

		#region OrigRefNbr
		public new abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
		#endregion

		#region CuryVatExemptTotal
		public abstract class curyVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<curyVatExemptTotal> { }
		protected Decimal? _CuryVatExemptTotal;

		/// <summary>
		/// The portion of the document total that is exempt from VAT.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// This field is relevant only if the <see cref="FeaturesSet.VatReporting">VAT Reporting</see> feature is enabled.
		/// </summary>
		/// <value>
		/// The value of this field is calculated as the taxable amount for the tax with <see cref="Tax.ExemptTax"/> set to <c>true</c>.
		/// </value>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.vatExemptTotal))]
		[PXUIField(DisplayName = "Tax Exempt Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryVatExemptTotal
		{
			get
			{
				return this._CuryVatExemptTotal;
			}
			set
			{
				this._CuryVatExemptTotal = value;
			}
		}
		#endregion

		#region VatExemptTaxTotal
		public abstract class vatExemptTotal : PX.Data.BQL.BqlDecimal.Field<vatExemptTotal> { }
		protected Decimal? _VatExemptTotal;

		/// <summary>
		/// The portion of the document total that is exempt from VAT.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// This field is relevant only if the <see cref="FeaturesSet.VatReporting">VAT Reporting</see> feature is enabled.
		/// </summary>
		/// <value>
		/// The value of this field is calculated as the taxable amount for the tax with <see cref="Tax.ExemptTax"/> set to <c>true</c>.
		/// </value>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? VatExemptTotal
		{
			get
			{
				return this._VatExemptTotal;
			}
			set
			{
				this._VatExemptTotal = value;
			}
		}
		#endregion

		#region CuryVatTaxableTotal
		public abstract class curyVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<curyVatTaxableTotal> { }
		protected Decimal? _CuryVatTaxableTotal;

		/// <summary>
		/// The portion of the document total that is subjected to VAT.
		/// Given in the <see cref="CuryID">currency</see> of the document.
		/// This field is relevant only if the <see cref="FeaturesSet.VatReporting">VAT Reporting</see> feature is enabled.
		/// </summary>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.vatTaxableTotal))]
		[PXUIField(DisplayName = "Taxable Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryVatTaxableTotal
		{
			get
			{
				return this._CuryVatTaxableTotal;
			}
			set
			{
				this._CuryVatTaxableTotal = value;
			}
		}
		#endregion

		#region VatTaxableTotal
		public abstract class vatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<vatTaxableTotal> { }
		protected Decimal? _VatTaxableTotal;

		/// <summary>
		/// The portion of the document total that is subjected to VAT.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// This field is relevant only if the <see cref="FeaturesSet.VatReporting">VAT Reporting</see> feature is enabled.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? VatTaxableTotal
		{
			get
			{
				return this._VatTaxableTotal;
			}
			set
			{
				this._VatTaxableTotal = value;
			}
		}
		#endregion

		#region ExternalTaxesImportInProgress
		/// <summary>
		/// Indicates that taxes were calculated by an external system
		/// </summary>
		public abstract class externalTaxesImportInProgress : PX.Data.BQL.BqlBool.Field<externalTaxesImportInProgress> { }
		[PXBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? ExternalTaxesImportInProgress
		{
			get;
			set;
		}
		#endregion
		#region DisableAutomaticTaxCalculation
		public new abstract class disableAutomaticTaxCalculation : PX.Data.BQL.BqlBool.Field<disableAutomaticTaxCalculation> { }
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		#endregion
		#region CuryOrigDocAmt
		public new abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }
		#endregion
		#region CuryRetainageTotal
		public new abstract class curyRetainageTotal : PX.Data.BQL.BqlDecimal.Field<curyRetainageTotal> { }
		#endregion
		#region CuryRetainageUnreleasedAmt
		public new abstract class curyRetainageUnreleasedAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainageUnreleasedAmt> { }
		#endregion

		#region OrigDocAmt
		public new abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }
		#endregion
		#region CuryDocBal
		public new abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
		#endregion
		#region DocBal
		public new abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
		#endregion
		#region CuryInitDocBal
		public new abstract class curyInitDocBal : PX.Data.BQL.BqlDecimal.Field<curyInitDocBal> { }

		/// <summary>
		/// The entered in migration mode balance of the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.initDocBal))]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXUIVerify(typeof(
			Where<ARInvoice.hold, Equal<True>,
				Or<ARInvoice.isMigratedRecord, NotEqual<True>,
				Or<Where<ARInvoice.curyInitDocBal, GreaterEqual<decimal0>,
					And<ARInvoice.curyInitDocBal, LessEqual<ARInvoice.curyOrigDocAmt>>>>>>),
			PXErrorLevel.Error, Common.Messages.IncorrectMigratedBalance,
			CheckOnInserted = false,
			CheckOnRowSelected = false,
			CheckOnVerify = false,
			CheckOnRowPersisting = true)]
		public override decimal? CuryInitDocBal
		{
			get;
			set;
		}
		#endregion
		#region CuryDiscBal
		public new abstract class curyDiscBal : PX.Data.BQL.BqlDecimal.Field<curyDiscBal> { }
		#endregion
		#region DiscBal
		public new abstract class discBal : PX.Data.BQL.BqlDecimal.Field<discBal> { }
		#endregion
		#region CuryOrigDiscAmt
		public new abstract class curyOrigDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDiscAmt> { }
		#endregion
		#region OrigDiscAmt
		public new abstract class origDiscAmt : PX.Data.BQL.BqlDecimal.Field<origDiscAmt> { }
		#endregion
		#region DrCr
		public abstract class drCr : PX.Data.BQL.BqlString.Field<drCr> { }
		protected string _DrCr;

		/// <summary>
		/// Read-only field indicating whether the document is of debit or credit type.
		/// </summary>
		/// <value>
		/// Possible values are <see cref="GL.DrCr.Credit"/> (for Invoice, Debit Memo, Financial Charge, Small Credit Write-Off and Cash Sale)
		/// and <see cref="GL.DrCr.Debit"/> (for Credit Memo and Cash Return).
		/// </value>
		[PXString(1, IsFixed = true)]
		public virtual string DrCr
		{
			[PXDependsOnFields(typeof(docType), typeof(pendingPayment))]
			get
			{
				if (DocType == ARDocType.PrepaymentInvoice && PendingPayment != true)
				{
					return GL.DrCr.Debit;
				}
				return ARInvoiceType.DrCr(this._DocType);
			}
			set
			{
			}
		}
		#endregion

		#region CuryFreightCost
		public abstract class curyFreightCost : PX.Data.BQL.BqlDecimal.Field<curyFreightCost> { }
		protected Decimal? _CuryFreightCost;
		/// <summary>
		/// Freight cost of the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.freightCost))]
		[PXUIField(DisplayName = "Freight Cost", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFreightCost
		{
			get
			{
				return this._CuryFreightCost;
			}
			set
			{
				this._CuryFreightCost = value;
			}
		}
		#endregion
		#region FreightCost
		public abstract class freightCost : PX.Data.BQL.BqlDecimal.Field<freightCost> { }
		protected Decimal? _FreightCost;
		/// <summary>
		/// Freight cost of the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBDecimal(4)]
		public virtual Decimal? FreightCost
		{
			get
			{
				return this._FreightCost;
			}
			set
			{
				this._FreightCost = value;
			}
		}
		#endregion
		#region CuryGoodsTotal
		public abstract class curyGoodsTotal : PX.Data.BQL.BqlDecimal.Field<curyGoodsTotal> { }
		protected Decimal? _CuryGoodsTotal;
		/// <summary>
		/// The total goods amount of the <see cref="ARTran">lines</see> of the document.
		/// Given in the <see cref="curyID">currency of the document</see>.
		/// </summary>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.goodsTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Goods Total", Enabled = false)]
		public virtual Decimal? CuryGoodsTotal
		{
			get
			{
				return this._CuryGoodsTotal;
			}
			set
			{
				this._CuryGoodsTotal = value;
			}
		}
		#endregion
		#region GoodsTotal
		public abstract class goodsTotal : PX.Data.BQL.BqlDecimal.Field<goodsTotal> { }
		protected Decimal? _GoodsTotal;
		/// <summary>
		/// The total goods amount of the <see cref="ARTran">lines</see> of the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? GoodsTotal
		{
			get
			{
				return this._GoodsTotal;
			}
			set
			{
				this._GoodsTotal = value;
			}
		}
		#endregion
		#region CuryLineTotal
		public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }
		protected Decimal? _CuryLineTotal;
		/// <summary>
		/// The total amount of the <see cref="ARTran">lines</see> of the document.
		/// Given in the <see cref="curyID">currency of the document</see>.
		/// </summary>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.lineTotal))]
		[PXUIField(DisplayName = "Detail Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryLineTotal
		{
			get
			{
				return this._CuryLineTotal;
			}
			set
			{
				this._CuryLineTotal = value;
			}
		}
		#endregion
		#region LineTotal
		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }
		protected Decimal? _LineTotal;

		/// <summary>
		/// The total amount of the <see cref="ARTran">lines</see> of the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LineTotal
		{
			get
			{
				return this._LineTotal;
			}
			set
			{
				this._LineTotal = value;
			}
		}
		#endregion
		#region CuryLineDiscTotal
		/// <inheritdoc cref="CuryLineDiscTotal"/>
		public abstract class curyLineDiscTotal : PX.Data.BQL.BqlDecimal.Field<curyLineDiscTotal> { }
		/// <summary>
		/// The total <see cref="lineDiscTotal">discount of the document</see> (in the currency of the document),
		/// which is calculated as the sum of line discounts of the order.
		/// </summary>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.lineDiscTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Line Discounts", Enabled = false)]
		public virtual Decimal? CuryLineDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region LineDiscTotal
		/// <inheritdoc cref="LineDiscTotal"/>
		public abstract class lineDiscTotal : PX.Data.BQL.BqlDecimal.Field<lineDiscTotal> { }

		/// <summary>
		/// The total line discount of the document, which is calculated as the sum of line discounts of the invoice.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Line Discounts", Enabled = false)]
		public virtual Decimal? LineDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryGroupDiscTotal
		/// <inheritdoc cref="CuryGroupDiscTotal"/>
		public abstract class curyGroupDiscTotal : PX.Data.BQL.BqlDecimal.Field<curyGroupDiscTotal> { }
		/// <summary>
		/// The total <see cref="groupDiscTotal">discount of the document</see> (in the currency of the document),
		/// which is calculated as the sum of group discounts of the invoice.
		/// </summary>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.groupDiscTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Group Discounts", Enabled = false)]
		public virtual Decimal? CuryGroupDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region GroupDiscTot
		/// <inheritdoc cref="GroupDiscTotal"/>
		public abstract class groupDiscTotal : PX.Data.BQL.BqlDecimal.Field<groupDiscTotal> { }

		/// <summary>
		/// The total group discount of the document, which is calculated as the sum of group discounts of the invoice.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Group Discounts", Enabled = false)]
		public virtual Decimal? GroupDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryDocumentDiscTotal
		/// <inheritdoc cref="CuryDocumentDiscTotal"/>
		public abstract class curyDocumentDiscTotal : PX.Data.BQL.BqlDecimal.Field<curyDocumentDiscTotal> { }
		/// <summary>
		/// The total <see cref="documentDiscTotal">discount of the document</see> (in the currency of the document),
		/// which is calculated as the sum of document discounts of the invoice.
		/// </summary>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.documentDiscTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Document Discount", Enabled = false)]
		public virtual Decimal? CuryDocumentDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region DocumentDiscTotal
		/// <inheritdoc cref="DocumentDiscTotal"/>
		public abstract class documentDiscTotal : PX.Data.BQL.BqlDecimal.Field<documentDiscTotal> { }

		/// <summary>
		/// The total document discount of the document, which is calculated as the sum of document discounts of the invoice.
		/// </summary>
		/// <remarks>
		/// <para>If the <see cref="FeaturesSet.customerDiscounts">Customer Discounts</see> feature is not enabled on
		/// the Enable/Disable Features (CS100000) form,
		/// a user can enter a document-level discount manually. This manual discount has no discount code or
		/// sequence and is not recalculated by the system. If the manual discount needs to be changed, a user has to
		/// correct it manually.</para>
		/// </remarks>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Document Discount", Enabled = false)]
		public virtual Decimal? DocumentDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryDiscTot
		public abstract class curyDiscTot : PX.Data.BQL.BqlDecimal.Field<curyDiscTot> { }

		/// <summary>
		/// The <see cref="ARInvoiceDiscountDetail.CuryDiscountAmt">group and document discount total</see> for the document.
		/// Given in the <see cref="curyID">currency of the document</see>.
		/// </summary>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.discTot))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Group and Document Discount Total", Enabled = false)]
		public virtual Decimal? CuryDiscTot
		{
			get;
			set;
		}
		#endregion
		#region DiscTot
		public abstract class discTot : PX.Data.BQL.BqlDecimal.Field<discTot> { }

		/// <summary>
		/// The <see cref="ARInvoiceDiscountDetail.DiscountAmt">group and document discount total</see> for the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DiscTot
		{
			get;
			set;
		}
		#endregion
		#region CuryOrderDiscTotal
		/// <inheritdoc cref="CuryOrderDiscTotal"/>
		public abstract class curyOrderDiscTotal : PX.Data.BQL.BqlDecimal.Field<curyOrderDiscTotal> { }

		/// <summary>
		/// The total <see cref="orderDiscTotal">discount of the document</see> (in the currency of the document),
		/// which is calculated as the sum of all group, document and line discounts of the invoice.
		/// </summary>
		[PXCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.orderDiscTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCalced(typeof(Add<ARInvoice.curyDiscTot, ARInvoice.curyLineDiscTotal>), typeof(Decimal))]
		[PXFormula(typeof(Add<ARInvoice.curyDiscTot, ARInvoice.curyLineDiscTotal>))]
		[PXUIField(DisplayName = "Discount Total", Enabled =  false)]
		public virtual Decimal? CuryOrderDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region OrderDiscTotal
		/// <inheritdoc cref="OrderDiscTotal"/>
		public abstract class orderDiscTotal : PX.Data.BQL.BqlDecimal.Field<orderDiscTotal> { }

		/// <summary>
		/// The total discount of the document, which is calculated as the sum of group, document and line discounts of the invoice.
		/// </summary>
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCalced(typeof(Add<ARInvoice.discTot, ARInvoice.lineDiscTotal>), typeof(Decimal))]
		[PXUIField(DisplayName = "Discount Total")]
		public virtual Decimal? OrderDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryMiscTot
		public abstract class curyMiscTot : PX.Data.BQL.BqlDecimal.Field<curyMiscTot> { }
		protected Decimal? _CuryMiscTot;
		/// <summary>
		/// The total misc amount of the <see cref="ARTran">lines</see> of the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.miscTot))]
		[PXUIField(DisplayName = "Misc. Total", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryMiscTot
		{
			get
			{
				return this._CuryMiscTot;
			}
			set
			{
				this._CuryMiscTot = value;
			}
		}
		#endregion
		#region MiscTot
		public abstract class miscTot : PX.Data.BQL.BqlDecimal.Field<miscTot> { }
		protected Decimal? _MiscTot;
		/// <summary>
		/// The total misc amount of the <see cref="ARTran">lines</see> of the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBDecimal(4)]
		public virtual Decimal? MiscTot
		{
			get
			{
				return this._MiscTot;
			}
			set
			{
				this._MiscTot = value;
			}
		}
		#endregion
		#region CuryGoodsExtPriceTotal
		/// <inheritdoc cref="CuryGoodsExtPriceTotal"/>
		public abstract class curyGoodsExtPriceTotal : PX.Data.BQL.BqlDecimal.Field<curyGoodsExtPriceTotal> { }

		/// <summary>
		/// The <see cref="goodsExtPriceTotal">total amount</see> on all lines of the document, except for Misc. Charges, before Line-level discounts
		/// are applied (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.goodsExtPriceTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Goods")]
		public virtual Decimal? CuryGoodsExtPriceTotal
		{
			get;
			set;
		}
		#endregion
		#region GoodsExtPriceTotal
		/// <inheritdoc cref="CuryGoodsExtPriceTotal"/>
		public abstract class goodsExtPriceTotal : PX.Data.BQL.BqlDecimal.Field<goodsExtPriceTotal> { }

		/// <summary>
		/// The total amount on all lines of the document, except for Misc. Charges, before Line-level discounts are applied.
		/// </summary>
		/// <remarks>
		/// <para>This total is calculated as the sum of the amounts in the
		/// <see cref="ARTran.curyExtPrice">Ext. Price</see> for all stock items and non-stock items that require shipment.
		/// This total does not include the freight amount.</para>
		/// <para>This field is not available for transfer orders.
		/// This field is available only if the <see cref="FeaturesSet.inventory">Inventory</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.</para>
		/// </remarks>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? GoodsExtPriceTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryMiscExtPriceTotal
		/// <inheritdoc cref="CuryMiscExtPriceTotal"/>
		public abstract class curyMiscExtPriceTotal : PX.Data.BQL.BqlDecimal.Field<curyMiscExtPriceTotal> { }

		/// <summary>
		/// The <see cref="miscTot">total amount</see> calculated as the sum of the amounts in
		/// <see cref="ARTran.curyExtPrice">Ext. Price</see> of the order non-stock items
		/// (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.miscExtPriceTotal))]
		[PXUIField(DisplayName = "Misc. Charges", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryMiscExtPriceTotal
		{
			get;
			set;
		}
		#endregion
		#region MiscExtPriceTotal
		/// <inheritdoc cref="MiscExtPriceTotal"/>
		public abstract class miscExtPriceTotal : PX.Data.BQL.BqlDecimal.Field<miscExtPriceTotal> { }

		/// <summary>
		/// The total amount calculated as the sum of the amounts in <see cref="SOLine.curyExtPrice">Ext. Price</see>
		/// of the order non-stock items.
		/// </summary>
		/// <remarks>
		/// This field is not available for transfer orders.
		/// </remarks>
		[PXDBDecimal(4)]
		public virtual Decimal? MiscExtPriceTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryDetaiExtPricelTotal
		/// <inheritdoc cref="CuryDetailExtPriceTotal"/>
		public abstract class curyDetailExtPriceTotal : PX.Data.BQL.BqlDecimal.Field<curyDetailExtPriceTotal> { }

		/// <summary>
		/// The <see cref="detailExtPriceTotal">sum</see> of the
		/// <see cref="curyGoodsExtPriceTotal">goods</see> and
		/// the <see cref="curyMiscExtPriceTotal">misc. charges amount</see> values.
		/// </summary>
		[PXCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.detailExtPriceTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCalced(typeof(Switch<Case<Where<docType, Equal<ARDocType.prepaymentInvoice>>,
			Add<ARInvoice.curyLineTotal, ARInvoice.curyLineDiscTotal>>,
			Add<ARInvoice.curyGoodsExtPriceTotal, ARInvoice.curyMiscExtPriceTotal>>), typeof(Decimal))]
		[PXFormula(typeof(Switch<Case<Where<docType, Equal<ARDocType.prepaymentInvoice>>,
			Add<ARInvoice.curyLineTotal, ARInvoice.curyLineDiscTotal>>,
			Add<ARInvoice.curyGoodsExtPriceTotal,ARInvoice.curyMiscExtPriceTotal>>))]
		[PXUIField(DisplayName = "Detail Total")]
		public virtual Decimal? CuryDetailExtPriceTotal
		{
			get;
			set;
		}
		#endregion
		#region DetailExtPriceTotal
		/// <inheritdoc cref="DetailExtPriceTotal"/>
		public abstract class detailExtPriceTotal : PX.Data.BQL.BqlDecimal.Field<detailExtPriceTotal> { }

		/// <summary>
		/// The sum of the <see cref="goodsExtPriceTotal">goods</see> and
		/// the <see cref="miscExtPriceTotal">misc. charges amount</see> values.
		/// </summary>
		[PXDecimal(4)]
		[PXDBCalced(typeof(Switch<Case<Where<docType, Equal<ARDocType.prepaymentInvoice>>,
			Add<ARInvoice.lineTotal, ARInvoice.lineDiscTotal>>,
			Add<ARInvoice.goodsExtPriceTotal, ARInvoice.miscExtPriceTotal>>), typeof(Decimal))]
		public virtual Decimal? DetailExtPriceTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxTotal
		public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }
		protected Decimal? _CuryTaxTotal;
		/// <summary>
		/// The total amount of tax associated with the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.taxTotal), SignCondition = typeof(ARDocType.PositiveAmountTypes.Contains<docType>))]
		[PXUIField(DisplayName = "Tax Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTaxTotal
		{
			get
			{
				return this._CuryTaxTotal;
			}
			set
			{
				this._CuryTaxTotal = value;
			}
		}
		#endregion
		#region TaxTotal
		public abstract class taxTotal : PX.Data.BQL.BqlDecimal.Field<taxTotal> { }
		protected Decimal? _TaxTotal;

		/// <summary>
		/// The total amount of tax associated with the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBDecimal(4, SignCondition = typeof(ARDocType.PositiveAmountTypes.Contains<docType>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TaxTotal
		{
			get
			{
				return this._TaxTotal;
			}
			set
			{
				this._TaxTotal = value;
			}
		}
		#endregion
		#region CuryFreightTot
		public abstract class curyFreightTot : PX.Data.BQL.BqlDecimal.Field<curyFreightTot> { }
		protected Decimal? _CuryFreightTot;
		/// <summary>
		/// The total amount of freight associated with the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.freightTot))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Freight Total", Enabled = false)]
		public virtual Decimal? CuryFreightTot
		{
			get
			{
				return this._CuryFreightTot;
			}
			set
			{
				this._CuryFreightTot = value;
			}
		}
		#endregion
		#region FreightTot
		public abstract class freightTot : PX.Data.BQL.BqlDecimal.Field<freightTot> { }
		protected Decimal? _FreightTot;
		/// <summary>
		/// The total amount of freight associated with the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBDecimal(4)]
		public virtual Decimal? FreightTot
		{
			get
			{
				return this._FreightTot;
			}
			set
			{
				this._FreightTot = value;
			}
		}
		#endregion
		#region CuryFreightAmt
		public abstract class curyFreightAmt : PX.Data.BQL.BqlDecimal.Field<curyFreightAmt> { }
		protected Decimal? _CuryFreightAmt;
		/// <summary>
		/// The amount of freight associated with the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.freightAmt))]
		[PXUIField(DisplayName = "Freight Price", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFreightAmt
		{
			get
			{
				return this._CuryFreightAmt;
			}
			set
			{
				this._CuryFreightAmt = value;
			}
		}
		#endregion
		#region FreightAmt
		public abstract class freightAmt : PX.Data.BQL.BqlDecimal.Field<freightAmt> { }
		protected Decimal? _FreightAmt;
		/// <summary>
		/// The amount of freight associated with the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBDecimal(4)]
		public virtual Decimal? FreightAmt
		{
			get
			{
				return this._FreightAmt;
			}
			set
			{
				this._FreightAmt = value;
			}
		}
		#endregion
		#region CuryPremiumFreightAmt
		public abstract class curyPremiumFreightAmt : PX.Data.BQL.BqlDecimal.Field<curyPremiumFreightAmt> { }
		protected Decimal? _CuryPremiumFreightAmt;
		/// <summary>
		/// The amount of premium freight associated with the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.premiumFreightAmt))]
		[PXUIField(DisplayName = "Premium Freight Price", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryPremiumFreightAmt
		{
			get
			{
				return this._CuryPremiumFreightAmt;
			}
			set
			{
				this._CuryPremiumFreightAmt = value;
			}
		}
		#endregion
		#region PremiumFreightAmt
		public abstract class premiumFreightAmt : PX.Data.BQL.BqlDecimal.Field<premiumFreightAmt> { }
		protected Decimal? _PremiumFreightAmt;
		/// <summary>
		/// The amount of premium freight associated with the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBDecimal(4)]
		public virtual Decimal? PremiumFreightAmt
		{
			get
			{
				return this._PremiumFreightAmt;
			}
			set
			{
				this._PremiumFreightAmt = value;
			}
		}
		#endregion
		#region CuryDocDisc
		[Obsolete("This field is obsolete and will be removed in 2021R1.")]
		public new abstract class curyDocDisc : PX.Data.BQL.BqlDecimal.Field<curyDocDisc> { }
		#endregion
		#region DocDisc
		[Obsolete("This field is obsolete and will be removed in 2021R1.")]
		public new abstract class docDisc : PX.Data.BQL.BqlDecimal.Field<docDisc> { }
        #endregion
        #region CuryPaymentTotal
        public abstract class curyPaymentTotal : PX.Data.BQL.BqlDecimal.Field<curyPaymentTotal> { }
        [PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.paymentTotal))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Paid", Enabled = false)]
        public virtual Decimal? CuryPaymentTotal
        {
            get;
            set;
        }
        #endregion
        #region PaymentTotal
        public abstract class paymentTotal : PX.Data.BQL.BqlDecimal.Field<paymentTotal> { }
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Paid", Enabled = false)]
        public virtual Decimal? PaymentTotal
        {
            get;
            set;
        }
        #endregion
        #region CuryBalanceWOTotal
        public abstract class curyBalanceWOTotal : PX.Data.BQL.BqlDecimal.Field<curyBalanceWOTotal> { }
		protected Decimal? _CuryBalanceWOTotal;
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.balanceWOTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Write-Off Total", Enabled = false)]
        public virtual Decimal? CuryBalanceWOTotal
        {
            get
            {
                return this._CuryBalanceWOTotal;
            }
            set
            {
                this._CuryBalanceWOTotal = value;
            }
        }
        #endregion
        #region BalanceWOTotal
        public abstract class balanceWOTotal : PX.Data.BQL.BqlDecimal.Field<balanceWOTotal> { }
		protected Decimal? _BalanceWOTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? BalanceWOTotal
        {
            get
            {
                return this._BalanceWOTotal;
            }
            set
            {
                this._BalanceWOTotal = value;
            }
        }
        #endregion

		#region CuryUnreleasedPaymentAmt
		public abstract class curyUnreleasedPaymentAmt : Data.BQL.BqlDecimal.Field<curyUnreleasedPaymentAmt> { }
		[PXCurrency(typeof(curyInfoID), typeof(unreleasedPaymentAmt))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Not Released", Enabled = false)]
		public virtual decimal? CuryUnreleasedPaymentAmt
		{
			get;
			set;
		}
		#endregion
		#region UnreleasedPaymentAmt
		public abstract class unreleasedPaymentAmt : Data.BQL.BqlDecimal.Field<unreleasedPaymentAmt> { }
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? UnreleasedPaymentAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryCCAuthorizedAmt
		public abstract class curyCCAuthorizedAmt : Data.BQL.BqlDecimal.Field<curyCCAuthorizedAmt> { }
		[PXCurrency(typeof(curyInfoID), typeof(cCAuthorizedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Authorized", Enabled = false)]
		public virtual decimal? CuryCCAuthorizedAmt
		{
			get;
			set;
		}
		#endregion
		#region CCAuthorizedAmt
		public abstract class cCAuthorizedAmt : Data.BQL.BqlDecimal.Field<cCAuthorizedAmt> { }
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CCAuthorizedAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryPaidAmt
		public abstract class curyPaidAmt : Data.BQL.BqlDecimal.Field<curyPaidAmt> { }
		[PXCurrency(typeof(curyInfoID), typeof(paidAmt))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Released", Enabled = false)]
		public virtual decimal? CuryPaidAmt
		{
			get;
			set;
		}
		#endregion
		#region PaidAmt
		public abstract class paidAmt : Data.BQL.BqlDecimal.Field<paidAmt> { }
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? PaidAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryUnpaidBalance
		public abstract class curyUnpaidBalance : Data.BQL.BqlDecimal.Field<curyUnpaidBalance> { }
		[PXDBCurrency(typeof(curyInfoID), typeof(unpaidBalance))]
		[PXFormula(typeof(Sub<curyOrigDocAmt, Add<curyPaymentTotal, Add<curyDiscAppliedAmt, curyBalanceWOTotal>>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unpaid Balance", Enabled = false)]
		public decimal? CuryUnpaidBalance
		{
			get;
			set;
		}
		#endregion
		#region UnpaidBalance
		public abstract class unpaidBalance : Data.BQL.BqlDecimal.Field<unpaidBalance> { }
		[PXDBBaseCury(typeof(branchID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public decimal? UnpaidBalance
		{
			get;
			set;
		}
		#endregion
		#region CuryDiscAppliedAmt
		public abstract class curyDiscAppliedAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscAppliedAmt> { }
		[PXDBCurrency(typeof(curyInfoID), typeof(discAppliedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.CashDiscountTaken, Enabled = false)]
		public virtual decimal? CuryDiscAppliedAmt
		{
			get;
			set;
		}
		#endregion
		#region DiscAppliedAmt
		public abstract class discAppliedAmt : PX.Data.BQL.BqlDecimal.Field<discAppliedAmt> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? DiscAppliedAmt
		{
			get;
			set;
		}
		#endregion

        #region Released
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		#endregion
		#region OpenDoc
		public new abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }
		#endregion
		#region Hold
		public new abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		#endregion
		#region BatchNbr
		public new abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		#endregion
		#region CommnPct
		public abstract class commnPct : PX.Data.BQL.BqlDecimal.Field<commnPct> { }
		protected Decimal? _CommnPct;

		/// <summary>
		/// The commission percent used for the salesperson.
		/// </summary>
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Commission %", Enabled = false)]
		public virtual Decimal? CommnPct
		{
			get
			{
				return this._CommnPct;
			}
			set
			{
				this._CommnPct = value;
			}
		}
		#endregion
		#region CuryCommnAmt
		public abstract class curyCommnAmt : PX.Data.BQL.BqlDecimal.Field<curyCommnAmt> { }
		protected Decimal? _CuryCommnAmt;

		/// <summary>
		/// The commission amount calculated on this document for the salesperson.
		/// Given in the <see cref="CuryID">currency</see> of the document.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.commnAmt))]
		//[PXFormula(typeof(Mult<ARInvoice.curyCommnblAmt, Div<ARInvoice.commnPct, decimal100>>))]
		[PXUIField(DisplayName = "Commission Amt.", Enabled = false)]
		public virtual Decimal? CuryCommnAmt
		{
			get
			{
				return this._CuryCommnAmt;
			}
			set
			{
				this._CuryCommnAmt = value;
			}
		}
		#endregion
		#region CommnAmt
		public abstract class commnAmt : PX.Data.BQL.BqlDecimal.Field<commnAmt> { }
		protected Decimal? _CommnAmt;

		/// <summary>
		/// The commission amount calculated on this document for the salesperson.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CommnAmt
		{
			get
			{
				return this._CommnAmt;
			}
			set
			{
				this._CommnAmt = value;
			}
		}
		#endregion
		#region CuryApplicationBalance
		public abstract class curyApplicationBalance : PX.Data.BQL.BqlDecimal.Field<curyApplicationBalance> { }

		[PXCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.applicationBalance), BaseCalc = false)]
		[PXUIField(DisplayName = "Application Balance")]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Add<Sub<ARInvoice.curyPaymentTotal, ARRegister.curyOrigDocAmt>, ARInvoice.curyBalanceWOTotal>))]
		public virtual decimal? CuryApplicationBalance
		{
			get;
			set;
		}
		#endregion
		#region ApplicationBalance
		public abstract class applicationBalance : PX.Data.BQL.BqlDecimal.Field<applicationBalance> { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Add<Sub<ARInvoice.paymentTotal, ARRegister.origDocAmt>, ARInvoice.balanceWOTotal>))]
        public virtual decimal? ApplicationBalance
		{
			get;
			set;
		}
		#endregion
		#region ApplyOverdueCharge
		public abstract class applyOverdueCharge : PX.Data.BQL.BqlBool.Field<applyOverdueCharge> { }
		protected bool? _ApplyOverdueCharge;

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the document can be available
		/// on the Calculate Overdue Charges (AR507000) processing form.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = Messages.ApplyOverdueCharges, Visibility = PXUIVisibility.Visible)]
		[PXDefault(true)]
		public virtual bool? ApplyOverdueCharge
		{
			get
			{
				return _ApplyOverdueCharge;
			}
			set
			{
				_ApplyOverdueCharge = value;
			}
		}
		#endregion
		#region LastFinChargeDate
		public abstract class lastFinChargeDate : PX.Data.BQL.BqlDateTime.Field<lastFinChargeDate> { }
		protected DateTime? _LastFinChargeDate;

		/// <summary>
		/// The date of the most recent <see cref="ARFinChargeTran">Financial Charge</see> associated with this document.
		/// </summary>
		[PXDate()]
		[PXUIField(DisplayName = "Last Fin. Charge Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? LastFinChargeDate
		{
			get
			{
				return this._LastFinChargeDate;
			}
			set
			{
				this._LastFinChargeDate = value;
			}
		}
		#endregion

		#region LastPaymentDate
		public abstract class lastPaymentDate : PX.Data.BQL.BqlDateTime.Field<lastPaymentDate> { }
		protected DateTime? _LastPaymentDate;

		/// <summary>
		/// The date of the most recent payment associated with this document.
		/// </summary>
		[PXDate()]
		[PXUIField(DisplayName = "Last Payment Date")]
		public virtual DateTime? LastPaymentDate
		{
			get
			{
				return this._LastPaymentDate;
			}
			set
			{
				this._LastPaymentDate = value;
			}
		}
		#endregion
		#region CuryCommnblAmt
		public abstract class curyCommnblAmt : PX.Data.BQL.BqlDecimal.Field<curyCommnblAmt> { }
		protected Decimal? _CuryCommnblAmt;

		/// <summary>
		/// The amount used as the base to calculate commission for this document.
		/// Given in the <see cref="CuryID">currency</see> of the document.
		/// </summary>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.commnblAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Commissionable", Enabled = false)]
		public virtual Decimal? CuryCommnblAmt
		{
			get
			{
				return this._CuryCommnblAmt;
			}
			set
			{
				this._CuryCommnblAmt = value;
			}
		}
		#endregion
		#region CommnblAmt
		public abstract class commnblAmt : PX.Data.BQL.BqlDecimal.Field<commnblAmt> { }
		protected Decimal? _CommnblAmt;

		/// <summary>
		/// The amount used as the base to calculate commission for this document.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CommnblAmt
		{
			get
			{
				return this._CommnblAmt;
			}
			set
			{
				this._CommnblAmt = value;
			}
		}
		#endregion
		#region CuryWhTaxBal
		public abstract class curyWhTaxBal : PX.Data.BQL.BqlDecimal.Field<curyWhTaxBal> { }

		/// <summary>
		/// The balance of tax withheld on the document.
		/// Given in the <see cref="CuryID">currency</see> of the document.
		/// </summary>
		[PXDecimal(4)]
		[PXFormula(typeof(decimal0))]
		public virtual Decimal? CuryWhTaxBal
		{
			get;
			set;
		}

		#endregion
		#region WhTaxBal
		public abstract class whTaxBal : PX.Data.BQL.BqlDecimal.Field<whTaxBal> { }

		/// <summary>
		/// The balance of tax withheld on the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// </summary>
		[PXDecimal(4)]
		[PXFormula(typeof(decimal0))]
		public virtual Decimal? WhTaxBal
		{
			get;
			set;
		}
		#endregion
		#region ScheduleID
		public new abstract class scheduleID : PX.Data.BQL.BqlString.Field<scheduleID> { }
		#endregion
		#region Scheduled
		public new abstract class scheduled : PX.Data.BQL.BqlBool.Field<scheduled> { }
		#endregion
		#region CreatedByID
		public new abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion
		#region LastModifiedByID
		public new abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion
		#region Voided
		public new abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		#endregion
		//public abstract new class dontPrint : PX.Data.BQL.BqlBool.Field<dontPrint> { }
		
		#region CreditHold
		public abstract class creditHold : PX.Data.BQL.BqlBool.Field<creditHold> { }
		protected Boolean? _CreditHold;

		/// <summary>
		/// When set to <c>true</c> indicates that the document is on credit hold,
		/// which means that the credit check failed for the <see cref="CustomerID">Customer</see>.
		/// The document can't be released while it's on credit hold.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Credit Hold")]
		public virtual Boolean? CreditHold
		{
			get
			{
				return this._CreditHold;
			}
			set
			{
				this._CreditHold = value;
			}
		}
		#endregion
		#region ApprovedCredit
		public abstract class approvedCredit : PX.Data.BQL.BqlBool.Field<approvedCredit> { }
		protected Boolean? _ApprovedCredit;

		/// <summary>
		/// Specifies (if set to <c>true</c>) that credit has been approved for the document.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? ApprovedCredit
		{
			get
			{
				return this._ApprovedCredit;
			}
			set
			{
				this._ApprovedCredit = value;
			}
		}
		#endregion
		#region ApprovedCreditAmt
		public abstract class approvedCreditAmt : PX.Data.BQL.BqlDecimal.Field<approvedCreditAmt> { }
		protected Decimal? _ApprovedCreditAmt;

		/// <summary>
		/// The amount of credit approved for the document.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ApprovedCreditAmt
		{
			get
			{
				return this._ApprovedCreditAmt;
			}
			set
			{
				this._ApprovedCreditAmt = value;
			}
		}
		#endregion
		#region ApprovedCaptureFailed
		public abstract class approvedCaptureFailed : Data.BQL.BqlBool.Field<approvedCaptureFailed> { }
		/// <summary>
		/// Specifies (if set to <c>true</c>) that invoice has been approved with CC payment that failed on attempt to capture.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? ApprovedCaptureFailed
		{
			get;
			set;
		}
		#endregion
		#region ApprovedPrepaymentRequired
		public abstract class approvedPrepaymentRequired : Data.BQL.BqlBool.Field<approvedPrepaymentRequired> { }
		/// <summary>
		/// Specifies (if set to <c>true</c>) that invoice has been approved not fully prepaid.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? ApprovedPrepaymentRequired
		{
			get;
			set;
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;

		/// <summary>
		/// The identifier of the <see cref="PMProject">project</see> associated with the document
		/// or the <see cref="PMSetup.NonProjectCode">non-project code</see>, which indicates that the document is not related to any particular project.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PMProject.ProjectID"/> field.
		/// </value>
		[NonProjectBase]
		[ProjectDefault(BatchModule.AR)]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		protected string _PaymentMethodID;

		/// <summary>
		/// The identifier of the <see cref="PaymentMethod">payment method</see> that is used for the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PaymentMethod.PaymentMethodID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Coalesce<Search2<CustomerPaymentMethod.paymentMethodID,
											InnerJoin<Customer, On<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>,
												And<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>,
												And<CustomerPaymentMethod.isActive,Equal<True>>>>,
											InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>,
												And<PaymentMethod.useForAR,Equal<True>,
												And<PaymentMethod.isActive,Equal<True>>>>>>,
											Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>>,
								   Search2<Customer.defPaymentMethodID, InnerJoin<PaymentMethod,On<PaymentMethod.paymentMethodID,Equal<Customer.defPaymentMethodID>,
												And<PaymentMethod.useForAR,Equal<True>,
												And<PaymentMethod.isActive,Equal<True>>>>>,
										 Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
								Where<PaymentMethod.isActive, Equal<True>,
								And<PaymentMethod.useForAR, Equal<True>>>>), DescriptionField = typeof(PaymentMethod.descr))]
		[PXUIFieldAttribute(DisplayName = "Payment Method")]
		[PXForeignReference(typeof(Field<ARInvoice.paymentMethodID>.IsRelatedTo<PaymentMethod.paymentMethodID>))]
		public virtual String PaymentMethodID
		{
			get
			{
				return this._PaymentMethodID;
			}
			set
			{
				this._PaymentMethodID = value;
			}
		}
		#endregion
		#region PMInstanceID
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }
		protected int? _PMInstanceID;

		/// <summary>
		/// The identifier of the <see cref="CustomerPaymentMethod">customer payment method</see> (card or account number) associated with the document.
		/// </summary>
		/// <value>
		/// Defaults according to the settings of the <see cref="CustomerPaymentMethod">customer payment methods</see>
		/// that are specified for the <see cref="CustomerID">customer</see> associated with the document.
		/// Corresponds to the <see cref="CustomerPaymentMethod.PMInstanceID"/> field.
		/// </value>
		[PXDBInt()]
		[PXUIField(DisplayName = "Card/Account Nbr.")]
		[PXDefault(typeof(
			Coalesce<
				Search2<Customer.defPMInstanceID,
					InnerJoin<CustomerPaymentMethod,
						On<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>,
						And<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>>>>,
					Where<Customer.bAccountID, Equal<Current2<ARInvoice.customerID>>,
						And<CustomerPaymentMethod.isActive, Equal<True>,
						And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<ARInvoice.paymentMethodID>>>>>>,
				Search2<CustomerPaymentMethod.pMInstanceID,
					LeftJoin<CCProcessingCenterPmntMethodBranch,
						On<CustomerPaymentMethod.paymentMethodID, Equal<CCProcessingCenterPmntMethodBranch.paymentMethodID>,
						And<CustomerPaymentMethod.cCProcessingCenterID, Equal<CCProcessingCenterPmntMethodBranch.processingCenterID>,
						And<Current2<ARInvoice.branchID>, Equal<CCProcessingCenterPmntMethodBranch.branchID>>>>>,
					Where<CustomerPaymentMethod.bAccountID, Equal<Current2<ARInvoice.customerID>>,
						And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<ARInvoice.paymentMethodID>>,
						And<CustomerPaymentMethod.isActive, Equal<True>>>>,
					OrderBy<Asc<Switch<Case<Where<CCProcessingCenterPmntMethodBranch.paymentMethodID, IsNull>, True>, False>,
						Desc<CustomerPaymentMethod.expirationDate,
						Desc<CustomerPaymentMethod.pMInstanceID>>>>>
			>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<CustomerPaymentMethod.pMInstanceID, Where<CustomerPaymentMethod.bAccountID, Equal<Current2<ARInvoice.customerID>>,
			And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<ARInvoice.paymentMethodID>>>>>), DescriptionField = typeof(CustomerPaymentMethod.descr))]
		[PXRestrictor(typeof(Where<CustomerPaymentMethod.isActive, Equal<boolTrue>, Or<CustomerPaymentMethod.pMInstanceID,
					Equal<Current<ARInvoice.pMInstanceID>>>>), AR.Messages.InactiveCreditCardMayNotBeProcessed, typeof(CustomerPaymentMethod.descr))]
		[DeprecatedProcessing]
		[DisabledProcCenter]
		[PXForeignReference(
			typeof(CompositeKey<
				Field<ARInvoice.customerID>.IsRelatedTo<CustomerPaymentMethod.bAccountID>,
				Field<ARInvoice.pMInstanceID>.IsRelatedTo<CustomerPaymentMethod.pMInstanceID>
			>))]
		public virtual int? PMInstanceID
		{
			get
			{
				return _PMInstanceID;
			}
			set
			{
				_PMInstanceID = value;
			}
		}
		#endregion

		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		protected Int32? _CashAccountID;

		/// <summary>
		/// The identifier of the <see cref="CashAccount">Cash Account</see> associated with the document.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="CustomerPaymentMethod.CashAccountID">Cash Account</see> selected for the <see cref="PMInstanceID">Customer Payment Method</see>,
		/// or (if the above is unavailable) to the Cash Account selected as the default one for Accounts Receivable in the settings of the
		/// <see cref="PaymentMethodID">Payment Method</see> (see the <see cref="PaymentMethodAccount.ARIsDefault"/> field).
		/// </value>
		[PXDefault(typeof(Coalesce<Search2<CustomerPaymentMethod.cashAccountID,
										InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CustomerPaymentMethod.cashAccountID>,
											And<PaymentMethodAccount.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>,
											And<PaymentMethodAccount.useForAR, Equal<True>>>>>,
										Where<CustomerPaymentMethod.bAccountID, Equal<Current<ARInvoice.customerID>>,
											And<CustomerPaymentMethod.pMInstanceID, Equal<Current2<ARInvoice.pMInstanceID>>>>>,
							Search2<CashAccount.cashAccountID,
								InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
									And<PaymentMethodAccount.useForAR, Equal<True>,
									And<PaymentMethodAccount.aRIsDefault, Equal<True>,
									And<PaymentMethodAccount.paymentMethodID, Equal<Current2<ARInvoice.paymentMethodID>>>>>>>,
									Where<CashAccount.branchID,Equal<Current<ARInvoice.branchID>>,
										And<Match<Current<AccessInfo.userName>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[CashAccount(typeof(ARInvoice.branchID), typeof(Search2<CashAccount.cashAccountID,
				InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
					And<PaymentMethodAccount.paymentMethodID, Equal<Current<ARInvoice.paymentMethodID>>,
					And<PaymentMethodAccount.useForAR, Equal<True>>>>>, Where<Match<Current<AccessInfo.userName>>>>), Visibility = PXUIVisibility.Visible)]
		[PXForeignReference(typeof(Field<ARInvoice.cashAccountID>.IsRelatedTo<CashAccount.cashAccountID>))]
		public virtual Int32? CashAccountID
		{
			get
			{
				return this._CashAccountID;
			}
			set
			{
				this._CashAccountID = value;
			}
		}
		#endregion
		#region Status
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		/// <summary>
		/// The status of the document.
		/// The value of the field is determined by the values of the status flags,
		/// such as <see cref="Hold"/>, <see cref="CRCaseStatusesAttribute.Released"/>, <see cref="Voided"/>, <see cref="Scheduled"/>.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="ARDocStatus.ListAttribute"/>.
		/// Defaults to <see cref="ARDocStatus.Hold"/>.
		/// </value>
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(ARDocStatus.Hold)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[ARDocStatus.List]
		public override string Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion
		#region DocDesc
		public new abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }
		#endregion
		#region Methods
		/// <summary>
		/// This attribute is intended for the status syncronization in the ARInvoice<br/>
		/// Namely, it sets a corresponeded string to the Status field, depending <br/>
		/// upon Voided, Released, CreditHold, Hold, Sheduled,Released, OpenDoc, PrintInvoice,EmailInvoice<br/>
		/// of the ARInvoice<br/>
		/// [SetStatus()]
		/// </summary>
		protected new class SetStatusAttribute : PXEventSubscriberAttribute, IPXRowUpdatingSubscriber, IPXRowInsertingSubscriber
		{
			protected class Definition : IPrefetchable
			{
				public Boolean? _PrintBeforeRelease;
				public Boolean? _EmailBeforeRelease;
				void IPrefetchable.Prefetch()
				{
					using (PXDataRecord rec =
						PXDatabase.SelectSingle<ARSetup>(
						new PXDataField("PrintBeforeRelease"),
						new PXDataField("EmailBeforeRelease")))
					{
						_PrintBeforeRelease = rec != null ? rec.GetBoolean(0) : false;
						_EmailBeforeRelease = rec != null ? rec.GetBoolean(1) : false;
					}
				}
			}

			protected Definition _Definition;

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);
				_Definition = PXDatabase.GetSlot<Definition>(typeof(SetStatusAttribute).FullName, typeof(ARSetup));
				sender.Graph.FieldUpdating.AddHandler<ARInvoice.hold>((cache, e) =>
				{
					PXBoolAttribute.ConvertValue(e);

					ARInvoice item = e.Row as ARInvoice;
					if (item != null)
					{
						StatusSet(cache, item, (bool?)e.NewValue, item.CreditHold, item.Printed != true && item.DontPrint != true, item.Emailed != true && item.DontEmail != true);
					}
				});

				sender.Graph.FieldUpdating.AddHandler<ARInvoice.creditHold>((cache, e) =>
				{
					PXBoolAttribute.ConvertValue(e);

					ARInvoice item = e.Row as ARInvoice;
					if (item != null)
					{
						StatusSet(cache, item, item.Hold, (bool?)e.NewValue, item.Printed != true && item.DontPrint != true, item.Emailed != true && item.DontEmail != true);
					}
				});

				sender.Graph.FieldUpdating.AddHandler<ARInvoice.printed>((cache, e) =>
				{
					PXBoolAttribute.ConvertValue(e);

					ARInvoice item = e.Row as ARInvoice;
					if (item != null)
					{
						StatusSet(cache, item, item.Hold, item.CreditHold, (bool?)e.NewValue != true && item.DontPrint != true, item.Emailed != true && item.DontEmail != true);
					}
				});
				sender.Graph.FieldUpdating.AddHandler<ARInvoice.emailed>((cache, e) =>
				{
					PXBoolAttribute.ConvertValue(e);

					ARInvoice item = e.Row as ARInvoice;
					if (item != null)
					{
						StatusSet(cache, item, item.Hold, item.CreditHold, item.Printed != true && item.DontPrint != true, (bool?)e.NewValue != true && item.DontEmail != true);
					}
				});
				sender.Graph.FieldUpdating.AddHandler<ARInvoice.dontPrint>((cache, e) =>
				{
					PXBoolAttribute.ConvertValue(e);

					ARInvoice item = e.Row as ARInvoice;
					if (item != null)
					{
						StatusSet(cache, item, item.Hold, item.CreditHold, (bool?)e.NewValue != true && item.Printed != true, item.Emailed != true && item.DontEmail != true);
					}
				});
				sender.Graph.FieldUpdating.AddHandler<ARInvoice.dontEmail>((cache, e) =>
				{
					PXBoolAttribute.ConvertValue(e);

					ARInvoice item = e.Row as ARInvoice;
					if (item != null)
					{
						StatusSet(cache, item, item.Hold, item.CreditHold, item.Printed != true && item.DontPrint != true, (bool?)e.NewValue != true && item.Emailed != true);
					}
				});


				sender.Graph.FieldVerifying.AddHandler<ARInvoice.status>((cache, e) => { e.NewValue = cache.GetValue<ARInvoice.status>(e.Row); });
				sender.Graph.RowSelecting.AddHandler<ARInvoice>(RowSelecting);
				sender.Graph.RowSelected.AddHandler(
					sender.GetItemType(),  (cache, e) =>
					{
						ARInvoice item = e.Row as ARInvoice;

						if (item != null)
						{
							StatusSet(cache, item, item.Hold, item.CreditHold, item.Printed != true && item.DontPrint != true, item.Emailed != true && item.DontEmail != true);
						}
					});
			}

			protected virtual void StatusSet(PXCache cache, ARInvoice item, bool? HoldVal, bool? CreditHoldVal, bool? toPrint, bool? toEmail)
			{
				if (item.Canceled == true)
				{
					item.Status = ARDocStatus.Canceled;
				}
				else if (item.Voided == true)
				{
					item.Status = ARDocStatus.Voided;
				}
				else if (HoldVal == true && item.OrigModule == BatchModule.SO)
				{
					item.Status = ARDocStatus.Hold;
				}
				else if (item.PendingProcessing == true)
				{
					item.Status = ARDocStatus.CCHold;
				}
				else if (CreditHoldVal == true && (item.Approved == true || item.DontApprove == true))
				{
					item.Status = ARDocStatus.CreditHold;
				}
				else if (HoldVal == true && item.OrigModule != BatchModule.SO)
				{
					item.Status = ARDocStatus.Hold;
				}
				else if (item.Scheduled == true)
				{
					item.Status = ARDocStatus.Scheduled;
				}
				else if (item.Rejected == true)
				{
					item.Status = ARDocStatus.Rejected;
				}
				else if (item.Released == false)
				{
					if ((item.DocType == ARDocType.Invoice || item.DocType == ARDocType.CreditMemo || item.DocType == ARDocType.DebitMemo))
					{
						if (item.Approved != true && item.DontApprove != true)
						{
							item.Status = ARDocStatus.PendingApproval;
						}
						else if (_Definition != null && _Definition._PrintBeforeRelease == true && toPrint == true)
							item.Status = ARDocStatus.PendingPrint;
						else if (_Definition != null && _Definition._EmailBeforeRelease == true && toEmail == true)
							item.Status = ARDocStatus.PendingEmail;
						else
							item.Status = ARDocStatus.Balanced;
					}
					else
						item.Status = ARDocStatus.Balanced;
				}
				else if (item.OpenDoc == true)
				{
					item.Status = item.DocType == ARDocType.PrepaymentInvoice
						? ARDocStatus.Unapplied
						: ARDocStatus.Open;
				}
				else if (item.OpenDoc == false)
				{
					item.Status = ARDocStatus.Closed;
				}
			}

			public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
			{
				ARInvoice item = (ARInvoice)e.Row;
				if (item != null)
					StatusSet(sender, item, item.Hold, item.CreditHold, item.Printed != true && item.DontPrint != true, item.Emailed != true && item.DontEmail != true);
			}

			public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
			{
				ARInvoice item = (ARInvoice)e.Row;
				StatusSet(sender, item, item.Hold, item.CreditHold, item.Printed != true && item.DontPrint != true, item.Emailed != true && item.DontEmail != true);
			}

			public virtual void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
			{
				ARInvoice item = (ARInvoice)e.NewRow;
				StatusSet(sender, item, item.Hold, item.CreditHold, item.Printed != true && item.DontPrint != true, item.Emailed != true && item.DontEmail != true);
				}
			}
		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		protected int? _WorkgroupID;

		/// <summary>
		/// The workgroup that is responsible for the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.TM.EPCompanyTree.WorkGroupID">EPCompanyTree.WorkGroupID</see> field.
		/// </value>
		[PXDBInt]
		[PXDefault(typeof(Customer.workgroupID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCompanyTreeSelector]
		[PXUIField(DisplayName = Messages.WorkGroupID, Visibility = PXUIVisibility.Visible)]
		[PXForeignReference(typeof(Field<ARInvoice.workgroupID>.IsRelatedTo<TM.EPCompanyTree.workGroupID>))]
		public virtual int? WorkgroupID
		{
			get
			{
				return this._WorkgroupID;
			}
			set
			{
				this._WorkgroupID = value;
			}
		}
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		protected int? _OwnerID;

		/// <summary>
		/// The <see cref="Contact">Contact</see> responsible for the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Contact.ContactID"/> field.
		/// </value>
		[PXDefault(typeof(Customer.ownerID), PersistingCheck = PXPersistingCheck.Nothing)]
		[Owner(typeof(ARInvoice.workgroupID), Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? OwnerID
		{
			get
			{
				return this._OwnerID;
			}
			set
			{
				this._OwnerID = value;
			}
		}
		#endregion
		#region SalesPersonID
		public new abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }

		/// <summary>
		/// The identifier of the <see cref="CustSalesPeople">salesperson</see> to whom the document belongs.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CustSalesPeople.SalesPersonID"/> field.
		/// </value>
		[SalesPerson(DisplayName = "Default Salesperson")]
		[PXDefault(typeof(Search<CustDefSalesPeople.salesPersonID, Where<CustDefSalesPeople.bAccountID, Equal<Current<ARRegister.customerID>>, And<CustDefSalesPeople.locationID, Equal<Current<ARRegister.customerLocationID>>, And<CustDefSalesPeople.isDefault, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public override Int32? SalesPersonID
		{
			get
			{
				return this._SalesPersonID;
			}
			set
			{
				this._SalesPersonID = value;
			}
		}
		#endregion
		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		/// <summary>
		/// The identifier of the <see cref="PX.Data.Note">Note</see> object associated with the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field.
		/// </value>
		[PXSearchable(SM.SearchCategory.AR | SM.SearchCategory.SO, Messages.SearchableTitleDocumentWithDynamicPrefix, new Type[] { typeof(ARInvoice.origModule), typeof(ARInvoice.docType), typeof(ARInvoice.refNbr), typeof(ARInvoice.customerID), typeof(Customer.acctName) },
			new Type[] { typeof(ARInvoice.invoiceNbr), typeof(ARInvoice.docDesc) },
			NumberFields = new Type[] { typeof(ARInvoice.refNbr) },
			Line1Format = "{0:d}{1}{2}", Line1Fields = new Type[] { typeof(ARInvoice.docDate), typeof(ARInvoice.status), typeof(ARInvoice.invoiceNbr) },
			Line2Format = "{0}", Line2Fields = new Type[] { typeof(ARInvoice.docDesc) },
			WhereConstraint = typeof(Where<ARInvoice.origModule, Equal<BatchModule.moduleSO>, Or<Where<ARRegister.docType, NotEqual<ARDocType.cashSale>, And<ARRegister.docType, NotEqual<ARDocType.cashReturn>>>>>),
			MatchWithJoin = typeof(InnerJoin<Customer, On<Customer.bAccountID, Equal<ARInvoice.customerID>>>),
			SelectForFastIndexing = typeof(Select2<ARInvoice, InnerJoin<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>>, Where<ARInvoice.origModule, Equal<BatchModule.moduleSO>, Or<Where<ARRegister.docType, NotEqual<ARDocType.cashSale>, And<ARRegister.docType, NotEqual<ARDocType.cashReturn>>>>>>)
		)]
		[PXNote(ShowInReferenceSelector = true, Selector = typeof(
			Search2<
				ARInvoice.refNbr,
			InnerJoinSingleTable<ARRegister, On<ARInvoice.docType, Equal<ARRegister.docType>,
				And<ARInvoice.refNbr, Equal<ARRegister.refNbr>>>,
			InnerJoinSingleTable<Customer, On<ARRegister.customerID, Equal<Customer.bAccountID>>>>,
			Where2<
				Where<ARRegister.origModule, In3<BatchModule.moduleAR, BatchModule.moduleEP, BatchModule.moduleSO>,
					Or<ARRegister.released, Equal<True>>>,
				And<Match<Customer, Current<AccessInfo.userName>>>>,
			OrderBy<
				Desc<ARRegister.refNbr>>>))]
		public override Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region RefNoteID
		public abstract new class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

		/// <summary>
		/// The identifier of the <see cref="PX.Data.Note">Note</see> object associated with the document reference.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field.
		/// </value>
		[PXDBGuid()]
		public override Guid? RefNoteID
		{
			get
			{
				return this._RefNoteID;
			}
			set
			{
				this._RefNoteID = value;
			}
		}
		#endregion

		#region Hidden
		public abstract class hidden : PX.Data.BQL.BqlBool.Field<hidden> { }
		protected Boolean? _Hidden = false;

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the document can be associated with only one sales order
		/// (which happens when <see cref="SO.SOOrder.BillSeparately"/> is set to <c>true</c> for the sales order).
		/// </summary>
		[PXBool()]
		public virtual Boolean? Hidden
		{
			get
			{
				return this._Hidden;
			}
			set
			{
				this._Hidden = value;
			}
		}
		#endregion
		#region HiddenOrderType
		public abstract class hiddenOrderType : PX.Data.BQL.BqlString.Field<hiddenOrderType> { }
		protected string _HiddenOrderType;

		/// <summary>
		/// The <see cref="SO.SOOrder.OrderType"/> type of the related sales order when the document can be associated
		/// with only one sales order (which happens when <see cref="SO.SOOrder.BillSeparately"/> is set to <c>true</c> for the sales order).
		/// </summary>
		[PXString()]
		public virtual string HiddenOrderType
		{
			get
			{
				return this._HiddenOrderType;
			}
			set
			{
				this._HiddenOrderType = value;
			}
		}
		#endregion
		#region HiddenOrderNbr
		public abstract class hiddenOrderNbr : PX.Data.BQL.BqlString.Field<hiddenOrderNbr> { }
		protected string _HiddenOrderNbr;

		/// <summary>
		/// The <see cref="SO.SOOrder.OrderNbr"/> reference number of the related sales order when the document can be associated
		/// with only one sales order (which happens when <see cref="SO.SOOrder.BillSeparately"/> is set to <c>true</c> for the sales order).
		/// </summary>
		[PXString()]
		public virtual string HiddenOrderNbr
		{
			get
			{
				return this._HiddenOrderNbr;
			}
			set
			{
				this._HiddenOrderNbr = value;
			}
		}
		#endregion

		#region HiddenByShipment
		public abstract class hiddenByShipment : PX.Data.BQL.BqlBool.Field<hiddenByShipment>
        {
		}
		protected bool? _HiddenByShipment = false;
		/// <summary>
		/// Specifies (if set to <c>true</c>) that the document can be associated with only one shipment.
		/// </summary>
		[PXBool]
		public virtual bool? HiddenByShipment
		{
			get { return _HiddenByShipment; }
			set { _HiddenByShipment = value; }
		}
		#endregion
		#region HiddenShipmentType
		public abstract class hiddenShipmentType : PX.Data.BQL.BqlString.Field<hiddenShipmentType>
        {
		}
		/// <summary>
		/// The <see cref="SO.SOShipment.ShipmentType"/> type of the related shipment when the document can be associated with only one shipment.
		/// </summary>
		[PXString]
		public virtual string HiddenShipmentType
		{
			get;
			set;
		}
		#endregion
		#region HiddenShipmentNbr
		public abstract class hiddenShipmentNbr : PX.Data.BQL.BqlString.Field<hiddenShipmentNbr>
        {
		}
		/// <summary>
		/// The <see cref="SO.SOShipment.ShipmentNbr"/> reference number of the related shipment when the document can be associated with only one shipment.
		/// </summary>
		[PXString]
		public virtual string HiddenShipmentNbr
		{
			get;
			set;
		}
		#endregion

		#region IsTaxValid
		public new abstract class isTaxValid : PX.Data.BQL.BqlBool.Field<isTaxValid> { }
		#endregion
		#region IsTaxPosted
		public new abstract class isTaxPosted : PX.Data.BQL.BqlBool.Field<isTaxPosted> { }
		#endregion
		#region IsTaxSaved
		public new abstract class isTaxSaved : PX.Data.BQL.BqlBool.Field<isTaxSaved> { }
		#endregion
		#region NonTaxable
		public new abstract class nonTaxable : PX.Data.BQL.BqlBool.Field<nonTaxable> { }
		#endregion
		#region ApplyPaymentWhenTaxAvailable
		public abstract class applyPaymentWhenTaxAvailable : PX.Data.BQL.BqlBool.Field<applyPaymentWhenTaxAvailable> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the Avalara taxes should be included in the document balance calculation
		/// (because these taxes will be calculated only during the release process).
		/// </summary>
		[PXBool()]
		public virtual bool? ApplyPaymentWhenTaxAvailable { get; set; }

		#endregion
		#region IsPaymentsTransferred
		/// <summary>
		/// A Boolean field that indicates whether the payments and prepayment applied to the related sales orders
		/// should be transferred to the invoice during the document creation.
		/// When set to <see langword="false"/>, the payments and prepayments will not be transferred to the invoice
		/// during the document creation but will be transferred within the <b>Complete Processing</b> actions
		/// execution when all orders are already added to the invoice.
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		public virtual Boolean? IsPaymentsTransferred { get; set; }
		public abstract class isPaymentsTransferred : PX.Data.BQL.BqlBool.Field<isPaymentsTransferred> { }
		#endregion
		#region ProformaExists
		public abstract class proformaExists : PX.Data.BQL.BqlBool.Field<proformaExists> { }

		/// <summary>
		/// If true a corresponding proforma document exists thus making this document (or part of it) read-only.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Pro Forma Invoice Exists")]
		public virtual Boolean? ProformaExists
		{
			get;
			set;

		}
		#endregion

		#region Revoked
		public abstract class revoked : PX.Data.BQL.BqlBool.Field<revoked> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the document has been revoked.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = Messages.Revoked, Enabled = true, Visible = false)]
		public virtual bool? Revoked
		{
			get;
			set;
		}
		#endregion

		#region PendingPPD
		public new abstract class pendingPPD : PX.Data.BQL.BqlBool.Field<pendingPPD> { }
		#endregion
		#region IsMigratedRecord
		public new abstract class isMigratedRecord : PX.Data.BQL.BqlBool.Field<isMigratedRecord> { }
		#endregion
		#region PaymentsByLinesAllowed
		public new abstract class paymentsByLinesAllowed : PX.Data.BQL.BqlBool.Field<paymentsByLinesAllowed> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Pay by Line",
			Visibility = PXUIVisibility.Visible,
			FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		[PXDefault(false)]
		public override bool? PaymentsByLinesAllowed
		{
			get;
			set;
		}
		#endregion

		#region RetainageApply
		public new abstract class retainageApply : PX.Data.BQL.BqlBool.Field<retainageApply> { }
		#endregion
		#region IsRetainageDocument
		public new abstract class isRetainageDocument : PX.Data.BQL.BqlBool.Field<isRetainageDocument> { }
		#endregion
		#region CuryOrigDocAmtWithRetainageTotal
		public new abstract class curyOrigDocAmtWithRetainageTotal : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmtWithRetainageTotal> { }
		#endregion

		#region DisableAutomaticDiscountCalculation
		public abstract class disableAutomaticDiscountCalculation : PX.Data.BQL.BqlBool.Field<disableAutomaticDiscountCalculation> { }
		protected Boolean? _DisableAutomaticDiscountCalculation;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Disable Automatic Discount Update")]
		public virtual Boolean? DisableAutomaticDiscountCalculation
		{
			get { return this._DisableAutomaticDiscountCalculation; }
			set { this._DisableAutomaticDiscountCalculation = value; }
		}
		#endregion
		#region DeferDiscountCalculation
		public abstract class deferPriceDiscountRecalculation : PX.Data.BQL.BqlBool.Field<deferPriceDiscountRecalculation> { }
		[PXBool]
		[PXUIField(DisplayName = "Defer Price/Discount Recalculation")]
		public virtual Boolean? DeferPriceDiscountRecalculation
		{
			get; set;
		}
		#endregion
		#region IsPriceAndDiscountsValid
		public abstract class isPriceAndDiscountsValid : PX.Data.BQL.BqlBool.Field<disableAutomaticDiscountCalculation> { }
		[PXBool]
		[PXUIField(DisplayName = "Prices and discounts are up to date.", Enabled = false)]
		public virtual Boolean? IsPriceAndDiscountsValid
		{
			get; set;
		} = true;
		#endregion

		#region CorrectionDocType
		/// <exclude/>
		public abstract class correctionDocType : Data.BQL.BqlString.Field<correctionDocType>
		{
		}
		/// <summary>
		/// The type of the correction document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="DocType"/> field.
		/// </value>
		[PXString(3, IsFixed = true)]
		[ARInvoiceType.List]
		public virtual string CorrectionDocType
		{
			get;
			set;
		}
		#endregion
		#region CorrectionRefNbr
		/// <exclude/>
		public abstract class correctionRefNbr : Data.BQL.BqlString.Field<correctionRefNbr>
		{
		}
		/// <summary>
		/// The reference number of the correction document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="RefNbr"/> field.
		/// </value>
		[PXString(15, IsUnicode = true)]
		public virtual string CorrectionRefNbr
		{
			get;
			set;
		}
		#endregion
		#region IsUnderCancellation
		/// <exclude/>
		public abstract class isUnderCancellation : Data.BQL.BqlBool.Field<isUnderCancellation>
		{
		}
		/// <summary>
		/// When set to <c>true</c>, indicates that Cancel action was applied to the invoice.
		/// </summary>
		[PXBool]
		public virtual bool? IsUnderCancellation
		{
			get;
			set;
		}
		#endregion

		#region PendingProcessingCntr
		public new abstract class pendingProcessingCntr : Data.BQL.BqlInt.Field<pendingProcessingCntr> { }
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? PendingProcessingCntr
		{
			get;
			set;
	}
		#endregion
		#region PendingProcessing
		public new abstract class pendingProcessing : Data.BQL.BqlBool.Field<pendingProcessing> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXFormula(typeof(Switch<Case<
			Where<pendingProcessingCntr, Greater<int0>,
				And<origModule, Equal<BatchModule.moduleSO>,
				And<docType, In3<ARDocType.invoice, ARDocType.debitMemo>,
				And<released, Equal<False>>>>>, True>, False>))]
		public override bool? PendingProcessing
		{
			get => base.PendingProcessing;
			set => base.PendingProcessing = value;
		}
		#endregion
		#region CaptureFailedCntr
		public new abstract class captureFailedCntr : Data.BQL.BqlInt.Field<captureFailedCntr> { }
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? CaptureFailedCntr
		{
			get;
			set;
		}
		#endregion

		public new abstract class documentKey : PX.Data.BQL.BqlString.Field<documentKey> { }

		#region IsHiddenInIntercompanySales
		public abstract class isHiddenInIntercompanySales : PX.Data.BQL.BqlBool.Field<isHiddenInIntercompanySales> { }
		/// <summary>
		/// Specifies (if set to <c>true</c>) that the document is not allowed to generate intercompany bill
		[PXDBBool]
		[PXUIField(DisplayName = "Exclude from Intercompany Processing", FieldClass = nameof(FeaturesSet.InterBranch))]
		[PXDefault(false)]
		public virtual bool? IsHiddenInIntercompanySales
		{
			get;
			set;
		}
		#endregion
		#region IsLoadApplications
		public abstract class isLoadApplications : PX.Data.BQL.BqlBool.Field<isLoadApplications> { }
		/// <summary>
		/// If <c>true</c>, available applications are not loaded automatically during an invoice creation
		/// that is executed in an import scenario or an API call. If the value is <c>false</c>,
		/// the automatic loading is on. The default value is <c>true</c>.
		/// </summary>
		[PXBool]
		public virtual bool? IsLoadApplications
		{
			get;
			set;
		}
		#endregion

		#region PendingPayment
		public new abstract class pendingPayment : PX.Data.BQL.BqlBool.Field<pendingPayment> { }
		#endregion

		#region AuthorizedPaymentCntr
		public abstract class authorizedPaymentCntr : PX.Data.BQL.BqlInt.Field<authorizedPaymentCntr> { }

		/// <summary>
		/// The counter of the applied payments in pre-authorized status.
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? AuthorizedPaymentCntr
		{
			get;
			set;
		}
		#endregion
		public new abstract class printInvoice : PX.Data.BQL.BqlBool.Field<printInvoice> { }
		public new abstract class printed: PX.Data.BQL.BqlBool.Field<printed> { }
		public new abstract class dontEmail : PX.Data.BQL.BqlBool.Field<dontEmail> { }
		public new abstract class emailed : PX.Data.BQL.BqlBool.Field<emailed> { }

	}

	[Serializable()]
	[PXHidden]
	[PXCacheName(Messages.ARInvoiceNbr)]
	public partial class ARInvoiceNbr : PXBqlTable, PX.Data.IBqlTable
	{
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected String _DocType;
		[PXDBString(3, IsKey = true)]
		[PXDefault()]
		public virtual String DocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected String _RefNbr;
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXDefault()]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
		protected Guid? _RefNoteID;
		[PXDBGuid()]
		public virtual Guid? RefNoteID
		{
			get
			{
				return this._RefNoteID;
			}
			set
			{
				this._RefNoteID = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
	}
		#endregion
	}

	public class LastFinchargeDateAttribute : PXDBScalarAttribute
	{
		public LastFinchargeDateAttribute() : base(typeof(
			Search<ARFinChargeTran.docDate,
				Where<ARFinChargeTran.origDocType, Equal<ARInvoice.docType>,
				And<ARFinChargeTran.origRefNbr, Equal<ARInvoice.refNbr>>>,
				OrderBy<Desc<ARFinChargeTran.docDate>>>)) { }
	}

	public class LastPaymentDateAttribute : PXDBScalarAttribute
	{
		public LastPaymentDateAttribute() : base(typeof(
			Search<ARAdjust.adjgDocDate,
				Where<ARAdjust.adjdDocType, Equal<ARInvoice.docType>,
				And<ARAdjust.adjdRefNbr, Equal<ARInvoice.refNbr>>>,
				OrderBy<Desc<ARAdjust.adjgDocDate>>>)) { }
	}
}

	namespace PX.Objects.AR.Standalone
{
	[Serializable()]
	[PXHidden(ServiceVisible = false)]
	public partial class ARInvoice : PXBqlTable, PX.Data.IBqlTable
	{
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected string _DocType;
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		public virtual String DocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected string _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region BillAddressID
		public abstract class billAddressID : PX.Data.BQL.BqlInt.Field<billAddressID> { }
		protected Int32? _BillAddressID;
		[PXDBInt()]
		public virtual Int32? BillAddressID
		{
			get
			{
				return this._BillAddressID;
			}
			set
			{
				this._BillAddressID = value;
			}
		}
		#endregion
		#region BillContactID
		public abstract class billContactID : PX.Data.BQL.BqlInt.Field<billContactID> { }
		protected Int32? _BillContactID;
		[PXDBInt()]
		public virtual Int32? BillContactID
		{
			get
			{
				return this._BillContactID;
			}
			set
			{
				this._BillContactID = value;
			}
		}
		#endregion
		#region ShipAddressID
		public abstract class shipAddressID : PX.Data.BQL.BqlInt.Field<shipAddressID> { }

		[PXDBInt]
		public virtual int? ShipAddressID
		{
			get;
			set;
		}
		#endregion
		#region ShipContactID
		public abstract class shipContactID : PX.Data.BQL.BqlInt.Field<shipContactID> { }

		[PXDBInt]
		public virtual int? ShipContactID
		{
			get;
			set;
		}
		#endregion
		#region TermsID
		public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }
		protected String _TermsID;
		[PXDBString(10, IsUnicode = true)]
		public virtual String TermsID
		{
			get
			{
				return this._TermsID;
			}
			set
			{
				this._TermsID = value;
			}
		}
		#endregion
		#region DiscDate
		public abstract class discDate : PX.Data.BQL.BqlDateTime.Field<discDate> { }
		protected DateTime? _DiscDate;
		[PXDBDate()]
		public virtual DateTime? DiscDate
		{
			get
			{
				return this._DiscDate;
			}
			set
			{
				this._DiscDate = value;
			}
		}
		#endregion
		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
		protected String _InvoiceNbr;
		[PXDBString(40, IsUnicode = true)]
		public virtual String InvoiceNbr
		{
			get
			{
				return this._InvoiceNbr;
			}
			set
			{
				this._InvoiceNbr = value;
			}
		}
		#endregion
		#region InvoiceDate
		public abstract class invoiceDate : PX.Data.BQL.BqlDateTime.Field<invoiceDate> { }
		protected DateTime? _InvoiceDate;
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		public virtual DateTime? InvoiceDate
		{
			get
			{
				return this._InvoiceDate;
			}
			set
			{
				this._InvoiceDate = value;
			}
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		protected String _TaxZoneID;
		[PXDBString(10, IsUnicode = true)]
		public virtual String TaxZoneID
		{
			get
			{
				return this._TaxZoneID;
			}
			set
			{
				this._TaxZoneID = value;
			}
		}
		#endregion
		#region ExternalTaxExemptionNumber
		public abstract class externalTaxExemptionNumber : PX.Data.BQL.BqlString.Field<externalTaxExemptionNumber> { }

		[PXDBString(30, IsUnicode = true)]
		public virtual string ExternalTaxExemptionNumber { get; set; }
		#endregion
		#region AvalaraCustomerUsageType
		public abstract class avalaraCustomerUsageType : PX.Data.BQL.BqlString.Field<avalaraCustomerUsageType> { }
		protected String _AvalaraCustomerUsageType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TXAvalaraCustomerUsageType.Default)]
		public virtual String AvalaraCustomerUsageType
		{
			get
			{
				return this._AvalaraCustomerUsageType;
			}
			set
			{
				this._AvalaraCustomerUsageType = value;
			}
		}
		#endregion
		#region MasterRefNbr
		public abstract class masterRefNbr : PX.Data.BQL.BqlString.Field<masterRefNbr> { }
		protected String _MasterRefNbr;
		[PXDBString(15, IsUnicode = true)]
		public virtual String MasterRefNbr
		{
			get
			{
				return this._MasterRefNbr;
			}
			set
			{
				this._MasterRefNbr = value;
			}
		}
		#endregion
		#region InstallmentNbr
		public abstract class installmentNbr : PX.Data.BQL.BqlShort.Field<installmentNbr> { }
		protected Int16? _InstallmentNbr;
		[PXDBShort()]
		public virtual Int16? InstallmentNbr
		{
			get
			{
				return this._InstallmentNbr;
			}
			set
			{
				this._InstallmentNbr = value;
			}
		}
		#endregion
		#region CuryTaxTotal
		public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }
		protected Decimal? _CuryTaxTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTaxTotal
		{
			get
			{
				return this._CuryTaxTotal;
			}
			set
			{
				this._CuryTaxTotal = value;
			}
		}
		#endregion
		#region TaxTotal
		public abstract class taxTotal : PX.Data.BQL.BqlDecimal.Field<taxTotal> { }
		protected Decimal? _TaxTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TaxTotal
		{
			get
			{
				return this._TaxTotal;
			}
			set
			{
				this._TaxTotal = value;
			}
		}
		#endregion
		#region CuryLineTotal
		public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }
		protected Decimal? _CuryLineTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryLineTotal
		{
			get
			{
				return this._CuryLineTotal;
			}
			set
			{
				this._CuryLineTotal = value;
			}
		}
		#endregion
		#region LineTotal
		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }
		protected Decimal? _LineTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LineTotal
		{
			get
			{
				return this._LineTotal;
			}
			set
			{
				this._LineTotal = value;
			}
		}
		#endregion

		#region CuryLineDiscTotal
		public abstract class curyLineDiscTotal : PX.Data.BQL.BqlDecimal.Field<curyLineDiscTotal> { }
		/// <summary>
		/// The total <see cref="lineDiscTotal">discount of the document</see> (in the currency of the document),
		/// which is calculated as the sum of line discounts of the order.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Line Discounts", Enabled = false, Required = false)]
		public virtual Decimal? CuryLineDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region LineDiscTotal
		public abstract class lineDiscTotal : PX.Data.BQL.BqlDecimal.Field<lineDiscTotal> { }

		/// <summary>
		/// The total line discount of the document, which is calculated as the sum of line discounts of the invoice.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Line Discounts", Enabled = false)]
		public virtual Decimal? LineDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryGoodsExtPriceTotal
		/// <inheritdoc cref="CuryGoodsExtPriceTotal"/>
		public abstract class curyGoodsExtPriceTotal : PX.Data.BQL.BqlDecimal.Field<curyGoodsExtPriceTotal> { }

		/// <summary>
		/// The <see cref="goodsExtPriceTotal">total amount</see> on all lines of the document, except for Misc. Charges and null or empty Line Types
		/// (in the currency of the document).
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Goods")]
		public virtual Decimal? CuryGoodsExtPriceTotal
		{
			get;
			set;
		}
		#endregion
		#region GoodsExtPriceTotal
		/// <inheritdoc cref="CuryGoodsExtPriceTotal"/>
		public abstract class goodsExtPriceTotal : PX.Data.BQL.BqlDecimal.Field<goodsExtPriceTotal> { }

		/// <summary>
		/// The total amount on all lines of the document, except for Misc. Charges and null or empty Line Types
		/// (in base currency).
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? GoodsExtPriceTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryMiscExtPriceTotal
		/// <inheritdoc cref="CuryMiscExtPriceTotal"/>
		public abstract class curyMiscExtPriceTotal : PX.Data.BQL.BqlDecimal.Field<curyMiscExtPriceTotal> { }

		/// <summary>
		/// The <see cref="miscTot">total amount</see> calculated as the sum of the amounts in
		/// <see cref="ARTran.curyExtPrice">Ext. Price</see>
		/// (in the currency of the document).
		/// </summary>
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Misc. Charges", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryMiscExtPriceTotal
		{
			get;
			set;
		}
		#endregion
		#region MiscExtPriceTotal
		/// <inheritdoc cref="MiscExtPriceTotal"/>
		public abstract class miscExtPriceTotal : PX.Data.BQL.BqlDecimal.Field<miscExtPriceTotal> { }

		/// <summary>
		/// The total amount calculated as the sum of the amounts in <see cref="ARTran.curyExtPrice">Ext. Price</see>
		/// (in base currency).
		/// </summary>
		[PXDBDecimal(4)]
		public virtual Decimal? MiscExtPriceTotal
		{
			get;
			set;
		}
		#endregion

		#region CuryVatTaxableTotal
		public abstract class curyVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<curyVatTaxableTotal> { }
		protected Decimal? _CuryVatTaxableTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryVatTaxableTotal
		{
			get
			{
				return this._CuryVatTaxableTotal;
			}
			set
			{
				this._CuryVatTaxableTotal = value;
			}
		}
		#endregion

		#region VatTaxableTotal
		public abstract class vatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<vatTaxableTotal> { }
		protected Decimal? _VatTaxableTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? VatTaxableTotal
		{
			get
			{
				return this._VatTaxableTotal;
			}
			set
			{
				this._VatTaxableTotal = value;
			}
		}
		#endregion

		#region CuryVatExemptTotal
		public abstract class curyVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<curyVatExemptTotal> { }
		protected Decimal? _CuryVatExemptTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryVatExemptTotal
		{
			get
			{
				return this._CuryVatExemptTotal;
			}
			set
			{
				this._CuryVatExemptTotal = value;
			}
		}
		#endregion

		#region VatExemptTotal
		public abstract class vatExemptTotal : PX.Data.BQL.BqlDecimal.Field<vatExemptTotal> { }
		protected Decimal? _VatExemptTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? VatExemptTotal
		{
			get
			{
				return this._VatExemptTotal;
			}
			set
			{
				this._VatExemptTotal = value;
			}
		}
		#endregion

		#region CommnPct
		public abstract class commnPct : PX.Data.BQL.BqlDecimal.Field<commnPct> { }
		protected Decimal? _CommnPct;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CommnPct
		{
			get
			{
				return this._CommnPct;
			}
			set
			{
				this._CommnPct = value;
			}
		}
		#endregion
		#region CuryCommnAmt
		public abstract class curyCommnAmt : PX.Data.BQL.BqlDecimal.Field<curyCommnAmt> { }
		protected Decimal? _CuryCommnAmt;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal(4)]
		public virtual Decimal? CuryCommnAmt
		{
			get
			{
				return this._CuryCommnAmt;
			}
			set
			{
				this._CuryCommnAmt = value;
			}
		}
		#endregion
		#region CommnAmt
		public abstract class commnAmt : PX.Data.BQL.BqlDecimal.Field<commnAmt> { }
		protected Decimal? _CommnAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CommnAmt
		{
			get
			{
				return this._CommnAmt;
			}
			set
			{
				this._CommnAmt = value;
			}
		}
		#endregion
		#region CuryCommnblAmt
		public abstract class curyCommnblAmt : PX.Data.BQL.BqlDecimal.Field<curyCommnblAmt> { }
		protected Decimal? _CuryCommnblAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryCommnblAmt
		{
			get
			{
				return this._CuryCommnblAmt;
			}
			set
			{
				this._CuryCommnblAmt = value;
			}
		}
		#endregion
		#region CommnblAmt
		public abstract class commnblAmt : PX.Data.BQL.BqlDecimal.Field<commnblAmt> { }
		protected Decimal? _CommnblAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CommnblAmt
		{
			get
			{
				return this._CommnblAmt;
			}
			set
			{
				this._CommnblAmt = value;
			}
		}
		#endregion
		//public abstract new class dontPrint : PX.Data.BQL.BqlBool.Field<dontPrint> { }
		#region CreditHold
		public abstract class creditHold : PX.Data.BQL.BqlBool.Field<creditHold> { }
		protected Boolean? _CreditHold;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Credit Hold")]
		public virtual Boolean? CreditHold
		{
			get
			{
				return this._CreditHold;
			}
			set
			{
				this._CreditHold = value;
			}
		}
		#endregion
		#region ApprovedCredit
		public abstract class approvedCredit : PX.Data.BQL.BqlBool.Field<approvedCredit> { }
		protected Boolean? _ApprovedCredit;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? ApprovedCredit
		{
			get
			{
				return this._ApprovedCredit;
			}
			set
			{
				this._ApprovedCredit = value;
			}
		}
		#endregion
		#region ApprovedCreditAmt
		public abstract class approvedCreditAmt : PX.Data.BQL.BqlDecimal.Field<approvedCreditAmt> { }
		protected Decimal? _ApprovedCreditAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ApprovedCreditAmt
		{
			get
			{
				return this._ApprovedCreditAmt;
			}
			set
			{
				this._ApprovedCreditAmt = value;
			}
		}
		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		protected int? _WorkgroupID;
		[PXDBInt]
		public virtual int? WorkgroupID
		{
			get
			{
				return this._WorkgroupID;
			}
			set
			{
				this._WorkgroupID = value;
			}
		}
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		protected int? _OwnerID;
		[PXDBInt]
		public virtual int? OwnerID
		{
			get
			{
				return this._OwnerID;
			}
			set
			{
				this._OwnerID = value;
			}
		}
		#endregion
		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
		protected Guid? _RefNoteID;
		[PXDBGuid()]
		public virtual Guid? RefNoteID
		{
			get
			{
				return this._RefNoteID;
			}
			set
			{
				this._RefNoteID = value;
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PXDBInt()]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion

		#region IsPaymentsTransferred
		/// <summary>
		/// A Boolean field that indicates whether the payments and prepayment applied to the related sales orders
		/// should be transferred to the invoice during the document creation.
		/// When set to <see langword="false"/>, the payments and prepayments will not be transferred to the invoice
		/// during the document creation but will be transferred within the <b>Complete Processing</b> actions
		/// execution when all orders are already added to the invoice.
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		public virtual Boolean? IsPaymentsTransferred { get; set; }
		public abstract class isPaymentsTransferred : PX.Data.BQL.BqlBool.Field<isPaymentsTransferred> { }
		#endregion

		#region Revoked
		public abstract class revoked : PX.Data.BQL.BqlBool.Field<revoked> { }
		protected Boolean? _Revoked;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Revoked", Enabled = true, Visible = false)]
		public virtual Boolean? Revoked
		{
			get
			{
				return this._Revoked;
			}
			set
			{
				this._Revoked = value;
			}
		}
		#endregion
		#region ApplyOverdueCharge
		public abstract class applyOverdueCharge : PX.Data.BQL.BqlBool.Field<applyOverdueCharge> { }
		protected Boolean? _ApplyOverdueCharge;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? ApplyOverdueCharge
		{
			get
			{
				return _ApplyOverdueCharge;
			}
			set
			{
				_ApplyOverdueCharge = value;
			}
		}
        #endregion

		#region CuryPaymentTotal
		public abstract class curyPaymentTotal : PX.Data.BQL.BqlDecimal.Field<curyPaymentTotal> { }
		[PXDBDecimal()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryPaymentTotal
		{
			get;
			set;
		}
		#endregion

		#region PaymentTotal
		public abstract class paymentTotal : PX.Data.BQL.BqlDecimal.Field<paymentTotal> { }
		[PXDBDecimal()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? PaymentTotal
		{
			get;
			set;
		}
		#endregion

		#region CuryBalanceWOTotal
		public abstract class curyBalanceWOTotal : PX.Data.BQL.BqlDecimal.Field<curyBalanceWOTotal> { }
		[PXDBDecimal()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryBalanceWOTotal
		{
			get;
			set;
		}
		#endregion

		#region BalanceWOTotal
		public abstract class balanceWOTotal : PX.Data.BQL.BqlDecimal.Field<balanceWOTotal> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BalanceWOTotal
		{
			get;
			set;
		}
		#endregion

		#region CuryUnpaidBalance
		public abstract class curyUnpaidBalance : Data.BQL.BqlDecimal.Field<curyUnpaidBalance> { }
		[PXDBDecimal()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public decimal? CuryUnpaidBalance
		{
			get;
			set;
		}
		#endregion

		#region UnpaidBalance
		public abstract class unpaidBalance : Data.BQL.BqlDecimal.Field<unpaidBalance> { }
		[PXDBDecimal()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public decimal? UnpaidBalance
		{
			get;
			set;
		}
		#endregion

		#region CuryDiscAppliedAmt
		public abstract class curyDiscAppliedAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscAppliedAmt> { }
		[PXDBDecimal()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryDiscAppliedAmt
		{
			get;
			set;
		}
		#endregion

		#region DiscAppliedAmt
		public abstract class discAppliedAmt : PX.Data.BQL.BqlDecimal.Field<discAppliedAmt> { }
		[PXDBDecimal()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? DiscAppliedAmt
		{
			get;
			set;
		}
		#endregion
    }
}
