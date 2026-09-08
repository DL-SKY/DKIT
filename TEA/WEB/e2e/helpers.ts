import { test as base, expect, type Page } from "@playwright/test";
import { resolve } from "node:path";

export const FIXTURES_ROOT = resolve(import.meta.dirname, "../tests/fixtures");

export const test = base.extend<object>({
  page: async ({ page }, use) => {
    await page.addInitScript(() => {
      delete window.showOpenFilePicker;
      delete window.showSaveFilePicker;
    });
    await use(page);
  },
});

export { expect };

export function fixturePath(name: string): string {
  return resolve(FIXTURES_ROOT, name);
}

export async function chooseFixture(page: Page, name: string): Promise<void> {
  const chooserPromise = page.waitForEvent("filechooser");
  await page.getByTestId("toolbar-open").click();
  const chooser = await chooserPromise;
  await chooser.setFiles(fixturePath(name));
}

export async function openFixture(page: Page, name: string): Promise<void> {
  await chooseFixture(page, name);
  await expect(page.getByTestId("file-name")).toHaveText(name);
}
