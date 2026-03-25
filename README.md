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

## Installation

### Claude Code (CLI / Desktop)

Claude Code has full skill support. Two options:

**Option A — Plugin marketplace (recommended for most users)**

Run these commands inside Claude Code:

```
/plugin marketplace add Nikki72581/acumatica-erp-skill
/plugin install acumatica-erp@Nikki72581-acumatica-erp-skill
```

To update later:

```
/plugin update acumatica-erp
```

**Option B — Symlink (if you have this repo cloned locally)**

```bash
ln -s /path/to/acumatica-erp-skill/acumatica-erp ~/.claude/skills/acumatica-erp
```

Replace `/path/to/acumatica-erp-skill` with wherever you cloned this repo. Changes you make to the repo are immediately reflected — no sync step needed.

**Option C — Remote / cloud Claude Code environment**

If you're running Claude Code in a cloud or containerized environment that mounts skills at `/mnt/skills/user/`:

```bash
cd /mnt/skills/user
git clone https://github.com/Nikki72581/acumatica-erp-skill.git
ln -s acumatica-erp-skill/acumatica-erp acumatica-erp
```

To update:

```bash
cd /mnt/skills/user/acumatica-erp-skill && git pull origin main
```

---

### Claude.ai (Web Interface)

Claude.ai does not support custom skill installation from GitHub directly. The closest equivalent is adding the skill content to a **Project's custom instructions**:

1. Open [claude.ai](https://claude.ai) and create or open a Project for your Acumatica work
2. Go to **Project Instructions**
3. Copy the full contents of [`acumatica-erp/SKILL.md`](acumatica-erp/SKILL.md) and paste it in
4. Save

**Limitation:** Project instructions have a size cap, so the full `dac-table-map.md` reference (105KB) cannot be included this way. You can paste in specific table sections as needed, or attach the file in individual conversations.

For the full skill experience including reference files, Claude Code is recommended.

I've also added a zip file in the project called acumatica-erp.zip. This can be downloaded and imported into Claude Skills and it will include the DAC Map. 
---

## Updating the Skill

**GitHub is the source of truth.** Workflow for contributors:

1. Edit files locally in VSCode
2. `git commit` + `git push`
3. Other users run `/plugin update acumatica-erp` (Claude Code) or re-paste updated content (Claude.ai web)

Never edit directly in Claude for permanent changes — use it to discover what needs changing, then commit from VSCode.
