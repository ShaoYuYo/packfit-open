# Reusing the PackFit calculation modules

The JavaScript modules can be imported by another ES-module project. There is no npm package or formal semver compatibility promise yet. All input numbers are deliberately unit-bearing.

## Runtime estimate

Example:

    import {calculateRuntime} from "./src/calc.js";
    const result = calculateRuntime({
      capacityWh:56, powerW:12, reservePct:10, efficiencyPct:90
    });
    // result.usableWh === 45.36; result.hours === 3.78

This applies a constant load and a single efficiency factor. Do not double-count losses already included in measured power input.

## Nominal pack geometry and energy

    import {calculatePack} from "./src/calc.js";
    const result = calculatePack({
      series:3, parallel:1, nominalCellV:3.85, cellCapacityMah:5000,
      cellMassG:57, overheadMassG:65,
      xMm:240, yMm:70, zMm:6.8, loadW:16
    });
    // nominal energy ~57.75 Wh, total mass 236 g

Pack envelope volume uses a bounding rectangular box; it does not prove physical fit, safety or actual cell placement.

## Consumer calculations

    import {estimatePowerBank,estimateChargeCost,estimateChargeTime} from "./src/consumer.js";

These take explicit capacity, efficiency and price assumptions. The Windows batteryreport parser is also exported as parseWindowsReport(htmlText). Never inject imported report HTML into a DOM element.

## Usage scheduling

    import {estimateDayPlan} from "./src/planner.js";
    const day = estimateDayPlan({
      capacityWh:56, startPct:80, reservePct:10,
      tasks:[{hours:2,watts:10},{hours:1,watts:14}]
    });
    // day.totalWh === 34; day.fits === true

This uses constant-average watts per segment without sleep and discharge-curve effects.

## Portable scenario data

    import {serializeScenarios,parseScenarioFile} from "./src/scenario-transfer.js";

Format: packfit.scenarios, version 1. Store only validated numeric inputs and human-readable names; input JSON is strictly revalidated and canonicalized.

Use tests as executable examples; open a public Issue before introducing new physics models or external dependencies.
