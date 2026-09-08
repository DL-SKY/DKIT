import { expect, openFixture, test } from "./helpers";

test.describe("localization, graph, dice check", () => {
  test("Generate preview does not dirty; Apply writes keys", async ({ page }) => {
    await page.goto("/");
    await openFixture(page, "localization-demo.json");
    await page.getByTestId("toolbar-localization").click();
    await expect(page.getByRole("dialog", { name: "TEA Localization" })).toBeVisible();

    await page.getByRole("button", { name: "Generate Localization Keys" }).click();
    await expect(page.getByText(/Updated fields: 7/)).toBeVisible();
    await expect(page.getByTestId("file-status")).toHaveText("Saved");
    await expect(page.locator(".meta-panel").getByRole("textbox", { name: "Title", exact: true })).toHaveValue(
      "Hello world",
    );

    const tsvDownloadPromise = page.waitForEvent("download");
    await page.getByRole("button", { name: "Download TSV" }).click();
    const tsvDownload = await tsvDownloadPromise;
    expect(tsvDownload.suggestedFilename()).toMatch(/^LOCALIZATION_DEMO_localization_\d{8}_\d{6}\.txt$/);

    await page.getByRole("button", { name: "Apply Keys" }).click();
    await expect(page.getByTestId("file-status")).toHaveText("Modified");
    await expect(page.locator(".meta-panel").getByRole("textbox", { name: "Title", exact: true })).toHaveValue(
      "LOCALIZATION_DEMO_ADV_TITLE",
    );

    await page.getByRole("button", { name: "Generate Localization Keys" }).click();
    await expect(page.getByText(/Updated fields: 0/)).toBeVisible();
  });

  test("Scene Graph overlay opens and filters start / unreachable / broken", async ({ page }) => {
    await page.goto("/");
    await openFixture(page, "graph-filter.json");
    await page.getByTestId("toolbar-graph").click();
    await expect(page.getByRole("dialog", { name: "Scene Graph Preview" })).toBeVisible();
    await expect(page.getByTestId("graph-stats")).toContainText("Nodes: 2");
    await expect(page.getByTestId("graph-stats")).toContainText("Broken: 1");
    await expect(page.getByTestId("graph-stats")).toContainText("Unreachable: 1");
    await expect(page.locator(".graph-node")).toHaveCount(3);

    await page.getByTestId("graph-filter-unreachable").click();
    await expect(page.getByTestId("graph-stats")).toContainText("Showing: 1 of 3");
    await expect(page.locator(".graph-node")).toHaveCount(1);
    await expect(page.locator(".graph-node.unreachable")).toHaveCount(1);

    await page.getByTestId("graph-filter-broken").click();
    await expect(page.locator(".graph-node")).toHaveCount(1);
    await expect(page.locator(".graph-node.broken")).toHaveCount(1);

    await page.getByTestId("graph-filter-start").click();
    await expect(page.locator(".graph-node")).toHaveCount(1);
    await expect(page.locator(".graph-node.start")).toHaveCount(1);

    await page.getByTestId("graph-filter-all").click();
    await expect(page.locator(".graph-node")).toHaveCount(3);
  });

  test("adds DiceCheck choice, outcome action and restriction profiles", async ({ page }) => {
    await page.goto("/");
    await openFixture(page, "minimal.json");

    await page.getByRole("button", { name: "Dice Check" }).click();
    await expect(page.getByRole("heading", { name: "Selected Choice" })).toBeVisible();
    await expect(page.getByRole("heading", { name: "Dice Check" })).toBeVisible();
    await page.getByLabel("Difficulty Class").fill("12");

    await page.getByRole("button", { name: "Go To Scene" }).first().click();
    await expect(page.getByText("0: GoToScene")).toBeVisible();

    await page.locator(".choices-panel").getByRole("button", { name: "Add Restriction" }).click();
    await expect(page.locator(".choices-panel").getByText("Restriction 1")).toBeVisible();
    const typeSelect = page.locator(".choices-panel .restriction-card").first().getByLabel("Type");
    await typeSelect.selectOption("TimeNow");
    await expect(page.locator(".restriction-card").first().getByLabel("Compare")).toBeVisible();
    await typeSelect.selectOption("ActivePartyCount");
    await expect(page.locator(".restriction-card").first().getByLabel("Compare")).toBeVisible();
    await expect(page.getByTestId("file-status")).toHaveText("Modified");
  });
});
