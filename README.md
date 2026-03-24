# acumatica-erp-skill

> A Claude Code skill for Acumatica ERP consulting — SQL views, DAC design, Generic Inquiries, PXProjection patterns, BQL, OData, and customization project workflows.

Maintained by **Nicole / [Junova Consulting](https://junova.consulting)** — built from real client engagements, not documentation.

---

## What This Is

This repo contains a Claude Code skill (`SKILL.md`) and a living reference (`references/dac-table-map.md`) that give Claude deep, accurate context when working on Acumatica ERP customizations.

It covers the SQL View → PXProjection DAC → Generic Inquiry pipeline in detail, documents field-level gotchas discovered in production, and maps the most common Acumatica tables used in reporting and integration work.

**Schema version**: Acumatica **25R1 Build 25.101.0153.8** — verify fields if you're on a different build.

---

## Repo Structure

```
acumatica-erp-skill/
├── acumatica-erp/
│   ├── SKILL.md                        # Main skill — Claude reads this
│   ├── references/
│   │   └── dac-table-map.md            # Living DAC → SQL field map
│   └── .claude-plugin/
│       └── plugin.json
└── .claude-plugin/
    └── marketplace.json
```

---

## Skill Coverage

### Core Topics
- **SQL View → PXProjection → GI pipeline** — step-by-step with production-tested patterns
- **CompanyID multi-tenancy rules** — always-on in every join
- **DAC attribute quick reference** — SQL type → `[PXDBxxx]` attribute → BQL type
- **Customization project best practices** — naming, idempotent scripts, SaaS deployment workflow
- **OData / REST API access patterns** — endpoint structure, auth, GI publishing, Atlas architecture notes

### Gotchas (Hard-Won from Client Work)
| Gotcha | Table(s) | Summary |
|--------|----------|---------|
| `LastCost` is virtual | `INItemSite` | No physical SQL column — derive from `INCostStatus` |
| `DocType` vs `TranType` ambiguity | `INTran` | Both columns exist physically; use `t.DocType` for consistency |
| `SiteID` not on `INPIDetail` | `INPIDetail` | Get it from `INPIHeader` via `PIID` join |
| Phantom rows from LEFT JOIN | `INSiteStatus` | Use INNER JOIN when you only want items with warehouse presence |
| `SubItemID` row multiplication | `INSiteStatus` | Pre-aggregate before joining |
| Virtual/calculated DAC fields | General | `[PXDBCalced]` / `[PXDBScalar]` fields have no SQL column |
| `MinQty` labeled "Reorder Point" | `INItemSite` | Field is `MinQty` in SQL/DAC, shown as "Reorder Point" in UI |

### Reference Tables (dac-table-map.md)
| Module | Tables |
|--------|--------|
| Inventory | `InventoryItem`, `INItemClass`, `INSite`, `INSiteStatus`, `INItemSite` |
| IN Transactions | `INTran`, `INRegister` |
| Physical Inventory | `INPIHeader`, `INPIDetail` |
| Sales | `SOOrder`, `SOLine` |
| AR | `ARInvoice`, `ARTran`, `Customer` |
| AP | `APInvoice`, `APTran`, `Vendor` |
| GL | `GLTran`, `Batch`, `Account`, `Sub` |
| Common | `CSAnswers` (attributes), `NoteDoc` (attachments) |

---

## How to Use

### In Claude.ai (Chat Interface)

The skill is loaded automatically if your Claude environment is configured to pull from this repo. Claude will read `SKILL.md` and `references/dac-table-map.md` before writing any Acumatica SQL or DAC code.

Trigger phrases that activate this skill:
- Any mention of "Acumatica", "GI", "Generic Inquiry", "customization project"
- Table names: `InventoryItem`, `INTran`, `ARInvoice`, `SOOrder`, etc.
- Screen IDs: `IN202500`, `SO301000`, etc.
- Concepts: BQL, PXGraph, PXCache, PXSelector, PXProjection

### In Claude Code (CLI)

```bash
# Add this repo as a marketplace (one-time)
/plugin marketplace add your-github-username/acumatica-erp-skill

# Install the skill
/plugin install acumatica-erp@your-github-username-acumatica-erp-skill
```

---

## Maintenance

### Adding a New Table to the Reference

Edit `acumatica-erp/references/dac-table-map.md` and follow the existing table format:

```markdown
## TableName

**Table**: `dbo.TableName`
**DAC**: `PX.Objects.Module.TableName`
**Screen**: XX000000

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CompanyID` | (automatic) | `int` | |
| ...
```

### Adding a New Gotcha

Add it to the **Known Gotchas** section in `SKILL.md` and the corresponding table section in `dac-table-map.md`.

### Version Updates

When upgrading to a new Acumatica build:
1. Update the schema version header in `SKILL.md`
2. Update `acumatica-erp/.claude-plugin/plugin.json` version
3. Verify any fields marked ⚠️ in `dac-table-map.md`
4. Add a changelog entry below

---

## Changelog

| Date | Version | Notes |
|------|---------|-------|
| 2026-03-24 | 1.0.0 | Initial repo migration from local skill. Covers 25R1 Build 25.101.0153.8. |

---

## License

MIT — use freely, attribute appreciated.

---

*Built with real client data, not documentation. If you find an error, open a PR.*
