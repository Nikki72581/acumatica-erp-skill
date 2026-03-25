# Acumatica 25R1 DAC Source Files

**Version**: 25R1 (Build 25.101.0153.8)

## Files Included

DAC source files for 25R1 have not yet been extracted into this folder.
Field mappings are documented in `dac-table-map.md` based on:

- Direct SSMS inspection of the Island Parts & Supplies tenant (SaaS-hosted, 25R1)
- Local 25R1 demo instance verification
- Confirmed gotchas from live deployment

When 25R1 DAC source files become available, add them to this folder following the same structure as 25R2.

## Known 25R1 Field Behavior (from live deployment)

| Table | Field | Behavior |
|-------|-------|----------|
| `INItemSite` | `LastCost` | NO physical SQL column — computed at runtime from `INCostStatus` |
| `INItemSite` | `LastCostDate` | NO physical SQL column |
| `INItemSite` | `MinQty` | Physical column — UI label is "Reorder Point" (not MinQty) |
| `INPIDetail` | `SiteID` | NOT reliably present as physical column — use `INPIHeader.SiteID` via join |
| `INTran` | `DocType` | Physical column — same values as TranType |
| `INTran` | `TranType` | Physical column — same values as DocType |
| `INSiteStatus` | PK | Composite: `(InventoryID, SubItemID, SiteID)` — aggregate before joining |