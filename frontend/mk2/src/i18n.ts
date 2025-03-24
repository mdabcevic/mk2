import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import HttpApi from "i18next-http-backend";
import LanguageDetector from "i18next-browser-languagedetector";

i18n
  .use(HttpApi) // Lazy load translations
  .use(LanguageDetector) // Detect browser language
  .use(initReactI18next) // React binding
  .init({
    supportedLngs: ["en", "hr"], // Define supported languages
    fallbackLng: "hr", // Default fallback language
    detection: {
      order: ["localStorage", "cookie", "navigator", "htmlTag"],
      caches: ["localStorage", "cookie"],
    },
    backend: {
      loadPath: (lng: any, ns: any) => `/assets/i18n_locales/${ns}/${lng}.json`, // Load from separate folders
    },
    ns: ["public", "admin"], // Define namespaces
    defaultNS: "public",
    interpolation: {
      escapeValue: false,
    },
  });

export default i18n;