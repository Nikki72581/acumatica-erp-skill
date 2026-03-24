# Changelog

All notable changes to the Acumatica ERP Skill are documented here.

Format: `[version] — YYYY-MM-DD`

---

## [1.0.0] — 2026-03-24

### Initial Release

Migrated from local Claude skill to versioned GitHub repository.

**SKILL.md coverage:**
- SQL View → PXProjection DAC → Generic Inquiry pipeline (full pattern with code)
- CompanyID multi-tenancy rules
- DAC field attribute quick reference (SQL type ↔ `[PXDBxxx]` ↔ BQL type)
- Customization project best practices and SaaS deployment workflow
- OData / REST API access patterns
- Known gotchas section (6 documented patterns)

**dac-table-map.md coverage:**
- InventoryItem
- INItemClass
- INSite / INSiteStatus / INItemSite (including SubItemID composite PK warning)
- INTran / INRegister (DocType vs TranType ambiguity documented)
- INPIHeader / INPIDetail (SiteID location gotcha documented)
- SOOrder / SOLine
- ARInvoice / ARTran
- Customer
- APInvoice / APTran
- Vendor
- GLTran / Batch / Account / Sub
- CSAnswers (Attributes)
- NoteDoc (Attachments)
- Common Join Patterns
- Status Code Reference

**Schema version**: Acumatica 25R1 Build 25.101.0153.8

---

## Upcoming / Backlog

- [ ] Report Designer (RPx) integration with GIs
- [ ] Webhook and push notification patterns
- [ ] Dashboard widget / pivot table configuration
- [ ] Mobile framework DAC considerations
- [ ] 2025 R2+ schema delta verification
- [ ] Construction and Manufacturing vertical tables
- [ ] Inter-company and multi-currency considerations
- [ ] Attribute (CSAnswers) advanced querying patterns
