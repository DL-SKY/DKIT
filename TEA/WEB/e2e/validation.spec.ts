import { expect, openFixture, test } from "./helpers";

test.describe("validation UI", () => {
  test("search, filters, navigate, Fix and Fix All", async ({ page }) => {
    await page.goto("/");
    await openFixture(page, "validation-fix.json");

    await expect(page.getByRole("heading", { name: "Validation (10)" })).toBeVisible();
    await expect(page.getByTestId("fix-all")).toHaveText("Fix All (9)");

    await page.getByRole("button", { name: "Unfixable", exact: true }).click();
    await expect(page.getByRole("heading", { name: "Validation (1 of 10)" })).toBeVisible();
    await page.getByRole("button", { name: /KEY_1/ }).click();
    await expect(page.getByRole("heading", { name: "Selected Content" })).toBeVisible();

    await page.getByRole("button", { name: "All", exact: true }).click();
    await page.getByTestId("validation-search").fill("go");
    await expect(page.getByRole("heading", { name: "Validation (3 of 10)" })).toBeVisible();
    await page.getByRole("button", { name: /Choice 'go'.*DiceCheck/ }).click();
    await expect(page.getByRole("heading", { name: "Selected Choice" })).toBeVisible();

    await page.getByTestId("validation-search").fill("");
    await page.getByRole("button", { name: "Fixable", exact: true }).click();
    await expect(page.getByRole("heading", { name: "Validation (9 of 10)" })).toBeVisible();

    await page.getByRole("button", { name: "All", exact: true }).click();
    await page.getByRole("button", { name: "Fix", exact: true }).first().click();
    await expect(page.getByTestId("file-status")).toHaveText("Modified");

    await page.getByTestId("fix-all").click();
    await expect(page.getByRole("heading", { name: "Validation (1)" })).toBeVisible();
    await expect(page.getByTestId("fix-all")).toBeDisabled();
    await expect(page.getByText(/KEY_1/)).toBeVisible();
    await expect(page.getByRole("button", { name: "Fix", exact: true })).toHaveCount(0);
  });
});
