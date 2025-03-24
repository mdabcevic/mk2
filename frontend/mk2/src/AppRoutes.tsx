import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Home from "./components/pages/home";
import NotFoundPage from "./components/pages/not-found-page";
import Layout from "./components/layout";
import AboutUs from "./components/pages/about-us";
import App from "./App";


const AppRoutes = () => {
  return (
    <Router>
      <Routes>
        {/* Wrap all routes inside the Layout */}
        <Route path="/" element={<Layout />}>
          <Route path="app" element={<App />} />
          <Route path="home" element={<Home />} />
          <Route path="aboutus" element={<AboutUs />} />
          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Routes>
    </Router>
  );
};

export default AppRoutes;