import { CustomHudLayout, Instance } from "cs_script/point_script";

// Change to fit the layout being tested
const MAX_BOSSES = 2;
const HP_STAGES = 5;

/**
 * Modify this list as needed to show your HUD in action.
 * @type {{ name?: string, hp: number, max: number, type: 'hit' | 'hp' }[]}
 */
const bosses = [
    { hp: 25, max: 100, type: "hit" },
    { name: "Wow", hp: 500, max: 1000, type: "hp" },
    { name: "Hello", hp: 5, max: 10, type: "hp" },
];

const hud = getHud();
hud.Reset();

if (bosses.length > 0) {
    hud.SetHasClass("bosses", "active", true);

    if (bosses.length === 1) {
        hud.SetHasClass("bosses", "single", true);
    } else {
        hud.SetHasClass("bosses", "multi", true);
    }
}

if (bosses.length > MAX_BOSSES) {
    hud.SetHasClass("bosses", "overflown", true);
    hud.SetDialogVariableString(
        "bosses",
        "not_shown_count",
        (bosses.length - MAX_BOSSES).toString(),
    );
} else {
    hud.SetDialogVariableString("bosses", "not_shown_count", "0");
}

for (let i = 0; i < Math.min(MAX_BOSSES, bosses.length); i++) {
    const boss = bosses[i];
    hud.SetHasClass(`boss-${i}`, "active", true);
    hud.SetHasClass(`boss-${i}`, boss.type, true);

    if (boss.name) {
        hud.SetDialogVariableString(`boss-${i}`, "name", boss.name);
        hud.SetHasClass(`boss-${i}`, "named", true);
    }

    hud.SetDialogVariableString(`boss-${i}`, "current_hp", boss.hp.toString());
    hud.SetDialogVariableString(`boss-${i}`, "max_hp", boss.max.toString());
    hud.SetDialogVariableString(
        `boss-${i}`,
        "percentage_rounded",
        Math.round((boss.hp / boss.max) * 100).toString(),
    );

    const segment = Math.round((boss.hp / boss.max) * (HP_STAGES - 1));
    hud.SetHasClass(`boss-${i}`, `hp_stage_${segment}`, true);
}

function getHud() {
    const entity = Instance.FindEntityByName("boss_hud");

    if (entity instanceof CustomHudLayout) {
        return entity;
    } else {
        throw new Error("not found");
    }
}
