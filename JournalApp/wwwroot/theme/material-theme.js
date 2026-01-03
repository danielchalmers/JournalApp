import * as mcu from "@material/material-color-utilities";

const mcuAny = mcu;

const normalizeHex = (value) => {
  if (!value) return "#5B5B5B";
  return value.startsWith("#") ? value : `#${value}`;
};

const getVariant = () => {
  const variant = mcuAny.Variant;
  if (!variant) return null;
  return variant.EXPRESSIVE ?? variant.Expressive ?? variant.expressive ?? null;
};

const createScheme = (sourceHex, isDark) => {
  const argbFromHex = mcuAny.argbFromHex;
  if (!argbFromHex) throw new Error("Material Color Utilities missing argbFromHex");
  const sourceArgb = argbFromHex(normalizeHex(sourceHex));

  if (mcuAny.SchemeExpressive) {
    return new mcuAny.SchemeExpressive(sourceArgb, isDark, 0.0);
  }

  const variant = getVariant();
  if (mcuAny.themeFromSourceColor) {
    const theme = mcuAny.themeFromSourceColor(sourceArgb, variant ? { variant } : undefined);
    return isDark ? theme.schemes.dark : theme.schemes.light;
  }

  throw new Error("Material Color Utilities not available");
};

const toHex = (argb) => {
  return mcuAny.hexFromArgb(argb);
};

export const getThemeTokens = (sourceHex, isDark) => {
  const scheme = createScheme(sourceHex, isDark);

  return {
    primary: toHex(scheme.primary),
    onPrimary: toHex(scheme.onPrimary),
    primaryContainer: toHex(scheme.primaryContainer),
    onPrimaryContainer: toHex(scheme.onPrimaryContainer),
    secondary: toHex(scheme.secondary),
    onSecondary: toHex(scheme.onSecondary),
    tertiary: toHex(scheme.tertiary),
    onTertiary: toHex(scheme.onTertiary),
    error: toHex(scheme.error),
    onError: toHex(scheme.onError),
    background: toHex(scheme.background),
    onBackground: toHex(scheme.onBackground),
    surface: toHex(scheme.surface),
    onSurface: toHex(scheme.onSurface),
    surfaceVariant: toHex(scheme.surfaceVariant),
    onSurfaceVariant: toHex(scheme.onSurfaceVariant),
    outline: toHex(scheme.outline),
    shadow: toHex(scheme.shadow)
  };
};
