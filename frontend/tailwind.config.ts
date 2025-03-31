/** @type {import('tailwindcss').Config} */
export default {
    content: [
      "./index.html",
      "./src/**/*.{js,ts,jsx,tsx}", // Ensures Tailwind scans all your React components
    ],
    theme: {
      extend: {}, // Customize Tailwind here if needed
    },
    plugins: [],
  };
  