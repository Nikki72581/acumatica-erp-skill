using System;
using PX.Data;
using PX.Data.BQL;

namespace PX.Commerce.Objects.Availability
{
	/// <summary>
	/// <inheritdoc cref="PX.Objects.IN.InventoryItem" />
	/// </summary>
	[PXHidden] //TODO, Remove after merge
	public class InventoryItem : PXBqlTable, IBqlTable
	{
		#region InventoryID
		/// <summary>
		/// <inheritdoc cref="PX.Objects.IN.InventoryItem.InventoryID" />
		/// </summary>
		[PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Inventory ID")]
		public virtual int? InventoryID { get; set; }
		/// <inheritdoc cref="InventoryID" />
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region InventoryCD
		/// <summary>
		/// <inheritdoc cref="PX.Objects.IN.InventoryItem.InventoryCD" />
		/// </summary>
		[PXDBString]
		[PXUIField(DisplayName = "Inventory CD")]
		public virtual string InventoryCD { get; set; }
		/// <inheritdoc cref="InventoryCD" />
		public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }
		#endregion
		#region NoteID
		/// <summary>
		/// <inheritdoc cref="PX.Objects.IN.InventoryItem.NoteID" />
		/// </summary>
		[PXDBGuid()]
		[PXUIField(DisplayName = "NoteID")]
		public virtual Guid? NoteID { get; set; }
		/// <inheritdoc cref="NoteID" />
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		#endregion
		#region TemplateItemID
		/// <summary>
		/// <inheritdoc cref="PX.Objects.IN.InventoryItem.TemplateItemID" />
		/// </summary>
		[PXDBInt()]
		[PXUIField(DisplayName = "Template Item ID")]
		public virtual int? TemplateItemID { get; set; }
		/// <inheritdoc cref="TemplateItemID" />
		public abstract class templateItemID : PX.Data.BQL.BqlInt.Field<templateItemID> { }
		#endregion

		#region Availability
		/// <summary>
		/// Specifies whether to export product availability this inventory item.
		/// </summary>
		[PXDBString(1, IsUnicode = false)]
		[PXUIField(DisplayName = "Availability")]
		[BCItemAvailabilities.ListDef]
		public virtual string Availability { get; set; }
		/// <inheritdoc cref="Availability" />
		public abstract class availability : PX.Data.BQL.BqlString.Field<availability> { }
		#endregion
		#region ExportToExternal
		/// <summary>
		/// Indicates whether to export this inventory item to external systems.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Export to External System")]
		public virtual bool? ExportToExternal { get; set; }
		/// <inheritdoc cref="ExportToExternal" />
		public abstract class exportToExternal : PX.Data.BQL.BqlBool.Field<exportToExternal> { }
		#endregion
	}

	/// <summary>
	/// Represents an inventory item that is a child of another inventory item.
	/// </summary>
	[PXHidden]
	public class ChildInventoryItem : PX.Objects.IN.InventoryItem
	{
		/// <summary>
		/// The ID of the inventory item.
		/// </summary>
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		/// <summary>
		/// Indicates if this item is a template item.
		/// </summary>
		public new abstract class isTemplate : PX.Data.BQL.BqlBool.Field<isTemplate> { }
		/// <summary>
		/// The ID of the template item this item belongs to.
		/// </summary>
		public new abstract class templateItemID : PX.Data.BQL.BqlInt.Field<templateItemID> { }
		/// <summary>
		/// Indicates if this item is a stock item.
		/// </summary>
		public new abstract class stkItem : PX.Data.BQL.BqlBool.Field<stkItem> { }
		/// <summary>
		/// The availability setting for this item.
		/// </summary>
		public new abstract class availability : PX.Data.BQL.BqlString.Field<availability> { }
		/// <summary>
		/// Indicates if this item should be exported.
		/// </summary>
		public new abstract class exportToExternal : PX.Data.BQL.BqlBool.Field<exportToExternal> { }
		/// <inheritdoc cref="PX.Objects.IN.InventoryItem.availabilityAdjustment"/>
		public new abstract class availabilityAdjustment : PX.Data.BQL.BqlDecimal.Field<availabilityAdjustment> { }
		/// <summary>
		/// The human-readability code for this item.
		/// </summary>
		public new abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }
		/// <summary>
		/// The status of the item.
		/// </summary>
		public new abstract class itemStatus : PX.Data.BQL.BqlString.Field<itemStatus> { }
		/// <summary>
		/// The NoteID for this record.
		/// </summary>
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		/// <summary>
		/// The time this record was last modified.
		/// </summary>
		public new abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		public new abstract class baseUnit : BqlString.Field<baseUnit> { }

		public new abstract class salesUnit : BqlString.Field<salesUnit> { }
	}
}
