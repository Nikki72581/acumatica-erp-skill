# acumatica-erp-skill

A Claude skill for Acumatica ERP consulting work — SQL views, DAC design, Generic Inquiries, customization projects, and version-specific field mapping.

Maintained by [Junova Consulting](https://junova.co) — Independent ERP + AI Consulting for SMBs.

## What This Skill Does

When loaded into Claude, this skill provides:

- Deep context on Acumatica's data access layer (DACs, BQL, PXProjection, PXGraph)
- Authoritative SQL view patterns for GI-backed reporting
- Version-specific field mappings across 25R1, 25R2, and 26R1
- Known gotchas from real client deployments (virtual fields, phantom rows, multi-tenant joins)
- Customization project deployment best practices for SaaS-hosted tenants

## Structure

```
acumatica-erp/
├── SKILL.md                        # Main skill — loaded by Claude
└── references/
    ├── dac-table-map.md            # Authoritative field mappings for all supported tables
    └── dacs/
        ├── 25R1/README.md          # 25R1 known field behavior (source files TBD)
        ├── 25R2/                   # 25R2 DAC source files + README
        │   ├── README.md
        │   └── PX.Objects/...
        └── 26R1/                   # 26R1 DAC source files + README
            ├── README.md
            └── PX.Objects/...
```

## Versions Covered

| Version | DAC Source Files | Field Map Coverage |
|---------|-----------------|-------------------|
| 25R1 | Notes only (from live deployment) | ✅ Full |
| 25R2 | ✅ Full source | ✅ Full + architecture change notes |
| 26R1 | ✅ Full source | 🔄 In progress |

## Usage

This skill is loaded automatically when placed in the `/mnt/skills/user/` path in Claude's environment. To sync from this repo into Claude:

```bash
cd /mnt/skills/user/acumatica-erp
git pull origin main
```

## Updating the Skill

**GitHub is the source of truth.** Workflow:

1. Edit files locally in VSCode
2. `git commit` + `git push`
3. In Claude: `git pull` to sync

Never edit directly in Claude for permanent changes — use it to discover needed changes, then commit from VSCode.