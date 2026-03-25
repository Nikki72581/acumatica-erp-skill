using System;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.PM;
using PX.Objects.Common;
using PX.Objects.Common.Discount.Attributes;
using PX.Objects.Common.Discount;
using PX.Objects.Common.Bql;
using PX.Objects.IN.Matrix.Interfaces;
using PX.Objects.IN.RelatedItems;
using PX.Objects.IN.Attributes;
using PX.Objects.DR;
using PX.Objects.SO.DAC.Projections;
using PX.Objects.SO.GraphExtensions;
using PX.Objects.CS.Attributes;

namespace PX.Objects.SO
{
	/// <summary>
	/// Represents sales order details.
	/// </summary>
	/// <remarks>
	/// The records of this type are created and edited on the <i>Sales Orders (SO301000)</i> form (corresponds to the
	/// <see cref="SOOrderEntry"/> graph).
	/// </remarks>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.SOLine)]
	public partial class SOLine : PXBqlTable, PX.Data.IBqlTable, ILSPrimary, IHasMinGrossProfit, ISortOrder, IMatrixItemLine, ISubstitutableLine, IDemandLine, IAllocatableLine, ILineCost
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOLine>.By<orderType, orderNbr, lineNbr>
		{
			public static SOLine Find(PXGraph graph, string orderType, string orderNbr, int? lineNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, orderType, orderNbr, lineNbr, options);
		}
		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<SOLine>.By<branchID> { }
			public class Order : SOOrder.PK.ForeignKeyOf<SOLine>.By<orderType, orderNbr> { }
			public class OrderType : SOOrderType.PK.ForeignKeyOf<SOLine>.By<orderType> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<SOLine>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<SOLine>.By<subItemID> { }
			public class OriginalOrderType : SOOrderType.PK.ForeignKeyOf<SOLine>.By<origOrderType> { }
			public class OriginalOrder : SOOrder.PK.ForeignKeyOf<SOLine>.By<origOrderType, origOrderNbr> { }
			public class OriginalOrderLine : SOLine.PK.ForeignKeyOf<SOLine>.By<origOrderType, origOrderNbr, origLineNbr> { }
			public class POSite : INSite.PK.ForeignKeyOf<SOLine>.By<pOSiteID> { }
			public class OrderTypeOperation : SOOrderTypeOperation.PK.ForeignKeyOf<SOLine>.By<orderType, operation> { }
			public class Site : INSite.PK.ForeignKeyOf<SOLine>.By<siteID> { }
			public class SiteStatus : IN.INSiteStatus.PK.ForeignKeyOf<SOLine>.By<inventoryID, subItemID, siteID> { }
			public class SiteStatusByCostCenter : INSiteStatusByCostCenter.PK.ForeignKeyOf<SOLine>.By<inventoryID, subItemID, siteID, costCenterID> { }
			public class Location : INLocation.PK.ForeignKeyOf<SOLine>.By<locationID> { }
			public class LocationStatus : IN.INLocationStatus.PK.ForeignKeyOf<SOLine>.By<inventoryID, subItemID, siteID, locationID> { }
			public class LotSerialStatus : IN.INLotSerialStatus.PK.ForeignKeyOf<SOLine>.By<inventoryID, subItemID, siteID, locationID, lotSerialNbr> { }
			public class ReasonCode : CS.ReasonCode.PK.ForeignKeyOf<SOLine>.By<reasonCode> { }
			public class Customer : AR.Customer.PK.ForeignKeyOf<SOLine>.By<customerID> { }
			public class Invoice : SOInvoice.PK.ForeignKeyOf<SOLine>.By<invoiceType, invoiceNbr> { }
			public class InvoiceLine : ARTran.PK.ForeignKeyOf<SOLine>.By<invoiceType, invoiceNbr, invoiceLineNbr> { }
			public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<SOLine>.By<curyInfoID> { }
			public class TaxCategory : TX.TaxCategory.PK.ForeignKeyOf<SOLine>.By<taxCategoryID> { }
			public class Project : PMProject.PK.ForeignKeyOf<SOLine>.By<projectID> { }
			public class Task : PMTask.PK.ForeignKeyOf<SOLine>.By<projectID, taskID> { }
			public class SalesPerson : AR.SalesPerson.PK.ForeignKeyOf<SOLine>.By<salesPersonID> { }
			public class SalesAccount : GL.Account.PK.ForeignKeyOf<SOLine>.By<salesAcctID> { }
			public class SalesSubaccount : GL.Sub.PK.ForeignKeyOf<SOLine>.By<salesSubID> { }
			public class CostCode : PMCostCode.PK.ForeignKeyOf<SOLine>.By<costCodeID> { }
			public class Discount : ARDiscount.PK.ForeignKeyOf<SOLine>.By<discountID> { }
			public class DiscountSequence : AR.DiscountSequence.PK.ForeignKeyOf<SOLine>.By<discountID, discountSequenceID> { }
			public class Vendor : AP.Vendor.PK.ForeignKeyOf<SOLine>.By<vendorID> { }
			public class DefaultShedule : DRSchedule.PK.ForeignKeyOf<SOLine>.By<defScheduleID> { }
			public class BlanketOrder : BlanketSOOrder.PK.ForeignKeyOf<SOLine>.By<blanketType, blanketNbr> { }
			public class BlanketLine : BlanketSOLine.PK.ForeignKeyOf<SOLine>.By<blanketType, blanketNbr, blanketLineNbr> { }

			public class BlanketSplit : BlanketSOLineSplit.PK.ForeignKeyOf<SOLine>.By<blanketType, blanketNbr, blanketLineNbr, blanketSplitLineNbr> { }
			public class BlanketOrderLink : SOBlanketOrderLink.PK.ForeignKeyOf<SOLine>.By<blanketType, blanketNbr, orderType, orderNbr> { }
		}
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
		[Branch(typeof(SOOrder.branchID))]
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
		#region OrderType
		/// <inheritdoc cref="OrderType"/>
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;

		/// <summary>
		/// The type of the sales order in which this line item is listed.
		/// </summary>
		/// <remarks>
		/// The field is included in the following foreign keys:
		/// <list type="bullet">
		/// <item><see cref="FK.Order"/>. The field is a part of the identifier of the sales order
		/// <see cref="SOOrder"/>.<see cref="SOOrder.orderType"/></item>
		/// <item><see cref="FK.OrderType"/>. The field is a part of the identifier of the parent order type
		/// <see cref="SOOrderType"/>.<see cref="SOOrderType.orderType"/></item>
		/// <item><see cref="FK.OrderTypeOperation"/>. The field is a part of the identifier of the operation (issues,
		/// receipts) of a particular order types
		/// <see cref="SOOrderTypeOperation"/>.<see cref="SOOrderTypeOperation.orderType"/></item>
		/// <item><see cref="FK.BlanketOrderLink"/>. The field is a part of the identifier of the linked blanket sales
		/// order <see cref="SOBlanketOrderLink"/>.<see cref="SOBlanketOrderLink.orderType"/></item>
		/// </list>
		/// </remarks>
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault(typeof(SOOrder.orderType))]
		[PXUIField(DisplayName = "Order Type", Visible = false, Enabled = false)]
        [PXSelector(typeof(Search<SOOrderType.orderType>), CacheGlobal = true)]
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
		#region OrderNbr
		/// <inheritdoc cref="OrderNbr"/>
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;

		/// <summary>
		/// The reference number of the sales order in which this line item is listed.
		/// </summary>
		/// <remarks>
		/// The field is included in the following foreign keys:
		/// <list type="bullet">
		/// <item><see cref="FK.Order"/>. The field is a part of the identifier of the sales order
		/// <see cref="SOOrder"/>.<see cref="SOOrder.orderNbr"/></item>
		/// <item><see cref="FK.BlanketOrderLink"/>. The field is a part of the identifier of the linked blanket sales
		/// order <see cref="SOBlanketOrderLink"/>.<see cref="SOBlanketOrderLink.orderNbr"/></item>
		/// </list>
		/// </remarks>
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDBDefault(typeof(SOOrder.orderNbr), DefaultForUpdate = false, FieldUpdateMode = PXFieldUpdateMode.OnParentPersisted)]
		[PXParent(typeof(FK.Order))]
		[PXUIField(DisplayName = "Order Nbr.", Visible = false, Enabled = false)]
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
		#region LineNbr
		/// <inheritdoc cref="LineNbr"/>
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;

		/// <summary>
		/// The line number of the document.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(SOOrder.lineCntr))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region SortOrder
		/// <inheritdoc cref="SortOrder"/>
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
		protected Int32? _SortOrder;

		/// <summary>
		/// The order number of the document line.
		/// </summary>
		/// <remarks>
		/// The system regenerates this number automatically when lines are reordered.
		/// </remarks>
		[PXUIField(DisplayName = AP.APTran.sortOrder.DispalyName, Visible = false, Enabled = false)]
		[PXDBInt]
		[PXDefault()]
		public virtual Int32? SortOrder
		{
			get
			{
				return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
			}
		}
		#endregion
		#region Behavior
		/// <inheritdoc cref="Behavior"/>
		public abstract class behavior : PX.Data.BQL.BqlString.Field<behavior> { }
		protected String _Behavior;

		/// <summary>
		/// The behavior which is defined by the <see cref="OrderType"/> of the sales order in which this line item is
		/// listed.
		/// </summary>
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXDefault(typeof(Search<SOOrderType.behavior, Where<SOOrderType.orderType, Equal<Current<orderType>>>>))]
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
		#region DefaultOperation
		public abstract class defaultOperation : PX.Data.BQL.BqlString.Field<defaultOperation> { }
		[PXDBString(SOOrderType.defaultOperation.Length, IsFixed = true)]
		[PXDefault(typeof(SOOrderType.defaultOperation))]
		[PXSelectorMarker(typeof(Search<SOOrderTypeOperation.operation, Where<SOOrderTypeOperation.orderType, Equal<Current<orderType>>>>))]
		public virtual string DefaultOperation
		{
			get;
			set;
		}
		#endregion
		#region Operation
		/// <inheritdoc cref="Operation"/>
		public abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }
		protected String _Operation;

		/// <summary>
		/// The part of the identifier of the <see cref="SOOrderTypeOperation">operation</see> to be performed in
		/// inventory to fulfill the order.
		/// The field is included in the <see cref="FK.OrderTypeOperation"/> foreign key.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="SOOrderTypeOperation.operation"/> field.
		/// </value>
		/// <remarks>
		/// An order of the RR or RM type includes lines with the Receipt operation and lines with the
		/// <see cref="SOOperation.Issue">Issue</see> operation. Orders of other return types include only lines with
		/// the <see cref="SOOperation.Receipt">Receipt</see> operation.
		/// </remarks>
		[PXDBString(1, IsFixed = true, InputMask = ">a")]
		[PXUIField(DisplayName = "Operation", Visibility = PXUIVisibility.Dynamic, Enabled = false)]
		[PXDefault(typeof(SOOrderType.defaultOperation))]
		[SOOperation.List]
		[PXSelectorMarker(typeof(Search<SOOrderTypeOperation.operation, Where<SOOrderTypeOperation.orderType, Equal<Current<SOLine.orderType>>>>))]
		[PXFormula(typeof(
			Switch<Case<Where<orderQty, Less<decimal0>, And<defaultOperation, Equal<SOOperation.issue>,
				Or<orderQty, Equal<decimal0>, And<invoiceNbr, IsNotNull>>>>, SOOperation.receipt,
			Case<Where<orderQty, Less<decimal0>, And<defaultOperation, Equal<SOOperation.receipt>>>, SOOperation.issue>>,
			defaultOperation>))]
		public virtual String Operation
		{
			get
			{
				return this._Operation;
			}
			set
			{
				this._Operation = value;
			}
		}
		#endregion
		#region LineSign
		public abstract class lineSign : Data.BQL.BqlShort.Field<lineSign> { }
		[PXDBShort]
		[PXFormula(typeof(IIf<operation.IsEqual<defaultOperation>, short1, shortMinus1>))]
		[PXDefault]
		public virtual short? LineSign
		{
			get;
			set;
		}
		#endregion
		#region ShipComplete
		/// <inheritdoc cref="ShipComplete"/>
		public abstract class shipComplete : PX.Data.BQL.BqlString.Field<shipComplete> { }
		protected String _ShipComplete;

		/// <summary>
		/// The way the line item should be shipped.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in <see cref="SOShipComplete"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(SOOrder.shipComplete))]
		[SOShipComplete.List()]
		[PXUIField(DisplayName="Shipping Rule")]
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
		#region Completed
		/// <inheritdoc cref="Completed"/>
		public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }
		protected Boolean? _Completed;

		/// <summary>
		/// A Boolean value that indicates whether the line is completed.
		/// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Completed", Enabled = true)]
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
        #region OpenLine
        /// <inheritdoc cref="OpenLine"/>
        public abstract class openLine : PX.Data.BQL.BqlBool.Field<openLine> { }
        protected Boolean? _OpenLine;

        /// <summary>
        /// The identifier of the open detail line of the parent order.
        /// </summary>
        [PXDBBool()]
		[PXFormula(typeof(Switch<Case<Where2<Where<requireShipping, Equal<True>, And<lineType, NotEqual<SOLineType.miscCharge>, Or<behavior, Equal<SOBehavior.bL>>>>, And<completed, NotEqual<True>, And<orderQty, NotEqual<decimal0>>>>, True>, False>))]
		[DirtyFormula(typeof(Switch<Case<Where<openLine, Equal<True>, And<Where<isFree, NotEqual<True>, Or<manualDisc, Equal<True>>>>>, int1>, int0>),
			typeof(SumCalc<SOOrder.openLineCntr>), IsUnbound: true, SkipZeroUpdates = false, ValidateAggregateCalculation = true)]
		[PXFormula(null, typeof(CountCalc<BlanketSOLine.childLineCntr>))]
		[DirtyFormula(typeof(Switch<Case<Where<openLine, Equal<True>, And<Where<isFree, NotEqual<True>, Or<manualDisc, Equal<True>>>>>, int1>, int0>),
			typeof(SumCalc<BlanketSOLine.openChildLineCntr>), IsUnbound:true, SkipZeroUpdates = false)]
		[PXFormula(null, typeof(CountCalc<BlanketSOLineSplit.childLineCntr>))]
		[PXUnboundFormula(typeof(Switch<Case<Where<cancelled, Equal<False>>, int1>, int0>), typeof(SumCalc<BlanketSOLineSplit.effectiveChildLineCntr>))]
		[DirtyFormula(typeof(Switch<Case<Where<openLine, Equal<True>, And<Where<isFree, NotEqual<True>, Or<manualDisc, Equal<True>>>>>, int1>, int0>),
			typeof(SumCalc<BlanketSOLineSplit.openChildLineCntr>), IsUnbound:true, SkipZeroUpdates = false)]
        [PXUIField(DisplayName = "Open Line", Enabled = false)]
        public virtual Boolean? OpenLine
        {
            get
            {
                return this._OpenLine;
            }
            set
            {
                this._OpenLine = value;
            }
        }
        #endregion

		#region IsBeingCopied
		public abstract class isBeingCopied : PX.Data.BQL.BqlBool.Field<isBeingCopied> { }

		/// <summary>
		/// A Boolean value that indicates whether a <see cref="SOLine"/> was created by copying.
		/// This flag can be used to mark copied sales order lines if they need to be distinguished
		/// from the lines that are not created by copying.
		/// </summary>
		[PXBool()]
		public virtual Boolean? IsBeingCopied { get; set; }
		#endregion

		#region CustomerID
		/// <inheritdoc cref="CustomerID"/>
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;

		/// <summary>
		/// The identifier of the <see cref="AR.Customer">customer</see> of the original sales order.
		/// </summary>
		/// <remarks>
		/// The field is included in the <see cref="FK.Customer"/> foreign key. The field is a part of the identifier
		/// of the reason code <see cref="AR.Customer"/>.<see cref="AR.Customer.bAccountID"/>.
		/// </remarks>
		[PXDBInt()]
		[PXDefault(typeof(SOOrder.customerID))]
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
		public abstract class customerLocationID : Data.BQL.BqlInt.Field<customerLocationID> { }

		/// <summary>
		/// The customer location.
		/// </summary>
		/// <value>
		/// By default, the system copies to it the value of the <see cref="SOOrder.customerLocationID">Location</see>
		/// field.
		/// </value>
		/// <remarks>
		/// This field is available only for blanket sales orders. This field cannot be empty.
		/// </remarks>
		[LocationActive(typeof(Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>,
			And<MatchWithBranch<Location.cBranchID>>>), DescriptionField = typeof(Location.descr),
			DisplayName = "Ship-To Location")]
		[PXDefault(typeof(SOOrder.customerLocationID))]
		[PXForeignReference(
			typeof(CompositeKey<
				Field<customerID>.IsRelatedTo<Location.bAccountID>,
				Field<customerLocationID>.IsRelatedTo<Location.locationID>
			>))]
		public virtual int? CustomerLocationID
		{
			get;
			set;
		}
		#endregion
		#region OrderDate
		/// <inheritdoc cref="OrderDate"/>
		public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }
		protected DateTime? _OrderDate;

		/// <summary>
		/// The date of the sales order in which this line item is listed.
		/// </summary>
		[PXDBDate()]
		[PXDBDefault(typeof(SOOrder.orderDate))]
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
		#region CancelDate
		/// <inheritdoc cref="CancelDate"/>
		public abstract class cancelDate : PX.Data.BQL.BqlDateTime.Field<cancelDate> { }
		protected DateTime? _CancelDate;

		/// <summary>
		/// The expiration date of the order in which this line item is listed.
		/// </summary>
		[PXDBDate()]
		[PXDefault(typeof(SOOrder.cancelDate))]
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
		/// <value>
		/// The default value is specified in the <see cref="SOOrder.requestDate">Requested On</see> field of the order.
		/// </value>
		[PXDBDate()]
		[PXDefault(typeof(SOOrder.requestDate))]
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
		/// The date when the item should be shipped.
		/// </summary>
		/// <value>
		/// By default, this date is calculated as a date that is earlier than the
		/// <see cref="requestDate">Requested On date</see> by the number of lead days but not earlier than the
		/// <see cref="AccessInfo.businessDate">current business date</see>.
		/// </value>
		[PXDBDate()]
		[PXFormula(typeof(IIf<Where<SOLine.requestDate, Equal<Parent<SOOrder.requestDate>>>,
			Parent<SOOrder.shipDate>,
			DateMinusDaysNotLessThenDate<SOLine.requestDate,
				IsNull<Selector<Current<SOOrder.customerLocationID>, Location.cLeadTime>, decimal0>,
				Parent<SOOrder.orderDate>>>))]
		[PXUIField(DisplayName = "Ship On", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region InvoiceType
		/// <inheritdoc cref="InvoiceType"/>
		public abstract class invoiceType : PX.Data.BQL.BqlString.Field<invoiceType> { }

		/// <summary>
		/// Type of the Invoice to which the return SO line is applied.
		/// </summary>
		/// <remarks>
		/// The field is included in the following foreign keys:
		/// <list type="bullet">
		/// <item><see cref="FK.Invoice"/>. The field is a part of the identifier of the Invoice
		/// <see cref="SOInvoice"/>.<see cref="SOInvoice.docType"/></item>
		/// <item><see cref="FK.InvoiceLine"/>. The field is a part of the identifier of the Invoice Line
		/// <see cref="ARTran"/>.<see cref="ARTran.tranType"/></item>
		/// </list>
		/// </remarks>
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Invoice Type", Enabled = false, Visibility = PXUIVisibility.Dynamic)]
		[ARDocType.List()]
		public virtual string InvoiceType { get; set; }
		#endregion
		#region InvoiceNbr
		/// <inheritdoc cref="InvoiceNbr"/>
		public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
		protected String _InvoiceNbr;

		/// <summary>
		/// Number of the Invoice to which the return SO line is applied.
		/// </summary>
		/// <remarks>
		/// The field is included in the following foreign keys:
		/// <list type="bullet">
		/// <item><see cref="FK.Invoice"/>. The field is a part of the identifier of the Invoice
		/// <see cref="SOInvoice"/>.<see cref="SOInvoice.refNbr"/></item>
		/// <item><see cref="FK.InvoiceLine"/>. The field is a part of the identifier of the Invoice Line
		/// <see cref="ARTran"/>.<see cref="ARTran.refNbr"/></item>
		/// </list>
		/// </remarks>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Invoice Nbr.", Enabled = false, Visibility = PXUIVisibility.Dynamic)]
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
		#region InvoiceLineNbr
		/// <inheritdoc cref="InvoiceLineNbr"/>
		public abstract class invoiceLineNbr : PX.Data.BQL.BqlInt.Field<invoiceLineNbr> { }

		/// <summary>
		/// Number of the Invoice line to which the return SO line is applied.
		/// </summary>
		/// <remarks>
		/// The field is included in the <see cref="FK.InvoiceLine"/> foreign key. The field is a part of the
		/// identifier of the Invoice Line <see cref="ARTran"/>.<see cref="ARTran.lineNbr"/>.
		/// </remarks>
		[PXDBInt]
		[PXUIField(DisplayName = "Invoice Line Nbr.", Enabled = false, Visible = false)]
		public virtual int? InvoiceLineNbr
		{
			get;
			set;
		}
		#endregion
		#region InvoiceDate
		/// <inheritdoc cref="InvoiceDate"/>
		public abstract class invoiceDate : PX.Data.BQL.BqlDateTime.Field<invoiceDate> { }
		protected DateTime? _InvoiceDate;

		/// <summary>
		/// Date of the Invoice line to which the return SO line is applied.
		/// </summary>
		[PXDBDate()]
		[PXUIField(DisplayName = "Original Sale Date")]
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
		#region InvtMult
		/// <inheritdoc cref="InvtMult"/>
		public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
		protected Int16? _InvtMult;

		/// <summary>
		/// The <see cref="SOOrderTypeOperation.invtMult">Inventory Multiplier</see> of the
		/// <see cref="operation">operation</see> of the line.
		/// </summary>
		[PXDBShort()]
		[PXDefault()]
		[PXFormula(typeof(Selector<SOLine.defaultOperation, SOOrderTypeOperation.invtMult>))]
		[PXUIField(DisplayName = "Inventory Multiplier")]
		public virtual Int16? InvtMult
		{
			get
			{
				return this._InvtMult;
			}
			set
			{
				this._InvtMult = value;
			}
		}
		#endregion
		#region ManualPrice
		/// <inheritdoc cref="ManualPrice"/>
		public abstract class manualPrice : PX.Data.BQL.BqlBool.Field<manualPrice> { }
		protected Boolean? _ManualPrice;

		/// <summary>
		/// A Boolean value that indicates whether the unit price in this line has been corrected or specified manually. 
		/// </summary>
		/// <remarks>
		/// If the field value is <see langword="false"/> then the system updates the
		/// <see cref="unitPrice">unit price</see> in the document line with the current price (if one is specified).
		/// If the field value is <see langword="true"/> then <see cref="customerID">customer ID</see> is changed in
		/// the sales order or return order, the system does not update unit prices in the line.
		/// </remarks>
		[PXDBBool()]
		[PXDefault(false)]
        [PXUIField(DisplayName = "Manual Price")]
		public virtual Boolean? ManualPrice
		{
			get
			{
				return this._ManualPrice;
			}
			set
			{
				this._ManualPrice = value;
			}
		}
		#endregion
		#region IsStockItem
		/// <inheritdoc cref="IsStockItem"/>
		public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }

		/// <summary>
		/// A Boolean value that indicates whether the Inventory Item of the line is a stock item.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Is stock", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
		public virtual bool? IsStockItem
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		/// <inheritdoc cref="InventoryID"/>
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
			public class InventoryBaseUnitRule : 
				InventoryItem.baseUnit.PreventEditIfExists<
					Select<SOLine,
					Where<inventoryID, Equal<Current<InventoryItem.inventoryID>>,
						And<lineType, In3<SOLineType.inventory, SOLineType.nonInventory>,
						And<completed, NotEqual<True>>>>>>
			{ }
		}
		protected Int32? _InventoryID;

		/// <summary>
		/// The inventory ID of the Inventory Item to be sold or returned.
		/// </summary>
		/// <remarks>
		/// The field is included in the following foreign keys:
		/// <list type="bullet">
		/// <item><see cref="FK.InventoryItem"/>. The field is the identifier of the Stock Item or Non-Stock Item
		/// <see cref="InventoryItem"/>.<see cref="InventoryItem.inventoryID"/></item>
		/// <item><see cref="FK.SiteStatus"/>. The field is a part of the identifier of the warehouse container
		/// <see cref="INSiteStatus"/>.<see cref="INSiteStatus.inventoryID"/></item>
		/// <item><see cref="FK.LocationStatus"/>. The field is a part of the identifier of the Location inventory item
		/// status <see cref="INSiteStatus"/>.<see cref="INLocationStatus.inventoryID"/></item>
		/// <item><see cref="FK.LotSerialStatus"/>. The field is a part of the identifier of the Location inventory
		/// item status by Lot Serial numbers
		/// <see cref="INLotSerialStatus"/>.<see cref="INLotSerialStatus.inventoryID"/></item>
		/// </list>
		/// </remarks>
		[SOLineInventoryItem(Filterable=true)]
		[PXDefault()]
		[PXForeignReference(typeof(FK.InventoryItem))]
		[ConvertedInventoryItem(typeof(isStockItem))]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
		protected String _LineType;

		/// <summary>
		/// The type of the line.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in <see cref="SOLineType"/>.
		/// </value>
		[PXDBString(2, IsFixed = true)]
		[SOLineType.List()]
		[PXUIField(DisplayName = "Line Type", Visible = false, Enabled = false)]
		[PXFormula(typeof(Selector<SOLine.inventoryID, Switch<
			Case<Where<InventoryItem.stkItem, Equal<True>,
				Or<InventoryItem.kitItem, Equal<True>, And<InventoryItem.nonStockShip, Equal<True>>>>, SOLineType.inventory,
			Case<Where<InventoryItem.nonStockShip, Equal<True>>, SOLineType.nonInventory>>,
			SOLineType.miscCharge>>))]
		[PXFormula(null, typeof(CountCalc<SOOrderSite.lineCntr>), ValidateAggregateCalculation = true)]
		public virtual String LineType
		{
			get
			{
				return this._LineType;
			}
			set
			{
				this._LineType = value;
			}
		}
		#endregion
		#region IsKit
		/// <inheritdoc cref="IsKit"/>
		public abstract class isKit : PX.Data.BQL.BqlBool.Field<isKit> { }

		/// <summary>
		/// A Boolean value that indicates whether the item is a kit.
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Is a Kit", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
		[PXFormula(typeof(Selector<SOLine.inventoryID, InventoryItem.kitItem>))]
		public virtual Boolean? IsKit
		{
			get;
			set;
		}
		#endregion
		#region SubItemID
		/// <inheritdoc cref="SubItemID"/>
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;

		/// <summary>
		/// Represents a Subitem (or subitem code), which is used to indicate the particular size, color, or other
		/// variation of the inventory item.
		/// </summary>
		/// <remarks>
		/// The field is included in the following foreign keys:
		/// <list type="bullet">
		/// <item><see cref="FK.SubItem"/>. The field is the identifier of the Subitem
		/// <see cref="INSubItem"/>.<see cref="INSubItem.subItemID"/></item>
		/// <item><see cref="FK.SiteStatus"/>. The field is a part of the identifier of the warehouse container
		/// <see cref="INSiteStatus"/>.<see cref="INSiteStatus.subItemID"/></item>
		/// <item><see cref="FK.LocationStatus"/>. The field is a part of the identifier of the Location inventory item
		/// status <see cref="INSiteStatus"/>.<see cref="INLocationStatus.subItemID"/></item>
		/// <item><see cref="FK.LotSerialStatus"/>. The field is a part of the identifier of the Location inventory
		/// item status by Lot Serial numbers
		/// <see cref="INLotSerialStatus"/>.<see cref="INLotSerialStatus.subItemID"/></item>
		/// </list>
		/// </remarks>
		[PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
			Where<InventoryItem.inventoryID, Equal<Current<SOLine.inventoryID>>,
			And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<SOLine.inventoryID>))]
		[SubItem(typeof(SOLine.inventoryID))]
		[SubItemStatusVeryfier(typeof(SOLine.inventoryID), typeof(SOLine.siteID), InventoryItemStatus.Inactive, InventoryItemStatus.NoSales)]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region TranType
		/// <inheritdoc cref="TranType"/>
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		protected String _TranType;

		/// <summary>
		/// The <see cref="SOOrderTypeOperation.iNDocType">Inventory Transaction Type</see> of the
		/// <see cref="operation">operation</see> of the line.
		/// </summary>
		[PXFormula(typeof(Selector<SOLine.operation, SOOrderTypeOperation.iNDocType>))]
		[PXString(SOOrderTypeOperation.iNDocType.Length, IsFixed = true)]
		public virtual String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region TranDate
		/// <inheritdoc cref="ILSPrimary.TranDate"/>
		public virtual DateTime? TranDate
		{
			get { return this._OrderDate; }
		}
		#endregion
		#region PlanType
		/// <inheritdoc cref="PlanType"/>
		public abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }
		protected String _PlanType;

		/// <summary>
		/// The <see cref="SOOrderTypeOperation.orderPlanType">Order Plan Type</see> of the
		/// <see cref="operation">operation</see> of the line.
		/// </summary>
		[PXFormula(typeof(Selector<SOLine.operation, SOOrderTypeOperation.orderPlanType>))]
		[PXString(SOOrderTypeOperation.orderPlanType.Length, IsFixed = true)]
		public virtual String PlanType
		{
			get
			{
				return this._PlanType;
			}
			set
			{
				this._PlanType = value;
			}
		}
		#endregion
		#region OrigPlanType
		/// <inheritdoc cref="PlanType"/>
		[Obsolete(Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R2)]
		public abstract class origPlanType : PX.Data.BQL.BqlString.Field<origPlanType> { }

		[Obsolete(Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R2)]
		[PXDBString(2, IsFixed = true)]
		[PXSelector(typeof(Search<INPlanType.planType>), CacheGlobal = true)]
		public virtual String OrigPlanType
		{
			get;
			set;
		}
		#endregion   
		#region RequireReasonCode
		/// <inheritdoc cref="RequireReasonCode"/>
		public abstract class requireReasonCode : PX.Data.BQL.BqlBool.Field<requireReasonCode> { }
		protected Boolean? _RequireReasonCode;

		/// <summary>
		/// The <see cref="SOOrderTypeOperation.requireReasonCode">Require Reason Code</see> of the
		/// <see cref="operation">operation</see> of the line.
		/// </summary>
		[PXFormula(typeof(Selector<SOLine.operation, SOOrderTypeOperation.requireReasonCode>))]
		[PXBool]
		public virtual Boolean? RequireReasonCode
		{
			get
			{
				return this._RequireReasonCode;
			}
			set
			{
				this._RequireReasonCode = value;
			}
		}
		#endregion
		#region RequireShipping
		/// <inheritdoc cref="RequireShipping"/>
		public abstract class requireShipping : PX.Data.BQL.BqlBool.Field<requireShipping> { }
		protected bool? _RequireShipping;

		/// <summary>
		/// The <see cref="SOOrderType.requireShipping">Process Shipments</see> of the
		/// <see cref="orderType">order type</see> of the line.
		/// </summary>
        [PXBool()]
        [PXFormula(typeof(Selector<SOLine.orderType, SOOrderType.requireShipping>))]
		public virtual bool? RequireShipping
		{
			get
			{
				return this._RequireShipping;
			}
			set
			{
				this._RequireShipping = value;
			}
		}
		#endregion
		#region RequireAllocation
		/// <inheritdoc cref="RequireAllocation"/>
		public abstract class requireAllocation : PX.Data.BQL.BqlBool.Field<requireAllocation> { }
		protected bool? _RequireAllocation;

		/// <summary>
		/// The <see cref="SOOrderType.requireAllocation">Require Stock Allocation</see> of the
		/// <see cref="orderType">order type</see> of the line.
		/// </summary>
        [PXBool()]
        [PXFormula(typeof(Selector<SOLine.orderType, SOOrderType.requireAllocation>))]
		public virtual bool? RequireAllocation
		{
			get
			{
				return this._RequireAllocation;
			}
			set
			{
				this._RequireAllocation = value;
			}
		}
		#endregion
		#region RequireLocation
		/// <inheritdoc cref="RequireLocation"/>
		public abstract class requireLocation : PX.Data.BQL.BqlBool.Field<requireLocation> { }
		protected bool? _RequireLocation;

		/// <summary>
		/// The <see cref="SOOrderType.requireLocation">Require Location</see> of the
		/// <see cref="orderType">order type</see> of the line.
		/// </summary>
        [PXBool()]
        [PXFormula(typeof(Selector<SOLine.orderType, SOOrderType.requireLocation>))]
		public virtual bool? RequireLocation
		{
			get
			{
				return this._RequireLocation;
			}
			set
			{
				this._RequireLocation = value;
			}
		}
		#endregion
		#region LineQtyAvail
		/// <inheritdoc cref="LineQtyAvail"/>
		public abstract class lineQtyAvail : PX.Data.BQL.BqlDecimal.Field<lineQtyAvail> { }

		/// <summary>
		/// The <see cref="INSiteStatus.qtyAvail">Quantity Available</see> of the item of the line.
		/// </summary>
		[PXDecimal(6)]
		public decimal? LineQtyAvail
		{
			get;
			set;
		}
		#endregion
		#region LineQtyHardAvail
		/// <inheritdoc cref="LineQtyHardAvail"/>
		public abstract class lineQtyHardAvail : PX.Data.BQL.BqlDecimal.Field<lineQtyHardAvail> { }

		/// <summary>
		/// The <see cref="INSiteStatus.qtyHardAvail">Quantity Hard Available</see> of the item of the line.
		/// </summary>
		[PXDecimal(6)]
		public decimal? LineQtyHardAvail
		{
			get;
			set;
		}
		#endregion
		#region SiteID
		/// <inheritdoc cref="SiteID"/>
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;

		/// <summary>
		/// The identifier of the <see cref="INSite">warehouse</see> from which the specified quantity of the
		/// Inventory Item should be delivered.
		/// </summary>
		/// <remarks>
		/// The field is included in the following foreign keys:
		/// <list type="bullet">
		/// <item><see cref="FK.Site"/>. The field is identifier of the Warehouse
		/// <see cref="INSite"/>.<see cref="INSite.siteID"/></item>
		/// <item><see cref="FK.SiteStatus"/>. The field is a part of the identifier of the Warehouse inventory item
		/// status <see cref="INSiteStatus"/>.<see cref="INSiteStatus.siteID"/></item>
		/// <item><see cref="FK.LocationStatus"/>. The field is a part of the identifier of the Location inventory item
		/// status <see cref="INLocationStatus"/>.<see cref="INLocationStatus.siteID"/></item>
		/// <item><see cref="FK.LotSerialStatus"/>. The field is a part of the identifier of the Location inventory
		/// item status by Lot Serial numbers
		/// <see cref="INLotSerialStatus"/>.<see cref="INLotSerialStatus.siteID"/></item>
		/// </list>
		/// This field is available only if the <see cref="FeaturesSet.warehouse">Multiple Warehouses</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.
		/// </remarks>
		[SOSiteAvail(DocumentBranchType = typeof(SOOrder.branchID))]
		[PXParent(typeof(Select<SOOrderSite, Where<SOOrderSite.orderType, Equal<Current<SOLine.orderType>>, And<SOOrderSite.orderNbr, Equal<Current<SOLine.orderNbr>>, And<SOOrderSite.siteID, Equal<Current2<SOLine.siteID>>>>>>), LeaveChildren = true, ParentCreate = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIRequired(typeof(IIf<Where<SOLine.lineType, NotEqual<SOLineType.miscCharge>>, True, False>))]
		[InterBranchRestrictor(typeof(Where2<SameOrganizationBranch<INSite.branchID, Current<SOOrder.branchID>>,
			Or<Current<SOOrder.behavior>, Equal<SOBehavior.qT>>>))]
		[PXUnboundFormula(typeof(IIf<Where<openLine.IsEqual<True>>, int1, int0>), typeof(SumCalc<SOOrderSite.openLineCntr>), SkipZeroUpdates = false, ValidateAggregateCalculation = true)]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region LocationID
		/// <inheritdoc cref="LocationID"/>
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;

		/// <summary>
		/// The identifier of the location of the original sales order.
		/// </summary>
		/// <remarks>
		/// The field is included in the following foreign keys:
		/// <list type="bullet">
		/// <item><see cref="FK.Location"/>. The field is the identifier of the Location
		/// <see cref="INLocation"></see>.<see cref="INLocation.locationID"/></item>
		/// <item><see cref="FK.LocationStatus"/>. The field is a part of the identifier of the Location inventory item
		/// status <see cref="SOOrder"/>.<see cref="INLocationStatus.locationID"/></item>
		/// <item><see cref="FK.LotSerialStatus"/>. The field is a part of the identifier of the Location inventory
		/// item status by Lot Serial numbers
		/// <see cref="FK.LotSerialStatus"/>.<see cref="INLotSerialStatus.locationID"/></item>
		/// </list>
		/// </remarks>
		[SOLocationAvail(typeof(SOLine.inventoryID), typeof(SOLine.subItemID), typeof(SOLine.costCenterID), typeof(SOLine.siteID), typeof(SOLine.tranType), typeof(SOLine.invtMult))]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region LotSerialNbr
		/// <inheritdoc cref="LotSerialNbr"/>
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;

		/// <summary>
		/// The lot or serial number of the item for returns.
		/// </summary>
		/// <remarks>
		/// <para>The field is included in the <see cref="FK.LotSerialStatus"/> foreign key. The field is a part of the
		/// identifier of the Location inventory item status by Lot Serial numbers
		/// <see cref="SOLine"/>.<see cref="SOLine.locationID"/>.</para>
		/// <para>This field is available only if the
		/// <see cref="FeaturesSet.lotSerialTracking">Lot and Serial Tracking</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.</para>
		/// </remarks>
		[INLotSerialNbr(typeof(SOLine.inventoryID), typeof(SOLine.subItemID), typeof(SOLine.locationID), typeof(SOLine.costCenterID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region OrigOrderType
		/// <inheritdoc cref="OrigOrderType"/>
		public abstract class origOrderType : PX.Data.BQL.BqlString.Field<origOrderType> { }
		protected String _OrigOrderType;

		/// <summary>
		/// The identifier of the <see cref="SOOrderType">type</see> of the original order.
		/// </summary>
		/// <remarks>
		/// The field is included in the following foreign keys:
		/// <list type="bullet">
		/// <item><see cref="FK.OriginalOrderType"/>. The field is the identifier of the Order type
		/// <see cref="SOOrderType"></see>.<see cref="SOOrderType.orderType"/></item>
		/// <item><see cref="FK.OriginalOrder"/>. The field is a part of the identifier of the Sales order
		/// <see cref="SOOrder"/>.<see cref="SOOrder.orderType"/></item>
		/// <item><see cref="FK.OriginalOrderLine"/>. The field is a part of the identifier of the Sales order line
		/// <see cref="SOLine"/>.<see cref="SOLine.orderType"/></item>
		/// </list>
		/// </remarks>
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Orig. Order Type", Enabled = false)]
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
		/// </summary>
		/// <remarks>
		/// The field is included in the following foreign keys:
		/// <list type="bullet">
		/// <item><see cref="FK.OriginalOrder"/>. The field is a part of the identifier of the Sales order
		/// <see cref="SOOrder"/>.<see cref="SOOrder.orderNbr"/></item>
		/// <item><see cref="FK.OriginalOrderLine"/>. The field is a part of the identifier of the Sales order line
		/// <see cref="SOLine"/>.<see cref="SOLine.orderNbr"/></item>
		/// </list>
		/// </remarks>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Orig. Order Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Current<origOrderType>>>>), ValidateValue = false)]
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
		#region OrigLineNbr
		/// <inheritdoc cref="OrigLineNbr"/>
		public abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }
		protected Int32? _OrigLineNbr;

		/// <summary>
		/// The part of the identifier of the original <see cref="SOLine">line</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="SOLine.lineNbr"/> field.
		/// </value>
		/// <remarks>
		/// The field is included in the <see cref="FK.OriginalOrderLine"/> foreign key.
		/// </remarks>
		[PXDBInt()]
		public virtual Int32? OrigLineNbr
		{
			get
			{
				return this._OrigLineNbr;
			}
			set
			{
				this._OrigLineNbr = value;
			}
		}
		#endregion
		#region OrigShipmentType
		/// <inheritdoc cref="OrigShipmentType"/>
		public abstract class origShipmentType : Data.BQL.BqlString.Field<origShipmentType> { }

		/// <summary>
		/// The <see cref="ARTran.sOShipmentType">Shipment Type</see> of the original line of the Accounts Receivable
		/// invoice or memo.
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		public virtual string OrigShipmentType
		{
			get;
			set;
		}
		#endregion
		#region UOM
		/// <inheritdoc cref="UOM"/>
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;

		/// <summary>
		/// The unit of measure (UOM) used for the item with this <see cref="inventoryID">inventory ID</see>.
		/// </summary>
		[INUnit(typeof(SOLine.inventoryID), DisplayName="UOM")]     
		[PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<SOLine.inventoryID>>>>))]
		public virtual String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion
		#region InvoiceUOM
		/// <inheritdoc cref="InvoiceUOM"/>
		public abstract class invoiceUOM : PX.Data.BQL.BqlString.Field<invoiceUOM> { }

		/// <summary>
		/// The unit of measure (UOM) of the original line of the invoice.
		/// </summary>
		[PXDBString(6, IsUnicode = true, InputMask = ">aaaaaa")]
		public virtual string InvoiceUOM
		{
			get;
			set;
		}
		#endregion
		#region ClosedQty
		/// <inheritdoc cref="ClosedQty"/>
		public abstract class closedQty : PX.Data.BQL.BqlDecimal.Field<closedQty> { }
		protected Decimal? _ClosedQty;

		/// <summary>
		/// The closed quantity of the item, calculated as the subtraction of the
		/// <see cref="SOLine.openQty">quantity of the item to be shipped</see> from the
		/// <see cref="SOLine.orderQty">quantity of the item sold</see>.
		/// </summary>
		[PXDBCalced(typeof(Sub<SOLine.orderQty, SOLine.openQty>), typeof(decimal))]
		[PXQuantity(typeof(SOLine.uOM), typeof(SOLine.baseClosedQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ClosedQty
		{
			get
			{
				return this._ClosedQty;
			}
			set
			{
				this._ClosedQty = value;
			}
		}
		#endregion
		#region BaseClosedQty
		/// <inheritdoc cref="BaseClosedQty"/>
		public abstract class baseClosedQty : PX.Data.BQL.BqlDecimal.Field<baseClosedQty> { }
		protected Decimal? _BaseClosedQty;

		/// <summary>
		/// The <see cref="closedQty">closed quantity</see> of the item, expressed in the base unit of measure.
		/// </summary>
		[PXDBCalced(typeof(Sub<SOLine.baseOrderQty, SOLine.baseOpenQty>), typeof(decimal))]
		[PXQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseClosedQty
		{
			get
			{
				return this._BaseClosedQty;
			}
			set
			{
				this._BaseClosedQty = value;
			}
		}
		#endregion
		#region OrderQty
		/// <inheritdoc cref="OrderQty"/>
		public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
		protected Decimal? _OrderQty;

		/// <summary>
		/// The quantity of the item sold in the <see cref="uOM">unit of measure</see>.
		/// </summary>
		[PXDBQuantity(typeof(SOLine.uOM), typeof(SOLine.baseOrderQty), InventoryUnitType.SalesUnit)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]
		[PXUnboundFormula(typeof(Switch<Case<Where<operation, Equal<defaultOperation>, And<lineType, NotEqual<SOLineType.miscCharge>>>,
			orderQty>,
			decimal0>),
			typeof(SumCalc<SOOrder.orderQty>), ValidateAggregateCalculation = true)]
		[PXUnboundFormula(typeof(Switch<Case<Where<SOLine.lineType, NotEqual<SOLineType.miscCharge>>,
																				 SOLine.orderQty>,
																				 decimal0>),
			typeof(SumCalc<SOBlanketOrderLink.orderedQty>))]
		[PXUnboundFormula(typeof(Switch<Case<Where<cancelled, Equal<False>>, orderQty>, decimal0>), typeof(SumCalc<BlanketSOLine.qtyOnOrders>))]
		[PXUnboundFormula(typeof(Switch<Case<Where<cancelled, Equal<False>>, orderQty>, decimal0>), typeof(SumCalc<BlanketSOLineSplit.qtyOnOrders>))]
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

		/// <inheritdoc cref="OrderQty"/>
		public virtual Decimal? Qty
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
		#region BaseOrderQty
		/// <inheritdoc cref="BaseOrderQty"/>
		public abstract class baseOrderQty : PX.Data.BQL.BqlDecimal.Field<baseOrderQty> { }
		protected Decimal? _BaseOrderQty;

		/// <summary>
		/// The quantity of the item sold, expressed in the base unit of measure.
		/// </summary>
		/// <remarks>
		/// This quantity is used for calculating discounts if the
		/// <see cref="ARSetup.applyQuantityDiscountBy">Base UOM</see> field is <see langword="true"/>.
		/// </remarks>
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Base Order Qty.", Visible = false, Enabled = false)]
		public virtual Decimal? BaseOrderQty
		{
			get
			{
				return this._BaseOrderQty;
			}
			set
			{
				this._BaseOrderQty = value;
			}
		}

		/// <inheritdoc cref="BaseOrderQty"/>
		public virtual Decimal? BaseQty
		{
			get
			{
				return this._BaseOrderQty;
			}
			set
			{
				this._BaseOrderQty = value;
			}
		}
		#endregion
		#region VerifyOrderQty
		public abstract class verifyOrderQty : PX.Data.BQL.BqlDecimal.Field<verifyOrderQty> { }
		/// <summary>
		/// This will store the initial Order Qty of the SO to the verification process while updating the SOLine/SOLineSplits
		/// </summary>
		[PXDBCalced(typeof(SOLine.orderQty), typeof(decimal))]
		[PXDecimal()]
		public virtual Decimal? VerifyOrderQty
		{
			get;
			set;
		}
		#endregion
		#region UnassignedQty
		/// <inheritdoc cref="UnassignedQty"/>
		public abstract class unassignedQty : PX.Data.BQL.BqlDecimal.Field<unassignedQty> { }
		protected Decimal? _UnassignedQty;

		/// <summary>
		/// Contains the difference between the line quantity and the quantity on child split lines, for Inventory
		/// Items with lot or serial number. 
		/// </summary>
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnassignedQty
		{
			get
			{
				return this._UnassignedQty;
			}
			set
			{
				this._UnassignedQty = value;
			}
		}
		#endregion
		#region ShippedQty
		/// <inheritdoc cref="ShippedQty"/>
		public abstract class shippedQty : PX.Data.BQL.BqlDecimal.Field<shippedQty> { }
		protected Decimal? _ShippedQty;

		/// <summary>
		/// The quantity of the stock item being prepared for shipment and already shipped for this order.
		/// </summary>
		[PXDBQuantity(typeof(SOLine.uOM), typeof(SOLine.baseShippedQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. On Shipments", Enabled = false)]
		public virtual Decimal? ShippedQty
		{
			get
			{
				return this._ShippedQty;
			}
			set
			{
				this._ShippedQty = value;
			}
		}
		#endregion
		#region BaseShippedQty
		/// <inheritdoc cref="ShippedQty"/>
		public abstract class baseShippedQty : PX.Data.BQL.BqlDecimal.Field<baseShippedQty> { }
		protected Decimal? _BaseShippedQty;

		/// <summary>
		/// The <see cref="shippedQty">quantity</see> of the stock item being prepared for shipment and already shipped
		/// for this order, expressed in the base unit of measure.
		/// </summary>
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseShippedQty
		{
			get
			{
				return this._BaseShippedQty;
			}
			set
			{
				this._BaseShippedQty = value;
			}
		}
		#endregion
		#region OpenQty
		/// <inheritdoc cref="OpenQty"/>
		public abstract class openQty : PX.Data.BQL.BqlDecimal.Field<openQty> { }
		protected Decimal? _OpenQty;

		/// <summary>
		/// The quantity of the item to be shipped.
		/// </summary>
		/// <remarks>
		/// That is, the total quantity minus the quantity shipped according to closed shipment documents.
		/// </remarks>
		[PXDBQuantity(typeof(SOLine.uOM), typeof(SOLine.baseOpenQty))]
		[PXFormula(typeof(Switch<
			Case<Where2<Where<requireShipping, Equal<True>, And<lineType, NotEqual<SOLineType.miscCharge>, Or<behavior, Equal<SOBehavior.bL>>>>, And<completed, NotEqual<True>>>,
				Switch<Case<Where<lineSign, Equal<shortMinus1>>, Minimum<Sub<orderQty, closedQty>, decimal0>>, Maximum<Sub<orderQty, closedQty>, decimal0>>>,
				decimal0>))]
		[PXUnboundFormula(typeof(Switch<Case<Where<lineType, NotEqual<SOLineType.miscCharge>>, openQty.Multiply<lineSign>>, decimal0>),
			typeof(SumCalc<SOOrder.openOrderQty>), ValidateAggregateCalculation = true)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Open Qty.", Enabled = false)]
		public virtual Decimal? OpenQty
		{
			get
			{
				return this._OpenQty;
			}
			set
			{
				this._OpenQty = value;
			}
		}
		#endregion
		#region BaseOpenQty
		/// <inheritdoc cref="BaseOpenQty"/>
		public abstract class baseOpenQty : PX.Data.BQL.BqlDecimal.Field<baseOpenQty> { }

		protected Decimal? _BaseOpenQty;
		/// <summary>
		/// The <see cref="openQty">quantity of the item to be shipped</see>, expressed in the base unit of measure.
		/// </summary>
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Base Open Qty.")]
		public virtual Decimal? BaseOpenQty
		{
			get
			{
				return this._BaseOpenQty;
			}
			set
			{
				this._BaseOpenQty = value;
			}
		}
		#endregion
		#region UnshippedQty
		/// <inheritdoc cref="UnshippedQty"/>
		public abstract class unshippedQty : Data.BQL.BqlDecimal.Field<unshippedQty> { }

		/// <summary>
		/// The quantity of the blanket sales order line that have not been shipped yet in child orders.
		/// </summary>
		/// <remarks>
		/// This field is available only for blanket sales orders.
		/// </remarks>
		[PXQuantity]
		[PXFormula(typeof(Switch<Case<Where<lineType, NotEqual<SOLineType.miscCharge>>, openQty>, decimal0>))]
		[PXUIField(DisplayName = "Unshipped Qty.", Enabled = false, FieldClass = FeaturesSet.inventory.FieldClass)]
		public virtual decimal? UnshippedQty
		{
			get;
			set;
		}
		#endregion
		#region BilledQty
		/// <inheritdoc cref="BilledQty"/>
		public abstract class billedQty : PX.Data.BQL.BqlDecimal.Field<billedQty> { }
		protected Decimal? _BilledQty;

		/// <summary>
		/// The quantity of stock and non-stock items that were billed.
		/// </summary>
		[PXDBQuantity(typeof(SOLine.uOM), typeof(SOLine.baseBilledQty), MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Billed Quantity", Enabled = false)]
		public virtual Decimal? BilledQty
		{
			get
			{
				return this._BilledQty;
			}
			set
			{
				this._BilledQty = value;
			}
		}
		#endregion
		#region BaseBilledQty
		/// <inheritdoc cref="BaseBilledQty"/>
		public abstract class baseBilledQty : PX.Data.BQL.BqlDecimal.Field<baseBilledQty> { }
		protected Decimal? _BaseBilledQty;

		/// <summary>
		/// The <see cref="billedQty">quantity</see> of stock and non-stock items that were billed, expressed in the
		/// base unit of measure.
		/// </summary>
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseBilledQty
		{
			get
			{
				return this._BaseBilledQty;
			}
			set
			{
				this._BaseBilledQty = value;
			}
		}
		#endregion
		#region UnbilledQty
		/// <inheritdoc cref="UnbilledQty"/>
		public abstract class unbilledQty : PX.Data.BQL.BqlDecimal.Field<unbilledQty> { }
		protected Decimal? _UnbilledQty;

		/// <summary>
		/// The quantity of stock and non-stock items that were not yet billed.
		/// </summary>
		[PXDBQuantityWithOrigQty(typeof(uOM), typeof(baseUnbilledQty), typeof(shippedQty), typeof(baseShippedQty))]
		[PXFormula(typeof(Switch<Case<Where<requireShipping, Equal<True>, And<lineType, NotEqual<SOLineType.miscCharge>, And<completed, Equal<True>>>>, Sub<shippedQty, billedQty>>, Sub<orderQty, billedQty>>))]
		[PXUnboundFormula(typeof(unbilledQty.Multiply<lineSign>), typeof(SumCalc<SOOrder.unbilledOrderQty>), ValidateAggregateCalculation = true)]
		[PXUnboundFormula(typeof(IIf<lineType.IsEqual<SOLineType.miscCharge>, unbilledQty.Multiply<lineSign>, decimal0>), typeof(SumCalc<SOOrder.unbilledMiscQty>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Quantity", Enabled = false)]
		public virtual Decimal? UnbilledQty
		{
			get
			{
				return this._UnbilledQty;
			}
			set
			{
				this._UnbilledQty = value;
			}
		}
		#endregion
		#region BaseUnbilledQty
		/// <inheritdoc cref="BaseUnbilledQty"/>
		public abstract class baseUnbilledQty : PX.Data.BQL.BqlDecimal.Field<baseUnbilledQty> { }
		protected Decimal? _BaseUnbilledQty;

		/// <summary>
		/// The <see cref="unbilledQty">quantity</see> of stock and non-stock items that were not yet billed, expressed
		/// in the base unit of measure.
		/// </summary>
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseUnbilledQty
		{
			get
			{
				return this._BaseUnbilledQty;
			}
			set
			{
				this._BaseUnbilledQty = value;
			}
		}
		#endregion
		#region CompleteQtyMin
		/// <inheritdoc cref="CompleteQtyMin"/>
		public abstract class completeQtyMin : PX.Data.BQL.BqlDecimal.Field<completeQtyMin> { }
		protected Decimal? _CompleteQtyMin;

		/// <summary>
		/// The minimum percentage of goods shipped (with respect to the ordered quantity) for the system to mark the
		/// order as completely shipped.
		/// </summary>
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 100.0)]
		[PXDefault(TypeCode.Decimal, "100.0",
			typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<SOLine.inventoryID>>>>),
			SourceField = typeof(InventoryItem.undershipThreshold), CacheGlobal = true)]
		[PXUIField(DisplayName = "Undership Threshold (%)")]
		public virtual Decimal? CompleteQtyMin
		{
			get
			{
				return this._CompleteQtyMin;
			}
			set
			{
				this._CompleteQtyMin = value;
			}
		}
		#endregion
		#region CompleteQtyMax
		/// <inheritdoc cref="CompleteQtyMax"/>
		public abstract class completeQtyMax : PX.Data.BQL.BqlDecimal.Field<completeQtyMax> { }
		protected Decimal? _CompleteQtyMax;

		/// <summary>
		/// The maximum percentage of goods shipped (with respect to the ordered quantity) allowed by the customer.
		/// </summary>
		[PXDBDecimal(2, MinValue = 100.0, MaxValue = 999.0)]
		[PXDefault(TypeCode.Decimal, "100.0",
			typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<SOLine.inventoryID>>>>),
			SourceField = typeof(InventoryItem.overshipThreshold), CacheGlobal = true)]
		[PXUIField(DisplayName = "Overship Threshold (%)")]
		public virtual Decimal? CompleteQtyMax
		{
			get
			{
				return this._CompleteQtyMax;
			}
			set
			{
				this._CompleteQtyMax = value;
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
		[CurrencyInfo(typeof(SOOrder.curyInfoID))]
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
		#region PriceType
		/// <inheritdoc cref="PriceType"/>
		public abstract class priceType : IBqlField
		{
		}

		/// <summary>
		/// The type of the item price of the line.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in <see cref="PriceTypes"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PriceTypes.List]
		[PXUIField(DisplayName = "Price Type", Visible = false, Enabled = false)]
		public virtual string PriceType
		{
			get;
			set;
		}
		#endregion
		#region IsPromotionalPrice
		/// <inheritdoc cref="IsPromotionalPrice"/>
		public abstract class isPromotionalPrice : IBqlField
		{
		}

		/// <inheritdoc cref="ARSalesPrice.isPromotionalPrice"/>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Promotional Price", Visible = false, Enabled = false)]
		public virtual bool? IsPromotionalPrice
		{
			get;
			set;
		}
		#endregion
		#region CuryUnitPrice
		/// <inheritdoc cref="CuryUnitPrice"/>
		public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }
		protected Decimal? _CuryUnitPrice;

		/// <summary>
		/// The <see cref="unitPrice">unit price</see> of the item (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(SOLine.curyInfoID), typeof(SOLine.unitPrice))]
		[PXUIField(DisplayName = "Unit Price", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryUnitPrice
		{
			get
			{
				return this._CuryUnitPrice;
			}
			set
			{
				this._CuryUnitPrice = value;
			}
		}
		#endregion
		#region UnitPrice
		/// <inheritdoc cref="UnitPrice"/>
		public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }
		protected Decimal? _UnitPrice;

		/// <summary>
		/// The unit price of the item.
		/// </summary>
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Unit Price", Enabled = false)]
		public virtual Decimal? UnitPrice
		{
			get
			{
				return this._UnitPrice;
			}
			set
			{
				this._UnitPrice = value;
			}
		}
		#endregion
		#region UnitCost
		/// <inheritdoc cref="UnitCost"/>
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		protected Decimal? _UnitCost;

		/// <summary>
		/// The unit cost at which the item being returned was issued from inventory when it was sold.
		/// </summary>
		/// <value>
		/// For the return lines added with a link to an original invoice, this is the cost specified in the inventory
		/// issue transaction that was generated on release of the original invoice. For the return lines not linked
		/// to an invoice, the unit cost specified in this column depends on the items valuation method and the
		/// settings of the warehouse specified in the line.
		/// </value>
		/// <remarks>
		/// This field is available for orders of the CM, RC, RR, RM or CR type.
		/// </remarks>
		[PXDBPriceCost()]
		public virtual Decimal? UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				this._UnitCost = value;
			}
		}
		#endregion
		#region CuryUnitCost
		/// <inheritdoc cref="CuryUnitCost"/>
		public abstract class curyUnitCost : PX.Data.BQL.BqlDecimal.Field<curyUnitCost> { }
		protected Decimal? _CuryUnitCost;

		/// <summary>
		/// The <see cref="unitCost">unit cost</see> at which the item being returned was issued from inventory when
		/// it was sold (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(SOLine.curyInfoID), typeof(SOLine.unitCost),
			BaseCalc = false, KeepResultValue = true)]
		[PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.Dynamic)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryUnitCost
		{
			get
			{
				return this._CuryUnitCost;
			}
			set
			{
				this._CuryUnitCost = value;
			}
		}
		#endregion
		#region CuryExtPrice
		/// <inheritdoc cref="CuryExtPrice"/>
		public abstract class curyExtPrice : PX.Data.BQL.BqlDecimal.Field<curyExtPrice> { }
		protected Decimal? _CuryExtPrice;

		/// <summary>
		/// The <see cref="extPrice">extended price</see> of the item line (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOLine.curyInfoID), typeof(SOLine.extPrice))]
		[PXUIField(DisplayName = "Ext. Price")]
		[PXFormula(typeof(Mult<SOLine.orderQty, SOLine.curyUnitPrice>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryExtPrice
		{
			get
			{
				return this._CuryExtPrice;
			}
			set
			{
				this._CuryExtPrice = value;
			}
		}
		#endregion
		#region ExtPrice
		/// <inheritdoc cref="ExtPrice"/>
		public abstract class extPrice : PX.Data.BQL.BqlDecimal.Field<extPrice> { }
		protected Decimal? _ExtPrice;

		/// <summary>
		/// The extended price, which the system calculates as the <see cref="unitPrice">unit price</see> multiplied
		/// by the <see cref="orderQty">quantity</see>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? ExtPrice
		{
			get
			{
				return this._ExtPrice;
			}
			set
			{
				this._ExtPrice = value;
			}
		}
		#endregion
		#region CuryExtCost
		/// <inheritdoc cref="CuryExtCost"/>
		public abstract class curyExtCost : PX.Data.BQL.BqlDecimal.Field<curyExtCost> { }
		protected Decimal? _CuryExtCost;

		/// <summary>
		/// The <see cref="extCost">extended cost</see> of the item line (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOLine.curyInfoID), typeof(SOLine.extCost),
			BaseCalc = false, KeepResultValue = true)]
		[PXUIField(DisplayName = "Extended Cost")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryExtCost
		{
			get
			{
				return this._CuryExtCost;
			}
			set
			{
				this._CuryExtCost = value;
			}
		}
		#endregion
		#region ExtCost
		/// <inheritdoc cref="ExtCost"/>
		public abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }
		protected Decimal? _ExtCost;

		/// <summary>
		/// The extended cost, which the system calculates as the <see cref="unitCost">unit cost</see> multiplied
		/// by the <see cref="orderQty">quantity</see>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXFormula(typeof(Mult<SOLine.orderQty, SOLine.unitCost>))]
		public virtual Decimal? ExtCost
		{
			get
			{
				return this._ExtCost;
			}
			set
			{
				this._ExtCost = value;
			}
		}
		#endregion
		#region TaxZoneID
		/// <inheritdoc cref="TaxZoneID"/>
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		protected String _TaxZoneID;

		/// <summary>
		/// The tax zone associated with the <see cref="customerLocationID">customer location</see>.
		/// </summary>
		/// <value>
		/// If no tax zone is specified for this <see cref="customerLocationID">customer location</see>, the system
		/// inserts into this field the tax zone assigned to the selling branch.
		/// </value>
		/// <remarks>
		/// This field is available only for blanket sales orders.
		/// </remarks>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(SOOrder.taxZoneID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Tax Zone")]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
		[PXRestrictor(typeof(Where<TaxZone.isExternal, Equal<False>, Or<TaxZone.taxZoneID, Equal<Current<SOOrder.taxZoneID>>>>), TX.Messages.ExternalTaxProviderCannotBeSelectedOnLine)]
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
		#region TaxCategoryID
		/// <inheritdoc cref="TaxCategoryID"/>
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		protected String _TaxCategoryID;

		/// <summary>
		/// The tax category of the goods mentioned in this line.
		/// </summary>
		/// <remarks>
		/// <para>The field is included in the <see cref="FK.TaxCategory"/> foreign key.
		/// The field is the identifier of the tax category
		/// <see cref="TaxCategory"/>.<see cref="TaxCategory.taxCategoryID"/>.</para>
		/// <para>This field is not available for orders of the TR type.</para>
		/// </remarks>
		[PXDBString(TaxCategory.taxCategoryID.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		[SOTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), typeof(SOOrder.taxCalcMode), TaxCalc = TaxCalc.ManualLineCalc,
			   //Per Unit Tax settings
			   CuryTaxableAmtField = typeof(SOLine.curyTaxableAmt),
			   Inventory = typeof(SOLine.inventoryID), UOM = typeof(SOLine.uOM), LineQty = typeof(SOLine.orderQty))]
		[SOOpenTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), TaxCalc = TaxCalc.ManualLineCalc,
			   //Per Unit Tax settings
			   Inventory = typeof(SOLine.inventoryID), UOM = typeof(SOLine.uOM), LineQty = typeof(SOLine.openQty))]
		[SOUnbilledTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), TaxCalc = TaxCalc.ManualLineCalc,
			   //Per Unit Tax settings
			   Inventory = typeof(SOLine.inventoryID), UOM = typeof(SOLine.uOM), LineQty = typeof(SOLine.unbilledQty))]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		[PXDefault(typeof(Search<InventoryItem.taxCategoryID,
			Where<InventoryItem.inventoryID, Equal<Current<SOLine.inventoryID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing, SearchOnDefault = false)]
		public virtual String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
			}
		}
		#endregion
		#region AvalaraCustomerUsageType
		/// <inheritdoc cref="AvalaraCustomerUsageType"/>
		public abstract class avalaraCustomerUsageType : PX.Data.BQL.BqlString.Field<avalaraCustomerUsageType> { }

		/// <summary>
		/// The entity usage type of the customer location if sales to this location are tax-exempt.
		/// </summary>
		/// <value>
		/// By default, the system inserts the <see cref="SOOrder.avalaraCustomerUsageType">entity usage type</see>.
		/// </value>
		/// <remarks>
		/// This field is available only if the
		/// <see cref="FeaturesSet.avalaraTax">External Tax Calculation Integration</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.
		/// This field is not available for transfer orders.
		/// </remarks>
		[PXDefault(TXAvalaraCustomerUsageType.Default, typeof(SOOrder.avalaraCustomerUsageType))]
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Tax Exemption Type")]
		[TX.TXAvalaraCustomerUsageType.List]
		public virtual String AvalaraCustomerUsageType
		{
			get;
			set;
		}
		#endregion
		#region AlternateID
		/// <inheritdoc cref="AlternateID"/>
		public abstract class alternateID : PX.Data.BQL.BqlString.Field<alternateID> { }
		protected String _AlternateID;

		/// <summary>
		/// The alternate ID for the item, such as the barcode or the inventory ID used by the customer.
		/// </summary>
		[AlternativeItem(INPrimaryAlternateType.CPN, typeof(customerID), typeof(inventoryID), typeof(subItemID), typeof(uOM))]
		public virtual String AlternateID
		{
			get
			{
				return this._AlternateID;
			}
			set
			{
				this._AlternateID = value;
			}
		}
		#endregion
		#region CommnPct
		/// <inheritdoc cref="CommnPct"/>
		public abstract class commnPct : PX.Data.BQL.BqlDecimal.Field<commnPct> { }
		protected Decimal? _CommnPct;

		/// <summary>
		/// The default commission percentage of the salesperson.
		/// </summary>
		[PXDBDecimal(6, MinValue = 0, MaxValue=100)]
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
		/// <inheritdoc cref="CuryCommnAmt"/>
		public abstract class curyCommnAmt : PX.Data.BQL.BqlDecimal.Field<curyCommnAmt> { }
		protected Decimal? _CuryCommnAmt;

		/// <summary>
		/// The <see cref="commnAmt">default commission amount</see> of the salesperson
		/// (in the currency of the document).
		/// </summary>
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
		/// <inheritdoc cref="CommnAmt"/>
		public abstract class commnAmt : PX.Data.BQL.BqlDecimal.Field<commnAmt> { }
		protected Decimal? _CommnAmt;

		/// <summary>
		/// The default commission amount of the salesperson.
		/// </summary>
		[PXDBDecimal(4)]
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
		#region TranDesc
		/// <inheritdoc cref="TranDesc"/>
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		protected String _TranDesc;

		/// <summary>
		/// The description provided for the stock item.
		/// </summary>
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Line Description")]
		[PXLocalizableDefault(typeof(Search<InventoryItem.descr, Where<InventoryItem.inventoryID, Equal<Current<SOLine.inventoryID>>>>),
			typeof(Customer.localeName), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String TranDesc
		{
			get
			{
				return this._TranDesc;
			}
			set
			{
				this._TranDesc = value;
			}
		}
		#endregion
		#region UnitWeigth
		/// <inheritdoc cref="UnitWeigth"/>
		public abstract class unitWeigth : PX.Data.BQL.BqlDecimal.Field<unitWeigth> { }
		protected Decimal? _UnitWeigth;

		/// <summary>
		/// The unit weight of the item.
		/// </summary>
		[PXDBDecimal(6, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<InventoryItem.baseWeight, Where<InventoryItem.inventoryID, Equal<Current<SOLine.inventoryID>>>>))]
		[PXUIField(DisplayName = "Unit Weight")]
		public virtual Decimal? UnitWeigth
		{
			get
			{
				return this._UnitWeigth;
			}
			set
			{
				this._UnitWeigth = value;
			}
		}
		#endregion
		#region UnitVolume
		/// <inheritdoc cref="UnitVolume"/>
		public abstract class unitVolume : PX.Data.BQL.BqlDecimal.Field<unitVolume> { }
		protected Decimal? _UnitVolume;

		/// <summary>
		/// The unit volume of the item.
		/// </summary>
		[PXDBDecimal(6, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<InventoryItem.baseVolume, Where<InventoryItem.inventoryID, Equal<Current<SOLine.inventoryID>>>>))]
		public virtual Decimal? UnitVolume
		{
			get
			{
				return this._UnitVolume;
			}
			set
			{
				this._UnitVolume = value;
			}
		}
		#endregion
		#region ExtWeight
		/// <inheritdoc cref="ExtWeight"/>
		public abstract class extWeight : PX.Data.BQL.BqlDecimal.Field<extWeight> { }
		protected Decimal? _ExtWeight;

		/// <summary>
		/// The extended weight, which the system calculates as the <see cref="unitWeigth">unit weight</see> multiplied
		/// by the <see cref="orderQty">quantity</see>.
		/// </summary>
		[PXDBDecimal(6, MinValue = 0)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXFormula(typeof(Mult<Abs<Row<baseOrderQty>.WithDependency<orderQty>>, unitWeigth>), typeof(SumCalc<SOOrder.orderWeight>))]
		[PXUIField(DisplayName = "Ext. Weight")]
		public virtual Decimal? ExtWeight
		{
			get
			{
				return this._ExtWeight;
			}
			set
			{
				this._ExtWeight = value;
			}
		}
		#endregion
		#region ExtVolume
		/// <inheritdoc cref="ExtVolume"/>
		public abstract class extVolume : PX.Data.BQL.BqlDecimal.Field<extVolume> { }
		protected Decimal? _ExtVolume;

		/// <summary>
		/// The extended volume, which the system calculates as the <see cref="unitVolume">unit volume</see> multiplied
		/// by the <see cref="orderQty">quantity</see>.
		/// </summary>
		[PXDBDecimal(6, MinValue = 0)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXFormula(typeof(Mult<Abs<Row<baseOrderQty>.WithDependency<orderQty>>, unitVolume>), typeof(SumCalc<SOOrder.orderVolume>))]
		[PXUIField(DisplayName = "Ext. Volume")]
		public virtual Decimal? ExtVolume
		{
			get
			{
				return this._ExtVolume;
			}
			set
			{
				this._ExtVolume = value;
			}
		}
		#endregion
		#region IsFree
		/// <inheritdoc cref="IsFree"/>
		public abstract class isFree : PX.Data.BQL.BqlBool.Field<isFree> { }
		protected Boolean? _IsFree;

		/// <summary>
		/// A Boolean value that indicates whether the inventory item specified in the row is a free item.
		/// </summary>
		/// <remarks>
		/// If the field value is <see langword="true"/> then the system updates the
		/// <see cref="unitPrice">Unit Price</see>, <see cref="discPct">Discount Percent</see>,
		/// <see cref="discAmt">Discount Amount</see>, and  <see cref="extPrice">Ext. Price</see> amounts with 0 and
		/// set <see cref="manualDisc">Manual Discount</see> field to <see langword="true"/>.
		/// </remarks>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Free Item")]
		public virtual Boolean? IsFree
		{
			get
			{
				return this._IsFree;
			}
			set
			{
				this._IsFree = value;
			}
		}
		#endregion
		#region CalculateDiscountsOnImport
		/// <inheritdoc cref="CalculateDiscountsOnImport"/>
		public abstract class calculateDiscountsOnImport : PX.Data.BQL.BqlBool.Field<calculateDiscountsOnImport> { }
		protected Boolean? _CalculateDiscountsOnImport;

		/// <summary>
		/// A Boolean value that indicates whether the line discounts will be calculated automaticly.
		/// </summary>
		[PXBool()]
		[PXUIField(DisplayName = "Calculate automatic discounts on import")]
		public virtual Boolean? CalculateDiscountsOnImport
		{
			get
			{
				return this._CalculateDiscountsOnImport;
			}
			set
			{
				this._CalculateDiscountsOnImport = value;
			}
		}
		#endregion
		#region DiscPct
		/// <inheritdoc cref="DiscPct"/>
		public abstract class discPct : PX.Data.BQL.BqlDecimal.Field<discPct> { }
		protected Decimal? _DiscPct;

		/// <summary>
		/// The percent of the line-level discount.
		/// </summary>
		/// <remarks>
		/// If the <see cref="manualDisc">Manual Discount</see> field value is <see langword="true"/>, it indicates
		/// that the percent of the discount is specified by the line discount that has been applied manually, or has
		/// been entered manually or calculated based on the <see cref="discAmt">discount amount</see> of the line.
		/// </remarks>
		[PXDBDecimal(6, MinValue = -100, MaxValue=100)]
		[PXUIField(DisplayName = "Discount Percent")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DiscPct
		{
			get
			{
				return this._DiscPct;
			}
			set
			{
				this._DiscPct = value;
			}
		}
		#endregion
		#region CuryDiscAmt
		/// <inheritdoc cref="CuryDiscAmt"/>
		public abstract class curyDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscAmt> { }
		protected Decimal? _CuryDiscAmt;

		/// <summary>
		/// The <see cref="discAmt">amount</see> of the line-level discount of the line
		/// (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOLine.curyInfoID), typeof(SOLine.discAmt))]
		[PXUIField(DisplayName = "Discount Amount")]
		//[PXFormula(typeof(Div<Mult<Mult<SOLine.orderQty, SOLine.curyUnitPrice>, SOLine.discPct>, decimal100>))]->Causes SetValueExt for CuryDiscAmt 
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryDiscAmt
		{
			get
			{
				return this._CuryDiscAmt;
			}
			set
			{
				this._CuryDiscAmt = value;
			}
		}
		#endregion
		#region DiscAmt
		/// <inheritdoc cref="DiscAmt"/>
		public abstract class discAmt : PX.Data.BQL.BqlDecimal.Field<discAmt> { }
		protected Decimal? _DiscAmt;

		/// <summary>
		/// The amount of the line-level discount of the line.
		/// </summary>
		/// <remarks>
		/// If the <see cref="manualDisc">Manual Discount</see> field value is <see langword="true"/>, it indicates
		/// that the amount of the discount is specified by the line discount that has been applied manually, or has
		/// been entered manually or calculated based on the <see cref="discPct">discount percent</see> of the line.
		/// </remarks>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? DiscAmt
		{
			get
			{
				return this._DiscAmt;
			}
			set
			{
				this._DiscAmt = value;
			}
		}
		#endregion
		#region ManualDisc
		/// <inheritdoc cref="ManualDisc"/>
		public abstract class manualDisc : PX.Data.BQL.BqlBool.Field<manualDisc> { }
		protected Boolean? _ManualDisc;

		/// <summary>
		/// A Boolean value that indicates whether the discount has been applied manually.
		/// </summary>
		[ManualDiscountMode(typeof(SOLine.curyDiscAmt), typeof(SOLine.discPct), DiscountFeatureType.CustomerDiscount)]
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Manual Discount", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? ManualDisc
		{
			get
			{
				return this._ManualDisc;
			}
			set
			{
				this._ManualDisc = value;
			}
		}
		#endregion

		#region FreezeManualDisc
		/// <inheritdoc cref="FreezeManualDisc"/>
		public abstract class freezeManualDisc : PX.Data.BQL.BqlBool.Field<freezeManualDisc> { }
		protected Boolean? _FreezeManualDisc;

		/// <summary>
		/// A Boolean value that indicates whether the system will not recalculate
		/// the discount of the <see cref="DiscountType.Line">line</see> type.
		/// </summary>
		[PXBool()]
		public virtual Boolean? FreezeManualDisc
		{
			get
			{
				return this._FreezeManualDisc;
			}
			set
			{
				this._FreezeManualDisc = value;
			}
		}
		#endregion
		#region SkipDisc
		public abstract class skipDisc : PX.Data.IBqlField
		{
		}
		protected Boolean? _SkipDisc;
		[PXBool()]
		public virtual Boolean? SkipDisc
		{
			get
			{
				return this._SkipDisc;
			}
			set
			{
				this._SkipDisc = value;
			}
		}
		#endregion
		#region SkipLineDiscounts
		public abstract class skipLineDiscounts : PX.Data.BQL.BqlBool.Field<skipLineDiscounts> { }
		/// <summary>
		/// Indicates (if selected) that the automatic line discounts are not applied to this line.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Ignore Automatic Line Discounts", Visible = false)]
		public virtual bool? SkipLineDiscounts { get; set; }
		#endregion
		#region AutomaticDiscountsDisabled
		/// <inheritdoc cref="AutomaticDiscountsDisabled"/>
		public abstract class automaticDiscountsDisabled : PX.Data.BQL.BqlBool.Field<automaticDiscountsDisabled> { }

		/// <summary>
		/// A Boolean value that indicates whether the system does not need to calculate discounts, because they are
		/// already calculated.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Automatic Discounts Disabled", Visible = false, Enabled = false)]
		public virtual Boolean? AutomaticDiscountsDisabled
		{
			get;
			set;
		}
		#endregion
		#region DisableAutomaticTaxCalculation
		/// <inheritdoc cref="DisableAutomaticTaxCalculation"/>
		public abstract class disableAutomaticTaxCalculation : PX.Data.BQL.BqlBool.Field<disableAutomaticTaxCalculation> { }

		/// <summary>
		/// A Boolean value that indicates whether the system does not need to calculate taxes, because they are
		/// already calculated.
		/// </summary>
		[PXDBBool]
		[PXDefault(false, typeof(SOOrder.disableAutomaticTaxCalculation))]
		[PXUIField(DisplayName = "Disable Automatic Tax Calculation", Visible = false, Enabled = false)]
		public virtual Boolean? DisableAutomaticTaxCalculation
		{
			get;
			set;
		}
		#endregion
		#region CuryLineAmt
		/// <inheritdoc cref="CuryLineAmt"/>
		public abstract class curyLineAmt : PX.Data.BQL.BqlDecimal.Field<curyLineAmt> { }
		protected Decimal? _CuryLineAmt;

		/// <summary>
		/// The<see cref="lineAmt"> amount of the line</see>, which the system calculates as the extended price minus
		/// the line-level discount (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOLine.curyInfoID), typeof(SOLine.lineAmt))]
		[PXUIField(DisplayName = "Amount", Enabled = false)]
		[PXFormula(typeof(Sub<SOLine.curyExtPrice, SOLine.curyDiscAmt>))]
		[PXFormula(null, typeof(CountCalc<SOSalesPerTran.refCntr>))]
		[PXFormula(null, typeof(SumCalc<SOBlanketOrderLink.curyOrderedAmt>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryLineAmt
		{
			get
			{
				return this._CuryLineAmt;
			}
			set
			{
				this._CuryLineAmt = value;
			}
		}
		#endregion
		#region LineAmt
		/// <inheritdoc cref="LineAmt"/>
		public abstract class lineAmt : PX.Data.BQL.BqlDecimal.Field<lineAmt> { }
		protected Decimal? _LineAmt;

		/// <summary>
		/// The amount of the line, which the system calculates as the extended price minus the line-level discount.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LineAmt
		{
			get
			{
				return this._LineAmt;
			}
			set
			{
				this._LineAmt = value;
			}
		}
		#endregion
		#region CuryOpenAmt
		/// <inheritdoc cref="CuryOpenAmt"/>
		public abstract class curyOpenAmt : PX.Data.BQL.BqlDecimal.Field<curyOpenAmt> { }
		protected Decimal? _CuryOpenAmt;

		/// <summary>
		/// The <see cref="openAmt">amount</see> of the stock items not yet shipped according to the sales order
		/// (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOLine.curyInfoID), typeof(SOLine.openAmt))]
		[PXFormula(typeof(openQty.When<lineType.IsNotEqual<SOLineType.miscCharge>>.Else<decimal0>
			.Multiply<curyLineAmt.Divide<orderQty.When<orderQty.IsNotEqual<decimal0>>.Else<decimal1>>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Open Amount")]
		public virtual Decimal? CuryOpenAmt
		{
			get
			{
				return this._CuryOpenAmt;
			}
			set
			{
				this._CuryOpenAmt = value;
			}
		}
		#endregion
		#region OpenAmt
		/// <inheritdoc cref="OpenAmt"/>
		public abstract class openAmt : PX.Data.BQL.BqlDecimal.Field<openAmt> { }
		protected Decimal? _OpenAmt;

		/// <summary>
		/// The amount of the stock items not yet shipped according to the line.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OpenAmt
		{
			get
			{
				return this._OpenAmt;
			}
			set
			{
				this._OpenAmt = value;
			}
		}
		#endregion
		#region CuryBilledAmt
		/// <inheritdoc cref="CuryBilledAmt"/>
		public abstract class curyBilledAmt : PX.Data.BQL.BqlDecimal.Field<curyBilledAmt> { }
		protected Decimal? _CuryBilledAmt;

		/// <summary>
		/// The <see cref="billedAmt">sum of amounts</see> of the payments or prepayments that have been applied to
		/// the AR invoice generated for the line of the order (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOLine.curyInfoID), typeof(SOLine.billedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryBilledAmt
		{
			get
			{
				return this._CuryBilledAmt;
			}
			set
			{
				this._CuryBilledAmt = value;
			}
		}
		#endregion
		#region BilledAmt
		/// <inheritdoc cref="BilledAmt"/>
		public abstract class billedAmt : PX.Data.BQL.BqlDecimal.Field<billedAmt> { }
		protected Decimal? _BilledAmt;

		/// <summary>
		/// The sum of amounts of the payments or prepayments that have been applied to the AR invoice generated for
		/// the line of the order.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BilledAmt
		{
			get
			{
				return this._BilledAmt;
			}
			set
			{
				this._BilledAmt = value;
			}
		}
		#endregion
		#region CuryUnbilledAmt
		/// <inheritdoc cref="CuryUnbilledAmt"/>
		public abstract class curyUnbilledAmt : PX.Data.BQL.BqlDecimal.Field<curyUnbilledAmt> { }
		protected Decimal? _CuryUnbilledAmt;

		/// <summary>
		/// The <see cref="unbilledAmt">unbilled amount</see> for a stock item with the
		/// <see cref="SOLineType.inventory">Goods for Inventory</see> line type or a non-stock item with the
		/// <see cref="SOLineType.nonInventory">Non-Inventory Goods</see> line type (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOLine.curyInfoID), typeof(SOLine.unbilledAmt))]
		[PXFormula(typeof(SOLine.unbilledQty.When<SOLine.orderQty.IsNotEqual<decimal0>>.Else<decimal1>.Multiply<SOLine.curyLineAmt.Divide<SOLine.orderQty.When<SOLine.orderQty.IsNotEqual<decimal0>>.Else<decimal1>>>))]
		[PXUIField(DisplayName = "Unbilled Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryUnbilledAmt
		{
			get
			{
				return this._CuryUnbilledAmt;
			}
			set
			{
				this._CuryUnbilledAmt = value;
			}
		}
		#endregion
		#region UnbilledAmt
		/// <inheritdoc cref="UnbilledAmt"/>
		public abstract class unbilledAmt : PX.Data.BQL.BqlDecimal.Field<unbilledAmt> { }
		protected Decimal? _UnbilledAmt;

		/// <summary>
		/// The unbilled amount for a stock item with the <see cref="SOLineType.inventory">Goods for Inventory</see>
		/// line type or a non-stock item with the <see cref="SOLineType.nonInventory">Non-Inventory Goods</see> line
		/// type.
		/// </summary>
		/// <value>
		/// Calculated as the quantity in the sales order minus the quantity in the invoice or invoices generated for
		/// this order, multiplied by the discounted unit price in the order. The unbilled amount for a non-stock item
		/// with the <see cref="SOLineType.miscCharge">Misc. Charge</see> line type is calculated as the line amount
		/// minus the line discount (if applicable), and minus the line amount in the invoice or invoices generated for
		/// this order.
		/// </value>
		/// <remarks>
		/// This field is not available for transfer orders.
		/// </remarks>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledAmt
		{
			get
			{
				return this._UnbilledAmt;
			}
			set
			{
				this._UnbilledAmt = value;
			}
		}
		#endregion
		#region CuryDiscPrice
		/// <inheritdoc cref="CuryDiscPrice"/>
		public abstract class curyDiscPrice : PX.Data.BQL.BqlDecimal.Field<curyDiscPrice> { }
		protected Decimal? _CuryDiscPrice;

		/// <summary>
		/// The <see cref="discPrice">unit price, which has been recalculated</see> after the application of discounts
		/// (in the currency of the document).
		/// </summary>
		[EnsureSetupLoaded(typeof(CommonSetup))]
		[PXDBPriceCostCalced(typeof(Sub<SOLine.curyUnitPrice, PX.Data.Round<SOLine.curyUnitPrice.Multiply<discPct.Divide<decimal100>>, CurrentValue<CommonSetup.decPlPrcCst>>>), typeof(Decimal), CastToScale = 9, CastToPrecision = 25)]
		[PXFormula(typeof(Sub<SOLine.curyUnitPrice, PX.Data.Round<SOLine.curyUnitPrice.Multiply<discPct.Divide<decimal100>>, Current<CommonSetup.decPlPrcCst>>>))]
		[PXCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(SOLine.curyInfoID), typeof(SOLine.discPrice))]
		[PXUIField(DisplayName = "Disc. Unit Price", Enabled = false, Visible = false)]
		public virtual Decimal? CuryDiscPrice
		{
			get
			{
				return this._CuryDiscPrice;
			}
			set
			{
				this._CuryDiscPrice = value;
			}
		}
		#endregion
		#region DiscPrice
		/// <inheritdoc cref="DiscPrice"/>
		public abstract class discPrice : PX.Data.BQL.BqlDecimal.Field<discPrice> { }
		protected Decimal? _DiscPrice;

		/// <summary>
		/// The <see cref="unitPrice">unit price</see> that has been recalculated after the application of discounts.
		/// </summary>
		[PXDecimal(4)]
		[PXDBPriceCostCalced(typeof(Sub<SOLine.unitPrice, SOLine.unitPrice.Multiply<discPct.Divide<decimal100>>>), typeof(Decimal), CastToScale = 9, CastToPrecision = 25)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? DiscPrice
		{
			get
			{
				return this._DiscPrice;
			}
			set
			{
				this._DiscPrice = value;
			}
		}
		#endregion
		#region DiscountsAppliedToLine
		/// <inheritdoc cref="DiscountsAppliedToLine"/>
		public abstract class discountsAppliedToLine : PX.Data.BQL.BqlString.Field<discountsAppliedToLine> { }
		protected ushort[] _DiscountsAppliedToLine;

		/// <summary>
		/// Array of line numbers of discounts applied to the line.
		/// </summary>
		[PXDBPackedIntegerArray()]
		public virtual ushort[] DiscountsAppliedToLine
		{
			get
			{
				return this._DiscountsAppliedToLine;
			}
			set
			{
				this._DiscountsAppliedToLine = value;
			}
		}
		#endregion
		#region GroupDiscountRate
		/// <inheritdoc cref="DiscountsAppliedToLine"/>
		public abstract class groupDiscountRate : PX.Data.BQL.BqlDecimal.Field<groupDiscountRate> { }
        protected Decimal? _GroupDiscountRate;

        /// <summary>
        /// The rate of all discounts of the <see cref="DiscountType.Group">group</see> type applied to the line.
        /// </summary>
        [PXDBDecimal(18)]
        [PXDefault(TypeCode.Decimal, "1.0")]
        public virtual Decimal? GroupDiscountRate
        {
            get
            {
                return this._GroupDiscountRate;
            }
            set
            {
                this._GroupDiscountRate = value;
            }
        }
        #endregion
        #region DocumentDiscountRate
        /// <inheritdoc cref="DocumentDiscountRate"/>
        public abstract class documentDiscountRate : PX.Data.BQL.BqlDecimal.Field<documentDiscountRate> { }
        protected Decimal? _DocumentDiscountRate;

        /// <summary>
        /// The rate of all discounts of the <see cref="DiscountType.Document">document</see> type applied to the line.
        /// </summary>
        [PXDBDecimal(18)]
        [PXDefault(TypeCode.Decimal, "1.0")]
        public virtual Decimal? DocumentDiscountRate
        {
            get
            {
                return this._DocumentDiscountRate;
            }
            set
            {
                this._DocumentDiscountRate = value;
            }
        }
        #endregion
		#region AvgCost
		public abstract class avgCost : PX.Data.BQL.BqlDecimal.Field<avgCost> { }
		protected Decimal? _AvgCost;

		/// <summary>
		/// Average cost of the Inventory Item of the line.
		/// </summary>
		[PXPriceCost()]
		[PXUIField(DisplayName = "Average Cost", Enabled = false, Visible = false)]
		public virtual Decimal? AvgCost
		{
			get
			{
				return this._AvgCost;
			}
			set
			{
				this._AvgCost = value;
			}
		}
		#endregion
		#region InventorySource
		public abstract class inventorySource : Data.BQL.BqlString.Field<inventorySource> { }

		/// <summary>
		/// The source of material for a transaction line.
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(InventorySourceType.FreeStock)]
		[InventorySourceType.List]
		[PXUIField(DisplayName = "Inventory Source", FieldClass = FeaturesSet.projectAccounting.InventorySource)]
		public virtual string InventorySource
		{
			get;
			set;
		}
		#endregion
		#region ProjectID
		/// <inheritdoc cref="ProjectID"/>
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;

		/// <summary>
		/// The identifier of the <see cref="PMProject">project</see>.
		/// </summary>
		/// <remarks>
		/// The field is included in the following foreign keys:
		/// <list type="bullet">
		/// <item><see cref="FK.Project"/>. The field is the identifier of the Project
		/// <see cref="PMProject"/>.<see cref="PMProject.contractCD"/></item>
		/// <item><see cref="FK.Task"/>. The field is a part of the identifier of the Project Task
		/// <see cref="PMTask"/>.<see cref="PMTask.projectID"/></item>
		/// </list>
		/// </remarks>
		[PXDBInt()]
		[PXDefault(typeof(SOOrder.projectID))]
		[PXForeignReference(typeof(FK.Project))]
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
		#region ReasonCode
		/// <inheritdoc cref="SOLine.ReasonCode"/>
		public abstract class reasonCode : PX.Data.BQL.BqlString.Field<reasonCode> { }
		protected String _ReasonCode;

		/// <summary>
		/// The reason code to be used for creation or cancellation of the order, if applicable.
		/// </summary>
		/// <remarks>
		/// The field is included in the <see cref="FK.ReasonCode"/> foreign key. The field is the identifier of the
		/// reason code <see cref="CS.ReasonCode"/>.<see cref="CS.ReasonCode.reasonCodeID"/>.
		/// </remarks>
		[PXDBString(CS.ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID,
			Where<Current<SOLine.tranType>, Equal<INTranType.transfer>, And<ReasonCode.usage, Equal<ReasonCodeUsages.transfer>,
			   Or<Current<SOLine.tranType>, NotEqual<INTranType.transfer>, And<ReasonCode.usage, In3<ReasonCodeUsages.sales, ReasonCodeUsages.issue>>>>>>), DescriptionField = typeof(ReasonCode.descr))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName="Reason Code")]
		[PXForeignReference(typeof(FK.ReasonCode))]
		public virtual String ReasonCode
		{
			get
			{
				return this._ReasonCode;
			}
			set
			{
				this._ReasonCode = value;
			}
		}
		#endregion
		#region SalesPersonID
		/// <inheritdoc cref="SalesPersonID"/>
		public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }
		protected Int32? _SalesPersonID;

		/// <summary>
		/// The salesperson associated with the sale of the line item.
		/// </summary>
		/// <remarks>
		/// <para>The field is included in the <see cref="FK.SalesPerson"/> foreign key. The field is a part of the
		/// identifier of the salesperson
		/// <see cref="AR.SalesPerson"/>.<see cref="AR.SalesPerson.salesPersonID"/>.</para>
		/// <para>This field is available only if the <see cref="FeaturesSet.commissions">Commissions</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.
		/// This field is not available for orders of the TR type.</para>
		/// </remarks>
		[SalesPerson()]
		[PXParent(typeof(Select<SOSalesPerTran, Where<SOSalesPerTran.orderType, Equal<Current<SOLine.orderType>>, And<SOSalesPerTran.orderNbr, Equal<Current<SOLine.orderNbr>>, And<SOSalesPerTran.salespersonID, Equal<Current2<SOLine.salesPersonID>>>>>>), LeaveChildren = true, ParentCreate = true)]
		[PXDefault(typeof(SOOrder.salesPersonID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(FK.SalesPerson))]
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
		#region SalesAcctID
		/// <inheritdoc cref="SalesAcctID"/>
		public abstract class salesAcctID : PX.Data.BQL.BqlInt.Field<salesAcctID> { }
		protected Int32? _SalesAcctID;

		/// <summary>
		/// The account associated with the sale of the line item.
		/// </summary>
		/// <remarks>
		/// The field is included in the <see cref="FK.SalesAccount"/> foreign key. The field is the identifier of the
		/// Sales Account <see cref="GL.Account"/>.<see cref="GL.Account.accountID"/>.
		/// </remarks>
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(typeof(SOLine.branchID),Visible = false)]
		public virtual Int32? SalesAcctID
		{
			get
			{
				return this._SalesAcctID;
			}
			set
			{
				this._SalesAcctID = value;
			}
		}
		#endregion
		#region SalesSubID
		/// <inheritdoc cref="SalesSubID"/>
		public abstract class salesSubID : PX.Data.BQL.BqlInt.Field<salesSubID> { }
		protected Int32? _SalesSubID;

		/// <summary>
		/// The subaccount associated with the sale of the line item.
		/// </summary>
		/// <remarks>
		/// The field is included in the <see cref="FK.SalesSubaccount"/> foreign key. The field is the identifier of
		/// the Sales subaccount <see cref="GL.Sub"/>.<see cref="GL.Sub.subID"/>.
		/// </remarks>
		[PXFormula(typeof(Default<SOLine.branchID>))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[SOLineSubAccount(typeof(SOLine.salesAcctID), typeof(SOLine.branchID), Visible = false, ErrorHandling = PXErrorHandling.Always)]
		public virtual Int32? SalesSubID
		{
			get
			{
				return this._SalesSubID;
			}
			set
			{
				this._SalesSubID = value;
			}
		}
		#endregion
		#region TaskID
		/// <inheritdoc cref="TaskID"/>
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;

		/// <summary>
		/// The task of the project with which this document is associated.
		/// </summary>
		/// <remarks>
		/// <para>The field is included in the <see cref="FK.Task"/> foreign key. The field is a part of the identifier
		/// of the Project Task <see cref="PMTask"/>.<see cref="PMTask.projectID"/>.</para>
		/// <para>This field is available only if the
		/// <see cref="FeaturesSet.projectAccounting">Project Accounting</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form and the integration of the Projects
		/// submodule with Sales Orders has been enabled.</para>
		/// </remarks>
		[PXDefault(typeof(Coalesce<Search<PMAccountTask.taskID, Where<PMAccountTask.projectID, Equal<Current<SOLine.projectID>>, And<PMAccountTask.accountID, Equal<Current<SOLine.salesAcctID>>>>>,
			Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[ActiveOrInPlanningProjectTask(typeof(SOLine.projectID), BatchModule.SO, DisplayName = "Project Task")]
		[PXForeignReference(typeof(FK.Task))]
		public virtual Int32? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion
		#region CostCodeID
		/// <inheritdoc cref="CostCodeID"/>
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		protected Int32? _CostCodeID;

		/// <summary>
		/// The cost code with which this document is associated to track project costs and revenue.
		/// </summary>
		/// <remarks>
		/// <para>The field is included in the <see cref="FK.CostCode"/> foreign key. The field is the identifier of
		/// the cost code <see cref="PMCostCode"/>.<see cref="PMCostCode.costCodeID"/>.</para>
		/// <para>This field is available only if the <see cref="FeaturesSet.costCodes">Cost Codes</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form in addition to the integration of the
		/// Projects submodule with Sales Orders.</para>
		/// </remarks>
		[CostCode(typeof(salesAcctID), typeof(taskID), GL.AccountType.Income, DescriptionField = typeof(PMCostCode.description))]
		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
		public virtual Int32? CostCodeID
		{
			get
			{
				return this._CostCodeID;
			}
			set
			{
				this._CostCodeID = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote()]
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
		#region Commissionable
		/// <inheritdoc cref="Commissionable"/>
		public abstract class commissionable : PX.Data.BQL.BqlBool.Field<commissionable> { }
		protected bool? _Commissionable;

		/// <summary>
		/// A Boolean value that indicates whether the line is subjected to a sales commission.
		/// </summary>
		/// <remarks>
		/// This field is available only if the <see cref="FeaturesSet.commissions">Commissions</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.
		/// This field is not available for orders of the TR type.
		/// </remarks>
		[PXDBBool()]
		[PXFormula(typeof(Switch<Case<Where<SOLine.inventoryID, IsNotNull>, Selector<SOLine.inventoryID, InventoryItem.commisionable>>, True>))]
		[PXUIField(DisplayName = "Commissionable")]
		public bool? Commissionable
		{
			get
			{
				return _Commissionable;
			}
			set
			{
				_Commissionable = value;
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
		#region AutoCreateIssueLine
		/// <inheritdoc cref="AutoCreateIssueLine"/>
		public abstract class autoCreateIssueLine : PX.Data.BQL.BqlBool.Field<autoCreateIssueLine> { }
		protected bool? _AutoCreateIssueLine;

		/// <summary>
		/// A Boolean value that indicates whether the line of the Issue type will
		/// be created automatically for each order line of the Receipt type if the order is of the RR type.
		/// </summary>
		[PXDBBool()]
		[PXFormula(typeof(IsNull<Selector<operation, SOOrderTypeOperation.autoCreateIssueLine>, False>))]
		[PXUIField(DisplayName = "Auto Create Issue", Visibility = PXUIVisibility.Dynamic)]
		public virtual bool? AutoCreateIssueLine
		{
			get
			{
				return this._AutoCreateIssueLine;
			}
			set
			{
				this._AutoCreateIssueLine = value;
			}
		}
		#endregion
		#region ExpireDate
		/// <inheritdoc cref="ExpireDate"/>
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		protected DateTime? _ExpireDate;

		/// <summary>
		/// The expiration date for the item with the specified lot number.
		/// </summary>
		/// <remarks>
		/// This field is available only for only orders of the RR type.
		/// </remarks>
		[INExpireDate(typeof(SOLine.inventoryID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual DateTime? ExpireDate
		{
			get
			{
				return this._ExpireDate;
			}
			set
			{
				this._ExpireDate = value;
			}
		}
		#endregion

		#region IsLegacyDropShip
		/// <inheritdoc cref="IsLegacyDropShip"/>
		public abstract class isLegacyDropShip : PX.Data.BQL.BqlBool.Field<isLegacyDropShip> { }

		/// <summary>
		/// A Boolean value that indicates whether the line is a drop-ship which was created by the previous
		/// realization version.
		/// </summary>
		[PXDefault(false)]
		[PXDBBool]
		public virtual bool? IsLegacyDropShip
		{
			get;
			set;
		}
		#endregion

		#region DiscountID
		/// <inheritdoc cref="DiscountID"/>
		public abstract class discountID : PX.Data.BQL.BqlString.Field<discountID> { }
        protected String _DiscountID;

        /// <summary>
        /// The code of the discount of the line.
        /// </summary>
        /// <remarks>
        /// <para> The field is included in the following foreign keys:
        /// <list type="bullet">
        /// <item><see cref="FK.Discount"/>. The field is the identifier of the Discount
        /// <see cref="ARDiscount"/>.<see cref="ARDiscount.discountID"/></item>
        /// <item><see cref="FK.DiscountSequence"/>. The field is a part of the identifier of the Discount Sequence
        /// <see cref="AR.DiscountSequence"/>.<see cref="AR.DiscountSequence.discountID"/></item>
        /// </list></para>
        /// <para>This field is available only if the
        /// <see cref="FeaturesSet.customerDiscounts">Customer Discounts</see>
        /// feature is enabled on the Enable/Disable Features (CS100000) form.</para>
        /// </remarks>
        [PXDBString(10, IsUnicode = true)]
        [PXSelector(typeof(Search<ARDiscount.discountID, Where<ARDiscount.type, Equal<DiscountType.LineDiscount>>>))]
        [PXUIField(DisplayName = "Discount Code", Visible = true, Enabled = true)]
        public virtual String DiscountID
        {
            get
            {
                return this._DiscountID;
            }
            set
            {
                this._DiscountID = value;
            }
        }
        #endregion
        #region DiscountSequenceID
        /// <inheritdoc cref="DiscountSequenceID"/>
        public abstract class discountSequenceID : PX.Data.BQL.BqlString.Field<discountSequenceID> { }
        protected String _DiscountSequenceID;

        /// <summary>
        /// The identifier of the discount sequence of the line.
        /// </summary>
        /// <remarks>
        /// <para>The field is included in the <see cref="FK.DiscountSequence"/> foreign key. The field is a part of
        /// the identifier of the Discount Sequence
        /// <see cref="AR.DiscountSequence"/>.<see cref="AR.DiscountSequence.discountSequenceID"/>.</para>
        /// <para>This field is available only if the
        /// <see cref="FeaturesSet.customerDiscounts">Customer Discounts</see>
        /// feature is enabled on the Enable/Disable Features (CS100000) form.</para>
        /// </remarks>
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Discount Sequence", Visible = false, Enabled = false)]
        public virtual String DiscountSequenceID
        {
            get
            {
                return this._DiscountSequenceID;
            }
            set
            {
                this._DiscountSequenceID = value;
            }
        }
        #endregion
		#region POCreate
		/// <inheritdoc cref="POCreate"/>
		public abstract class pOCreate : PX.Data.BQL.BqlBool.Field<pOCreate> { }

		/// <summary>
		/// A Boolean value that indicates whether the order line was marked for purchasing (if it has not been shipped
		/// completely) and the line will be available for adding to a purchase order. 
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Mark for PO")]
		public virtual Boolean? POCreate
		{
			get;
			set;
		}
		#endregion
		#region IsSpecialOrder
		/// <inheritdoc cref="IsSpecialOrder"/>
		public abstract class isSpecialOrder : PX.Data.BQL.BqlBool.Field<isSpecialOrder> { }

		/// <summary>
		/// A Boolean value that indicates whether the line is a part of the special order.
		/// </summary>
		/// <remarks>
		/// A special order is a customer order for goods that a company does not normally keep in stock (due to their
		/// nature, specific components, dimensions, attributes, etc.) or for goods that have been acquired for a
		/// specific job only at a special purchase cost from a vendor. The special-ordered items must maintain their
		/// cost from purchase to sale and are not included in inventory cost calculations.
		/// </remarks>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Special Order", FieldClass = FeaturesSet.specialOrders.FieldClass)]
		[PXFormula(typeof(IIf<behavior.IsIn<SOBehavior.sO, SOBehavior.rM, SOBehavior.qT>.And<operation.IsEqual<SOOperation.issue>>,
			Selector<inventoryID, InventoryItem.isSpecialOrderItem>, False>))]
		[PXUnboundFormula(typeof(IIf<Where<isSpecialOrder, Equal<True>>, int1, int0>), typeof(SumCalc<SOOrder.specialLineCntr>))]
		public virtual Boolean? IsSpecialOrder
		{
			get;
			set;
		}
		#endregion
		#region OrigIsSpecialOrder
		public abstract class origIsSpecialOrder : PX.Data.BQL.BqlBool.Field<origIsSpecialOrder> { }
		[PXBool]
		public virtual Boolean? OrigIsSpecialOrder
		{
			get;
			set;
		}
		#endregion
		#region CostCenterID
		public abstract class costCenterID : Data.BQL.BqlInt.Field<costCenterID> { }
		[PXDBInt]
		[PXDefault(typeof(CostCenter.freeStock))]
		public virtual int? CostCenterID
		{
			get;
			set;
		}
		#endregion
		#region IsCostUpdatedOnPO
		public abstract class isCostUpdatedOnPO : PX.Data.BQL.BqlBool.Field<isCostUpdatedOnPO> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Update Cost On PO", FieldClass = FeaturesSet.specialOrders.FieldClass)]
		public virtual Boolean? IsCostUpdatedOnPO
		{
			get;
			set;
		}
		#endregion

		#region POSource
		/// <inheritdoc cref="POSource"/>
		public abstract class pOSource : PX.Data.BQL.BqlString.Field<pOSource> 
		{
			public sealed class SOLineBlanketPOSourceExtension : PXCacheExtension<SOLine>
			{
				public static bool IsActive() =>
					PXAccess.FeatureInstalled<FeaturesSet.blanketPO>();

				public abstract class pOSource : IBqlField { }

				[PXMergeAttributes(Method = MergeMethod.Merge)]
				[PXDBString]
				[INReplenishmentSource.SOListWithBlankets()]
				public string POSource { get; set; }
			}
		}
		protected string _POSource;

		/// <summary>
		/// The purchase order source to be used to fulfill this line.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in <see cref="INReplenishmentSource"/>.
		/// </value>
		[PXDBString()]
		[PXDefault(typeof(Switch<Case<Where<Current<pOCreate>, NotEqual<True>>, Null,
				Case<Where<FeatureInstalled<FeaturesSet.sOToPOLink>>, INReplenishmentSource.purchaseToOrder,
				Case<Where<FeatureInstalled<FeaturesSet.dropShipments>>, INReplenishmentSource.dropShipToOrder>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[INReplenishmentSource.SOList]
		[PXUIField(DisplayName = "PO Source")]
		public virtual string POSource
		{
			get
			{
				return this._POSource;
			}
			set
			{
				this._POSource = value;
			}
		}
		#endregion
		#region POCreated
		/// <inheritdoc cref="POCreated"/>
		public abstract class pOCreated : PX.Data.BQL.BqlBool.Field<pOCreated> { }
		protected Boolean? _POCreated;

		/// <summary>
		/// A Boolean value that indicates whether the line was added to a purchase order. 
		/// </summary>
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? POCreated
		{
			get
			{
				return this._POCreated;
			}
			set
			{
				this._POCreated = value;
			}
		}
		#endregion

		#region Drop-Ship Link

		#region POOrderStatus
		/// <inheritdoc cref="POOrderStatus"/>
		public abstract class pOOrderStatus : PX.Data.BQL.BqlString.Field<pOOrderStatus> { }

		/// <summary>
		/// The status of the drop-ship purchase order to which the sales order line is linked.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in <see cref="PO.POOrderStatus"/>.
		/// </value>
		[PXUIField(DisplayName = "Drop-Ship PO Status", Enabled = false, FieldClass = nameof(FeaturesSet.DropShipments))]
		[PO.POOrderStatus.List]
		[PXString(2, IsFixed = true)]
		public virtual string POOrderStatus
		{
			get;
			set;
		}
		#endregion
		#region POOrderType
		/// <inheritdoc cref="POOrderType"/>
		public abstract class pOOrderType : PX.Data.BQL.BqlString.Field<pOOrderType> { }

		/// <summary>
		/// The type of the drop-ship purchase order to which the sales order line is linked.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in <see cref="PO.POOrderType"/>.
		/// </value>
		[PO.POOrderType.List]
		[PXString(2, IsFixed = true)]
		public virtual string POOrderType
		{
			get;
			set;
		}
		#endregion
		#region POOrderNbr
		/// <inheritdoc cref="POOrderNbr"/>
		public abstract class pOOrderNbr : PX.Data.BQL.BqlString.Field<pOOrderNbr> { }

		/// <summary>
		/// The number of the drop-ship purchase order to which the sales order line is linked.
		/// </summary>
		[PXUIField(DisplayName = "Drop-Ship PO Nbr.", Enabled = false, FieldClass = nameof(FeaturesSet.DropShipments))]
		[PXSelector(typeof(Search<PO.POOrder.orderNbr, Where<PO.POOrder.orderType, Equal<Current<SOLine.pOOrderType>>>>))]
		[PXString(15, IsUnicode = true, InputMask = "")]
		public virtual string POOrderNbr
		{
			get;
			set;
		}
		#endregion
		#region POLineNbr
		/// <inheritdoc cref="POLineNbr"/>
		public abstract class pOLineNbr : PX.Data.BQL.BqlInt.Field<pOLineNbr> { }

		/// <summary>
		/// The number of the drop-ship purchase order line to which the sales order line is linked.
		/// </summary>
		[PXUIField(DisplayName = "Drop-Ship PO Line Nbr.", Enabled = false, FieldClass = nameof(FeaturesSet.DropShipments))]
		[PXInt]
		public virtual int? POLineNbr
		{
			get;
			set;
		}
		#endregion
		#region POLinkActive
		/// <inheritdoc cref="POLinkActive"/>
		public abstract class pOLinkActive : PX.Data.BQL.BqlBool.Field<pOLinkActive> { }

		/// <summary>
		/// A Boolean value that indicates whether the line has an active link to a line of the drop-ship purchase
		/// order. 
		/// </summary>
		[PXUIField(DisplayName = "PO Linked", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.DropShipments))]
		[PXBool]
		public virtual bool? POLinkActive
		{
			get;
			set;
		}
		#endregion

		#region IsPOLinkAllowed
		/// <inheritdoc cref="IsPOLinkAllowed"/>
		public abstract class isPOLinkAllowed : PX.Data.BQL.BqlBool.Field<isPOLinkAllowed> { }

		/// <summary>
		/// A Boolean value that indicates whether the
		/// <see cref="pOCreate">Mark for PO</see> field is <see langword="true"/> and the
		/// <see cref="operation">Operation</see> field is equal to the <see cref="SOOperation.issue"/> value of the
		/// line.
		/// </summary>
		[PXFormula(typeof(Switch<Case<Where<SOLine.pOCreate, Equal<True>, And<SOLine.operation, Equal<SOOperation.issue>>>, True>, False>))]
		[PXUIField(DisplayName = "", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
		[PXBool]
		public virtual bool? IsPOLinkAllowed
		{
			get;
			set;
		}
		#endregion

		#endregion

		#region VendorID
		/// <inheritdoc cref="VendorID"/>
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;

		/// <summary>
		/// The identifier of the <see cref="AP.Vendor">Vendor</see> of the sales order.
		/// </summary>
		/// <remarks>
		/// The field is included in the <see cref="FK.Vendor"/> foreign key. The field is a part of the identifier of
		/// the Vendor <see cref="AP.Vendor"/>.<see cref="AP.Vendor.bAccountID"/>.
		/// </remarks>
		[Vendor(typeof(Search2<BAccountR.bAccountID,
			LeftJoin<Branch,
				On<BAccountR.isBranch, Equal<True>, And<Branch.bAccountID, Equal<BAccountR.bAccountID>>>>,
			Where<Vendor.type, NotEqual<BAccountType.employeeType>>>))]
		[PXRestrictor(
			typeof(Where<Vendor.vStatus, IsNull,
				Or<Vendor.vStatus.IsIn<VendorStatus.active, VendorStatus.oneTime, VendorStatus.holdPayments>>>),
			AP.Messages.VendorIsInStatus, typeof(Vendor.vStatus))]
		[PXRestrictor(
			typeof(Where<Vendor.bAccountID, NotEqual<Current<SOOrder.customerID>>,
				And<Where<Branch.branchID, IsNull, Or<Branch.branchID, NotEqual<Current<SOOrder.branchID>>>>>>),
			Messages.IntercompanyInvalidPOtoSOVendor)]
		[PXFormula(typeof(Default<SOLine.siteID>))]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
        #region POSiteID
        /// <inheritdoc cref="POSiteID"/>
        public abstract class pOSiteID : PX.Data.BQL.BqlInt.Field<pOSiteID> { }
        protected Int32? _POSiteID;

        /// <summary>
        /// The identifier of the <see cref="INSite">destination warehouse</see> for the items to be purchased.
        /// The field is included in the <see cref="FK.POSite"/> foreign key.
        /// </summary>
        /// <value>
        /// The value of this field corresponds to the value of the <see cref="INSite.siteID"/> field.
        /// </value>
        [Site(DisplayName = "Purchase Warehouse", Required = true)]
		[PXForeignReference(typeof(FK.POSite))]
		public virtual Int32? POSiteID
        {
            get
            {
                return this._POSiteID;
            }
            set
            {
                this._POSiteID = value;
            }
        }
        #endregion
		#region DRTermStartDate
		/// <inheritdoc cref="DRTermStartDate"/>
		public abstract class dRTermStartDate : PX.Data.BQL.BqlDateTime.Field<dRTermStartDate> { }

		protected DateTime? _DRTermStartDate;

		/// <summary>
		/// The date when the process of deferred revenue recognition should start for the selected item.
		/// </summary>
		/// <remarks>
		/// This field is available only if the
		/// <see cref="FeaturesSet.defferedRevenue">Deferred Revenue Management</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.
		/// </remarks>
		[PXDBDate]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Term Start Date")]
		public DateTime? DRTermStartDate
		{
			get { return _DRTermStartDate; }
			set { _DRTermStartDate = value; }
		}
		#endregion
		#region DRTermEndDate
		/// <inheritdoc cref="DRTermEndDate"/>
		public abstract class dRTermEndDate : PX.Data.BQL.BqlDateTime.Field<dRTermEndDate> { }

		protected DateTime? _DRTermEndDate;

		/// <summary>
		/// The date when the process of deferred revenue recognition should finish for the selected item.
		/// </summary>
		/// <remarks>
		/// This field is available only if the
		/// <see cref="FeaturesSet.defferedRevenue">Deferred Revenue Management</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.
		/// </remarks>
		[PXDBDate]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Term End Date")]
		public DateTime? DRTermEndDate
		{
			get { return _DRTermEndDate; }
			set { _DRTermEndDate = value; }
		}
		#endregion
		#region ItemRequiresTerms
		/// <inheritdoc cref="ItemRequiresTerms"/>
		public abstract class itemRequiresTerms : PX.Data.BQL.BqlBool.Field<itemRequiresTerms> { }

		/// <summary>
		/// When set to <c>true</c>, indicates that the <see cref="DRTermStartDate"/> and <see cref="DRTermEndDate"/>
		/// fields are enabled for the line.
		/// </summary>
		/// <value>
		/// The value of this field is set by the <see cref="SOOrderEntry"/> graph based on the settings of the
		/// <see cref="InventoryID">item</see> selected for the line. In other contexts it is not populated. See the
		/// attribute on the <see cref="SOOrderEntry.SOLine_ItemRequiresTerms_CacheAttached"/> handler for details.
		/// </value>
		[PXBool]
		public virtual bool? ItemRequiresTerms
		{
			get;
			set;
		}
		#endregion
		#region ItemHasResidual
		/// <inheritdoc cref="ItemHasResidual"/>
		public abstract class itemHasResidual : PX.Data.BQL.BqlBool.Field<itemHasResidual> { }

		/// <inheritdoc cref="ARTran.itemHasResidual"/>
		[PXBool]
		[DR.DRTerms.VerifyResidual(typeof(inventoryID), null, typeof(curyUnitPriceDR), typeof(curyExtPrice))]
		public virtual bool? ItemHasResidual
		{
			get;
			set;
		}
		#endregion
		#region CuryUnitPriceDR
		/// <inheritdoc cref="CuryUnitPriceDR"/>
		public abstract class curyUnitPriceDR : PX.Data.BQL.BqlDecimal.Field<curyUnitPriceDR> { }

		protected decimal? _CuryUnitPriceDR;

		/// <inheritdoc cref="ARTran.curyUnitPriceDR"/>
		[PXUIField(DisplayName = "Unit Price for DR", Visible = false)]
		[PXDBDecimal(typeof(Search<CommonSetup.decPlPrcCst>))]
		public virtual decimal? CuryUnitPriceDR
		{
			get { return _CuryUnitPriceDR; }
			set { _CuryUnitPriceDR = value; }
		}
		#endregion
		#region DiscPctDR
		/// <inheritdoc cref="DiscPctDR"/>
		public abstract class discPctDR : PX.Data.BQL.BqlDecimal.Field<discPctDR> { }

		protected decimal? _DiscPctDR;

		/// <inheritdoc cref="ARTran.discPctDR"/>
		[PXUIField(DisplayName = "Discount Percent for DR", Visible = false)]
		[PXDBDecimal(6, MinValue = -100, MaxValue = 100)]
		public virtual decimal? DiscPctDR
		{
			get { return _DiscPctDR; }
			set { _DiscPctDR = value; }
		}
		#endregion
		#region DefScheduleID
		/// <inheritdoc cref="DefScheduleID"/>
		public abstract class defScheduleID : PX.Data.BQL.BqlInt.Field<defScheduleID> { }
		protected int? _DefScheduleID;

		/// <summary>
		/// The identifier of the <see cref="DRSchedule">deferred revenue or deferred expense schedule</see> of the
		/// sales order.
		/// </summary>
		/// <remarks>
		/// The field is included in the <see cref="FK.DefaultShedule"/> foreign key. The field is a part of the
		/// identifier of the deferred revenue or deferred expense schedule
		/// <see cref="DRSchedule"/>.<see cref="DRSchedule.scheduleID"/>.
		/// </remarks>
		[PXDBInt]
		public virtual int? DefScheduleID
		{
			get
			{
				return this._DefScheduleID;
			}
			set
			{
				this._DefScheduleID = value;
			}
		}
		#endregion
		#region IsCut
		/// <inheritdoc cref="IsCut"/>
		[Obsolete(Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R2)]
		public abstract class isCut : PX.Data.BQL.BqlBool.Field<isCut> { }
		protected bool? _IsCut;
		[Obsolete(Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R2)]
		[PXBool]
		[PXFormula(typeof(False))]
		public virtual bool? IsCut
		{
			get
			{
				return _IsCut;
			}
			set
			{
				_IsCut = value;
			}
		}
		#endregion
		#region IntercompanyPOLineNbr
		/// <inheritdoc cref="IntercompanyPOLineNbr"/>
		public abstract class intercompanyPOLineNbr : Data.BQL.BqlInt.Field<intercompanyPOLineNbr>
		{
		}

		/// <summary>
		/// The number of the Intercompany purchase order line to which the sales order line is linked.
		/// </summary>
		/// <remarks>
		/// This field is available only if the <see cref="FeaturesSet.interBranch">Inter-Branch Transactions</see>
		/// feature is enabled on the Enable/Disable Features (CS100000) form.
		/// </remarks>
		[PXDBInt]
		public virtual int? IntercompanyPOLineNbr
		{
			get;
			set;
		}
		#endregion
		/// <inheritdoc cref="ILSMaster.IsIntercompany"/>
		bool? ILSMaster.IsIntercompany => false;

		#region SubstitutionRequired
		/// <inheritdoc cref="SubstitutionRequired"/>
		public abstract class substitutionRequired : Data.BQL.BqlBool.Field<substitutionRequired> { }

		/// <summary>
		/// A Boolean value that indicates whether the current item has to be replaced with the related item that is
		/// specified on the Non-Stock Items (IN202000) or Stock Items (IN202500) form.
		/// </summary>
		/// <remarks>
		/// Shipment for the original item cannot be created if this field is <see langword="true"/>.
		/// </remarks>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Substitution Required")]
		public virtual bool? SubstitutionRequired { get; set; }
		#endregion

		#region BlanketType
		/// <inheritdoc cref="BlanketType"/>
		public abstract class blanketType : Data.BQL.BqlString.Field<blanketType> { }

		/// <summary>
		/// The <see cref="BlanketSOOrder.orderType">order type</see> of the blanket sales order from which the child
		/// order has been generated.
		/// </summary>
		/// <remarks>
		/// The field is included in the following foreign keys:
		/// <list type="bullet">
		/// <item><see cref="FK.BlanketOrder"/>. The field is a part of the identifier of the blanket sales order
		/// <see cref="BlanketSOOrder"/>.<see cref="BlanketSOOrder.orderType"/></item>
		/// <item><see cref="FK.BlanketLine"/>. The field is a part of the identifier of the blanket sales order line
		/// <see cref="BlanketSOLine"/>.<see cref="BlanketSOLine.orderType"/></item>
		/// <item><see cref="FK.BlanketSplit"/>. The field is a part of the identifier of the blanket sales order split
		/// line <see cref="BlanketSOLineSplit"/>.<see cref="BlanketSOLineSplit.orderType"/></item>
		/// <item><see cref="FK.BlanketOrderLink"/>. The field is a part of the identifier of the blanket sales order
		/// link <see cref="SOBlanketOrderLink"/>.<see cref="SOBlanketOrderLink.blanketType"/></item>
		/// </list>
		/// </remarks>
		[PXDBString(2, IsFixed = true)]
		public virtual string BlanketType
		{
			get;
			set;
		}
		#endregion
		#region BlanketNbr
		/// <inheritdoc cref="BlanketNbr"/>
		public abstract class blanketNbr : Data.BQL.BqlString.Field<blanketNbr> { }

		/// <summary>
		/// The <see cref="SOOrder.orderNbr">reference number</see> of the blanket sales order from which the child
		/// order has been generated.
		/// </summary>
		/// <remarks>
		/// The field is included in the following foreign keys:
		/// <list type="bullet">
		/// <item><see cref="FK.BlanketOrder"/>. The field is a part of the identifier of the blanket sales order
		/// <see cref="BlanketSOOrder"/>.<see cref="BlanketSOOrder.orderNbr"/></item>
		/// <item><see cref="FK.BlanketLine"/>. The field is a part of the identifier of the blanket sales order line
		/// <see cref="BlanketSOLine"/>.<see cref="BlanketSOLine.orderNbr"/></item>
		/// <item><see cref="FK.BlanketSplit"/>. The field is a part of the identifier of the blanket sales order split
		/// line <see cref="BlanketSOLineSplit"/>.<see cref="BlanketSOLineSplit.orderNbr"/></item>
		/// <item><see cref="FK.BlanketOrderLink"/>. The field is a part of the identifier of the blanket sales order
		/// link <see cref="SOBlanketOrderLink"/>.<see cref="SOBlanketOrderLink.blanketNbr"/></item>
		/// </list>
		/// </remarks>
		[PXDBString(15, IsUnicode = true)]
		[PXParent(typeof(FK.BlanketOrder))]
		[PXUIField(DisplayName = "Blanket SO Ref. Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Current<blanketType>>>>), ValidateValue = false)]
		[PXParent(typeof(FK.BlanketOrderLink))]
		[PXUnboundFormula(typeof(Switch<Case<Where<blanketNbr, IsNotNull>, int1>, int0>), typeof(SumCalc<SOOrder.blanketLineCntr>))]
		public virtual string BlanketNbr
		{
			get;
			set;
		}
		#endregion
		#region BlanketLineNbr
		/// <inheritdoc cref="BlanketLineNbr"/>
		public abstract class blanketLineNbr : Data.BQL.BqlInt.Field<blanketLineNbr> { }

		/// <summary>
		/// The <see cref="BlanketSOLine.lineNbr">blanket line number</see> of the blanket sales order from which the
		/// child order has been generated.
		/// </summary>
		/// <remarks>
		/// The field is included in the following foreign keys:
		/// <list type="bullet">
		/// <item><see cref="FK.BlanketLine"/>. The field is a part of the identifier of the blanket sales order line
		/// <see cref="BlanketSOLine"/>.<see cref="BlanketSOLine.lineNbr"/></item>
		/// <item><see cref="FK.BlanketSplit"/>. The field is a part of the identifier of the blanket sales order split
		/// line <see cref="BlanketSOLineSplit"/>.<see cref="BlanketSOLineSplit.lineNbr"/></item>
		/// </list>
		/// </remarks>
		[PXDBInt]
		[PXParent(typeof(FK.BlanketLine))]
		public virtual int? BlanketLineNbr
		{
			get;
			set;
		}
		#endregion
		#region BlanketSplitLineNbr
		/// <inheritdoc cref="BlanketSplitLineNbr"/>
		public abstract class blanketSplitLineNbr : Data.BQL.BqlInt.Field<blanketSplitLineNbr> { }

		/// <summary>
		/// The <see cref="BlanketSOLine.lineNbr">blanket split line number</see> of the blanket sales order from which
		/// the child order has been generated.
		/// </summary>
		/// <remarks>
		/// The field is included in the <see cref="FK.BlanketSplit"/> foreign key. The field is a part of the
		/// identifier of the blanket sales order split line
		/// <see cref="BlanketSOLineSplit"/>.<see cref="BlanketSOLineSplit.lineNbr"/>.
		/// </remarks>
		[PXDBInt]
		[PXParent(typeof(FK.BlanketSplit))]
		public virtual int? BlanketSplitLineNbr
		{
			get;
			set;
		}
		#endregion
		#region QtyOnOrders
		/// <inheritdoc cref="QtyOnOrders"/>
		public abstract class qtyOnOrders : Data.BQL.BqlDecimal.Field<qtyOnOrders> { }

		/// <summary>
		/// The quantity of a stock or non-stock item in a blanket sales order line distributed among child orders
		/// that are generated for this line.
		/// </summary>
		[PXDBQuantity(typeof(uOM), typeof(baseQtyOnOrders))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. On Orders", Enabled = false)]
		public virtual decimal? QtyOnOrders
		{
			get;
			set;
		}
		#endregion
		#region BaseQtyOnOrders
		/// <inheritdoc cref="BaseQtyOnOrders"/>
		public abstract class baseQtyOnOrders : Data.BQL.BqlDecimal.Field<baseQtyOnOrders> { }

		/// <summary>
		/// The quantity of the item sold, expressed in the base unit of measure in a blanket sales order line,
		/// distributed among child orders that are generated for this line.
		/// </summary>
		[PXDBDecimal(6, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BaseQtyOnOrders
		{
			get;
			set;
		}
		#endregion
		#region CustomerOrderNbr
		/// <inheritdoc cref="CustomerOrderNbr"/>
		public abstract class customerOrderNbr : Data.BQL.BqlString.Field<customerOrderNbr> { }

		/// <summary>
		/// The customer order number that the system inserts into the
		/// <see cref="SOOrder.customerOrderNbr">Customer Order Nbr.</see> field for a generated child order.	
		/// </summary>
		/// <remarks>
		/// This field is available only for blanket sales orders.
		/// </remarks>
		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "Customer Order Nbr.")]
		public virtual string CustomerOrderNbr
		{
			get;
			set;
		}
		#endregion
		#region SchedOrderDate
		/// <inheritdoc cref="SchedOrderDate"/>
		public abstract class schedOrderDate : Data.BQL.BqlDateTime.Field<schedOrderDate> { }

		/// <summary>
		/// The date on which a child order should be generated for the line of the blanket sales order.
		/// </summary>
		/// <value>
		/// By default, the system inserts the <see cref="AccessInfo.businessDate">current business date</see> to this
		/// field. The value in this field can be empty. The value cannot be earlier than the
		/// <see cref="SOOrder.orderDate">date of the blanket sales order</see> and later than the
		/// <see cref="SOOrder.cancelDate">expiration date of the blanket sales order</see>.
		/// </value>
		/// <remarks>
		/// This field is available only for blanket sales orders.
		/// </remarks>
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Sched. Order Date")]
		public virtual DateTime? SchedOrderDate
		{
			get;
			set;
		}
		#endregion
		#region SchedShipDate
		/// <inheritdoc cref="SchedShipDate"/>
		public abstract class schedShipDate : Data.BQL.BqlDateTime.Field<schedShipDate> { }

		/// <summary>
		/// The planned date of the shipment for a child order generated from this line.
		/// </summary>
		/// <value>
		/// The date in this field cannot be earlier than the
		/// <see cref="SOOrder.orderDate">date of the blanket sales order</see> and the
		/// <see cref="schedOrderDate">Sched. Order Date</see>. The value in this field cannot be later than the
		/// <see cref="SOOrder.cancelDate">expiration date of the blanket sales order</see>.
		/// </value>
		/// <remarks>
		/// This field is available only for blanket sales orders.
		/// </remarks>
		[PXDBDate]
		[PXUIField(DisplayName = "Sched. Shipment Date", FieldClass = FeaturesSet.inventory.FieldClass)]
		public virtual DateTime? SchedShipDate
		{
			get;
			set;
		}
		#endregion
		#region POCreateDate
		/// <inheritdoc cref="POCreateDate"/>
		public abstract class pOCreateDate : Data.BQL.BqlDateTime.Field<pOCreateDate> { }

		/// <summary>
		/// The planned date for creation of a purchase order.
		/// </summary>
		/// <value>
		/// By default, the system inserts the <see cref="AccessInfo.businessDate">current business date</see> to this
		/// field.
		/// </value>
		/// <remarks>
		/// This field is available only for blanket sales orders.
		/// </remarks>
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "PO Creation Date", FieldClass = FeaturesSet.inventory.FieldClass)]
		public virtual DateTime? POCreateDate
		{
			get;
			set;
		}
		#endregion
		#region BlanketOpenQty
		/// <inheritdoc cref="BlanketOpenQty"/>
		public abstract class blanketOpenQty : Data.BQL.BqlDecimal.Field<blanketOpenQty> { }

		/// <summary>
		/// The quantity of a stock or non-stock item in a blanket sales order line that has not been transferred to
		/// child orders.
		/// </summary>
		/// <value>
		/// This value is calculated as the difference between the line quantity and the quantity on child orders.
		/// </value>
		/// <remarks>
		/// This field is available only for blanket sales orders.
		/// </remarks>
		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<behavior, Equal<SOBehavior.bL>, And<lineType, NotEqual<SOLineType.miscCharge>, And<completed, Equal<False>>>>, Sub<orderQty, qtyOnOrders>>, decimal0>), typeof(decimal))]
		[PXFormula(typeof(Switch<Case<Where<behavior, Equal<SOBehavior.bL>, And<lineType, NotEqual<SOLineType.miscCharge>, And<completed, Equal<False>>>>, Sub<orderQty, qtyOnOrders>>, decimal0>), typeof(SumCalc<SOOrder.blanketOpenQty>))]
		[PXDefault]
		[PXUIField(DisplayName = "Blanket Open Qty.", Enabled = false, FieldClass = FeaturesSet.inventory.FieldClass)]
		public virtual decimal? BlanketOpenQty
		{
			get;
			set;
		}
		#endregion
		#region ChildLineCntr
		/// <inheritdoc cref="ChildLineCntr"/>
		public abstract class childLineCntr : Data.BQL.BqlInt.Field<childLineCntr> { }

		/// <summary>
		/// The identifier of the detail line of the parent order.
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? ChildLineCntr
		{
			get;
			set;
		}
		#endregion
		#region OpenChildLineCntr
		/// <inheritdoc cref="OpenChildLineCntr"/>
		public abstract class openChildLineCntr : Data.BQL.BqlInt.Field<openChildLineCntr> { }

		/// <summary>
		/// The identifier of the unshipped detail line of the parent order.
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? OpenChildLineCntr
		{
			get;
			set;
		}
		#endregion
		#region Cancelled
		/// <inheritdoc cref="Cancelled"/>
		public abstract class cancelled : Data.BQL.BqlBool.Field<cancelled> { }

		/// <summary>
		/// A Boolean value that indicates whether the order was cancelled.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? Cancelled
		{
			get;
			set;
		}
		#endregion
		#region ShipVia
		/// <inheritdoc cref="ShipVia"/>
		public abstract class shipVia : Data.BQL.BqlString.Field<shipVia> { }

		/// <summary>
		/// The ship via code that represents the carrier and its service to be used for shipping the ordered goods.
		/// </summary>
		/// <remarks>
		/// This field is available only for blanket sales orders. This field cannot be empty.
		/// </remarks>
		[PXActiveCarrierSelector(typeof(Carrier.carrierID),
			typeof(Carrier.carrierID), typeof(Carrier.description), typeof(Carrier.isCommonCarrier), typeof(Carrier.confirmationRequired), typeof(Carrier.packageRequired),
			FieldClass = FeaturesSet.inventory.FieldClass)]
		[PXDefault(typeof(SOOrder.shipVia), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<shipVia>.IsRelatedTo<Carrier.carrierID>))]
		public virtual string ShipVia
		{
			get;
			set;
		}
		#endregion
		#region FOBPoint
		/// <inheritdoc cref="FOBPoint"/>
		public abstract class fOBPoint : Data.BQL.BqlString.Field<fOBPoint> { }

		/// <summary>
		/// The point at which the ownership of the goods passes to the customer.
		/// </summary>
		/// <remarks>
		/// This field is available only for blanket sales orders. This field cannot be empty.
		/// </remarks>
		[ActiveFOBPointSelector(typeof(FOBPoint.fOBPointID), FieldClass = FeaturesSet.inventory.FieldClass, DisplayName = "FOB")]
		[PXDefault(typeof(SOOrder.fOBPoint), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<fOBPoint>.IsRelatedTo<FOBPoint.fOBPointID>))]
		public virtual string FOBPoint
		{
			get;
			set;
		}
		#endregion
		#region ShipTermsID
		/// <inheritdoc cref="ShipTermsID"/>
		public abstract class shipTermsID : Data.BQL.BqlString.Field<shipTermsID>
		{
		}

		/// <summary>
		/// The shipping terms used for the customer.
		/// </summary>
		/// <remarks>
		/// This field is available only for blanket sales orders. This field cannot be empty.
		/// </remarks>
		[ActiveShipTermsSelector(typeof(ShipTerms.shipTermsID), FieldClass = FeaturesSet.inventory.FieldClass)]
		[PXDefault(typeof(SOOrder.shipTermsID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<shipTermsID>.IsRelatedTo<ShipTerms.shipTermsID>))]
		public virtual string ShipTermsID
		{
			get;
			set;
		}
		#endregion
		#region ShipZoneID
		/// <inheritdoc cref="ShipZoneID"/>
		public abstract class shipZoneID : Data.BQL.BqlString.Field<shipZoneID> { }

		/// <summary>
		/// The identification of the shipping zone of the customer to be used to calculate the freight.
		/// </summary>
		/// <remarks>
		/// This field is available only for blanket sales orders. This field cannot be empty.
		/// </remarks>
		[ActiveShippingZoneSelector(typeof(ShippingZone.zoneID), FieldClass = FeaturesSet.inventory.FieldClass)]
		[PXDefault(typeof(SOOrder.shipZoneID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<shipZoneID>.IsRelatedTo<ShippingZone.zoneID>))]
		public virtual string ShipZoneID
		{
			get;
			set;
		}
		#endregion

		#region Properties
		/// <exclude/>
		public bool? SkipLineDiscountsBuffer { get; set; }
		#endregion

		#region Margin fields

		#region CuryTaxableAmt
		/// <summary>
		/// The line amount that is subject to tax (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryTaxableAmt { get; set; }
		/// <inheritdoc cref="CuryTaxableAmt"/>
		public abstract class curyTaxableAmt : Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }
		#endregion
		#region TaxableAmt
		/// <summary>
		/// The line amount that is subject to tax.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TaxableAmt { get; set; }
		/// <inheritdoc cref="TaxableAmt"/>
		public abstract class taxableAmt : Data.BQL.BqlDecimal.Field<taxableAmt> { }
		#endregion

		#region CuryNetSales
		/// <summary>
		/// The line amount without the tax and with the applied group and document discounts (in the currency of the document).
		/// </summary>
		/// <remarks>
		/// The value has the negative sign for the receipt lines.
		/// </remarks>
		[PXDBCurrency(typeof(curyInfoID), typeof(netSales))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryNetSales { get; set; }
		/// <inheritdoc cref="CuryNetSales"/>
		public abstract class curyNetSales : Data.BQL.BqlDecimal.Field<curyNetSales> { }
		#endregion
		#region NetSales
		/// <summary>
		/// The line amount without the tax and with the applied group and document discounts.
		/// </summary>
		/// <remarks>
		/// The value has the negative sign for the receipt lines.
		/// </remarks>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? NetSales { get; set; }
		/// <inheritdoc cref="NetSales"/>
		public abstract class netSales : Data.BQL.BqlDecimal.Field<netSales> { }
		#endregion

		#region CuryMarginAmt
		/// <summary>
		/// The line's estimated margin amount (in the currency of the document).
		/// </summary>
		/// <remarks>
		/// The value is not available for the transfer order.
		/// The value is empty for the line with zero <see cref="CuryExtCost">extended cost</see>
		/// and for the line marked for drop-ship or blanket for drop-ship.
		/// </remarks>
		[PXDBCurrency(typeof(curyInfoID), typeof(marginAmt))]
		[PXUIField(DisplayName = "Est. Margin Amount", Enabled = false)]
		public virtual decimal? CuryMarginAmt { get; set; }
		/// <inheritdoc cref="CuryMarginAmt"/>
		public abstract class curyMarginAmt : Data.BQL.BqlDecimal.Field<curyMarginAmt> { }
		#endregion
		#region MarginAmt
		/// <summary>
		/// The line's estimated margin amount.
		/// </summary>
		/// <remarks>
		/// The value is not available for the transfer order.
		/// The value is empty for the line with zero <see cref="ExtCost">extended cost</see>
		/// and for the line marked for drop-ship or blanket for drop-ship.
		/// </remarks>
		[PXDBDecimal(4)]
		public virtual decimal? MarginAmt { get; set; }
		/// <inheritdoc cref="MarginAmt"/>
		public abstract class marginAmt : Data.BQL.BqlDecimal.Field<marginAmt> { }
		#endregion

		#region MarginPct
		/// <summary>
		/// The line's estimated margin percent.
		/// </summary>
		/// <remarks>
		/// The value is not available for the transfer order.
		/// The value is empty for the line with zero <see cref="CuryExtCost">extended cost</see>
		/// and for the line marked for drop-ship or blanket for drop-ship.
		/// </remarks>
		[PXDBDecimal(2)]
		[PXUIField(DisplayName = "Est. Margin (%)", Enabled = false)]
		public virtual decimal? MarginPct { get; set; }
		/// <inheritdoc cref="MarginPct"/>
		public abstract class marginPct : Data.BQL.BqlDecimal.Field<marginPct> { }
		#endregion
		#endregion
		#region Order Orchestration
		#region IsOrchestratedLine
		/// <summary>
		/// A Boolean value that indicates whether the <see cref="SOLine"/> record was orchestrated.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Orchestrated", Enabled = false, FieldClass = nameof(FeaturesSet.OrderOrchestration))]
		public Boolean? IsOrchestratedLine { get; set; }
		public abstract class isOrchestratedLine : PX.Data.BQL.BqlBool.Field<isOrchestratedLine> { }
		#endregion

		#region OrchestrationOriginalLineNbr
		/// <summary>
		/// The original number of the <see cref="SOLine"/> record that it had before the orchestration.
		/// </summary>
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBInt]
		public Int32? OrchestrationOriginalLineNbr { get; set; }
		public abstract class orchestrationOriginalLineNbr : PX.Data.BQL.BqlInt.Field<orchestrationOriginalLineNbr> { }
		#endregion

		#region OrchestrationOriginalSiteID
		/// <summary>
		/// The identifier of the original <see cref="INSite">warehouse</see>, which the line had before the order orchestration.
		/// </summary>
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBInt]
		public virtual Int32? OrchestrationOriginalSiteID { get; set; }
		public abstract class orchestrationOriginalSiteID : PX.Data.BQL.BqlInt.Field<orchestrationOriginalSiteID> { }
		#endregion

		#region OrchestrationPlanID
		/// <summary>
		/// The ID of the orchestration plan (see <see cref="SOOrchestrationPlan"/>) associated with this <see cref="SOLine"/> record.
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Orchestration Plan", Enabled = false, FieldClass = nameof(FeaturesSet.OrderOrchestration))]
		[PXSelector(typeof(SOOrchestrationPlan.planID))]
		public String OrchestrationPlanID { get; set; }
		public abstract class orchestrationPlanID : PX.Data.BQL.BqlString.Field<orchestrationPlanID> { }
		#endregion

		#endregion


		#region Methods
		public static SOLine FromSOLineSplit(SOLineSplit item)
		{
			SOLine ret = new SOLine();
			ret.OrderType = item.OrderType;
			ret.OrderNbr = item.OrderNbr;
			ret.LineNbr = item.LineNbr;
			ret.Behavior = item.Behavior;
			ret.Operation = item.Operation;
			ret.InventoryID = item.InventoryID;
			ret.SiteID = item.SiteID;
			ret.SubItemID = item.SubItemID;
			ret.LocationID = item.LocationID;
			ret.LotSerialNbr = item.LotSerialNbr;
			ret.Qty = item.Qty;
			ret.OpenQty = item.Qty;
			ret.BaseOpenQty = item.BaseQty;
			ret.UOM = item.UOM;
			ret.OrderDate = item.OrderDate;
			ret.BaseQty = item.BaseQty;
			ret.InvtMult = item.InvtMult;
			ret.PlanType = item.PlanType;
			ret.ShipDate = item.ShipDate;
			ret.RequireAllocation = item.RequireAllocation;
			ret.RequireLocation = item.RequireLocation;
			ret.RequireShipping = item.RequireShipping;
			ret.ProjectID = item.ProjectID;
			ret.TaskID = item.TaskID;
			ret.CostCenterID = item.CostCenterID;
			ret.LineType = item.LineType;
			ret.ShipComplete = item.ShipComplete;
			ret.ShippedQty = item.ShippedQty;
			ret.BaseShippedQty = item.BaseShippedQty;
			ret.TranType = item.TranType;
			ret.POCreate = item.POCreate;
			ret.POSource = item.POSource;

			return ret;
		}
		#endregion
	}

	public class SOLineType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Inventory, Messages.Inventory),
					Pair(NonInventory, Messages.NonInventory),
					Pair(MiscCharge, Messages.MiscCharge),
				}) {}
		}

		public const string Inventory = "GI";
		public const string NonInventory = "GN";
		public const string MiscCharge = "MI";
		public const string Freight = "FR";
		public const string Discount = "DS";
		public const string Reallocation = "RA";

		public class inventory() : PX.Data.BQL.BqlString.Constant<inventory>(Inventory);

		public class nonInventory() : PX.Data.BQL.BqlString.Constant<nonInventory>(NonInventory);

		public class miscCharge() : PX.Data.BQL.BqlString.Constant<miscCharge>(MiscCharge);

		public class freight() : PX.Data.BQL.BqlString.Constant<freight>(Freight);

		public class discount() : PX.Data.BQL.BqlString.Constant<discount>(Discount);

		public class reallocation() : PX.Data.BQL.BqlString.Constant<reallocation>(Reallocation);
	}

	public class SOShipComplete
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(ShipComplete, Messages.ShipComplete),
					Pair(BackOrderAllowed, Messages.BackOrderAllowed),
					Pair(CancelRemainder, Messages.CancelRemainder),
				}) {}
		}

		public const string ShipComplete = "C";
		public const string BackOrderAllowed = "B";
		public const string CancelRemainder = "L";

		public class shipComplete() : PX.Data.BQL.BqlString.Constant<shipComplete>(ShipComplete);

		public class backOrderAllowed() : PX.Data.BQL.BqlString.Constant<backOrderAllowed>(BackOrderAllowed);

		public class cancelRemainder() : PX.Data.BQL.BqlString.Constant<cancelRemainder>(CancelRemainder);
	}

	/// <exclude/>
	// Acuminator disable once PX1011 InheritanceFromPXCacheExtension [Justification: has field marked as virtual]
	public class SOLineSuppressValidationWithManufacturingExt : PXCacheExtension<SOLine>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.manufacturingProductConfigurator>() ||
				   PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.manufacturingEstimating>();
		}

		[PXCustomize.AnyAttribute(typeof(PXFormulaAttribute), nameof(PXFormulaAttribute.ValidateAggregateCalculation), false)]
		public virtual decimal? OrderQty { get; set; }
	}
}
