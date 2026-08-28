import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import enTranslations from './locales/en.json';

// English only. The language detector is deliberately not used: with a single
// supported language it can only ever mis-detect (e.g. a browser set to another
// locale, or a stale `i18nextLng` left in localStorage from a previous session),
// so the language is pinned explicitly instead.
i18n
  .use(initReactI18next)
  .init({
    resources: {
      en: {
        translation: enTranslations,
      },
    },
    lng: 'en',
    fallbackLng: 'en',
    supportedLngs: ['en'],
    debug: false,
    interpolation: {
      escapeValue: false,
    },
  });

export default i18n;

