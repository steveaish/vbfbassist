module.exports = {
  root: true,
  // process.env.REACT_APP_* is injected by vite.config.js (CRA-style), so allow `process`
  env: { browser: true, es2020: true, node: true },
  extends: [
    'eslint:recommended',
    'plugin:react/recommended',
    'plugin:react/jsx-runtime',
    'plugin:react-hooks/recommended',
  ],
  ignorePatterns: ['dist', '.eslintrc.cjs'],
  parserOptions: { ecmaVersion: 'latest', sourceType: 'module' },
  settings: { react: { version: '18.2' } },
  plugins: ['react-refresh'],
  rules: {
    'react-refresh/only-export-components': [
      'warn',
      { allowConstantExport: true },
    ],
  },
  overrides: [
    {
      // helper modules, not components
      files: ['src/utils/**'],
      rules: { 'react-refresh/only-export-components': 'off' },
    },
  ],
}
