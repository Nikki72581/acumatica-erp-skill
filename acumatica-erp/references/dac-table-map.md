# Acumatica DAC-to-Table Field Mappings

This reference contains detailed field mappings for the most commonly used Acumatica tables in SQL view and GI work. Fields are listed with their SQL column name, DAC property name, data type, and notes on usage.

> **This is a living document.** Add mappings as you encounter new tables in client work. Mark fields with ⚠️ if they've caused issues.

> **Version coverage**: This document covers **25R1**, **25R2**, and **26R1** (as encountered).
> Version-specific differences are tagged inline with `[25R1]` / `[25R2]` / `[26R1]`.
> When no version tag is present, the behavior applies to all supported versions.
> For a consolidated version changelog, see the **Version Differences Reference** section in `SKILL.md`.

---

## Table of Contents

1. [InventoryItem](#inventoryitem)
2. [INItemClass](#initemclass)
3. [INSite / INSiteStatus / INItemSite](#inventory-site-tables)
4. [INTran / INRegister](#inventory-transactions)
5. [INPIHeader / INPIDetail](#physical-inventory)
6. [SOOrder / SOLine](#sales-orders)
7. [ARInvoice / ARTran](#accounts-receivable)
8. [Customer](#customer)
9. [APInvoice / APTran](#accounts-payable)
10. [Vendor](#vendor)
11. [GLTran / Batch / Account / Sub](#general-ledger)
12. [CSAnswers (Attributes)](#attributes)
13. [NoteDoc (Attachments)](#attachments)
14. [Common Join Patterns](#common-join-patterns)
15. [Status Code Reference](#status-code-reference)

---

## InventoryItem

**Table**: `dbo.InventoryItem`  
**DAC**: `PX.Objects.IN.InventoryItem`  
**Screen**: IN202500 (Stock Items), IN202000 (Non-Stock Items)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CompanyID` | (automatic) | `int` | Multi-tenant filter — always include in joins |
| `InventoryID` | `InventoryID` | `int` | PK — internal surrogate key |
| `InventoryCD` | `InventoryCD` | `nvarchar(30)` | Natural key — what users see as "Item ID" |
| `Descr` | `Descr` | `nvarchar(255)` | Item description |
| `ItemClassID` | `ItemClassID` | `int` | FK → INItemClass.ItemClassID |
| `ItemStatus` | `ItemStatus` | `char(2)` | 'AC'=Active, 'IN'=Inactive, 'MS'=MarkedForDeletion, 'NR'=NoReceipts, 'NS'=NoSales, 'NP'=NoPurchases |
| `ItemType` | `ItemType` | `char(1)` | 'F'=Finished Good, 'M'=Component Part, etc. |
| `StkItem` | `StkItem` | `bit` | 1=Stock item, 0=Non-stock |
| `BaseUnit` | `BaseUnit` | `nvarchar(6)` | Base unit of measure |
| `SalesUnit` | `SalesUnit` | `nvarchar(6)` | Sales UOM |
| `PurchaseUnit` | `PurchaseUnit` | `nvarchar(6)` | Purchase UOM |
| `BasePrice` | `BasePrice` | `decimal(19,6)` | Default base price |
| `NoteID` | `NoteID` | `uniqueidentifier` | Links to NoteDoc for attachments, CSAnswers for attributes |
| `CreatedDateTime` | `CreatedDateTime` | `datetime` | Audit field |
| `LastModifiedDateTime` | `LastModifiedDateTime` | `datetime` | Audit field |

⚠️ **Known issue**: `ItemStatus` values are stored as short char codes, NOT the display names. Filter on 'AC', not 'Active'.


---

## INItemClass

**Table**: `dbo.INItemClass`  
**DAC**: `PX.Objects.IN.INItemClass`

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CompanyID` | (automatic) | `int` | |
| `ItemClassID` | `ItemClassID` | `int` | PK |
| `ItemClassCD` | `ItemClassCD` | `nvarchar(30)` | Natural key |
| `Descr` | `Descr` | `nvarchar(255)` | Class description |
| `StkItem` | `StkItem` | `bit` | Stock/non-stock classification |
| `ItemType` | `ItemType` | `char(1)` | Default item type for class |

**Join to InventoryItem**: `ON ic.CompanyID = ii.CompanyID AND ic.ItemClassID = ii.ItemClassID`

---

## Inventory Site Tables

### INSite (Warehouses)

**Table**: `dbo.INSite`  
**DAC**: `PX.Objects.IN.INSite`

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CompanyID` | (automatic) | `int` | |
| `SiteID` | `SiteID` | `int` | PK |
| `SiteCD` | `SiteCD` | `nvarchar(30)` | Warehouse code |
| `Descr` | `Descr` | `nvarchar(255)` | Warehouse name |

### INSiteStatus (Qty on Hand)

**Table**: `dbo.INSiteStatus` **[25R1/25R2]** / `dbo.INSiteStatusByCostCenter` **[26R1]**
**DAC**: `PX.Objects.IN.INSiteStatus`

🚨 **[26R1] Underlying table changed**: The `INSiteStatus` DAC in 26R1 is a `[INSiteStatusProjection]` where ALL fields use `BqlField = typeof(INSiteStatusByCostCenter.xxx)`. The physical SQL table is now `dbo.INSiteStatusByCostCenter`. SQL views written for 25R1/25R2 that join `dbo.INSiteStatus` must be verified on 26R1 instances — the table name may have changed. Until confirmed on a live 26R1 tenant, prefer querying `dbo.INSiteStatusByCostCenter` on 26R1.

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CompanyID` | (automatic) | `int` | |
| `InventoryID` | `InventoryID` | `int` | Composite PK part 1 |
| `SiteID` | `SiteID` | `int` | Composite PK part 2 |
| `SubItemID` | `SubItemID` | `int` | Composite PK part 3 (if sub-items enabled) |
| `QtyOnHand` | `QtyOnHand` | `decimal(25,6)` | Current on-hand quantity |
| `QtyAvail` | `QtyAvail` | `decimal(25,6)` | Available quantity |
| `QtyNotAvail` | `QtyNotAvail` | `decimal(25,6)` | Not available (allocated, in-transit, etc.) |
| `QtyHardAvail` | `QtyHardAvail` | `decimal(25,6)` | Hard-allocated qty |
| `QtyMLPrepared` | `QtyMLPrepared` | `decimal(25,6)` | **[26R1+]** ML-driven allocation: prepared qty |
| `QtyMLBooked` | `QtyMLBooked` | `decimal(25,6)` | **[26R1+]** ML-driven allocation: booked qty |
| `QtyMLDispatched` | `QtyMLDispatched` | `decimal(25,6)` | **[26R1+]** ML-driven allocation: dispatched qty |
| `QtyMLAllocated` | `QtyMLAllocated` | `decimal(25,6)` | **[26R1+]** ML-driven allocation: total allocated |
| `QtyMLToPurchase` | `QtyMLToPurchase` | `decimal(25,6)` | **[26R1+]** ML purchase planning: qty to purchase |
| `QtyPurchaseForML` | `QtyPurchaseForML` | `decimal(25,6)` | **[26R1+]** ML purchase planning: on PO |
| `QtyPurchaseForMLPrepared` | `QtyPurchaseForMLPrepared` | `decimal(25,6)` | **[26R1+]** ML purchase planning: prepared |
| `QtyReceiptsForML` | `QtyReceiptsForML` | `decimal(25,6)` | **[26R1+]** ML purchase planning: received |
| _(none)_ | `QtyExpired` | `decimal(25,6)` | **All versions** — fully unbound `[PXDecimal]`, no physical column |

⚠️ **SubItemID**: If the tenant doesn't use sub-items, this is still present but set to 0. Include it in joins if you need exact matches, or omit it and aggregate if you want item-level totals regardless of sub-item.

### INItemSite (Item-Warehouse Settings & Costs)

**Table**: `dbo.INItemSite`  
**DAC**: `PX.Objects.IN.INItemSite`  
**Screen**: IN204500 (Item Warehouse Details)

This is one of the most field-dense tables in Acumatica. It holds per-item, per-warehouse settings for costing, replenishment, ABC classification, demand forecasting, and preferred vendor defaults.

#### Key Fields

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CompanyID` | (automatic) | `int` | |
| `InventoryID` | `InventoryID` | `int` | Composite PK |
| `SiteID` | `SiteID` | `int` | Composite PK |
| `Active` | `Active` | `bit` | Whether item is active at this warehouse |
| `SiteStatus` | `SiteStatus` | `char(2)` | Status at warehouse level (can differ from InventoryItem.ItemStatus) |
| `IsDefault` | `IsDefault` | `bit` | Whether this is the item's default warehouse |

#### Cost Fields — THE CRITICAL SECTION

⚠️ **This is where SQL queries diverge from what the UI shows.** The "correct" cost to use depends on `ValMethod`.

🚨 **CRITICAL: `LastCost` and `LastCostDate` are NOT directly queryable from `dbo.INItemSite` in any version.**

- **[25R1]**: `LastCost` has no physical column anywhere on `INItemSite`. It is computed at DAC runtime from `INCostStatus`. Referencing `isite.LastCost` in a SQL view causes `Invalid column name 'LastCost'`.
- **[25R2]**: `INItemSite` DAC was restructured as a `[PXProjection]` joining `INItemSite + INSite + INItemStats`. `LastCost` now maps to `dbo.INItemStats.LastCost`. Still not on `dbo.INItemSite` directly — you must join `dbo.INItemStats` explicitly in SQL.
- **Universal rule**: Never reference `isite.LastCost` in a SQL view on any version. Use the `INCostStatus` pattern below, which works on all versions.

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `ValMethod` | `ValMethod` | `char(1)` | **Valuation method**: 'A'=Average, 'S'=Standard, 'F'=FIFO, 'P'=Specific |
| _(none)_ | `LastCost` | _(version-dependent)_ | 🚨 **[25R1]** DAC-only virtual — no physical SQL column. Computed at runtime from `INCostStatus`. **[25R2/26R1]** Physically lives on `dbo.INItemStats.LastCost` — NOT on `dbo.INItemSite`. Requires explicit `JOIN dbo.INItemStats`. See INCostStatus pattern below — works on all versions. |
| _(none)_ | `LastCostDate` | _(version-dependent)_ | 🚨 **[25R1]** DAC-only virtual. **[25R2/26R1]** `[PXDate]` unbound — no direct SQL column on any table. |
| _(varies)_ | `AvgCost` | `decimal(19,6)` | **[25R1/25R2]** Physical column on `dbo.INItemSite`. 🚨 **[26R1]** `[PXDBPriceCostCalced]` computed as `INItemStats.TotalCost / INItemStats.QtyOnHand` — **NO physical column anywhere.** Do NOT reference `isite.AvgCost` in SQL on 26R1. |
| _(varies)_ | `MinCost` | `decimal(19,6)` | **[25R1/25R2]** Physical on `dbo.INItemSite`. **[26R1]** `[PXDBPriceCost(BqlField = typeof(INItemStats.minCost))]` — physically on `dbo.INItemStats`. |
| _(varies)_ | `MaxCost` | `decimal(19,6)` | **[25R1/25R2]** Physical on `dbo.INItemSite`. **[26R1]** `[PXDBPriceCost(BqlField = typeof(INItemStats.maxCost))]` — physically on `dbo.INItemStats`. |
| _(varies)_ | `TranUnitCost` | `decimal(19,6)` | **[25R1/25R2]** Physical column on `dbo.INItemSite` (cost from last inventory transaction). 🚨 **[26R1]** `[PXDBCalced]` expression — **NOT a physical column.** Computed from StdCost/INItemStats at runtime. Do NOT reference in SQL on 26R1. |
| `StdCost` | `StdCost` | `decimal(19,6)` | Current standard cost — only meaningful when ValMethod='S' |
| `StdCostDate` | `StdCostDate` | `datetime` | Date standard cost was set |
| `StdCostOverride` | `StdCostOverride` | `bit` | Whether this warehouse overrides item-level std cost |
| `LastStdCost` | `LastStdCost` | `decimal(19,6)` | Previous standard cost (before last update) |
| `PendingStdCost` | `PendingStdCost` | `decimal(19,6)` | Staged standard cost — not yet active |
| `PendingStdCostDate` | `PendingStdCostDate` | `datetime` | Date pending std cost becomes effective |
| `PendingStdCostReset` | `PendingStdCostReset` | `bit` | Whether to reset pending on effective date |

⚠️ **How to get accurate cost in SQL — use `INCostStatus` cost layers (works on all versions):**

The recommended approach for SQL views is to derive cost from `INCostStatus`, which stores the actual cost layers Acumatica uses internally for inventory valuation. This works for ALL valuation methods (Average, Standard, FIFO, Specific) and ALL supported versions (25R1, 25R2).

```sql
-- Derive weighted average unit cost from INCostStatus cost layers.
-- Replaces any reference to INItemSite.LastCost in SQL views.
-- Works on 25R1 and 25R2.
-- CostSiteID = INSite.SiteID for warehouse-level costing (most common, confirmed on 25R1).
-- CostSiteID = INLocation.LocationID for location-level costing (rare).
LEFT JOIN (
    SELECT
        cs.CompanyID,
        cs.InventoryID,
        cs.CostSiteID,
        CASE 
            WHEN SUM(cs.QtyOnHand) > 0 
            THEN SUM(cs.TotalCost) / SUM(cs.QtyOnHand)
            ELSE 0 
        END AS UnitCost
    FROM dbo.INCostStatus cs
    WHERE cs.QtyOnHand > 0          -- only active cost layers
    GROUP BY cs.CompanyID, cs.InventoryID, cs.CostSiteID
) cost 
    ON cost.CompanyID = ii.CompanyID 
    AND cost.InventoryID = ii.InventoryID 
    AND cost.CostSiteID = ss.SiteID  -- assumes warehouse-level costing
```

**[25R2 alternative]** — if you specifically want `INItemStats.LastCost` (the value Acumatica's UI shows in 25R2):
```sql
LEFT JOIN dbo.INItemStats ist
    ON ist.CompanyID  = isite.CompanyID
    AND ist.InventoryID = isite.InventoryID
    AND ist.SiteID      = isite.SiteID
-- Then use: ist.LastCost
```
Note: `INItemStats` did not exist as a backing table for this purpose in 25R1. Do not use this join pattern on 25R1 tenants without verifying the table structure first.

**Fallback options if `INCostStatus` is not suitable:**
- `AvgCost` — **[25R1/25R2]** physical column, usually populated for average-cost companies. 🚨 **[26R1]** computed field — not a physical column, do not use in SQL.
- `TranUnitCost` — **[25R1/25R2]** physical column, cost from the last inventory transaction. 🚨 **[26R1]** computed field — not a physical column, do not use in SQL.
- `StdCost` — physical column on all versions, but only meaningful for ValMethod='S'
- `LastStdCost` — physical column, but zero/stale for non-standard-cost companies

⚠️ **Legacy cost CASE pattern (25R1/25R2 only — do NOT use on 26R1):**
```sql
-- [25R1/25R2] ONLY — AvgCost and TranUnitCost are NOT physical columns in 26R1
CASE isite.ValMethod
    WHEN 'A' THEN ISNULL(isite.AvgCost, 0)     -- Average costing
    WHEN 'S' THEN ISNULL(isite.StdCost, 0)      -- Standard costing
    ELSE ISNULL(isite.TranUnitCost, 0)           -- Fallback to last txn cost
END AS EffectiveCost
-- NOTE: Do NOT use isite.LastCost here — it does not exist in SQL on any version.
-- NOTE: Do NOT use isite.AvgCost or isite.TranUnitCost on 26R1 — they are computed.
-- For 26R1: use INCostStatus aggregation or JOIN dbo.INItemStats instead.
```


#### Price Fields

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `BasePrice` | `BasePrice` | `decimal(19,6)` | Current base selling price at this warehouse |
| `BasePriceDate` | `BasePriceDate` | `datetime` | Date base price was set |
| `BasePriceOverride` | `BasePriceOverride` | `bit` | Whether this warehouse overrides item-level base price |
| `LastBasePrice` | `LastBasePrice` | `decimal(19,6)` | Previous base price |
| `PendingBasePrice` | `PendingBasePrice` | `decimal(19,6)` | Staged price — not yet active |
| `PendingBasePriceDate` | `PendingBasePriceDate` | `datetime` | Date pending price becomes effective |
| `RecPrice` | `RecPrice` | `decimal(19,6)` | Recommended/MSRP price |
| `RecPriceOverride` | `RecPriceOverride` | `bit` | |
| `MarkupPct` | `MarkupPct` | `decimal(19,6)` | Markup percentage |
| `MarkupPctOverride` | `MarkupPctOverride` | `bit` | |
| `Commissionable` | `Commissionable` | `bit` | Whether this item earns commissions at this warehouse |

#### Location Defaults

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `DfltReceiptLocationID` | `DfltReceiptLocationID` | `int` | Default bin for receipts → FK to INLocation |
| `DfltShipLocationID` | `DfltShipLocationID` | `int` | Default bin for shipments → FK to INLocation |
| `DfltPutawayLocationID` | `DfltPutawayLocationID` | `int` | **[26R1+]** Default bin for WMS putaway → FK to INLocation |
| `DfltSalesUnit` | `DfltSalesUnit` | `nvarchar(6)` | Default sales UOM at this warehouse |
| `DfltPurchaseUnit` | `DfltPurchaseUnit` | `nvarchar(6)` | Default purchase UOM at this warehouse |

#### Preferred Vendor

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `PreferredVendorOverride` | `PreferredVendorOverride` | `bit` | Whether this warehouse overrides item-level preferred vendor |
| `PreferredVendorID` | `PreferredVendorID` | `int` | FK → Vendor.BAccountID |
| `PreferredVendorLocationID` | `PreferredVendorLocationID` | `int` | Preferred vendor location |

#### ABC Classification & Movement Class

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `ABCCodeOverride` | `ABCCodeOverride` | `bit` | Whether this warehouse overrides item-level ABC |
| `ABCCodeID` | `ABCCodeID` | `nvarchar(10)` | ABC code assignment |
| `ABCCodeIsFixed` | `ABCCodeIsFixed` | `bit` | Whether ABC code is manually fixed (won't auto-recalculate) |
| `MovementClassOverride` | `MovementClassOverride` | `bit` | |
| `MovementClassID` | `MovementClassID` | `int` | Current movement class |
| `PendingMovementClassID` | `PendingMovementClassID` | `int` | |
| `MovementClassIsFixed` | `MovementClassIsFixed` | `bit` | |

#### Replenishment & Planning

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `ReplenishmentClassID` | `ReplenishmentClassID` | `nvarchar(10)` | |
| `ReplenishmentPolicyOverride` | `ReplenishmentPolicyOverride` | `bit` | |
| `ReplenishmentPolicyID` | `ReplenishmentPolicyID` | `nvarchar(10)` | |
| `ReplenishmentSource` | `ReplenishmentSource` | `char(1)` | P=Purchased, M=Manufactured, T=Transfer |
| `ReplenishmentMethod` | `ReplenishmentMethod` | `char(1)` | |
| `ReplenishmentSourceSiteID` | `ReplenishmentSourceSiteID` | `int` | Source warehouse for transfers |
| `SafetyStockOverride` | `SafetyStockOverride` | `bit` | |
| `SafetyStock` | `SafetyStock` | `decimal(25,6)` | |
| `SafetyStockSuggested` | `SafetyStockSuggested` | `decimal(25,6)` | System-calculated suggestion |
| `MinQtyOverride` | `MinQtyOverride` | `bit` | |
| `MinQty` | `MinQty` | `decimal(25,6)` | Reorder point |
| `MinQtySuggested` | `MinQtySuggested` | `decimal(25,6)` | |
| `MaxQtyOverride` | `MaxQtyOverride` | `bit` | |
| `MaxQty` | `MaxQty` | `decimal(25,6)` | Maximum stock level |
| `MaxQtySuggested` | `MaxQtySuggested` | `decimal(25,6)` | |
| `TransferERQOverride` | `TransferERQOverride` | `bit` | |
| `TransferERQ` | `TransferERQ` | `decimal(25,6)` | Economic reorder qty for transfers |
| `ServiceLevelOverride` | `ServiceLevelOverride` | `bit` | |
| `ServiceLevel` | `ServiceLevel` | `decimal(19,6)` | Target service level |
| `ServiceLevelPct` | `ServiceLevelPct` | `decimal(19,6)` | Service level as percentage |
| `MaxShelfLifeOverride` | `MaxShelfLifeOverride` | `bit` | |
| `MaxShelfLife` | `MaxShelfLife` | `int` | Max shelf life in days |
| `LaunchDateOverride` | `LaunchDateOverride` | `bit` | |
| `LaunchDate` | `LaunchDate` | `datetime` | Item launch date at warehouse |
| `TerminationDateOverride` | `TerminationDateOverride` | `bit` | |
| `TerminationDate` | `TerminationDate` | `datetime` | Item termination date at warehouse |
| `PlanningMethod` | `PlanningMethod` | `char(2)` | MRP planning method |

#### Demand Forecasting (Exponential Smoothing)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `AutoFitModel` | `AutoFitModel` | `bit` | Whether to auto-fit forecast model |
| `ForecastModelType` | `ForecastModelType` | `char(2)` | Forecast model in use |
| `ForecastPeriodType` | `ForecastPeriodType` | `char(1)` | D=Daily, W=Weekly, M=Monthly |
| `LastForecastDate` | `LastForecastDate` | `datetime` | |
| `DemandPerDayAverage` | `DemandPerDayAverage` | `decimal(25,6)` | Calculated avg daily demand |
| `DemandPerDayMSE` | `DemandPerDayMSE` | `decimal(25,6)` | Mean squared error |
| `DemandPerDayMAD` | `DemandPerDayMAD` | `decimal(25,6)` | Mean absolute deviation |
| _(none)_ | `DemandPerDaySTDEV` | `decimal(25,6)` | **[26R1]** Unbound `[PXDecimal]` — computed as `Math.Sqrt(DemandPerDayMSE)` in the DAC getter. **NOT a physical SQL column.** Do not reference in SQL views. |
| `LeadTimeAverage` | `LeadTimeAverage` | `decimal(25,6)` | Average lead time |
| `LeadTimeMSE` | `LeadTimeMSE` | `decimal(25,6)` | Lead time MSE |
| _(none)_ | `LeadTimeSTDEV` | `decimal(25,6)` | **[26R1]** Unbound `[PXDecimal]` — computed as `Math.Sqrt(LeadTimeMSE)`. **NOT a physical SQL column.** |
| `ESSmoothingConstantL` | `ESSmoothingConstantL` | `decimal(19,6)` | Level smoothing constant (alpha) |
| `ESSmoothingConstantLOverride` | `ESSmoothingConstantLOverride` | `bit` | |
| `ESSmoothingConstantT` | `ESSmoothingConstantT` | `decimal(19,6)` | Trend smoothing constant (beta) |
| `ESSmoothingConstantTOverride` | `ESSmoothingConstantTOverride` | `bit` | |
| `ESSmoothingConstantS` | `ESSmoothingConstantS` | `decimal(19,6)` | Seasonal smoothing constant (gamma) |
| `ESSmoothingConstantSOverride` | `ESSmoothingConstantSOverride` | `bit` | |

#### Purchase Order Creation

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| _(varies)_ | `POCreate` | `bit` | **[25R1/25R2]** Physical column. **[26R1]** `[PXDBCalced]` derived from `ReplenishmentSource` — NOT a physical column in SQL. |
| _(varies)_ | `POSource` | `char(1)` | **[25R1/25R2]** Physical column. **[26R1]** `[PXDBCalced]` derived from `ReplenishmentSource` — NOT a physical column in SQL. |
| `SubItemOverride` | `SubItemOverride` | `bit` | |

#### GL Override

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `OverrideInvtAcctSub` | `OverrideInvtAcctSub` | `bit` | Whether this warehouse overrides item-level GL accounts |
| `InvtAcctID` | `InvtAcctID` | `int` | Inventory GL account → FK to Account |
| `InvtSubID` | `InvtSubID` | `int` | Inventory GL subaccount → FK to Sub |

#### Other

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `ProductManagerOverride` | `ProductManagerOverride` | `bit` | |
| `ProductWorkgroupID` | `ProductWorkgroupID` | `int` | |
| `ProductManagerID` | `ProductManagerID` | `int` | |
| `PriceWorkgroupID` | `PriceWorkgroupID` | `int` | |
| `PriceManagerID` | `PriceManagerID` | `int` | |
| `CountryOfOrigin` | `CountryOfOrigin` | `nvarchar(2)` | ISO country code |
| `NoteID` | `NoteID` | `uniqueidentifier` | For attachments/attributes |

#### The Override Pattern

INItemSite uses a pervasive `*Override` + `*Value` pattern. For nearly every configurable field, there's a boolean `[FieldName]Override` that indicates whether the warehouse-level value should be used instead of the item-level default. **In SQL, you see the raw warehouse value regardless of the override flag.** The override flag is only respected by the DAC/BQL layer in the application. This means:

```sql
-- What the application does (respects override):
-- IF ABCCodeOverride = 1 THEN use INItemSite.ABCCodeID
-- ELSE use InventoryItem.ABCCodeID (or item class default)

-- What your SQL view must do manually:
CASE WHEN isite.ABCCodeOverride = 1 
     THEN isite.ABCCodeID 
     ELSE ii.ABCCodeID    -- or whatever the parent default source is
END AS EffectiveABCCode
```

⚠️ **This is a major source of SQL-vs-UI discrepancy.** If your GI shows different values than a raw SQL query, check whether the field has an Override flag and whether the SQL is respecting it.

---

## Inventory Transactions

### INTran (Transaction Lines)

**Table**: `dbo.INTran`  
**DAC**: `PX.Objects.IN.INTran`

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CompanyID` | (automatic) | `int` | |
| `DocType` | `DocType` | `char(1)` | Physical column (PK part 1). 1-char document type matching `INRegister.DocType`. Use this for joining to `INRegister`. |
| `TranType` | `TranType` | `char(3)` | Also a physical column — 3-char line-level transaction type code (e.g., `ISS`=Issue, `RCP`=Receipt, `TFR`=Transfer). Different values from `DocType`. |
| `RefNbr` | `RefNbr` | `nvarchar(15)` | Document reference number (PK part 2) |
| `LineNbr` | `LineNbr` | `int` | Line number within document (PK part 3) |
| `InventoryID` | `InventoryID` | `int` | FK → InventoryItem |
| `SiteID` | `SiteID` | `int` | FK → INSite |
| `LocationID` | `LocationID` | `int` | FK → INLocation (bin) |
| `Qty` | `Qty` | `decimal(25,6)` | Transaction quantity |
| `TranAmt` | `TranAmt` | `decimal(19,4)` | Transaction amount (base currency) |
| `UnitCost` | `UnitCost` | `decimal(19,6)` | Unit cost |
| `TranDate` | `TranDate` | `datetime` | Transaction date |
| `Released` | `Released` | `bit` | 1=Released, 0=Unreleased |
| `IsSpecialOrder` | `IsSpecialOrder` | `bit` | **[26R1+]** Whether this is a special order transaction |
| `InventorySource` | `InventorySource` | `char(1)` | **[26R1+]** Material source type for project accounting |
| `CostLayerType` | `CostLayerType` | `char(1)` | **[26R1+]** Distinguishes Normal vs. Special Order cost layers |
| `CostCenterID` | `CostCenterID` | `int` | **[26R1+]** Cost center for multi-cost-center setups |

⚠️ **DocType vs TranType — CORRECTED**: `DocType` (1-char, e.g., `R`, `I`, `T`) is the document-level type, identical to `INRegister.DocType`. `TranType` (3-char, e.g., `RCP`, `ISS`, `TFR`) is the line-level transaction type. They are **not the same values**. Always join `INRegister` to `INTran` on `r.DocType = t.DocType AND r.RefNbr = t.RefNbr`. Never join on `TranType`. Confirmed via 26R1 DAC source.

### INRegister (Transaction Headers)

**Table**: `dbo.INRegister`  
**DAC**: `PX.Objects.IN.INRegister`

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CompanyID` | (automatic) | `int` | |
| `DocType` | `DocType` | `char(1)` | R=Receipt, I=Issue, T=Transfer, A=Adjustment, P=Production |
| `RefNbr` | `RefNbr` | `nvarchar(15)` | PK with DocType |
| `Status` | `Status` | `char(1)` | H=Hold, B=Balanced, R=Released |
| `Released` | `Released` | `bit` | ⚠️ Redundant with Status='R' but commonly used in filters |
| `TranDate` | `TranDate` | `datetime` | |
| `TotalQty` | `TotalQty` | `decimal(25,6)` | |
| `TotalAmount` | `TotalAmount` | `decimal(19,4)` | |

**Join INTran to INRegister**: `ON r.CompanyID = t.CompanyID AND r.DocType = t.DocType AND r.RefNbr = t.RefNbr`

⚠️ The older documented pattern `r.DocType = t.TranType` also works since both columns exist and hold the same values on 25R1 and 25R2. Using `t.DocType` is preferred for consistency.

---

## Physical Inventory

### INPIHeader

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CompanyID` | (automatic) | `int` | |
| `PIID` | `PIID` | `nvarchar(15)` | PK |
| `SiteID` | `SiteID` | `int` | Warehouse |
| `Status` | `Status` | `char(1)` | ⚠️ Verify on target tenant — likely 'N'=New, 'P'=InProgress, 'C'=Completed |
| `TotalPhysicalQty` | `TotalPhysicalQty` | `decimal` | |
| `TotalVarQty` | `TotalVarQty` | `decimal` | Total variance |

### INPIDetail

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CompanyID` | (automatic) | `int` | |
| `PIID` | `PIID` | `nvarchar(15)` | FK → INPIHeader |
| `LineNbr` | `LineNbr` | `int` | |
| `InventoryID` | `InventoryID` | `int` | |
| `LocationID` | `LocationID` | `int` | |
| `BookQty` | `BookQty` | `decimal` | System quantity at time of count |
| `PhysicalQty` | `PhysicalQty` | `decimal` | Counted quantity |
| `VarQty` | `VarQty` | `decimal` | Variance (Physical - Book) |

⚠️ **SiteID on INPIDetail — version-dependent:**
- **[25R1]**: `SiteID` is NOT reliably present as a physical column on `INPIDetail`. Get it from `INPIHeader` via join.
- **[25R2]**: `SiteID` IS a proper FK field on `INPIDetail` (confirmed via DAC source — `[Site()]` attribute with `[PXDefault(typeof(INPIHeader.siteID))]`).
- **Safe cross-version pattern**: Always join `INPIHeader` for `SiteID` regardless of version: `INNER JOIN dbo.INPIHeader h ON h.CompanyID = d.CompanyID AND h.PIID = d.PIID` and use `h.SiteID`.

---

## Sales Orders

### SOOrder

**Table**: `dbo.SOOrder`  
**DAC**: `PX.Objects.SO.SOOrder`  
**Screen**: SO301000 (Sales Orders)  
**Interfaces**: `IAssign`, `IFreightBase`, `IInvoice`, `ICreatePaymentDocument`

SOOrder is one of the most field-heavy DACs in Acumatica. It carries the full lifecycle of a sales order including approval workflow, credit management, shipping, billing, payment tracking, intercompany, and blanket order support.

#### Key / Identity Fields

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CompanyID` | (automatic) | `int` | |
| `OrderType` | `OrderType` | `char(2)` | PK part 1. 'SO'=Sales Order, 'IN'=Invoice, 'QT'=Quote, 'CM'=Credit Memo, 'RC'=Return, 'RM'=RMA |
| `OrderNbr` | `OrderNbr` | `nvarchar(15)` | PK part 2 |
| `Behavior` | `Behavior` | `char(2)` | Order type behavior — controls which tabs/features are active |
| `ARDocType` | `ARDocType` | `char(3)` | Corresponding AR document type when invoiced |
| `IsInvoiceOrder` | `IsInvoiceOrder` | `bit` | Whether this order type creates invoices directly |
| `CustomerID` | `CustomerID` | `int` | FK → Customer.BAccountID |
| `CustomerLocationID` | `CustomerLocationID` | `int` | Customer ship-to location |
| `BranchID` | `BranchID` | `int` | FK → Branch |
| `ContactID` | `ContactID` | `int` | FK → Contact |

#### Dates

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `OrderDate` | `OrderDate` | `datetime` | Order creation date |
| `CancelDate` | `CancelDate` | `datetime` | Auto-cancel date (for quotes/expiring orders) |
| `RequestDate` | `RequestDate` | `datetime` | Customer requested delivery date |
| `ShipDate` | `ShipDate` | `datetime` | Planned ship date |
| `DueDate` | `DueDate` | `datetime` | Payment due date (from terms) |
| `DiscDate` | `DiscDate` | `datetime` | Discount date (early payment) |
| `ExpireDate` | `ExpireDate` | `datetime` | Expiration date (for quotes) |
| `IsExpired` | `IsExpired` | `bit` | Calculated — whether past expire date |

#### Customer Reference

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CustomerOrderNbr` | `CustomerOrderNbr` | `nvarchar(40)` | Customer's PO number |
| `CustomerRefNbr` | `CustomerRefNbr` | `nvarchar(40)` | Additional customer reference |
| `OrderDesc` | `OrderDesc` | `nvarchar(255)` | Order description/notes |

#### Status & Workflow Flags

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `Status` | `Status` | `char(1)` | N=New, H=Hold, O=Open, C=Completed, L=Cancelled, B=Back Order |
| `Hold` | `Hold` | `bit` | On hold |
| `DontApprove` | `DontApprove` | `bit` | Skip approval workflow |
| `Approved` | `Approved` | `bit` | Approved flag |
| `Rejected` | `Rejected` | `bit` | Rejected flag |
| `CreditHold` | `CreditHold` | `bit` | On credit hold |
| `Completed` | `Completed` | `bit` | Fully completed |
| `Cancelled` | `Cancelled` | `bit` | Cancelled |
| `OpenDoc` | `OpenDoc` | `bit` | Currently open |
| `BackOrdered` | `BackOrdered` | `bit` | Has back-ordered lines |
| `Emailed` | `Emailed` | `bit` | Confirmation emailed |
| `Printed` | `Printed` | `bit` | Printed |
| `ShipmentDeleted` | `ShipmentDeleted` | `bit` | Associated shipment was deleted |

⚠️ **Status vs. boolean flags**: The `Status` field is a computed/aggregate status. The individual boolean flags (`Hold`, `Approved`, `CreditHold`, etc.) represent the actual state. In SQL views, prefer filtering on the boolean flags for precision — `Status` can lag behind in edge cases during workflow transitions.

#### Currency Amount Fields (Document Currency)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CuryID` | `CuryID` | `nvarchar(5)` | Currency code (USD, EUR, etc.) |
| `CuryInfoID` | `CuryInfoID` | `bigint` | FK → CurrencyInfo for exchange rate |
| `CuryOrderTotal` | `CuryOrderTotal` | `decimal(19,4)` | **Grand total** (doc currency) |
| `CuryLineTotal` | `CuryLineTotal` | `decimal(19,4)` | Sum of line amounts |
| `CuryTaxTotal` | `CuryTaxTotal` | `decimal(19,4)` | Total tax |
| `CuryDiscTot` | `CuryDiscTot` | `decimal(19,4)` | Total discount |
| `CuryLineDiscTotal` | `CuryLineDiscTotal` | `decimal(19,4)` | Line-level discounts |
| `CuryGroupDiscTotal` | `CuryGroupDiscTotal` | `decimal(19,4)` | Group-level discounts |
| `CuryDocumentDiscTotal` | `CuryDocumentDiscTotal` | `decimal(19,4)` | Document-level discounts |
| `CuryOrderDiscTotal` | `CuryOrderDiscTotal` | `decimal(19,4)` | Order-level discounts |
| `CuryMiscTot` | `CuryMiscTot` | `decimal(19,4)` | Miscellaneous charges |
| `CuryFreightAmt` | `CuryFreightAmt` | `decimal(19,4)` | Freight amount charged |
| `CuryFreightCost` | `CuryFreightCost` | `decimal(19,4)` | Actual freight cost |
| `CuryPremiumFreightAmt` | `CuryPremiumFreightAmt` | `decimal(19,4)` | Premium freight |
| `CuryFreightTot` | `CuryFreightTot` | `decimal(19,4)` | Total freight |
| `CuryVatExemptTotal` | `CuryVatExemptTotal` | `decimal(19,4)` | VAT exempt total |
| `CuryVatTaxableTotal` | `CuryVatTaxableTotal` | `decimal(19,4)` | VAT taxable total |
| `CuryGoodsExtPriceTotal` | `CuryGoodsExtPriceTotal` | `decimal(19,4)` | Goods extended price total |
| `CuryMiscExtPriceTotal` | `CuryMiscExtPriceTotal` | `decimal(19,4)` | Misc extended price total |
| `CuryControlTotal` | `CuryControlTotal` | `decimal(19,4)` | Manual control total |
| `CuryTermsDiscAmt` | `CuryTermsDiscAmt` | `decimal(19,4)` | Early payment discount |
| `CuryManDisc` | `CuryManDisc` | `decimal(19,4)` | Manual discount |

#### Base Currency Amounts

Every `Cury*` field has a corresponding base-currency field without the `Cury` prefix:

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `OrderTotal` | `OrderTotal` | `decimal(19,4)` | Grand total (base currency) |
| `LineTotal` | `LineTotal` | `decimal(19,4)` | |
| `TaxTotal` | `TaxTotal` | `decimal(19,4)` | |
| `DiscTot` | `DiscTot` | `decimal(19,4)` | |
| `FreightAmt` | `FreightAmt` | `decimal(19,4)` | |
| `FreightCost` | `FreightCost` | `decimal(19,4)` | |
| `MiscTot` | `MiscTot` | `decimal(19,4)` | |
| `ControlTotal` | `ControlTotal` | `decimal(19,4)` | |

⚠️ **Cury vs Base**: For multi-currency tenants, always decide upfront whether your SQL view reports in document currency or base currency. Mixing them produces meaningless totals. For cross-order aggregation (e.g., total sales by customer), use the base currency fields. For single-order display, use `Cury*` fields and show `CuryID`.

#### Open / Unbilled / Billed Tracking

These fields track the lifecycle of order amounts through shipping, billing, and payment.

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CuryOpenOrderTotal` | `CuryOpenOrderTotal` | `decimal(19,4)` | Not yet shipped/completed |
| `CuryOpenLineTotal` | `CuryOpenLineTotal` | `decimal(19,4)` | |
| `CuryOpenDiscTotal` | `CuryOpenDiscTotal` | `decimal(19,4)` | |
| `CuryOpenTaxTotal` | `CuryOpenTaxTotal` | `decimal(19,4)` | |
| `OpenOrderQty` | `OpenOrderQty` | `decimal(25,6)` | |
| `CuryUnbilledOrderTotal` | `CuryUnbilledOrderTotal` | `decimal(19,4)` | Shipped but not yet invoiced |
| `CuryUnbilledLineTotal` | `CuryUnbilledLineTotal` | `decimal(19,4)` | |
| `CuryUnbilledMiscTot` | `CuryUnbilledMiscTot` | `decimal(19,4)` | |
| `CuryUnbilledTaxTotal` | `CuryUnbilledTaxTotal` | `decimal(19,4)` | |
| `CuryUnbilledDiscTotal` | `CuryUnbilledDiscTotal` | `decimal(19,4)` | |
| `CuryUnbilledFreightTot` | `CuryUnbilledFreightTot` | `decimal(19,4)` | |
| `UnbilledOrderQty` | `UnbilledOrderQty` | `decimal(25,6)` | |
| `CuryBilledFreightTot` | `CuryBilledFreightTot` | `decimal(19,4)` | Already invoiced freight |
| `OrderQty` | `OrderQty` | `decimal(25,6)` | Total ordered quantity |

⚠️ **For "what's still open" reports**, use `CuryOpenOrderTotal` / `OpenOrderQty`. For "what hasn't been invoiced yet", use `CuryUnbilledOrderTotal` / `UnbilledOrderQty`. These are different — an order can be shipped (no longer open) but not yet billed (still unbilled).

#### Payment Tracking

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CuryPaymentTotal` | `CuryPaymentTotal` | `decimal(19,4)` | Total payments applied |
| `CuryUnreleasedPaymentAmt` | `CuryUnreleasedPaymentAmt` | `decimal(19,4)` | Payments not yet released |
| `CuryCCAuthorizedAmt` | `CuryCCAuthorizedAmt` | `decimal(19,4)` | Credit card authorized amount |
| `CuryPaidAmt` | `CuryPaidAmt` | `decimal(19,4)` | Actually paid |
| `CuryBilledPaymentTotal` | `CuryBilledPaymentTotal` | `decimal(19,4)` | Payments already billed |
| `CuryUnpaidBalance` | `CuryUnpaidBalance` | `decimal(19,4)` | Remaining unpaid |
| `CuryUnrefundedBalance` | `CuryUnrefundedBalance` | `decimal(19,4)` | For returns — not yet refunded |
| `CuryPaymentOverall` | `CuryPaymentOverall` | `decimal(19,4)` | Overall payment amount |
| `PaymentMethodID` | `PaymentMethodID` | `nvarchar(10)` | Payment method |
| `PMInstanceID` | `PMInstanceID` | `int` | Payment method instance (stored card) |
| `CashAccountID` | `CashAccountID` | `int` | Cash account for payment |
| `ExtRefNbr` | `ExtRefNbr` | `nvarchar(40)` | External payment reference |
| `PaymentCntr` | `PaymentCntr` | `int` | Count of payments |
| `AuthorizedPaymentCntr` | `AuthorizedPaymentCntr` | `int` | Count of authorized payments |
| `IsFullyPaid` | `IsFullyPaid` | `bit` | Calculated |
| `ArePaymentsApplicable` | `ArePaymentsApplicable` | `bit` | |

#### Prepayment

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `OverridePrepayment` | `OverridePrepayment` | `bit` | |
| `PrepaymentReqPct` | `PrepaymentReqPct` | `decimal(19,6)` | Required prepayment percentage |
| `CuryPrepaymentReqAmt` | `CuryPrepaymentReqAmt` | `decimal(19,4)` | Required prepayment amount |
| `PrepaymentReqSatisfied` | `PrepaymentReqSatisfied` | `bit` | Whether prepayment requirement is met |

#### Shipping & Freight

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `ShipComplete` | `ShipComplete` | `char(1)` | B=Back Order Allowed, C=Cancel Remainder, S=Ship Complete |
| `ShipVia` | `ShipVia` | `nvarchar(15)` | Carrier code |
| `FOBPoint` | `FOBPoint` | `nvarchar(15)` | FOB point |
| `ShipTermsID` | `ShipTermsID` | `nvarchar(10)` | Shipping terms |
| `ShipZoneID` | `ShipZoneID` | `nvarchar(15)` | Shipping zone |
| `WillCall` | `WillCall` | `bit` | Customer pickup |
| `Resedential` | `Resedential` | `bit` | Residential delivery (⚠️ note Acumatica's typo — "Resedential" not "Residential") |
| `SaturdayDelivery` | `SaturdayDelivery` | `bit` | |
| `GroundCollect` | `GroundCollect` | `bit` | |
| `Insurance` | `Insurance` | `bit` | |
| `UseCustomerAccount` | `UseCustomerAccount` | `bit` | Use customer's carrier account |
| `FreightCostIsValid` | `FreightCostIsValid` | `bit` | Whether freight calculation is current |
| `IsPackageValid` | `IsPackageValid` | `bit` | Whether package info is current |
| `OverrideFreightAmount` | `OverrideFreightAmount` | `bit` | Manual freight override |
| `FreightAmountSource` | `FreightAmountSource` | `char(1)` | Source of freight amount |
| `FreightTaxCategoryID` | `FreightTaxCategoryID` | `nvarchar(10)` | Tax category for freight |
| `IsManualPackage` | `IsManualPackage` | `bit` | |
| `PackageLineCntr` | `PackageLineCntr` | `int` | |
| `PackageWeight` | `PackageWeight` | `decimal(25,6)` | |
| `OrderWeight` | `OrderWeight` | `decimal(25,6)` | Total order weight |
| `OrderVolume` | `OrderVolume` | `decimal(25,6)` | Total order volume |

#### Shipment & Billing Counters

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `LineCntr` | `LineCntr` | `int` | Total line count (for auto-numbering) |
| `BilledCntr` | `BilledCntr` | `int` | Lines billed |
| `ReleasedCntr` | `ReleasedCntr` | `int` | Lines released |
| `ShipmentCntr` | `ShipmentCntr` | `int` | Total shipments created |
| `OpenShipmentCntr` | `OpenShipmentCntr` | `int` | Open (not confirmed) shipments |
| `OpenLineCntr` | `OpenLineCntr` | `int` | Open lines |
| `OpenSiteCntr` | `OpenSiteCntr` | `int` | Sites with open lines |
| `SiteCntr` | `SiteCntr` | `int` | Total sites involved |
| `BillSeparately` | `BillSeparately` | `bit` | Invoice this order separately |
| `ShipSeparately` | `ShipSeparately` | `bit` | Ship this order separately |

#### Tax

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `OverrideTaxZone` | `OverrideTaxZone` | `bit` | |
| `TaxZoneID` | `TaxZoneID` | `nvarchar(10)` | Tax zone |
| `TaxCalcMode` | `TaxCalcMode` | `char(1)` | Tax calculation mode |
| `IsTaxValid` | `IsTaxValid` | `bit` | Whether tax calculation is current |
| `IsOpenTaxValid` | `IsOpenTaxValid` | `bit` | |
| `IsUnbilledTaxValid` | `IsUnbilledTaxValid` | `bit` | |
| `ExternalTaxExemptionNumber` | `ExternalTaxExemptionNumber` | `nvarchar(30)` | For Avalara/external tax |
| `AvalaraCustomerUsageType` | `AvalaraCustomerUsageType` | `nvarchar(25)` | Avalara usage type |
| `ExternalTaxesImportInProgress` | `ExternalTaxesImportInProgress` | `bit` | |

#### Addresses & Contacts

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `BillAddressID` | `BillAddressID` | `int` | FK → SOBillingAddress |
| `ShipAddressID` | `ShipAddressID` | `int` | FK → SOShippingAddress |
| `BillContactID` | `BillContactID` | `int` | FK → SOBillingContact |
| `ShipContactID` | `ShipContactID` | `int` | FK → SOShippingContact |

#### Sales & Commission

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `SalesPersonID` | `SalesPersonID` | `int` | FK → SalesPerson |
| `CommnPct` | `CommnPct` | `decimal(19,6)` | Commission percentage |
| `TermsID` | `TermsID` | `nvarchar(10)` | Payment terms |
| `EmployeeID` | `EmployeeID` | `int` | Responsible employee |

#### Project

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `ProjectID` | `ProjectID` | `int` | FK → PMProject.ContractID |

#### Warehouse / Site Defaults

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `DefaultSiteID` | `DefaultSiteID` | `int` | Default warehouse for the order |
| `DestinationSiteID` | `DestinationSiteID` | `int` | For transfer orders |
| `LastSiteID` | `LastSiteID` | `int` | Last warehouse used |
| `LastShipDate` | `LastShipDate` | `datetime` | Last shipment date |
| `DefaultOperation` | `DefaultOperation` | `char(1)` | Default line operation type |

#### Invoicing

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `InvoiceNbr` | `InvoiceNbr` | `nvarchar(15)` | Invoice reference |
| `InvoiceDate` | `InvoiceDate` | `datetime` | Invoice date |
| `FinPeriodID` | `FinPeriodID` | `char(6)` | Financial period (YYYYMM) |
| `UpdateNextNumber` | `UpdateNextNumber` | `bit` | |

#### Original Order (for returns/credit memos)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `OrigOrderType` | `OrigOrderType` | `char(2)` | Original order type being returned |
| `OrigOrderNbr` | `OrigOrderNbr` | `nvarchar(15)` | Original order number |

#### Intercompany

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `IsIntercompany` | `IsIntercompany` | `bit` | Whether this is an intercompany order |
| `IntercompanyPOType` | `IntercompanyPOType` | `char(2)` | Linked intercompany PO type |
| `IntercompanyPONbr` | `IntercompanyPONbr` | `nvarchar(15)` | Linked intercompany PO number |
| `IntercompanyPOReturnNbr` | `IntercompanyPOReturnNbr` | `nvarchar(15)` | |

#### Blanket Orders

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `QtyOnOrders` | `QtyOnOrders` | `decimal(25,6)` | Qty on child orders |
| `BlanketOpenQty` | `BlanketOpenQty` | `decimal(25,6)` | Remaining open qty on blanket |
| `BlanketLineCntr` | `BlanketLineCntr` | `int` | |
| `MinSchedOrderDate` | `MinSchedOrderDate` | `datetime` | Earliest scheduled date |

#### Order Type Flags (Calculated/Virtual)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `IsCreditMemoOrder` | `IsCreditMemoOrder` | `bit` | Calculated from OrderType/Behavior |
| `IsRMAOrder` | `IsRMAOrder` | `bit` | |
| `IsMixedOrder` | `IsMixedOrder` | `bit` | |
| `IsTransferOrder` | `IsTransferOrder` | `bit` | |
| `IsDebitMemoOrder` | `IsDebitMemoOrder` | `bit` | |
| `IsNoAROrder` | `IsNoAROrder` | `bit` | Order type doesn't create AR docs |
| `IsCashSaleOrder` | `IsCashSaleOrder` | `bit` | |

⚠️ **These `Is*Order` fields may be virtual (calculated in C#, not stored in the DB).** If you need these in a SQL view, you'll have to replicate the logic using CASE statements against `OrderType` and `Behavior`. Check whether they have `[PXDBBool]` or `[PXBool]` — only `PXDBBool` fields exist in SQL.

#### Approval Workflow

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `WorkgroupID` | `WorkgroupID` | `int` | Approval workgroup |
| `OwnerID` | `OwnerID` | `uniqueidentifier` | Approval owner |
| `Priority` | `Priority` | `int` | Priority level |

#### Discount Control

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `ManDisc` | `ManDisc` | `decimal(19,4)` | Manual discount (base) |
| `DisableAutomaticDiscountCalculation` | `DisableAutomaticDiscountCalculation` | `bit` | |
| `DisableAutomaticTaxCalculation` | `DisableAutomaticTaxCalculation` | `bit` | |

#### Credit Approval

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `ApprovedCredit` | `ApprovedCredit` | `bit` | Credit was approved |
| `ApprovedCreditByPayment` | `ApprovedCreditByPayment` | `bit` | Credit satisfied by payment |
| `ApprovedCreditAmt` | `ApprovedCreditAmt` | `decimal(19,4)` | Approved credit amount |
| `InclCustOpenOrders` | `InclCustOpenOrders` | `bit` | Include open orders in credit check |

#### Audit

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CreatedByID` | `CreatedByID` | `uniqueidentifier` | |
| `CreatedByScreenID` | `CreatedByScreenID` | `char(8)` | |
| `CreatedDateTime` | `CreatedDateTime` | `datetime` | |
| `LastModifiedByID` | `LastModifiedByID` | `uniqueidentifier` | |
| `LastModifiedByScreenID` | `LastModifiedByScreenID` | `char(8)` | |
| `LastModifiedDateTime` | `LastModifiedDateTime` | `datetime` | |
| `NoteID` | `NoteID` | `uniqueidentifier` | For attachments/attributes |

#### Denormalized Display Fields

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CustomerID_Customer_acctName` | `CustomerID_Customer_acctName` | `nvarchar(255)` | ⚠️ Denormalized — may not exist as a SQL column. Join to Customer instead. |
| `DefaultSiteID_INSite_descr` | `DefaultSiteID_INSite_descr` | `nvarchar(255)` | ⚠️ Same — join to INSite |
| `ShipVia_Carrier_description` | `ShipVia_Carrier_description` | `nvarchar(255)` | ⚠️ Same — join to Carrier |

⚠️ **Fields ending in `_TableName_fieldName`** are DAC-level description fields populated by `[PXSelector]` attributes at runtime. They typically do NOT exist as physical SQL columns. Never reference them in SQL views — always join to the parent table.

---

### SOLine

**Table**: `dbo.SOLine`  
**DAC**: `PX.Objects.SO.SOLine`  
**Interfaces**: `ILSPrimary`, `IHasMinGrossProfit`, `ISortOrder`, `IMatrixItemLine`

#### Key Fields

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CompanyID` | (automatic) | `int` | |
| `OrderType` | `OrderType` | `char(2)` | Composite PK part 1 |
| `OrderNbr` | `OrderNbr` | `nvarchar(15)` | Composite PK part 2 |
| `LineNbr` | `LineNbr` | `int` | Composite PK part 3 |
| `SortOrder` | `SortOrder` | `int` | Display sort order (can differ from LineNbr) |
| `BranchID` | `BranchID` | `int` | Line-level branch override |

#### Line Type & Behavior

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `Behavior` | `Behavior` | `char(2)` | From order type |
| `DefaultOperation` | `DefaultOperation` | `char(1)` | Default operation for order type |
| `Operation` | `Operation` | `char(1)` | Line operation (I=Issue, R=Receipt) |
| `LineSign` | `LineSign` | `smallint` | 1 or -1 — determines sign of quantities |
| `LineType` | `LineType` | `char(2)` | GI=Goods for Inventory, GN=Goods for Non-Inventory, MN=Misc Non-stock, FT=Freight |
| `IsStockItem` | `IsStockItem` | `bit` | |
| `IsKit` | `IsKit` | `bit` | Kit item flag |
| `InvtMult` | `InvtMult` | `smallint` | Inventory multiplier (1=issue, -1=receipt) |

#### Item & Location

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `InventoryID` | `InventoryID` | `int` | FK → InventoryItem |
| `SubItemID` | `SubItemID` | `int` | Sub-item (if enabled) |
| `SiteID` | `SiteID` | `int` | Warehouse |
| `LocationID` | `LocationID` | `int` | Bin location |
| `LotSerialNbr` | `LotSerialNbr` | `nvarchar(100)` | Lot/serial number |
| `UOM` | `UOM` | `nvarchar(6)` | Unit of measure |
| `InvoiceUOM` | `InvoiceUOM` | `nvarchar(6)` | UOM for invoicing (can differ) |
| `AlternateID` | `AlternateID` | `nvarchar(50)` | Alternate/cross-reference ID |

#### Quantities

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `OrderQty` | `OrderQty` | `decimal(25,6)` | Ordered quantity |
| `BaseOrderQty` | `BaseOrderQty` | `decimal(25,6)` | In base UOM |
| `OpenQty` | `OpenQty` | `decimal(25,6)` | Not yet shipped |
| `BaseOpenQty` | `BaseOpenQty` | `decimal(25,6)` | |
| `ShippedQty` | `ShippedQty` | `decimal(25,6)` | Already shipped |
| `BaseShippedQty` | `BaseShippedQty` | `decimal(25,6)` | |
| `BilledQty` | `BilledQty` | `decimal(25,6)` | Already invoiced |
| `BaseBilledQty` | `BaseBilledQty` | `decimal(25,6)` | |
| `UnbilledQty` | `UnbilledQty` | `decimal(25,6)` | Shipped but not invoiced |
| `BaseUnbilledQty` | `BaseUnbilledQty` | `decimal(25,6)` | |
| `ClosedQty` | `ClosedQty` | `decimal(25,6)` | Quantity considered closed |
| `BaseClosedQty` | `BaseClosedQty` | `decimal(25,6)` | |
| `UnassignedQty` | `UnassignedQty` | `decimal(25,6)` | Not yet allocated |
| `UnshippedQty` | `UnshippedQty` | `decimal(25,6)` | |
| `CompleteQtyMin` | `CompleteQtyMin` | `decimal(19,6)` | Min % to consider line complete |
| `CompleteQtyMax` | `CompleteQtyMax` | `decimal(19,6)` | Max % overship allowed |
| `LineQtyAvail` | `LineQtyAvail` | `decimal(25,6)` | Available for allocation |
| `LineQtyHardAvail` | `LineQtyHardAvail` | `decimal(25,6)` | Hard-available |
| `QtyOnOrders` | `QtyOnOrders` | `decimal(25,6)` | For blanket lines |
| `BlanketOpenQty` | `BlanketOpenQty` | `decimal(25,6)` | |

⚠️ **OrderQty vs OpenQty vs UnbilledQty**: For "what's left to ship" use `OpenQty`. For "what's shipped but not invoiced" use `UnbilledQty`. For backlog/pipeline reporting use `OrderQty` with a status filter.

#### Pricing & Amounts (Document Currency)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `ManualPrice` | `ManualPrice` | `bit` | Price was manually overridden |
| `CuryUnitPrice` | `CuryUnitPrice` | `decimal(19,6)` | Unit selling price |
| `CuryExtPrice` | `CuryExtPrice` | `decimal(19,4)` | Extended price (qty × unit price) |
| `CuryUnitCost` | `CuryUnitCost` | `decimal(19,6)` | Unit cost (doc currency) |
| `CuryExtCost` | `CuryExtCost` | `decimal(19,4)` | Extended cost |
| `CuryLineAmt` | `CuryLineAmt` | `decimal(19,4)` | Line amount (after line discount) |
| `CuryOpenAmt` | `CuryOpenAmt` | `decimal(19,4)` | Open amount |
| `CuryBilledAmt` | `CuryBilledAmt` | `decimal(19,4)` | Billed amount |
| `CuryUnbilledAmt` | `CuryUnbilledAmt` | `decimal(19,4)` | Unbilled amount |
| `CuryDiscPrice` | `CuryDiscPrice` | `decimal(19,6)` | Discounted unit price |
| `CuryCommnAmt` | `CuryCommnAmt` | `decimal(19,4)` | Commission amount |
| `PriceType` | `PriceType` | `char(1)` | Source of price |
| `IsPromotionalPrice` | `IsPromotionalPrice` | `bit` | |

#### Base Currency Amounts

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `UnitPrice` | `UnitPrice` | `decimal(19,6)` | Base currency |
| `ExtPrice` | `ExtPrice` | `decimal(19,4)` | |
| `UnitCost` | `UnitCost` | `decimal(19,6)` | |
| `ExtCost` | `ExtCost` | `decimal(19,4)` | |
| `LineAmt` | `LineAmt` | `decimal(19,4)` | |
| `OpenAmt` | `OpenAmt` | `decimal(19,4)` | |
| `BilledAmt` | `BilledAmt` | `decimal(19,4)` | |
| `UnbilledAmt` | `UnbilledAmt` | `decimal(19,4)` | |
| `CommnAmt` | `CommnAmt` | `decimal(19,4)` | |
| `AvgCost` | `AvgCost` | `decimal(19,6)` | Average cost at time of order |

#### Discounts

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `DiscPct` | `DiscPct` | `decimal(19,6)` | Line discount percentage |
| `CuryDiscAmt` | `CuryDiscAmt` | `decimal(19,4)` | Line discount amount |
| `DiscAmt` | `DiscAmt` | `decimal(19,4)` | Base currency |
| `ManualDisc` | `ManualDisc` | `bit` | Manual discount override |
| `DiscountID` | `DiscountID` | `nvarchar(10)` | Applied discount code |
| `DiscountSequenceID` | `DiscountSequenceID` | `nvarchar(10)` | Discount sequence |
| `GroupDiscountRate` | `GroupDiscountRate` | `decimal(19,9)` | Group discount rate applied |
| `DocumentDiscountRate` | `DocumentDiscountRate` | `decimal(19,9)` | Document discount rate applied |
| `IsFree` | `IsFree` | `bit` | Free item from promotion |
| `SkipDisc` | `SkipDisc` | `bit` | Skip discount calculation |

#### Line Status

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `Completed` | `Completed` | `bit` | Line is completed |
| `OpenLine` | `OpenLine` | `bit` | Line is open |
| `Cancelled` | `Cancelled` | `bit` | Line cancelled |
| `ShipComplete` | `ShipComplete` | `char(1)` | Line-level ship complete rule |
| `RequireShipping` | `RequireShipping` | `bit` | |
| `RequireAllocation` | `RequireAllocation` | `bit` | |
| `RequireLocation` | `RequireLocation` | `bit` | |

#### Dates (Line-Level Overrides)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CustomerID` | `CustomerID` | `int` | ⚠️ Line-level customer (for multi-customer orders) |
| `CustomerLocationID` | `CustomerLocationID` | `int` | |
| `OrderDate` | `OrderDate` | `datetime` | Line date (usually matches header) |
| `CancelDate` | `CancelDate` | `datetime` | |
| `RequestDate` | `RequestDate` | `datetime` | Line-level requested date |
| `ShipDate` | `ShipDate` | `datetime` | Line-level ship date |
| `SchedOrderDate` | `SchedOrderDate` | `datetime` | Scheduled order date (blanket) |
| `SchedShipDate` | `SchedShipDate` | `datetime` | Scheduled ship date (blanket) |

#### Tax

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `TaxZoneID` | `TaxZoneID` | `nvarchar(10)` | Line-level tax zone override |
| `TaxCategoryID` | `TaxCategoryID` | `nvarchar(10)` | Tax category |
| `AvalaraCustomerUsageType` | `AvalaraCustomerUsageType` | `nvarchar(25)` | |

#### Invoice Link

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `InvoiceType` | `InvoiceType` | `char(3)` | AR doc type when invoiced |
| `InvoiceNbr` | `InvoiceNbr` | `nvarchar(15)` | AR invoice reference |
| `InvoiceLineNbr` | `InvoiceLineNbr` | `int` | AR invoice line |
| `InvoiceDate` | `InvoiceDate` | `datetime` | |

#### PO / Drop-Ship

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `POCreate` | `POCreate` | `bit` | Whether PO should be created |
| `POSource` | `POSource` | `char(1)` | PO source type |
| `POCreated` | `POCreated` | `bit` | PO has been created |
| `IsSpecialOrder` | `IsSpecialOrder` | `bit` | |
| `POSiteID` | `POSiteID` | `int` | PO warehouse |
| `VendorID` | `VendorID` | `int` | Drop-ship vendor |
| `POCreateDate` | `POCreateDate` | `datetime` | |
| `IsCostUpdatedOnPO` | `IsCostUpdatedOnPO` | `bit` | |

#### Original Order (for returns)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `OrigOrderType` | `OrigOrderType` | `char(2)` | |
| `OrigOrderNbr` | `OrigOrderNbr` | `nvarchar(15)` | |
| `OrigLineNbr` | `OrigLineNbr` | `int` | |
| `OrigShipmentType` | `OrigShipmentType` | `char(1)` | |

#### Blanket Order Link

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `BlanketType` | `BlanketType` | `char(2)` | Parent blanket order type |
| `BlanketNbr` | `BlanketNbr` | `nvarchar(15)` | Parent blanket order number |
| `BlanketLineNbr` | `BlanketLineNbr` | `int` | Parent blanket line |
| `BlanketSplitLineNbr` | `BlanketSplitLineNbr` | `int` | |
| `ChildLineCntr` | `ChildLineCntr` | `int` | |
| `OpenChildLineCntr` | `OpenChildLineCntr` | `int` | |
| `CustomerOrderNbr` | `CustomerOrderNbr` | `nvarchar(40)` | Line-level customer PO |

#### Project / GL

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `ProjectID` | `ProjectID` | `int` | FK → PMProject |
| `TaskID` | `TaskID` | `int` | FK → PMTask |
| `CostCodeID` | `CostCodeID` | `int` | Cost code |
| `SalesAcctID` | `SalesAcctID` | `int` | Sales GL account |
| `SalesSubID` | `SalesSubID` | `int` | Sales GL subaccount |
| `ReasonCode` | `ReasonCode` | `nvarchar(10)` | |
| `RequireReasonCode` | `RequireReasonCode` | `bit` | |

#### Commission

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `SalesPersonID` | `SalesPersonID` | `int` | Line-level salesperson |
| `CommnPct` | `CommnPct` | `decimal(19,6)` | Commission percentage |
| `Commissionable` | `Commissionable` | `bit` | Whether line is commissionable |

#### Physical Properties

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `UnitWeigth` | `UnitWeigth` | `decimal(25,6)` | ⚠️ Acumatica typo — "Weigth" not "Weight" |
| `UnitVolume` | `UnitVolume` | `decimal(25,6)` | |
| `ExtWeight` | `ExtWeight` | `decimal(25,6)` | Extended weight |
| `ExtVolume` | `ExtVolume` | `decimal(25,6)` | Extended volume |

#### Shipping (Line-Level Overrides)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `ShipVia` | `ShipVia` | `nvarchar(15)` | Line-level carrier override |
| `FOBPoint` | `FOBPoint` | `nvarchar(15)` | |
| `ShipTermsID` | `ShipTermsID` | `nvarchar(10)` | |
| `ShipZoneID` | `ShipZoneID` | `nvarchar(15)` | |

#### Deferred Revenue

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `DRTermStartDate` | `DRTermStartDate` | `datetime` | Deferred revenue term start |
| `DRTermEndDate` | `DRTermEndDate` | `datetime` | Deferred revenue term end |
| `DefScheduleID` | `DefScheduleID` | `int` | Deferral schedule |
| `CuryUnitPriceDR` | `CuryUnitPriceDR` | `decimal(19,6)` | Unit price for DR |
| `DiscPctDR` | `DiscPctDR` | `decimal(19,6)` | Discount % for DR |
| `ItemRequiresTerms` | `ItemRequiresTerms` | `bit` | |
| `ItemHasResidual` | `ItemHasResidual` | `bit` | |

#### Audit & Metadata

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `NoteID` | `NoteID` | `uniqueidentifier` | For attachments |
| `CreatedByID` | `CreatedByID` | `uniqueidentifier` | |
| `CreatedDateTime` | `CreatedDateTime` | `datetime` | |
| `LastModifiedByID` | `LastModifiedByID` | `uniqueidentifier` | |
| `LastModifiedDateTime` | `LastModifiedDateTime` | `datetime` | |

#### Inventory Transaction Link

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `TranType` | `TranType` | `char(3)` | Inventory transaction type |
| `TranDate` | `TranDate` | `datetime` | |
| `PlanType` | `PlanType` | `char(2)` | Inventory plan type |
| `OrigPlanType` | `OrigPlanType` | `char(2)` | |
| `InventorySource` | `InventorySource` | `char(1)` | |

---

## Accounts Receivable

ARInvoice inherits from ARRegister. In the database, these map to the same physical table (`ARRegister`) — ARInvoice is a DAC extension that adds invoice-specific fields. When querying in SQL, you're always querying `dbo.ARRegister`, and the "ARInvoice" fields are just additional columns on that same table.

### ARRegister (Base AR Document)

**Table**: `dbo.ARRegister`  
**DAC**: `PX.Objects.AR.ARRegister`  
**Interfaces**: `IRegister`, `IAssign`, `IBalance`, `IDocumentKey`

This is the base table for ALL AR documents — invoices, credit memos, debit memos, payments, prepayments, etc.

#### Key Fields

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CompanyID` | (automatic) | `int` | |
| `DocType` | `DocType` | `char(3)` | PK part 1. See doc types below |
| `RefNbr` | `RefNbr` | `nvarchar(15)` | PK part 2 |
| `PrintDocType` | `PrintDocType` | `char(3)` | Display-friendly doc type |
| `DocumentKey` | `DocumentKey` | `nvarchar(18)` | Concatenated key for lookups |
| `BranchID` | `BranchID` | `int` | FK → Branch |
| `OrigModule` | `OrigModule` | `char(2)` | Source module: 'AR', 'SO', 'EP', etc. |
| `CustomerID` | `CustomerID` | `int` | FK → Customer.BAccountID |
| `CustomerLocationID` | `CustomerLocationID` | `int` | |
| `CuryID` | `CuryID` | `nvarchar(5)` | Currency code |
| `CuryInfoID` | `CuryInfoID` | `bigint` | FK → CurrencyInfo |

#### AR Doc Types

| Code | Constant | Meaning |
|------|----------|---------|
| `INV` | `ARDocType.Invoice` | Invoice |
| `DRM` | `ARDocType.DebitMemo` | Debit Memo |
| `CRM` | `ARDocType.CreditMemo` | Credit Memo |
| `PMT` | `ARDocType.Payment` | Payment |
| `VPM` | `ARDocType.VoidPayment` | Void Payment |
| `PPM` | `ARDocType.Prepayment` | Prepayment |
| `REF` | `ARDocType.Refund` | Refund |
| `VRF` | `ARDocType.VoidRefund` | Void Refund |
| `FCH` | `ARDocType.FinCharge` | Finance Charge |
| `SMC` | `ARDocType.SmallCreditWO` | Small Credit Write-Off |
| `SMB` | `ARDocType.SmallBalanceWO` | Small Balance Write-Off |
| `CSL` | `ARDocType.CashSale` | Cash Sale |
| `RCS` | `ARDocType.CashReturn` | Cash Return |
| `PPD` | `ARDocType.PrepaymentInvoice` | Prepayment Invoice |

⚠️ **The doc type list is much longer than most people realize.** If you're filtering on `DocType = 'INV'` you're only getting invoices — not debit memos, finance charges, etc. For "all receivable documents" you need to include multiple types.

#### Dates & Periods

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `DocDate` | `DocDate` | `datetime` | Document date |
| `OrigDocDate` | `OrigDocDate` | `datetime` | Original document date (for adjustments) |
| `DueDate` | `DueDate` | `datetime` | Payment due date |
| `FinPeriodID` | `FinPeriodID` | `char(6)` | Financial period (YYYYMM) |
| `TranPeriodID` | `TranPeriodID` | `char(6)` | Transaction period |
| `ClosedDate` | `ClosedDate` | `datetime` | Date document was closed |
| `ClosedFinPeriodID` | `ClosedFinPeriodID` | `char(6)` | Period when closed |
| `ClosedTranPeriodID` | `ClosedTranPeriodID` | `char(6)` | |
| `StatementDate` | `StatementDate` | `datetime` | Statement date |

#### Amounts (Document Currency)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CuryOrigDocAmt` | `CuryOrigDocAmt` | `decimal(19,4)` | **Original document amount** |
| `CuryDocBal` | `CuryDocBal` | `decimal(19,4)` | **Current remaining balance** |
| `CuryDocUnpaidBal` | `CuryDocUnpaidBal` | `decimal(19,4)` | Unpaid balance |
| `CuryInitDocBal` | `CuryInitDocBal` | `decimal(19,4)` | Initial balance (for migrated docs) |
| `CuryOrigDiscAmt` | `CuryOrigDiscAmt` | `decimal(19,4)` | Original discount amount |
| `CuryDiscTaken` | `CuryDiscTaken` | `decimal(19,4)` | Discount actually taken |
| `CuryDiscBal` | `CuryDiscBal` | `decimal(19,4)` | Remaining discount balance |
| `CuryDocDisc` | `CuryDocDisc` | `decimal(19,4)` | Document discount |
| `CuryChargeAmt` | `CuryChargeAmt` | `decimal(19,4)` | Finance charge amount |
| `CuryRoundDiff` | `CuryRoundDiff` | `decimal(19,4)` | Rounding difference |
| `CuryDiscountedDocTotal` | `CuryDiscountedDocTotal` | `decimal(19,4)` | Total after discounts |
| `CuryDiscountedTaxableTotal` | `CuryDiscountedTaxableTotal` | `decimal(19,4)` | |
| `CuryDiscountedPrice` | `CuryDiscountedPrice` | `decimal(19,4)` | |

#### Amounts (Base Currency)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `OrigDocAmt` | `OrigDocAmt` | `decimal(19,4)` | Original amount (base) |
| `DocBal` | `DocBal` | `decimal(19,4)` | Current balance (base) |
| `InitDocBal` | `InitDocBal` | `decimal(19,4)` | |
| `OrigDiscAmt` | `OrigDiscAmt` | `decimal(19,4)` | |
| `DiscTaken` | `DiscTaken` | `decimal(19,4)` | |
| `DiscBal` | `DiscBal` | `decimal(19,4)` | |
| `DocDisc` | `DocDisc` | `decimal(19,4)` | |
| `ChargeAmt` | `ChargeAmt` | `decimal(19,4)` | |
| `RGOLAmt` | `RGOLAmt` | `decimal(19,4)` | Realized gain/loss |
| `RoundDiff` | `RoundDiff` | `decimal(19,4)` | |

#### Status & Workflow

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `Status` | `Status` | `char(1)` | ⚠️ **CALCULATED — see logic below** |
| `Hold` | `Hold` | `bit` | On hold |
| `Released` | `Released` | `bit` | Released to GL |
| `OpenDoc` | `OpenDoc` | `bit` | Has open balance |
| `Voided` | `Voided` | `bit` | Voided |
| `Scheduled` | `Scheduled` | `bit` | Scheduled for future |
| `Canceled` | `Canceled` | `bit` | Cancelled |
| `Approved` | `Approved` | `bit` | Approval workflow approved |
| `Rejected` | `Rejected` | `bit` | Approval workflow rejected |
| `DontApprove` | `DontApprove` | `bit` | Skip approval |
| `PendingProcessing` | `PendingProcessing` | `bit` | Pending CC processing |
| `PendingPayment` | `PendingPayment` | `bit` | |
| `SelfVoidingDoc` | `SelfVoidingDoc` | `bit` | |

#### ⚠️ ARRegister.Status — CALCULATED, NOT STORED DIRECTLY

**This is critical for SQL views.** The `Status` field on ARRegister is computed by a `SetStatusAttribute` event handler in C#. It's set based on the combination of boolean flags. The actual status calculation logic from the Acumatica source code is:

```sql
-- Replicating ARRegister.Status in SQL:
CASE
    WHEN ar.Canceled = 1 THEN 'L'           -- Canceled
    WHEN ar.Voided = 1 THEN 'V'             -- Voided
    WHEN ar.Hold = 1 AND ar.Released = 1 THEN 'Z'  -- Reserved
    WHEN ar.Hold = 1 THEN 'H'               -- Hold
    WHEN ar.Scheduled = 1 THEN 'S'          -- Scheduled
    WHEN ar.Rejected = 1 THEN 'J'           -- Rejected
    WHEN ar.Released != 1 
         AND ar.Approved != 1 
         AND ar.DontApprove != 1 
         AND ar.DocType IN ('RCS','REF','INV','DRM','CRM') 
    THEN 'P'                                 -- PendingApproval
    WHEN ar.Released != 1 
         AND ar.PendingProcessing = 1 
    THEN 'W'                                 -- CCHold
    WHEN ar.Released != 1 THEN 'B'           -- Balanced
    WHEN ar.OpenDoc = 1 
         AND ar.DocType = 'PPD' 
    THEN 'U'                                 -- Unapplied (Prepayment Invoice)
    WHEN ar.OpenDoc = 1 THEN 'N'             -- Open
    WHEN ar.OpenDoc = 0 THEN 'C'             -- Closed
    ELSE 'B'                                 -- Fallback
END AS CalculatedStatus
```

⚠️ **The status char codes above need verification against `ARDocStatus` constants on the target tenant.** The logic order is authoritative (from source code), but the single-char values may vary. Always cross-reference with a `SELECT DISTINCT Status FROM ARRegister` on the target database.

⚠️ **Key insight from the source**: Status is evaluated in priority order — `Canceled` beats `Voided` beats `Hold` beats `Scheduled` beats `Rejected`, etc. A document that is both `Hold=1` and `Released=1` gets status "Reserved", not "Hold". This matters for aging reports.

#### Retainage (Construction)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `RetainageApply` | `RetainageApply` | `bit` | Whether retainage applies |
| `IsRetainageDocument` | `IsRetainageDocument` | `bit` | This is a retainage billing doc |
| `IsRetainageReversing` | `IsRetainageReversing` | `bit` | |
| `DefRetainagePct` | `DefRetainagePct` | `decimal(19,6)` | Default retainage percentage |
| `CuryRetainageTotal` | `CuryRetainageTotal` | `decimal(19,4)` | Total retainage amount |
| `CuryRetainageUnreleasedAmt` | `CuryRetainageUnreleasedAmt` | `decimal(19,4)` | Retainage not yet released |
| `CuryRetainageReleased` | `CuryRetainageReleased` | `decimal(19,4)` | Retainage already released |
| `CuryRetainedTaxTotal` | `CuryRetainedTaxTotal` | `decimal(19,4)` | |
| `CuryRetainedDiscTotal` | `CuryRetainedDiscTotal` | `decimal(19,4)` | |
| `CuryRetainageUnpaidTotal` | `CuryRetainageUnpaidTotal` | `decimal(19,4)` | |
| `CuryRetainagePaidTotal` | `CuryRetainagePaidTotal` | `decimal(19,4)` | |
| `CuryOrigDocAmtWithRetainageTotal` | `CuryOrigDocAmtWithRetainageTotal` | `decimal(19,4)` | Total including retainage |
| `CuryLineRetainageTotal` | `CuryLineRetainageTotal` | `decimal(19,4)` | |
| `RetainageAcctID` | `RetainageAcctID` | `int` | GL account for retainage |
| `RetainageSubID` | `RetainageSubID` | `int` | GL sub for retainage |

#### GL Accounts

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `ARAccountID` | `ARAccountID` | `int` | AR control account |
| `ARSubID` | `ARSubID` | `int` | AR sub |
| `PrepaymentAccountID` | `PrepaymentAccountID` | `int` | Prepayment account |
| `PrepaymentSubID` | `PrepaymentSubID` | `int` | |

#### Cancellation / Correction

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `IsCancellation` | `IsCancellation` | `bit` | This doc cancels another |
| `IsCorrection` | `IsCorrection` | `bit` | This doc corrects another |
| `IsUnderCorrection` | `IsUnderCorrection` | `bit` | This doc is being corrected |

#### Commission & Sales

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `SalesPersonID` | `SalesPersonID` | `int` | FK → SalesPerson |

#### Batch Link

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `BatchNbr` | `BatchNbr` | `nvarchar(15)` | GL batch when released |
| `BatchSeq` | `BatchSeq` | `smallint` | Sequence within batch |

#### Original Document Reference

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `OrigDocType` | `OrigDocType` | `char(3)` | Original doc being adjusted |
| `OrigRefNbr` | `OrigRefNbr` | `nvarchar(15)` | |

#### Migration

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `IsMigratedRecord` | `IsMigratedRecord` | `bit` | Imported during data migration |

#### Print/Email

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `DontPrint` | `DontPrint` | `bit` | Skip printing |
| `Printed` | `Printed` | `bit` | Already printed |
| `DontEmail` | `DontEmail` | `bit` | Skip emailing |
| `Emailed` | `Emailed` | `bit` | Already emailed |
| `PrintInvoice` | `PrintInvoice` | `bit` | |
| `EmailInvoice` | `EmailInvoice` | `bit` | |

#### Miscellaneous

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `DocDesc` | `DocDesc` | `nvarchar(255)` | Document description |
| `DocClass` | `DocClass` | `char(1)` | Document class |
| `ScheduleID` | `ScheduleID` | `nvarchar(15)` | Schedule reference |
| `ExternalRef` | `ExternalRef` | `nvarchar(40)` | External reference number |
| `ImpRefNbr` | `ImpRefNbr` | `nvarchar(15)` | Import reference |
| `LineCntr` | `LineCntr` | `int` | Line counter |
| `AdjCntr` | `AdjCntr` | `int` | Adjustment counter |
| `NoteID` | `NoteID` | `uniqueidentifier` | |
| `RefNoteID` | `RefNoteID` | `uniqueidentifier` | |

#### Audit

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CreatedByID` | `CreatedByID` | `uniqueidentifier` | |
| `CreatedByScreenID` | `CreatedByScreenID` | `char(8)` | |
| `CreatedDateTime` | `CreatedDateTime` | `datetime` | |
| `LastModifiedByID` | `LastModifiedByID` | `uniqueidentifier` | |
| `LastModifiedByScreenID` | `LastModifiedByScreenID` | `char(8)` | |
| `LastModifiedDateTime` | `LastModifiedDateTime` | `datetime` | |

---

### ARInvoice (extends ARRegister)

**Table**: `dbo.ARRegister` (same physical table!)  
**DAC**: `PX.Objects.AR.ARInvoice` extends `ARRegister`  
**Screen**: AR301000 (Invoices and Memos)  
**Interfaces**: `IInvoice`, `ICreatePaymentDocument`

ARInvoice adds invoice-specific fields on top of ARRegister. In SQL, it's the same table — just more columns.

#### Invoice-Specific Fields

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `InvoiceNbr` | `InvoiceNbr` | `nvarchar(40)` | Customer-facing invoice number |
| `InvoiceDate` | `InvoiceDate` | `datetime` | Invoice date (can differ from DocDate) |
| `DiscDate` | `DiscDate` | `datetime` | Early payment discount date |
| `TermsID` | `TermsID` | `nvarchar(10)` | Payment terms |
| `MasterRefNbr` | `MasterRefNbr` | `nvarchar(15)` | Master installment ref |
| `InstallmentCntr` | `InstallmentCntr` | `smallint` | |
| `InstallmentNbr` | `InstallmentNbr` | `smallint` | |
| `DocDesc` | `DocDesc` | `nvarchar(255)` | (inherited, repeated for clarity) |

#### Addresses & Contacts

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `BillAddressID` | `BillAddressID` | `int` | FK → ARAddress |
| `BillContactID` | `BillContactID` | `int` | FK → ARContact |
| `MultiShipAddress` | `MultiShipAddress` | `bit` | Multiple shipping addresses |
| `ShipAddressID` | `ShipAddressID` | `int` | FK → SOShippingAddress |
| `ShipContactID` | `ShipContactID` | `int` | FK → SOShippingContact |

#### Amount Breakdowns (Document Currency)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CuryGoodsTotal` | `CuryGoodsTotal` | `decimal(19,4)` | Goods subtotal |
| `CuryLineTotal` | `CuryLineTotal` | `decimal(19,4)` | Line total |
| `CuryTaxTotal` | `CuryTaxTotal` | `decimal(19,4)` | Tax total |
| `CuryLineDiscTotal` | `CuryLineDiscTotal` | `decimal(19,4)` | Line-level discounts |
| `CuryGroupDiscTotal` | `CuryGroupDiscTotal` | `decimal(19,4)` | Group discounts |
| `CuryDocumentDiscTotal` | `CuryDocumentDiscTotal` | `decimal(19,4)` | Document discounts |
| `CuryOrderDiscTotal` | `CuryOrderDiscTotal` | `decimal(19,4)` | Order-level discounts |
| `CuryDiscTot` | `CuryDiscTot` | `decimal(19,4)` | Total discounts |
| `CuryMiscTot` | `CuryMiscTot` | `decimal(19,4)` | Miscellaneous charges |
| `CuryFreightAmt` | `CuryFreightAmt` | `decimal(19,4)` | Freight charged |
| `CuryFreightCost` | `CuryFreightCost` | `decimal(19,4)` | Actual freight cost |
| `CuryFreightTot` | `CuryFreightTot` | `decimal(19,4)` | Total freight |
| `CuryPremiumFreightAmt` | `CuryPremiumFreightAmt` | `decimal(19,4)` | |
| `CuryGoodsExtPriceTotal` | `CuryGoodsExtPriceTotal` | `decimal(19,4)` | Goods extended price |
| `CuryMiscExtPriceTotal` | `CuryMiscExtPriceTotal` | `decimal(19,4)` | Misc extended price |
| `CuryDetaiExtPricelTotal` | `CuryDetaiExtPricelTotal` | `decimal(19,4)` | ⚠️ Acumatica typo — "Detai" and "Pricel" |
| `CuryVatExemptTotal` | `CuryVatExemptTotal` | `decimal(19,4)` | VAT exempt total |
| `CuryVatTaxableTotal` | `CuryVatTaxableTotal` | `decimal(19,4)` | VAT taxable total |

#### Payment Tracking

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CuryPaymentTotal` | `CuryPaymentTotal` | `decimal(19,4)` | Total payments applied |
| `CuryUnreleasedPaymentAmt` | `CuryUnreleasedPaymentAmt` | `decimal(19,4)` | |
| `CuryCCAuthorizedAmt` | `CuryCCAuthorizedAmt` | `decimal(19,4)` | CC authorized |
| `CuryPaidAmt` | `CuryPaidAmt` | `decimal(19,4)` | Actually paid |
| `CuryUnpaidBalance` | `CuryUnpaidBalance` | `decimal(19,4)` | |
| `CuryBalanceWOTotal` | `CuryBalanceWOTotal` | `decimal(19,4)` | Balance write-off total |
| `CuryDiscAppliedAmt` | `CuryDiscAppliedAmt` | `decimal(19,4)` | Discount applied |
| `CuryApplicationBalance` | `CuryApplicationBalance` | `decimal(19,4)` | |
| `CuryDiscBal` | `CuryDiscBal` | `decimal(19,4)` | (inherited) |
| `CuryWhTaxBal` | `CuryWhTaxBal` | `decimal(19,4)` | Withholding tax balance |
| `PaymentMethodID` | `PaymentMethodID` | `nvarchar(10)` | |
| `PMInstanceID` | `PMInstanceID` | `int` | Stored payment method |
| `CashAccountID` | `CashAccountID` | `int` | |
| `AuthorizedPaymentCntr` | `AuthorizedPaymentCntr` | `int` | |
| `IsPaymentsTransferred` | `IsPaymentsTransferred` | `bit` | |

#### Commission

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `SalesPersonID` | `SalesPersonID` | `int` | (inherited) |
| `CommnPct` | `CommnPct` | `decimal(19,6)` | Commission percentage |
| `CuryCommnAmt` | `CuryCommnAmt` | `decimal(19,4)` | Commission amount |
| `CuryCommnblAmt` | `CuryCommnblAmt` | `decimal(19,4)` | Commissionable amount |

#### Credit Hold & Approval

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CreditHold` | `CreditHold` | `bit` | |
| `ApprovedCredit` | `ApprovedCredit` | `bit` | |
| `ApprovedCreditAmt` | `ApprovedCreditAmt` | `decimal(19,4)` | |
| `ApprovedCaptureFailed` | `ApprovedCaptureFailed` | `bit` | |
| `ApprovedPrepaymentRequired` | `ApprovedPrepaymentRequired` | `bit` | |
| `WorkgroupID` | `WorkgroupID` | `int` | |
| `OwnerID` | `OwnerID` | `uniqueidentifier` | |

#### Tax

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `TaxZoneID` | `TaxZoneID` | `nvarchar(10)` | |
| `TaxCalcMode` | `TaxCalcMode` | `char(1)` | |
| `IsTaxValid` | `IsTaxValid` | `bit` | |
| `IsTaxPosted` | `IsTaxPosted` | `bit` | |
| `IsTaxSaved` | `IsTaxSaved` | `bit` | |
| `NonTaxable` | `NonTaxable` | `bit` | |
| `ExternalTaxExemptionNumber` | `ExternalTaxExemptionNumber` | `nvarchar(30)` | |
| `AvalaraCustomerUsageType` | `AvalaraCustomerUsageType` | `nvarchar(25)` | |
| `ExternalTaxesImportInProgress` | `ExternalTaxesImportInProgress` | `bit` | |
| `HasPPDTaxes` | `HasPPDTaxes` | `bit` | Has prompt payment discount taxes |
| `PendingPPD` | `PendingPPD` | `bit` | Pending PPD |

#### Finance Charges

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `ApplyOverdueCharge` | `ApplyOverdueCharge` | `bit` | |
| `LastFinChargeDate` | `LastFinChargeDate` | `datetime` | |
| `LastPaymentDate` | `LastPaymentDate` | `datetime` | |

#### Project

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `ProjectID` | `ProjectID` | `int` | FK → PMProject |
| `PaymentsByLinesAllowed` | `PaymentsByLinesAllowed` | `bit` | Line-level payment application |

#### SO Link (Hidden Navigation Fields)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `HiddenOrderType` | `HiddenOrderType` | `char(2)` | Source SO order type |
| `HiddenOrderNbr` | `HiddenOrderNbr` | `nvarchar(15)` | Source SO order number |
| `HiddenByShipment` | `HiddenByShipment` | `bit` | Created from shipment |
| `HiddenShipmentType` | `HiddenShipmentType` | `char(1)` | |
| `HiddenShipmentNbr` | `HiddenShipmentNbr` | `nvarchar(15)` | |

⚠️ **These `Hidden*` fields** are used for GI navigation back to the source SO/shipment. They may be useful in SQL views that need to link AR invoices back to their originating sales orders.

#### Intercompany

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `IsHiddenInIntercompanySales` | `IsHiddenInIntercompanySales` | `bit` | |

#### Correction

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CorrectionDocType` | `CorrectionDocType` | `char(3)` | |
| `CorrectionRefNbr` | `CorrectionRefNbr` | `nvarchar(15)` | |
| `IsUnderCancellation` | `IsUnderCancellation` | `bit` | |
| `PendingProcessingCntr` | `PendingProcessingCntr` | `int` | |
| `CaptureFailedCntr` | `CaptureFailedCntr` | `int` | |

#### Miscellaneous

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `ProformaExists` | `ProformaExists` | `bit` | Pro-forma invoice exists |
| `Revoked` | `Revoked` | `bit` | |
| `DrCr` | `DrCr` | `char(1)` | Debit/Credit indicator |
| `Payable` | `Payable` | `bit` | (from ARRegister) |
| `Paying` | `Paying` | `bit` | (from ARRegister) |
| `SignBalance` | `SignBalance` | `smallint` | 1 or -1 for balance sign |
| `SignAmount` | `SignAmount` | `smallint` | 1 or -1 for amount sign |

---

### ARTran (AR Invoice Lines)

**Table**: `dbo.ARTran`  
**DAC**: `PX.Objects.AR.ARTran`  
**Interfaces**: `IHasMinGrossProfit`, `IDocumentLine`, `ISortOrder`, `ILSPrimary`, `IAccountable`

#### Key Fields

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CompanyID` | (automatic) | `int` | |
| `TranType` | `TranType` | `char(3)` | PK part 1 — matches ARRegister.DocType |
| `RefNbr` | `RefNbr` | `nvarchar(15)` | PK part 2 |
| `LineNbr` | `LineNbr` | `int` | PK part 3 |
| `SortOrder` | `SortOrder` | `int` | Display order |
| `BranchID` | `BranchID` | `int` | Line-level branch |
| `CustomerID` | `CustomerID` | `int` | |

#### SO/Shipment Link

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `SOOrderType` | `SOOrderType` | `char(2)` | Source SO order type |
| `SOOrderNbr` | `SOOrderNbr` | `nvarchar(15)` | Source SO order number |
| `SOOrderLineNbr` | `SOOrderLineNbr` | `int` | Source SO line |
| `SOOrderLineOperation` | `SOOrderLineOperation` | `char(1)` | |
| `SOOrderSortOrder` | `SOOrderSortOrder` | `int` | |
| `SOOrderLineSign` | `SOOrderLineSign` | `smallint` | |
| `SOShipmentType` | `SOShipmentType` | `char(1)` | |
| `SOShipmentNbr` | `SOShipmentNbr` | `nvarchar(15)` | Source shipment |
| `SOShipmentLineGroupNbr` | `SOShipmentLineGroupNbr` | `int` | |
| `SOShipmentLineNbr` | `SOShipmentLineNbr` | `int` | |

⚠️ **This is how you trace an AR invoice line back to its SO and shipment.** For reports that need the full chain (SO → Shipment → AR Invoice), join on `SOOrderType`/`SOOrderNbr`/`SOOrderLineNbr`.

#### Item & Location

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `InventoryID` | `InventoryID` | `int` | FK → InventoryItem |
| `IsStockItem` | `IsStockItem` | `bit` | |
| `LineType` | `LineType` | `char(2)` | GI, GN, MN, FT, etc. |
| `SiteID` | `SiteID` | `int` | Warehouse |
| `SubItemID` | `SubItemID` | `int` | |
| `LocationID` | `LocationID` | `int` | Bin |
| `LotSerialNbr` | `LotSerialNbr` | `nvarchar(100)` | |
| `ExpireDate` | `ExpireDate` | `datetime` | Lot expiration |
| `UOM` | `UOM` | `nvarchar(6)` | Unit of measure |
| `InvtMult` | `InvtMult` | `smallint` | Inventory multiplier |

#### Quantities

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `Qty` | `Qty` | `decimal(25,6)` | Line quantity |
| `BaseQty` | `BaseQty` | `decimal(25,6)` | In base UOM |
| `UnassignedQty` | `UnassignedQty` | `decimal(25,6)` | |

#### Pricing (Document Currency)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `ManualPrice` | `ManualPrice` | `bit` | Price was manually set |
| `CuryUnitPrice` | `CuryUnitPrice` | `decimal(19,6)` | Unit price |
| `CuryExtPrice` | `CuryExtPrice` | `decimal(19,4)` | Extended price |
| `CuryTranAmt` | `CuryTranAmt` | `decimal(19,4)` | **Line amount after discounts** |
| `CuryTranBal` | `CuryTranBal` | `decimal(19,4)` | Line balance remaining |
| `CuryOrigTranAmt` | `CuryOrigTranAmt` | `decimal(19,4)` | Original line amount |

#### Base Currency

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `UnitPrice` | `UnitPrice` | `decimal(19,6)` | |
| `ExtPrice` | `ExtPrice` | `decimal(19,4)` | |
| `TranAmt` | `TranAmt` | `decimal(19,4)` | |
| `TranBal` | `TranBal` | `decimal(19,4)` | |
| `OrigTranAmt` | `OrigTranAmt` | `decimal(19,4)` | |

#### Cost

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `UnitCost` | `UnitCost` | `decimal(19,6)` | Unit cost |
| `TranCost` | `TranCost` | `decimal(19,4)` | Total line cost |
| `TranCostOrig` | `TranCostOrig` | `decimal(19,4)` | Original cost |
| `IsTranCostFinal` | `IsTranCostFinal` | `bit` | Cost is finalized |
| `AccrueCost` | `AccrueCost` | `bit` | |
| `CostBasis` | `CostBasis` | `decimal(19,4)` | |
| `CuryAccruedCost` | `CuryAccruedCost` | `decimal(19,4)` | |

⚠️ **For margin/profit analysis**: Margin = `CuryTranAmt - TranCost`. But `TranCost` may not be final until the inventory transaction is released (`IsTranCostFinal = 1`). For FIFO/Average costing, the cost may update after the invoice is released.

#### Discounts

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `DiscPct` | `DiscPct` | `decimal(19,6)` | Discount percentage |
| `CuryDiscAmt` | `CuryDiscAmt` | `decimal(19,4)` | Discount amount |
| `DiscAmt` | `DiscAmt` | `decimal(19,4)` | |
| `ManualDisc` | `ManualDisc` | `bit` | |
| `DiscountID` | `DiscountID` | `nvarchar(10)` | |
| `DiscountSequenceID` | `DiscountSequenceID` | `nvarchar(10)` | |
| `GroupDiscountRate` | `GroupDiscountRate` | `decimal(19,9)` | |
| `DocumentDiscountRate` | `DocumentDiscountRate` | `decimal(19,9)` | |
| `IsFree` | `IsFree` | `bit` | Promotional free item |

#### Tax

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `TaxID` | `TaxID` | `nvarchar(30)` | |
| `TaxCategoryID` | `TaxCategoryID` | `nvarchar(10)` | |
| `CuryTaxableAmt` | `CuryTaxableAmt` | `decimal(19,4)` | |
| `CuryTaxAmt` | `CuryTaxAmt` | `decimal(19,4)` | |
| `CuryOrigTaxableAmt` | `CuryOrigTaxableAmt` | `decimal(19,4)` | |
| `CuryOrigTaxAmt` | `CuryOrigTaxAmt` | `decimal(19,4)` | |

#### Retainage

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `RetainagePct` | `RetainagePct` | `decimal(19,6)` | |
| `CuryRetainageAmt` | `CuryRetainageAmt` | `decimal(19,4)` | |
| `CuryOrigRetainageAmt` | `CuryOrigRetainageAmt` | `decimal(19,4)` | |
| `CuryRetainageBal` | `CuryRetainageBal` | `decimal(19,4)` | |
| `CuryRetainedTaxableAmt` | `CuryRetainedTaxableAmt` | `decimal(19,4)` | |
| `CuryRetainedTaxAmt` | `CuryRetainedTaxAmt` | `decimal(19,4)` | |
| `CuryCashDiscBal` | `CuryCashDiscBal` | `decimal(19,4)` | |

#### GL Accounts

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `AccountID` | `AccountID` | `int` | Revenue GL account |
| `SubID` | `SubID` | `int` | Revenue GL sub |
| `ExpenseAccrualAccountID` | `ExpenseAccrualAccountID` | `int` | |
| `ExpenseAccrualSubID` | `ExpenseAccrualSubID` | `int` | |
| `ExpenseAccountID` | `ExpenseAccountID` | `int` | |
| `ExpenseSubID` | `ExpenseSubID` | `int` | |

#### Project

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `ProjectID` | `ProjectID` | `int` | FK → PMProject |
| `TaskID` | `TaskID` | `int` | FK → PMTask |
| `CostCodeID` | `CostCodeID` | `int` | |
| `PMDeltaOption` | `PMDeltaOption` | `char(1)` | Project delta calculation |
| `ExpenseDate` | `ExpenseDate` | `datetime` | |

#### Commission

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `SalesPersonID` | `SalesPersonID` | `int` | |
| `EmployeeID` | `EmployeeID` | `int` | |
| `CommnPct` | `CommnPct` | `decimal(19,6)` | |
| `CuryCommnAmt` | `CuryCommnAmt` | `decimal(19,4)` | |
| `Commissionable` | `Commissionable` | `bit` | |

#### Status & Dates

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `Released` | `Released` | `bit` | Line released |
| `TranDate` | `TranDate` | `datetime` | Transaction date |
| `OrigInvoiceDate` | `OrigInvoiceDate` | `datetime` | |
| `FinPeriodID` | `FinPeriodID` | `char(6)` | |
| `TranPeriodID` | `TranPeriodID` | `char(6)` | |
| `TranDesc` | `TranDesc` | `nvarchar(255)` | Line description |
| `TranClass` | `TranClass` | `char(1)` | |
| `DrCr` | `DrCr` | `char(1)` | |
| `Date` | `Date` | `datetime` | |
| `ReasonCode` | `ReasonCode` | `nvarchar(10)` | |

#### Original Document

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `OrigDocType` | `OrigDocType` | `char(3)` | |
| `OrigRefNbr` | `OrigRefNbr` | `nvarchar(15)` | |
| `OrigLineNbr` | `OrigLineNbr` | `int` | |
| `OrigInvoiceType` | `OrigInvoiceType` | `char(3)` | |
| `OrigInvoiceNbr` | `OrigInvoiceNbr` | `nvarchar(15)` | |
| `OrigInvoiceLineNbr` | `OrigInvoiceLineNbr` | `int` | |

#### Inventory Transaction Link

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `InvtDocType` | `InvtDocType` | `char(3)` | Inventory doc type |
| `InvtRefNbr` | `InvtRefNbr` | `nvarchar(15)` | Inventory doc ref |
| `InvtReleased` | `InvtReleased` | `bit` | Inventory doc released |
| `RequireINUpdate` | `RequireINUpdate` | `bit` | Needs inventory update |

#### Blanket Order Link

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `BlanketType` | `BlanketType` | `char(2)` | |
| `BlanketNbr` | `BlanketNbr` | `nvarchar(15)` | |
| `BlanketLineNbr` | `BlanketLineNbr` | `int` | |
| `BlanketSplitLineNbr` | `BlanketSplitLineNbr` | `int` | |

#### Cancellation

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `IsCancellation` | `IsCancellation` | `bit` | |
| `Canceled` | `Canceled` | `bit` | |
| `SubstitutionRequired` | `SubstitutionRequired` | `bit` | |

#### Deferred Revenue

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `DeferredCode` | `DeferredCode` | `nvarchar(10)` | Deferral code |
| `DefScheduleID` | `DefScheduleID` | `int` | Deferral schedule |
| `DRTermStartDate` | `DRTermStartDate` | `datetime` | |
| `DRTermEndDate` | `DRTermEndDate` | `datetime` | |
| `CuryUnitPriceDR` | `CuryUnitPriceDR` | `decimal(19,6)` | |
| `DiscPctDR` | `DiscPctDR` | `decimal(19,6)` | |
| `RequiresTerms` | `RequiresTerms` | `bit` | |
| `ItemHasResidual` | `ItemHasResidual` | `bit` | |

#### Audit

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `NoteID` | `NoteID` | `uniqueidentifier` | |
| `CreatedByID` | `CreatedByID` | `uniqueidentifier` | |
| `CreatedDateTime` | `CreatedDateTime` | `datetime` | |
| `LastModifiedByID` | `LastModifiedByID` | `uniqueidentifier` | |
| `LastModifiedDateTime` | `LastModifiedDateTime` | `datetime` | |

---

## Customer

**Table**: `dbo.Customer` (extends `BAccount`)  
**DAC**: `PX.Objects.AR.Customer`

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CompanyID` | (automatic) | `int` | |
| `BAccountID` | `BAccountID` | `int` | PK — this is the same as CustomerID in AR/SO tables |
| `AcctCD` | `AcctCD` | `nvarchar(30)` | Customer ID as seen by users |
| `AcctName` | `AcctName` | `nvarchar(255)` | Customer name |
| `CustomerClassID` | `CustomerClassID` | `nvarchar(10)` | |
| `Status` | `Status` | `char(1)` | A=Active, H=Hold, I=Inactive |

---

## Accounts Payable

Like AR, APInvoice inherits from APRegister. They share the same physical table (`dbo.APRegister`). APInvoice adds invoice-specific fields (payment scheduling, withholding tax, landed cost, etc.).

### APRegister (Base AP Document)

**Table**: `dbo.APRegister`  
**DAC**: `PX.Objects.AP.APRegister`  
**Interfaces**: `IRegister`, `IBalance`, `IProjectHeader`, `IDocumentKey`

#### Key Fields

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CompanyID` | (automatic) | `int` | |
| `DocType` | `DocType` | `char(3)` | PK part 1. See doc types below |
| `RefNbr` | `RefNbr` | `nvarchar(15)` | PK part 2 |
| `PrintDocType` | `PrintDocType` | `char(3)` | Display-friendly |
| `BranchID` | `BranchID` | `int` | |
| `OrigModule` | `OrigModule` | `char(2)` | Source module: 'AP', 'PO', 'EP', etc. |
| `VendorID` | `VendorID` | `int` | FK → Vendor.BAccountID |
| `VendorLocationID` | `VendorLocationID` | `int` | |
| `CuryID` | `CuryID` | `nvarchar(5)` | Currency |
| `CuryInfoID` | `CuryInfoID` | `bigint` | |

#### AP Doc Types

| Code | Meaning |
|------|---------|
| `INV` | Bill (Vendor Invoice) |
| `ACR` | Credit Adjustment |
| `ADR` | Debit Adjustment |
| `CHK` | Check |
| `VCK` | Void Check |
| `PPM` | Prepayment |
| `REF` | Vendor Refund |
| `QCK` | Quick Check |
| `VQC` | Void Quick Check |

⚠️ **AP's "INV" means Bill, not Invoice.** AP and AR both use `INV` as a doc type but they mean different things. In AP, `INV` is a vendor bill (a payable). When joining AP and AR tables, always filter on module or table context — never assume `DocType = 'INV'` means the same thing across modules.

#### Dates & Periods

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `DocDate` | `DocDate` | `datetime` | Document date |
| `OrigDocDate` | `OrigDocDate` | `datetime` | |
| `FinPeriodID` | `FinPeriodID` | `char(6)` | Financial period |
| `TranPeriodID` | `TranPeriodID` | `char(6)` | Transaction period |
| `ClosedDate` | `ClosedDate` | `datetime` | Date document was closed |
| `ClosedFinPeriodID` | `ClosedFinPeriodID` | `char(6)` | |
| `ClosedTranPeriodID` | `ClosedTranPeriodID` | `char(6)` | |

#### Amounts (Document Currency)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CuryOrigDocAmt` | `CuryOrigDocAmt` | `decimal(19,4)` | **Original document amount** |
| `CuryDocBal` | `CuryDocBal` | `decimal(19,4)` | **Current remaining balance** |
| `CuryInitDocBal` | `CuryInitDocBal` | `decimal(19,4)` | Initial balance (migrated docs) |
| `CuryOrigDiscAmt` | `CuryOrigDiscAmt` | `decimal(19,4)` | Original discount |
| `CuryDiscTaken` | `CuryDiscTaken` | `decimal(19,4)` | Discount taken |
| `CuryDiscBal` | `CuryDiscBal` | `decimal(19,4)` | Remaining discount |
| `CuryDiscTot` | `CuryDiscTot` | `decimal(19,4)` | Total discounts |
| `CuryDocDisc` | `CuryDocDisc` | `decimal(19,4)` | Document discount |
| `CuryChargeAmt` | `CuryChargeAmt` | `decimal(19,4)` | Finance charges |
| `CuryRoundDiff` | `CuryRoundDiff` | `decimal(19,4)` | Rounding |
| `CuryTaxRoundDiff` | `CuryTaxRoundDiff` | `decimal(19,4)` | Tax rounding |

#### Withholding Tax

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CuryOrigWhTaxAmt` | `CuryOrigWhTaxAmt` | `decimal(19,4)` | Original withholding tax |
| `CuryWhTaxBal` | `CuryWhTaxBal` | `decimal(19,4)` | Withholding tax balance |
| `CuryTaxWheld` | `CuryTaxWheld` | `decimal(19,4)` | Tax withheld |

#### Base Currency Amounts

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `OrigDocAmt` | `OrigDocAmt` | `decimal(19,4)` | |
| `DocBal` | `DocBal` | `decimal(19,4)` | |
| `InitDocBal` | `InitDocBal` | `decimal(19,4)` | |
| `OrigDiscAmt` | `OrigDiscAmt` | `decimal(19,4)` | |
| `DiscTaken` | `DiscTaken` | `decimal(19,4)` | |
| `DiscBal` | `DiscBal` | `decimal(19,4)` | |
| `DiscTot` | `DiscTot` | `decimal(19,4)` | |
| `DocDisc` | `DocDisc` | `decimal(19,4)` | |
| `ChargeAmt` | `ChargeAmt` | `decimal(19,4)` | |
| `OrigWhTaxAmt` | `OrigWhTaxAmt` | `decimal(19,4)` | |
| `WhTaxBal` | `WhTaxBal` | `decimal(19,4)` | |
| `TaxWheld` | `TaxWheld` | `decimal(19,4)` | |
| `RGOLAmt` | `RGOLAmt` | `decimal(19,4)` | Realized gain/loss |
| `RoundDiff` | `RoundDiff` | `decimal(19,4)` | |
| `TaxRoundDiff` | `TaxRoundDiff` | `decimal(19,4)` | |

#### Status & Workflow

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `Status` | `Status` | `char(1)` | ⚠️ Likely computed like AR — see note below |
| `Hold` | `Hold` | `bit` | |
| `Released` | `Released` | `bit` | |
| `OpenDoc` | `OpenDoc` | `bit` | |
| `Voided` | `Voided` | `bit` | |
| `Printed` | `Printed` | `bit` | |
| `Prebooked` | `Prebooked` | `bit` | Pre-booked (not yet released) |
| `Scheduled` | `Scheduled` | `bit` | |
| `Approved` | `Approved` | `bit` | |
| `Rejected` | `Rejected` | `bit` | |
| `DontApprove` | `DontApprove` | `bit` | |

⚠️ **APRegister.Status** follows a similar pattern to ARRegister — it's computed from the boolean flags. The priority order is likely: Voided → Prebooked → Hold → Scheduled → Rejected → PendingApproval → Balanced → Open → Closed. Verify with `SELECT DISTINCT Status FROM APRegister` on the target tenant, and cross-reference with `APDocStatus` constants in the Acumatica source.

#### GL Accounts

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `APAccountID` | `APAccountID` | `int` | AP control account |
| `APSubID` | `APSubID` | `int` | |

#### Batch Links

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `BatchNbr` | `BatchNbr` | `nvarchar(15)` | GL batch when released |
| `PrebookBatchNbr` | `PrebookBatchNbr` | `nvarchar(15)` | GL batch for prebook |
| `VoidBatchNbr` | `VoidBatchNbr` | `nvarchar(15)` | GL batch for void |

#### Original Document

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `OrigDocType` | `OrigDocType` | `char(3)` | |
| `OrigRefNbr` | `OrigRefNbr` | `nvarchar(15)` | |

#### Approval Workflow

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `EmployeeID` | `EmployeeID` | `int` | |
| `EmployeeWorkgroupID` | `EmployeeWorkgroupID` | `int` | |
| `WorkgroupID` | `WorkgroupID` | `int` | |
| `OwnerID` | `OwnerID` | `uniqueidentifier` | |

#### Retainage (Construction)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `RetainageApply` | `RetainageApply` | `bit` | |
| `IsRetainageDocument` | `IsRetainageDocument` | `bit` | |
| `IsRetainageReversing` | `IsRetainageReversing` | `bit` | |
| `DefRetainagePct` | `DefRetainagePct` | `decimal(19,6)` | |
| `CuryRetainageTotal` | `CuryRetainageTotal` | `decimal(19,4)` | |
| `CuryRetainageUnreleasedAmt` | `CuryRetainageUnreleasedAmt` | `decimal(19,4)` | |
| `CuryRetainageReleased` | `CuryRetainageReleased` | `decimal(19,4)` | |
| `CuryRetainedTaxTotal` | `CuryRetainedTaxTotal` | `decimal(19,4)` | |
| `CuryRetainedDiscTotal` | `CuryRetainedDiscTotal` | `decimal(19,4)` | |
| `CuryRetainageUnpaidTotal` | `CuryRetainageUnpaidTotal` | `decimal(19,4)` | |
| `CuryRetainagePaidTotal` | `CuryRetainagePaidTotal` | `decimal(19,4)` | |
| `CuryOrigDocAmtWithRetainageTotal` | `CuryOrigDocAmtWithRetainageTotal` | `decimal(19,4)` | |
| `CuryLineRetainageTotal` | `CuryLineRetainageTotal` | `decimal(19,4)` | |
| `RetainageAcctID` | `RetainageAcctID` | `int` | |
| `RetainageSubID` | `RetainageSubID` | `int` | |

#### Project

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `ProjectID` | `ProjectID` | `int` | FK → PMProject |
| `HasMultipleProjects` | `HasMultipleProjects` | `bit` | Lines span multiple projects |
| `PaymentsByLinesAllowed` | `PaymentsByLinesAllowed` | `bit` | |

#### Tax

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `TaxCalcMode` | `TaxCalcMode` | `char(1)` | |
| `IsTaxValid` | `IsTaxValid` | `bit` | |
| `IsTaxPosted` | `IsTaxPosted` | `bit` | |
| `IsTaxSaved` | `IsTaxSaved` | `bit` | |
| `NonTaxable` | `NonTaxable` | `bit` | |

#### Miscellaneous

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `DocDesc` | `DocDesc` | `nvarchar(255)` | |
| `DocClass` | `DocClass` | `char(1)` | |
| `ScheduleID` | `ScheduleID` | `nvarchar(15)` | |
| `ImpRefNbr` | `ImpRefNbr` | `nvarchar(15)` | |
| `LineCntr` | `LineCntr` | `int` | |
| `AdjCntr` | `AdjCntr` | `int` | |
| `IsMigratedRecord` | `IsMigratedRecord` | `bit` | |
| `Payable` | `Payable` | `bit` | |
| `Paying` | `Paying` | `bit` | |
| `SignBalance` | `SignBalance` | `smallint` | |
| `SignAmount` | `SignAmount` | `smallint` | |

#### VAT / Cost Adjustment

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `TaxCostINAdjRefNbr` | `TaxCostINAdjRefNbr` | `nvarchar(15)` | Inventory adjustment for tax cost |
| `IsExpectedPPVValid` | `IsExpectedPPVValid` | `bit` | Purchase price variance valid |

#### Audit

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `NoteID` | `NoteID` | `uniqueidentifier` | |
| `RefNoteID` | `RefNoteID` | `uniqueidentifier` | |
| `CreatedByID` | `CreatedByID` | `uniqueidentifier` | |
| `CreatedByScreenID` | `CreatedByScreenID` | `char(8)` | |
| `CreatedDateTime` | `CreatedDateTime` | `datetime` | |
| `LastModifiedByID` | `LastModifiedByID` | `uniqueidentifier` | |
| `LastModifiedByScreenID` | `LastModifiedByScreenID` | `char(8)` | |
| `LastModifiedDateTime` | `LastModifiedDateTime` | `datetime` | |

---

### APInvoice (extends APRegister)

**Table**: `dbo.APRegister` (same physical table!)  
**DAC**: `PX.Objects.AP.APInvoice` extends `APRegister`  
**Screen**: AP301000 (Bills and Adjustments)  
**Interfaces**: `IInvoice`, `IProjectTaxes`, `IAssign`, `IApprovable`

#### Invoice-Specific Fields

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `InvoiceNbr` | `InvoiceNbr` | `nvarchar(40)` | Vendor's invoice number |
| `InvoiceDate` | `InvoiceDate` | `datetime` | Vendor's invoice date |
| `TermsID` | `TermsID` | `nvarchar(10)` | Payment terms |
| `DueDate` | `DueDate` | `datetime` | Payment due date |
| `DiscDate` | `DiscDate` | `datetime` | Early payment discount date |
| `MasterRefNbr` | `MasterRefNbr` | `nvarchar(15)` | Master installment ref |
| `InstallmentNbr` | `InstallmentNbr` | `smallint` | |
| `InstallmentCntr` | `InstallmentCntr` | `smallint` | |
| `ManualEntry` | `ManualEntry` | `bit` | Manual data entry (vs. system-generated) |

#### Supplied-By Vendor (Drop-Ship / Third Party)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `SuppliedByVendorID` | `SuppliedByVendorID` | `int` | Actual supplier (may differ from VendorID) |
| `SuppliedByVendorLocationID` | `SuppliedByVendorLocationID` | `int` | |
| `PaymentInfoLocationID` | `PaymentInfoLocationID` | `int` | Location for payment info |

#### Amount Breakdowns (Document Currency)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CuryTaxTotal` | `CuryTaxTotal` | `decimal(19,4)` | Tax total |
| `CuryLineTotal` | `CuryLineTotal` | `decimal(19,4)` | Line total |
| `CuryTaxAmt` | `CuryTaxAmt` | `decimal(19,4)` | Tax amount |
| `CuryVatExemptTotal` | `CuryVatExemptTotal` | `decimal(19,4)` | VAT exempt |
| `CuryVatTaxableTotal` | `CuryVatTaxableTotal` | `decimal(19,4)` | VAT taxable |
| `CuryLineDiscTotal` | `CuryLineDiscTotal` | `decimal(19,4)` | Line discounts |
| `CuryOrderDiscTotal` | `CuryOrderDiscTotal` | `decimal(19,4)` | |
| `CuryDetailExtPriceTotal` | `CuryDetailExtPriceTotal` | `decimal(19,4)` | Detail extended price |

#### Payment Selection

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `SeparateCheck` | `SeparateCheck` | `bit` | Pay on separate check |
| `PaySel` | `PaySel` | `bit` | Selected for payment |
| `PayDate` | `PayDate` | `datetime` | Scheduled payment date |
| `PayTypeID` | `PayTypeID` | `nvarchar(10)` | Payment method for this bill |
| `PayAccountID` | `PayAccountID` | `int` | Cash account for payment |
| `PayLocationID` | `PayLocationID` | `int` | |
| `EstPayDate` | `EstPayDate` | `datetime` | Estimated payment date |

⚠️ **For AP aging / cash flow forecasting**, `PayDate` and `EstPayDate` are the key fields. `DueDate` is the contractual due date from terms; `PayDate` is when the payment is actually scheduled.

#### Prebook Accounts

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `PrebookAcctID` | `PrebookAcctID` | `int` | Pre-booking GL account |
| `PrebookSubID` | `PrebookSubID` | `int` | |

#### Tax Flags

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `TaxZoneID` | `TaxZoneID` | `nvarchar(10)` | |
| `HasWithHoldTax` | `HasWithHoldTax` | `bit` | Has withholding tax lines |
| `HasUseTax` | `HasUseTax` | `bit` | Has use tax |
| `IsTaxDocument` | `IsTaxDocument` | `bit` | This is a tax document |
| `PendingPPD` | `PendingPPD` | `bit` | Pending prompt payment discount |
| `SetWarningOnDiscount` | `SetWarningOnDiscount` | `bit` | |
| `ExternalTaxExemptionNumber` | `ExternalTaxExemptionNumber` | `nvarchar(30)` | |
| `EntityUsageType` | `EntityUsageType` | `nvarchar(25)` | Avalara entity usage |
| `ExternalTaxesImportInProgress` | `ExternalTaxesImportInProgress` | `bit` | |

#### Landed Cost

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `LCEnabled` | `LCEnabled` | `bit` | Landed cost enabled |

#### Joint Payees (Construction)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `IsJointPayees` | `IsJointPayees` | `bit` | Joint check payees |

#### Intercompany

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `IntercompanyInvoiceNoteID` | `IntercompanyInvoiceNoteID` | `uniqueidentifier` | Link to intercompany AR invoice |

#### Discount Control

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `DisableAutomaticDiscountCalculation` | `DisableAutomaticDiscountCalculation` | `bit` | |

---

## Vendor

**Table**: `dbo.Vendor` (extends `BAccount`)  
**DAC**: `PX.Objects.AP.Vendor`

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CompanyID` | (automatic) | `int` | |
| `BAccountID` | `BAccountID` | `int` | PK — same as VendorID in AP tables |
| `AcctCD` | `AcctCD` | `nvarchar(30)` | Vendor ID as seen by users |
| `AcctName` | `AcctName` | `nvarchar(255)` | Vendor name |
| `VendorClassID` | `VendorClassID` | `nvarchar(10)` | |
| `Status` | `Status` | `char(1)` | A=Active, H=Hold, I=Inactive |

**Join to APRegister**: `ON v.CompanyID = ap.CompanyID AND v.BAccountID = ap.VendorID`

⚠️ **Vendor and Customer share the BAccount table.** An entity can be both a customer and a vendor simultaneously. The `BAccount` table holds the common record; `Customer` and `Vendor` tables add role-specific fields. When joining, always use the role-specific table (Customer or Vendor), not BAccount directly, unless you need entities regardless of role.

---

## Common Join Patterns

These are copy-paste-ready join clauses for the most frequent table combinations.

### Item + Class + Warehouse Qty
```sql
FROM dbo.InventoryItem ii
INNER JOIN dbo.INItemClass ic
    ON ic.CompanyID = ii.CompanyID AND ic.ItemClassID = ii.ItemClassID
LEFT JOIN dbo.INSiteStatus ss
    ON ss.CompanyID = ii.CompanyID AND ss.InventoryID = ii.InventoryID
LEFT JOIN dbo.INSite ins
    ON ins.CompanyID = ss.CompanyID AND ins.SiteID = ss.SiteID
LEFT JOIN dbo.INItemSite isite
    ON isite.CompanyID = ii.CompanyID AND isite.InventoryID = ii.InventoryID AND isite.SiteID = ss.SiteID
WHERE ii.StkItem = 1 AND ii.ItemStatus = 'AC'
```

### Sales Order + Customer + Lines
```sql
FROM dbo.SOOrder so
INNER JOIN dbo.Customer c
    ON c.CompanyID = so.CompanyID AND c.BAccountID = so.CustomerID
INNER JOIN dbo.SOLine sl
    ON sl.CompanyID = so.CompanyID AND sl.OrderType = so.OrderType AND sl.OrderNbr = so.OrderNbr
WHERE so.OrderType = 'SO' AND so.Status NOT IN ('L', 'H')  -- exclude cancelled and on-hold
```

### AR Invoice + Customer + Lines
```sql
-- NOTE: ARInvoice is physically dbo.ARRegister
FROM dbo.ARRegister ar
INNER JOIN dbo.Customer c
    ON c.CompanyID = ar.CompanyID AND c.BAccountID = ar.CustomerID
LEFT JOIN dbo.ARTran art
    ON art.CompanyID = ar.CompanyID AND art.TranType = ar.DocType AND art.RefNbr = ar.RefNbr
WHERE ar.Released = 1
    AND ar.DocType IN ('INV', 'DRM', 'CRM')  -- invoices, debit/credit memos
```

### AP Bill + Vendor + Open Balance (AP Aging)
```sql
-- NOTE: APInvoice is physically dbo.APRegister
FROM dbo.APRegister ap
INNER JOIN dbo.Vendor v
    ON v.CompanyID = ap.CompanyID AND v.BAccountID = ap.VendorID
WHERE ap.Released = 1
    AND ap.OpenDoc = 1
    AND ap.DocType IN ('INV', 'ACR', 'ADR')  -- bills, credit/debit adjustments
```

### AP Bill + Payment Scheduling (Cash Flow Forecast)
```sql
FROM dbo.APRegister ap
INNER JOIN dbo.Vendor v
    ON v.CompanyID = ap.CompanyID AND v.BAccountID = ap.VendorID
WHERE ap.Released = 1
    AND ap.OpenDoc = 1
    AND ap.Voided = 0
    -- Use PayDate for scheduled, DueDate as fallback
ORDER BY ISNULL(ap.PayDate, ap.DueDate)
```

### Inventory Transactions (Released Only)
```sql
FROM dbo.INTran t
INNER JOIN dbo.INRegister r
    ON r.CompanyID = t.CompanyID AND r.DocType = t.TranType AND r.RefNbr = t.RefNbr
INNER JOIN dbo.InventoryItem ii
    ON ii.CompanyID = t.CompanyID AND ii.InventoryID = t.InventoryID
WHERE r.Released = 1
```

### Full Order-to-Cash Chain (SO → Shipment → AR Invoice)
```sql
-- Trace from AR invoice line back to its originating SO
FROM dbo.ARTran art
INNER JOIN dbo.ARRegister ar
    ON ar.CompanyID = art.CompanyID AND ar.DocType = art.TranType AND ar.RefNbr = art.RefNbr
LEFT JOIN dbo.SOLine sl
    ON sl.CompanyID = art.CompanyID 
    AND sl.OrderType = art.SOOrderType 
    AND sl.OrderNbr = art.SOOrderNbr 
    AND sl.LineNbr = art.SOOrderLineNbr
LEFT JOIN dbo.SOOrder so
    ON so.CompanyID = sl.CompanyID 
    AND so.OrderType = sl.OrderType 
    AND so.OrderNbr = sl.OrderNbr
WHERE ar.Released = 1
```

---

## Status Code Reference

### InventoryItem.ItemStatus
| Code | Meaning |
|------|---------|
| `AC` | Active |
| `IN` | Inactive |
| `MS` | Marked for Deletion |
| `NR` | No Receipts |
| `NS` | No Sales |
| `NP` | No Purchases |

### SOOrder.Status
| Code | Meaning |
|------|---------|
| `N` | New |
| `H` | Hold |
| `O` | Open |
| `C` | Completed |
| `L` | Cancelled |
| `B` | Back Order |

### ARInvoice.Status / APInvoice.Status
| Code | Meaning |
|------|---------|
| `N` | New |
| `H` | Hold |
| `B` | Balanced |
| `O` | Open (released, has balance) |
| `C` | Closed (fully paid) |
| `V` | Voided |

### INRegister.Status
| Code | Meaning |
|------|---------|
| `H` | Hold |
| `B` | Balanced |
| `R` | Released |

### Customer.Status / Vendor.Status
| Code | Meaning |
|------|---------|
| `A` | Active |
| `H` | Hold |
| `I` | Inactive |

⚠️ **Always verify status codes on the target tenant.** While these are standard, some tenants may have customizations that add or modify status values. The safest approach is to query `SELECT DISTINCT Status FROM [table]` on the target database (or use a GI to inspect values) before hardcoding filters.
