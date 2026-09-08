import { chooseFixture, expect, fixturePath, openFixture, test } from "./helpers";

test.describe("empty state and open", () => {
  test("empty toolbar keeps editor actions disabled", async ({ page }) => {
    await page.goto("/");
    await expect(page.getByRole("heading", { name: "Adventure Editor" })).toBeVisible();
    await expect(page.getByTestId("toolbar-new-file")).toBeEnabled();
    await expect(page.getByTestId("toolbar-save")).toBeDisabled();
    await expect(page.getByTestId("toolbar-save-as")).toBeDisabled();
    await expect(page.getByTestId("toolbar-graph")).toBeDisabled();
    await expect(page.getByTestId("toolbar-localization")).toBeDisabled();
    await expect(page.getByRole("heading", { name: /Validation/ })).toHaveCount(0);
    await expect(page.getByTestId("file-status")).toHaveText("No file");
    await expect(page.getByTestId("save-mode")).toHaveText("—");
  });

  test("creates a new adventure from empty state", async ({ page }) => {
    await page.goto("/");
    const onDialog = (dialog: { accept: (value?: string) => Promise<void> }) => {
      if (dialogCount === 0) {
        dialogCount += 1;
        void dialog.accept("new_from_scratch");
        return;
      }
      void dialog.accept("intro_scene");
      page.off("dialog", onDialog);
    };
    let dialogCount = 0;
    page.on("dialog", onDialog);
    await page.getByTestId("toolbar-new-file").click();

    await expect(page.getByTestId("file-name")).toHaveText("new_from_scratch.json");
    await expect(page.getByTestId("file-status")).toHaveText("Modified");
    await expect(page.getByTestId("save-mode")).toHaveText("Download only");
    await expect(page.getByTestId("toolbar-save")).toBeEnabled();
    await expect(page.getByRole("heading", { name: "Validation (0)" })).toBeVisible();
    await expect(page.getByRole("button", { name: "intro_scene" })).toBeVisible();

    const downloadPromise = page.waitForEvent("download");
    await page.getByTestId("toolbar-save-as").click();
    const download = await downloadPromise;
    expect(download.suggestedFilename()).toBe("new_from_scratch.json");
  });

  test("invalid JSON stays on empty state", async ({ page }) => {
    await page.goto("/");
    await chooseFixture(page, "invalid-syntax.json");
    await expect(page.locator(".error-banner")).toContainText("Failed to parse JSON");
    await expect(page.getByRole("heading", { name: "Adventure Editor" })).toBeVisible();
    await expect(page.getByTestId("toolbar-save")).toBeDisabled();
  });

  test("non-object JSON is rejected", async ({ page }) => {
    await page.goto("/");
    await chooseFixture(page, "invalid-array.json");
    await expect(page.locator(".error-banner")).toContainText("JSON root must be an adventure object.");
    await expect(page.getByRole("heading", { name: "Adventure Editor" })).toBeVisible();
  });

  test("open file, edit content/choice, mark dirty, then Save As downloads JSON", async ({ page }) => {
    await page.goto("/");
    await openFixture(page, "minimal.json");
    await expect(page.locator(".meta-panel").getByLabel("Title")).toHaveValue("Preview build");
    await expect(page.getByTestId("file-status")).toHaveText("Saved");
    await expect(page.getByTestId("save-mode")).toHaveText("Download only");
    await expect(page.getByTestId("toolbar-graph")).toBeEnabled();
    await expect(page.getByTestId("toolbar-localization")).toBeEnabled();

    await page.getByRole("button", { name: "0: Text" }).click();
    await expect(page.getByRole("heading", { name: "Selected Content" })).toBeVisible();
    await page.locator(".content-panel").getByLabel("Value").fill("Edited text");

    await page.getByRole("button", { name: "0: go" }).click();
    await expect(page.getByRole("heading", { name: "Selected Choice" })).toBeVisible();
    await page.locator(".choices-panel").getByRole("textbox", { name: "Text", exact: true }).fill("Go now");

    await expect(page.getByTestId("file-status")).toHaveText("Modified");

    const downloadPromise = page.waitForEvent("download");
    await page.getByTestId("toolbar-save-as").click();
    const download = await downloadPromise;
    expect(download.suggestedFilename()).toBe("minimal.json");
    await expect(page.getByTestId("file-status")).toHaveText("Saved");
  });

  test("unsaved Open asks to continue; Cancel keeps the current file", async ({ page }) => {
    await page.goto("/");
    await openFixture(page, "minimal.json");
    await page.locator(".meta-panel").getByLabel("Title").fill("Dirty title");
    await expect(page.getByTestId("file-status")).toHaveText("Modified");

    const cancelChooserPromise = page.waitForEvent("filechooser");
    await page.getByTestId("toolbar-open").click();
    const cancelChooser = await cancelChooserPromise;
    page.once("dialog", (dialog) => {
      void dialog.dismiss();
    });
    await cancelChooser.setFiles(fixturePath("localization-demo.json"));
    await expect(page.locator(".meta-panel").getByLabel("Title")).toHaveValue("Dirty title");
    await expect(page.getByTestId("file-name")).toHaveText("minimal.json");

    const continueChooserPromise = page.waitForEvent("filechooser");
    await page.getByTestId("toolbar-open").click();
    const continueChooser = await continueChooserPromise;
    page.once("dialog", (dialog) => {
      void dialog.accept();
    });
    await continueChooser.setFiles(fixturePath("localization-demo.json"));
    await expect(page.getByTestId("file-name")).toHaveText("localization-demo.json");
    await expect(page.locator(".meta-panel").getByLabel("Title")).toHaveValue("Hello world");
    await expect(page.getByTestId("file-status")).toHaveText("Saved");
  });
});
