using System;
using System.Diagnostics;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CM;//TODO on switching to Extensions: remove SOOrder_CuryDocBal_CacheAttached(PXCache sender) from ARPaymentSOBalanceCalculator
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.WorkflowAPI;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.Common;
using PX.Objects.Common.Attributes;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.IN.RelatedItems;
using PX.Objects.PM;
using PX.Objects.SO.Attributes;
using PX.Objects.SO.Interfaces;
using PX.Objects.TX;
using PX.TM;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Common;
using PX.Objects.CS.Attributes;

namespace PX.Objects.SO
{
	/// <summary>
	/// Represents sales order and transfer order documents.
	/// </summary>
	/// <remarks>
	/// The records of this type are created and edited on:
	/// <list type="bullet">
	/// <item><description>The <i>Sales Orders (SO301000)</i> form (corresponds to the <see cref="SOOrderEntry"/> graph)</description></item>
	/// <item><description>The <i>Process Orders (SO501000)</i> form (corresponds to the <see cref="SOCreateShipment"/> graph)</description></item>
	/// <item><description>The <i>Print/Email Orders (SO502000)</i> form (corresponds to the <see cref="SOOrderProcess"/> graph)</description></item>
	/// </list>
	/// </remarks>
	[PXPrimaryGraph(typeof(SOOrderEntry))]
	[PXGroupMask(typeof(LeftJoinSingleTable<Customer,
			On<Customer.bAccountID, Equal<SOOrder.customerID>>>),
		WhereRestriction = typeof(Where<Customer.bAccountID, IsNull, Or<Match<Customer, Current<AccessInfo.userName>>>>)
	)]
	[PXCacheName(Messages.SOOrder)]
	[DebuggerDisplay("OrderType = {OrderType}, OrderNbr = {OrderNbr}")]
    public partial class SOOrder : PXBqlTable, PX.Data.IBqlTable, PX.Data.EP.IAssign, IFreightBase, IInvoice, ICreatePaymentDocument, ISubstitutableDocument, INotable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOOrder>.By<orderType, orderNbr>
		{
			public static SOOrder Find(PXGraph graph, string orderType, string orderNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, orderType, orderNbr, options);
		}
		public static class FK
		{
			public class Customer : AR.Customer.PK.ForeignKeyOf<SOOrder>.By<customerID> { }
			public class CustomerLocation : Location.PK.ForeignKeyOf<SOOrder>.By<customerID, customerLocationID> { }
			public class Contact : CR.Contact.PK.ForeignKeyOf<SOOrder>.By<contactID> { }
			public class Branch : GL.Branch.PK.ForeignKeyOf<SOOrder>.By<branchID> { }
			public class OrderType : SOOrderType.PK.ForeignKeyOf<SOOrder>.By<orderType> { }
			public class BillingAddress : SOBillingAddress.PK.ForeignKeyOf<SOOrder>.By<billAddressID> { }
			public class ShippingAddress : SOShippingAddress.PK.ForeignKeyOf<SOOrder>.By<shipAddressID> { }
			public class BillingContact : SOBillingContact.PK.ForeignKeyOf<SOOrder>.By<billContactID> { }
			public class ShippingContact : SOShippingContact.PK.ForeignKeyOf<SOOrder>.By<shipContactID> { }
			public class FreightTaxCategory: TaxCategory.PK.ForeignKeyOf<SOOrder>.By<freightTaxCategoryID> { }
			public class FOBPoint : CS.FOBPoint.PK.ForeignKeyOf<SOOrder>.By<fOBPoint> { }
			public class Invoice : SOInvoice.PK.ForeignKeyOf<SOOrder>.By<aRDocType, invoiceNbr> { }
			public class ShipTerms : CS.ShipTerms.PK.ForeignKeyOf<SOOrder>.By<shipTermsID> { }
			public class ShippingZone : CS.ShippingZone.PK.ForeignKeyOf<SOOrder>.By<shipZoneID> { }
			public class Carrier : CS.Carrier.PK.ForeignKeyOf<SOOrder>.By<shipVia> { }
			public class DefaultSite : INSite.PK.ForeignKeyOf<SOOrder>.By<defaultSiteID> { }
			public class DestinationSite : INSite.PK.ForeignKeyOf<SOOrder>.By<destinationSiteID> { }
			public class OriginalOrderType : SOOrderType.PK.ForeignKeyOf<SOOrder>.By<origOrderType> { }
			public class OriginalOrder : SOOrder.PK.ForeignKeyOf<SOOrder>.By<origOrderType, origOrderNbr> { }
			public class LastSite : INSite.PK.ForeignKeyOf<SOOrder>.By<lastSiteID> { }
			public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<SOOrder>.By<curyInfoID> { }
			public class Currency : CM.Currency.PK.ForeignKeyOf<SOOrder>.By<curyID> { }
			public class TaxZone : TX.TaxZone.PK.ForeignKeyOf<SOOrder>.By<taxZoneID> { }
			public class Project : PMProject.PK.ForeignKeyOf<SOOrder>.By<projectID> { }
			public class SalesPerson : AR.SalesPerson.PK.ForeignKeyOf<SOOrder>.By<salesPersonID> { }
			public class Terms : CS.Terms.PK.ForeignKeyOf<SOOrder>.By<termsID> { }
			public class Workgroup : EPCompanyTree.PK.ForeignKeyOf<SOOrder>.By<workgroupID> { }
			public class Owner : CR.Contact.PK.ForeignKeyOf<SOOrder>.By<ownerID> { }
			public class ToSite : INSite.PK.ForeignKeyOf<SOOrder>.By<destinationSiteID> { }
			public class PaymentMethod : CA.PaymentMethod.PK.ForeignKeyOf<SOOrder>.By<paymentMethodID> { }
			public class CustomerPaymentMethod : AR.CustomerPaymentMethod.PK.ForeignKeyOf<SOOrder>.By<pMInstanceID> { }
			public class CashAccount : Account.PK.ForeignKeyOf<SOOrder>.By<cashAccountID> { }
			public class IntercompanyPOOrder : PO.POOrder.PK.ForeignKeyOf<SOOrder>.By<intercompanyPOType, intercompanyPONbr> { }
			//todo public class IntercompanyPOReturn : PO.POReceipt.PK.ForeignKeyOf<SOOrder>.By<PO.POReceiptType.poreturn, intercompanyPOReturnNbr> { }//uncomment after fix AC-169735
			//todo public class FinancialPeriod : FinPeriod.PK.ForeignKeyOf<SOOrder>.By<finPeriodID> { }
		}
		#endregion
		#region Events
		public class Events : PXEntityEvent<SOOrder>.Container<Events>
		{
			public PXEntityEvent<SOOrder> ShipmentCreationFailed;
			public PXEntityEvent<SOOrder> OrderDeleted;

			public PXEntityEvent<SOOrder> PaymentRequirementsSatisfied;
			public PXEntityEvent<SOOrder> PaymentRequirementsViolated;

			public PXEntityEvent<SOOrder> ObtainedPaymentInPendingProcessing;
			public PXEntityEvent<SOOrder> LostLastPaymentInPendingProcessing;

			public PXEntityEvent<SOOrder> CreditLimitSatisfied;
			public PXEntityEvent<SOOrder> CreditLimitViolated;

			public PXEntityEvent<SOOrder> BlanketCompleted;
			public PXEntityEvent<SOOrder> BlanketReopened;

			public PXEntityEvent<SOOrder, CRQuote> OrderCreatedFromQuote;

			public PXEntityEvent<SOOrder> GotShipmentConfirmed;
			public PXEntityEvent<SOOrder> GotShipmentCorrected;
		}
		#endregion

		#region RiskLineCntr
		/// <inheritdoc cref="RiskLineCntr"/>
		public abstract class riskLineCntr : PX.Data.BQL.BqlInt.Field<riskLineCntr> { }

		/// <summary>
		/// The number of risks of the order.
		/// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? RiskLineCntr { get; set; }

		#endregion

		#region Selected
		/// <inheritdoc cref="Selected"/>
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;

		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true"/>) that the record is selected for
		/// processing by a user.
		/// </summary>
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region OrderType
		/// <inheritdoc cref="OrderType"/>
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;

		/// <summary>
		/// The type of the document, which is a part of the identifier of the order.
		/// The identifier of the <see cref="SOOrderType">order type</see>.
		/// The field is included in the <see cref="FK.OrderType"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="SOOrderType.orderType"/> field.
		/// </value>
		/// <remarks>
		/// The type of the document, which is one of the predefined order types or a custom order type created by
		/// using the Order Types (SO201000) form.
		/// </remarks>
		[PXDBString(2, IsKey = true, IsFixed = true, InputMask=">aa")]
		[PXDefault(SOOrderTypeConstants.SalesOrder, typeof(SOSetup.defaultOrderType))]
        [PXSelector(typeof(Search2<SOOrderType.orderType,
            InnerJoin<SOOrderTypeOperation, On2<SOOrderTypeOperation.FK.OrderType, And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>>>>,
            Where<FeatureInstalled<FeaturesSet.inventory>.Or<SOOrderType.behavior.IsNotEqual<SOBehavior.bL>>>>))]
        [PXRestrictor(typeof(Where<SOOrderTypeOperation.iNDocType, NotEqual<INTranType.transfer>, Or<FeatureInstalled<FeaturesSet.warehouse>>>), ErrorMessages.ElementDoesntExist, typeof(SOOrderType.orderType))]
        [PXRestrictor(typeof(Where<SOOrderType.requireAllocation, NotEqual<True>, Or<AllocationAllowed>>), ErrorMessages.ElementDoesntExist, typeof(SOOrderType.orderType))]
        [PXRestrictor(typeof(Where<SOOrderType.active,Equal<True>>), null)]
		[PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PX.Data.EP.PXFieldDescription]
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region Behavior
		/// <inheritdoc cref="Behavior"/>
		public abstract class behavior : PX.Data.BQL.BqlString.Field<behavior> { }
		protected String _Behavior;

		/// <summary>
		/// The behavior, which is defined by <see cref="OrderType"/> (which is the link to <see cref="SOOrderType"/>).
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in <see cref="SOBehavior"/>.
		/// </value>
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXDefault(typeof(Search<SOOrderType.behavior, Where<SOOrderType.orderType, Equal<Current<SOOrder.orderType>>>>))]
		[PXUIField(DisplayName = "Behavior", Enabled = false, IsReadOnly = true)]
		[SOBehavior.List()]
		public virtual String Behavior
		{
			get
			{
				return this._Behavior;
			}
			set
			{
				this._Behavior = value;
			}
		}
		#endregion
		#region ARDocType
		/// <inheritdoc cref="ARDocType"/>
		public abstract class aRDocType : PX.Data.BQL.BqlString.Field<aRDocType> { }
		protected String _ARDocType;

		/// <summary>
		/// The type of an accounts receivable document to be generated on release of a document of this type.
		/// The field is included in the <see cref="FK.Invoice"/> foreign key.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in <see cref="AR.ARDocType"/>.
		/// </value>
		[PXString(ARRegister.docType.Length, IsFixed = true)]
		[PXFormula(typeof(Selector<SOOrder.orderType, SOOrderType.aRDocType>))]
		public virtual String ARDocType
		{
			get
			{
				return this._ARDocType;
			}
			set
			{
				this._ARDocType = value;
			}
		}
		#endregion
		#region IsInvoiceOrder
		/// <inheritdoc cref="IsInvoiceOrder"/>
		public abstract class isInvoiceOrder : Data.BQL.BqlBool.Field<isInvoiceOrder> { }

		/// <summary>
		/// A Boolean value that indicates whether the order is used only for an invoice.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Where<aRDocType, NotEqual<ARDocType.noUpdate>,
					And<Selector<orderType, SOOrderType.requireShipping>, Equal<False>>>))]
		public virtual bool? IsInvoiceOrder
		{
			get;
			set;
		}
		#endregion
		#region OrderNbr
		/// <inheritdoc cref="OrderNbr"/>
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;

		/// <summary>
		/// The unique reference number of the order.
		/// </summary>
		/// <remarks>
		/// When the new sales order is saved for the first time, the system automatically generates
		/// this number by using the numbering sequence assigned to orders of <see cref="SOOrderType"/>.
		/// </remarks>
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[SO.RefNbr(typeof(Search2<SOOrder.orderNbr,
			LeftJoinSingleTable<Customer, On<SOOrder.customerID, Equal<Customer.bAccountID>,
				And<Where<Match<Customer, Current<AccessInfo.userName>>>>>>,
			Where<SOOrder.orderType, Equal<Optional<SOOrder.orderType>>,
				And<Where<Customer.bAccountID, IsNotNull,
					Or<Exists<Select<SOOrderType,
						Where<SOOrderType.orderType, Equal<SOOrder.orderType>,
							And<SOOrderType.aRDocType, Equal<ARDocType.noUpdate>,
							And<SOOrderType.behavior, In3<SOBehavior.sO, SOBehavior.tR>>>>>>>>>>,
			 OrderBy<Desc<SOOrder.orderNbr>>>), Filterable = true)]
		[SO.Numbering()]
		[PX.Data.EP.PXFieldDescription]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
		#region CustomerID
		/// <inheritdoc cref="CustomerID"/>
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;

		/// <summary>
		/// The identifier of the <see cref="AR.Customer">customer</see>. The field is a part of the identifier of the
		/// <see cref="Location">customer location</see>.
		/// The field is included in the foreign keys <see cref="FK.Customer"/> and <see cref="FK.CustomerLocation"/>.
		/// </summary>
		/// <value>
		/// For a customer, the value of this field corresponds to the value of the <see cref="AR.Customer.bAccountID"/>
		/// field.
		/// For a customer location, the value of this field corresponds to the value of the
		/// <see cref="Location.bAccountID"/> field.
		/// </value>
		[PXDefault]
        [Customer(
			typeof(Search<BAccountR.bAccountID, Where<True, Equal<True>>>), // TODO: remove fake Where after AC-101187
			Visibility = PXUIVisibility.SelectorVisible,
			DescriptionField = typeof(Customer.acctName),
			Filterable = true)]
		[CustomerOrOrganizationInNoUpdateDocRestrictor]
		[PXRestrictor(typeof(Where<Optional<SOOrder.isTransferOrder>, Equal<True>,
				Or<Customer.status, IsNull,
				Or<Customer.status, Equal<CustomerStatus.active>,
				Or<Customer.status, Equal<CustomerStatus.oneTime>,
				Or<Customer.status, Equal<CustomerStatus.creditHold>>>>>>),
			AR.Messages.CustomerIsInStatus,
			typeof(Customer.status))]
		[PXForeignReference(typeof(Field<SOOrder.customerID>.IsRelatedTo<BAccount.bAccountID>))]

		public virtual Int32? CustomerID
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
		#region CustomerLocationID
		/// <inheritdoc cref="CustomerLocationID"/>
		public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }
		protected Int32? _CustomerLocationID;

		/// <summary>
		/// The identifier of the <see cref="Location">customer location</see>.
		/// The field is included in the <see cref="FK.CustomerLocation"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Location.locationID"/> field.
		/// </value>
		[LocationActive(typeof(Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>,
			And<MatchWithBranch<Location.cBranchID>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Coalesce<Search2<BAccountR.defLocationID,
			InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>>,
			Where<BAccountR.bAccountID, Equal<Current<SOOrder.customerID>>,
				And<CRLocation.isActive, Equal<True>,
				And<MatchWithBranch<CRLocation.cBranchID>>>>>,
			Search<CRLocation.locationID,
			Where<CRLocation.bAccountID, Equal<Current<SOOrder.customerID>>,
			And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.cBranchID>>>>>>))]
		[PXForeignReference(
			typeof(CompositeKey<
				Field<SOOrder.customerID>.IsRelatedTo<Location.bAccountID>,
				Field<SOOrder.customerLocationID>.IsRelatedTo<Location.locationID>
			>))]
		public virtual Int32? CustomerLocationID
		{
			get
			{
				return this._CustomerLocationID;
			}
			set
			{
				this._CustomerLocationID = value;
			}
		}
		#endregion
		#region ContactID
		/// <inheritdoc cref="ContactID"/>
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		/// <summary>
		/// The identifier of the <see cref="CR.Contact">contact</see>.
		/// The field is included in the <see cref="FK.Contact"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Contact.contactID"/> field.
		/// </value>
		[ContactRaw(typeof(SOOrder.customerID), WithContactDefaultingByBAccount = true)]
		[PXUIEnabled(typeof(Where<SOOrder.customerID, IsNotNull>))]
		public virtual Int32? ContactID { get; set; }
		#endregion
		#region BranchID
		/// <inheritdoc cref="BranchID"/>
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;

		/// <summary>
		/// The identifier of the <see cref="GL.Branch">branch</see>.
		/// The field is included in the <see cref="FK.Branch"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="GL.Branch.branchID"/> field.
		/// </value>
		[Branch(typeof(AccessInfo.branchID), IsDetail = false, TabOrder = 0)]
		[PXFormula(typeof(Switch<Case<Where<SOOrder.customerLocationID, IsNotNull,
										And<Selector<SOOrder.customerLocationID, Location.cBranchID>, IsNotNull>>,
									Selector<SOOrder.customerLocationID, Location.cBranchID>,
								Case<Where<Current2<SOOrder.branchID>, IsNotNull>,
									Current2<SOOrder.branchID>>>,
								Current<AccessInfo.branchID>>))]
		public virtual Int32? BranchID
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
		#region OrderDate
		/// <inheritdoc cref="OrderDate"/>
		public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }
		protected DateTime? _OrderDate;

		/// <summary>
		/// The date of the document.
		/// </summary>
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? OrderDate
		{
			get
			{
				return this._OrderDate;
			}
			set
			{
				this._OrderDate = value;
			}
		}
		#endregion
		#region CustomerOrderNbr
		/// <inheritdoc cref="CustomerOrderNbr"/>
		public abstract class customerOrderNbr : PX.Data.BQL.BqlString.Field<customerOrderNbr> { }
		protected String _CustomerOrderNbr;

		/// <summary>
		/// The reference number of the original customer document that the sales order is based on.
		/// </summary>
		/// <remarks>
		/// A reference number must be specified if the
		/// <see cref="SOOrderType.customerOrderIsRequired">Require Customer Order Nbr</see> field is
		/// <see langword="true"/> for the order type.
		/// This field is available for orders of the <see cref="SOBehavior.tR">TR type</see>.
		/// </remarks>
		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "Customer Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String CustomerOrderNbr
		{
			get
			{
				return this._CustomerOrderNbr;
			}
			set
			{
				this._CustomerOrderNbr = value;
			}
		}
		#endregion
		#region CustomerRefNbr
		/// <inheritdoc cref="CustomerRefNbr"/>
		public abstract class customerRefNbr : PX.Data.BQL.BqlString.Field<customerRefNbr> { }
		protected String _CustomerRefNbr;

		/// <summary>
		/// The reference number of the sales order in a third-party application if Acumatica ERP is integrated with
		/// such an application and imports the sales orders from it.
		/// </summary>
		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "External Reference")]
		public virtual String CustomerRefNbr
		{
			get
			{
				return this._CustomerRefNbr;
			}
			set
			{
				this._CustomerRefNbr = value;
			}
		}
		#endregion
		#region CancelDate
		/// <inheritdoc cref="CancelDate"/>
		public abstract class cancelDate : PX.Data.BQL.BqlDateTime.Field<cancelDate> { }
		protected DateTime? _CancelDate;

		/// <summary>
		/// The expiration date of the order, by which the order can be selected for canceling on
		/// the Process Orders (SO501000) form.
		/// </summary>
		[PXDBDate()]
		[PXFormula(typeof(Switch<Case<Where<MaxDate, Less<Add<SOOrder.orderDate, Selector<SOOrder.orderType, SOOrderType.daysToKeep>>>>, MaxDate>, Add<SOOrder.orderDate, Selector<SOOrder.orderType, SOOrderType.daysToKeep>>>))]
		[PXUIField(DisplayName = "Cancel By", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? CancelDate
		{
			get
			{
				return this._CancelDate;
			}
			set
			{
				this._CancelDate = value;
			}
		}
		#endregion
		#region RequestDate
		/// <inheritdoc cref="RequestDate"/>
		public abstract class requestDate : PX.Data.BQL.BqlDateTime.Field<requestDate> { }
		protected DateTime? _RequestDate;

		/// <summary>
		/// The date when the customer wants to receive the goods.
		/// </summary>
		/// <remarks>
		/// This date provides the default values for the <see cref="SOLine.requestDate"/> dates for order lines.
		/// </remarks>
		/// <value>
		/// The default value is the <see cref="AccessInfo.businessDate">current business date</see>.
		/// </value>
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Requested On", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? RequestDate
		{
			get
			{
				return this._RequestDate;
			}
			set
			{
				this._RequestDate = value;
			}
		}
		#endregion
		#region ShipDate
		/// <inheritdoc cref="ShipDate"/>
		public abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }
		protected DateTime? _ShipDate;

		/// <summary>
		/// The date when the ordered goods are scheduled to be shipped.
		/// </summary>
		/// <remarks>
		/// By default, it is the date that is specified in <see cref="requestDate"/> minus the number of lead days,
		/// but it is not earlier than the <see cref="AccessInfo.businessDate">current business date</see>.
		/// </remarks>
		[PXDBDate()]
		[PXFormula(typeof(DateMinusDaysNotLessThenDate<SOOrder.requestDate, IsNull<Selector<Current<SOOrder.customerLocationID>, Location.cLeadTime>, decimal0>, SOOrder.orderDate>))]
		[PXUIField(DisplayName = "Sched. Shipment", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? ShipDate
		{
			get
			{
				return this._ShipDate;
			}
			set
			{
				this._ShipDate = value;
			}
		}
		#endregion
        #region DontApprove
		/// <inheritdoc cref="DontApprove"/>
        public abstract class dontApprove : PX.Data.BQL.BqlBool.Field<dontApprove> { }
        protected Boolean? _DontApprove;

		/// <summary>
		/// A Boolean value that specifies (if set to <see langword="true"/>) that the order does not need to
		/// be approved.
		/// </summary>
        [PXBool()]
		[PXUnboundDefault(true)]
		[PXUIField(DisplayName = "Don't Approve", Visible = false, Enabled = false)]
        public virtual Boolean? DontApprove
        {
            get
            {
                return this._DontApprove;
            }
            set
            {
                this._DontApprove = value;
            }
        }
        #endregion

        #region Hold
		/// <inheritdoc cref="Hold"/>
        public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
        protected Boolean? _Hold;

		/// <summary>
		/// A Boolean value that indicates whether the order is on hold.
		/// </summary>
		/// <remarks>
		/// If the order is on hold, additions and changes can be made and order quantities do not affect the item
		/// availability.
		/// </remarks>
        [PXDBBool()]
        [PXDefault(false, typeof(Search<SOOrderType.holdEntry, Where<SOOrderType.orderType, Equal<Current<SOOrder.orderType>>>>))]
        [PXUIField(DisplayName = "Hold", Enabled = false)]
        public virtual Boolean? Hold
        {
            get
            {
                return this._Hold;
            }
            set
            {
                this._Hold = value;
            }
        }
        #endregion
        #region Approved
		/// <inheritdoc cref="Approved"/>
        public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }
        protected Boolean? _Approved;

		/// <summary>
		/// A Boolean value that indicates whether the sales order has been approved.
		/// </summary>
		/// <remarks>
		/// This field is available only if the <see cref="FeaturesSet.approvalWorkflow">Approval Workflow</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.
		/// </remarks>
        [PXDBBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? Approved
        {
            get
            {
                return this._Approved;
            }
            set
            {
                this._Approved = value;
            }
        }
        #endregion
        #region Rejected
		/// <inheritdoc cref="Rejected"/>
        public abstract class rejected : PX.Data.BQL.BqlBool.Field<rejected> { }
        protected bool? _Rejected = false;

		/// <summary>
		/// A Boolean value that indicates whether the order was rejected by one of the persons assigned to approve it.
		/// </summary>
		/// <remarks>
		/// This status is available for orders of the SO, SA, CM, CS, TR, QT, and IN order types only
		/// if the <see cref="FeaturesSet.approvalWorkflow">Approval Workflow</see>feature is enabled on
		/// the Enable/Disable Features (CS100000) form.
		/// </remarks>
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public bool? Rejected
        {
            get
            {
                return _Rejected;
            }
            set
            {
                _Rejected = value;
            }
        }
		#endregion

		#region Emailed
		/// <inheritdoc cref="Emailed"/>
		public abstract class emailed : PX.Data.BQL.BqlBool.Field<emailed> { }

		/// <summary>
		/// A Boolean value that indicates whether the document has been emailed to the
		/// <see cref="CustomerID">customer</see>.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Emailed")]
		public virtual Boolean? Emailed
		{
			get; set;
		}
		#endregion

		#region Printed
		public abstract class printed : PX.Data.BQL.BqlBool.Field<printed> { }

		/// <summary>
		/// A Boolean value that indicates whether the document has been printed.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Printed")]
		public virtual Boolean? Printed
		{
			get; set;
		}
		#endregion

		#region CreditHold
		/// <inheritdoc cref="CreditHold"/>
		public abstract class creditHold : PX.Data.BQL.BqlBool.Field<creditHold> { }
		protected Boolean? _CreditHold;

		/// <summary>
		/// A Boolean value that specifies (if set to <see langword="true"/>) that the customer has failed the credit
		/// check, which the system performed when the order was taken off hold.
		/// </summary>
		/// <remarks>
		/// An order with this status can be saved with only the Credit Hold or On Hold status if the
		/// <see cref="SOOrderType.creditHoldEntry">Hold Document on Failed Credit Check</see> field is
		/// <see langword="true"/> for the order type.
		/// This status is available for orders of the QT, SO, SA, IN, and CM order types.
		/// </remarks>
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
		#region Completed
		/// <inheritdoc cref="Completed"/>
		public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }
		protected Boolean? _Completed;

		/// <summary>
		/// A Boolean value that specifies (if set to <see langword="true"/>) that all related inventory documents
		/// required for the order type have been generated and released.
		/// </summary>
		/// <remarks>
		/// Completed orders of the QT order type can be opened again.
		/// </remarks>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Completed")]
		public virtual Boolean? Completed
		{
			get
			{
				return this._Completed;
			}
			set
			{
				this._Completed = value;
			}
		}
		#endregion
		#region Cancelled
		/// <inheritdoc cref="Cancelled"/>
		public abstract class cancelled : PX.Data.BQL.BqlBool.Field<cancelled> { }
		protected Boolean? _Cancelled;

		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true"/>) that the order has been canceled on the
		/// date specified in <see cref="CancelDate"/>.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Canceled")]
		public virtual Boolean? Cancelled
		{
			get
			{
				return this._Cancelled;
			}
			set
			{
				this._Cancelled = value;
			}
		}
		#endregion
		#region OpenDoc
		/// <inheritdoc cref="OpenDoc"/>
		public abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }
		protected Boolean? _OpenDoc;

		/// <summary>
		/// A Boolean value that indicates whether the order has unbilled amount.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? OpenDoc
		{
			get
			{
				return this._OpenDoc;
			}
			set
			{
				this._OpenDoc = value;
			}
		}
		#endregion
		#region ShipmentDeleted
		/// <inheritdoc cref="ShipmentDeleted"/>
		[Obsolete(Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R2)]
		public abstract class shipmentDeleted : PX.Data.BQL.BqlBool.Field<shipmentDeleted> { }
		protected Boolean? _ShipmentDeleted;

		/// <exclude/>
		[PXBool()]
		[Obsolete(Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R2)]
		public virtual Boolean? ShipmentDeleted
		{
			get
			{
				return this._ShipmentDeleted;
			}
			set
			{
				this._ShipmentDeleted = value;
			}
		}
		#endregion
		#region BackOrdered
		/// <inheritdoc cref="BackOrdered"/>
		public abstract class backOrdered : PX.Data.BQL.BqlBool.Field<backOrdered> { }
		protected Boolean? _BackOrdered;

		/// <summary>
		/// A Boolean value that indicates whether the order can not be shipped because the specified items are not
		/// available.
		/// </summary>
		/// <remarks>
		/// <para>This status can be assigned to an open order manually in the following case: When a user attempts
		/// to create a shipment, the system detects that the order cannot be shipped in full and displays a message
		/// about this.</para>
		/// <para>This status can be assigned to orders automatically when shipment for order is created if the
		/// shipping rules specified on the document level and on the line level do not allow shipment creation.</para>
		/// <para>This status is available for sales orders of the SO and SA types.</para>
		/// </remarks>
		[PXBool()]
		[PXUIField(DisplayName = "BackOrdered")]
		public virtual Boolean? BackOrdered
		{
			get
			{
				return this._BackOrdered;
			}
			set
			{
				this._BackOrdered = value;
			}
		}
		#endregion
		#region LastSiteID
		/// <inheritdoc cref="LastSiteID"/>
		public abstract class lastSiteID : PX.Data.BQL.BqlInt.Field<lastSiteID> { }
		protected Int32? _LastSiteID;

		/// <summary>
		/// The identifier of the <see cref="INSite">warehouse</see> of the last confirmed shipment.
		/// The field is included in the <see cref="FK.LastSite"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="INSite.siteID"/> field.
		/// </value>
		[PXDBInt()]
		[PXUIField(DisplayName = "Last Shipment Site")]
		public virtual Int32? LastSiteID
		{
			get
			{
				return this._LastSiteID;
			}
			set
			{
				this._LastSiteID = value;
			}
		}
		#endregion
		#region LastShipDate
		/// <inheritdoc cref="LastShipDate"/>
		public abstract class lastShipDate : PX.Data.BQL.BqlDateTime.Field<lastShipDate> { }
		protected DateTime? _LastShipDate;

		/// <summary>
		/// The date of the last confirmed shipment.
		/// </summary>
		[PXDBDate()]
		[PXUIField(DisplayName = "Last Shipment Date")]
		public virtual DateTime? LastShipDate
		{
			get
			{
				return this._LastShipDate;
			}
			set
			{
				this._LastShipDate = value;
			}
		}
		#endregion
		#region BillSeparately
		/// <inheritdoc cref="BillSeparately"/>
		public abstract class billSeparately : PX.Data.BQL.BqlBool.Field<billSeparately> { }
		protected Boolean? _BillSeparately;

		/// <summary>
		/// A Boolean value that indicates whether the document should be billed separately (that is, it requires a
		/// separate invoice).
		/// </summary>
		/// <remarks>
		/// This field is not available for transfer orders.
		/// </remarks>
		[PXDBBool()]
		[PXDefault(typeof(Search<SOOrderType.billSeparately, Where<SOOrderType.orderType, Equal<Current<SOOrder.orderType>>>>))]
		[PXUIField(DisplayName = "Bill Separately")]
		public virtual Boolean? BillSeparately
		{
			get
			{
				return this._BillSeparately;
			}
			set
			{
				this._BillSeparately = value;
			}
		}
		#endregion
		#region ShipSeparately
		/// <inheritdoc cref="ShipSeparately"/>
		public abstract class shipSeparately : PX.Data.BQL.BqlBool.Field<shipSeparately> { }
		protected Boolean? _ShipSeparately;

		/// <summary>
		/// A Boolean value that indicates whether the goods for the customer should be shipped separately for each
		/// sales order.
		/// </summary>
		/// <remarks>
		/// This field is available only if the <see cref="FeaturesSet.inventory">Inventory</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.
		/// </remarks>
		[PXDBBool()]
		[PXDefault(typeof(Search<SOOrderType.shipSeparately,Where<SOOrderType.orderType, Equal<Current<SOOrder.orderType>>>>))]
		[PXUIField(DisplayName = "Ship Separately")]
		public virtual Boolean? ShipSeparately
		{
			get
			{
				return this._ShipSeparately;
			}
			set
			{
				this._ShipSeparately = value;
			}
		}
		#endregion
		#region Status
		/// <inheritdoc cref="Status"/>
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected string _Status;

		/// <summary>
		/// The status of the order.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in <see cref="SOOrderStatus"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[SOOrderStatus.List]
		[PXDefault]
		public virtual String Status
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
		#region NoteID
		/// <inheritdoc cref="NoteID"/>
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;

		/// <summary>
		/// The identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the document.
		/// </summary>
		/// <value>
		/// The value corresponds to the <see cref="PX.Data.Note.noteID">Note.NoteID</see> field.
		/// </value>
		[PXSearchable(SM.SearchCategory.SO, "{0}: {1} - {3}", new Type[] { typeof(SOOrder.orderType), typeof(SOOrder.orderNbr), typeof(SOOrder.customerID), typeof(Customer.acctName) },
		   new Type[] { typeof(SOOrder.customerRefNbr), typeof(SOOrder.customerOrderNbr), typeof(SOOrder.orderDesc) },
		   NumberFields = new Type[] { typeof(SOOrder.orderNbr) },
		   Line1Format = "{0:d}{1}{2}{3}", Line1Fields = new Type[] { typeof(SOOrder.orderDate), typeof(SOOrder.status), typeof(SOOrder.customerRefNbr), typeof(SOOrder.customerOrderNbr) },
		   Line2Format = "{0}", Line2Fields = new Type[] { typeof(SOOrder.orderDesc) },
		   MatchWithJoin = typeof(InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<SOOrder.customerID>>>),
		   SelectForFastIndexing = typeof(Select2<SOOrder, LeftJoin<Customer, On<SOOrder.customerID, Equal<Customer.bAccountID>>>>)
	   )]
        [PXNote(ShowInReferenceSelector = true, Selector = typeof(
			Search2<
				SOOrder.orderNbr,
			LeftJoinSingleTable<Customer,
				On<SOOrder.customerID, Equal<Customer.bAccountID>,
				And<Where<Match<Customer, Current<AccessInfo.userName>>>>>>,
			Where<
				Customer.bAccountID, IsNotNull,
				Or<Exists<
					Select<
						SOOrderType,
					Where<
						SOOrderType.orderType, Equal<SOOrder.orderType>,
						And<SOOrderType.aRDocType, Equal<ARDocType.noUpdate>>>>>>>,
			OrderBy<
				Desc<SOOrder.orderNbr>>>))]
		public virtual Guid? NoteID
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
		#region LineCntr
		/// <inheritdoc cref="LineCntr"/>
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
		protected Int32? _LineCntr;

		/// <summary>
		/// The counter of the child lines added to the order.
		/// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? LineCntr
		{
			get
			{
				return this._LineCntr;
			}
			set
			{
				this._LineCntr = value;
			}
		}
		#endregion
		#region BilledCntr
		/// <inheritdoc cref="BilledCntr"/>
		public abstract class billedCntr : PX.Data.BQL.BqlInt.Field<billedCntr> { }
		protected Int32? _BilledCntr;

		/// <summary>
		/// The counter of the shipments added to the order that have an invoice.
		/// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? BilledCntr
		{
			get
			{
				return this._BilledCntr;
			}
			set
			{
				this._BilledCntr = value;
			}
		}
		#endregion
		#region ReleasedCntr
		/// <inheritdoc cref="ReleasedCntr"/>
		public abstract class releasedCntr : PX.Data.BQL.BqlInt.Field<releasedCntr> { }
		protected Int32? _ReleasedCntr;

		/// <summary>
		/// The counter of the shipments added to the order that have a released invoice.
		/// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? ReleasedCntr
		{
			get
			{
				return this._ReleasedCntr;
			}
			set
			{
				this._ReleasedCntr = value;
			}
		}
		#endregion
		#region PaymentCntr
		/// <inheritdoc cref="PaymentCntr"/>
		public abstract class paymentCntr : PX.Data.BQL.BqlInt.Field<paymentCntr> { }
		protected Int32? _PaymentCntr;

		/// <summary>
		/// The counter of the shipments added to the order that have a payment.
		/// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? PaymentCntr
		{
			get
			{
				return this._PaymentCntr;
			}
			set
			{
				this._PaymentCntr = value;
			}
		}
		#endregion
		#region AuthorizedPaymentCntr
		public abstract class authorizedPaymentCntr : PX.Data.BQL.BqlInt.Field<authorizedPaymentCntr> { }

		/// <summary>
		/// The counter of the applied payments in pre-authorized state.
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? AuthorizedPaymentCntr
		{
			get;
			set;
		}
		#endregion
		#region OrderDesc
		/// <inheritdoc cref="OrderDesc"/>
		public abstract class orderDesc : PX.Data.BQL.BqlString.Field<orderDesc> { }
		protected String _OrderDesc;

		/// <summary>
		/// A brief description of the document.
		/// </summary>
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String OrderDesc
		{
			get
			{
				return this._OrderDesc;
			}
			set
			{
				this._OrderDesc = value;
			}
		}
		#endregion
		#region BillAddressID
		/// <inheritdoc cref="BillAddressID"/>
		public abstract class billAddressID : PX.Data.BQL.BqlInt.Field<billAddressID> { }
		protected Int32? _BillAddressID;

		/// <summary>
		/// The identifier of the <see cref="SOBillingAddress">billing address</see>.
		/// The field is included in the <see cref="FK.BillingAddress"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="SOAddress.addressID"/> field.
		/// </value>
		[PXDBInt()]
		[SOBillingAddress(typeof(Select2<BAccountR,
			InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>,
			LeftJoin<Customer, On<Customer.bAccountID, Equal<BAccountR.bAccountID>>,
			InnerJoin<Address, On<Address.bAccountID, Equal<BAccountR.bAccountID>,
			                  And<Where2<Where<Customer.bAccountID, IsNotNull,
												             And<Address.addressID, Equal<Customer.defBillAddressID>>>,
																Or<Where<Customer.bAccountID, IsNull,
																		 And<Address.addressID, Equal<BAccountR.defAddressID>>>>>>>,
			LeftJoin<SOBillingAddress, On<SOBillingAddress.customerID, Equal<Address.bAccountID>, And<SOBillingAddress.customerAddressID, Equal<Address.addressID>, And<SOBillingAddress.revisionID, Equal<Address.revisionID>, And<SOBillingAddress.isDefaultAddress, Equal<boolTrue>>>>>>>>>,
			Where<BAccountR.bAccountID, Equal<Current<SOOrder.customerID>>>>))]
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
		#region ShipAddressID
		/// <inheritdoc cref="ShipAddressID"/>
		public abstract class shipAddressID : PX.Data.BQL.BqlInt.Field<shipAddressID> { }
		protected Int32? _ShipAddressID;

		/// <summary>
		/// The identifier of the <see cref="SOShippingAddress">shipping address</see>.
		/// The field is included in the <see cref="FK.ShippingAddress"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="SOShippingAddress.addressID"/> field.
		/// </value>
		[PXDBInt()]
        [SOShippingAddress(typeof(
                    Select2<Address,
                        InnerJoin<CRLocation,
				  On<CRLocation.bAccountID, Equal<Address.bAccountID>,
				 And<Address.addressID, Equal<CRLocation.defAddressID>,
					 And<CRLocation.bAccountID, Equal<Current<SOOrder.customerID>>,
                            And<CRLocation.locationID, Equal<Current<SOOrder.customerLocationID>>>>>>,
                        LeftJoin<SOShippingAddress,
                            On<SOShippingAddress.customerID, Equal<Address.bAccountID>,
                            And<SOShippingAddress.customerAddressID, Equal<Address.addressID>,
                            And<SOShippingAddress.revisionID, Equal<Address.revisionID>,
                            And<SOShippingAddress.isDefaultAddress, Equal<True>>>>>>>,
                        Where<True, Equal<True>>>))]
		public virtual Int32? ShipAddressID
		{
			get
			{
				return this._ShipAddressID;
			}
			set
			{
				this._ShipAddressID = value;
			}
		}
		#endregion
		#region BillContactID
		/// <inheritdoc cref="BillContactID"/>
		public abstract class billContactID : PX.Data.BQL.BqlInt.Field<billContactID> { }
		protected Int32? _BillContactID;

		/// <summary>
		/// The identifier of the <see cref="SOBillingContact">billing contact</see>.
		/// The field is included in the <see cref="FK.BillingContact"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="SOBillingContact.contactID"/> field.
		/// </value>
		[PXDBInt()]
		[SOBillingContact(typeof(Select2<BAccountR,
			InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>,
			LeftJoin<Customer, On<Customer.bAccountID, Equal<BAccountR.bAccountID>>,
			InnerJoin<Contact, On<Contact.bAccountID, Equal<BAccountR.bAccountID>,
			                  And<Where2<Where<Customer.bAccountID, IsNotNull,
												             And<Contact.contactID, Equal<Customer.defBillContactID>>>,
																Or<Where<Customer.bAccountID, IsNull,
																		 And<Contact.contactID, Equal<BAccountR.defContactID>>>>>>>,
			LeftJoin<SOBillingContact, On<SOBillingContact.customerID, Equal<Contact.bAccountID>, And<SOBillingContact.customerContactID, Equal<Contact.contactID>, And<SOBillingContact.revisionID, Equal<Contact.revisionID>, And<SOBillingContact.isDefaultContact, Equal<boolTrue>>>>>>>>>,
			Where<BAccountR.bAccountID, Equal<Current<SOOrder.customerID>>>>))]
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
		#region ShipContactID
		/// <inheritdoc cref="ShipContactID"/>
		public abstract class shipContactID : PX.Data.BQL.BqlInt.Field<shipContactID> { }
		protected Int32? _ShipContactID;

		/// <summary>
		/// The identifier of the <see cref="SOShippingContact">shipping contact</see>.
		/// The field is included in the <see cref="FK.ShippingContact"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="SOShippingContact.contactID"/> field.
		/// </value>
		[PXDBInt()]
		[SOShippingContact(typeof(Select2<Contact,
                    InnerJoin<CRLocation,
				  On<CRLocation.bAccountID, Equal<Contact.bAccountID>,
				 And<Contact.contactID, Equal<CRLocation.defContactID>,
					 And<CRLocation.bAccountID, Equal<Current<SOOrder.customerID>>,
                        And<CRLocation.locationID, Equal<Current<SOOrder.customerLocationID>>>>>>,
                    LeftJoin<SOShippingContact,
                        On<SOShippingContact.customerID, Equal<Contact.bAccountID>,
			     And<SOShippingContact.customerContactID, Equal<Contact.contactID>,
					 And<SOShippingContact.revisionID, Equal<Contact.revisionID>,
                        And<SOShippingContact.isDefaultContact, Equal<True>>>>>>>
                    , Where<True, Equal<True>>>))]
		public virtual Int32? ShipContactID
		{
			get
			{
				return this._ShipContactID;
			}
			set
			{
				this._ShipContactID = value;
			}
		}
		#endregion
		#region CuryID
		/// <inheritdoc cref="CuryID"/>
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;

		/// <summary>
		/// The identifier of the <see cref="CM.Currency">currency</see> of the document.
		/// The field is included in the <see cref="FK.Currency"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="CM.Currency.curyID"/> field.
		/// </value>
		/// <remarks>
		/// This field is available only if the
		/// <see cref="FeaturesSet.multicurrency">Multicurrency Accounting</see> feature is enabled on
		/// the Enable/Disable Features (CS100000) form.
		/// </remarks>
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Current<AccessInfo.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region CuryInfoID
		/// <inheritdoc cref="CuryInfoID"/>
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;

		/// <summary>
		/// The identifier of the <see cref="CM.CurrencyInfo">currency and exchange rate information</see>.
		/// The field is included in the <see cref="FK.CurrencyInfo"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="CM.CurrencyInfo.curyInfoID"/> field.
		/// </value>
		[PXDBLong()]
		[CurrencyInfo()]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
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
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.lineDiscTotal))]
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
		/// The total line discount of the document, which is calculated as the sum of line discounts of the order.
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
		/// which is calculated as the sum of group discounts of the order.
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.groupDiscTotal))]
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
		/// The total group discount of the document, which is calculated as the sum of group discounts of the order.
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
		/// which is calculated as the sum of document discounts of the order.
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.documentDiscTotal))]
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
		/// The total document discount of the document, which is calculated as the sum of document discounts of the order.
		/// </summary>
		/// <remarks>
		/// <para>If the <see cref="FeaturesSet.customerDiscounts">Customer Discounts</see> feature is not enabled on
		/// the Enable/Disable Features (CS100000) form,
		/// a user can enter a document-level discount manually. This manual discount has no discount code or
		/// sequence and is not recalculated by the system. If the manual discount needs to be changed, a user has to
		/// correct it manually.</para>
		/// <para>This field is not available for blanket sales orders.</para>
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
		#region DiscTot
		/// <inheritdoc cref="DiscTot"/>
		public abstract class discTot : PX.Data.BQL.BqlDecimal.Field<discTot> { }

		/// <summary>
		/// The total group and document discount of the document, which is calculated as the sum of group and document discounts of the order.
		/// </summary>
		[PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Group and Document Discount Total")]
		public virtual Decimal? DiscTot
        {
			get;
			set;
		}
        #endregion
        #region CuryDiscTot
		/// <inheritdoc cref="CuryDiscTot"/>
        public abstract class curyDiscTot : PX.Data.BQL.BqlDecimal.Field<curyDiscTot> { }

		/// <summary>
		/// The total <see cref="discTot">discount of the document</see> (in the currency of the document),
		/// which is calculated as the sum of all discounts of the order.
		/// </summary>
        [PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.discTot))]
        [PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Document Discounts")]
        public virtual Decimal? CuryDiscTot
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
		/// which is calculated as the sum of all group, document and line discounts of the order.
		/// </summary>
		[PXCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.orderDiscTotal))]
        [PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCalced(typeof(Add<SOOrder.curyDiscTot, SOOrder.curyLineDiscTotal>), typeof(Decimal))]
		[PXFormula(typeof(Add<SOOrder.curyDiscTot, SOOrder.curyLineDiscTotal>))]
		[PXUIField(DisplayName = "Discount Total", Enabled = false)]
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
		/// The total discount of the document, which is calculated as the sum of group, document and line discounts of the order.
		/// </summary>
		/// <remarks>
		/// <para>This field is not available for blanket sales orders.</para>
		/// </remarks>
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCalced(typeof(Add<SOOrder.discTot, SOOrder.lineDiscTotal>), typeof(Decimal))]
		[PXUIField(DisplayName = "Discount Total")]
		public virtual Decimal? OrderDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryOrderTotal
		/// <inheritdoc cref="CuryOrderTotal"/>
		public abstract class curyOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyOrderTotal> { }
		protected Decimal? _CuryOrderTotal;

		/// <summary>
		/// The <see cref="orderTotal">total amount of the document</see> (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.orderTotal), SignCondition = typeof(defaultOperation.IsEqual<SOOperation.issue>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Order Total")]
		public virtual Decimal? CuryOrderTotal
		{
			get
			{
				return this._CuryOrderTotal;
			}
			set
			{
				this._CuryOrderTotal = value;
			}
		}
		#endregion
		#region OrderTotal
		/// <inheritdoc cref="OrderTotal"/>
		public abstract class orderTotal : PX.Data.BQL.BqlDecimal.Field<orderTotal> { }
		protected Decimal? _OrderTotal;

		/// <summary>
		/// The total amount of the document.
		/// </summary>
		[PXDBDecimal(4, SignCondition = typeof(defaultOperation.IsEqual<SOOperation.issue>))]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? OrderTotal
		{
			get
			{
				return this._OrderTotal;
			}
			set
			{
				this._OrderTotal = value;
			}
		}
		#endregion
		#region CuryLineTotal
		/// <inheritdoc cref="CuryLineTotal"/>
		public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }
		protected Decimal? _CuryLineTotal;

		/// <summary>
		/// The <see cref="lineTotal">total amount</see> on all lines of the document, except for Misc. Charges, after Line-level discounts
		/// are applied (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.lineTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Line Total")]
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
		/// <inheritdoc cref="CuryLineTotal"/>
		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }
		protected Decimal? _LineTotal;

		/// <summary>
		/// The total amount on all lines of the document, except for Misc. Charges, after Line-level discounts are applied.
		/// </summary>
		/// <remarks>
		/// <para>This total is calculated as the sum of the amounts in the
		/// <see cref="SOLine.curyLineAmt">Amount</see> for all stock items and non-stock items that require shipment.
		/// This total does not include the freight amount.</para>
		/// <para>This field is not available for transfer orders.
		/// This field is available only if the <see cref="FeaturesSet.inventory">Inventory</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.</para>
		/// </remarks>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
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

		#region CuryVatExemptTotal
		/// <inheritdoc cref="CuryVatExemptTotal"/>
		public abstract class curyVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<curyVatExemptTotal> { }
        protected Decimal? _CuryVatExemptTotal;

		/// <summary>
		/// The document <see cref="vatExemptTotal">total that is exempt</see>
		/// from VAT (in the currency of the document).
		/// </summary>
        [PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.vatExemptTotal))]
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
		/// <inheritdoc cref="VatExemptTotal"/>
        public abstract class vatExemptTotal : PX.Data.BQL.BqlDecimal.Field<vatExemptTotal> { }
        protected Decimal? _VatExemptTotal;

		/// <summary>
		/// The document total that is exempt from VAT.
		/// </summary>
		/// <remarks>
		/// <para>This total is calculated as the taxable amount for the tax with the
		/// <see cref="Tax.exemptTax">Include in VAT Exempt Total</see> field is <see langword="true"/>.</para>
		/// <para>This field is available only if the <see cref="FeaturesSet.vATReporting">VAT Reporting</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.</para>
		/// </remarks>
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
		/// <inheritdoc cref="CuryVatTaxableTotal"/>
        public abstract class curyVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<curyVatTaxableTotal> { }
        protected Decimal? _CuryVatTaxableTotal;

		/// <summary>
		/// The document <see cref="vatTaxableTotal">total that is subject</see>
		/// to VAT (in the currency of the document).
		/// </summary>
        [PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.vatTaxableTotal))]
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
		/// <inheritdoc cref="VatTaxableTotal"/>
        public abstract class vatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<vatTaxableTotal> { }
        protected Decimal? _VatTaxableTotal;

		/// <summary>
		/// The document total that is subject to VAT.
		/// </summary>
		/// <remarks>
		/// <para>This field is available only if the <see cref="Tax.exemptTax">Include in VAT Exempt Total</see> field
		/// is <see langword="true"/>. If the document contains multiple transactions with different taxes applied and
		/// for each of the applied taxes <see cref="Tax.exemptTax">Include in VAT Exempt Total</see> field is
		/// <see langword="true"/>, the taxable amount calculated for each line of the document is added to this field.
		/// </para>
		/// <para>This field is available only if the <see cref="FeaturesSet.vATReporting">VAT Reporting</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.</para>
		/// </remarks>
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

		#region CuryTaxTotal
		/// <inheritdoc cref="CuryTaxTotal"/>
		public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }
		protected Decimal? _CuryTaxTotal;

		/// <summary>
		/// The <see cref="taxTotal">total amount</see> of tax paid on the document (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.taxTotal), SignCondition = typeof(defaultOperation.IsEqual<SOOperation.issue>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Total")]
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
		/// <inheritdoc cref="TaxTotal"/>
		public abstract class taxTotal : PX.Data.BQL.BqlDecimal.Field<taxTotal> { }
		protected Decimal? _TaxTotal;

		/// <summary>
		/// The total amount of tax paid on the document.
		/// </summary>
		[PXDBDecimal(4, SignCondition = typeof(defaultOperation.IsEqual<SOOperation.issue>))]
		[PXDefault(TypeCode.Decimal,"0.0")]
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
		#region CuryPremiumFreightAmt
		/// <inheritdoc cref="CuryPremiumFreightAmt"/>
		public abstract class curyPremiumFreightAmt : PX.Data.BQL.BqlDecimal.Field<curyPremiumFreightAmt> { }
		protected Decimal? _CuryPremiumFreightAmt;

		/// <summary>
		/// Any <see cref="premiumFreightAmt">additional freight charges</see> for handling the order
		/// (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.premiumFreightAmt))]
		[PXUIField(DisplayName = "Premium Freight Price")]
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
		/// <inheritdoc cref="PremiumFreightAmt"/>
		public abstract class premiumFreightAmt : PX.Data.BQL.BqlDecimal.Field<premiumFreightAmt> { }
		protected Decimal? _PremiumFreightAmt;

		/// <summary>
		/// Any additional freight charges for handling the order.
		/// </summary>
		/// <remarks>
		/// <para>This field is not available for transfer orders.</para>
		/// <para>To correct the excessive freight charges in a previous order of the customer, premium freight amount
		/// can be manually adjusted.</para>
		/// </remarks>
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
		#region CuryFreightCost
		/// <inheritdoc cref="CuryFreightCost"/>
		public abstract class curyFreightCost : PX.Data.BQL.BqlDecimal.Field<curyFreightCost> { }
		protected Decimal? _CuryFreightCost;

		/// <summary>
		/// The <see cref="freightCost">freight cost</see> calculated for the document
		/// (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.freightCost))]
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
		/// <inheritdoc cref="FreightCost"/>
		public abstract class freightCost : PX.Data.BQL.BqlDecimal.Field<freightCost> { }
		protected Decimal? _FreightCost;

		/// <summary>
		/// The freight cost calculated for the document.
		/// </summary>
		/// <remarks>
		/// This field is not available for transfer orders.
		/// </remarks>
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
		#region FreightCostIsValid
		/// <inheritdoc cref="FreightCostIsValid"/>
		public abstract class freightCostIsValid : PX.Data.BQL.BqlBool.Field<freightCostIsValid> { }
		protected Boolean? _FreightCostIsValid;

		/// <summary>
		/// A Boolean value that indicates whether the freight rates are up to date.
		/// </summary>
		/// <value>
		/// If the value is set to <see langword="false" />, the sales order has been modified and the rates
		/// should be updated.
		/// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Freight Cost Is up-to-date", Enabled=false)]
		public virtual Boolean? FreightCostIsValid
		{
			get
			{
				return this._FreightCostIsValid;
			}
			set
			{
				this._FreightCostIsValid = value;
			}
		}
		#endregion
		#region IsPackageValid
		/// <inheritdoc cref="IsPackageValid"/>
		public abstract class isPackageValid : PX.Data.BQL.BqlBool.Field<isPackageValid> { }
		protected Boolean? _IsPackageValid;

		/// <summary>
		/// A Boolean value that indicates whether the packages of the orders are calculated correct.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsPackageValid
		{
			get
			{
				return this._IsPackageValid;
			}
			set
			{
				this._IsPackageValid = value;
			}
		}
		#endregion
		#region OverrideFreightAmount
		/// <inheritdoc cref="OverrideFreightAmount"/>
		public abstract class overrideFreightAmount : PX.Data.BQL.BqlBool.Field<overrideFreightAmount> { }

		/// <summary>
		/// A Boolean value that indicates whether the <see cref="freightAmt">Freight Price</see> can be changed
		/// manually.
		/// </summary>
		/// <remarks>
		/// The system will preserve the manually entered <see cref="freightAmt">Freight Price</see> value in
		/// the sales order and will not recalculate the value if the quantity, extended price, or amount is modified
		/// in order lines.
		/// </remarks>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override Freight Price")]
		public virtual bool? OverrideFreightAmount
		{
			get;
			set;
		}
		#endregion
		#region FreightAmountSource
		/// <inheritdoc cref="FreightAmountSource"/>
		public abstract class freightAmountSource : PX.Data.BQL.BqlString.Field<freightAmountSource> { }

		/// <summary>
		/// The document from which the system extracts data to calculate the freight price in the sales order
		/// invoice created for the current sales order.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in <see cref="FreightAmountSourceAttribute"/>.
		/// The default value is <see cref="FreightAmountSourceAttribute.shipmentBased"/>.
		/// </value>
		/// <remarks>
		/// If the shipping terms are selected in the <see cref="shipTermsID">Shipping Terms</see> field, the system
		/// fills in this field automatically, based on the value of the
		/// <see cref="ShipTerms.freightAmountSource">Invoice Freight Price Based On</see>.
		/// </remarks>
		[PXDBString(1, IsFixed = true)]
		[PXDefault]
		[FreightAmountSource]
		[PXUIField(DisplayName = "Invoice Freight Price Based On", Enabled = false)]
		[PXFormula(typeof(Switch<Case<Where<SOOrder.overrideFreightAmount, Equal<True>>, FreightAmountSourceAttribute.orderBased>,
			IsNull<Selector<SOOrder.shipTermsID, ShipTerms.freightAmountSource>, FreightAmountSourceAttribute.shipmentBased>>))]
		public virtual string FreightAmountSource
		{
			get;
			set;
		}
		#endregion
		#region CuryFreightAmt
		/// <inheritdoc cref="CuryFreightAmt"/>
		public abstract class curyFreightAmt : PX.Data.BQL.BqlDecimal.Field<curyFreightAmt> { }
		protected Decimal? _CuryFreightAmt;

		/// <summary>
		/// The <see cref="freightAmt">freight amount</see> calculated in accordance with
		/// the shipping terms (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.freightAmt))]
		[PXUIField(DisplayName = "Freight Price", Enabled = false)]
		[PXUIVerify(typeof(Where<SOOrder.curyFreightAmt, GreaterEqual<decimal0>>), PXErrorLevel.Error, CS.Messages.Entry_GE, typeof(decimal0))]
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
		/// <inheritdoc cref="FreightAmt"/>
		public abstract class freightAmt : PX.Data.BQL.BqlDecimal.Field<freightAmt> { }
		protected Decimal? _FreightAmt;

		/// <summary>
		/// The freight amount calculated in accordance with the shipping terms from the
		/// <see cref="shipTermsID">Shipping Terms</see> field.
		/// </summary>
		/// <remarks>
		/// This field is not available for transfer orders.
		/// </remarks>
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
		#region CuryFreightTot
		/// <inheritdoc cref="CuryFreightTot"/>
		public abstract class curyFreightTot : PX.Data.BQL.BqlDecimal.Field<curyFreightTot> { }
		protected Decimal? _CuryFreightTot;

		/// <summary>
		/// The <see cref="freightTot">sum</see> of the
		/// <see cref="curyPremiumFreightAmt">additional freight charges</see> and
		/// the <see cref="curyFreightAmt">freight amount</see> values.
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID),typeof(SOOrder.freightTot))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(Add<SOOrder.curyPremiumFreightAmt, SOOrder.curyFreightAmt>))]
		[PXUIField(DisplayName = "Freight Total")]
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
		/// <inheritdoc cref="FreightTot"/>
		public abstract class freightTot : PX.Data.BQL.BqlDecimal.Field<freightTot> { }

		/// <summary>
		/// The sum of the <see cref="premiumFreightAmt">additional freight charges</see> and
		/// the <see cref="freightAmt">freight amount</see> values.
		/// </summary>
		protected Decimal? _FreightTot;
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
		#region FreightTaxCategoryID
		/// <inheritdoc cref="FreightTaxCategoryID"/>
		public abstract class freightTaxCategoryID : PX.Data.BQL.BqlString.Field<freightTaxCategoryID> { }
		protected String _FreightTaxCategoryID;

		/// <summary>
		/// The identifier of the <see cref="TaxCategory">tax category that applies to the total freight amount</see>.
		/// The field is included in the <see cref="FK.FreightTaxCategory"/> foreign key.
		/// </summary>
		/// <remarks>
		/// <para>The default value is the tax category associated with the <see cref="shipVia">ship via code</see>
		/// ship via code of the order.</para>
		/// <para>This field is not available for transfer orders.</para>
		/// </remarks>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="TaxCategory.taxCategoryID"/> field.
		/// </value>
		[PXDBString(TaxCategory.taxCategoryID.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Freight Tax Category", Visibility = PXUIVisibility.Visible)]
		[SOOrderTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), typeof(taxCalcMode), TaxCalc = TaxCalc.ManualLineCalc,
			CuryTaxableAmtField = typeof(SOOrder.curyTaxableFreightAmt))]
		[SOOrderUnbilledFreightTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), typeof(taxCalcMode), TaxCalc = TaxCalc.ManualLineCalc)]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
        [PXDefault(typeof(Search<Carrier.taxCategoryID, Where<Carrier.carrierID, Equal<Current<SOOrder.shipVia>>>>), SearchOnDefault = false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String FreightTaxCategoryID
		{
			get
			{
				return this._FreightTaxCategoryID;
			}
			set
			{
				this._FreightTaxCategoryID = value;
			}
		}
		#endregion
		#region CuryMiscTot
		/// <inheritdoc cref="CuryMiscTot"/>
		public abstract class curyMiscTot : PX.Data.BQL.BqlDecimal.Field<curyMiscTot> { }
		protected Decimal? _CuryMiscTot;

		/// <summary>
		/// The <see cref="miscTot">total amount</see> calculated as the sum of the amounts in
		/// <see cref="SOLine.curyLineAmt">Amount</see> of the order non-stock items
		/// (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.miscTot))]
		[PXUIField(DisplayName = "Misc. Charges", Enabled = false)]
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
		/// <inheritdoc cref="MiscTot"/>
		public abstract class miscTot : PX.Data.BQL.BqlDecimal.Field<miscTot> { }
		protected Decimal? _MiscTot;

		/// <summary>
		/// The total amount calculated as the sum of the amounts in <see cref="SOLine.curyLineAmt">Amount</see>
		/// of the order non-stock items.
		/// </summary>
		/// <remarks>
		/// This field is not available for transfer orders.
		/// </remarks>
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
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.goodsExtPriceTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Goods")]
		public virtual Decimal? CuryGoodsExtPriceTotal
		{
			get;
			set;
		}
		#endregion
		#region GoodsExtPriceTotal
		/// <inheritdoc cref="GoodsExtPriceTotal"/>
		public abstract class goodsExtPriceTotal : PX.Data.BQL.BqlDecimal.Field<goodsExtPriceTotal> { }

		/// <summary>
		/// The total amount on all lines of the document, except for Misc. Charges, before Line-level discounts are applied.
		/// </summary>
		/// <remarks>
		/// <para>This total is calculated as the sum of the amounts in the
		/// <see cref="SOLine.curyExtPrice">Ext. Price</see> for all stock items and non-stock items that require shipment.
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
		/// <see cref="SOLine.curyExtPrice">Ext. Price</see> of the order non-stock items
		/// (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.miscExtPriceTotal))]
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
		[PXCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.detailExtPriceTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCalced(typeof(Add<SOOrder.curyGoodsExtPriceTotal, SOOrder.curyMiscExtPriceTotal>), typeof(Decimal))]
		[PXFormula(typeof(Add<SOOrder.curyGoodsExtPriceTotal, SOOrder.curyMiscExtPriceTotal>))]
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
		[PXDBCalced(typeof(Add<SOOrder.goodsExtPriceTotal, SOOrder.miscExtPriceTotal>), typeof(Decimal))]
		public virtual Decimal? DetailExtPriceTotal
		{
			get;
			set;
		}
		#endregion

		#region OrderQty
		/// <inheritdoc cref="OrderQty"/>
		public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
		protected Decimal? _OrderQty;

		/// <summary>
		/// The summarized quantity of all items that have been added to the child order from the blanket sales order.
		/// </summary>
		/// <remarks>
		/// If any items that are not from the current blanket sales order have been added to the child order,
		/// their quantity is not summed up to the value in this field.
		/// </remarks>
		[PXDBQuantity()]
		[PXUIField(DisplayName="Ordered Qty.")]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? OrderQty
		{
			get
			{
				return this._OrderQty;
			}
			set
			{
				this._OrderQty = value;
			}
		}
		#endregion
		#region OrderWeight
		/// <inheritdoc cref="OrderWeight"/>
		public abstract class orderWeight : PX.Data.BQL.BqlDecimal.Field<orderWeight> { }
		protected Decimal? _OrderWeight;

		/// <summary>
		/// The total weight of the goods according to the document.
		/// </summary>
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Order Weight", Enabled = false)]
		public virtual Decimal? OrderWeight
		{
			get
			{
				return this._OrderWeight;
			}
			set
			{
				this._OrderWeight = value;
			}
		}
		#endregion
		#region OrderVolume
		/// <inheritdoc cref="OrderVolume"/>
		public abstract class orderVolume : PX.Data.BQL.BqlDecimal.Field<orderVolume> { }
		protected Decimal? _OrderVolume;

		/// <summary>
		/// The total volume of goods according to the document.
		/// </summary>
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Order Volume", Enabled = false)]
		public virtual Decimal? OrderVolume
		{
			get
			{
				return this._OrderVolume;
			}
			set
			{
				this._OrderVolume = value;
			}
		}
		#endregion
		#region CuryOpenOrderTotal
		/// <inheritdoc cref="CuryOpenOrderTotal"/>
		public abstract class curyOpenOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyOpenOrderTotal> { }
		protected Decimal? _CuryOpenOrderTotal;

		/// <summary>
		/// The <see cref="openOrderTotal">sum of unshipped amounts</see> calculated for the lines with nonzero
		/// unshipped quantities of stock items (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.openOrderTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unshipped Amount", Enabled = false)]
		public virtual Decimal? CuryOpenOrderTotal
		{
			get
			{
				return this._CuryOpenOrderTotal;
			}
			set
			{
				this._CuryOpenOrderTotal = value;
			}
		}
		#endregion
		#region OpenOrderTotal
		/// <inheritdoc cref="OpenOrderTotal"/>
		public abstract class openOrderTotal : PX.Data.BQL.BqlDecimal.Field<openOrderTotal> { }
		protected Decimal? _OpenOrderTotal;

		/// <summary>
		/// The sum of unshipped amounts calculated for the lines with nonzero unshipped quantities of stock items.
		/// </summary>
		/// <remarks>
		/// The unshipped amount for each line is calculated as the amount in the
		/// <see cref="SOLine.curyExtPrice">Ext. Price</see> field (after Line-level discounts were applied) divided
		/// by the line quantity (the <see cref="SOLine.orderQty">Qty.</see> field) and multiplied by the unshipped
		/// quantity (the <see cref="SOLine.openQty">Open Qty.</see> field). At the moment of order creation when
		/// no item quantities are shipped, the amount is equal to the <see cref="lineTotal">Line Total</see>;
		/// this total does not include any freight amount.
		/// <para/>
		/// This field is available only if the <see cref="FeaturesSet.inventory">Inventory</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.
		/// </remarks>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OpenOrderTotal
		{
			get
			{
				return this._OpenOrderTotal;
			}
			set
			{
				this._OpenOrderTotal = value;
			}
		}
		#endregion
		#region CuryOpenLineTotal
		/// <inheritdoc cref="CuryOpenLineTotal"/>
		public abstract class curyOpenLineTotal : PX.Data.BQL.BqlDecimal.Field<curyOpenLineTotal> { }
		protected Decimal? _CuryOpenLineTotal;

		/// <summary>
		/// The <see cref="openLineTotal">sum of line open amount</see> (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.openLineTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unshipped Line Total")]
		public virtual Decimal? CuryOpenLineTotal
		{
			get
			{
				return this._CuryOpenLineTotal;
			}
			set
			{
				this._CuryOpenLineTotal = value;
			}
		}
		#endregion
		#region OpenLineTotal
		/// <inheritdoc cref="OpenLineTotal"/>
		public abstract class openLineTotal : PX.Data.BQL.BqlDecimal.Field<openLineTotal> { }
		protected Decimal? _OpenLineTotal;

		/// <summary>
		/// The sum of child <see cref="SOLine4.openAmt">line open amounts</see>. When <see cref="SOLine4.operation"/>
		/// is not equal <see cref="SOOrder.defaultOperation"/>, the line open amounts are with negative values.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OpenLineTotal
		{
			get
			{
				return this._OpenLineTotal;
			}
			set
			{
				this._OpenLineTotal = value;
			}
		}
		#endregion
		#region CuryOpenDiscTotal
		/// <inheritdoc cref="CuryOpenDiscTotal"/>
		public abstract class curyOpenDiscTotal : PX.Data.BQL.BqlDecimal.Field<curyOpenDiscTotal> { }

		/// <summary>
		/// The discount <see cref="openDiscTotal">total</see> of open lines (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.openDiscTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryOpenDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region OpenDiscTotal
		/// <inheritdoc cref="OpenDiscTotal"/>
		public abstract class openDiscTotal : PX.Data.BQL.BqlDecimal.Field<openDiscTotal> { }

		/// <summary>
		/// The discount total of open lines, which is calculated as a sum of values for each line.
		/// The value for each line is calculated as the <see cref="SOLine4.openAmt">line open amount</see>
		/// multiplied by the difference between <see cref="SOLine4.groupDiscountRate"/> and
		/// <see cref="SOLine4.documentDiscountRate"/>. The negative value is used for the
		/// <see cref="SOLine4.openAmt">line open amount</see> if <see cref="SOLine4.operation"/> is not equal to
		/// <see cref="SOOrder.defaultOperation"/>.
		/// </summary>
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OpenDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryOpenTaxTotal
		/// <inheritdoc cref="CuryOpenTaxTotal"/>
		public abstract class curyOpenTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyOpenTaxTotal> { }
		protected Decimal? _CuryOpenTaxTotal;

		/// <summary>
		/// The <see cref="openTaxTotal">sum</see> of tax amounts of child transactions
		/// (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.openTaxTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unshipped Tax Total")]
		public virtual Decimal? CuryOpenTaxTotal
		{
			get
			{
				return this._CuryOpenTaxTotal;
			}
			set
			{
				this._CuryOpenTaxTotal = value;
			}
		}
		#endregion
		#region OpenTaxTotal
		/// <inheritdoc cref="OpenTaxTotal"/>
		public abstract class openTaxTotal : PX.Data.BQL.BqlDecimal.Field<openTaxTotal> { }
		protected Decimal? _OpenTaxTotal;

		/// <summary>
		/// The sum of <see cref="SOTaxTran.taxAmt">tax amounts</see> of child transactions.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OpenTaxTotal
		{
			get
			{
				return this._OpenTaxTotal;
			}
			set
			{
				this._OpenTaxTotal = value;
			}
		}
		#endregion
		#region OpenOrderQty
		/// <inheritdoc cref="OpenOrderQty"/>
		public abstract class openOrderQty : PX.Data.BQL.BqlDecimal.Field<openOrderQty> { }
		protected Decimal? _OpenOrderQty;

		/// <summary>
		/// The quantity of the stock items not yet shipped according to the sales order.
		/// </summary>
		/// <remarks>
		/// <para>The unshipped quantity for each line is specified in <see cref="SOLine.openQty"/>.
		/// </para>
		/// This field is available only if the <see cref="FeaturesSet.inventory">Inventory</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.
		/// </remarks>
		[PXDBQuantity()]
		[PXUIField(DisplayName = "Unshipped Quantity")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OpenOrderQty
		{
			get
			{
				return this._OpenOrderQty;
			}
			set
			{
				this._OpenOrderQty = value;
			}
		}
		#endregion
		#region CuryUnbilledOrderTotal
		/// <inheritdoc cref="CuryUnbilledOrderTotal"/>
		public abstract class curyUnbilledOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyUnbilledOrderTotal> { }
		protected Decimal? _CuryUnbilledOrderTotal;

		/// <summary>
		/// The <see cref="unbilledOrderTotal">unbilled balance</see> of the sales order, which is calculated as
		/// the sum of the <see cref="SOLine.curyUnbilledAmt">unbilled line amounts</see>
		/// (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.unbilledOrderTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Balance", Enabled = false)]
		public virtual Decimal? CuryUnbilledOrderTotal
		{
			get
			{
				return this._CuryUnbilledOrderTotal;
			}
			set
			{
				this._CuryUnbilledOrderTotal = value;
			}
		}
		#endregion
		#region UnbilledOrderTotal
		/// <inheritdoc cref="UnbilledOrderTotal"/>
		public abstract class unbilledOrderTotal : PX.Data.BQL.BqlDecimal.Field<unbilledOrderTotal> { }
		protected Decimal? _UnbilledOrderTotal;

		/// <summary>
		/// The unbilled balance of the sales order, which is calculated as the sum
		/// of the <see cref="SOLine.curyUnbilledAmt">unbilled line amounts</see>.
		/// </summary>
		/// <remarks>
		/// The value is calculated as the sum of the <see cref="SOLine.curyUnbilledAmt">unbilled line amounts</see>,
		/// unbilled amount of the freight price and premium freight price, and the unbilled amount of all taxes,
		/// minus all line, group, and document discounts of the order.
		/// <para>This field is not available for transfer orders.</para>
		/// </remarks>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledOrderTotal
		{
			get
			{
				return this._UnbilledOrderTotal;
			}
			set
			{
				this._UnbilledOrderTotal = value;
			}
		}
		#endregion
		#region CuryUnbilledLineTotal
		/// <inheritdoc cref="CuryUnbilledLineTotal"/>
		public abstract class curyUnbilledLineTotal : PX.Data.BQL.BqlDecimal.Field<curyUnbilledLineTotal> { }
		protected Decimal? _CuryUnbilledLineTotal;

		/// <summary>
		/// The <see cref="unbilledLineTotal">sum</see> of unbilled amounts of child lines
		/// (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.unbilledLineTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Line Total")]
		public virtual Decimal? CuryUnbilledLineTotal
		{
			get
			{
				return this._CuryUnbilledLineTotal;
			}
			set
			{
				this._CuryUnbilledLineTotal = value;
			}
		}
		#endregion
		#region UnbilledLineTotal
		/// <inheritdoc cref="UnbilledLineTotal"/>
		public abstract class unbilledLineTotal : PX.Data.BQL.BqlDecimal.Field<unbilledLineTotal> { }
		protected Decimal? _UnbilledLineTotal;

		/// <summary>
		/// The sum of <see cref="BlanketSOLine.unbilledAmt">unbilled amount</see> of child lines for which the
		/// following condition is <see langword="true"/>: <see cref="BlanketSOLine.lineType"/> is not equal to
		/// <see cref="SOLineType.miscCharge"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledLineTotal
		{
			get
			{
				return this._UnbilledLineTotal;
			}
			set
			{
				this._UnbilledLineTotal = value;
			}
		}
		#endregion
		#region CuryUnbilledMiscTot
		/// <inheritdoc cref="CuryUnbilledMiscTot"/>
		public abstract class curyUnbilledMiscTot : PX.Data.BQL.BqlDecimal.Field<curyUnbilledMiscTot> { }
		protected Decimal? _CuryUnbilledMiscTot;

		/// <summary>
		/// The <see cref="unbilledMiscTot">sum</see> of unbilled amounts of child lines
		/// (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.unbilledMiscTot))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Misc. Total")]
		public virtual Decimal? CuryUnbilledMiscTot
		{
			get
			{
				return this._CuryUnbilledMiscTot;
			}
			set
			{
				this._CuryUnbilledMiscTot = value;
			}
		}
		#endregion
		#region UnbilledMiscTot
		/// <inheritdoc cref="UnbilledMiscTot"/>
		public abstract class unbilledMiscTot : PX.Data.BQL.BqlDecimal.Field<unbilledMiscTot> { }
		protected Decimal? _UnbilledMiscTot;

		/// <summary>
		/// The sum of <see cref="BlanketSOLine.unbilledAmt">unbilled amounts</see> of child lines for which the
		/// following condition is <see langword="true"/>:
		/// <see cref="BlanketSOLine.lineType"/> is equal to <see cref="SOLineType.miscCharge"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledMiscTot
		{
			get
			{
				return this._UnbilledMiscTot;
			}
			set
			{
				this._UnbilledMiscTot = value;
			}
		}
		#endregion
		#region CuryUnbilledTaxTotal
		/// <inheritdoc cref="CuryUnbilledTaxTotal"/>
		public abstract class curyUnbilledTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyUnbilledTaxTotal> { }
		protected Decimal? _CuryUnbilledTaxTotal;

		/// <summary>
		/// The <see cref="unbilledTaxTotal">total amount</see> of tax paid on the unbilled part of the document
		/// (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.unbilledTaxTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Tax Total")]
		public virtual Decimal? CuryUnbilledTaxTotal
		{
			get
			{
				return this._CuryUnbilledTaxTotal;
			}
			set
			{
				this._CuryUnbilledTaxTotal = value;
			}
		}
		#endregion
		#region UnbilledTaxTotal
		/// <inheritdoc cref="UnbilledTaxTotal"/>
		public abstract class unbilledTaxTotal : PX.Data.BQL.BqlDecimal.Field<unbilledTaxTotal> { }
		protected Decimal? _UnbilledTaxTotal;

		/// <summary>
		/// The total amount of tax paid on the unbilled part of the document.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledTaxTotal
		{
			get
			{
				return this._UnbilledTaxTotal;
			}
			set
			{
				this._UnbilledTaxTotal = value;
			}
		}
		#endregion
		#region CuryUnbilledDiscTotal
		/// <inheritdoc cref="CuryUnbilledDiscTotal"/>
		public abstract class curyUnbilledDiscTotal : PX.Data.BQL.BqlDecimal.Field<curyUnbilledDiscTotal> { }

		/// <summary>
		/// The discount <see cref="unbilledDiscTotal">total</see> for unbilled lines (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.unbilledDiscTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryUnbilledDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region UnbilledDiscTotal
		/// <inheritdoc cref="UnbilledDiscTotal"/>
		public abstract class unbilledDiscTotal : PX.Data.BQL.BqlDecimal.Field<unbilledDiscTotal> { }

		/// <summary>
		///The discount total for unbilled lines.
		/// </summary>
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryBilledFreightTot
		/// <inheritdoc cref="CuryBilledFreightTot"/>
		public abstract class curyBilledFreightTot : BqlDecimal.Field<curyBilledFreightTot> { }
		/// <summary>
		/// The sum of amounts of <see cref="SOFreightDetail">the freight details</see> that have been
		/// transferred to the SO invoice generated for the sales order (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(billedFreightTot))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryBilledFreightTot
		{
			get;
			set;
		}
		#endregion
		#region BilledFreightTot
		/// <inheritdoc cref="BilledFreightTot"/>
		public abstract class billedFreightTot : BqlDecimal.Field<billedFreightTot> { }
		/// <summary>
		/// The sum of amounts of <see cref="SOFreightDetail">the freight details</see> that have been
		/// transferred to the SO invoice generated for the sales order (in the base currency).
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BilledFreightTot
		{
			get;
			set;
		}
		#endregion
		#region CuryUnbilledFreightTot
		/// <inheritdoc cref="CuryUnbilledFreightTot"/>
		public abstract class curyUnbilledFreightTot : BqlDecimal.Field<curyUnbilledFreightTot> { }
		/// <summary>
		/// The unbilled part of the sales order <see cref="SOOrder.curyFreightTot">freight total</see>
		/// in the document currency.
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(unbilledFreightTot))]
		[PXFormula(typeof(
			IIf<Where<completed, Equal<True>, And<Add<releasedCntr, billedCntr>, GreaterEqual<shipmentCntr>,
				Or<Selector<orderType, SOOrderType.requireShipping>, Equal<False>, And<Add<releasedCntr, billedCntr>, Greater<int0>,
				Or<Sub<curyFreightTot, curyBilledFreightTot>, Less<decimal0>>>>>>, decimal0,
			Sub<curyFreightTot, curyBilledFreightTot>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryUnbilledFreightTot
		{
			get;
			set;
		}
		#endregion
		#region UnbilledFreightTot
		/// <inheritdoc cref="UnbilledFreightTot"/>
		public abstract class unbilledFreightTot : BqlDecimal.Field<unbilledFreightTot> { }
		/// <summary>
		/// The unbilled part of the sales order <see cref="SOOrder.curyFreightTot">freight total</see>
		/// in the base currency.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? UnbilledFreightTot
		{
			get;
			set;
		}
		#endregion
		#region UnbilledOrderQty
		/// <inheritdoc cref="UnbilledOrderQty"/>
		public abstract class unbilledOrderQty : PX.Data.BQL.BqlDecimal.Field<unbilledOrderQty> { }
		protected Decimal? _UnbilledOrderQty;

		/// <summary>
		/// The quantity of stock and non-stock items that were not yet billed.
		/// </summary>
		/// <remarks>
		/// This field is not available for transfer orders.
		/// </remarks>
		[PXDBQuantity()]
		[PXUIField(DisplayName = "Unbilled Quantity")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledOrderQty
		{
			get
			{
				return this._UnbilledOrderQty;
			}
			set
			{
				this._UnbilledOrderQty = value;
			}
		}
		#endregion
		#region CuryControlTotal
		/// <inheritdoc cref="CuryControlTotal"/>
		public abstract class curyControlTotal : PX.Data.BQL.BqlDecimal.Field<curyControlTotal> { }
		protected Decimal? _CuryControlTotal;

		/// <summary>
		/// The <see cref="controlTotal">control total</see> of the document (in the currency of the document).
		/// A user enters this amount manually.
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.controlTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Control Total")]
		public virtual Decimal? CuryControlTotal
		{
			get
			{
				return this._CuryControlTotal;
			}
			set
			{
				this._CuryControlTotal = value;
			}
		}
		#endregion
		#region ControlTotal
		/// <inheritdoc cref="ControlTotal"/>
		public abstract class controlTotal : PX.Data.BQL.BqlDecimal.Field<controlTotal> { }
		protected Decimal? _ControlTotal;

		/// <summary>
		/// The control total of the document. A user enters this amount manually.
		/// </summary>
		/// <remarks>
		/// <para>This amount should be equal to the sum of the amounts of all detail lines of the document.</para>
		/// <para>The document can be released only if the value in this field is equal to the value in
		/// <see cref="orderTotal"/>.</para>
		/// <para>This field is available only if the
		/// <see cref="SOOrderType.requireControlTotal">Require Control Total</see> field is <see langword="true"/>.
		/// </para>
		/// </remarks>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ControlTotal
		{
			get
			{
				return this._ControlTotal;
			}
			set
			{
				this._ControlTotal = value;
			}
		}
		#endregion
		#region CuryPaymentTotal
		/// <inheritdoc cref="CuryPaymentTotal"/>
		public abstract class curyPaymentTotal : PX.Data.BQL.BqlDecimal.Field<curyPaymentTotal> { }
		protected Decimal? _CuryPaymentTotal;

		/// <summary>
		/// The <see cref="paymentTotal">total amount</see> that has been paid for this sales order (in the currency
		/// of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.paymentTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Paid", Enabled=false)]
		public virtual Decimal? CuryPaymentTotal
		{
			get
			{
				return this._CuryPaymentTotal;
			}
			set
			{
				this._CuryPaymentTotal = value;
			}
		}
		#endregion
		#region PaymentTotal
		/// <inheritdoc cref="PaymentTotal"/>
		public abstract class paymentTotal : PX.Data.BQL.BqlDecimal.Field<paymentTotal> { }
		protected Decimal? _PaymentTotal;

		/// <summary>
		/// The total amount that has been paid for this sales order.
		/// </summary>
		/// <remarks>
		/// This field is not available for transfer orders.
		/// </remarks>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? PaymentTotal
		{
			get
			{
				return this._PaymentTotal;
			}
			set
			{
				this._PaymentTotal = value;
			}
		}
		#endregion
		#region OverrideTaxZone
		/// <inheritdoc cref="OverrideTaxZone"/>
		public abstract class overrideTaxZone : PX.Data.BQL.BqlBool.Field<overrideTaxZone> { }
		protected Boolean? _OverrideTaxZone;

		/// <summary>
		/// A Boolean value that specifies (if set to <see langword="true"/>) that the specified customer tax zone will
		/// not be overridden if any location-related information is changed for the sales order.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override Tax Zone")]
		public virtual Boolean? OverrideTaxZone
		{
			get
			{
				return this._OverrideTaxZone;
			}
			set
			{
				this._OverrideTaxZone = value;
			}
		}
		#endregion
		#region TaxZoneID
		/// <inheritdoc cref="TaxZoneID"/>
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		protected String _TaxZoneID;

		/// <summary>
		/// The identifier of the <see cref="TX.TaxZone">tax zone</see> to be used to process customer sales orders.
		/// The field is included in the <see cref="FK.TaxZone"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="TX.TaxZone.taxZoneID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Customer Tax Zone", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
		[PXFormula(typeof(Default<SOOrder.customerLocationID>))]
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
		/// <inheritdoc cref="TaxCalcMode"/>
		public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }

		/// <summary>
		/// The tax calculation mode to be used for the sales order.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <list type="bullet">
		/// <item><description><see cref="TaxCalculationMode.taxSetting"/> (default): The document
		/// uses the settings of the selected customer, or of the location of the customer if the
		/// <see cref="FeaturesSet.accountLocations">Business Account Locations</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.</description></item>
		/// <item><description><see cref="TaxCalculationMode.gross"/>: The tax amount is included in
		/// the item price.</description></item>
		/// <item><description><see cref="TaxCalculationMode.net"/>: The tax amount is not included in
		/// the item price.</description></item>
		/// </list>
		/// </value>
		/// <remarks>
		/// This field is available only if the
		/// <see cref="FeaturesSet.netGrossEntryMode">Net/Gross Entry Mode</see>
		/// feature has been enabled on the Enable/Disable Features (CS100000) form.
		/// </remarks>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TaxCalculationMode.TaxSetting, typeof(Search<Location.cTaxCalcMode, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>,
			And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>))]
		[TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string TaxCalcMode { get; set; }
		#endregion
		#region ExternalTaxExemptionNumber
		/// <inheritdoc cref="ExternalTaxExemptionNumber"/>
		public abstract class externalTaxExemptionNumber : PX.Data.BQL.BqlString.Field<externalTaxExemptionNumber> { }

		/// <summary>
		/// The tax exemption number for reporting purposes.
		/// </summary>
		/// <remarks>
		/// The field is used if the system is integrated with External Tax Calculation
		/// and the <see cref="FeaturesSet.AvalaraTax">External Tax Calculation Integration</see> feature is enabled on
		/// the Enable/Disable Features (CS100000) form.
		/// </remarks>
		[PXDefault(typeof(Search<Location.cAvalaraExemptionNumber,
			Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>,
				And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Exemption Number")]
		public virtual String ExternalTaxExemptionNumber { get; set; }
		#endregion
		#region AvalaraCustomerUsageType
		/// <inheritdoc cref="AvalaraCustomerUsageType"/>
		public abstract class avalaraCustomerUsageType : PX.Data.BQL.BqlString.Field<avalaraCustomerUsageType> { }
		protected String _AvalaraCustomerUsageType;

		/// <summary>
		/// The entity usage type of the customer location if sales to this location are tax-exempt.
		/// </summary>
		/// <value>
		/// By default, the system copies the value of this field from the customer record.
		/// </value>
		/// <remarks>
		/// <para>This field is available only if the
		/// <see cref="FeaturesSet.AvalaraTax">External Tax Calculation Integration</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.</para>
		/// <para>This field is not available for transfer orders.</para>
		/// </remarks>
		[PXDefault(
			TXAvalaraCustomerUsageType.Default,
			typeof(Search<Location.cAvalaraCustomerUsageType,
					Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>,
						And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>))]
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
		#region ProjectID
		/// <inheritdoc cref="ProjectID"/>
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;

		/// <summary>
		/// The identifier of the <see cref="PMProject">project</see>.
		/// The field is included in the <see cref="FK.Project"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMProject.contractID"/> field.
		/// The value specifies the project with which this sales order is associated or the non-project code, which
		/// indicates that this order is not associated with any project.
		/// The non-project code is specified on the Projects Preferences (PM101000) form.
		/// </value>
		/// <remarks>
		/// <para>This field is available only if the
		/// <see cref="FeaturesSet.projectAccounting">Project Accounting</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form and the integration of the Projects
		/// submodule with Sales Orders has been enabled
		/// (that is, <see cref="PMSetup.visibleInSO"/> is <see langword="true"/>.</para>
		/// </remarks>
		[ProjectDefault(BatchModule.SO)]
		[PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
		[PXRestrictor(typeof(Where<PMProject.visibleInSO, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
		[PXRestrictor(typeof(Where<PMProject.restrictProjectSelect, Equal<PMRestrictOption.allProjects>,
			Or<PMProject.customerID, Equal<Current<SOOrder.customerID>>,
			Or<Current<SOOrder.behavior>, Equal<SOBehavior.tR>,
			Or<PMProject.nonProject, Equal<True>>>>>), PM.Messages.CustomerMismatch, typeof(PMProject.contractCD))]
		[ProjectBase]
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
		#region ShipComplete
		/// <inheritdoc cref="ShipComplete"/>
		public abstract class shipComplete : PX.Data.BQL.BqlString.Field<shipComplete> { }
		protected String _ShipComplete;

		/// <summary>
		/// An option that controls whether incomplete and partial shipments for the order are allowed.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <list type="bullet">
		/// <item><description><see cref="SOShipComplete.shipComplete"/>: The first shipment for
		/// the order should include all lines of the order.</description></item>
		/// <item><description><see cref="SOShipComplete.backOrderAllowed"/>: The first shipment
		/// for the order should include at least one order line.</description></item>
		/// <item><description><see cref="SOShipComplete.cancelRemainder"/>: The first shipment
		/// for the order should include at least one order line.</description></item>
		/// </list>
		/// </value>
		/// <remarks>
		/// This field is available only if the <see cref="FeaturesSet.inventory">Inventory</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.
		/// form.
		/// </remarks>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(SOShipComplete.CancelRemainder)]
		[SOShipComplete.List()]
		[PXUIField(DisplayName = "Shipping Rule")]
		public virtual String ShipComplete
		{
			get
			{
				return this._ShipComplete;
			}
			set
			{
				this._ShipComplete = value;
			}
		}
		#endregion
		#region FOBPoint
		/// <inheritdoc cref="SOOrder.FOBPoint"/>
		public abstract class fOBPoint : PX.Data.BQL.BqlString.Field<fOBPoint> { }
		protected String _FOBPoint;

		/// <summary>
		/// The identifier of the <see cref="CS.FOBPoint">point</see> where ownership of
		/// the goods is transferred to the customer.
		/// The field is included in the <see cref="FK.FOBPoint"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="CS.FOBPoint"/> field.
		/// </value>
		/// <remarks>
		/// This field is available only for blanket sales orders and cannot be empty.
		/// </remarks>
		[ActiveFOBPointSelector(typeof(FOBPoint.fOBPointID))]
		[PXDefault(typeof(Search<Location.cFOBPointID, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>), PersistingCheck=PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(FK.FOBPoint))]
		public virtual String FOBPoint
		{
			get
			{
				return this._FOBPoint;
			}
			set
			{
				this._FOBPoint = value;
			}
		}
		#endregion
		#region ShipVia
		/// <inheritdoc cref="ShipVia"/>
		public abstract class shipVia : PX.Data.BQL.BqlString.Field<shipVia> { }
		protected String _ShipVia;

		/// <summary>
		/// The identifier of the <see cref="CS.Carrier">ship via code</see> that represents the carrier and
		/// its service to be used for shipping the ordered goods.
		/// The field is included in the <see cref="FK.Carrier"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="CS.Carrier.carrierID"/> field.
		/// </value>
		/// <remarks>
		/// For this ship via code, if Manual is specified as the freight calculation method, the freight amount must
		/// be specified in the <see cref="CuryFreightAmt">Freight Price</see> field.
		/// Changing the Ship Via code for an open sales order may update the
		/// <see cref="taxZoneID">customer tax zone</see> field.
		/// </remarks>
		[PXActiveCarrierSelector(typeof(Carrier.carrierID),
			typeof(Carrier.carrierID), typeof(Carrier.description), typeof(Carrier.isCommonCarrier), typeof(Carrier.confirmationRequired), typeof(Carrier.packageRequired))]
		[PXDefault(typeof(Search<Location.cCarrierID, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>, And<Current<SOOrder.behavior>, NotEqual<SOBehavior.bL>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(FK.Carrier))]
		public virtual String ShipVia
		{
			get
			{
				return this._ShipVia;
			}
			set
			{
				this._ShipVia = value;
			}
		}
		#endregion
		#region WillCall
		/// <summary>
		/// A Boolean value that indicates whether the customer picks the goods from the warehouse (will call).
		/// </summary>
		/// <value><para>If the value is <see langword="false" />, the common carrier is
		/// to be used for shipping goods.</para>
		/// <para>If <see cref="shipVia"/> is <see langword="null"/>,
		/// the value of this field is <see langword="true"/>.
		/// If <see cref="shipVia"/> is not <see langword="null"/>, the value of this field directly corresponds
		/// to the state of the <see cref="Carrier.isCommonCarrier">Common Carrier</see> field for
		/// the selected ship via code.</para>
		/// </value>
		[PXBool]
		[PXFormula(typeof(Switch<Case2<Where<Selector<shipVia, Carrier.isCommonCarrier>, NotEqual<True>>, True>, False>))]
		[PXUIField(DisplayName = "Will Call", IsReadOnly = true)]
		public bool? WillCall { get; set; }

		/// <inheritdoc cref="WillCall"/>
		public abstract class willCall : PX.Data.BQL.BqlBool.Field<willCall> { }
		#endregion
		#region PackageLineCntr
		/// <inheritdoc cref="PackageLineCntr"/>
		public abstract class packageLineCntr : PX.Data.BQL.BqlInt.Field<packageLineCntr> { }
		protected Int32? _PackageLineCntr;

		/// <summary>
		/// The counter of child package lines added to the order.
		/// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? PackageLineCntr
		{
			get
			{
				return this._PackageLineCntr;
			}
			set
			{
				this._PackageLineCntr = value;
			}
		}
		#endregion
		#region PackageWeight
		/// <inheritdoc cref="PackageWeight"/>
		public abstract class packageWeight : PX.Data.BQL.BqlDecimal.Field<packageWeight> { }
		protected Decimal? _PackageWeight;

		/// <summary>
		/// The total (gross) weight of the packages for this sales order, including the weight of the boxes used for
		/// packages.
		/// </summary>
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Package Weight", Enabled=false)]
		public virtual Decimal? PackageWeight
		{
			get
			{
				return this._PackageWeight;
			}
			set
			{
				this._PackageWeight = value;
			}
		}
		#endregion
		#region UseCustomerAccount
		/// <inheritdoc cref="UseCustomerAccount"/>
		public abstract class useCustomerAccount : PX.Data.BQL.BqlBool.Field<useCustomerAccount> { }
		protected Boolean? _UseCustomerAccount;

		/// <summary>
		/// A Boolean value that specifies (if set to <see langword="true"/>) that the customer account with the
		/// carrier should be billed for the shipping of this order.
		/// </summary>
		/// <remarks>
		/// This field is available only if the
		/// <see cref="FeaturesSet.carrierIntegration">Shipping Carrier Integration</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.
		/// </remarks>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Customer's Account")]
		public virtual Boolean? UseCustomerAccount
		{
			get
			{
				return this._UseCustomerAccount;
			}
			set
			{
				this._UseCustomerAccount = value;
			}
		}
		#endregion
		#region Resedential
		/// <inheritdoc cref="Resedential"/>
		public abstract class resedential : PX.Data.BQL.BqlBool.Field<resedential> { }
		protected Boolean? _Resedential;

		/// <summary>
		/// A Boolean value that indicates whether the shipment should be delivered to a residential area.
		/// </summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<Location.cResedential, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>))]
		[PXUIField(DisplayName = "Residential Delivery")]
		public virtual Boolean? Resedential
		{
			get
			{
				return this._Resedential;
			}
			set
			{
				this._Resedential = value;
			}
		}
		#endregion
		#region SaturdayDelivery
		/// <inheritdoc cref="SaturdayDelivery"/>
		public abstract class saturdayDelivery : PX.Data.BQL.BqlBool.Field<saturdayDelivery> { }
		protected Boolean? _SaturdayDelivery;

		/// <summary>
		/// A Boolean value that indicates whether the order may be delivered on Saturday.
		/// </summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<Location.cSaturdayDelivery, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>))]
		[PXUIField(DisplayName = "Saturday Delivery")]
		public virtual Boolean? SaturdayDelivery
		{
			get
			{
				return this._SaturdayDelivery;
			}
			set
			{
				this._SaturdayDelivery = value;
			}
		}
		#endregion
		#region GroundCollect
		/// <inheritdoc cref="GroundCollect"/>
		public abstract class groundCollect : PX.Data.BQL.BqlBool.Field<groundCollect> { }
		protected Boolean? _GroundCollect;

		/// <summary>
		/// A Boolean value that indicates whether a user selects to use the FedEx Ground Collect option.
		/// </summary>
		/// <remarks>
		/// This field is available only if the
		/// <see cref="FeaturesSet.carrierIntegration">Shipping Carrier Integration</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form, integration with the FedEx carrier
		/// is established, and FedEx is selected in the <see cref="shipVia">Ship Via</see> field.
		/// </remarks>
		[PXDBBool()]
		[PXDefault(typeof(Search<Location.cGroundCollect, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>))]
		[PXUIField(DisplayName = "Ground Collect")]
		public virtual Boolean? GroundCollect
		{
			get
			{
				return this._GroundCollect;
			}
			set
			{
				this._GroundCollect = value;
			}
		}
		#endregion
		#region Insurance
		/// <inheritdoc cref="Insurance"/>
		public abstract class insurance : PX.Data.BQL.BqlBool.Field<insurance> { }
		protected Boolean? _Insurance;

		/// <summary>
		/// A Boolean value that indicates whether a user selects to indicate that insurance is required for this order.
		/// </summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<Location.cInsurance, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>))]
		[PXUIField(DisplayName = "Insurance")]
		public virtual Boolean? Insurance
		{
			get
			{
				return this._Insurance;
			}
			set
			{
				this._Insurance = value;
			}
		}
		#endregion
		#region Priority
		/// <inheritdoc cref="Priority"/>
		public abstract class priority : PX.Data.BQL.BqlShort.Field<priority> { }
		protected Int16? _Priority;

		/// <summary>
		/// The level of priority for processing orders of this customer, as specified
		/// <see cref="Location.cOrderPriority"/> field for the <see cref="FK.Customer">customer</see>.
		/// </summary>
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName="Priority")]
		public virtual Int16? Priority
		{
			get
			{
				return this._Priority;
			}
			set
			{
				this._Priority = value;
			}
		}
		#endregion
		#region SalesPersonID
		/// <inheritdoc cref="SalesPersonID"/>
		public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }
		protected Int32? _SalesPersonID;

		/// <summary>
		/// The identifier of the <see cref="AR.SalesPerson">salesperson</see> to be used by default
		/// for each sales order line.
		/// The field is included in the <see cref="FK.SalesPerson"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="AR.SalesPerson.salesPersonID"/> field.
		/// </value>
		[SalesPerson(DisplayName = "Default Salesperson")]
		[PXDefault(typeof(Search<CustDefSalesPeople.salesPersonID, Where<CustDefSalesPeople.bAccountID, Equal<Current<SOOrder.customerID>>, And<CustDefSalesPeople.locationID, Equal<Current<SOOrder.customerLocationID>>, And<CustDefSalesPeople.isDefault, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<SOOrder.salesPersonID>.IsRelatedTo<SalesPerson.salesPersonID>))]
		public virtual Int32? SalesPersonID
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
		#region CommnPct
		/// <inheritdoc cref="CommnPct"/>
		public abstract class commnPct : PX.Data.BQL.BqlDecimal.Field<commnPct> { }

		/// <summary>
		/// The default commission percentage of the salesperson.
		/// </summary>
		protected Decimal? _CommnPct;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal,"0.0")]
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
		#region TermsID
		/// <inheritdoc cref="TermsID"/>
		public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }
		protected String _TermsID;

		/// <summary>
		/// The identifier of the <see cref="CS.Terms">credit terms</see> used in relations with the customer.
		/// The field is included in the <see cref="FK.Terms"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="CS.Terms.termsID"/> field.
		/// </value>
		/// <remarks>
		/// This field is not available for transfer orders.
		/// </remarks>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<Customer.termsID, Where<Customer.bAccountID, Equal<Current<SOOrder.customerID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
		[Terms(typeof(SOOrder.invoiceDate), typeof(SOOrder.dueDate), typeof(SOOrder.discDate), typeof(SOOrder.curyOrderTotal), typeof(SOOrder.curyTermsDiscAmt), typeof(SOOrder.curyTaxTotal), typeof(SOOrder.branchID))]
		[PXForeignReference(typeof(Field<termsID>.IsRelatedTo<Terms.termsID>))]
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
		#region DueDate
		/// <inheritdoc cref="DueDate"/>
		public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
		protected DateTime? _DueDate;

		/// <summary>
		/// The due date of the invoice according to the credit terms.
		/// </summary>
		/// <remarks>
		/// This field is not available for transfer orders.
		/// </remarks>
		[PXDBDate()]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Due Date")]
		public virtual DateTime? DueDate
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
		/// <inheritdoc cref="DiscDate"/>
		public abstract class discDate : PX.Data.BQL.BqlDateTime.Field<discDate> { }
		protected DateTime? _DiscDate;

		/// <summary>
		/// The date when the cash discount is available for the invoice based on the credit terms.
		/// </summary>
		/// <remarks>
		/// This field is not available for transfer orders.
		/// </remarks>
		[PXDBDate()]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
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
		/// <inheritdoc cref="InvoiceNbr"/>
		public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
		protected String _InvoiceNbr;

		/// <summary>
		/// The reference number of the original invoice (which lists the goods that were ordered and
		/// later returned by the customer).
		/// The field is included in the <see cref="FK.Invoice"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="SOInvoice.refNbr"/> field.
		/// </value>
		/// <remarks>
		/// This field is available for orders of the CR, RC, RR, and RM types.
		/// </remarks>
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Invoice Nbr.", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
		[SOInvoiceNbr()]
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
		/// <inheritdoc cref="InvoiceDate"/>
		public abstract class invoiceDate : PX.Data.BQL.BqlDateTime.Field<invoiceDate> { }
		protected DateTime? _InvoiceDate;

		/// <summary>
		/// The date of the invoice generated for the order.
		/// </summary>
		/// <remarks>
		/// <para>Date can be entered manually if the <see cref="SOOrderType.billSeparately">Bill Separately</see>
		/// field is <see langword="true"/> for the order type.</para>
		/// <para>This field is not available for transfer orders.</para>
		/// </remarks>
		[PXDBDate()]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Invoice Date", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(Default<SOOrder.orderDate>))]
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
		#region FinPeriodID
		/// <inheritdoc cref="FinPeriodID"/>
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;

		/// <summary>
		/// The period to post the transactions generated by the invoice.
		/// </summary>
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[SOFinPeriod(typeof(SOOrder.invoiceDate), typeof(SOOrder.branchID))]
		//[AROpenPeriod(typeof(SOOrder.invoiceDate))]
		[PXUIField(DisplayName = "Post Period")]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region WorkgroupID
		/// <inheritdoc cref="WorkgroupID"/>
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		protected int? _WorkgroupID;

		/// <summary>
		/// The identifier of the <see cref="EPCompanyTree">workgroup</see> responsible for the sales order.
		/// The field is included in the <see cref="FK.Workgroup"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="EPCompanyTree.workGroupID"/> field.
		/// </value>
		[PXDBInt]
		[PXDefault(typeof(Customer.workgroupID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PX.TM.PXCompanyTreeSelector]
		[PXUIField(DisplayName = "Workgroup", Enabled = false)]
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
		/// <inheritdoc cref="OwnerID"/>
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		protected int? _OwnerID;

		/// <summary>
		/// The identifier of the <see cref="CR.Standalone">employee</see> in the workgroup who is responsible for
		/// the sales order.
		/// The field is included in the <see cref="FK.Owner"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="CR.Standalone.EPEmployee.bAccountID"/>
		/// field.
		/// </value>
		[PXDefault(typeof(Coalesce<
			Search<CREmployee.defContactID, Where<CREmployee.userID, Equal<Current<AccessInfo.userID>>, And<CREmployee.vStatus, NotEqual<VendorStatus.inactive>>>>,
            Search<BAccount.ownerID, Where<BAccount.bAccountID, Equal<Current<SOOrder.customerID>>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PX.TM.Owner(typeof(workgroupID))]
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
		#region EmployeeID
		/// <inheritdoc cref="EmployeeID"/>
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		protected Int32? _EmployeeID;

		/// <summary>
		/// The identifier of the <see cref="BAccount">business account</see> of the employee of the order's
		/// <see cref="ownerID">owner</see>.
		/// </summary>
		[PXInt()]
        [PXFormula(typeof(Switch<Case<Where<SOOrder.ownerID, IsNotNull>, Selector<SOOrder.ownerID, EP.EPEmployee.bAccountID>>, Null>))]//#B36E
        public virtual Int32? EmployeeID
		{
			get
			{
				return this._EmployeeID;
			}
			set
			{
				this._EmployeeID = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#region GotReadyForArchiveAt
		[PX.Data.Archiving.PXDBGotReadyForArchive]
		[PXFormula(typeof(
			Today.When<
				cancelled.IsEqual<True>.
				Or<
					completed.IsEqual<True>.
					And<openLineCntr.IsEqual<Zero>>.
					And<
						behavior.IsEqual<SOBehavior.qT>.
						Or<
							behavior.IsIn<SOBehavior.sO, SOBehavior.iN, SOBehavior.cM, SOBehavior.rM>.
							And<shipmentCntr.IsGreater<Zero>>.
							And<shipmentCntr.IsEqual<releasedCntr>>>.
						Or<
							behavior.IsEqual<SOBehavior.tR>.
							And<shipmentCntr.IsGreater<Zero>>.
							And<openShipmentCntr.IsEqual<Zero>>>.
						Or<
							behavior.IsEqual<SOBehavior.bL>.
							And<unbilledOrderQty.IsEqual<Zero>>.
							And<curyUnbilledOrderTotal.IsEqual<Zero>>>>>>.
			ElseNull))]
		public virtual DateTime? GotReadyForArchiveAt { get; set; }
		public abstract class gotReadyForArchiveAt : PX.Data.BQL.BqlDateTime.Field<gotReadyForArchiveAt> { }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
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
		#region CustomerID_Customer_acctName
		/// <summary>
		/// The account name of the <see cref="AR.Customer">customer</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="AR.Customer.acctName"/> field.
		/// </value>
		public abstract class customerID_Customer_acctName : PX.Data.BQL.BqlString.Field<customerID_Customer_acctName> { }
		#endregion
		#region CuryTermsDiscAmt
		/// <inheritdoc cref="CuryTermsDiscAmt"/>
		public abstract class curyTermsDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyTermsDiscAmt> { }
		protected Decimal? _CuryTermsDiscAmt = 0m;

		/// <summary>
		/// The <see cref="termsDiscAmt">discount amount</see> of the prompt payment (in the currency of the document).
		/// </summary>
		[PXDecimal(4)]
		public virtual Decimal? CuryTermsDiscAmt
		{
			get
			{
				return this._CuryTermsDiscAmt;
			}
			set
			{
				this._CuryTermsDiscAmt = value;
			}
		}
		#endregion
		#region TermsDiscAmt
		/// <inheritdoc cref="TermsDiscAmt"/>
		public abstract class termsDiscAmt : PX.Data.BQL.BqlDecimal.Field<termsDiscAmt> { }
		protected Decimal? _TermsDiscAmt = 0m;

		/// <summary>
		/// The discount amount of the prompt payment.
		/// </summary>
		[PXDecimal(4)]
		public virtual Decimal? TermsDiscAmt
		{
			get
			{
				return this._TermsDiscAmt;
			}
			set
			{
				this._TermsDiscAmt = value;
			}
		}
		#endregion
		#region ShipTermsID
		/// <inheritdoc cref="ShipTermsID"/>
		public abstract class shipTermsID : PX.Data.BQL.BqlString.Field<shipTermsID>
		{
			public class PreventEditIfSOExists : PreventEditOf<ShipTerms.freightAmountSource>.On<ShipTermsMaint>
				.IfExists<Select<SOOrder, Where<SOOrder.shipTermsID, Equal<Current<ShipTerms.shipTermsID>>>>>
			{
				protected override string CreateEditPreventingReason(GetEditPreventingReasonArgs arg, object so, string fld, string tbl, string foreignTbl)
				{
					return PXMessages.LocalizeFormat(Messages.ShipTermsUsedInSO, ((SOOrder)so).OrderType, ((SOOrder)so).OrderNbr);
				}
			}
		}
		protected String _ShipTermsID;

		/// <summary>
		/// The identifier of the <see cref="CS.ShipTerms">shipping terms</see> used for this customer.
		/// The field is included in the <see cref="FK.ShipTerms"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="CS.ShipTerms.shipTermsID"/> field.
		/// </value>
		[ActiveShipTermsSelector(typeof(ShipTerms.shipTermsID))]
		[PXDefault(typeof(Search<Location.cShipTermsID, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(FK.ShipTerms))]
		public virtual String ShipTermsID
		{
			get
			{
				return this._ShipTermsID;
			}
			set
			{
				this._ShipTermsID = value;
			}
		}
		#endregion
		#region ShipZoneID
		/// <inheritdoc cref="ShipZoneID"/>
		public abstract class shipZoneID : PX.Data.BQL.BqlString.Field<shipZoneID> { }
		protected String _ShipZoneID;

		/// <summary>
		/// The identifier of the <see cref="CS.ShippingZone">shipping zone</see> of the customer to be used to
		/// calculate freight.
		/// The field is included in the <see cref="FK.ShippingZone"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="CS.ShippingZone.zoneID"/> field.
		/// </value>
		[ActiveShippingZoneSelector(typeof(ShippingZone.zoneID), DescriptionField = typeof(ShippingZone.description), CacheGlobal = true)]
		[PXDefault(typeof(Search<Location.cShipZoneID, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(FK.ShippingZone))]
		public virtual String ShipZoneID
		{
			get
			{
				return this._ShipZoneID;
			}
			set
			{
				this._ShipZoneID = value;
			}
		}
		#endregion
		#region InclCustOpenOrders
		/// <inheritdoc cref="InclCustOpenOrders"/>
		public abstract class inclCustOpenOrders : PX.Data.BQL.BqlBool.Field<inclCustOpenOrders> { }
		protected Boolean? _InclCustOpenOrders;

		/// <summary>
		/// A Boolean value that indicates whether the sales order totals affect balances of the customer.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? InclCustOpenOrders
		{
			get
			{
				return this._InclCustOpenOrders;
			}
			set
			{
				this._InclCustOpenOrders = value;
			}
		}
		#endregion
		#region ShipmentCntr
		/// <inheritdoc cref="ShipmentCntr"/>
		public abstract class shipmentCntr : PX.Data.BQL.BqlInt.Field<shipmentCntr> { }
		protected Int32? _ShipmentCntr;

		/// <summary>
		/// The counter of <see cref="SOOrderShipment">order-shipment relations</see> added to the order.
		/// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? ShipmentCntr
		{
			get
			{
				return this._ShipmentCntr;
			}
			set
			{
				this._ShipmentCntr = value;
			}
		}
		#endregion
		#region OpenShipmentCntr
		/// <inheritdoc cref="OpenShipmentCntr"/>
		public abstract class openShipmentCntr : PX.Data.BQL.BqlInt.Field<openShipmentCntr> { }
		protected Int32? _OpenShipmentCntr;

		/// <summary>
		/// The counter of unconfirmed <see cref="SOShipment">shipments</see> added to the order.
		/// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? OpenShipmentCntr
		{
			get
			{
				return this._OpenShipmentCntr;
			}
			set
			{
				this._OpenShipmentCntr = value;
			}
		}
		#endregion
		#region OpenSiteCntr
		/// <inheritdoc cref="OpenSiteCntr"/>
		public abstract class openSiteCntr : PX.Data.BQL.BqlInt.Field<openSiteCntr> { }

		/// <summary>
		/// The number of warehouses without shipments of the order.
		/// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? OpenSiteCntr
		{
			get;
			set;
		}
		#endregion
		#region SiteCntr
		/// <inheritdoc cref="SiteCntr"/>
		public abstract class siteCntr : PX.Data.BQL.BqlInt.Field<siteCntr> { }

		/// <summary>
		/// The counter of warehouses for all shipments of the order.
		/// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? SiteCntr
		{
			get;
			set;
		}
		#endregion
		#region OpenLineCntr
		/// <inheritdoc cref="OpenLineCntr"/>
		public abstract class openLineCntr : PX.Data.BQL.BqlInt.Field<openLineCntr> { }
		protected Int32? _OpenLineCntr;

		/// <summary>
		///The counter of open lines added to the order.
		/// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? OpenLineCntr
		{
			get
			{
				return this._OpenLineCntr;
			}
			set
			{
				this._OpenLineCntr = value;
			}
		}
		#endregion
		#region DefaultSiteID
		/// <inheritdoc cref="DefaultSiteID"/>
		public abstract class defaultSiteID : PX.Data.BQL.BqlInt.Field<defaultSiteID> { }
		protected Int32? _DefaultSiteID;

		/// <summary>
		/// The identifier of the <see cref="INSite">warehouse</see> from which the goods should be shipped.
		/// The field is included in the <see cref="FK.DefaultSite"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="INSite.siteID"/> field.
		/// </value>
		/// <remarks>
		/// This field is available only if the <see cref="FeaturesSet.inventory">Inventory</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.
		/// </remarks>
		[IN.Site(DisplayName = "Preferred Warehouse ID", DescriptionField = typeof(INSite.descr))]
		[PXDefault(typeof(Search<Location.cSiteID, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<defaultSiteID>.IsRelatedTo<INSite.siteID>))]
		public virtual Int32? DefaultSiteID
		{
			get
			{
				return this._DefaultSiteID;
			}
			set
			{
				this._DefaultSiteID = value;
			}
		}
		#endregion
		#region DestinationSiteID
		/// <inheritdoc cref="DestinationSiteID"/>
		public abstract class destinationSiteID : PX.Data.BQL.BqlInt.Field<destinationSiteID> { }
		protected Int32? _DestinationSiteID;

		/// <summary>
		/// The identifier of the <see cref="INSite">destination warehouse</see> for the items to be transferred.
		/// The field is included in the foreign keys <see cref="FK.DestinationSite"/> and <see cref="FK.ToSite"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="INSite.siteID"/> field.
		/// </value>
		/// <remarks>
		/// This field is available only if the <see cref="FeaturesSet.warehouse">Multiple Warehouses</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.
		/// </remarks>
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[IN.ToSite(typeof(INTransferType.twoStep), typeof(SOOrder.branchID), DisplayName = "Destination Warehouse", DescriptionField = typeof(INSite.descr))]
		[PXUIRequired(typeof(isTransferOrder.IsEqual<True>))]
		[PXForeignReference(typeof(Field<destinationSiteID>.IsRelatedTo<INSite.siteID>))]
		public virtual Int32? DestinationSiteID
		{
			get
			{
				return this._DestinationSiteID;
			}
			set
			{
				this._DestinationSiteID = value;
			}
		}
		#endregion
        #region DestinationSiteIdErrorMessage
		/// <inheritdoc cref="DestinationSiteIdErrorMessage"/>
        public abstract class destinationSiteIdErrorMessage : PX.Data.BQL.BqlString.Field<destinationSiteIdErrorMessage> { }

		/// <summary>
		/// The error message that is raised because of the setting of the
		/// <see cref="shipAddressID">shipping address</see> or
		/// the <see cref="shipContactID">shipping contact</see> fields.
		/// </summary>
        [PXString(150, IsUnicode = true)]
        public virtual string DestinationSiteIdErrorMessage { get; set; }
        #endregion
		#region DefaultOperation
		/// <inheritdoc cref="DefaultOperation"/>
		public abstract class defaultOperation : PX.Data.BQL.BqlString.Field<defaultOperation> { }
		protected String _DefaultOperation;

		/// <summary>
		/// The default <see cref="SOOrderTypeOperation.operation">operation</see> (which can be an issue or receipt)
		/// of the <see cref="orderType">order type</see>.
		/// </summary>
		[PXDBString(SOOrderType.defaultOperation.Length, IsFixed = true)]
		[PXFormula(typeof(Selector<SOOrder.orderType, SOOrderType.defaultOperation>))]
		[PXSelectorMarker(typeof(Search<SOOrderTypeOperation.operation, Where<SOOrderTypeOperation.orderType, Equal<Current<orderType>>>>), CacheGlobal = true)]
		public virtual String DefaultOperation
		{
			get
			{
				return this._DefaultOperation;
			}
			set
			{
				this._DefaultOperation = value;
			}
		}
		#endregion
		#region ActiveOperationsCntr
		/// <inheritdoc cref="ActiveOperationsCntr"/>
		public abstract class activeOperationsCntr : BqlInt.Field<activeOperationsCntr> { }
		/// <summary>
		/// The number of active <see cref="SOOrderTypeOperation">operations</see> (which can be an issue or receipt)
		/// of the <see cref="orderType">order type</see>.
		/// </summary>
		[PXInt]
		[PXFormula(typeof(Selector<orderType, SOOrderType.activeOperationsCntr>))]
		public virtual int? ActiveOperationsCntr
		{
			get;
			set;
		}
		#endregion
		#region DefaultTranType
		/// <inheritdoc cref="DefaultTranType"/>
		public abstract class defaultTranType : Data.BQL.BqlString.Field<defaultTranType> { }

		/// <summary>
		/// The default transfer type.
		/// </summary>
		/// <value>
		/// The value is the <see cref="orderType">order type</see> of the order if the order type is
		/// <see cref="SOOrderType.iNDocType">Inventory Transaction Type</see>.
		/// </value>
		[PXString(SOOrderTypeOperation.iNDocType.Length, IsFixed = true)]
		[PXFormula(typeof(Selector<orderType, SOOrderType.iNDocType>))]
		public virtual string DefaultTranType
		{
			get;
			set;
		}
		#endregion
		#region OrigOrderType
		/// <inheritdoc cref="OrigOrderType"/>
		public abstract class origOrderType : PX.Data.BQL.BqlString.Field<origOrderType> { }
		protected String _OrigOrderType;

		/// <summary>
		/// The identifier of the <see cref="SOOrderType">type</see> of the original order.
		/// The field is included in the foreign keys <see cref="FK.OriginalOrderType"/> and
		/// <see cref="FK.OriginalOrder"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="SOOrderType.orderType"/> field.
		/// The value of this field corresponds to the value of the <see cref="SOOrder.orderType"/> field.
		/// </value>
		/// <remarks>
		/// The field is used only for returns.
		/// </remarks>
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName="Orig. Order Type", Enabled=false)]
		public virtual String OrigOrderType
		{
			get
			{
				return this._OrigOrderType;
			}
			set
			{
				this._OrigOrderType = value;
			}
		}
		#endregion
		#region OrigOrderNbr
		/// <inheritdoc cref="OrigOrderNbr"/>
		public abstract class origOrderNbr : PX.Data.BQL.BqlString.Field<origOrderNbr> { }
		protected String _OrigOrderNbr;

		/// <summary>
		/// The identifier of the <see cref="SOOrder">reference number</see> of the original sales order.
		/// The field is included in the <see cref="FK.OriginalOrder"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="SOOrder.orderNbr"/> field.
		/// </value>
		/// <remarks>
		/// The field is used only for returns.
		/// </remarks>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Orig. Order Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Current<SOOrder.origOrderType>>>>))]
		public virtual String OrigOrderNbr
		{
			get
			{
				return this._OrigOrderNbr;
			}
			set
			{
				this._OrigOrderNbr = value;
			}
		}
		#endregion
		#region ManDisc
		/// <inheritdoc cref="ManDisc"/>
		[Obsolete(Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R2)]
		public abstract class manDisc : PX.Data.BQL.BqlDecimal.Field<manDisc> { }
		protected Decimal? _ManDisc;

		/// <exclude/>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[Obsolete(Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R2)]
		public virtual Decimal? ManDisc
		{
			get
			{
				return this._ManDisc;
			}
			set
			{
				this._ManDisc = value;
			}
		}
		#endregion
		#region CuryManDisc
		/// <inheritdoc cref="CuryManDisc"/>
		[Obsolete(Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R2)]
		public abstract class curyManDisc : PX.Data.BQL.BqlDecimal.Field<curyManDisc> { }
		protected Decimal? _CuryManDisc;

		/// <exclude/>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.manDisc))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Manual Total")]
		[Obsolete(Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R2)]
		public virtual Decimal? CuryManDisc
		{
			get
			{
				return this._CuryManDisc;
			}
			set
			{
				this._CuryManDisc = value;
			}
		}
		#endregion
		#region ApprovedCredit
		/// <inheritdoc cref="ApprovedCredit"/>
		public abstract class approvedCredit : PX.Data.BQL.BqlBool.Field<approvedCredit> { }
		protected Boolean? _ApprovedCredit;

		/// <summary>
		/// A Boolean value that indicates whether a user approved an order that has a failed credit check.
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
		#region ApprovedCreditByPayment
		/// <inheritdoc cref="ApprovedCreditByPayment"/>
		public abstract class approvedCreditByPayment : PX.Data.BQL.BqlBool.Field<approvedCreditByPayment> { }

		/// <summary>
		/// A Boolean value that indicates whether a user approved an order that has a failed credit check by a payment.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual Boolean? ApprovedCreditByPayment
		{
			get;
			set;
		}
		#endregion
		#region ApprovedCreditAmt
		/// <inheritdoc cref="ApprovedCreditAmt"/>
		public abstract class approvedCreditAmt : PX.Data.BQL.BqlDecimal.Field<approvedCreditAmt> { }
		protected Decimal? _ApprovedCreditAmt;

		/// <summary>
		/// The amount of the order that a user approved on a failed credit check.
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
		#region DefaultSiteID_INSite_descr
		/// <summary>
		/// The <see cref="INSite.descr">description</see> of the <see cref="INSite">warehouse</see>
		/// from which the goods should be shipped.
		/// </summary>
		public abstract class defaultSiteID_INSite_descr : PX.Data.BQL.BqlString.Field<defaultSiteID_INSite_descr> { }
		#endregion
		#region ShipVia_Carrier_description
		/// <summary>
		/// The <see cref="CS.Carrier.description">description</see> of the <see cref="CS.Carrier">ship via code</see>
		/// that represents the carrier and its service to be used for shipping the ordered goods.
		/// </summary>
		public abstract class shipVia_Carrier_description : PX.Data.BQL.BqlString.Field<shipVia_Carrier_description> { }
		#endregion
        #region PaymentMethodID
		/// <inheritdoc cref="PaymentMethodID"/>
        public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
        protected String _PaymentMethodID;

		/// <summary>
		/// The identifier of the <see cref="CA.PaymentMethod">payment method</see> to be used to pay for the sales
		/// order. The field is included in the <see cref="FK.PaymentMethod"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="CA.PaymentMethod.paymentMethodID"/>
		/// field. By default, the value is the default payment method of the customer.
		/// </value>
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(typeof(Coalesce<Search2<CustomerPaymentMethod.paymentMethodID, InnerJoin<Customer, On<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>>>,
                                        Where<Customer.bAccountID, Equal<Current<SOOrder.customerID>>,
                                              And<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>>>>,
                                   Search<Customer.defPaymentMethodID,
                                         Where<Customer.bAccountID, Equal<Current<SOOrder.customerID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
                                Where<PaymentMethod.isActive, Equal<boolTrue>,
                                And<PaymentMethod.useForAR, Equal<boolTrue>>>>), DescriptionField = typeof(PaymentMethod.descr))]
        [PXUIFieldAttribute(DisplayName = "Payment Method")]
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
		/// <inheritdoc cref="PMInstanceID"/>
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }
		protected Int32? _PMInstanceID;

		/// <summary>
		/// The identifier of the <see cref="AR.CustomerPaymentMethod">default card or account number</see> for
		/// the payment method (for payment methods that require card or account numbers).
		/// The field is included in the <see cref="FK.CustomerPaymentMethod"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="AR.CustomerPaymentMethod.pMInstanceID"/> field.
		/// </value>
		/// <remarks>
		/// If the customer has more than one card or account number, a user can select one from the list of cards
		/// or accounts available for the customer.
		/// </remarks>
		[PXDBInt()]
		[PXDBChildIdentity(typeof(CustomerPaymentMethod.pMInstanceID))]
		[PXUIField(DisplayName = "Card/Account Nbr.")]
		[PXDefault(typeof(
			Coalesce<
				Search2<Customer.defPMInstanceID,
					InnerJoin<CustomerPaymentMethod,
						On<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>,
						And<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>>>>,
					Where<Customer.bAccountID, Equal<Current2<SOOrder.customerID>>,
						And<CustomerPaymentMethod.isActive, Equal<True>,
						And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<SOOrder.paymentMethodID>>>>>>,
				Search2<CustomerPaymentMethod.pMInstanceID,
					LeftJoin<CCProcessingCenterPmntMethodBranch,
						On<CustomerPaymentMethod.paymentMethodID, Equal<CCProcessingCenterPmntMethodBranch.paymentMethodID>,
						And<CustomerPaymentMethod.cCProcessingCenterID, Equal<CCProcessingCenterPmntMethodBranch.processingCenterID>,
						And<Current2<SOOrder.branchID>, Equal<CCProcessingCenterPmntMethodBranch.branchID>>>>>,
					Where<CustomerPaymentMethod.bAccountID, Equal<Current2<SOOrder.customerID>>,
						And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<SOOrder.paymentMethodID>>,
						And<CustomerPaymentMethod.isActive, Equal<True>>>>,
					OrderBy<Asc<Switch<Case<Where<CCProcessingCenterPmntMethodBranch.paymentMethodID, IsNull>, True>, False>,
						Desc<CustomerPaymentMethod.expirationDate,
						Desc<CustomerPaymentMethod.pMInstanceID>>>>>
			>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<CustomerPaymentMethod.pMInstanceID, Where<CustomerPaymentMethod.bAccountID, Equal<Current2<SOOrder.customerID>>,
			And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<SOOrder.paymentMethodID>>>>>), DescriptionField = typeof(CustomerPaymentMethod.descr))]
		[PXRestrictor(typeof(Where<CustomerPaymentMethod.isActive, Equal<boolTrue>, Or<CustomerPaymentMethod.pMInstanceID,
					Equal<Current<SOOrder.pMInstanceID>>>>), AR.Messages.InactiveCreditCardMayNotBeProcessed, typeof(CustomerPaymentMethod.descr))]
		[DeprecatedProcessing]
		[DisabledProcCenter]
		public virtual Int32? PMInstanceID
		{
			get
			{
				return this._PMInstanceID;
			}
			set
			{
				this._PMInstanceID = value;
			}
		}
		#endregion

		#region CashAccountID
		/// <inheritdoc cref="CashAccountID"/>
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		protected Int32? _CashAccountID;

		/// <summary>
		/// The identifier of the <see cref="Account">cash account</see> associated with the customer payment method.
		/// The field is included in the <see cref="FK.CashAccount"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.accountID"/> field.
		/// By default, the value is filled in with the cash account specified as the default for the selected method.
		/// </value>
		[PXDefault(typeof(Coalesce<Search2<CustomerPaymentMethod.cashAccountID,
									InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID,Equal<CustomerPaymentMethod.cashAccountID>,
										And<PaymentMethodAccount.paymentMethodID,Equal<CustomerPaymentMethod.paymentMethodID>,
										And<PaymentMethodAccount.useForAR, Equal<True>>>>>,
									Where<CustomerPaymentMethod.bAccountID, Equal<Current<SOOrder.customerID>>,
										And<CustomerPaymentMethod.pMInstanceID, Equal<Current2<SOOrder.pMInstanceID>>>>>,
								Search2<CashAccount.cashAccountID,
                                InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
                                    And<PaymentMethodAccount.useForAR, Equal<True>,
                                    And<PaymentMethodAccount.aRIsDefault, Equal<True>,
                                    And<PaymentMethodAccount.paymentMethodID, Equal<Current2<SOOrder.paymentMethodID>>>>>>>,
                                    Where<CashAccount.branchID,Equal<Current<SOOrder.branchID>>,
										And<Match<Current<AccessInfo.userName>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [CashAccount(typeof(SOOrder.branchID), typeof(Search2<CashAccount.cashAccountID,
                InnerJoin<PaymentMethodAccount,
                    On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
                        And<PaymentMethodAccount.useForAR,Equal<True>,
                        And<PaymentMethodAccount.paymentMethodID,
                        Equal<Current2<SOOrder.paymentMethodID>>>>>>,
                        Where<Match<Current<AccessInfo.userName>>>>), SuppressCurrencyValidation = false)]
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
		#region ExtRefNbr
		/// <inheritdoc cref="ExtRefNbr"/>
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		protected String _ExtRefNbr;

		/// <summary>
		/// The reference number of the payment.
		/// </summary>
		/// <remarks>
		/// This field is available only for sales orders of the Cash Sales or Cash Return type.
		/// </remarks>
		[PXDBString(40, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Payment Ref.", Enabled = false)]
		[SOOrderPaymentRef(
			typeof(cashAccountID),
			typeof(paymentMethodID),
			typeof(updateNextNumber))]
		public virtual String ExtRefNbr
		{
			get
			{
				return this._ExtRefNbr;
			}
			set
			{
				this._ExtRefNbr = value;
			}
		}
		#endregion
		#region UpdateNextNumber
		/// <inheritdoc cref="UpdateNextNumber"/>
		public abstract class updateNextNumber : PX.Data.BQL.BqlBool.Field<updateNextNumber> { }

		/// <summary>
		/// A part of the reference number of the payment.
		/// </summary>
		/// <remarks>
		/// The field is used by the <see cref="SOOrderPaymentRefAttribute">SOOrderPaymentRef</see> attribute.
		/// </remarks>
		[PXBool()]
		[PXUIField(DisplayName = "Update Next Number", Visibility = PXUIVisibility.Invisible)]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? UpdateNextNumber
		{
			get;
			set;
		}
		#endregion
		#region CuryUnreleasedPaymentAmt
		/// <inheritdoc cref="CuryUnreleasedPaymentAmt"/>
		public abstract class curyUnreleasedPaymentAmt : Data.BQL.BqlDecimal.Field<curyUnreleasedPaymentAmt> { }

		/// <summary>
		/// The <see cref="unreleasedPaymentAmt">sum of the payments</see> that have been applied to the sales order
		/// and that have not been released yet (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(unreleasedPaymentAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Not Released", Enabled = false)]
		public virtual decimal? CuryUnreleasedPaymentAmt
		{
			get;
			set;
		}
		#endregion
		#region UnreleasedPaymentAmt
		/// <inheritdoc cref="UnreleasedPaymentAmt"/>
		public abstract class unreleasedPaymentAmt : Data.BQL.BqlDecimal.Field<unreleasedPaymentAmt> { }

		/// <summary>
		/// The sum of the payments that have been applied to the sales order and that have not been released yet.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? UnreleasedPaymentAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryCCAuthorizedAmt
		/// <inheritdoc cref="CuryCCAuthorizedAmt"/>
		public abstract class curyCCAuthorizedAmt : Data.BQL.BqlDecimal.Field<curyCCAuthorizedAmt> { }

		/// <summary>
		/// The <see cref="cCAuthorizedAmt">sum</see> of all authorized credit card payments that have been applied
		/// to the sales order (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(cCAuthorizedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Authorized", Enabled = false)]
		public virtual decimal? CuryCCAuthorizedAmt
		{
			get;
			set;
		}
		#endregion
		#region CCAuthorizedAmt
		/// <inheritdoc cref="CCAuthorizedAmt"/>
		public abstract class cCAuthorizedAmt : Data.BQL.BqlDecimal.Field<cCAuthorizedAmt> { }

		/// <summary>
		/// The sum of all authorized credit card payments that have been applied to the sales order.
		/// </summary>
		/// <remarks>
		/// This field is available only if the
		/// <see cref="FeaturesSet.integratedCardProcessing">Integrated Card Processing</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.
		/// </remarks>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CCAuthorizedAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryPaidAmt
		/// <inheritdoc cref="CuryPaidAmt"/>
		public abstract class curyPaidAmt : Data.BQL.BqlDecimal.Field<curyPaidAmt> { }

		/// <summary>
		/// The <see cref="paidAmt">sum</see> of all released payments that have been applied to the sales order
		/// (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(paidAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Released", Enabled = false)]
		public virtual decimal? CuryPaidAmt
		{
			get;
			set;
		}
		#endregion
		#region PaidAmt
		/// <inheritdoc cref="PaidAmt"/>
		public abstract class paidAmt : Data.BQL.BqlDecimal.Field<paidAmt> { }

		/// <summary>
		/// The sum of all released payments that have been applied to the sales order.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? PaidAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryBilledPaymentTotal
		/// <inheritdoc cref="CuryBilledPaymentTotal"/>
		public abstract class curyBilledPaymentTotal : Data.BQL.BqlDecimal.Field<curyBilledPaymentTotal> { }

		/// <summary>
		/// The <see cref="billedPaymentTotal">sum of amounts</see> of the payments or prepayments that have been
		/// applied to the AR invoice generated for the order (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(billedPaymentTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Transferred to Invoices", Enabled = false)]
		public virtual decimal? CuryBilledPaymentTotal
		{
			get;
			set;
		}
		#endregion
		#region BilledPaymentTotal
		/// <inheritdoc cref="BilledPaymentTotal"/>
		public abstract class billedPaymentTotal : Data.BQL.BqlDecimal.Field<billedPaymentTotal> { }

		/// <summary>
		/// The sum of amounts of the payments or prepayments that have been applied to the AR invoice generated for
		/// the order.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BilledPaymentTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryTransferredToChildrenPaymentTotal
		/// <inheritdoc cref="CuryTransferredToChildrenPaymentTotal"/>
		public abstract class curyTransferredToChildrenPaymentTotal : Data.BQL.BqlDecimal.Field<curyTransferredToChildrenPaymentTotal> { }

		/// <summary>
		/// The <see cref="transferredToChildrenPaymentTotal">sum of amounts</see> of the payments or prepayments that
		/// have been applied to the child orders generated for the blanket order. (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(transferredToChildrenPaymentTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Transferred to Child Orders", Enabled = false, Visible = false)]
		public virtual decimal? CuryTransferredToChildrenPaymentTotal
		{
			get;
			set;
		}
		#endregion
		#region TransferredToChildrenPaymentTotal
		/// <inheritdoc cref="TransferredToChildrenPaymentTotal"/>
		public abstract class transferredToChildrenPaymentTotal : Data.BQL.BqlDecimal.Field<transferredToChildrenPaymentTotal> { }

		/// <summary>
		/// The sum of amounts of the payments or prepayments that have been applied to the child orders generated for
		/// the blanket order.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TransferredToChildrenPaymentTotal
		{
			get;
			set;
		}
		#endregion

		#region CuryUnpaidBalance
		/// <inheritdoc cref="CuryUnpaidBalance"/>
		public abstract class curyUnpaidBalance : PX.Data.BQL.BqlDecimal.Field<curyUnpaidBalance> { }

		/// <summary>
		/// The <see cref="unpaidBalance">unpaid amount</see> of the order (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.unpaidBalance))]
		[PXFormula(typeof(
			IIf<Where<behavior, Equal<SOBehavior.bL>, And<completed, Equal<True>>>, decimal0,
				Maximum<
					Sub<
						Switch<
							Case<Where<behavior, Equal<SOBehavior.bL>>, Sub<curyOrderTotal, curyTransferredToChildrenPaymentTotal>,
							Case<Where<Add<releasedCntr, billedCntr>, Equal<int0>>, curyOrderTotal>>, curyUnbilledOrderTotal>,
						curyPaymentTotal>,
					decimal0>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Unpaid Balance", Enabled = false)]
		public decimal? CuryUnpaidBalance
		{
			get;
			set;
		}
		#endregion
		#region UnpaidBalance
		/// <inheritdoc cref="UnpaidBalance"/>
		public abstract class unpaidBalance : PX.Data.BQL.BqlDecimal.Field<unpaidBalance> { }

		/// <summary>
		/// The unpaid amount of the order.
		/// </summary>
		/// <remarks>
		/// Once an invoice for the unpaid amount of the order is generated, the unpaid amount becomes 0.
		/// </remarks>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public decimal? UnpaidBalance
		{
			get;
			set;
		}
		#endregion
		#region CuryUnrefundedBalance
		public abstract class curyUnrefundedBalance : BqlDecimal.Field<curyUnrefundedBalance> { }
		[PXDBCurrency(typeof(curyInfoID), typeof(unrefundedBalance))]
		[PXFormula(typeof(
			Switch<Case<Where<behavior, Equal<SOBehavior.mO>, And<curyOrderTotal, Less<decimal0>>>,
				Maximum<
					Sub<Data.Minus<Switch<Case<Where<Add<releasedCntr, billedCntr>, Equal<int0>>, curyOrderTotal>, curyUnbilledOrderTotal>>,
						curyPaymentTotal>,
					decimal0>>,
				decimal0>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIVisible(typeof(Where<behavior.IsEqual<SOBehavior.mO>>))]
		[PXUIField(DisplayName = "Unrefunded Balance", Enabled = false)]
		public decimal? CuryUnrefundedBalance
		{
			get;
			set;
		}
		#endregion
		#region UnrefundedBalance
		public abstract class unrefundedBalance : BqlDecimal.Field<unrefundedBalance> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public decimal? UnrefundedBalance
		{
			get;
			set;
		}
		#endregion
		#region IsManualPackage
		/// <inheritdoc cref="IsManualPackage"/>
		public abstract class isManualPackage : PX.Data.BQL.BqlBool.Field<isManualPackage> { }
		protected bool? _IsManualPackage = false;

		/// <summary>
		/// A Boolean value that indicates whether the system prevents automatic recalculation of packages on any
		/// changes to the order or on automatic creation of shipments.
		/// </summary>
		/// <remarks>
		/// <para>If field value is <see langword="true"/>, the packages will not be automatically recalculated to
		/// further optimize the cost even if the order is included in a consolidated shipment.</para>
		/// <para>This field is available only if the
		/// <see cref="FeaturesSet.autoPackaging">Automatic Packaging</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.</para>
		/// </remarks>
		[PXDBBool()]
		[PXUIField(DisplayName = "Manual Packaging")]
		public virtual bool? IsManualPackage
		{
			get
			{
				return _IsManualPackage;
			}
			set
			{
				_IsManualPackage = value;
			}
		}
		#endregion
		#region IsTaxValid
		/// <inheritdoc cref="IsTaxValid"/>
		public abstract class isTaxValid : PX.Data.BQL.BqlBool.Field<isTaxValid> { }

		/// <summary>
		/// A Boolean value that indicates whether the system needs to update taxes after saving the order.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Tax Is Up to Date", Enabled = false)]
		public virtual Boolean? IsTaxValid
		{
			get; set;
		}
		#endregion
		#region IsOpenTaxValid
		/// <inheritdoc cref="IsOpenTaxValid"/>
		public abstract class isOpenTaxValid : PX.Data.BQL.BqlBool.Field<isOpenTaxValid> { }

		/// <summary>
		/// A Boolean value that indicates whether the system needs to update unshipped taxes after saving the order.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsOpenTaxValid
		{
			get;
			set;
		}
		#endregion
		#region IsUnbilledTaxValid
		/// <inheritdoc cref="IsUnbilledTaxValid"/>
		public abstract class isUnbilledTaxValid : PX.Data.BQL.BqlBool.Field<isUnbilledTaxValid> { }

		/// <summary>
		/// A Boolean value that indicates whether the system needs to update unbilled taxes after saving the order.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsUnbilledTaxValid
		{
			get;
			set;
		}
		#endregion
		#region ExternalTaxesImportInProgress
		/// <inheritdoc cref="ExternalTaxesImportInProgress"/>
		public abstract class externalTaxesImportInProgress : PX.Data.BQL.BqlBool.Field<externalTaxesImportInProgress> { }

		/// <summary>
		/// A Boolean value that indicates whether the taxes were calculated by an external system.
		/// </summary>
		[PXBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? ExternalTaxesImportInProgress
		{
			get;
			set;
		}
		#endregion
		#region IsTaxValid
		/// <inheritdoc cref="IsManualTaxesValid"/>
		public abstract class isManualTaxesValid : PX.Data.BQL.BqlBool.Field<isTaxValid> { }

		/// <summary>
		/// A Boolean value that specifies (if set to <see langword="false"/>)
		/// that <see cref="disableAutomaticTaxCalculation"/> is
		/// <see langword="true"/> and a user changed the order. In this case, a warning is displayed.
		/// </summary>
		[PXDBBool()]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Tax Is Up to Date", Enabled = false, Visible = false)]
		public virtual Boolean? IsManualTaxesValid
		{
			get; set;
		}
		#endregion
		#region OrderTaxAllocated
		public abstract class orderTaxAllocated : PX.Data.BQL.BqlBool.Field<orderTaxAllocated> { }
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true"/>) that order taxes are fully allocated to
		/// the invoice.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? OrderTaxAllocated
		{
			get;
			set;
		}
		#endregion


		#region IInvoice Members
		/// <inheritdoc cref="CuryDocBal"/>
		public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
		protected decimal? _CuryDocBal;

		/// <inheritdoc cref="IInvoice.CuryDocBal"/>
		[PXFormula(typeof(Switch<Case<
			Where<Add<releasedCntr, billedCntr>, Equal<int0>, And<behavior, NotEqual<SOBehavior.bL>>>,
				IIf<behavior.IsEqual<SOBehavior.mO>, Data.Abs<curyOrderTotal>, curyOrderTotal>>,
				IIf<behavior.IsEqual<SOBehavior.mO>, Data.Abs<curyUnbilledOrderTotal>, curyUnbilledOrderTotal>>))]
		[PXCurrency(typeof(curyInfoID), typeof(docBal))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public decimal? CuryDocBal
		{
			get
			{
				return this._CuryDocBal;
			}
			set
			{
				this._CuryDocBal = value;
			}
		}

		/// <inheritdoc cref="DocBal"/>
		public abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
		protected decimal? _DocBal;

		/// <inheritdoc cref="IInvoice.DocBal"/>
		[PXBaseCury]
		[PXFormula(typeof(Switch<Case<Where<Add<releasedCntr, billedCntr>, Equal<int0>>,
			IIf<behavior.IsEqual<SOBehavior.mO>, Data.Abs<orderTotal>, orderTotal>>,
			IIf<behavior.IsEqual<SOBehavior.mO>, Data.Abs<unbilledOrderTotal>, unbilledOrderTotal>>))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public decimal? DocBal
		{
			get
			{
				return this._DocBal;
			}
			set
			{
				this._DocBal = value; ;
			}
		}

		/// <inheritdoc cref="IInvoice.CuryDiscBal"/>
		public decimal? CuryDiscBal
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}

		/// <inheritdoc cref="IInvoice.DiscBal"/>
		public decimal? DiscBal
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}

		/// <inheritdoc cref="IInvoice.CuryWhTaxBal"/>
		public decimal? CuryWhTaxBal
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}

		/// <inheritdoc cref="IInvoice.WhTaxBal"/>
		public decimal? WhTaxBal
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}

		/// <inheritdoc cref="IInvoice.DocType"/>
		public string DocType
		{
			get { return null; }
			set { }
		}

		/// <inheritdoc cref="IInvoice.RefNbr"/>
		public string RefNbr
		{
			get { return null; }
			set { }
		}

		/// <inheritdoc cref="IInvoice.OrigModule"/>
		public string OrigModule
		{
			get { return null; }
			set {  }
		}

		/// <inheritdoc cref="IInvoice.CuryOrigDocAmt"/>
		public decimal? CuryOrigDocAmt
		{
			get { return null; }
			set { }
		}

		/// <inheritdoc cref="IInvoice.OrigDocAmt"/>
		public decimal? OrigDocAmt
		{
			get { return null; }
			set { }
		}

		/// <inheritdoc cref="IInvoice.DocDate"/>
		public DateTime? DocDate
		{
			get { return null; }
			set { }
		}

		/// <inheritdoc cref="IInvoice.DocDesc"/>
		public string DocDesc
		{
			get { return null; }
			set {  }
		}
		#endregion
		#region DisableAutomaticDiscountCalculation
		/// <inheritdoc cref="DisableAutomaticDiscountCalculation"/>
		public abstract class disableAutomaticDiscountCalculation : PX.Data.BQL.BqlBool.Field<disableAutomaticDiscountCalculation> { }
		protected Boolean? _DisableAutomaticDiscountCalculation;

		/// <summary>
		/// A Boolean value that indicates whether the system treats discounts that have already been applied to the
		/// selected sales order as manual.
		/// </summary>
		/// <value>
		/// <para>If the value is <see langword="false"/>) for the sales order, the system updates all automatic line,
		/// group, and document discounts when users run discount recalculation or add new lines to the order.</para>
		/// <para>The default state of this field is the same as the state of the
		/// <see cref="SOOrderType.disableAutomaticDiscountCalculation">Disable Automatic Discount Update</see>
		/// for the applicable order type.</para>
		/// </value>
		[PXDBBool]
		[PXDefault(false, typeof(Search<SOOrderType.disableAutomaticDiscountCalculation, Where<SOOrderType.orderType, Equal<Current<SOOrder.orderType>>>>))]
		[PXUIField(DisplayName = "Disable Automatic Discount Update")]
		public virtual Boolean? DisableAutomaticDiscountCalculation
		{
			get { return this._DisableAutomaticDiscountCalculation; }
			set { this._DisableAutomaticDiscountCalculation = value; }
		}
		#endregion
		#region DeferDiscountCalculation
		/// <inheritdoc cref="DeferPriceDiscountRecalculation"/>
		public abstract class deferPriceDiscountRecalculation : PX.Data.BQL.BqlBool.Field<deferPriceDiscountRecalculation> { }

		/// <summary>
		/// A Boolean value that indicates whether the system will update discounts,
		/// taxes and prices after saving the order, and the system will not update them before.
		/// </summary>
		/// <value>If the value is <see langword="false"/>,
		/// the system updates discounts, taxes, and prices while the order is editing.
		/// </value>
		/// <remarks>
		/// The field depends on <see cref="SOOrderType.deferPriceDiscountRecalculation"/>.
		/// </remarks>
		[PXBool]
		[PXUIField(DisplayName = "Defer Price/Discount Recalculation")]
		public virtual Boolean? DeferPriceDiscountRecalculation
		{
			get; set;
		}
		#endregion
		#region IsPriceAndDiscountsValid
		/// <inheritdoc cref="IsPriceAndDiscountsValid"/>
		public abstract class isPriceAndDiscountsValid : PX.Data.BQL.BqlBool.Field<disableAutomaticDiscountCalculation> { }

		/// <summary>
		/// A Boolean value that indicates whether the system needs to update prices and discounts after saving the
		/// order.
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Prices and discounts are up to date.", Enabled = false)]
		public virtual Boolean? IsPriceAndDiscountsValid
		{
			get; set;
		} = true;
		#endregion
		#region DisableAutomaticTaxCalculation
		/// <inheritdoc cref="DisableAutomaticTaxCalculation"/>
		public abstract class disableAutomaticTaxCalculation : PX.Data.BQL.BqlBool.Field<disableAutomaticTaxCalculation> { }

		/// <summary>
		/// A Boolean value that specifies (if set to <see langword="true"/>)
		/// that the system does not need to calculate taxes, because they are already calculated.
		/// </summary>
		[PXDBBool]
		[PXDefault(false, typeof(Search<SOOrderType.disableAutomaticTaxCalculation, Where<SOOrderType.orderType, Equal<Current<SOOrder.orderType>>>>))]
		[PXUIField(DisplayName = "Disable Automatic Tax Calculation")]
		public virtual Boolean? DisableAutomaticTaxCalculation
		{
			get;
			set;
		}
		#endregion
		#region OverridePrepayment
		/// <inheritdoc cref="OverridePrepayment"/>
		public abstract class overridePrepayment : PX.Data.BQL.BqlBool.Field<overridePrepayment> { }

		/// <summary>
		/// A Boolean value that specifies (if set to <see langword="true"/>) that fields
		/// <see cref="prepaymentReqPct">Prepayment Percent</see> and
		/// <see cref="curyPrepaymentReqAmt">Prepayment Amount</see> will have a higher priority than the predefined
		/// values that has been specified for the credit terms of the customer.
		/// </summary>
		/// <remarks>
		/// This field is available for sales orders with the <see cref="SOBehavior.sO">Sales Orders</see> automation
		/// behavior when the <see cref="Terms.prepaymentRequired">Prepayment Required</see> field of the customer is
		/// <see langword="true"/>.
		/// </remarks>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override Prepayment")]
		public virtual bool? OverridePrepayment
		{
			get;
			set;
		}
		#endregion
		#region PrepaymentReqPct
		/// <inheritdoc cref="PrepaymentReqPct"/>
		public abstract class prepaymentReqPct : PX.Data.BQL.BqlDecimal.Field<prepaymentReqPct> { }

		/// <summary>
		/// The percent of the total amount of this sales order that the customer has to make before the user can
		/// proceed to shipping the ordered items and preparing an invoice for the sales order.
		/// </summary>
		/// <remarks>
		/// <para>If the <see cref="curyPrepaymentReqAmt">Prepayment Amount</see> changed, than this amount is updated
		/// automatically, and if this value changed, the system automatically updates the
		/// <see cref="curyPrepaymentReqAmt">Prepayment Amount</see> to the appropriate percent based on the amount.
		/// </para>
		/// <para>This field is available when the <see cref="Terms.prepaymentRequired">Prepayment Required</see> field
		/// is <see langword="true"/> and becomes available only for sales orders with the
		/// <see cref="SOBehavior.sO">Sales Orders</see> automation behavior when the
		/// <see cref="overridePrepayment">Override Prepayment</see> field is <see langword="true"/>.</para>
		/// </remarks>
		[PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
		[PXFormula(typeof(
			IIf<Where<defaultTranType.IsNotEqual<INTranType.transfer>
				.And<termsID.IsNotNull>
				.And<allowsRequiredPrepayment.IsEqual<True>>>,
				IsNull<Selector<SOOrder.termsID, Terms.prepaymentPct>, decimal0>, decimal0>))]
		[PXUIField(DisplayName = "Prepayment Percent")]
		public virtual decimal? PrepaymentReqPct
		{
			get;
			set;
		}
		#endregion
		#region PrepaymentReqPctToRestore
		/// <inheritdoc cref="PrepaymentReqPctToRestore"/>
		public abstract class prepaymentReqPctToRestore : PX.Data.BQL.BqlDecimal.Field<prepaymentReqPctToRestore> { }

		/// <summary>
		/// The value that the <see cref="PrepaymentReqPct">Prepayment Percent</see> field had before
		/// the <see cref="PrepaymentReqPct">Prepayment Percent</see> was automatically reset to zero.
		/// </summary>
		/// <remarks>
		/// <para>If the <see cref="OrderTotal">Order Total</see> changed to a negative value, than
		/// this value is updated automatically, and if the <see cref="OrderTotal">Order Total</see>
		/// changed to a positive value, this field is used to restore <see cref="curyPrepaymentReqAmt">Prepayment Percent</see>.
		[PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
		public virtual decimal? PrepaymentReqPctToRestore
		{
			get;
			set;
		}
		#endregion
		#region CuryPrepaymentReqAmt
		/// <inheritdoc cref="CuryPrepaymentReqAmt"/>
		public abstract class curyPrepaymentReqAmt : PX.Data.BQL.BqlDecimal.Field<curyPrepaymentReqAmt> { }

		/// <summary>
		/// The <see cref="prepaymentReqAmt">amount</see> of funds (in the currency of the document)
		/// the customer has to pay before the user can proceed
		/// to shipping and preparing an invoice for the sales order.
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.prepaymentReqAmt))]
		[PXDefault]
		[PXUIField(DisplayName = "Prepayment Amount")]
		[PXFormula(typeof(
			prepaymentReqPct.Multiply<curyOrderTotal>.Divide<decimal100>.When<overridePrepayment.IsNotEqual<True>>.
			Else<curyPrepaymentReqAmt.FromCurrent>))]
		public virtual decimal? CuryPrepaymentReqAmt
		{
			get;
			set;
		}
		#endregion
		#region PrepaymentReqAmt
		/// <inheritdoc cref="PrepaymentReqAmt"/>
		public abstract class prepaymentReqAmt : PX.Data.BQL.BqlDecimal.Field<prepaymentReqAmt> { }

		/// <summary>
		/// The amount of funds the customer has to pay before the user can proceed to shipping and preparing
		/// an invoice for the sales order.
		/// </summary>
		/// <remarks>
		/// <para>If the <see cref="prepaymentReqPct">Prepayment Percent</see> changed, than this amount is updated
		/// automatically, and if this value changed, the system automatically updates the
		/// <see cref="prepaymentReqPct">Prepayment Percent</see> to the appropriate percent based on the amount.
		/// </para>
		/// <para>This field is available when the <see cref="Terms.prepaymentRequired">Prepayment Required</see> field
		/// is <see langword="true"/> and becomes available only for sales orders with the
		/// <see cref="SOBehavior.sO">Sales Orders</see> automation behavior when the
		/// <see cref="overridePrepayment">Override Prepayment</see> field is <see langword="true"/>.</para>
		/// </remarks>
		[PXDBBaseCury()]
		[PXDefault]
		public virtual decimal? PrepaymentReqAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryPaymentOverall
		/// <inheritdoc cref="CuryPaymentOverall"/>
		public abstract class curyPaymentOverall : PX.Data.BQL.BqlDecimal.Field<curyPaymentOverall> { }

		/// <summary>
		/// The <see cref="paymentOverall">sum</see> of amounts of child payments (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.paymentOverall))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryPaymentOverall
		{
			get;
			set;
		}
		#endregion
		#region PaymentOverall
		/// <inheritdoc cref="PaymentOverall"/>
		public abstract class paymentOverall : PX.Data.BQL.BqlDecimal.Field<paymentOverall> { }

		/// <summary>
		/// The sum of <see cref="SOAdjust.curyAdjdAmt"/> and <see cref="SOAdjust.curyAdjdBilledAmt"/>
		/// of all order's payments for which the following condition is <see langword="true"/>:
		/// <see cref="SOAdjust.voided"/> is <see langword="false"/>
		/// or <see cref="SOAdjust.isCCPayment"/> is <see langword="false"/> or <see cref="SOAdjust.isCCAuthorized"/>
		/// is <see langword="true"/> or <see cref="SOAdjust.isCCCaptured"/> is <see langword="true"/> or
		/// <see cref="SOAdjust.paymentReleased"/> is <see langword="true"/>.
		/// </summary>
		/// <remarks>
		/// The field is used to calculate <see cref="prepaymentReqSatisfied"/>.
		/// </remarks>
		[PXDBBaseCury()]
		[PXDefault]
		public virtual decimal? PaymentOverall
		{
			get;
			set;
		}
		#endregion
		#region PrepaymentReqSatisfied
		/// <inheritdoc cref="PrepaymentReqSatisfied"/>
		public abstract class prepaymentReqSatisfied : PX.Data.BQL.BqlBool.Field<prepaymentReqSatisfied> { }

		/// <summary>
		/// A Boolean value that specifies (if set to <see langword="true"/>) that the amount of the prepayment or
		/// repayments applied to the sales order is greater than or equal to the value specified in the
		/// <see cref="curyPrepaymentReqAmt">Prepayment Amount</see> field.
		/// </summary>
		/// <remarks>
		/// This field is not available only for sales orders with the <see cref="SOBehavior.sO">Sales Orders</see>
		/// automation behavior when the <see cref="Terms.prepaymentRequired">Prepayment Required</see> field is
		/// <see langword="true"/>.
		/// </remarks>
		[PXDBBool]
		[PXDefault(typeof(
			True.When<
				allowsRequiredPrepayment.IsEqual<False>.
				Or<curyPaymentOverall.IsGreaterEqual<curyPrepaymentReqAmt>.
				Or<curyUnpaidBalance.IsEqual<decimal0>>>>.
			Else<False>))]
		[PXUIField(DisplayName = "Prepayment Requirements Satisfied", Enabled = false)]
		public virtual bool? PrepaymentReqSatisfied
		{
			get;
			set;
		}
		#endregion
		#region ForceCompleteOrder
		/// <summary>
		/// A Boolean value that indicates whether the order is completed independently of the
		/// <see cref="openLineCntr"/>.
		/// </summary>
		/// <remarks>
		/// The field is used for drop-ship processing.
		/// </remarks>
		[PXBool, PXUnboundDefault(false)]
		public virtual Boolean? ForceCompleteOrder { get; set; }

		/// <inheritdoc cref="ForceCompleteOrder"/>
		public abstract class forceCompleteOrder : PX.Data.BQL.BqlBool.Field<forceCompleteOrder> { }
		#endregion
		#region PaymentsNeedValidationCntr
		/// <summary>
		/// The counter of payments added to the order for which the following condition is <see langword="true"/>:
		/// <see cref="SOAdjust.isCCPayment"/> is
		/// <see langword="true"/>, <see cref="SOAdjust.syncLock"/> is <see langword="true"/>,
		/// <see cref="SOAdjust.syncLockReason"/> is not equal to <see cref="ARPayment.syncLockReason.newCard"/>, and
		/// <see cref="SOAdjust.curyAdjdAmt"/> is not equal to 0.
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? PaymentsNeedValidationCntr
		{
			get;
			set;
		}

		/// <inheritdoc cref="PaymentsNeedValidationCntr"/>
		public abstract class paymentsNeedValidationCntr : PX.Data.BQL.BqlInt.Field<paymentsNeedValidationCntr> { }
		#endregion
		#region ArePaymentsApplicable
		/// <inheritdoc cref="ArePaymentsApplicable"/>
		public abstract class arePaymentsApplicable : Data.BQL.BqlBool.Field<arePaymentsApplicable> { }

		/// <summary>
		/// A Boolean value that indicates whether the order does not have the transfer behavior and has a related cash
		/// invoice.
		/// </summary>
		[PXBool]
		[PXUIField(Enabled = false, Visible = false)]
		[PXFormula(typeof(
			IIf<Where<defaultTranType, NotEqual<INTranType.transfer>,
				And<aRDocType, NotIn3<ARDocType.cashSale, ARDocType.cashReturn, ARDocType.cashSaleOrReturn>>>,
				True, False>))]
		public virtual bool? ArePaymentsApplicable
		{
			get;
			set;
		}
		#endregion
		#region IsFullyPaid
		/// <inheritdoc cref="IsFullyPaid"/>
		public abstract class isFullyPaid : Data.BQL.BqlBool.Field<isFullyPaid> { }

		/// <summary>
		/// A Boolean value that specifies (if set to <see langword="true"/>) that <see cref="curyPaymentOverall"/>
		/// is greater than 0 and <see cref="curyPaymentOverall"/> is greater than or equal to sum of
		/// <see cref="curyUnbilledOrderTotal"/> and <see cref="curyBilledPaymentTotal"/>.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(
			curyPaymentOverall.IsGreater<decimal0>
				.And<curyPaymentOverall.IsGreaterEqual<curyUnbilledOrderTotal.Add<curyBilledPaymentTotal>>>))]
		public virtual bool? IsFullyPaid
		{
			get;
			set;
		}
		#endregion
		#region AllowsRequiredPrepayment
		/// <inheritdoc cref="AllowsRequiredPrepayment"/>
		public abstract class allowsRequiredPrepayment : Data.BQL.BqlBool.Field<allowsRequiredPrepayment> { }

		/// <summary>
		/// If <see langword="true"/>, the Prepayment Required functionality works in the current Sales Order 
		/// </summary>
		[PXBool]
		[PXFormula(typeof(
			Brackets<behavior.IsEqual<SOBehavior.sO>.And<aRDocType.IsNotEqual<ARDocType.noUpdate>>>.
			Or<behavior.IsEqual<SOBehavior.rM>.And<aRDocType.IsIn<ARDocType.invoice, ARDocType.debitMemo>.
				And<defaultOperation.IsEqual<SOOperation.issue>>>>))]
		public bool? AllowsRequiredPrepayment
		{
			get;
			set;
		}
		#endregion

		#region IsIntercompany
		/// <inheritdoc cref="IsIntercompany"/>
		public abstract class isIntercompany : Data.BQL.BqlBool.Field<isIntercompany> { }

		/// <summary>
		/// A Boolean value that specifies (if set to <see langword="true"/>) that the following condition is
		/// <see langword="true"/>: <see cref="behavior"/> is one of <see cref="SOBehavior.sO"/>,
		/// <see cref="SOBehavior.iN"/>, <see cref="SOBehavior.rM"/>, or <see cref="SOBehavior.cM"/>,
		/// <see cref="CR.BAccount.isBranch"/> that is related to <see cref="customerID"/> is equal to
		/// <see langword="true"/>, and <see cref="defaultTranType"/> is not equal to <see cref="INTranType.transfer"/>.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Where<behavior, In3<SOBehavior.sO, SOBehavior.iN, SOBehavior.rM, SOBehavior.cM>,
			And<Selector<customerID, Customer.isBranch>, Equal<True>,
			And<defaultTranType, NotEqual<INTranType.transfer>>>>))]
		[PXDefault]
		public virtual bool? IsIntercompany
		{
			get;
			set;
		}
		#endregion
		#region IntercompanyPOType
		/// <inheritdoc cref="IntercompanyPOType"/>
		public abstract class intercompanyPOType : Data.BQL.BqlString.Field<intercompanyPOType>
		{
		}

		/// <summary>
		/// The identifier of the <see cref="PO.POOrder">type</see> of the purchase order for which the sales order
		/// has been created.
		/// The field is included in the <see cref="FK.IntercompanyPOOrder"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PO.POOrder.orderType"/> field.
		/// </value>
		/// <remarks>
		/// The field is visible for orders with the <see cref="SOBehavior.iN">Invoice</see> and
		/// <see cref="SOBehavior.sO">Sales Orders</see> automation behavior.
		/// </remarks>
		[PXDBString(2, IsFixed = true)]
		[PO.POOrderType.List()]
		[PXUIField(DisplayName = "Related Order Type", Enabled = false, FieldClass = nameof(FeaturesSet.InterBranch))]
		public virtual string IntercompanyPOType
		{
			get;
			set;
		}
		#endregion
		#region IntercompanyPONbr
		/// <inheritdoc cref="IntercompanyPONbr"/>
		public abstract class intercompanyPONbr : Data.BQL.BqlString.Field<intercompanyPONbr>
		{
		}

		/// <summary>
		/// The identifier of the <see cref="PO.POOrder">purchase order</see> for which the sales order has been
		/// created. The field is included in the <see cref="FK.IntercompanyPOOrder"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PO.POOrder.orderNbr"/> field.
		/// </value>
		/// <remarks>
		/// This field is available for orders with the <see cref="SOBehavior.iN">Invoice</see> and
		/// <see cref="SOBehavior.sO">Sales Orders</see> automation behavior.
		/// </remarks>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Related Order Nbr.", Enabled = false, FieldClass = nameof(FeaturesSet.InterBranch))]
		[PXSelector(typeof(Search<PO.POOrder.orderNbr, Where<PO.POOrder.orderType, Equal<Current<intercompanyPOType>>>>))]
		public virtual string IntercompanyPONbr
		{
			get;
			set;
		}
		#endregion
		#region IntercompanyPOReturnNbr
		/// <inheritdoc cref="IntercompanyPOReturnNbr"/>
		public abstract class intercompanyPOReturnNbr : Data.BQL.BqlString.Field<intercompanyPOReturnNbr>
		{
		}

		/// <summary>
		/// The link to the purchase return for which the return order has been created.
		/// </summary>
		/// <remarks>
		/// This field is available only for orders with the
		/// <see cref="SOBehavior.rM">RMA</see> automation behavior.
		/// </remarks>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Related PO Return Nbr.", Enabled = false, FieldClass = nameof(FeaturesSet.InterBranch))]
		[PXSelector(typeof(Search<PO.POReceipt.receiptNbr, Where<PO.POReceipt.receiptType, Equal<PO.POReceiptType.poreturn>>>))]
		public virtual string IntercompanyPOReturnNbr
		{
			get;
			set;
		}
		#endregion
		#region IntercompanyPOWithEmptyInventory
		/// <inheritdoc cref="IntercompanyPOWithEmptyInventory"/>
		public abstract class intercompanyPOWithEmptyInventory : Data.BQL.BqlBool.Field<intercompanyPOWithEmptyInventory> { }

		/// <summary>
		/// A Boolean value that specifies (if set to <see langword="true"/>)
		/// that <see cref="isIntercompany"/> is <see langword="true"/> and
		/// a child <see cref="PO.POLine"/> exists for which the following condition is <see langword="true"/>:
		/// <see cref="PO.POLine.inventoryID"/> is <see langword="null"/> and <see cref="PO.POLine.lineType"/>
		/// is not equal to <see cref="PO.POLineType.Description"/>.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IntercompanyPOWithEmptyInventory
		{
			get;
			set;
		}
		#endregion

		#region SuggestRelatedItems
		/// <summary>
		/// A Boolean value that indicates whether the related items can be suggested for the order.
		/// </summary>
		[PXBool]
		public virtual bool? SuggestRelatedItems { get; set; }

		/// <inheritdoc cref="SuggestRelatedItems"/>
		public abstract class suggestRelatedItems : Data.BQL.BqlBool.Field<suggestRelatedItems> { }
		#endregion

		#region IsCreditMemoOrder
		/// <inheritdoc cref="IsCreditMemoOrder"/>
		public abstract class isCreditMemoOrder : Data.BQL.BqlBool.Field<isCreditMemoOrder> { }

		/// <summary>
		/// A Boolean value that indicates whether the order has a related credit memo.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Where<aRDocType, In3<ARDocType.creditMemo, ARDocType.cashReturn>,
					And<defaultTranType, In3<INTranType.creditMemo, INTranType.return_, INTranType.noUpdate>>>))]
		public virtual bool? IsCreditMemoOrder
		{
			get;
			set;
		}
		#endregion
		#region IsRMAOrder
		/// <inheritdoc cref="IsRMAOrder"/>
		public abstract class isRMAOrder : Data.BQL.BqlBool.Field<isRMAOrder> { }

		/// <summary>
		/// A Boolean value that indicates whether the order has the RMA behavior.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Where<behavior, Equal<SOBehavior.rM>>))]
		public virtual bool? IsRMAOrder
		{
			get;
			set;
		}
		#endregion
		#region IsMixedOrder
		public abstract class isMixedOrder : Data.BQL.BqlBool.Field<isMixedOrder> { }
		[PXBool]
		[PXFormula(typeof(Where<behavior, Equal<SOBehavior.mO>>))]
		public virtual bool? IsMixedOrder
		{
			get;
			set;
		}
		#endregion
		#region IsTransferOrder
		/// <inheritdoc cref="IsTransferOrder"/>
		public abstract class isTransferOrder : Data.BQL.BqlBool.Field<isTransferOrder> { }

		/// <summary>
		/// A Boolean value that indicates whether the order has the transfer behavior.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Where<behavior, Equal<SOBehavior.tR>,
			Or<defaultTranType, Equal<INTranType.transfer>>>))]
		public virtual bool? IsTransferOrder
		{
			get;
			set;
		}
		#endregion
		#region IsDebitMemoOrder
		/// <inheritdoc cref="IsDebitMemoOrder"/>
		public abstract class isDebitMemoOrder : Data.BQL.BqlBool.Field<isDebitMemoOrder> { }

		/// <summary>
		/// A Boolean value that indicates whether the order has a related debit memo.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Where<aRDocType, Equal<ARDocType.debitMemo>,
			And<defaultTranType, Equal<INTranType.debitMemo>>>))]
		public virtual bool? IsDebitMemoOrder
		{
			get;
			set;
		}
		#endregion
		#region IsNoAROrder
		/// <inheritdoc cref="IsNoAROrder"/>
		public abstract class isNoAROrder : Data.BQL.BqlBool.Field<isNoAROrder> { }

		/// <summary>
		/// A Boolean value that indicates whether the order does not have an invoice.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Where<aRDocType, Equal<ARDocType.noUpdate>>))]
		public virtual bool? IsNoAROrder
		{
			get;
			set;
		}
		#endregion
		#region IsCashSaleOrder
		/// <inheritdoc cref="IsCashSaleOrder"/>
		public abstract class isCashSaleOrder : Data.BQL.BqlBool.Field<isCashSaleOrder> { }

		/// <summary>
		/// A Boolean value that indicates whether the order has a related cash invoice.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Where<aRDocType, In3<ARDocType.cashSale, ARDocType.cashReturn, ARDocType.cashSaleOrReturn>>))]
		public virtual bool? IsCashSaleOrder
		{
			get;
			set;
		}
		#endregion
		#region IsPaymentInfoEnabled
		/// <inheritdoc cref="IsPaymentInfoEnabled"/>
		public abstract class isPaymentInfoEnabled : Data.BQL.BqlBool.Field<isPaymentInfoEnabled> { }

		/// <summary>
		/// A Boolean value that indicates whether the order can be paid.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Where<isCashSaleOrder, Equal<True>,
			Or<Selector<orderType, SOOrderType.canHavePayments>, Equal<True>,
			Or<Selector<orderType, SOOrderType.canHaveRefunds>, Equal<True>>>>))]
		public virtual bool? IsPaymentInfoEnabled
		{
			get;
			set;
		}
		#endregion
		#region IsUserInvoiceNumbering
		/// <inheritdoc cref="IsUserInvoiceNumbering"/>
		public abstract class isUserInvoiceNumbering : Data.BQL.BqlBool.Field<isUserInvoiceNumbering> { }

		/// <summary>
		/// A Boolean value that indicates whether the order type is Manual Invoice Numbering.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Where<Selector<orderType, SOOrderType.userInvoiceNumbering>, Equal<True>>))]
		public virtual bool? IsUserInvoiceNumbering
		{
			get;
			set;
		}
		#endregion
		#region IsFreightAvailable
		/// <inheritdoc cref="IsFreightAvailable"/>
		public abstract class isFreightAvailable : BqlBool.Field<isFreightAvailable> { }

		/// <summary>
		/// A Boolean value that indicates whether the order does not have the transfer behavior.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Where<behavior, NotIn3<SOBehavior.tR, SOBehavior.bL>,
			And<defaultTranType, NotEqual<INTranType.transfer>>>))]
		public virtual bool? IsFreightAvailable
		{
			get;
			set;
		}
		#endregion
		#region IsLegacyMiscBilling
		/// <inheritdoc cref="IsLegacyMiscBilling"/>
		public abstract class isLegacyMiscBilling : BqlBool.Field<isLegacyMiscBilling> { }

		/// <summary>
		/// A boolean value that indicates (if set to <see langword="true" />) that separate invoicing
		/// is allowed for the lines where <see cref="SOLine.lineType"/> equals to <see cref="SOLineType.miscCharge"/>.
		/// </summary>
		[PXDefault(false)]
		[PXDBBool]
		public virtual bool? IsLegacyMiscBilling
		{
			get;
			set;
		}
		#endregion

		#region ExpireDate
		/// <inheritdoc cref="ExpireDate"/>
		public abstract class expireDate : BqlDateTime.Field<expireDate> { }

		/// <summary>
		/// The expiration date of a blanket sales order.
		/// </summary>
		/// <remarks>
		/// When the date in this field is earlier than the current business date, the system displays a warning message
		/// about the expiration of the order, and the order cannot be changed.
		/// </remarks>
		[PXDBDate]
		[PXUIField(DisplayName = "Expires On")]
		public virtual DateTime? ExpireDate
		{
			get;
			set;
		}
		#endregion
		#region IsExpired
		/// <inheritdoc cref="IsExpired"/>
		public abstract class isExpired : BqlBool.Field<isExpired> { }

		/// <summary>
		/// A Boolean value that indicates whether the order is expired.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsExpired
		{
			get;
			set;
		}
		#endregion
		#region MinSchedOrderDate
		/// <inheritdoc cref="MinSchedOrderDate"/>
		public abstract class minSchedOrderDate : BqlDateTime.Field<minSchedOrderDate> { }

		/// <summary>
		/// The date on which a child order should be generated for the line of the blanket sales order.
		/// </summary>
		/// <remarks>
		/// This field is available only for blanket sales orders.
		/// </remarks>
		[PXDBDate]
		[PXUIField(DisplayName = "Sched. Order Date", Enabled = false, Visible = false)]
		public virtual DateTime? MinSchedOrderDate
		{
			get;
			set;
		}
		#endregion
		#region QtyOnOrders
		/// <inheritdoc cref="QtyOnOrders"/>
		public abstract class qtyOnOrders : BqlDecimal.Field<qtyOnOrders> { }

		/// <summary>
		/// The quantity of the stock and non-stock items in a blanket sales order distributed among child orders.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. on Child Orders", Enabled = false)]
		public virtual decimal? QtyOnOrders
		{
			get;
			set;
		}
		#endregion
		#region BlanketOpenQty
		/// <inheritdoc cref="BlanketOpenQty"/>
		public abstract class blanketOpenQty : BqlDecimal.Field<blanketOpenQty> { }

		/// <summary>
		/// The summarized quantity of the stock or non-stock items in the blanket sales order that has not been
		/// transferred to child orders.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Open Quantity", Enabled = false)]
		public virtual decimal? BlanketOpenQty
		{
			get;
			set;
		}
		#endregion
		#region BlanketLineCntr
		/// <inheritdoc cref="BlanketLineCntr"/>
		public abstract class blanketLineCntr : BqlDecimal.Field<blanketLineCntr> { }

		/// <summary>
		/// The counter of the sales order detail lines added to the parent blanket order.
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? BlanketLineCntr
		{
			get;
			set;
		}
		#endregion
		#region BlanketSOAdjustCntr
		/// <inheritdoc cref="BlanketSOAdjustCntr"/>
		public abstract class blanketSOAdjustCntr : BqlDecimal.Field<blanketSOAdjustCntr> { }

		/// <summary>
		/// The counter of the <see cref="SOAdjust"></see> lines added to the current order and having blanket reference.
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? BlanketSOAdjustCntr
		{
			get;
			set;
		}
		#endregion

		#region SpecialLineCntr
		/// <inheritdoc cref="SpecialLineCntr"/>
		public abstract class specialLineCntr : BqlInt.Field<specialLineCntr> { }

		/// <summary>
		/// The counter of the special-ordered child lines. Corresponds to <see cref="SOLine.isSpecialOrder" />.
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? SpecialLineCntr
		{
			get;
			set;
		}
		#endregion

		#region Margin fields

		#region NoMarginLineCntr
		/// <summary>
		/// The number of detail lines with the empty <see cref="SOLine.MarginPct">margin</see>.
		/// Such lines are not considered in the <see cref="SOOrder.marginPct">order margin</see> calculation.
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? NoMarginLineCntr { get; set; }
		/// <inheritdoc cref="NoMarginLineCntr"/>
		public abstract class noMarginLineCntr : BqlInt.Field<noMarginLineCntr> { }
		#endregion

		#region CuryTaxableFreightAmt
		/// <summary>
		/// The sum of <see cref="SOOrder.CuryFreightAmt">freight amount</see> and <see cref="SOOrder.CuryPremiumFreightAmt">additional freight charges</see>
		/// that are subject to taxes (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(taxableFreightAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryTaxableFreightAmt { get; set; }
		/// <inheritdoc cref="CuryTaxableFreightAmt"/>
		public abstract class curyTaxableFreightAmt : BqlDecimal.Field<curyTaxableFreightAmt> { }
		#endregion
		#region TaxableFreightAmt
		/// <summary>
		/// The sum of <see cref="SOOrder.FreightAmt">freight amount</see> and <see cref="SOOrder.PremiumFreightAmt">additional freight charges</see>
		/// that are subject to taxes.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TaxableFreightAmt { get; set; }
		/// <inheritdoc cref="TaxableFreightAmt"/>
		public abstract class taxableFreightAmt : BqlDecimal.Field<taxableFreightAmt> { }
		#endregion

		#region CurySalesCostTotal
		/// <summary>
		/// The sum of the <see cref="SOLine.CuryExtCost">extended costs</see> of the detail lines for which the <see cref="SOLine.MarginPct">margin</see> is not empty
		/// (in the currency of the document).
		/// </summary>
		/// <remarks>
		/// The line's <see cref="SOLine.CuryExtCost">extended cost</see> is used with the negative sign for the receipt lines.
		/// </remarks>
		[PXDBCurrency(typeof(curyInfoID), typeof(salesCostTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CurySalesCostTotal { get; set; }
		/// <inheritdoc cref="CurySalesCostTotal"/>
		public abstract class curySalesCostTotal : BqlDecimal.Field<curySalesCostTotal> { }
		#endregion
		#region SalesCostTotal
		/// <summary>
		/// The sum of the <see cref="SOLine.ExtCost">extended costs</see> of the detail lines for which the <see cref="SOLine.MarginPct">margin</see> is not empty.
		/// </summary>
		/// <remarks>
		/// The line's <see cref="SOLine.ExtCost">extended cost</see> is used with the negative sign for the receipt lines.
		/// </remarks>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? SalesCostTotal { get; set; }
		/// <inheritdoc cref="SalesCostTotal"/>
		public abstract class salesCostTotal : BqlDecimal.Field<salesCostTotal> { }
		#endregion

		#region CuryNetSalesTotal
		/// <summary>
		/// The sum of the <see cref="SOLine.CuryNetSales">line amounts without the taxes and with the applied group and document discounts</see> (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(netSalesTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryNetSalesTotal { get; set; }
		/// <inheritdoc cref="CuryNetSalesTotal"/>
		public abstract class curyNetSalesTotal : BqlDecimal.Field<curyNetSalesTotal> { }
		#endregion
		#region NetSalesTotal
		/// <summary>
		/// The sum of the <see cref="SOLine.NetSales">line amounts without the taxes and with the applied group and document discounts</see>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? NetSalesTotal { get; set; }
		/// <inheritdoc cref="NetSalesTotal"/>
		public abstract class netSalesTotal : BqlDecimal.Field<netSalesTotal> { }
		#endregion

		#region CuryOrderNetSales
		/// <summary>
		/// The sum of the <see cref="SOOrder.CuryNetSalesTotal">line amounts without the taxes and with the applied group and document discounts</see>
		/// and the <see cref="SOOrder.CuryTaxableFreightAmt">taxable freight amount</see> (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(orderNetSales))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryOrderNetSales { get; set; }
		/// <inheritdoc cref="CuryOrderNetSales"/>
		public abstract class curyOrderNetSales : BqlDecimal.Field<curyOrderNetSales> { }
		#endregion
		#region OrderNetSales
		/// <summary>
		/// The sum of the <see cref="SOOrder.NetSalesTotal">line amounts without the taxes and with the applied group and document discounts</see>
		/// and the <see cref="SOOrder.TaxableFreightAmt">taxable freight amount</see>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? OrderNetSales { get; set; }
		/// <inheritdoc cref="OrderNetSales"/>
		public abstract class orderNetSales : BqlDecimal.Field<orderNetSales> { }
		#endregion

		#region CuryOrderCosts
		/// <summary>
		/// The sum of the <see cref="SOOrder.CurySalesCostTotal">extended costs of the detail lines</see>
		/// and the <see cref="SOOrder.CuryFreightCost">freight cost</see> (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(orderCosts))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryOrderCosts { get; set; }
		/// <inheritdoc cref="CuryOrderCosts"/>
		public abstract class curyOrderCosts : BqlDecimal.Field<curyOrderCosts> { }
		#endregion
		#region OrderCosts
		/// <summary>
		/// The sum of the <see cref="SOOrder.SalesCostTotal">extended costs of the detail lines</see>
		/// and the <see cref="SOOrder.FreightCost">freight cost</see> (in the currency of the document).
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? OrderCosts { get; set; }
		/// <inheritdoc cref="OrderCosts"/>
		public abstract class orderCosts : BqlDecimal.Field<orderCosts> { }
		#endregion

		#region CuryMarginAmt
		/// <summary>
		/// The order's estimated margin amount (in the currency of the document).
		/// </summary>
		/// <remarks>
		/// The value is not available for the transfer order.
		/// </remarks>
		[PXDBCurrency(typeof(curyInfoID), typeof(marginAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Est. Margin Amount")]
		public virtual decimal? CuryMarginAmt { get; set; }
		/// <inheritdoc cref="CuryMarginAmt"/>
		public abstract class curyMarginAmt : BqlDecimal.Field<curyMarginAmt> { }
		#endregion
		#region MarginAmt
		/// <summary>
		/// The order's estimated margin amount.
		/// </summary>
		/// <remarks>
		/// The value is not available for the transfer order.
		/// </remarks>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? MarginAmt { get; set; }
		/// <inheritdoc cref="MarginAmt"/>
		public abstract class marginAmt : BqlDecimal.Field<marginAmt> { }
		#endregion

		#region MarginPct
		/// <summary>
		/// The order's estimated margin percent.
		/// </summary>
		/// <remarks>
		/// The value is not available for the transfer order.
		/// </remarks>
		[PXDBDecimal(2)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Est. Margin (%)")]
		public virtual decimal? MarginPct { get; set; }
		/// <inheritdoc cref="MarginPct"/>
		public abstract class marginPct : BqlDecimal.Field<marginPct> { }
		#endregion

		#endregion

		#region TabVisibility
		#region ShowDiscountsTab
		/// <inheritdoc cref="ShowDiscountsTab"/>
		public abstract class showDiscountsTab : PX.Data.BQL.BqlBool.Field<showDiscountsTab> { }

		/// <summary>
		/// A Boolean value that indicates whether the Discounts tab is shown on the Sales Orders (SO301000) form.
		/// </summary>
		[PXBool()]
		[PXUIField(Visible = false)]
		[PXFormula(typeof(Switch<Case<Where<SOOrder.behavior, NotIn3<SOBehavior.bL, SOBehavior.tR>>, True>, False>))]
		public virtual Boolean? ShowDiscountsTab
		{
			get; set;
		}
		#endregion
		#region ShowShipmentsTab
		/// <inheritdoc cref="ShowShipmentsTab"/>
		public abstract class showShipmentsTab : PX.Data.BQL.BqlBool.Field<showShipmentsTab> { }

		/// <summary>
		/// A Boolean value that indicates whether the Shipments tab is shown on the Sales Orders (SO301000) form.
		/// </summary>
		[PXBool()]
		[PXUIField(Visible = false)]
		[PXFormula(typeof(Switch<Case<Where<SOOrder.behavior, NotEqual<SOBehavior.bL>>, True>, False>))]
		public virtual Boolean? ShowShipmentsTab
		{
			get; set;
		}
		#endregion
		#region ShowOrdersTab
		/// <inheritdoc cref="ShowOrdersTab"/>
		public abstract class showOrdersTab : PX.Data.BQL.BqlBool.Field<showOrdersTab> { }

		/// <summary>
		/// A Boolean value that indicates whether the Child Orders tab is shown on the Sales Orders (SO301000).
		/// </summary>
		[PXBool()]
		[PXUIField(Visible = false)]
		[PXFormula(typeof(Switch<Case<Where<SOOrder.behavior, Equal<SOBehavior.bL>>, True>, False>))]
		public virtual Boolean? ShowOrdersTab
		{
			get; set;
		}
		#endregion
		#region ChildLineCntr
		/// <inheritdoc cref="ChildLineCntr"/>
		public abstract class childLineCntr : Data.BQL.BqlInt.Field<childLineCntr> { }

		/// <summary>
		/// The counter of the sales order details added to child orders.
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? ChildLineCntr
		{
			get;
			set;
		}
		#endregion
		#endregion

		#region IAssign Members
		/// <inheritdoc cref="PX.Data.EP.IAssign.WorkgroupID"/>
		int? PX.Data.EP.IAssign.WorkgroupID { get; set; }
		/// <inheritdoc cref="PX.Data.EP.IAssign.OwnerID"/>
		int? PX.Data.EP.IAssign.OwnerID { get; set; }
		bool? IInvoice.Released { get => true; set { } }
		#endregion

		#region Order Orchestration

		#region IsOrchestrationAllowed
		/// <summary>
		/// A Boolean value that indicates whether orchestration is allowed for the current <see cref="SOOrderType">sales order type</see>.
		/// </summary>
		[PXUnboundDefault(false)]
		[PXBool]
		public bool? IsOrchestrationAllowed { get; set; }
		public abstract class isOrchestrationAllowed : PX.Data.BQL.BqlBool.Field<isOrchestrationAllowed> { }
		#endregion

		#region OrchestrationStatus
		/// <summary>
		/// The status of the order orchestration.
		/// </summary>
		[PXDBString(2, IsFixed = true)]
		[OrchestrationStatus.List]
		[PXDefault(Objects.SO.OrchestrationStatus.None)]
		[PXUIField(DisplayName = "Orchestration Status", Enabled = false, FieldClass = nameof(FeaturesSet.OrderOrchestration))]
		public string OrchestrationStatus { get; set; }
		public abstract class orchestrationStatus : PX.Data.BQL.BqlString.Field<orchestrationStatus> { }
		#endregion

		#region KeepOrchestrationStatus
		/// <exclude />
		[PXBool]
		[PXUnboundDefault(false)]
		public bool? KeepOrchestrationStatus { get; set; }
		public abstract class keepOrchestrationStatus : PX.Data.BQL.BqlBool.Field<keepOrchestrationStatus> { }
		#endregion

		#region OrchestrationStrategy
		/// <inheritdoc cref="SOOrderType.OrchestrationStrategy"/>
		[PXDBString(2, IsFixed = true)]
		[PXDefault(typeof(Switch<Case<Where<SOOrder.willCall, Equal<True>>, OrchestrationStrategies.doNotOrchestrate>,
										Selector<SOOrder.orderType, SOOrderType.orchestrationStrategy>>))]
		[PXUIField(DisplayName = "Fulfillment Strategy", FieldClass = nameof(FeaturesSet.OrderOrchestration))]
		[OrchestrationStrategies.List]
		[PXFormula(typeof(Default<SOOrder.willCall>))]
		public string OrchestrationStrategy { get; set; }
		public abstract class orchestrationStrategy : PX.Data.BQL.BqlString.Field<orchestrationStrategy> { }
		#endregion

		#region LimitWarehouse
		/// <inheritdoc cref="SOOrderType.LimitWarehouse"/>
		[PXDBBool]
		[PXUIField(DisplayName = "Limit Number of Fulfillment Warehouses", FieldClass = nameof(FeaturesSet.OrderOrchestration))]
		[PXDefault(typeof(Switch<Case<Where<orchestrationStrategy, Equal<OrchestrationStrategies.doNotOrchestrate>, Or<orchestrationStrategy, IsNull>>, False>,
								Switch<Case<Where<Selector<SOOrder.orderType, SOOrderType.orchestrationStrategy>, NotEqual<orchestrationStrategy>>, False>,
									  Selector<SOOrder.orderType, SOOrderType.limitWarehouse>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<orchestrationStrategy>))]
		public bool? LimitWarehouse { get; set; }
		public abstract class limitWarehouse : PX.Data.BQL.BqlBool.Field<limitWarehouse> { }
		#endregion

		#region NumberOfWarehouses
		/// <inheritdoc cref="SOOrderType.NumberOfWarehouses"/>
		[PXDefault(typeof(Switch<Case<Where<limitWarehouse, Equal<False>>, int0>,
								Switch<Case<Where<limitWarehouse, Equal<True>, And<Selector<SOOrder.orderType, SOOrderType.orchestrationStrategy>, NotEqual<orchestrationStrategy>>>, int1>,
											Selector<SOOrder.orderType, SOOrderType.numberOfWarehouses>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<limitWarehouse>))]
		[PXUIVerify(typeof(Where<limitWarehouse, NotEqual<True>, Or<numberOfWarehouses, GreaterEqual<int1>>>),
			TriggerPoints.FieldVerifying | TriggerPoints.RowPersisting,
			PXErrorLevel.Error, Messages.NumberOfWarehousesCannotBeLessThan1)]
		[PXDBInt]
		[PXUIField(DisplayName = "Number of Fulfillment Warehouses", FieldClass = nameof(FeaturesSet.OrderOrchestration))]
		public Int32? NumberOfWarehouses { get; set; }
		public abstract class numberOfWarehouses : PX.Data.BQL.BqlInt.Field<numberOfWarehouses> { }
		#endregion

		#endregion

		#region UnbilledMiscQty
		/// <inheritdoc cref="UnbilledMiscQty"/>
		public abstract class unbilledMiscQty : PX.Data.BQL.BqlDecimal.Field<unbilledMiscQty> { }
		protected Decimal? _UnbilledMiscQty;

		/// <summary>
		/// The sum of the <see cref="SOLine.unbilledQty">unbilled quantity</see> of miscellaneous lines.
		/// </summary>
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledMiscQty
		{
			get
			{
				return this._UnbilledMiscQty;
			}
			set
			{
				this._UnbilledMiscQty = value;
			}
		}
		#endregion
	}

	public class SOOrderTypeConstants
	{
		public const string SalesOrder = SOBehavior.SO;
		public const string TransferOrder = SOBehavior.TR;
		public const string RMAOrder = SOBehavior.RM;
		public const string QuoteOrder = SOBehavior.QT;
		public const string Invoice = SOBehavior.IN;
		public const string CreditMemo = SOBehavior.CM;
		public const string DebitMemo = "DM";
		public const string StandardOrder = "ST";

		public class salesOrder() : PX.Data.BQL.BqlString.Constant<salesOrder>(SalesOrder);
		public class transferOrder() : PX.Data.BQL.BqlString.Constant<transferOrder>(TransferOrder);
		public class rmaOrder() : PX.Data.BQL.BqlString.Constant<rmaOrder>(RMAOrder);
		public class quoteOrder() : PX.Data.BQL.BqlString.Constant<quoteOrder>(QuoteOrder);
		public class invoiceOrder() : PX.Data.BQL.BqlString.Constant<invoiceOrder>(Invoice);
		public class creditMemo() : PX.Data.BQL.BqlString.Constant<creditMemo>(CreditMemo);
	}

	public class SO
	{
		/// <summary>
		/// Specialized selector for SOOrder RefNbr.<br/>
		/// By default, defines the following set of columns for the selector:<br/>
		/// SOOrder.orderNbr,SOOrder.orderDate, SOOrder.customerID,<br/>
		/// SOOrder.customerID_Customer_acctName, SOOrder.customerLocationID,<br/>
		/// SOOrder.curyID, SOOrder.curyOrderTotal, SOOrder.status,SOOrder.invoiceNbr<br/>
		/// </summary>
		public class RefNbrAttribute : PXSelectorAttribute
		{
			public RefNbrAttribute(Type SearchType)
				: base(SearchType,
				typeof(SOOrder.orderNbr),
                typeof(SOOrder.customerOrderNbr),
                typeof(SOOrder.orderDate),
				typeof(SOOrder.customerID),
				typeof(SOOrder.customerID_Customer_acctName),
				typeof(SOOrder.customerLocationID),
				typeof(SOOrder.curyID),
				typeof(SOOrder.curyOrderTotal),
				typeof(SOOrder.status),
				typeof(SOOrder.invoiceNbr))
			{
			}
		}

		/// <summary>
		/// Specialized for SOOrder version of the <see cref="AutoNumberAttribute"/><br/>
		/// It defines how the new numbers are generated for the SO Order. <br/>
		/// References SOOrder.orderDate fields of the document,<br/>
		/// and also define a link between  numbering ID's defined in SO Order Type: namely SOOrderType.orderNumberingID. <br/>
		/// </summary>
		public class NumberingAttribute : AutoNumberAttribute
		{
            public NumberingAttribute()
                : base(typeof(Search<SOOrderType.orderNumberingID, Where<SOOrderType.orderType, Equal<Current<SOOrder.orderType>>, And<SOOrderType.active, Equal<True>>>>), typeof(SOOrder.orderDate))
            {; }
        }
    }

	public class SOOrderStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public static readonly (string, string)[] ValuesToLabels = new[]
			{
				(Open, Messages.Open),
				(Hold, Messages.Hold),
				(PendingApproval, EP.Messages.Balanced),
				(Voided, EP.Messages.Voided),
				(PendingProcessing, Messages.PendingProcessing),
				(AwaitingPayment, Messages.AwaitingPayment),
				(CreditHold, Messages.CreditHold),
				(Completed, Messages.Completed),
				(Cancelled, Messages.Cancelled),
				(BackOrder, Messages.BackOrder),
				(Shipping, Messages.Shipping),
				(Invoiced, Messages.Invoiced),
				(Expired, Messages.Expired),
			};

			public ListAttribute() : base(ValuesToLabels) { }
		}

		public class ListWithoutOrdersAttribute : PXStringListAttribute
		{
			public ListWithoutOrdersAttribute() : base(
				new[]
				{

					Pair(Open, Messages.Open),
					Pair(Hold, Messages.Hold),
					Pair(PendingApproval, EP.Messages.Balanced),
					Pair(Voided, EP.Messages.Voided),
					Pair(PendingProcessing, Messages.PendingProcessing),
					Pair(AwaitingPayment, Messages.AwaitingPayment),
					Pair(CreditHold, Messages.CreditHold),
					Pair(Completed, Messages.Completed),
					Pair(Cancelled, Messages.Cancelled),
					Pair(BackOrder, Messages.BackOrder),
					Pair(Shipping, Messages.Shipping),
					Pair(Invoiced, Messages.Invoiced),
				})
			{ }
		}

		public const string Initial = "_";
		public const string Open = "N";
		public const string Hold = "H";
		public const string PendingApproval = "P";
		public const string Voided = "V";
		public const string PendingProcessing = "E";
		public const string AwaitingPayment = "A";
		public const string CreditHold = "R";
		public const string Completed = "C";
		public const string Cancelled = "L";
		public const string BackOrder = "B";
		public const string Shipping = "S";
		public const string Invoiced = "I";
		public const string Expired = "D";

		public class voided() : PX.Data.BQL.BqlString.Constant<voided>(Voided);
		public class pendingApproval() : PX.Data.BQL.BqlString.Constant<pendingApproval>(PendingApproval);
		public class open() : PX.Data.BQL.BqlString.Constant<open>(Open);
		public class hold() : PX.Data.BQL.BqlString.Constant<hold>(Hold);
		public class pendingProcessing() : PX.Data.BQL.BqlString.Constant<pendingProcessing>(PendingProcessing);
		public class awaitingPayment() : PX.Data.BQL.BqlString.Constant<awaitingPayment>(AwaitingPayment);
		public class creditHold() : PX.Data.BQL.BqlString.Constant<creditHold>(CreditHold);
		public class completed() : PX.Data.BQL.BqlString.Constant<completed>(Completed);
		public class cancelled() : PX.Data.BQL.BqlString.Constant<cancelled>(Cancelled);
		public class backOrder() : PX.Data.BQL.BqlString.Constant<backOrder>(BackOrder);
		public class shipping() : PX.Data.BQL.BqlString.Constant<shipping>(Shipping);
		public class invoiced() : PX.Data.BQL.BqlString.Constant<invoiced>(Invoiced);
		public class expired() : PX.Data.BQL.BqlString.Constant<expired>(Expired);
	}

	public static class SOOrder_ExtensionMethods
	{
		public static void MarkOpen(this SOOrder order)
		{
			if (order != null)
			{
				order.Completed = false;
				order.Status = SOOrderStatus.Open;
			}
		}

		public static void MarkCompleted(this SOOrder order)
		{
			if (order != null)
			{
				order.Completed = true;
				order.Status = SOOrderStatus.Completed;
			}
		}

		public static void SatisfyPrepaymentRequirements(this SOOrder order, PXGraph graph)
		{
			if (order != null && order.Completed == false &&
				order.AllowsRequiredPrepayment == true)
			{
				if (order.PrepaymentReqSatisfied == false)
				{
					graph.LiteUpdate(order, _ =>
					{
						_.Set(o => o.PrepaymentReqSatisfied, true);
					});

					SOOrder.Events.Select(e => e.PaymentRequirementsSatisfied).FireOn(graph, order);

					var customer = Customer.PK.Find(graph, order.CustomerID);
					if (customer?.Status == CustomerStatus.CreditHold)
						SOOrder.Events.Select(e => e.CreditLimitViolated).FireOn(graph, order);
				}
			}
		}

		public static void ViolatePrepaymentRequirements(this SOOrder order, PXGraph graph)
		{
			if (order != null && order.Completed == false &&
				order.AllowsRequiredPrepayment == true)
			{
				if (order.PrepaymentReqSatisfied == true)
				{
					graph.LiteUpdate(order, _ =>
					{
						_.Set(o => o.PrepaymentReqSatisfied, false);
					});

					SOOrder.Events.Select(e => e.PaymentRequirementsViolated).FireOn(graph, order);
				}
			}
		}

		public static void SatisfyCreditLimitByPayment(this SOOrder order, PXGraph graph)
		{
			var orderType = SOOrderType.PK.Find(graph, order.OrderType);
			if (orderType?.RemoveCreditHoldByPayment == true)
			{
				graph.Caches[typeof(SOOrder)].RaiseExceptionHandling<SOOrder.status>(order, null, null);
				graph.LiteUpdate(order, _ => _.Set(o => o.ApprovedCreditByPayment, true));

				if (order.CreditHold == true)
					SOOrder.Events.Select(ev => ev.CreditLimitSatisfied).FireOn(graph, order);
			}

			var customer = Customer.PK.Find(graph, order.CustomerID);
			if (customer?.Status == CustomerStatus.CreditHold)
				SOOrder.Events.Select(e => e.CreditLimitViolated).FireOn(graph, order);
		}
	}
}
