import { useEffect, useState } from "react";
import { API_TOAST_EVENT, type ApiToast } from "../lib/http";

export function Toaster() {
  const [toast, setToast] = useState<ApiToast | null>(null);
  useEffect(() => {
    const receive = (event: Event) => {
      setToast((event as CustomEvent<ApiToast>).detail);
    };
    window.addEventListener(API_TOAST_EVENT, receive);
    return () => window.removeEventListener(API_TOAST_EVENT, receive);
  }, []);
  useEffect(() => {
    if (!toast) return;
    const timer = window.setTimeout(() => setToast(null), 6000);
    return () => window.clearTimeout(timer);
  }, [toast]);
  if (!toast) return null;
  return <div className="toast" role="alert">
    <span className="toast-icon">!</span>
    <span><strong>{toast.title}</strong>{toast.detail && <small>{toast.detail}</small>}</span>
    <button type="button" aria-label="Dismiss notification" onClick={() => setToast(null)}>×</button>
  </div>;
}
