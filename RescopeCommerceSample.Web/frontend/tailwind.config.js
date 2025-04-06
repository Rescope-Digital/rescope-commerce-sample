/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["../Views/**/*.cshtml", "../wwwroot/js/**/*.js"],
  theme: {
    extend: {
      fontFamily: {
        sans: '"Lato", ui-sans-serif, system-ui, sans-serif, "Apple Color Emoji", "Segoe UI Emoji", "Segoe UI Symbol", "Noto Color Emoji"',
      },
    },
  },
  plugins: [],
};
