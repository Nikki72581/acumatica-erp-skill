---
name: acumatica-erp
description: Use this skill whenever the user asks about Acumatica ERP — including SQL views, Generic Inquiries (GIs), DAC design, PXProjection patterns, customization packages, BQL queries, import scenarios, OData API integration, or any reporting/data retrieval task involving Acumatica. Also trigger when the user mentions specific Acumatica tables (InventoryItem, INTran, ARInvoice, SOOrder, etc.), screen IDs (IN202500, SO301000, etc.), or Acumatica-specific concepts like BQL, PXGraph, PXCache, or PXSelector. If the user is building SQL views that will back a GI, troubleshooting GI behavior, designing DACs, or working with Acumatica's customization framework, always consult this skill first. Even casual mentions of "Acumatica", "GI", "generic inquiry", or "customization project" should trigger this skill.
---

# Acumatica ERP — SQL Views, DACs, and Generic Inquiries

This skill provides Claude with deep context on Acumatica ERP internals for building SQL views, designing DACs, configuring Generic Inquiries, and deploying customization packages. It is written and maintained by Nicole (Junova Consulting) based on real-world consulting engagements.

> **Always read `references/dac-table-map.md` before writing any SQL or DAC code.**
> It contains authoritative field mappings, join patterns, and version-tagged gotchas.

> **Supported versions**: This skill covers Acumatica **25R1**, **25R2**, and **26R1** (as encountered).
> Version-specific differences are called out inline with `[25R1]` / `[25R2]` / `[26R1]` tags.
> When no version tag is present, the behavior applies to all supported versions.
> DAC source files for each version are stored in `references/dacs/` by version folder.
> See the **[Version Differences Reference](#version-differences-reference)** section for a consolidated changelog.

---

## Core Architecture: How Data Flows in Acumatica

Acumatica's data access layer sits between the SQL Server database and the UI. Understanding this chain is essential for building correct SQL views and GIs.

```
SQL Server tables
    ↓
DAC (Data Access Class) — C# class mapping columns to typed fields
    ↓
BQL (Business Query Language) — Acumatica's LINQ-like query abstraction
    ↓
PXGraph (Business Logic Controller) — orchestrates CRUD + business rules
    ↓
Generic Inquiry / Screen — UI presentation layer
```

### Key Principles

1. **DACs are the contract.** A GI doesn't read SQL tables directly — it reads DACs. Every field you want in a GI must exist as a DAC property with the correct `[PXDBxxx]` attribute.

2. **PXProjection DACs bridge SQL views to the GI layer.** When a standard DAC doesn't give you the aggregation or joins you need, you create a SQL view in the database and a read-only `[PXProjection]` DAC that maps to it. The GI then reads the projection DAC.

3. **CompanyID is implicit but critical.** Acumatica is multi-tenant at the database level. Every table has a `CompanyID` column. DACs filter on it automatically. SQL views must include `CompanyID` in their output and join conditions, or you'll get cross-tenant data leakage. PXProjection handles this on the DAC side, but your SQL view's JOINs must be CompanyID-aware.

4. **SaaS-hosted tenants cannot use direct DB access.** All customization must go through the Customization Project framework (Database Scripts tab for SQL, Code tab for DACs). No SSMS access, no ad-hoc queries. Self-hosted and Private Cloud tenants may have read-only DB access.

5. **The GI is a single-pass row fetcher.** It lacks window functions, CTEs, multi-level GROUP BY, running totals, and true aggregation. This is the #1 reason SQL views exist — to push that logic to the database and present flat results to the GI.

---

## The SQL View → PXProjection → GI Pattern

This is the most common pattern for advanced reporting in Acumatica. It has been used successfully on multiple Junova client engagements.

### Step 1: Write the SQL View

```sql
CREATE OR ALTER VIEW dbo.vw_YourViewName AS
SELECT
    t1.CompanyID,           -- ALWAYS include CompanyID
    t1.KeyField1,           -- Must match DAC key fields
    t1.KeyField2,
    -- your computed/aggregated columns
    SUM(t2.Amount) AS TotalAmount,
    COUNT(*) AS LineCount
FROM dbo.PrimaryTable t1
INNER JOIN dbo.SecondaryTable t2
    ON t2.CompanyID = t1.CompanyID      -- ALWAYS join on CompanyID
    AND t2.ForeignKey = t1.PrimaryKey
WHERE t1.SomeStatus = 'AC'              -- Filter early for performance
GROUP BY t1.CompanyID, t1.KeyField1, t1.KeyField2
;
GO
```

**Critical rules for SQL views:**
- Always include `CompanyID` in SELECT and GROUP BY
- Always join on `CompanyID` in addition to logical keys
- Use `ISNULL()` liberally — Acumatica has many nullable columns
- Use `CREATE OR ALTER VIEW` so the script is idempotent on republish
- Name views with a `vw_` or `jv_` prefix to distinguish from base tables
- Window functions (ROW_NUMBER, SUM OVER, etc.) work fine in views and are the primary way to get running totals and rankings

### Step 2: Create the PXProjection DAC

```csharp
using PX.Data;
using PX.Data.BQL;

namespace YourNamespace
{
    [Serializable]
    [PXCacheName("Your View Display Name")]
    [PXProjection(typeof(Select<YourViewDAC>), Persistent = false)]  // read-only
    public class YourViewDAC : IBqlTable
    {
        #region CompanyID
        // CompanyID is handled automatically by PXProjection — 
        // you do NOT need to declare it in the DAC
        #endregion

        #region KeyField1
        public abstract class keyField1 : BqlInt.Field<keyField1> { }
        
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Key Field 1")]
        public virtual int? KeyField1 { get; set; }
        #endregion

        #region TotalAmount
        public abstract class totalAmount : BqlDecimal.Field<totalAmount> { }
        
        [PXDBDecimal(2)]
        [PXUIField(DisplayName = "Total Amount")]
        public virtual decimal? TotalAmount { get; set; }
        #endregion
    }
}
```

**Critical rules for PXProjection DACs:**
- `Persistent = false` — this is read-only, no writes
- Every column in the SQL view that you want accessible in the GI must have a DAC field
- The DAC field names must EXACTLY match the SQL view column aliases (case-insensitive on SQL Server, but keep them consistent)
- You need at least one `IsKey = true` field — this should form a unique row identifier
- Use the correct `[PXDBxxx]` attribute matching the SQL data type:
  - `int` → `[PXDBInt]`
  - `varchar/nvarchar` → `[PXDBString(length, IsUnicode = true)]`
  - `decimal/money` → `[PXDBDecimal(precision)]`
  - `datetime` → `[PXDBDate]` or `[PXDBDateAndTime]`
  - `bit` → `[PXDBBool]`
- Do NOT use `[PXDefault]` on projection DACs — there's nothing to default
- BQL abstract class pattern: `public abstract class fieldName : BqlType.Field<fieldName> { }`

### Step 3: Deploy via Customization Project

1. **Database Scripts tab** — Add SQL view as a script. Set execution order if multiple scripts depend on each other.
2. **Code tab** — Add the DAC `.cs` file.
3. **Publish** — SQL scripts execute first, then code compiles. If the DAC references a view that doesn't exist yet, compilation fails. Execution order matters.
4. **Generic Inquiry (SM208000)** — Create a new GI, add the projection DAC as a table, configure columns, conditions, and sorting.

### Step 4: Configure the Generic Inquiry

- **Tables tab**: Add your PXProjection DAC. It appears like any other DAC.
- **Relations tab**: If you need to join to other DACs for display (e.g., show Customer name alongside CustomerID), add the join here. Use Inner/Left joins as appropriate.
- **Conditions tab**: Add any default filters. Users can add their own at runtime.
- **Results Grid tab**: Configure visible columns, column order, and sorting.
- **Parameters tab** (optional): Add runtime parameters that map to Conditions with `@param` syntax.
- **Navigation**: You can make GI rows clickable to navigate to source screens using the Navigation tab.

---

## Common Acumatica Tables and Their Purposes

> **For detailed field mappings, see `references/dac-table-map.md`**

### Inventory Module (IN)
- `InventoryItem` — Master item record (InventoryID, InventoryCD, Descr, ItemClassID, BaseUnit, ItemStatus, StkItem flag)
- `INItemClass` — Item classification (ItemClassID, ItemClassCD, Descr)
- `INSite` — Warehouse/location (SiteID, SiteCD, Descr)
- `INSiteStatus` — Qty on hand per item/warehouse (InventoryID, SiteID, QtyOnHand, QtyAvail, QtyNotAvail). 🚨 **[26R1]** physical table is now `dbo.INSiteStatusByCostCenter` — verify SQL views on 26R1 tenants.
- `INItemSite` — Item-warehouse settings (InventoryID, SiteID, StdCost, DfltReceiptLocationID). ⚠️ `LastCost` is DAC-only — use `INCostStatus` for cost in SQL views. 🚨 `AvgCost` and `TranUnitCost` are computed (not physical) in **[26R1]** — do not reference in SQL on 26R1 tenants.
- `INCostStatus` — Inventory cost layers (InventoryID, CostSiteID, QtyOnHand, TotalCost, UnitCost). The source Acumatica uses internally for valuation. CostSiteID = INSite.SiteID for warehouse-level costing.
- `INTran` — Inventory transaction lines (DocType, TranType, RefNbr, LineNbr, InventoryID, SiteID, Qty, TranAmt, TranDate). ⚠️ Both DocType and TranType exist as physical columns in 25R1 and 25R2.
- `INRegister` — Inventory transaction headers (DocType, RefNbr, Released, TranDate, TotalQty, TotalAmount)
- `INLocation` — Warehouse bin locations (LocationID, LocationCD, SiteID)
- `INPIHeader` — Physical inventory count header (PIID, SiteID, Status, TotalPhysicalQty, TotalVarQty)
- `INPIDetail` — Physical inventory count lines (PIID, LineNbr, InventoryID, PhysicalQty, VarQty, BookQty)

### Sales Order Module (SO)
- `SOOrder` — Sales order header (OrderType, OrderNbr, CustomerID, OrderDate, Status, OrderTotal, CuryOrderTotal)
- `SOLine` — Sales order lines (OrderType, OrderNbr, LineNbr, InventoryID, OrderQty, CuryUnitPrice, CuryLineAmt)
- `SOShipment` — Shipment header (ShipmentNbr, ShipDate, Status, ShipmentQty)
- `SOShipLine` — Shipment lines (ShipmentNbr, LineNbr, InventoryID, ShippedQty)

### Accounts Receivable (AR)
- `ARInvoice` — AR invoice/credit memo header (DocType, RefNbr, CustomerID, DocDate, Status, CuryDocBal, CuryOrigDocAmt)
- `ARTran` — AR invoice lines (TranType, RefNbr, LineNbr, InventoryID, Qty, CuryTranAmt)
- `ARPayment` — AR payment header (DocType, RefNbr, CustomerID, AdjDate, CuryOrigDocAmt)
- `Customer` — Customer master (BAccountID, AcctCD, AcctName, CustomerClassID, Status)

### Accounts Payable (AP)
- `APInvoice` — AP bill header (DocType, RefNbr, VendorID, DocDate, Status, CuryDocBal)
- `APTran` — AP bill lines (TranType, RefNbr, LineNbr, InventoryID, Qty, CuryTranAmt)
- `APPayment` — AP payment header
- `Vendor` — Vendor master (BAccountID, AcctCD, AcctName, VendorClassID)

### General Ledger (GL)
- `GLTran` — GL transaction lines (Module, BatchNbr, LineNbr, AccountID, SubID, DebitAmt, CreditAmt, TranDate)
- `Batch` — GL batch header (Module, BatchNbr, Status, DateEntered)
- `Account` — GL account master (AccountID, AccountCD, Description, Type)
- `Sub` — GL subaccount (SubID, SubCD, Description)

### Project Module (PM)
- `PMProject` — Project header (ContractID, ContractCD, Description, Status, CustomerID)
- `PMTask` — Project task (ProjectID, TaskID, TaskCD, Description, Status)
- `PMTran` — Project transactions (TranID, ProjectID, TaskID, AccountGroupID, Amount, Qty, Date)
- `PMBudget` — Project budget lines

### Shared / System
- `BAccount` — Base business account (parent of Customer, Vendor, Employee)
- `Contact` — Contact records
- `Address` — Address records
- `NoteDoc` — File attachments (NoteID links to any record's NoteID field)
- `CSAnswers` — Attribute values (RefNoteID, AttributeID, Value) — this is how user-defined attributes are stored

---

## Known Gotchas and Troubleshooting

### Duplicate Rows in GI Results
**Symptom**: GI shows more rows than expected, with apparent duplicates.
**Common causes**:
1. Missing `CompanyID` in SQL view joins → cross-tenant cartesian product
2. JOIN to a table with multiple rows per key (e.g., INTran has many lines per item) without proper aggregation
3. GI Relations tab has an unintended cross-join — check that every relation has proper ON conditions
4. The view's key columns don't form a unique combination → PXProjection can't distinguish rows

**The DSDInventoryForWeb fix** (real example): Duplicate rows were caused by a GI joining to a table that had multiple status records per item. Fix was to replace the GI's direct table join with a SQL view that pre-aggregated the data, then use PXProjection to present the flat result.

### Missing Records in GI
**Symptom**: GI shows fewer rows than expected.
**Common causes**:
1. INNER JOIN in SQL view to a table with missing records — switch to LEFT JOIN
2. WHERE clause too restrictive (e.g., filtering by `ItemStatus = 'AC'` excludes items in other statuses)
3. GI Conditions tab has filters the user forgot about
4. The DAC's BQL `Select<>` in the PXProjection attribute is filtering rows — for SQL views, keep this simple

### PXProjection Won't Compile
**Common causes**:
1. SQL view doesn't exist yet — check Database Scripts execution order
2. DAC field name doesn't match view column alias exactly
3. Missing `using` statements in the `.cs` file
4. `[PXDBString]` without length parameter
5. No `IsKey = true` field defined
6. Incorrect BQL abstract class — must follow pattern: `public abstract class fieldName : BqlType.Field<fieldName> { }`

### Import Scenarios Triggering Unexpected Validation
**Lesson**: When an import fails with an unexpected validation error, check the customization project for `RowPersisting`, `RowUpdating`, or `FieldVerifying` event handlers on the affected DAC. These fire on ALL persist operations unless explicitly scoped.

### GI Performance Issues
1. SQL views with no indexes on the base tables will be slow — check execution plans
2. Avoid `SELECT *` — only include columns the GI actually needs
3. Use `ISNULL(col, default)` instead of COALESCE for single-column checks (minor but consistent)
4. For large datasets, consider adding `WITH (NOLOCK)` hints — acceptable for read-only reporting views
5. Window functions are powerful but expensive on large tables — filter early in a CTE before applying them
6. When a subquery scans INTran→INRegister for multiple aggregations (e.g., last txn date AND 90-day velocity), consolidate into a single pass using conditional aggregation (`CASE WHEN ... THEN ... END` inside `COUNT`/`SUM`) rather than separate subqueries

### INItemSite.LastCost — Behavior Differs by Version
🚨 **This is a confirmed, deployment-breaking gotcha. Read the version notes carefully.**

#### [25R1] — LastCost is fully virtual (NO physical SQL column)

**Symptom**: SQL view fails with `Invalid column name 'LastCost'` on INItemSite.

**Root cause**: `INItemSite.LastCost` exists in the DAC (visible in UI, accessible via BQL/OData) but has **no physical column** in the SQL table. The DAC computes it at runtime from `INCostStatus` cost layers. Same applies to `LastCostDate`.

**The fix for 25R1** — use `INCostStatus` cost layers:
```sql
LEFT JOIN (
    SELECT
        cs.CompanyID, cs.InventoryID, cs.CostSiteID,
        CASE WHEN SUM(cs.QtyOnHand) > 0 
             THEN SUM(cs.TotalCost) / SUM(cs.QtyOnHand)
             ELSE 0 END AS UnitCost
    FROM dbo.INCostStatus cs
    WHERE cs.QtyOnHand > 0
    GROUP BY cs.CompanyID, cs.InventoryID, cs.CostSiteID
) cost ON cost.CompanyID = ii.CompanyID 
    AND cost.InventoryID = ii.InventoryID 
    AND cost.CostSiteID = ss.SiteID
```
`CostSiteID` maps to `INSite.SiteID` for warehouse-level costing (most common). Confirmed on Island Parts & Supplies (SaaS-hosted, 25R1) and local 25R1 demo instance.

#### [25R2] — INItemSite is now a PXProjection; LastCost backed by INItemStats

In 25R2, the `INItemSite` DAC was restructured as a `[PXProjection]` joining `dbo.INItemSite + dbo.INSite + dbo.INItemStats`. `LastCost` now uses `[PXDBPriceCost(BqlField = typeof(INItemStats.lastCost))]` — it physically lives in `INItemStats`, not `INItemSite`.

- **Still do NOT reference `isite.LastCost` in SQL** — the `dbo.INItemSite` SQL table (the base table) doesn't have the column; only the DAC projection does.
- **To use `INItemStats.LastCost` in a 25R2 SQL view**, join explicitly: `LEFT JOIN dbo.INItemStats ist ON ist.CompanyID = isite.CompanyID AND ist.InventoryID = isite.InventoryID AND ist.SiteID = isite.SiteID`
- **The `INCostStatus` approach still works in 25R2** and is the recommended cross-version pattern.

**Cross-version rule**: Use `INCostStatus` aggregation for all versions unless you have a specific reason to prefer `INItemStats`. It works on 25R1, 25R2, and is likely stable going forward.

See `dac-table-map.md` Cost Fields section for full details.

### INTran — DocType vs TranType Column Ambiguity
⚠️ **Both `DocType` and `TranType` exist as physical columns on `INTran` across all versions, but they are NOT the same values.**

- `DocType` = 1-char document-level type (`R`, `I`, `T`, `A`, `P`) — matches `INRegister.DocType`. This is the join key.
- `TranType` = 3-char line-level transaction type (`RCP`, `ISS`, `TFR`, `ADJ`) — different values.

**Always join INRegister to INTran on `r.DocType = t.DocType AND r.RefNbr = t.RefNbr`** — do not join on `TranType`. The 26R1 DAC source confirms this distinction explicitly. Earlier guidance suggesting "both contain the same values" was incorrect; they represent different concepts at different granularities.

### INPIDetail.SiteID — Version-Dependent Physical Column Presence
⚠️ **Behavior differs by version.**

**[25R1]**: `SiteID` is NOT reliably present as a physical column on `INPIDetail`. The documented DAC field map for `INPIDetail` shows only `PIID`, `LineNbr`, `InventoryID`, `LocationID`, `BookQty`, `PhysicalQty`, `VarQty`. Always get `SiteID` from `INPIHeader` by joining through `PIID` on 25R1.

**[25R2]**: `SiteID` IS present as a proper FK field on `INPIDetail` (`[PXDefault(typeof(INPIHeader.siteID))]` with `[Site()]`). It is included in the FK definitions and appears to be a physical column.

**Safe cross-version pattern** (works on both): Always join to `INPIHeader` for SiteID rather than reading `d.SiteID` directly. This is safer and more explicit:
```sql
INNER JOIN dbo.INPIHeader h ON h.CompanyID = d.CompanyID AND h.PIID = d.PIID
-- Use h.SiteID, not d.SiteID
```

### Phantom Rows from LEFT JOIN to INSiteStatus
**Symptom**: GI returns thousands of unexpected rows with NULL warehouse, zero qty, zero cost, zero transactions.

**Root cause**: Using `LEFT JOIN` to `INSiteStatus` from `InventoryItem` returns a row for every active stock item, including items that have never been stocked at any warehouse (no `INSiteStatus` record exists). These phantom rows cascade NULLs through every downstream join.

**The fix**: Use `INNER JOIN` to `INSiteStatus` when the view should only include items with warehouse presence. For cycle count and inventory reporting views, items with no warehouse presence have nothing to count.

**Scale of impact**: On Island Parts (SaaS, 25R1), this accounted for ~50% of returned rows being useless NULL-warehouse records. The full dataset went from 27K+ rows to a manageable warehouse-specific count after switching to INNER JOIN.

### INSiteStatus — Underlying Table Changed in 26R1

🚨 **[26R1] The physical SQL table backing `INSiteStatus` changed to `INSiteStatusByCostCenter`.**

In 25R1 and 25R2, SQL views could join `dbo.INSiteStatus` directly. In 26R1, the `INSiteStatus` DAC is a projection via the `[INSiteStatusProjection]` custom attribute, where every field maps via `BqlField = typeof(INSiteStatusByCostCenter.xxx)`. The physical data now lives in `dbo.INSiteStatusByCostCenter`.

**Impact on existing SQL views**: Any SQL view joining `dbo.INSiteStatus` on a 26R1 tenant must be tested. If the table doesn't exist, the view will fail. Switch to `dbo.INSiteStatusByCostCenter` on 26R1. The schema (columns, PK, CompanyID join) is the same — only the table name changes.

**Pre-aggregation pattern for 26R1** (same logic, different source table):
```sql
INNER JOIN (
    SELECT CompanyID, InventoryID, SiteID,
           SUM(ISNULL(QtyOnHand, 0)) AS QtyOnHand,
           SUM(ISNULL(QtyAvail, 0))  AS QtyAvail
    FROM dbo.INSiteStatusByCostCenter  -- [26R1] use this instead of INSiteStatus
    GROUP BY CompanyID, InventoryID, SiteID
) ss ON ss.CompanyID = ii.CompanyID AND ss.InventoryID = ii.InventoryID
```

### INSiteStatus SubItemID — Silent Row Multiplication
⚠️ **`INSiteStatus` has a composite PK of `(InventoryID, SiteID, SubItemID)`.**

If a tenant has sub-items enabled (even partially), joining directly to `INSiteStatus` can produce multiple rows per item-warehouse combination. Pre-aggregate before joining:
```sql
INNER JOIN (
    SELECT CompanyID, InventoryID, SiteID,
           SUM(ISNULL(QtyOnHand, 0)) AS QtyOnHand,
           SUM(ISNULL(QtyAvail, 0))  AS QtyAvail
    FROM dbo.INSiteStatus
    GROUP BY CompanyID, InventoryID, SiteID
) ss ON ss.CompanyID = ii.CompanyID AND ss.InventoryID = ii.InventoryID
```

### DAC Field Names vs SQL Column Names — General Principle
🚨 **Not all DAC fields map to physical SQL columns.**

This is the single most common cause of `Invalid column name` errors in Acumatica SQL views. The DAC layer includes virtual/calculated fields (using `[PXDBCalced]`, `[PXDBScalar]`, or unbound `[PXxxx]` attributes) that exist at runtime but have no physical column in the SQL table. The Acumatica UI and OData API show these fields alongside physical ones with no visible distinction.

**Rule**: Before using any DAC field in a SQL view, verify it exists as a physical column — either by querying `INFORMATION_SCHEMA.COLUMNS` on a local instance or by checking the DAC source for `[PXDBCalced]`/`[PXDBScalar]` attributes. Known virtual/non-physical fields documented so far:
- `INItemSite.LastCost` — **[25R1]** virtual, computed from `INCostStatus`; **[25R2/26R1]** physical on `INItemStats` (not `INItemSite`), requires explicit join
- `INItemSite.LastCostDate` — **[all versions]** unbound (`[PXDate]` only, no DB backing) — never directly queryable in SQL
- `INItemSite.AvgCost` — **[25R1/25R2]** physical on `dbo.INItemSite`; **[26R1]** `[PXDBPriceCostCalced]` computed from `INItemStats.TotalCost / INItemStats.QtyOnHand` — NO physical column
- `INItemSite.TranUnitCost` — **[25R1/25R2]** physical on `dbo.INItemSite`; **[26R1]** `[PXDBCalced]` expression — NOT a physical column
- `INItemSite.MinCost` / `INItemSite.MaxCost` — **[25R1/25R2]** physical on `dbo.INItemSite`; **[26R1]** `BqlField` redirect to `INItemStats` — physically on `dbo.INItemStats`
- `INItemSite.POCreate` / `INItemSite.POSource` — **[25R1/25R2]** physical; **[26R1]** `[PXDBCalced]` derived from `ReplenishmentSource`
- `INItemSite.DemandPerDaySTDEV` / `INItemSite.LeadTimeSTDEV` — **[26R1]** confirmed unbound `[PXDecimal]` — computed in getter as `Math.Sqrt(MSE)`, no SQL column
- `INSiteStatus.QtyExpired` — **[all versions]** fully unbound `[PXDecimal]`, no physical column even in the backing table

---

## DAC Field Attribute Quick Reference

| SQL Type | DAC Attribute | BQL Type |
|----------|--------------|----------|
| `int` | `[PXDBInt]` | `BqlInt` |
| `bigint` | `[PXDBLong]` | `BqlLong` |
| `smallint` | `[PXDBShort]` | `BqlShort` |
| `bit` | `[PXDBBool]` | `BqlBool` |
| `varchar(n)` | `[PXDBString(n)]` | `BqlString` |
| `nvarchar(n)` | `[PXDBString(n, IsUnicode = true)]` | `BqlString` |
| `decimal(p,s)` | `[PXDBDecimal(s)]` | `BqlDecimal` |
| `money` | `[PXDBDecimal(4)]` | `BqlDecimal` |
| `datetime` | `[PXDBDate]` | `BqlDateTime` |
| `datetime2` | `[PXDBDateAndTime]` | `BqlDateTime` |
| `uniqueidentifier` | `[PXDBGuid]` | `BqlGuid` |
| `float` | `[PXDBDouble]` | `BqlDouble` |

---

## OData / REST API Access

Acumatica exposes data via OData and a contract-based REST API. Relevant for external reporting tools and the Atlas advanced query tool concept.

- **Endpoint pattern**: `https://{instance}/entity/{endpoint}/{version}/{entity}`
- **Authentication**: OAuth 2.0 or basic auth with session cookie
- **GIs are exposable via OData**: Any GI can be published as an OData endpoint from the GI configuration screen
- **Limitations**: No server-side aggregation via OData — all GROUP BY must happen client-side or be pre-computed in a SQL view
- **Rate limits**: Varies by hosting plan; SaaS-hosted tenants have stricter throttling
- **For the Atlas tool concept**: The architecture uses OData to fetch data into a per-tenant Postgres staging database, where advanced queries (CTEs, window functions, multi-level grouping) run locally

---

## Customization Project Best Practices

1. **One project per client engagement** — don't pile unrelated customizations together
2. **Naming convention**: `JV_{ClientCode}_{Purpose}` (e.g., `JV_IPS_CycleCount`)
3. **Database scripts are idempotent** — use `CREATE OR ALTER VIEW` and `IF NOT EXISTS` patterns
4. **Version your scripts** — add a comment block with date, author, and change description
5. **Test on a sandbox tenant first** — especially for SaaS-hosted clients
6. **Document the DAC-to-view mapping** — future maintainers need to know which view backs which DAC
7. **Keep Database Scripts clean** — only `CREATE OR ALTER VIEW` / `CREATE OR ALTER PROCEDURE` statements in published scripts. Diagnostic `SELECT` queries should be in a separate file for manual use, not published to the client.
8. **Validate column names on a local instance first** — before publishing to SaaS, run the SQL on a local instance matching the client's build version to catch `Invalid column name` errors that would otherwise only appear in the publish log. The version-specific DAC source files in `references/dacs/` can help identify which columns are physical vs virtual for a given version.

### SaaS Deployment Workflow

For SaaS-hosted clients where you have no SSMS access:

1. **Write SQL views** — reference the `dac-table-map.md` but verify any cost or calculated fields against the known virtual field list in the gotchas section
2. **Test locally** — run the SQL against a local Acumatica instance of the **same build version as the client tenant**. Confirm column names exist and joins produce rows. Column-level differences between 25R1 and 25R2 are documented in the Version Differences Reference section.
3. **Validate data assumptions via OData** — if the client has GIs exposed via OData, use them to check data-level assumptions (e.g., does CostSiteID map to SiteID? Are there released transactions?). OData is the only way to query live client data without SSMS.
4. **Publish** — deploy through the Customization Project Database Scripts tab. The publish log only shows errors, not SELECT results.
5. **Verify via GI** — after publish, check the Generic Inquiry for expected row counts, populated columns, and data reasonableness. Export a sample to Excel for analysis if needed.
6. **Iterate** — if the GI shows unexpected data (phantom rows, zero values), analyze the export, identify the root cause, and republish. Each iteration requires a full customization project publish.

---

---

## Version Differences Reference

This section is the consolidated changelog of known schema and DAC architecture differences across supported versions. **Always check this section first when starting work on a new client tenant** — confirm their version and cross-reference any tables you plan to query.

### How to Identify a Tenant's Version

In Acumatica: **Help → About** or check the URL pattern for build info. The customization project publish log also displays the build number. Key versions:
- **25R1** = Build 25.101.xxxx (e.g., 25.101.0153.8)
- **25R2** = Build 25.200.xxxx / 25.201.xxxx
- **26R1** = Build 26.101.xxxx

### INItemSite — Major Architecture Change in 25R2, Continued in 26R1

| Area | 25R1 | 25R2 | 26R1 |
|------|------|------|------|
| **DAC structure** | Plain table DAC | `[PXProjection]` joining `INItemSite + INSite + INItemStats` | Same `[PXProjection]` joining `INItemSite + INSite + INItemStats` |
| **`LastCost` field** | Virtual — no physical column, computed from `INCostStatus` at runtime | `[PXDBPriceCost(BqlField = typeof(INItemStats.lastCost))]` — physically on `dbo.INItemStats` | Same as 25R2 — physically on `dbo.INItemStats` |
| **`LastCostDate` field** | Virtual — no physical column | `[PXDate]` (unbound) — still no direct SQL column | `[PXDate]` (unbound) — no SQL column |
| **`MinCost` / `MaxCost`** | Physical columns on `dbo.INItemSite` | Moved to `INItemStats` | `[PXDBPriceCost(BqlField = typeof(INItemStats.minCost/maxCost))]` — physically on `dbo.INItemStats` |
| **`AvgCost`** | Physical column on `dbo.INItemSite` | Physical on `dbo.INItemSite`, calculated via `[PXDBPriceCostCalced]` from `INItemStats` | 🚨 `[PXPriceCost]` + `[PXDBPriceCostCalced]` — **NO physical column at all.** Computed as `INItemStats.TotalCost / INItemStats.QtyOnHand`. Do NOT reference in SQL. |
| **`TranUnitCost`** | Physical column on `dbo.INItemSite` | Physical column on `dbo.INItemSite` | 🚨 `[PXDBCalced]` expression (`Switch<...stdCost...INItemStats...>`) — **NOT a physical column.** Do NOT reference in SQL. |
| **`DfltPutawayLocationID`** | Not present | Not present | ✅ New physical column — default putaway location for WMS workflows |
| **`MinQty` UI label** | "Reorder Point" (gotcha persists) | "Reorder Point" (gotcha persists) | "Reorder Point" (gotcha persists) |
| **SQL approach** | Use `INCostStatus` for cost; `isite.*` for all other fields | Use `INCostStatus` for cost (still best cross-version); optionally join `dbo.INItemStats` for 25R2-native cost | Use `INCostStatus` for cost. Do not use `isite.AvgCost` or `isite.TranUnitCost` in SQL — both are computed |

🚨 **26R1 Action Required**: If any SQL view uses `isite.AvgCost` or `isite.TranUnitCost`, those references will fail with `Invalid column name` on 26R1 tenants. Replace with `INCostStatus` aggregation or explicit `INItemStats` join.

### INPIDetail — SiteID Column Presence

| Area | 25R1 | 25R2 | 26R1 |
|------|------|------|------|
| **`SiteID` physical column** | Not reliably present — use `INPIHeader.SiteID` via join | Present as FK field (`[Site()]` attribute with `[PXDefault(typeof(INPIHeader.siteID))]`) | ✅ Physical column confirmed — `[Site()]` compound attr, no BqlField redirect |
| **Safe pattern** | Join to `INPIHeader` | Join to `INPIHeader` (still safest cross-version) | Join to `INPIHeader` (still safest cross-version) |

### INTran — DocType / TranType

| Area | 25R1 | 25R2 | 26R1 |
|------|------|------|------|
| **Both columns present** | ✅ Confirmed via `INFORMATION_SCHEMA.COLUMNS` | ✅ Confirmed via DAC source | ✅ Confirmed via DAC source |
| **`DocType` length** | 1-char | 1-char | 1-char (`PXDBString(1, IsFixed=true)`) |
| **`TranType` length** | 3-char | 3-char | 3-char (`PXDBString(3, IsFixed=true)`) — line-level txn type (ISS, RCP, etc.) |
| **Recommended join field** | `t.DocType` (matches `INRegister.DocType`) | `t.DocType` (same) | `t.DocType` (same) |

⚠️ **Clarification**: `DocType` and `TranType` on `INTran` are NOT the same values. `DocType` is the 1-char header type (mirrors `INRegister.DocType`). `TranType` is the 3-char line-level code (e.g., `ISS`, `RCP`, `TFR`). Always join INRegister to INTran on `DocType` + `RefNbr`.

### INSiteStatus — Composite PK / SubItemID / Underlying Table Change

| Area | 25R1 | 25R2 | 26R1 |
|------|------|------|------|
| **PK structure** | `(InventoryID, SubItemID, SiteID)` — SubItemID row-multiplies | `(InventoryID, SubItemID, SiteID)` — same structure confirmed | Same composite PK |
| **Physical SQL table** | `dbo.INSiteStatus` | `dbo.INSiteStatus` | 🚨 `dbo.INSiteStatusByCostCenter` — the DAC is now a projection over this table |
| **SQL query pattern** | `FROM dbo.INSiteStatus` | `FROM dbo.INSiteStatus` | Verify on 26R1 tenant: `dbo.INSiteStatus` may not exist. Use `dbo.INSiteStatusByCostCenter` if needed. |
| **Pre-aggregation** | Required to avoid SubItemID row-multiplication | Same | Same |
| **New ML fields** | Not present | Not present | `QtyMLPrepared`, `QtyMLBooked`, `QtyMLDispatched`, `QtyMLAllocated`, `QtyMLToPurchase`, `QtyPurchaseForML`, `QtyPurchaseForMLPrepared`, `QtyReceiptsForML` — backed by `INSiteStatusByCostCenter` columns |

🚨 **26R1 Breaking Change Risk**: In 26R1 the `INSiteStatus` DAC is a `[INSiteStatusProjection]` over `dbo.INSiteStatusByCostCenter`. All DAC fields map via `BqlField = typeof(INSiteStatusByCostCenter.xxx)`. SQL views referencing `dbo.INSiteStatus` directly must be verified on a 26R1 instance — the table name may have changed. Until confirmed, use `dbo.INSiteStatusByCostCenter` on 26R1 tenants.

### 26R1 — New Fields Summary

Fields confirmed added in 26R1 DAC source (not present in 25R2):

| Table/DAC | New Field | Type | Purpose |
|-----------|-----------|------|---------|
| `INItemSite` | `DfltPutawayLocationID` | `int` | Default putaway bin for WMS workflows |
| `INSiteStatusByCostCenter` | `QtyMLPrepared` | `decimal` | ML-driven allocation: prepared qty |
| `INSiteStatusByCostCenter` | `QtyMLBooked` | `decimal` | ML-driven allocation: booked qty |
| `INSiteStatusByCostCenter` | `QtyMLDispatched` | `decimal` | ML-driven allocation: dispatched qty |
| `INSiteStatusByCostCenter` | `QtyMLAllocated` | `decimal` | ML-driven allocation: allocated qty |
| `INSiteStatusByCostCenter` | `QtyMLToPurchase` | `decimal` | ML purchase planning: to purchase qty |
| `INSiteStatusByCostCenter` | `QtyPurchaseForML` | `decimal` | ML purchase planning: purchased qty |
| `INSiteStatusByCostCenter` | `QtyPurchaseForMLPrepared` | `decimal` | ML purchase planning: prepared qty |
| `INSiteStatusByCostCenter` | `QtyReceiptsForML` | `decimal` | ML purchase planning: received qty |
| `INTran` | `IsSpecialOrder` | `bit` | Whether this is a special order transaction |
| `INTran` | `InventorySource` | `char(1)` | Material source for project accounting (`InventorySourceType` list) |
| `INTran` | `CostLayerType` | `char(1)` | Distinguishes Normal vs. Special Order cost layers |
| `INTran` | `CostCenterID` | `int` | Cost center identifier for multi-cost-center setups |
| `INRegister` | `IsCorrection` | `bit` | Whether document is a correction transaction |
| `INRegister` | `OrigReceiptNbr` | `nvarchar(15)` | Original receipt reference for corrections |
| `INRegister` | `IgnoreAllocationErrors` | `bit` | Skip allocation validation errors on release |
| `INSite` | `BuildingID` | `int` | FK → INSiteBuilding (building/facility grouping) |
| `INSite` | `CarrierFacility` | `nvarchar` | Carrier facility code for shipping |
| `INSite` | `NonStockPickingLocationID` | `int` | Default location for non-stock item picking |
| `INItemClass` | `PlanningMethod` | `nvarchar` | MRP planning method default for the class |
| `INItemClass` | `ReplenishmentSource` | `nvarchar` | Replenishment source default |
| `INItemClass` | `PostToExpenseAccount` | `nvarchar` | Expense account posting setting |
| `INItemClass` | `PreferredItemClasses` | `nvarchar` | Related item class preferences |
| `INItemClass` | `PriceOfSuggestedItems` | `nvarchar` | Price rule for substitution suggestions |

---

## What This Skill Does NOT Cover (Yet)

The following areas should be added as Nicole encounters them in practice:

- [ ] Report Designer (RPx) integration with GIs
- [ ] Webhook and push notification patterns
- [ ] Acumatica's built-in pivot table / dashboard widget configuration
- [ ] Mobile framework DAC considerations
- [x] Acumatica 25R2 schema changes — documented in Version Differences Reference above
- [x] 26R1 schema changes — documented from 26R1 DAC source files (March 2026). Key changes: INSiteStatus now projects `INSiteStatusByCostCenter`, INItemSite.AvgCost and TranUnitCost are no longer physical columns, new ML quantity fields, new INTran cost layer fields.
- [ ] Construction and Manufacturing vertical-specific tables
- [ ] Inter-company and multi-currency considerations in views
- [ ] Attribute (CSAnswers) querying patterns in detail
- [ ] `INItemStats` table — full field map needed (confirmed physical table in 25R2+; backs INItemSite projection; confirmed columns: InventoryID, SiteID, LastCost, MinCost, MaxCost, TotalCost, QtyOnHand, LastCostDate)
