using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN
{
	using System;
	using PX.Data;
	using PX.Objects.GL;
	using PX.Objects.AP;
	using PX.Objects.CM;
	using PX.Objects.CS;
	using PX.TM;
	using PX.Objects.CR;
	using PX.Data.BQL.Fluent;
	using PX.Objects.Common.GraphExtensions.Abstract;
	using PX.Objects.AR;

	public class DemandForecastModelType
	{
		public const string None = "NNN";
		public const string MovingAverage = "CMA";
		public const string ExponentialSmoothing = "ESC";
		public const string ExponentialSmoothingTrend = "EST";
		public const string ExponentialSmoothingSeasons = "ESS";
		public const string ExponentialSmoothingTrendAndSeasons = "ETS";
		public const string LinearRegression = "LRT";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(None, Messages.DFM_None),
					Pair(MovingAverage, Messages.DFM_MovingAverage),
				}
				) {}
		}

		public class none() : PX.Data.BQL.BqlString.Constant<none>(None);
	}

	public class DemandPeriodType
	{
		public const string Month = "MT";
		public const string Week = "WK";		
		public const string Quarter= "QT";
		public const string Day = "DY";
		
		public class month() : PX.Data.BQL.BqlString.Constant<month>(Month);
		public class quarter() : PX.Data.BQL.BqlString.Constant<quarter>(Quarter);
		public class week() : PX.Data.BQL.BqlString.Constant<week>(Week);
		public class day() : PX.Data.BQL.BqlString.Constant<day>(Day);

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Quarter, Messages.Quarter),
					Pair(Month, Messages.Month),
					Pair(Week, Messages.Week),
					Pair(Day, Messages.Day),
				}) {}
		}
	}

	[Serializable]
	[PXPrimaryGraph(typeof(INItemSiteMaint))]
	[PXCacheName(Messages.ItemWarehouseSettings, PXDacType.Config)]
    [PXProjection(typeof(Select2<INItemSite, 
	    InnerJoin<INSite, 
		    On<FK.Site>, 
		LeftJoin<INItemStats, 
			On<INItemStats.FK.ItemSite>>>>), new Type[] { typeof(INItemSite) })]
	[PXGroupMask(typeof(InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INItemSite.inventoryID>, And<Match<InventoryItem, Current<AccessInfo.userName>>>>,
		InnerJoin<INSite, On<INSite.siteID, Equal<INItemSite.siteID>, And<Match<INSite, Current<AccessInfo.userName>>>>>>))]
	public partial class INItemSite : PXBqlTable, PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INItemSite>.By<inventoryID, siteID>
		{
			public static INItemSite Find(PXGraph graph, int? inventoryID, int? siteID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, inventoryID, siteID, options);
		}
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INItemSite>.By<inventoryID> { }
			public class Site : INSite.PK.ForeignKeyOf<INItemSite>.By<siteID> { }
			public class InventoryAccount : GL.Account.PK.ForeignKeyOf<INItemSite>.By<invtAcctID> { }
			public class InventorySubaccount : Sub.PK.ForeignKeyOf<INItemSite>.By<invtSubID> { }
			public class ReplenishmentSourceSite : IN.INSite.PK.ForeignKeyOf<INItemSite>.By<replenishmentSourceSiteID> { }
			public class DefaultShipLocation : INLocation.PK.ForeignKeyOf<INItemSite>.By<dfltShipLocationID> { }
			public class DefaultReceiptLocation : INLocation.PK.ForeignKeyOf<INItemSite>.By<dfltReceiptLocationID> { }
			public class DefaultPutawayLocation : INLocation.PK.ForeignKeyOf<INItemSite>.By<dfltPutawayLocationID> { }
			public class PreferredVendor : Vendor.PK.ForeignKeyOf<INItemSite>.By<preferredVendorID> { }
			public class PreferredVendorLocation : Location.PK.ForeignKeyOf<INItemSite>.By<preferredVendorID, preferredVendorLocationID> { }
			public class ABCCode : INABCCode.PK.ForeignKeyOf<INItemSite>.By<aBCCodeID> { }
			public class MovementClass : INMovementClass.PK.ForeignKeyOf<INItemSite>.By<movementClassID> { }
			public class PendingMovementClass : INMovementClass.PK.ForeignKeyOf<INItemSite>.By<pendingMovementClassID> { }
			//todo: uncomment and add a constant parameter when AC-169735 is done
			//public class PendingMovementClassPeriod : FinPeriod.PK.ForeignKeyOf<INItemSite>.By<pendingMovementClassPeriodID> { }
			public class ReplenishmentClass : INReplenishmentClass.PK.ForeignKeyOf<INItemSite>.By<replenishmentClassID> { }
			public class ReplenishmentPolicy : INReplenishmentPolicy.PK.ForeignKeyOf<INItemSite>.By<replenishmentPolicyID> { }
			public class ProductWorkgroup : EPCompanyTree.PK.ForeignKeyOf<INItemSite>.By<productWorkgroupID> { }
			public class ProductManager : CR.Standalone.EPEmployee.PK.ForeignKeyOf<INItemSite>.By<productManagerID> { }
			public class PriceWorkgroup : EPCompanyTree.PK.ForeignKeyOf<INItemSite>.By<priceWorkgroupID> { }
			public class PriceManager : CR.Standalone.EPEmployee.PK.ForeignKeyOf<INItemSite>.By<priceManagerID> { }
			public class CountryOfOrigin : Country.PK.ForeignKeyOf<INItemSite>.By<countryOfOrigin> { }

		}
		#endregion
        #region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected Boolean? _Selected = false;
		[PXBool()]
		[PXUIField(DisplayName = "Selected")]
		public virtual Boolean? Selected
		{
			get
			{
				return this._Selected;
			}
			set
			{
				this._Selected = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[StockItem(IsKey = true, DirtyRead = true, DisplayName="Inventory ID", TabOrder = 1)]		
		[PXParent(typeof(FK.InventoryItem))]
		[PXDefault()]
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
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[IN.Site(IsKey = true, TabOrder = 2)]
		[PXDefault()]
		[PXParent(typeof(FK.Site))]
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
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		[PXDBBool(BqlField = typeof(INSite.active))]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? Active { get; set; }
		#endregion
		#region SiteStatus
		public abstract class siteStatus : PX.Data.BQL.BqlString.Field<siteStatus> { }
		protected String _SiteStatus;
		[PXDBString(2, IsFixed = true)]
		[PXDefault("AC")]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
		[PXStringList(new string[] { "AC", "IN" }, new string[] { "Active", "Inactive" })]
		public virtual String SiteStatus
		{
			get
			{
				return this._SiteStatus;
			}
			set
			{
				this._SiteStatus = value;
			}
		}
		#endregion
		#region OverrideInvtAcctSub
		public abstract class overrideInvtAcctSub : PX.Data.BQL.BqlBool.Field<overrideInvtAcctSub> { }
		protected bool? _OverrideInvtAcctSub;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = Messages.OverrideInventoryAcctSub)]
		public virtual bool? OverrideInvtAcctSub
		{
			get
			{
				return _OverrideInvtAcctSub;
			}
			set
			{
				_OverrideInvtAcctSub = value;
			}
		}
		#endregion
		#region InvtAcctID
		public abstract class invtAcctID : PX.Data.BQL.BqlInt.Field<invtAcctID>
		{
			/// <exclude/>
			public class WarnIfNonZeroCostLayerExists : WarnIfNonZeroCostLayerExists<invtAcctID, INItemSiteMaint>
			{
				public static bool IsActive() => true;

				protected override bool FilterBySiteID => true;

				protected override void OnPreventEdit(GetEditPreventingReasonArgs e)
				{
					if (e.NewValue == null) e.Cancel = true;
					base.OnPreventEdit(e);
				}

				protected override bool AccountIDChanged(GetEditPreventingReasonArgs e, out int? originalAccountID, out int? newAccountID)
				{
					var row = (INItemSite)e.Row;
					var overrideInvtAcctSub = (bool?)e.Cache.GetValueOriginal<overrideInvtAcctSub>(row);
					if (overrideInvtAcctSub == false && row.OverrideInvtAcctSub == false)
					{
						originalAccountID = newAccountID = null;
						return false;
					}
					else if (overrideInvtAcctSub != row.OverrideInvtAcctSub)
					{
						originalAccountID = overrideInvtAcctSub == false ?
												GetDefaultAccountID(e) :
												(int?)e.Cache.GetValueOriginal<invtAcctID>(e.Row);
						newAccountID = row.OverrideInvtAcctSub == false ?
											   GetDefaultAccountID(e) :
											   (int?)e.NewValue;

						return originalAccountID != newAccountID;
					}

					return base.AccountIDChanged(e, out originalAccountID, out newAccountID);
				}

				private int? GetDefaultAccountID(GetEditPreventingReasonArgs e)
				{
					var row = (INItemSite)e.Row;
					var item = InventoryItem.PK.Find(e.Graph, row.InventoryID);
					var site = INSite.PK.Find(e.Graph, row.SiteID);
					var postclass = INPostClass.PK.Find(e.Graph, item.PostClassID);
					var rowCopy = (INItemSite)e.Cache.CreateCopy(row);
					INItemSiteMaint.DefaultInvtAcctSub(e.Graph, rowCopy, item, site, postclass);
					return rowCopy.InvtAcctID;
				}

				protected virtual void _(Events.FieldUpdated<INItemSite, INItemSite.overrideInvtAcctSub> e)
				{
					this.GetEditPreventingReason(new GetEditPreventingReasonArgs(e.Cache, typeof(invtAcctID), e.Row, e.Row.InvtAcctID));
				}
			}
		}
		protected Int32? _InvtAcctID;
		[Account(DisplayName = "Inventory Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.IN)]
		[PXUIRequired(typeof(Where<INItemSite.overrideInvtAcctSub, Equal<True>>))]
		[PXUIEnabled(typeof(Where<INItemSite.overrideInvtAcctSub, Equal<True>>))]
		[PXDefault()]
		[PXForeignReference(typeof(FK.InventoryAccount))]
		public virtual Int32? InvtAcctID
		{
			get
			{
				return this._InvtAcctID;
			}
			set
			{
				this._InvtAcctID = value;
			}
		}
		#endregion
		#region InvtSubID
		public abstract class invtSubID : PX.Data.BQL.BqlInt.Field<invtSubID> { }
		protected Int32? _InvtSubID;
		[SubAccount(typeof(INItemSite.invtAcctID), DisplayName = "Inventory Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXUIRequired(typeof(Where<INItemSite.overrideInvtAcctSub, Equal<True>>))]
		[PXUIEnabled(typeof(Where<INItemSite.overrideInvtAcctSub, Equal<True>>))]
		[PXDefault()]
		[PXForeignReference(typeof(FK.InventorySubaccount))]
		public virtual Int32? InvtSubID
		{
			get
			{
				return this._InvtSubID;
			}
			set
			{
				this._InvtSubID = value;
			}
		}
		#endregion
		#region ValMethod
		public abstract class valMethod : PX.Data.BQL.BqlString.Field<valMethod> { }
		protected String _ValMethod;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(Search<InventoryItem.valMethod, Where<InventoryItem.inventoryID, Equal<Current<INItemSite.inventoryID>>>>))]
		public virtual String ValMethod
		{
			get
			{
				return this._ValMethod;
			}
			set
			{
				this._ValMethod = value;
			}
		}
		#endregion
		#region DfltShipLocationID
		public abstract class dfltShipLocationID : PX.Data.BQL.BqlInt.Field<dfltShipLocationID> { }
		protected Int32? _DfltShipLocationID;
		[PXRestrictor(typeof(Where<INLocation.active, Equal<True>>), Messages.LocationIsNotActive)]
		[IN.Location(typeof(INItemSite.siteID), DisplayName = "Default Issue From", DescriptionField = typeof(INLocation.descr))]
		[PXForeignReference(typeof(FK.DefaultShipLocation))]
		public virtual Int32? DfltShipLocationID
		{
			get
			{
				return this._DfltShipLocationID;
			}
			set
			{
				this._DfltShipLocationID = value;
			}
		}
		#endregion
		#region DfltReceiptLocationID
		public abstract class dfltReceiptLocationID : PX.Data.BQL.BqlInt.Field<dfltReceiptLocationID> { }
		protected Int32? _DfltReceiptLocationID;
		[PXRestrictor(typeof(Where<INLocation.active, Equal<True>>), Messages.LocationIsNotActive)]
		[IN.Location(typeof(INItemSite.siteID), DisplayName = "Default Receipt To", DescriptionField = typeof(INLocation.descr))]
		[PXForeignReference(typeof(FK.DefaultReceiptLocation))]
		public virtual Int32? DfltReceiptLocationID
		{
			get
			{
				return this._DfltReceiptLocationID;
			}
			set
			{
				this._DfltReceiptLocationID = value;
			}
		}
		#endregion
		#region DfltPutawayLocationID
		public abstract class dfltPutawayLocationID : PX.Data.BQL.BqlInt.Field<dfltPutawayLocationID> { }

		/// <summary>
		/// The location of the warehouse to be used by default to put away the selected stock item.
		/// </summary>
		[PXRestrictor(typeof(Where<INLocation.active, Equal<True>>), Messages.LocationIsNotActive)]
		[IN.Location(typeof(INItemSite.siteID), DisplayName = "Default Putaway To", DescriptionField = typeof(INLocation.descr))]
		[PXForeignReference(typeof(FK.DefaultPutawayLocation))]
		public virtual int? DfltPutawayLocationID { get; set; }
		#endregion
		#region DfltSalesUnit
		public abstract class dfltSalesUnit : PX.Data.BQL.BqlString.Field<dfltSalesUnit> { }
		protected String _DfltSalesUnit;
		[INUnit(null, typeof(InventoryItem.baseUnit), DisplayName = "Sales Unit")]
		[PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<INItemSite.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String DfltSalesUnit
		{
			get
			{
				return this._DfltSalesUnit;
			}
			set
			{
				this._DfltSalesUnit = value;
			}
		}
		#endregion
		#region DfltPurchaseUnit
		public abstract class dfltPurchaseUnit : PX.Data.BQL.BqlString.Field<dfltPurchaseUnit> { }
		protected String _DfltPurchaseUnit;
		[INUnit(null, typeof(InventoryItem.baseUnit), DisplayName = "Purchase Unit")]
		[PXDefault(typeof(Search<InventoryItem.purchaseUnit, Where<InventoryItem.inventoryID, Equal<Current<INItemSite.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String DfltPurchaseUnit
		{
			get
			{
				return this._DfltPurchaseUnit;
			}
			set
			{
				this._DfltPurchaseUnit = value;
			}
		}
		#endregion
		#region LastStdCost
		public abstract class lastStdCost : PX.Data.BQL.BqlDecimal.Field<lastStdCost> { }
		protected Decimal? _LastStdCost;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Cost", Enabled = false)]
		[CurySymbol(siteID: typeof(siteID))]
		public virtual Decimal? LastStdCost
		{
			get
			{
				return this._LastStdCost;
			}
			set
			{
				this._LastStdCost = value;
			}
		}
		#endregion
		#region PendingStdCost
		public abstract class pendingStdCost : PX.Data.BQL.BqlDecimal.Field<pendingStdCost> { }
		protected Decimal? _PendingStdCost;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Pending Cost")]
		[CurySymbol(siteID: typeof(siteID))]
		public virtual Decimal? PendingStdCost
		{
			get
			{
				return this._PendingStdCost;
			}
			set
			{
				this._PendingStdCost = value;
			}
		}
		#endregion
		#region PendingStdCostDate
		public abstract class pendingStdCostDate : PX.Data.BQL.BqlDateTime.Field<pendingStdCostDate> { }
		protected DateTime? _PendingStdCostDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Pending Cost Date")]
		public virtual DateTime? PendingStdCostDate
		{
			get
			{
				return this._PendingStdCostDate;
			}
			set
			{
				this._PendingStdCostDate = value;
			}
		}
		#endregion
		#region PendingStdCostReset
		public abstract class pendingStdCostReset : PX.Data.BQL.BqlBool.Field<pendingStdCostReset>
		{
		}
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? PendingStdCostReset
		{
			get;
			set;
		}
		#endregion
		#region StdCost
		public abstract class stdCost : PX.Data.BQL.BqlDecimal.Field<stdCost> { }
		protected Decimal? _StdCost;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Current Cost", Enabled = false)]
		[CurySymbol(siteID: typeof(siteID))]
		public virtual Decimal? StdCost
		{
			get
			{
				return this._StdCost;
			}
			set
			{
				this._StdCost = value;
			}
		}
		#endregion
		#region StdCostDate
		public abstract class stdCostDate : PX.Data.BQL.BqlDateTime.Field<stdCostDate> { }
		protected DateTime? _StdCostDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Effective Date", Enabled = false)]
        [PXFormula(typeof(Switch<Case<Where<INItemSite.lastStdCost, NotEqual<CS.decimal0>>, Current<AccessInfo.businessDate>>, INItemSite.stdCostDate>))]
		public virtual DateTime? StdCostDate
		{
			get
			{
				return this._StdCostDate;
			}
			set
			{
				this._StdCostDate = value;
			}
		}
		#endregion
		#region LastBasePrice
		public abstract class lastBasePrice : PX.Data.BQL.BqlDecimal.Field<lastBasePrice> { }
		protected Decimal? _LastBasePrice;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Price", Enabled = false)]
		public virtual Decimal? LastBasePrice
		{
			get
			{
				return this._LastBasePrice;
			}
			set
			{
				this._LastBasePrice = value;
			}
		}
		#endregion
		#region PendingBasePrice
		public abstract class pendingBasePrice : PX.Data.BQL.BqlDecimal.Field<pendingBasePrice> { }
		protected Decimal? _PendingBasePrice;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Pending Price")]
		public virtual Decimal? PendingBasePrice
		{
			get
			{
				return this._PendingBasePrice;
			}
			set
			{
				this._PendingBasePrice = value;
			}
		}
		#endregion
		#region PendingBasePriceDate
		public abstract class pendingBasePriceDate : PX.Data.BQL.BqlDateTime.Field<pendingBasePriceDate> { }
		protected DateTime? _PendingBasePriceDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Pending Price Date", Enabled = false)]
		public virtual DateTime? PendingBasePriceDate
		{
			get
			{
				return this._PendingBasePriceDate;
			}
			set
			{
				this._PendingBasePriceDate = value;
			}
		}
		#endregion
		#region BasePrice
		public abstract class basePrice : PX.Data.BQL.BqlDecimal.Field<basePrice> { }
		protected Decimal? _BasePrice;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Current Price", Enabled = false)]
		public virtual Decimal? BasePrice
		{
			get
			{
				return this._BasePrice;
			}
			set
			{
				this._BasePrice = value;
			}
		}
		#endregion
		#region BasePriceDate
		public abstract class basePriceDate : PX.Data.BQL.BqlDateTime.Field<basePriceDate> { }
		protected DateTime? _BasePriceDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Effective Date", Enabled = false)]
		public virtual DateTime? BasePriceDate
		{
			get
			{
				return this._BasePriceDate;
			}
			set
			{
				this._BasePriceDate = value;
			}
		}
		#endregion
		#region LastCostDate
		public abstract class lastCostDate : PX.Data.BQL.BqlDateTime.Field<lastCostDate> { }
		protected DateTime? _LastCostDate;
        [PXDate]
        public virtual DateTime? LastCostDate
		{
			get
			{
				return this._LastCostDate;
			}
			set
			{
				this._LastCostDate = value;
			}
		}
		#endregion
		#region LastCost
		public abstract class lastCost : PX.Data.BQL.BqlDecimal.Field<lastCost> { }
		protected Decimal? _LastCost;
		[PXDBPriceCost(BqlField = typeof(INItemStats.lastCost))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Cost", Enabled = true)]
		[CurySymbol(siteID: typeof(siteID))]
		public virtual Decimal? LastCost
		{
			get
			{
				return this._LastCost;
			}
			set
			{
				this._LastCost = value;
			}
		}
		#endregion
		#region AvgCost
		public abstract class avgCost : PX.Data.BQL.BqlDecimal.Field<avgCost> { }
		protected Decimal? _AvgCost;
		[PXPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Average Cost", Enabled = false)]
		[PXDBPriceCostCalced(typeof(Switch<Case<Where<INItemStats.qtyOnHand, Equal<decimal0>>, decimal0>, Div<INItemStats.totalCost, INItemStats.qtyOnHand>>), typeof(Decimal), CastToScale = 9, CastToPrecision = 25)]
		[CurySymbol(siteID: typeof(siteID))]
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
		#region MinCost
		public abstract class minCost : PX.Data.BQL.BqlDecimal.Field<minCost> { }
		protected Decimal? _MinCost;
		[PXDBPriceCost(BqlField = typeof(INItemStats.minCost))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Min. Cost", Enabled = false)]
		[CurySymbol(siteID: typeof(siteID))]
		public virtual Decimal? MinCost
		{
			get
			{
				return this._MinCost;
			}
			set
			{
				this._MinCost = value;
			}
		}
		#endregion
		#region MaxCost
		public abstract class maxCost : PX.Data.BQL.BqlDecimal.Field<maxCost> { }
		protected Decimal? _MaxCost;
        [PXDBPriceCost(BqlField = typeof(INItemStats.maxCost))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Max. Cost", Enabled = false)]
		[CurySymbol(siteID: typeof(siteID))]
		public virtual Decimal? MaxCost
		{
			get
			{
				return this._MaxCost;
			}
			set
			{
				this._MaxCost = value;
			}
		}
		#endregion
		#region TranUnitCost
		public abstract class tranUnitCost : PX.Data.BQL.BqlDecimal.Field<tranUnitCost> { }
		protected Decimal? _TranUnitCost;
		[PXDBCalced(typeof(Switch<Case<Where<INItemSite.valMethod, Equal<INValMethod.standard>>, INItemSite.stdCost,
								Case<Where2<
										Where<INItemSite.valMethod, Equal<INValMethod.average>, And<INSite.avgDefaultCost, Equal<INSite.avgDefaultCost.averageCost>,
											Or<INItemSite.valMethod, Equal<INValMethod.fIFO>, And<INSite.fIFODefaultCost, Equal<INSite.avgDefaultCost.averageCost>>>>>,
										And<INItemStats.qtyOnHand, NotEqual<decimal0>,
										And<Div<INItemStats.totalCost, INItemStats.qtyOnHand>, GreaterEqual<decimal0>>>>,
									Div<INItemStats.totalCost, INItemStats.qtyOnHand>,
								Case<Where<INItemStats.lastCostDate, GreaterEqual<INItemStats.dateAfterMinDate>>, INItemStats.lastCost>>>,
								Null>), typeof(Decimal), CastToScale = 9, CastToPrecision = 25)]
		public virtual Decimal? TranUnitCost
		{
			get
			{
				return this._TranUnitCost;
			}
			set
			{
				this._TranUnitCost = value;
			}
		}
		#endregion
		#region PreferredVendorOverride
		public abstract class preferredVendorOverride : PX.Data.BQL.BqlBool.Field<preferredVendorOverride> { }
		protected Boolean? _PreferredVendorOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override Preferred Vendor")]
		public virtual Boolean? PreferredVendorOverride
		{
			get
			{
				return this._PreferredVendorOverride;
			}
			set
			{
				this._PreferredVendorOverride = value;
			}
		}
		#endregion
		#region PreferredVendorID
		public abstract class preferredVendorID : PX.Data.BQL.BqlInt.Field<preferredVendorID>
		{
			public class PreventEditBAccountVOrgBAccountID<TGraph> :
				PreventEditBAccountRestrictToBase<BAccount.vOrgBAccountID, TGraph, INItemSite,
					SelectFrom<INItemSite>
					.Where<INItemSite.preferredVendorOverride.IsEqual<True>.
						And<INItemSite.preferredVendorID.IsEqual<BAccount.bAccountID.FromCurrent>>>>
				where TGraph : PXGraph
			{
				protected override string GetErrorMessage(BAccount baccount, INItemSite document, string documentBaseCurrency)
				{
					var inventory = InventoryItem.PK.Find(Base, document.InventoryID);
					var site = INSite.PK.Find(Base, document.SiteID);

					return PXMessages.LocalizeFormatNoPrefix(Messages.CannotChangeRestricToIfINItemSiteExists,
						documentBaseCurrency, baccount.AcctCD, inventory?.InventoryCD, site?.SiteCD);
				}

				protected override string GetBaseCurrency(INItemSite document)
				{
					var site = INSite.PK.Find(Base, document.SiteID);
					return site.BaseCuryID;
				}
			}


			public class PreventEditBAccountVOrgBAccountIDOnVendorMaint : PreventEditBAccountVOrgBAccountID<VendorMaint>
			{
				public static bool IsActive()
					=> PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
			}

			public class PreventEditBAccountVOrgBAccountIDOnCustomerMaint : PreventEditBAccountVOrgBAccountID<CustomerMaint>
			{
				public static bool IsActive()
					=> PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
			}
		}
		protected Int32? _PreferredVendorID;
		[AP.VendorNonEmployeeActiveOrHoldPayments(DisplayName = "Preferred Vendor", Required = false, DescriptionField = typeof(Vendor.acctName))]
		[PXForeignReference(typeof(FK.PreferredVendor))]
		public virtual Int32? PreferredVendorID
		{
			get
			{
				return this._PreferredVendorID;
			}
			set
			{
				this._PreferredVendorID = value;
			}
		}
		#endregion
		#region PreferredVendorLocationID
		public abstract class preferredVendorLocationID : PX.Data.BQL.BqlInt.Field<preferredVendorLocationID> { }
		protected Int32? _PreferredVendorLocationID;
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<INItemSite.preferredVendorID>>>),
			DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Preferred Location")]
		[PXDefault(typeof(Search<Vendor.defLocationID, Where<Vendor.bAccountID, Equal<Current<INItemSite.preferredVendorID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<INItemSite.preferredVendorID>))]
		public virtual Int32? PreferredVendorLocationID
		{
			get
			{
				return this._PreferredVendorLocationID;
			}
			set
			{
				this._PreferredVendorLocationID = value;
			}
		}
		#endregion

		#region ProductManagerOverride
		public abstract class productManagerOverride : PX.Data.IBqlField { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override Product Manager")]
		public virtual Boolean? ProductManagerOverride
		{
			get;
			set;
		}
		#endregion
		#region ProductWorkgroupID
		public abstract class productWorkgroupID : PX.Data.BQL.BqlInt.Field<productWorkgroupID> { }
		protected Int32? _ProductWorkgroupID;
		[PXDBInt()]
		[PXCompanyTreeSelector]
		[PXUIField(DisplayName = "Product Workgroup")]
		[PXDefault(typeof(Parent<InventoryItem.productWorkgroupID>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIEnabled(typeof(Where<INItemSite.productManagerOverride, Equal<True>>))]
		public virtual Int32? ProductWorkgroupID
		{
			get
			{
				return this._ProductWorkgroupID;
			}
			set
			{
				this._ProductWorkgroupID = value;
			}
		}
		#endregion
		#region ProductManagerID
		public abstract class productManagerID : PX.Data.BQL.BqlInt.Field<productManagerID> { }
		protected int? _ProductManagerID;
		[Owner(typeof(INItemSite.productWorkgroupID), DisplayName = "Product Manager")]
		[PXDefault(typeof(Parent<InventoryItem.productManagerID>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIEnabled(typeof(Where<INItemSite.productManagerOverride, Equal<True>>))]
		public virtual int? ProductManagerID
		{
			get
			{
				return this._ProductManagerID;
			}
			set
			{
				this._ProductManagerID = value;
			}
		}
		#endregion
		#region PriceWorkgroupID
		public abstract class priceWorkgroupID : PX.Data.BQL.BqlInt.Field<priceWorkgroupID> { }
		protected Int32? _PriceWorkgroupID;
		[PXDBInt()]
		[PXCompanyTreeSelector]
		[PXUIField(DisplayName = "Price Workgroup")]
		public virtual Int32? PriceWorkgroupID
		{
			get
			{
				return this._PriceWorkgroupID;
			}
			set
			{
				this._PriceWorkgroupID = value;
			}
		}
		#endregion
		#region PriceManagerID
		public abstract class priceManagerID : PX.Data.BQL.BqlInt.Field<priceManagerID> { }
		protected int? _PriceManagerID;
		[Owner(typeof(INItemSite.priceWorkgroupID), DisplayName = "Price Manager")]
		public virtual int? PriceManagerID
		{
			get
			{
				return this._PriceManagerID;
			}
			set
			{
				this._PriceManagerID = value;
			}
		}
		#endregion
		#region IsDefault
		public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }
		protected Boolean? _IsDefault;
		[PXBool()]
		[PXUIField(DisplayName = "Default")]
		public virtual Boolean? IsDefault
		{
			get
			{
				return this._IsDefault;
			}
			set
			{
				this._IsDefault = value;
			}
		}
		#endregion
		#region StdCostOverride
		public abstract class stdCostOverride : PX.Data.BQL.BqlBool.Field<stdCostOverride> { }
		protected Boolean? _StdCostOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override Std. Cost")]
		public virtual Boolean? StdCostOverride
		{
			get
			{
				return this._StdCostOverride;
			}
			set
			{
				this._StdCostOverride = value;
			}
		}
		#endregion
		#region BasePriceOverride
		public abstract class basePriceOverride : PX.Data.BQL.BqlBool.Field<basePriceOverride> { }
		protected Boolean? _BasePriceOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Price Override")]
		public virtual Boolean? BasePriceOverride
		{
			get
			{
				return this._BasePriceOverride;
			}
			set
			{
				this._BasePriceOverride = value;
			}
		}
		#endregion
		#region Commissionable
		public abstract class commissionable : PX.Data.BQL.BqlBool.Field<commissionable> { }
		protected Boolean? _Commissionable;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Subject to Commission", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? Commissionable
		{
			get
			{
				return this._Commissionable;
			}
			set
			{
				this._Commissionable = value;
			}
		}
		#endregion
		#region ABCCodeOverride
		public abstract class aBCCodeOverride : PX.Data.BQL.BqlBool.Field<aBCCodeOverride> { }
		protected Boolean? _ABCCodeOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "ABC Code Override")]
		public virtual Boolean? ABCCodeOverride
		{
			get
			{
				return this._ABCCodeOverride;
			}
			set
			{
				this._ABCCodeOverride = value;
			}
		}
		#endregion
		#region ABCCodeID
		public abstract class aBCCodeID : PX.Data.BQL.BqlString.Field<aBCCodeID> { }
		protected String _ABCCodeID;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "ABC Code", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(INABCCode.aBCCodeID), DescriptionField = typeof(INABCCode.descr))]
		public virtual String ABCCodeID
		{
			get
			{
				return this._ABCCodeID;
			}
			set
			{
				this._ABCCodeID = value;
			}
		}
		#endregion
		#region ABCCodeIsFixed
		public abstract class aBCCodeIsFixed : PX.Data.BQL.BqlBool.Field<aBCCodeIsFixed> { }
		protected Boolean? _ABCCodeIsFixed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Fixed ABC Code")]
		public virtual Boolean? ABCCodeIsFixed
		{
			get
			{
				return this._ABCCodeIsFixed;
			}
			set
			{
				this._ABCCodeIsFixed = value;
			}
		}
		#endregion
		#region MovementClassOverride
		public abstract class movementClassOverride : PX.Data.BQL.BqlBool.Field<movementClassOverride> { }
		protected Boolean? _MovementClassOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Movement Class Override")]
		public virtual Boolean? MovementClassOverride
		{
			get
			{
				return this._MovementClassOverride;
			}
			set
			{
				this._MovementClassOverride = value;
			}
		}
		#endregion
		#region MovementClassID
		public abstract class movementClassID : PX.Data.BQL.BqlString.Field<movementClassID> { }
		protected String _MovementClassID;
		[PXDBString(1)]
		[PXUIField(DisplayName = "Movement Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(INMovementClass.movementClassID), DescriptionField = typeof(INMovementClass.descr))]
		public virtual String MovementClassID
		{
			get
			{
				return this._MovementClassID;
			}
			set
			{
				this._MovementClassID = value;
			}
		}
		#endregion
		#region PendingMovementClassID
		public abstract class pendingMovementClassID : PX.Data.BQL.BqlString.Field<movementClassID> { }
		//Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty to be documented later
		[PXDBString(1)]
		public virtual String PendingMovementClassID { get; set; }
		#endregion
		#region MovementClassIsFixed
		public abstract class movementClassIsFixed : PX.Data.BQL.BqlBool.Field<movementClassIsFixed> { }
		protected Boolean? _MovementClassIsFixed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Fixed Movement Class")]
		public virtual Boolean? MovementClassIsFixed
		{
			get
			{
				return this._MovementClassIsFixed;
			}
			set
			{
				this._MovementClassIsFixed = value;
			}
		}
		#endregion
		#region PendingMovementClassPeriodID
		public abstract class pendingMovementClassPeriodID : PX.Data.BQL.BqlString.Field<pendingMovementClassPeriodID> { }
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty to be documented later
		[FinPeriodID]
		public virtual String PendingMovementClassPeriodID { get; set; }
		#endregion
		#region PendingMovementClassUpdateDate
		public abstract class pendingMovementClassUpdateDate : PX.Data.BQL.BqlDateTime.Field<pendingMovementClassUpdateDate> { }
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty to be documented later
		[PXDBDate()]
		public virtual DateTime? PendingMovementClassUpdateDate	{ get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(DescriptionField = typeof(INItemSite.siteID),
			Selector = typeof(INItemSite.siteID))] //TODO: need calculate description from inventoryID and siteID
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
		
		#region POCreate
		public abstract class pOCreate : PX.Data.BQL.BqlBool.Field<pOCreate> { }
		protected Boolean? _POCreate;		
		[PXDBCalced(
			typeof(Switch<Case<Where<INItemSite.replenishmentSource, Equal<INReplenishmentSource.purchaseToOrder>,
														Or<INItemSite.replenishmentSource, Equal<INReplenishmentSource.dropShipToOrder>>>, boolTrue>, boolFalse>), typeof(bool))]
		[PXUIField(DisplayName = "Mark fo PO",  Enabled = false)]
		public virtual Boolean? POCreate
		{
			get
			{
				return this._POCreate;
			}
			set
			{
				this._POCreate = value ?? false;
			}
		}
		#endregion
		#region POSource
		public abstract class pOSource : PX.Data.BQL.BqlString.Field<pOSource> { }
		protected string _POSource;
		[PXDBCalced(
			typeof(Switch<Case<Where<INItemSite.replenishmentSource, Equal<INReplenishmentSource.dropShipToOrder>>, INReplenishmentSource.dropShipToOrder,
										Case<Where<INItemSite.replenishmentSource, Equal<INReplenishmentSource.purchaseToOrder>>, INReplenishmentSource.purchaseToOrder>>,
										INReplenishmentSource.none>), typeof(string))]
		public virtual string POSource
		{
			get
			{
				return this._POSource;
			}
			set
			{
				this._POSource = value ?? INReplenishmentSource.None;
			}
		}
		#endregion

		#region MarkupPct
		public abstract class markupPct : PX.Data.BQL.BqlDecimal.Field<markupPct> { }
		protected Decimal? _MarkupPct;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Markup %", Enabled = false)]
		public virtual Decimal? MarkupPct
		{
			get
			{
				return this._MarkupPct;
			}
			set
			{
				this._MarkupPct = value;
			}
		}
		#endregion
		#region MarkupPctOverride
		public abstract class markupPctOverride : PX.Data.BQL.BqlBool.Field<markupPctOverride> { }
		protected Boolean? _MarkupPctOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override Markup %")]
		public virtual Boolean? MarkupPctOverride
		{
			get
			{
				return this._MarkupPctOverride;
			}
			set
			{
				this._MarkupPctOverride = value;
			}
		}
		#endregion

		#region RecPrice
		public abstract class recPrice : PX.Data.BQL.BqlDecimal.Field<recPrice> { }
		protected Decimal? _RecPrice;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "MSRP", Enabled = false)]
		[CurySymbol(siteID: typeof(siteID))]
		public virtual Decimal? RecPrice
		{
			get
			{
				return this._RecPrice;
			}
			set
			{
				this._RecPrice = value;
			}
		}
		#endregion
		#region RecPriceOverride
		public abstract class recPriceOverride : PX.Data.BQL.BqlBool.Field<recPriceOverride> { }
		protected Boolean? _RecPriceOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override Price")]
		public virtual Boolean? RecPriceOverride
		{
			get
			{
				return this._RecPriceOverride;
			}
			set
			{
				this._RecPriceOverride = value;
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
		
		#region ReplenishmentClassID
		public abstract class replenishmentClassID : PX.Data.BQL.BqlString.Field<replenishmentClassID> { }
		protected String _ReplenishmentClassID;
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Replenishment Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<INReplenishmentClass.replenishmentClassID>), DescriptionField = typeof(INReplenishmentClass.descr))]		
		public virtual String ReplenishmentClassID
		{
			get
			{
				return this._ReplenishmentClassID;
			}
			set
			{
				this._ReplenishmentClassID = value;
			}
		}
		#endregion
		#region ReplenishmentPolicyOverride
		public abstract class replenishmentPolicyOverride : PX.Data.BQL.BqlBool.Field<replenishmentPolicyOverride> { }
		protected Boolean? _ReplenishmentPolicyOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override Replenishment Settings")]
		public virtual Boolean? ReplenishmentPolicyOverride
		{
			get
			{
				return this._ReplenishmentPolicyOverride;
			}
			set
			{
				this._ReplenishmentPolicyOverride = value;
			}
		}
		#endregion
		#region ReplenishmentPolicyID
		public abstract class replenishmentPolicyID : PX.Data.BQL.BqlString.Field<replenishmentPolicyID> { }
		protected String _ReplenishmentPolicyID;		
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Seasonality")]
		[PXSelector(typeof(Search<INReplenishmentPolicy.replenishmentPolicyID>), DescriptionField = typeof(INReplenishmentPolicy.descr))]
		public virtual String ReplenishmentPolicyID
		{
			get
			{
				return this._ReplenishmentPolicyID;
			}
			set
			{
				this._ReplenishmentPolicyID = value;
			}
		}
		#endregion		
		#region ReplenishmentSource
		public abstract class replenishmentSource : PX.Data.BQL.BqlString.Field<replenishmentSource> { }
		protected string _ReplenishmentSource;

		/// <summary>
		/// Replenishment source
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Replenishment Source", FieldClass = "InvPlanning")]
		[INReplenishmentSource.List]
		public virtual string ReplenishmentSource
		{
			get
			{
				return this._ReplenishmentSource;
			}
			set
			{
				this._ReplenishmentSource = value;
			}
		}
		#endregion
		#region ReplenishmentMethod
		public abstract class replenishmentMethod : PX.Data.BQL.BqlString.Field<replenishmentMethod> { }
		protected String _ReplenishmentMethod;
		[PXDBString(1, IsFixed = true)]		
		[PXUIField(DisplayName = "Replenishment Method", Enabled = false)]
		[INReplenishmentMethod.List]		
		public virtual String ReplenishmentMethod
		{
			get
			{
				return this._ReplenishmentMethod;
			}
			set
			{
				this._ReplenishmentMethod = value;
			}
		}
		#endregion
		#region ReplenishmentSourceSiteID
		public abstract class replenishmentSourceSiteID : PX.Data.BQL.BqlInt.Field<replenishmentSourceSiteID> { }
		protected Int32? _ReplenishmentSourceSiteID;
		[IN.Site(DisplayName = "Replenishment Warehouse", DescriptionField = typeof(INSite.descr))]		
		[PXForeignReference(typeof(FK.ReplenishmentSourceSite))]
		public virtual Int32? ReplenishmentSourceSiteID
		{
			get
			{
				return this._ReplenishmentSourceSiteID;
			}
			set
			{
				this._ReplenishmentSourceSiteID = value;
			}
		}
		#endregion
		#region MaxShelfLifeOverride
		public abstract class maxShelfLifeOverride : PX.Data.BQL.BqlBool.Field<maxShelfLifeOverride> { }
		protected Boolean? _MaxShelfLifeOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override")]
		public virtual Boolean? MaxShelfLifeOverride
		{
			get
			{
				return this._MaxShelfLifeOverride;
			}
			set
			{
				this._MaxShelfLifeOverride = value;
			}
		}
		#endregion
		#region MaxShelfLife
		public abstract class maxShelfLife : PX.Data.BQL.BqlInt.Field<maxShelfLife> { }
		protected Int32? _MaxShelfLife;
		[PXDBInt()]
		[PXUIField(DisplayName = "Max. Shelf Life (Days)")]		
		public virtual Int32? MaxShelfLife
		{
			get
			{
				return this._MaxShelfLife;
			}
			set
			{
				this._MaxShelfLife = value;
			}
		}
		#endregion
		#region LaunchDateOverride
		public abstract class launchDateOverride : PX.Data.BQL.BqlBool.Field<launchDateOverride> { }
		protected Boolean? _LaunchDateOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override")]
		public virtual Boolean? LaunchDateOverride
		{
			get
			{
				return this._LaunchDateOverride;
			}
			set
			{
				this._LaunchDateOverride = value;
			}
		}
		#endregion
		#region LaunchDate
		public abstract class launchDate : PX.Data.BQL.BqlDateTime.Field<launchDate> { }
		protected DateTime? _LaunchDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Launch Date")]
		public virtual DateTime? LaunchDate
		{
			get
			{
				return this._LaunchDate;
			}
			set
			{
				this._LaunchDate = value;
			}
		}
		#endregion
		#region TerminationDateOverride
		public abstract class terminationDateOverride : PX.Data.BQL.BqlBool.Field<terminationDateOverride> { }
		protected Boolean? _TerminationDateOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override")]
		public virtual Boolean? TerminationDateOverride
		{
			get
			{
				return this._TerminationDateOverride;
			}
			set
			{
				this._TerminationDateOverride = value;
			}
		}
		#endregion
		#region TerminationDate
		public abstract class terminationDate : PX.Data.BQL.BqlDateTime.Field<terminationDate> { }
		protected DateTime? _TerminationDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Termination Date")]
		public virtual DateTime? TerminationDate
		{
			get
			{
				return this._TerminationDate;
			}
			set
			{
				this._TerminationDate = value;
			}
		}
		#endregion
		#region ServiceLevelOverride
		public abstract class serviceLevelOverride : PX.Data.BQL.BqlBool.Field<serviceLevelOverride> { }
		protected Boolean? _ServiceLevelOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override")]
		public virtual Boolean? ServiceLevelOverride
		{
			get
			{
				return this._ServiceLevelOverride;
			}
			set
			{
				this._ServiceLevelOverride = value;
			}
		}
		#endregion
		#region ServiceLevel
		public abstract class serviceLevel : PX.Data.BQL.BqlDecimal.Field<serviceLevel> { }
		protected decimal? _ServiceLevel;
		[PXDBDecimal(6, MinValue=0.500001, MaxValue=0.999999)]
		[PXUIField(DisplayName = "Service Level", Visible =true)]		
		public virtual decimal? ServiceLevel
		{
			get
			{
				return this._ServiceLevel;
			}
			set
			{
				this._ServiceLevel = value;
			}
		}
		#endregion
		#region ServiceLevelPct
		public abstract class serviceLevelPct : PX.Data.BQL.BqlDecimal.Field<serviceLevelPct> { }
		
		[PXDecimal(4, MinValue = 50.0001, MaxValue = 99.9999)]
		[PXUIField(DisplayName = "Service Level (%)", Visible = true)]
		public virtual decimal? ServiceLevelPct
		{
			[PXDependsOnFields(typeof(serviceLevel))]
			get
			{
				return this._ServiceLevel * 100.0m;
			}
			set
			{
				this._ServiceLevel = value/100.0m;
			}
		}
		#endregion
		#region SafetyStockOverride
		public abstract class safetyStockOverride : PX.Data.BQL.BqlBool.Field<safetyStockOverride> { }
		protected Boolean? _SafetyStockOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override")]
		public virtual Boolean? SafetyStockOverride
		{
			get
			{
				return this._SafetyStockOverride;
			}
			set
			{
				this._SafetyStockOverride = value;
			}
		}
		#endregion
		#region SafetyStock
		public abstract class safetyStock : PX.Data.BQL.BqlDecimal.Field<safetyStock> { }
		protected Decimal? _SafetyStock;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Safety Stock")]		
		public virtual Decimal? SafetyStock
		{
			get
			{
				return this._SafetyStock;
			}
			set
			{
				this._SafetyStock = value;
			}
		}
		#endregion
		#region MinQtyOverride
		public abstract class minQtyOverride : PX.Data.BQL.BqlBool.Field<minQtyOverride> { }
		protected Boolean? _MinQtyOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override")]
		public virtual Boolean? MinQtyOverride
		{
			get
			{
				return this._MinQtyOverride;
			}
			set
			{
				this._MinQtyOverride = value;
			}
		}
		#endregion
		#region MinQty
		public abstract class minQty : PX.Data.BQL.BqlDecimal.Field<minQty> { }
		protected Decimal? _MinQty;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Reorder Point")]		
		public virtual Decimal? MinQty
		{
			get
			{
				return this._MinQty;
			}
			set
			{
				this._MinQty = value;
			}
		}
		#endregion
		#region MaxQtyOverride
		public abstract class maxQtyOverride : PX.Data.BQL.BqlBool.Field<maxQtyOverride> { }
		protected Boolean? _MaxQtyOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override")]
		public virtual Boolean? MaxQtyOverride
		{
			get
			{
				return this._MaxQtyOverride;
			}
			set
			{
				this._MaxQtyOverride = value;
			}
		}
		#endregion
		#region MaxQty
		public abstract class maxQty : PX.Data.BQL.BqlDecimal.Field<maxQty> { }
		protected Decimal? _MaxQty;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Max Qty.")]		
		public virtual Decimal? MaxQty
		{
			get
			{
				return this._MaxQty;
			}
			set
			{
				this._MaxQty = value;
			}
		}
		#endregion
		#region TransferERQOverride
		public abstract class transferERQOverride : PX.Data.BQL.BqlBool.Field<transferERQOverride> { }
		protected Boolean? _TransferERQOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override")]
		public virtual Boolean? TransferERQOverride
		{
			get
			{
				return this._TransferERQOverride;
			}
			set
			{
				this._TransferERQOverride = value;
			}
		}
		#endregion
		#region TransferERQ
		public abstract class transferERQ : PX.Data.BQL.BqlDecimal.Field<transferERQ> { }
		protected Decimal? _TransferERQ;
		[PXDBQuantity]		
		[PXUIField(DisplayName = "Transfer ERQ")]
		public virtual Decimal? TransferERQ
		{
			get
			{
				return this._TransferERQ;
			}
			set
			{
				this._TransferERQ = value;
			}
		}
		#endregion

		#region SubItemOverride
		public abstract class subItemOverride : PX.Data.BQL.BqlBool.Field<subItemOverride> { }
		protected Boolean? _SubItemOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override", FieldClass = SubItemAttribute.DimensionName)]
		public virtual Boolean? SubItemOverride
		{
			get
			{
				return this._SubItemOverride;
			}
			set
			{
				this._SubItemOverride = value;
			}
		}
		#endregion		

		#region SafetyStockSuggested
		public abstract class safetyStockSuggested : PX.Data.BQL.BqlDecimal.Field<safetyStockSuggested> { }
		protected Decimal? _SafetyStockSuggested;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Safety Stock Suggested", Enabled=false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? SafetyStockSuggested
		{
			get
			{
				return this._SafetyStockSuggested;
			}
			set
			{
				this._SafetyStockSuggested = value;
			}
		}
		#endregion
		#region MinQtySuggested
		public abstract class minQtySuggested : PX.Data.BQL.BqlDecimal.Field<minQtySuggested> { }
		protected Decimal? _MinQtySuggested;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Reorder Point Suggested", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? MinQtySuggested
		{
			get
			{
				return this._MinQtySuggested;
			}
			set
			{
				this._MinQtySuggested = value;
			}
		}
		#endregion
		#region MaxQtySuggested
		public abstract class maxQtySuggested : PX.Data.BQL.BqlDecimal.Field<maxQtySuggested> { }
		protected Decimal? _MaxQtySuggested;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Max Qty Suggested")]
		public virtual Decimal? MaxQtySuggested
		{
			get
			{
				return this._MaxQtySuggested;
			}
			set
			{
				this._MaxQtySuggested = value;
			}
		}
		#endregion

		#region ESSmoothingConstantL
		public abstract class eSSmoothingConstantL : PX.Data.BQL.BqlDecimal.Field<eSSmoothingConstantL> { }
		protected Decimal? _ESSmoothingConstantL;
		[PXDBDecimal(9, MinValue = 0.0, MaxValue = 1.0)]
		[PXUIField(DisplayName = "Level Smoothing Constant")]
		//[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck =PXPersistingCheck.Nothing)]
		public virtual Decimal? ESSmoothingConstantL
		{
			get
			{
				return this._ESSmoothingConstantL;
			}
			set
			{
				this._ESSmoothingConstantL = value;
			}
		}
		#endregion
		#region ESSmoothingConstantLOverride
		public abstract class eSSmoothingConstantLOverride : PX.Data.BQL.BqlBool.Field<eSSmoothingConstantLOverride> { }
		protected Boolean? _ESSmoothingConstantLOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override")]
		public virtual Boolean? ESSmoothingConstantLOverride
		{
			get
			{
				return this._ESSmoothingConstantLOverride;
			}
			set
			{
				this._ESSmoothingConstantLOverride = value;
			}
		}
		#endregion
		#region ESSmoothingConstantT
		public abstract class eSSmoothingConstantT : PX.Data.BQL.BqlDecimal.Field<eSSmoothingConstantT> { }
		protected Decimal? _ESSmoothingConstantT;
		[PXDBDecimal(9)]
		[PXUIField(DisplayName = "Trend Smoothing Constant")]
		
		public virtual Decimal? ESSmoothingConstantT
		{
			get
			{
				return this._ESSmoothingConstantT;
			}
			set
			{
				this._ESSmoothingConstantT = value;
			}
		}
		#endregion
		#region ESSmoothingConstantTOverride
		public abstract class eSSmoothingConstantTOverride : PX.Data.BQL.BqlBool.Field<eSSmoothingConstantTOverride> { }
		protected Boolean? _ESSmoothingConstantTOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override")]
		public virtual Boolean? ESSmoothingConstantTOverride
		{
			get
			{
				return this._ESSmoothingConstantTOverride;
			}
			set
			{
				this._ESSmoothingConstantTOverride = value;
			}
		}
		#endregion
		#region ESSmoothingConstantS
		public abstract class eSSmoothingConstantS : PX.Data.BQL.BqlDecimal.Field<eSSmoothingConstantS> { }
		protected Decimal? _ESSmoothingConstantS;
		[PXDBDecimal(9)]
		[PXUIField(DisplayName = "Seasonality Smoothing Constant")]		
		public virtual Decimal? ESSmoothingConstantS
		{
			get
			{
				return this._ESSmoothingConstantS;
			}
			set
			{
				this._ESSmoothingConstantS = value;
			}
		}
		#endregion
		#region ESSmoothingConstantSOverride
		public abstract class eSSmoothingConstantSOverride : PX.Data.BQL.BqlBool.Field<eSSmoothingConstantSOverride> { }
		protected Boolean? _ESSmoothingConstantSOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override")]
		public virtual Boolean? ESSmoothingConstantSOverride
		{
			get
			{
				return this._ESSmoothingConstantSOverride;
			}
			set
			{
				this._ESSmoothingConstantSOverride = value;
			}
		}
		#endregion
		#region AutoFitModel
		public abstract class autoFitModel : PX.Data.BQL.BqlBool.Field<autoFitModel> { }
		protected Boolean? _AutoFitModel;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Auto Fit Model", Visibility = PXUIVisibility.Invisible)]
		public virtual Boolean? AutoFitModel
		{
			get
			{
				return this._AutoFitModel;
			}
			set
			{
				this._AutoFitModel = value;
			}
		}
		#endregion

		#region DemandPerDayAverage
		public abstract class demandPerDayAverage : PX.Data.BQL.BqlDecimal.Field<demandPerDayAverage> { }
		protected Decimal? _DemandPerDayAverage;
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Daily Demand Forecast", Enabled=false)]
		public virtual Decimal? DemandPerDayAverage
		{
			get
			{
				return this._DemandPerDayAverage;
			}
			set
			{
				this._DemandPerDayAverage = value;
			}
		}
		#endregion
		#region DemandPerDayMSE
		public abstract class demandPerDayMSE : PX.Data.BQL.BqlDecimal.Field<demandPerDayMSE> { }
		protected Decimal? _DemandPerDayMSE;
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Daily Demand Forecast Error(MSE)", Enabled=false)]
		public virtual Decimal? DemandPerDayMSE
		{
			get
			{
				return this._DemandPerDayMSE;
			}
			set
			{
				this._DemandPerDayMSE = value;
			}
		}
		#endregion
		#region DemandPerDayMAD
		public abstract class demandPerDayMAD : PX.Data.BQL.BqlDecimal.Field<demandPerDayMAD> { }
		protected Decimal? _DemandPerDayMAD;
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Daily Forecast Error(MAD)",Enabled = false)]
		public virtual Decimal? DemandPerDayMAD
		{
			get
			{
				return this._DemandPerDayMAD;
			}
			set
			{
				this._DemandPerDayMAD = value;
			}
		}
		#endregion
		#region DemandPerDaySTDEV
		public abstract class demandPerDaySTDEV : PX.Data.BQL.BqlDecimal.Field<demandPerDaySTDEV> { }
		[PXDecimal(6)]
		[PXUIField(DisplayName = "Daily Demand Forecast Error(STDEV)",Enabled = false)]		
		public virtual Decimal? DemandPerDaySTDEV
		{
			[PXDependsOnFields(typeof(demandPerDayMSE))]
			get
			{
				return this._DemandPerDayMSE.HasValue? (Decimal)Math.Sqrt((double)this._DemandPerDayMSE.Value): this._DemandPerDayMSE;				
			}
			set
			{
				
			}
		}
		#endregion

		#region LeadTimeAverage
		public abstract class leadTimeAverage : PX.Data.BQL.BqlDecimal.Field<leadTimeAverage> { }
		protected Decimal? _LeadTimeAverage;
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Lead Time Average")]
		public virtual Decimal? LeadTimeAverage
		{
			get
			{
				return this._LeadTimeAverage;
			}
			set
			{
				this._LeadTimeAverage = value;
			}
		}
		#endregion
		#region LeadTimeMSE
		public abstract class leadTimeMSE : PX.Data.BQL.BqlDecimal.Field<leadTimeMSE> { }
		protected Decimal? _LeadTimeMSE;
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Lead Time Deviation")]
		public virtual Decimal? LeadTimeMSE
		{
			get
			{
				return this._LeadTimeMSE;
			}
			set
			{
				this._LeadTimeMSE = value;
			}
		}
		#endregion
		#region LeadTimeSTDEV
		public abstract class leadTimeSTDEV : PX.Data.BQL.BqlDecimal.Field<leadTimeSTDEV> { }
		
		[PXDecimal(6)]
		[PXUIField(DisplayName = "Lead Time STDEV")]	
		public virtual Decimal? LeadTimeSTDEV
		{
			[PXDependsOnFields(typeof(leadTimeMSE))]
			get
			{

				return this._LeadTimeMSE.HasValue ? (Decimal)Math.Sqrt((double)this._LeadTimeMSE.Value) : this._LeadTimeMSE;
			}
			set
			{
		
			}
		}
		#endregion

		#region LastForecastDate
		public abstract class lastForecastDate : PX.Data.BQL.BqlDateTime.Field<lastForecastDate> { }
		protected DateTime? _LastForecastDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Last Forecast Date", Enabled = false)]
		public virtual DateTime? LastForecastDate
		{
			get
			{
				return this._LastForecastDate;
			}
			set
			{
				this._LastForecastDate = value;
			}
		}
		#endregion

		#region ForecastModelType
		public abstract class forecastModelType : PX.Data.BQL.BqlString.Field<forecastModelType> { }
		protected String _ForecastModelType;
		[PXDBString(3, IsFixed = true)]
		[DemandForecastModelType.List()]
		[PXUIField(DisplayName = "Demand Forecast Model Used")]
		[PXFormula(typeof(Default<INItemRep.forecastModelType>))]
		public virtual String ForecastModelType
		{
			get
			{
				return this._ForecastModelType;
			}
			set
			{
				this._ForecastModelType = value;
			}
		}
		#endregion
		#region ForecastPeriodType
		public abstract class forecastPeriodType : PX.Data.BQL.BqlString.Field<forecastPeriodType> { }
		protected String _ForecastPeriodType;
		[PXDBString(2, IsFixed = true)]
		[DemandPeriodType.List()]
		[PXUIField(DisplayName = "Forecast Period Type Used")]
		public virtual String ForecastPeriodType
		{
			get
			{
				return this._ForecastPeriodType;
			}
			set
			{
				this._ForecastPeriodType = value;
			}
		}
		#endregion
		#region LastForecastDate
		public abstract class lastFCApplicationDate : PX.Data.BQL.BqlDateTime.Field<lastFCApplicationDate> { }
		protected DateTime? _LastFCApplicationDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Last Forecast Results Application Date", Enabled = false)]
		public virtual DateTime? LastFCApplicationDate
		{
			get
			{
				return this._LastFCApplicationDate;
			}
			set
			{
				this._LastFCApplicationDate = value;
			}
		}
		#endregion

		#region CountryOfOrigin
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Country Of Origin")]
		[Country]
		[PXDefault(typeof(Search<InventoryItem.countryOfOrigin,
							Where<InventoryItem.inventoryID, Equal<Current<INItemSite.inventoryID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<InventoryItem.inventoryID>))]
		public virtual string CountryOfOrigin { get; set; }

		public abstract class countryOfOrigin : PX.Data.BQL.BqlString.Field<countryOfOrigin> { }
		#endregion

		#region PlanningMethod
		public abstract class planningMethod : Data.BQL.BqlString.Field<planningMethod> { }

		/// <summary>
		/// Planning method
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Planning Method", FieldClass = "InvPlanning")]
		[PXDefault(typeof(Search<InventoryItem.planningMethod,
							Where<InventoryItem.inventoryID, Equal<Current<INItemSite.inventoryID>>>>))]
		[INPlanningMethod.List]
		public string PlanningMethod
		{
			get;
			set;
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord, ForbidChangesOfPersistedRecords = true)]
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
}
