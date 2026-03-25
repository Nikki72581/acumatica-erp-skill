# Acumatica 25R2 DAC Source Files

**Version**: 25R2 (Build 25.200.xxxx / 25.201.xxxx)
**Extracted**: 2026-03-24
**Source**: AcumaticaDACs_25R21_20260324_182245.zip

## Files Included

### PX.Objects — Core ERP
- `AP/DAC/APInvoice.cs` — AP Invoice (extends APRegister)
- `AP/DAC/APRegister.cs` — Base AP document
- `AP/DAC/Vendor.cs` — Vendor master
- `AR/DAC/ARInvoice.cs` — AR Invoice (extends ARRegister)
- `AR/DAC/ARRegister.cs` — Base AR document
- `AR/DAC/ARTran.cs` — AR transaction lines
- `AR/DAC/Customer.cs` — Customer master
- `AR/DAC/Standalone/ARRegister.cs` — Standalone AR register
- `IN/DAC/INItemClass.cs` — Item class
- `IN/DAC/INItemSite.cs` — Item-warehouse settings ⚠️ Now a PXProjection joining INItemSite+INSite+INItemStats
- `IN/DAC/INPIDetail.cs` — Physical inventory detail (SiteID now a proper FK field)
- `IN/DAC/INPIHeader.cs` — Physical inventory header
- `IN/DAC/INRegister.cs` — Inventory register header
- `IN/DAC/INSite.cs` — Warehouse/site master
- `IN/DAC/INTran.cs` — Inventory transaction lines
- `IN/DAC/InventoryItem.cs` — Stock/non-stock item master
- `IN/DAC/Projections/INSiteStatus.cs` — Site status projection (uses INSiteStatusByCostCenter)
- `SO/DAC/SOLine.cs` — Sales order lines
- `SO/DAC/SOOrder.cs` — Sales order header

### PX.Commerce.Objects — Commerce/B2C
- `BC/DAC/Availability/InventoryItem.cs` — Commerce availability extension

## Key Architecture Changes vs 25R1

1. **INItemSite is now a PXProjection** joining `INItemSite + INSite + INItemStats`
   - `LastCost` → `[PXDBPriceCost(BqlField = typeof(INItemStats.lastCost))]` (was virtual in 25R1)
   - `AvgCost` → `[PXDBPriceCostCalced]` from INItemStats (was physical column direct in 25R1)
   - SQL views must still join `dbo.INItemSite` as the base table; join `dbo.INItemStats` separately for cost fields

2. **INPIDetail.SiteID** — Now a proper FK field (`[Site()]` attribute). Was unreliable in 25R1.

3. **INSiteStatus** — Now explicitly backed by `INSiteStatusByCostCenter` per the DAC source. PK structure (InventoryID, SubItemID, SiteID) unchanged.

## How to Use These Files

When writing SQL views or DACs for a 25R2 tenant, check the relevant DAC file here to verify:
- Whether a field has `[PXDBxxx]` (physical) vs `[PXxxx]` (virtual/unbound)
- Whether the DAC is a projection (check for `[PXProjection]` attribute on the class)
- What the BqlField mapping is for projected fields (tells you which table the data actually lives in)
