import { useEffect, useRef } from "react";

export function useScrollSelectedIntoView<T extends HTMLElement>(selected: boolean) {
  const ref = useRef<T | null>(null);
  useEffect(() => {
    if (!selected) {
      return;
    }
    ref.current?.scrollIntoView({ block: "nearest" });
  }, [selected]);
  return ref;
}
