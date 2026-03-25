using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN.Attributes;

namespace PX.Objects.IN
{
	[PXCacheName(Messages.INSiteStatus)]
	[INSiteStatusProjection]
	public class INSiteStatus : PXBqlTable, PX.Data.IBqlTable, IStatus
	{
		#region Keys
		public class PK : PrimaryKeyOf<INSiteStatus>.By<inventoryID, subItemID, siteID>
		{
			public static INSiteStatus Find(PXGraph graph, int? inventoryID, int? subItemID, int? siteID, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, inventoryID, subItemID, siteID, options);
		}
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INSiteStatus>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INSiteStatus>.By<subItemID> { }
			public class Site : INSite.PK.ForeignKeyOf<INSiteStatus>.By<siteID> { }
			public class ItemSite : INItemSite.PK.ForeignKeyOf<INSiteStatus>.By<inventoryID, siteID> { }
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(IsKey = true, BqlField = typeof(INSiteStatusByCostCenter.inventoryID))]
		[PXDefault()]
		[ConvertedInventoryItem(true)]
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
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[SubItem(IsKey = true, BqlField = typeof(INSiteStatusByCostCenter.subItemID))]
		[PXDefault()]
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
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[Site(IsKey = true, BqlField = typeof(INSiteStatusByCostCenter.siteID))]
		[PXDefault()]
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
		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		protected Decimal? _QtyOnHand;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyOnHand))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. On Hand")]
		public virtual Decimal? QtyOnHand
		{
			get
			{
				return this._QtyOnHand;
			}
			set
			{
				this._QtyOnHand = value;
			}
		}
		#endregion
		#region QtyNotAvail
		public abstract class qtyNotAvail : PX.Data.BQL.BqlDecimal.Field<qtyNotAvail> { }
		protected Decimal? _QtyNotAvail;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyNotAvail))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Not Available")]
		public virtual Decimal? QtyNotAvail
		{
			get
			{
				return this._QtyNotAvail;
			}
			set
			{
				this._QtyNotAvail = value;
			}
		}
		#endregion
		#region QtyExpired
		public abstract class qtyExpired : PX.Data.BQL.BqlDecimal.Field<qtyExpired> { }
		protected Decimal? _QtyExpired;
		[PXDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? QtyExpired
		{
			get
			{
				return this._QtyExpired;
			}
			set
			{
				this._QtyExpired = value;
			}
		}
		#endregion
		#region QtyAvail
		public abstract class qtyAvail : PX.Data.BQL.BqlDecimal.Field<qtyAvail> { }
		protected Decimal? _QtyAvail;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyAvail))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Available")]
		public virtual Decimal? QtyAvail
		{
			get
			{
				return this._QtyAvail;
			}
			set
			{
				this._QtyAvail = value;
			}
		}
		#endregion
		#region QtyHardAvail
		public abstract class qtyHardAvail : PX.Data.BQL.BqlDecimal.Field<qtyHardAvail> { }
		protected Decimal? _QtyHardAvail;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyHardAvail))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Hard Available")]
		public virtual Decimal? QtyHardAvail
		{
			get
			{
				return this._QtyHardAvail;
			}
			set
			{
				this._QtyHardAvail = value;
			}
		}
		#endregion
		#region QtyActual
		public abstract class qtyActual : PX.Data.BQL.BqlDecimal.Field<qtyActual> { }
		protected decimal? _QtyActual;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyActual))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Available for Issue")]
		public virtual decimal? QtyActual
		{
			get { return _QtyActual; }
			set { _QtyActual = value; }
		}
		#endregion
		#region QtyInTransit
		public abstract class qtyInTransit : PX.Data.BQL.BqlDecimal.Field<qtyInTransit> { }
		protected Decimal? _QtyInTransit;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyInTransit))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. In-Transit")]
		public virtual Decimal? QtyInTransit
		{
			get
			{
				return this._QtyInTransit;
			}
			set
			{
				this._QtyInTransit = value;
			}
		}
		#endregion
        #region QtyInTransitToSO
        public abstract class qtyInTransitToSO : PX.Data.BQL.BqlDecimal.Field<qtyInTransitToSO> { }
        protected Decimal? _QtyInTransitToSO;
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyInTransitToSO))]
        [PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. In Transit to SO")]
		public virtual Decimal? QtyInTransitToSO
        {
            get
            {
                return this._QtyInTransitToSO;
            }
            set
            {
                this._QtyInTransitToSO = value;
            }
        }
        #endregion
		#region QtyPOPrepared
		public abstract class qtyPOPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOPrepared> { }
		protected Decimal? _QtyPOPrepared;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyPOPrepared))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. PO Prepared")]
		public virtual Decimal? QtyPOPrepared
		{
			get
			{
				return this._QtyPOPrepared;
			}
			set
			{
				this._QtyPOPrepared = value;
			}
		}
		#endregion
		#region QtyPOOrders
		public abstract class qtyPOOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOOrders> { }
		protected Decimal? _QtyPOOrders;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyPOOrders))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Purchase Orders")]
		public virtual Decimal? QtyPOOrders
		{
			get
			{
				return this._QtyPOOrders;
			}
			set
			{
				this._QtyPOOrders = value;
			}
		}
		#endregion
		#region QtyPOReceipts
		public abstract class qtyPOReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPOReceipts> { }
		protected Decimal? _QtyPOReceipts;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyPOReceipts))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Purchase Receipts")]
		public virtual Decimal? QtyPOReceipts
		{
			get
			{
				return this._QtyPOReceipts;
			}
			set
			{
				this._QtyPOReceipts = value;
			}
		}
		#endregion

        #region QtyFSSrvOrdBooked
        public abstract class qtyFSSrvOrdBooked : PX.Data.BQL.BqlDecimal.Field<qtyFSSrvOrdBooked> { }
        protected Decimal? _QtyFSSrvOrdBooked;
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyFSSrvOrdBooked))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty. FS Booked", FieldClass = "SERVICEMANAGEMENT")]
        public virtual Decimal? QtyFSSrvOrdBooked
        {
            get
            {
                return this._QtyFSSrvOrdBooked;
            }
            set
            {
                this._QtyFSSrvOrdBooked = value;
            }
        }
        #endregion
        #region QtyFSSrvOrdAllocated
        public abstract class qtyFSSrvOrdAllocated : PX.Data.BQL.BqlDecimal.Field<qtyFSSrvOrdAllocated> { }
        protected Decimal? _QtyFSSrvOrdAllocated;
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyFSSrvOrdAllocated))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty. FS Allocated", FieldClass = "SERVICEMANAGEMENT")]
        public virtual Decimal? QtyFSSrvOrdAllocated
        {
            get
            {
                return this._QtyFSSrvOrdAllocated;
            }
            set
            {
                this._QtyFSSrvOrdAllocated = value;
            }
        }
        #endregion
        #region QtyFSSrvOrdPrepared
        public abstract class qtyFSSrvOrdPrepared : PX.Data.BQL.BqlDecimal.Field<qtyFSSrvOrdPrepared> { }
        protected Decimal? _QtyFSSrvOrdPrepared;
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyFSSrvOrdPrepared))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty. FS Prepared", FieldClass = "SERVICEMANAGEMENT")]
        public virtual Decimal? QtyFSSrvOrdPrepared
        {
            get
            {
                return this._QtyFSSrvOrdPrepared;
            }
            set
            {
                this._QtyFSSrvOrdPrepared = value;
            }
        }
        #endregion

		#region QtySOBackOrdered
		public abstract class qtySOBackOrdered : PX.Data.BQL.BqlDecimal.Field<qtySOBackOrdered> { }
		protected Decimal? _QtySOBackOrdered;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtySOBackOrdered))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. SO Backordered")]
		public virtual Decimal? QtySOBackOrdered
		{
			get
			{
				return this._QtySOBackOrdered;
			}
			set
			{
				this._QtySOBackOrdered = value;
			}
		}
		#endregion
		#region QtySOPrepared
		public abstract class qtySOPrepared : PX.Data.BQL.BqlDecimal.Field<qtySOPrepared> { }
		protected Decimal? _QtySOPrepared;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtySOPrepared))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. SO Prepared")]
		public virtual Decimal? QtySOPrepared
		{
			get
			{
				return this._QtySOPrepared;
			}
			set
			{
				this._QtySOPrepared = value;
			}
		}
		#endregion
		#region QtySOBooked
		public abstract class qtySOBooked : PX.Data.BQL.BqlDecimal.Field<qtySOBooked> { }
		protected Decimal? _QtySOBooked;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtySOBooked))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. SO Booked")]
		public virtual Decimal? QtySOBooked
		{
			get
			{
				return this._QtySOBooked;
			}
			set
			{
				this._QtySOBooked = value;
			}
		}
		#endregion
		#region QtySOShipped
		public abstract class qtySOShipped : PX.Data.BQL.BqlDecimal.Field<qtySOShipped> { }
		protected Decimal? _QtySOShipped;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtySOShipped))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. SO Shipped")]
		public virtual Decimal? QtySOShipped
		{
			get
			{
				return this._QtySOShipped;
			}
			set
			{
				this._QtySOShipped = value;
			}
		}
		#endregion
		#region QtySOShipping
		public abstract class qtySOShipping : PX.Data.BQL.BqlDecimal.Field<qtySOShipping> { }
		protected Decimal? _QtySOShipping;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtySOShipping))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. SO Shipping")]
		public virtual Decimal? QtySOShipping
		{
			get
			{
				return this._QtySOShipping;
			}
			set
			{
				this._QtySOShipping = value;
			}
		}
		#endregion

		#region QtyMLPrepared
		public abstract class qtyMLPrepared : Data.BQL.BqlDecimal.Field<qtyMLPrepared> { }
		/// <inheritdoc cref="INSiteStatusByCostCenter.QtyMLPrepared"/>.
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyMLPrepared))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.QtyMLPrepared)]
		public virtual decimal? QtyMLPrepared { get; set; }
		#endregion
		#region QtyMLBooked
		public abstract class qtyMLBooked : Data.BQL.BqlDecimal.Field<qtyMLBooked> { }
		/// <inheritdoc cref="INSiteStatusByCostCenter.QtyMLBooked"/>.
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyMLBooked))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.QtyMLBooked)]
		public virtual decimal? QtyMLBooked { get; set; }
		#endregion
		#region QtyMLDispatched
		public abstract class qtyMLDispatched : Data.BQL.BqlDecimal.Field<qtyMLDispatched> { }
		/// <inheritdoc cref="INSiteStatusByCostCenter.QtyMLDispatched"/>.
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyMLDispatched))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.QtyMLDispatched)]
		public virtual decimal? QtyMLDispatched { get; set; }
		#endregion
		#region QtyMLAllocated
		public abstract class qtyMLAllocated : Data.BQL.BqlDecimal.Field<qtyMLAllocated> { }
		/// <inheritdoc cref="INSiteStatusByCostCenter.QtyMLAllocated"/>.
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyMLAllocated))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.QtyMLAllocated)]
		public virtual decimal? QtyMLAllocated { get; set; }
		#endregion

		#region QtyINIssues
		public abstract class qtyINIssues : PX.Data.BQL.BqlDecimal.Field<qtyINIssues> { }
		protected Decimal? _QtyINIssues;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyINIssues))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Inventory Issues")]
		public virtual Decimal? QtyINIssues
		{
			get
			{
				return this._QtyINIssues;
			}
			set
			{
				this._QtyINIssues = value;
			}
		}
		#endregion
		#region QtyINReceipts
		public abstract class qtyINReceipts : PX.Data.BQL.BqlDecimal.Field<qtyINReceipts> { }
		protected Decimal? _QtyINReceipts;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyINReceipts))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Inventory Receipts")]
		public virtual Decimal? QtyINReceipts
		{
			get
			{
				return this._QtyINReceipts;
			}
			set
			{
				this._QtyINReceipts = value;
			}
		}
		#endregion
		#region QtyINAssemblyDemand
		public abstract class qtyINAssemblyDemand : PX.Data.BQL.BqlDecimal.Field<qtyINAssemblyDemand> { }
		protected Decimal? _QtyINAssemblyDemand;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyINAssemblyDemand))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty Demanded by Kit Assembly")]
		public virtual Decimal? QtyINAssemblyDemand
		{
			get
			{
				return this._QtyINAssemblyDemand;
			}
			set
			{
				this._QtyINAssemblyDemand = value;
			}
		}
		#endregion
		#region QtyINAssemblySupply
		public abstract class qtyINAssemblySupply : PX.Data.BQL.BqlDecimal.Field<qtyINAssemblySupply> { }
		protected Decimal? _QtyINAssemblySupply;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyINAssemblySupply))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Kit Assembly")]
		public virtual Decimal? QtyINAssemblySupply
		{
			get
			{
				return this._QtyINAssemblySupply;
			}
			set
			{
				this._QtyINAssemblySupply = value;
			}
		}
        #endregion
        #region QtyInTransitToProduction
        public abstract class qtyInTransitToProduction : PX.Data.BQL.BqlDecimal.Field<qtyInTransitToProduction> { }
        protected Decimal? _QtyInTransitToProduction;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity In Transit to Production.  
        /// </summary>
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyInTransitToProduction))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty In Transit to Production")]
        public virtual Decimal? QtyInTransitToProduction
        {
            get
            {
                return this._QtyInTransitToProduction;
            }
            set
            {
                this._QtyInTransitToProduction = value;
            }
        }
        #endregion
        #region QtyProductionSupplyPrepared
        public abstract class qtyProductionSupplyPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProductionSupplyPrepared> { }
        protected Decimal? _QtyProductionSupplyPrepared;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity Production Supply Prepared.  
        /// </summary>
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyProductionSupplyPrepared))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty Production Supply Prepared")]
        public virtual Decimal? QtyProductionSupplyPrepared
        {
            get
            {
                return this._QtyProductionSupplyPrepared;
            }
            set
            {
                this._QtyProductionSupplyPrepared = value;
            }
        }
        #endregion
        #region QtyProductionSupply
        public abstract class qtyProductionSupply : PX.Data.BQL.BqlDecimal.Field<qtyProductionSupply> { }
        protected Decimal? _QtyProductionSupply;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity Production Supply.  
        /// </summary>
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyProductionSupply))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Production Supply")]
        public virtual Decimal? QtyProductionSupply
        {
            get
            {
                return this._QtyProductionSupply;
            }
            set
            {
                this._QtyProductionSupply = value;
            }
        }
        #endregion
        #region QtyPOFixedProductionPrepared
        public abstract class qtyPOFixedProductionPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedProductionPrepared> { }
        protected Decimal? _QtyPOFixedProductionPrepared;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Purchase for Prod. Prepared.  
        /// </summary>
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyPOFixedProductionPrepared))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Purchase for Prod. Prepared")]
        public virtual Decimal? QtyPOFixedProductionPrepared
        {
            get
            {
                return this._QtyPOFixedProductionPrepared;
            }
            set
            {
                this._QtyPOFixedProductionPrepared = value;
            }
        }
        #endregion
        #region QtyPOFixedProductionOrders
        public abstract class qtyPOFixedProductionOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedProductionOrders> { }
        protected Decimal? _QtyPOFixedProductionOrders;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Purchase for Production.  
        /// </summary>
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyPOFixedProductionOrders))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Purchase for Production")]
        public virtual Decimal? QtyPOFixedProductionOrders
        {
            get
            {
                return this._QtyPOFixedProductionOrders;
            }
            set
            {
                this._QtyPOFixedProductionOrders = value;
            }
        }
        #endregion
        #region QtyProductionDemandPrepared
        public abstract class qtyProductionDemandPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProductionDemandPrepared> { }
        protected Decimal? _QtyProductionDemandPrepared;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production Demand Prepared.  
        /// </summary>
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyProductionDemandPrepared))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Production Demand Prepared")]
        public virtual Decimal? QtyProductionDemandPrepared
        {
            get
            {
                return this._QtyProductionDemandPrepared;
            }
            set
            {
                this._QtyProductionDemandPrepared = value;
            }
        }
        #endregion
        #region QtyProductionDemand
        public abstract class qtyProductionDemand : PX.Data.BQL.BqlDecimal.Field<qtyProductionDemand> { }
        protected Decimal? _QtyProductionDemand;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production Demand.  
        /// </summary>
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyProductionDemand))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Production Demand")]
        public virtual Decimal? QtyProductionDemand
        {
            get
            {
                return this._QtyProductionDemand;
            }
            set
            {
                this._QtyProductionDemand = value;
            }
        }
        #endregion
        #region QtyProductionAllocated
        public abstract class qtyProductionAllocated : PX.Data.BQL.BqlDecimal.Field<qtyProductionAllocated> { }
        protected Decimal? _QtyProductionAllocated;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production Allocated.  
        /// </summary>
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyProductionAllocated))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Production Allocated")]
        public virtual Decimal? QtyProductionAllocated
        {
            get
            {
                return this._QtyProductionAllocated;
            }
            set
            {
                this._QtyProductionAllocated = value;
            }
        }
        #endregion
        #region QtySOFixedProduction
        public abstract class qtySOFixedProduction : PX.Data.BQL.BqlDecimal.Field<qtySOFixedProduction> { }
        protected Decimal? _QtySOFixedProduction;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On SO to Production.  
        /// </summary>
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtySOFixedProduction))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On SO to Production")]
        public virtual Decimal? QtySOFixedProduction
        {
            get
            {
                return this._QtySOFixedProduction;
            }
            set
            {
                this._QtySOFixedProduction = value;
            }
        }
        #endregion
        #region QtyProdFixedPurchase
        // M9
        public abstract class qtyProdFixedPurchase : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedPurchase> { }
        protected Decimal? _QtyProdFixedPurchase;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production to Purchase.  
        /// </summary>
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyProdFixedPurchase))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Production to Purchase", Enabled = false)]
        public virtual Decimal? QtyProdFixedPurchase
        {
            get
            {
                return this._QtyProdFixedPurchase;
            }
            set
            {
                this._QtyProdFixedPurchase = value;
            }
        }
        #endregion
        #region QtyProdFixedProduction
        // MA
        public abstract class qtyProdFixedProduction : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedProduction> { }
        protected Decimal? _QtyProdFixedProduction;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production to Production
        /// </summary>
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyProdFixedProduction))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Production to Production", Enabled = false)]
        public virtual Decimal? QtyProdFixedProduction
        {
            get
            {
                return this._QtyProdFixedProduction;
            }
            set
            {
                this._QtyProdFixedProduction = value;
            }
        }
        #endregion
        #region QtyProdFixedProdOrdersPrepared
        // MB
        public abstract class qtyProdFixedProdOrdersPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedProdOrdersPrepared> { }
        protected Decimal? _QtyProdFixedProdOrdersPrepared;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production for Prod. Prepared
        /// </summary>
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyProdFixedProdOrdersPrepared))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Production for Prod. Prepared", Enabled = false)]
        public virtual Decimal? QtyProdFixedProdOrdersPrepared
        {
            get
            {
                return this._QtyProdFixedProdOrdersPrepared;
            }
            set
            {
                this._QtyProdFixedProdOrdersPrepared = value;
            }
        }
        #endregion
        #region QtyProdFixedProdOrders
        // MC
        public abstract class qtyProdFixedProdOrders : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedProdOrders> { }
        protected Decimal? _QtyProdFixedProdOrders;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production for Production
        /// </summary>
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyProdFixedProdOrders))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Production for Production", Enabled = false)]
        public virtual Decimal? QtyProdFixedProdOrders
        {
            get
            {
                return this._QtyProdFixedProdOrders;
            }
            set
            {
                this._QtyProdFixedProdOrders = value;
            }
        }
        #endregion
        #region QtyProdFixedSalesOrdersPrepared
        // MD
        public abstract class qtyProdFixedSalesOrdersPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedSalesOrdersPrepared> { }
        protected Decimal? _QtyProdFixedSalesOrdersPrepared;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production for SO Prepared
        /// </summary>
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyProdFixedSalesOrdersPrepared))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Production for SO Prepared", Enabled = false)]
        public virtual Decimal? QtyProdFixedSalesOrdersPrepared
        {
            get
            {
                return this._QtyProdFixedSalesOrdersPrepared;
            }
            set
            {
                this._QtyProdFixedSalesOrdersPrepared = value;
            }
        }
        #endregion
        #region QtyProdFixedSalesOrders
        // ME
        public abstract class qtyProdFixedSalesOrders : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedSalesOrders> { }
        protected Decimal? _QtyProdFixedSalesOrders;
        /// <summary>
        /// Production / Manufacturing 
        /// Specifies the quantity On Production for SO
        /// </summary>
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyProdFixedSalesOrders))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty On Production for SO Prepared", Enabled = false)]
        public virtual Decimal? QtyProdFixedSalesOrders
        {
            get
            {
                return this._QtyProdFixedSalesOrders;
            }
            set
            {
                this._QtyProdFixedSalesOrders = value;
            }
        }
        #endregion
        #region QtyINReplaned
        public abstract class qtyINReplaned : PX.Data.BQL.BqlDecimal.Field<qtyINReplaned> { }
		protected Decimal? _QtyINReplaned;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyINReplaned))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Replanned")]
		public virtual Decimal? QtyINReplaned
		{
			get
			{
				return this._QtyINReplaned;
			}
			set
			{
				this._QtyINReplaned = value;
			}
		}
		#endregion

        #region QtyFixedFSSrvOrd
        public abstract class qtyFixedFSSrvOrd : PX.Data.BQL.BqlDecimal.Field<qtyFixedFSSrvOrd> { }
        protected decimal? _QtyFixedFSSrvOrd;
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyFixedFSSrvOrd))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? QtyFixedFSSrvOrd
        {
            get
            {
                return this._QtyFixedFSSrvOrd;
            }
            set
            {
                this._QtyFixedFSSrvOrd = value;
            }
        }
        #endregion
        #region QtyPOFixedFSSrvOrd
        public abstract class qtyPOFixedFSSrvOrd : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrd> { }
        protected decimal? _QtyPOFixedFSSrvOrd;
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyPOFixedFSSrvOrd))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? QtyPOFixedFSSrvOrd
        {
            get
            {
                return this._QtyPOFixedFSSrvOrd;
            }
            set
            {
                this._QtyPOFixedFSSrvOrd = value;
            }
        }
        #endregion
        #region QtyPOFixedFSSrvOrdPrepared
        public abstract class qtyPOFixedFSSrvOrdPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrdPrepared> { }
        protected decimal? _QtyPOFixedFSSrvOrdPrepared;
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyPOFixedFSSrvOrdPrepared))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? QtyPOFixedFSSrvOrdPrepared
        {
            get
            {
                return this._QtyPOFixedFSSrvOrdPrepared;
            }
            set
            {
                this._QtyPOFixedFSSrvOrdPrepared = value;
            }
        }
        #endregion
        #region QtyPOFixedFSSrvOrdReceipts
        public abstract class qtyPOFixedFSSrvOrdReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrdReceipts> { }
        protected decimal? _QtyPOFixedFSSrvOrdReceipts;
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyPOFixedFSSrvOrdReceipts))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? QtyPOFixedFSSrvOrdReceipts
        {
            get
            {
                return this._QtyPOFixedFSSrvOrdReceipts;
            }
            set
            {
                this._QtyPOFixedFSSrvOrdReceipts = value;
            }
        }
        #endregion

		#region QtySOFixed
		public abstract class qtySOFixed : PX.Data.BQL.BqlDecimal.Field<qtySOFixed> { }
		protected decimal? _QtySOFixed;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtySOFixed))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtySOFixed
		{
			get
			{
				return this._QtySOFixed;
			}
			set
			{
				this._QtySOFixed = value;
			}
		}
		#endregion
		#region QtyPOFixedOrders
		public abstract class qtyPOFixedOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedOrders> { }
		protected decimal? _QtyPOFixedOrders;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyPOFixedOrders))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPOFixedOrders
		{
			get
			{
				return this._QtyPOFixedOrders;
			}
			set
			{
				this._QtyPOFixedOrders = value;
			}
		}
		#endregion
		#region QtyPOFixedPrepared
		public abstract class qtyPOFixedPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedPrepared> { }
		protected decimal? _QtyPOFixedPrepared;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyPOFixedPrepared))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPOFixedPrepared
		{
			get
			{
				return this._QtyPOFixedPrepared;
			}
			set
			{
				this._QtyPOFixedPrepared = value;
			}
		}
		#endregion
		#region QtyPOFixedReceipts
		public abstract class qtyPOFixedReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedReceipts> { }
		protected decimal? _QtyPOFixedReceipts;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyPOFixedReceipts))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPOFixedReceipts
		{
			get
			{
				return this._QtyPOFixedReceipts;
			}
			set
			{
				this._QtyPOFixedReceipts = value;
			}
		}
		#endregion
		#region QtyMLToPurchase
		public abstract class qtyMLToPurchase : Data.BQL.BqlDecimal.Field<qtyMLToPurchase> { }
		/// <inheritdoc cref="INSiteStatusByCostCenter.QtyMLToPurchase"/>.
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyMLToPurchase))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.QtyMLToPurchase)]
		public virtual decimal? QtyMLToPurchase { get; set; }
		#endregion
		#region QtyPurchaseForML
		public abstract class qtyPurchaseForML : Data.BQL.BqlDecimal.Field<qtyPurchaseForML> { }
		/// <inheritdoc cref="INSiteStatusByCostCenter.QtyPurchaseForML"/>.
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyPurchaseForML))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.PurchaseForML)]
		public virtual decimal? QtyPurchaseForML { get; set; }
		#endregion
		#region QtyPurchaseForMLPrepared
		public abstract class qtyPurchaseForMLPrepared : Data.BQL.BqlDecimal.Field<qtyPurchaseForMLPrepared> { }
		/// <inheritdoc cref="INSiteStatusByCostCenter.QtyPurchaseForMLPrepared"/>.
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyPurchaseForMLPrepared))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.PurchaseForMLPrepared)]
		public virtual decimal? QtyPurchaseForMLPrepared { get; set; }
		#endregion
		#region QtyReceiptsForML
		public abstract class qtyReceiptsForML : Data.BQL.BqlDecimal.Field<qtyReceiptsForML> { }
		/// <inheritdoc cref="INSiteStatusByCostCenter.QtyReceiptsForML"/>.
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyReceiptsForML))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.ReceiptsForML)]
		public virtual decimal? QtyReceiptsForML { get; set; }
		#endregion
		#region QtySODropShip
		public abstract class qtySODropShip : PX.Data.BQL.BqlDecimal.Field<qtySODropShip> { }
		protected decimal? _QtySODropShip;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtySODropShip))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtySODropShip
		{
			get
			{
				return this._QtySODropShip;
			}
			set
			{
				this._QtySODropShip = value;
			}
		}
		#endregion
		#region QtyPODropShipOrders
		public abstract class qtyPODropShipOrders : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipOrders> { }
		protected decimal? _QtyPODropShipOrders;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyPODropShipOrders))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPODropShipOrders
		{
			get
			{
				return this._QtyPODropShipOrders;
			}
			set
			{
				this._QtyPODropShipOrders = value;
			}
		}
		#endregion
		#region QtyPODropShipPrepared
		public abstract class qtyPODropShipPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipPrepared> { }
		protected decimal? _QtyPODropShipPrepared;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyPODropShipPrepared))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPODropShipPrepared
		{
			get
			{
				return this._QtyPODropShipPrepared;
			}
			set
			{
				this._QtyPODropShipPrepared = value;
			}
		}
		#endregion
		#region QtyPODropShipReceipts
		public abstract class qtyPODropShipReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipReceipts> { }
		protected decimal? _QtyPODropShipReceipts;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyPODropShipReceipts))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPODropShipReceipts
		{
			get
			{
				return this._QtyPODropShipReceipts;
			}
			set
			{
				this._QtyPODropShipReceipts = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID(BqlField = typeof(INSiteStatusByCostCenter.lastModifiedByScreenID))]
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
        [PXDBLastModifiedDateTime(BqlField = typeof(INSiteStatusByCostCenter.lastModifiedDateTime))]
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
    }
}
