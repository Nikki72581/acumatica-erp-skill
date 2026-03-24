# Contributing

This skill is maintained by Nicole / Junova Consulting. Contributions, corrections, and additions are welcome — especially field mappings from real client engagement data.

---

## Quick Contribution Guide

### Found a wrong field name or type?

Open an issue or PR against `acumatica-erp/references/dac-table-map.md`. Include:
- Acumatica version you're on
- The table and field in question
- What the docs/DAC says vs. what the physical column actually is
- How you verified it (SSMS query, OData, etc.)

### Discovered a new gotcha?

Add it to **both**:
1. The "Known Gotchas" section in `acumatica-erp/SKILL.md`
2. The relevant table section in `acumatica-erp/references/dac-table-map.md` with a ⚠️ marker

### Adding a new table to the reference map

Follow this template in `dac-table-map.md`:

```markdown
## TableName

**Table**: `dbo.TableName`
**DAC**: `PX.Objects.Module.TableName`
**Screen**: XXXXXX (Screen ID if applicable)

| SQL Column | DAC Field | Type | Notes |
|------------|-----------|------|-------|
| `CompanyID` | (automatic) | `int` | Multi-tenant filter |
| `PrimaryKeyField` | `PrimaryKeyField` | `int` | PK |
| ... | ... | ... | ... |

**Join pattern**: `ON x.CompanyID = y.CompanyID AND x.FK = y.PK`
```

### Updating for a new Acumatica version

1. Update the schema version comment at the top of `SKILL.md`
2. Update `version` in `.claude-plugin/plugin.json`
3. Review all ⚠️-marked fields — verify they still apply
4. Add a `CHANGELOG.md` entry with the date and what changed

---

## Verification Standards

- Prefer SSMS verification on a local instance over documentation-only claims
- If verified via OData only, note that in the field's Notes column
- `[PXDBCalced]` and virtual fields must be explicitly marked — this is the #1 source of `Invalid column name` errors

---

*The goal is zero surprises on client deployments. Document what actually happens, not what the docs say should happen.*
