# Acumatica 25R1 DAC Source Files

**Version**: 25R1 (Build 25.101.0153.8)

## Files Included

DAC source files for 25R1 have not yet been extracted into this folder.
The 25R1 field mappings are documented in `dac-table-map.md` based on:
- Direct SSMS inspection of the Island Parts & Supplies tenant (SaaS-hosted, 25R1)
- Local 25R1 demo instance verification
- Confirmed gotchas from live deployment (LastCost virtual field, phantom rows, SubItemID multiplication)

When 25R1 DAC source files become available, add them here following the same structure as the 25R2 folder.

## Known 25R1 Specifics (from live deployment)
- `INItemSite.LastCost` — NO physical SQL column; computed at DAC runtime from `INCostStatus`
- `INItemSite.LastCostDate` — NO physical SQL column
- `INPIDetail.SiteID` — NOT reliably present as physical column; get from `INPIHeader`
- `INTran.DocType` and `INTran.TranType` — both physical, both contain same values
- `INSiteStatus` composite PK: `(InventoryID, SubItemID, SiteID)` — aggregate before joining
